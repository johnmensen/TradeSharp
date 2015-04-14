using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Candlechart.Core;
using Candlechart.Indicator;
using FastGrid;
using TradeSharp.Util;

namespace Candlechart.Controls
{
    public partial class IndicatorsGridForm : Form
    {
        public delegate void IndicatorChangedDel(IChartIndicator indi);

        public delegate void IndicatorUpdateDel(string oldName, IChartIndicator indi);

        public IndicatorChangedDel onIndicatorAdd;

        public IndicatorUpdateDel onIndicatorUpdate;

        public IndicatorChangedDel onIndicatorRemove;

        public List<IChartIndicator> indicators = new List<IChartIndicator>();

        public CandleChartControl owner;

        private readonly string dependencyError;

        public IndicatorsGridForm()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
            dependencyError = Localizer.GetString("TitleError") + ": ";
            var blank = new IndicatorDescription(string.Empty, string.Empty, false);
            fastGrid.Columns.Add(new FastColumn(blank.Property(p => p.Number), "#")
                {
                    SortOrder = FastColumnSort.Ascending,
                    ColumnMinWidth = 50,
                    ColumnWidth = 50
                });
            fastGrid.Columns.Add(new FastColumn(blank.Property(p => p.IsFavorite), "*")
                {
                    IsHyperlinkStyleColumn = true,
                    HyperlinkActiveCursor = Cursors.Hand,
                    ImageList = imageList,
                    ColumnMinWidth = 25,
                    ColumnWidth = 25
                });
            fastGrid.Columns.Add(new FastColumn(blank.Property(p => p.Name), Localizer.GetString("TitleName"))
                {
                    ColumnMinWidth = 50
                });
            fastGrid.Columns.Add(new FastColumn(blank.Property(p => p.DisplayTypeName), Localizer.GetString("TitleType"))
                {
                    ColumnMinWidth = 50
                });
            fastGrid.Columns.Add(new FastColumn(blank.Property(p => p.Depends), Localizer.GetString("TitleReferences"))
                {
                    colorColumnFormatter = delegate(object depends, out Color? bc, out Color? fc)
                        {
                            bc = null;
                            fc = ((string) depends).StartsWith(dependencyError) ? Color.Red : (Color?) null;
                        },
                    ColumnMinWidth = 50
                });
            fastGrid.Columns.Add(new FastColumn(blank.Property(p => p.Close), "-")
                {
                    IsHyperlinkStyleColumn = true,
                    HyperlinkActiveCursor = Cursors.Hand,
                    ImageList = imageList,
                    ColumnMinWidth = 25,
                    ColumnWidth = 25
                });
        }

        private void BuildView()
        {
            var row = GetSelectedRow();
            var indicatorDescriptions = new List<IndicatorDescription>();
            var favorites = owner.getFavoriteIndicators();
            for (var i = 0; i < indicators.Count; i++)
            {
                var indi = indicators[i];
                var sources = indi.SeriesSourcesDisplay == null ? new string[]{} : indi.SeriesSourcesDisplay.Split(new[] { ';' });
                var depends = "";
                var dependErrors = "";
                foreach (var source in sources)
                {
                    if (!source.Contains('/'))
                    {
                        if (!string.IsNullOrEmpty(depends))
                            depends += "; ";
                        depends += source;
                        continue;
                    }
                    var sourceParams = source.Split(new[] {'/'});
                    if(sourceParams.Count() != 3)
                    {
                        if (!string.IsNullOrEmpty(dependErrors))
                            dependErrors += "; ";
                        dependErrors += Localizer.GetString("TitleSyntaxErrorSmall") + ": " + source;
                        continue;
                    }
                    var dependIndex = indicatorDescriptions.FindIndex(ind => ind.Name == sourceParams[0]);
                    if(dependIndex == -1)
                    {
                        if (!string.IsNullOrEmpty(dependErrors))
                            dependErrors += "; ";
                        dependErrors += sourceParams[0];
                        continue;
                    }
                    if (!string.IsNullOrEmpty(depends))
                        depends += "; ";
                    depends += indicatorDescriptions[dependIndex].Number;
                }
                if (!string.IsNullOrEmpty(dependErrors))
                    depends = dependencyError + dependErrors;
                indicatorDescriptions.Add(new IndicatorDescription(indi.UniqueName, null,
                                                                   favorites.Contains(indi.GetType().Name))
                    {
                        TypeName = indi.GetType().Name,
                        DisplayTypeName = indi.Name,
                        Number = i + 1,
                        Depends = depends
                    });
            }
            fastGrid.DataBind(indicatorDescriptions);
            fastGrid.CheckSize();
            SelectRow(row);
            fastGrid.Invalidate();
        }

