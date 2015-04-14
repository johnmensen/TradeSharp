using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using FastGrid;
using TradeSharp.Client.BL;
using TradeSharp.Util;
using TradeSharp.Util.Forms;

namespace TradeSharp.Client.Forms
{
    public partial class ManageTemplate : Form
    {
        private ToolTip chartIconToolTip;

        public ManageTemplate()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
            var blank = new TemplateDescription();
            fastGrid.Columns.Add(new FastColumn(blank.Property(p => p.Name), Localizer.GetString("TitleName"))
                {
                    ColumnMinWidth = 50,
                    IsHyperlinkStyleColumn = true,
                    ColorHyperlinkTextActive = Color.Blue,
                    HyperlinkFontActive = new Font(Font, FontStyle.Bold),
                    HyperlinkActiveCursor = Cursors.Hand
                });
            fastGrid.Columns.Add(new FastColumn(blank.Property(p => p.IndicatorsCount), Localizer.GetString("TitleIndicators"))
                {
                    ColumnMinWidth = 50,
                    IsHyperlinkStyleColumn = true,
                    ColorHyperlinkTextActive = Color.Blue,
                    HyperlinkFontActive = new Font(Font, FontStyle.Bold),
                    HyperlinkActiveCursor = Cursors.Hand
                });
            fastGrid.Columns.Add(new FastColumn(blank.Property(p => p.Close), "-")
                {
                    IsHyperlinkStyleColumn = true,
                    HyperlinkActiveCursor = Cursors.Hand,
                    ImageList = imageList,
                    ColumnMinWidth = 25,
                    ColumnWidth = 25
                });
            Rebind();
        }

        private void Rebind()
        {
            var templateDescription = new List<TemplateDescription>();
            foreach (var xmlTemplate in ChartTemplate.GetChartAllTemplates())              
                templateDescription.Add(new TemplateDescription(xmlTemplate));
            fastGrid.DataBind(templateDescription);
            fastGrid.CheckSize();
        }

        private void FastGridUserHitCell(object sender, MouseEventArgs e, int rowIndex, FastColumn col)
        {
            if (e.Button != MouseButtons.Left || rowIndex < 0)
                return;

            var description = fastGrid.rows[rowIndex].ValueObject as TemplateDescription;

            if (col.PropertyName == description.Property(p => p.Close))
            {
                var templateDescription = fastGrid.GetRowValues<TemplateDescription>(false).ToList();
                if (description != null)
                {
                    ChartTemplate.DellChartTemplate(description.Name);
                    templateDescription.Remove(description);
                    fastGrid.DataBind(templateDescription);
                    fastGrid.Invalidate();
                }
            }
            else if (col.PropertyName == description.Property(p => p.Name))
            {
                var dropDownDialog = new DropDownDialog("Введите название",
                                                        fastGrid.GetRowValues<TemplateDescription>(false)
                                                                .Select(x => x.Name)
                                                                .Cast<object>()
                                                                .ToList(), false);
                dropDownDialog.SelectedText = description.Name;
                var dialogResult = dropDownDialog.ShowDialog();
                var newName = dropDownDialog.SelectedText;
                if (dialogResult == DialogResult.Cancel || newName == description.Name) return;
                if (!ChartTemplate.UpdateChartTemplateName(description.Name, newName))
                {
                    description.Name = newName;
                    fastGrid.UpdateRow(rowIndex, description);
                    fastGrid.InvalidateRow(rowIndex);
                }
                else
                {
                    Rebind();
                }
            }
            else if (col.PropertyName == description.Property(p => p.IndicatorsCount))
            {
                if (description != null)
                    ShowTooltip(string.Format("Шаблон \"{0}\" содержит индикаторы", description.Name),
                                string.Join(Environment.NewLine, description.IndicatorNames), e.X, e.Y);
            }

        }

        private void ShowTooltip(string title, string text, int x, int y)
        {

            if (chartIconToolTip != null)
            {
                chartIconToolTip.Active = false;
                chartIconToolTip = null;
                return;
            }

            chartIconToolTip = new ToolTip { ToolTipTitle = title };
            chartIconToolTip.Show(text, this, x, y);
        }

        public class TemplateDescription
        {
            public string Name { get; set; }

            public string Close { get { return "ico delete.png"; } }

            private readonly int indiCount;
            public string IndicatorsCount
            {
                get { return indiCount == 0 ? "-" : indiCount.ToString(); }
            }

            public List<string> IndicatorNames { get; set; }

            public TemplateDescription()
            {
            }

            public TemplateDescription(XmlNode template)
            {
                Name = template.Attributes[ChartTemplate.AttributeName].Value;
                indiCount = template.ChildNodes.Count;
                IndicatorNames = new List<string>(template.ChildNodes.Cast<XmlNode>().Select(x => x.Attributes[ChartTemplate.AttributeIndicatorUniqueName].Value));
            }

            public override string ToString()
            {
                return Name;
            }
        }

        private void FastGridMouseClick(object sender, MouseEventArgs e)
        {
            if (chartIconToolTip != null)
            {
                chartIconToolTip.Active = false;
                chartIconToolTip = null;
            }
        }
    }
}