        private void IndicatorsWindowLoad(object sender, EventArgs e)
        {
            BuildView();
            CheckBrokenDependencies();
        }

        private void BtnAddClick(object sender, EventArgs e)
        {
            var dlg = new ChooseIndicatorDialog(owner);
            if (dlg.ShowDialog() != DialogResult.OK)
            {
                BuildView();
                return;
            }
            if (string.IsNullOrEmpty(dlg.SelectedIndiName)) return;
            var selectedTypeName = dlg.SelectedIndiName;

            foreach (var tp in PluginManager.Instance.typeIndicators)
            {
                var indnameattr = (DisplayNameAttribute)Attribute.GetCustomAttribute(tp, 
                    typeof(DisplayNameAttribute));
                if (indnameattr == null) continue;
                if (selectedTypeName != indnameattr.DisplayName) continue;
                var ind = (IChartIndicator)Activator.CreateInstance(tp);
                if (onIndicatorAdd == null)
                    return;
                onIndicatorAdd(ind);
                break;
            }
            BuildView();
            UpdateUserInterface();
            EditIndicator(fastGrid.rows.Count - 1);
        }

        private void CheckBrokenDependencies()
        {
            List<string> misArcs = new List<string>(), brokArcs = new List<string>();
            if (!owner.FindBrokenIndicatorArcs(misArcs, brokArcs)) return;
            var msg = new StringBuilder();
            msg.AppendFormat(Localizer.GetString("MessageIngicatorsMisorderPrefix"));
            foreach (var arc in misArcs) msg.AppendFormat("{0} ", arc);
            if (brokArcs.Count > 0)
            {
                msg.AppendLine(Localizer.GetString("MessageIngicatorsMisorderLoops"));
                foreach (var arc in brokArcs) msg.AppendLine(arc);                
            }
            MessageBox.Show(msg.ToString(), 
                Localizer.GetString("TitleWarning"), 
                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void FastGridUserHitCell(object sender, MouseEventArgs e, int rowIndex, FastColumn col)
        {
            UpdateUserInterface();
            var indDesc = (IndicatorDescription)fastGrid.rows[rowIndex].ValueObject;
            if (col.PropertyName == indDesc.Property(p => p.Close) && rowIndex >= 0)
            {
                DeleteIndicator(rowIndex);
                return;
            }
            if (e.Button == MouseButtons.Right && GetSelectedRow() >= 0)
            {
                contextMenuStrip.Show(fastGrid, e.Location);
                return;
            }
            if (col.PropertyName == indDesc.Property(p => p.IsFavorite) && rowIndex >= 0)
            {
                indDesc.IsFavorite = !indDesc.IsFavorite;
                var favList = owner.getFavoriteIndicators();
                if (indDesc.IsFavorite)
                    favList.Add(indDesc.TypeName);
                else
                    favList.Remove(indDesc.TypeName);
                owner.CallFavoriteIndicatorsAreUpdated(favList);
                BuildView();
                return;
            }
            if (e.Clicks == 2 && rowIndex >= 0)
                EditIndicator(rowIndex);
        }

        private int GetSelectedRow()
        {
            for (var i = 0; i < fastGrid.rows.Count; i++)
                if (fastGrid.rows[i].Selected)
                    return i;
            return -1;
        }

        private void SelectRow(int rowIndex)
        {
            foreach (var r in fastGrid.rows)
                r.Selected = false;
            if (rowIndex >= 0 && rowIndex < fastGrid.rows.Count)
                fastGrid.rows[rowIndex].Selected = true;
        }

        private void BtnPropertiesClick(object sender, EventArgs e)
        {
            var row = GetSelectedRow();
            if (row >= 0)
                EditIndicator(row);
        }

        private void BtnDeleteClick(object sender, EventArgs e)
        {
            var row = GetSelectedRow();
            if (row >= 0)
                DeleteIndicator(row);
        }

        private void BtnUpClick(object sender, EventArgs e)
        {
            var row = GetSelectedRow();
            if (row < 1)
                return;
            var indi = indicators[row];
            indicators.RemoveAt(row);
            indicators.Insert(row - 1, indi);
            BuildView();
            SelectRow(row -1);
            fastGrid.Invalidate();
            UpdateUserInterface();
        }

        private void BtnDownClick(object sender, EventArgs e)
        {
            var row = GetSelectedRow();
            if (row < 0 || row >= indicators.Count - 1)
                return;
            var indi = indicators[row];
            indicators.RemoveAt(row);
            indicators.Insert(row + 1, indi);
            BuildView();
            SelectRow(row + 1);
            fastGrid.Invalidate();
            UpdateUserInterface();
        }

        private void EditIndicator(int indicatorDescriptionRow)
        {
            var indi = indicators[indicatorDescriptionRow];
            var oldName = indi.UniqueName;
            var tmpIndi = ((BaseChartIndicator)indi).Copy();
            var dlg = new IndicatorSettingsWindow { Indi = (IChartIndicator)tmpIndi };
            // открыть окно атрибутов узла
            if (dlg.ShowDialog() != DialogResult.OK) return;

            // обновить список индикаторов в графике            
            ((BaseChartIndicator)dlg.Indi).Copy((BaseChartIndicator)indi);

            // обновить все связи между индикаторами
            owner.UpdateIndicatorPanesAndSeries();

            // обновить сам индикатор
            indi.AcceptSettings();

            if (indi.UniqueName == oldName)
            {
                var parametrizedName = ((BaseChartIndicator)indi).GenerateNameBySettings();
                if (!string.IsNullOrEmpty(parametrizedName))
                    indi.UniqueName = parametrizedName;
            }

            // обновить таблицу подстановки
            LookupTypeEditor.UpdatePropValues(indi);
            // проверить уникальность имени и если надо обновить связи на серии индюка
            if (oldName != indi.UniqueName)
            {
                owner.EnsureUniqueName(indi);
                owner.RefreshDisplaySeriesAndPanels(oldName, indi.UniqueName);
                owner.UpdateIndicatorPanesAndSeries();
            }

            // обновить подпись панели индикатора
            if (((BaseChartIndicator)indi).ownPane != null)
                ((BaseChartIndicator)indi).ownPane.Title = indi.UniqueName;

            if (onIndicatorUpdate != null)
                onIndicatorUpdate(oldName, indi);

            // проверить нарушения наследования
            CheckBrokenDependencies();
            BuildView();

            fastGrid.Invalidate();
            UpdateUserInterface();
        }

        private void DeleteIndicator(int indicatorDescriptionRow)
        {
            //var indi = indicators.Find(i => i.UniqueName == ((IndicatorDescription) fastGrid.rows[indicatorDescriptionRow].ValueObject).Name);
            var indi = indicators[indicatorDescriptionRow];
            owner.RemoveIndicator(indi);
            if (onIndicatorRemove != null)
                onIndicatorRemove(indi);
            CheckBrokenDependencies();
            BuildView();
            SelectRow(-1);
            UpdateUserInterface();
        }

        private void UpdateUserInterface()
        {
            var row = GetSelectedRow();
            if (row < 0)
            {
                btnProperties.Enabled = false;
                btnDelete.Enabled = false;
                btnUp.Enabled = false;
                btnDown.Enabled = false;
                return;
            }
            btnProperties.Enabled = true;
            btnDelete.Enabled = true;
            btnUp.Enabled = row != 0;
            btnDown.Enabled = row != indicators.Count - 1;
        }

        private void CopyToolStripMenuItemClick(object sender, EventArgs e)
        {
            var row = GetSelectedRow();
            if (row < 0)
                return;
            var indi = indicators[row];
            var newIndi = ((BaseChartIndicator)indi).Copy();
            if (onIndicatorAdd == null)
                return;
            onIndicatorAdd((IChartIndicator)newIndi);
            BuildView();
            SelectRow(fastGrid.rows.Count - 1);
            UpdateUserInterface();
            EditIndicator(fastGrid.rows.Count - 1);
        }

        private void EditToolStripMenuItemClick(object sender, EventArgs e)
        {
            var row = GetSelectedRow();
            if (row >= 0)
                EditIndicator(row);
        }

        private void DeleteToolStripMenuItemClick(object sender, EventArgs e)
        {
            var row = GetSelectedRow();
            if (row >= 0)
                DeleteIndicator(row);
        }

        private void IndicatorsGridFormHelpRequested(object sender, HelpEventArgs hlpevent)
        {
            HelpManager.Instance.ShowHelp(HelpManager.IndicatorWindow);
        }
    }

    public class IndicatorDescription
    {
        public int Number { get; set; }

        public string Name { get; set; }

        public string Category { get; set; }

        public bool IsFavorite { get; set; }

        public string TypeName { get; set; }

        public Type indicatorType;

        public string DisplayTypeName { get; set; }

        public string Depends { get; set; }

        public string Close { get { return "ico delete.png"; } }

        public IndicatorDescription(string name, string category, bool isFavorite)
        {
            Name = name;
            Category = category;
            IsFavorite = isFavorite;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
