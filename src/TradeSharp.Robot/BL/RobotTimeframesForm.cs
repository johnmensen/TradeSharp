using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Candlechart.ChartIcon;
using Entity;
using FastGrid;
using TradeSharp.Util;

namespace TradeSharp.Robot.BL
{
    public partial class RobotTimeframesForm : Form
    {
        private readonly List<TagGraphics> graphics;

        private readonly DropDownList listTickers, listTimeframes;
        
        public List<Cortege2<string, BarSettings>> UpdatedGraphics
        {
            get
            {
                var setsList = gridTickers.GetRowValues<TagGraphics>(false).Where(g => !g.IsNewRowDummy).Select(g => 
                    new Cortege2<string, BarSettings>(g.Symbol, g.BarSettings)).ToList();
                return setsList;
            }
        }

        public RobotTimeframesForm()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
            listTickers = new DropDownList
                {
                    CalcHeightAuto = true,
                    Width = 90,
                    MaxLines = 15,
                    Values = DalSpot.Instance.GetTickerNames().Cast<object>().ToList()
                };

            listTimeframes = new DropDownList
                {
                    CalcHeightAuto = true,
                    Width = 90,
                    MaxLines = 15,
                    Values = BarSettingsStorage.Instance.GetCollection()
                                               .Select(b => BarSettingsStorage.Instance.GetBarSettingsFriendlyName(b))
                                               .Cast<object>()
                                               .ToList()
                };

            SetupGrid();
        }

        public RobotTimeframesForm(IEnumerable<Cortege2<string, BarSettings>> graphics) : this()
        {
            this.graphics = graphics.Select(g => new TagGraphics
                {
                    BarSettings = g.b,
                    Symbol = g.a
                }).Union(new [] { new TagGraphics { IsNewRowDummy = true } }).ToList();
            gridTickers.DataBind(this.graphics);
            AnchorDummyRow();
        }

        private void AnchorDummyRow()
        {
            var rowDummy = gridTickers.rows.First(r => ((TagGraphics)r.ValueObject).IsNewRowDummy);

            var indexOfDummy = gridTickers.rows.IndexOf(rowDummy);
            if (indexOfDummy < gridTickers.rows.Count - 1)
            {
                gridTickers.rows.RemoveAt(indexOfDummy);
                gridTickers.rows.Add(rowDummy);
            }
            
            rowDummy.anchor = FastRow.AnchorPosition.AnchorBottom;
        }

        private void SetupGrid()
        {
            var fontBold = new Font(Font, FontStyle.Bold);
            gridTickers.Columns.Add(new FastColumn(TagGraphics.speciman.Property(s => s.Symbol), Localizer.GetString("TitleTicker"))
                {
                    IsHyperlinkStyleColumn = true,
                    HyperlinkActiveCursor = Cursors.Hand,
                    ColorHyperlinkTextActive = Color.Blue,
                    HyperlinkFontActive = fontBold
                });
            gridTickers.Columns.Add(new FastColumn(TagGraphics.speciman.Property(s => s.TimeframeString), Localizer.GetString("TitleTimeframe"))
                {
                    IsHyperlinkStyleColumn = true,
                    HyperlinkActiveCursor = Cursors.Hand,
                    ColorHyperlinkTextActive = Color.Green,
                    HyperlinkFontActive = fontBold
                });
            gridTickers.Columns.Add(new FastColumn(TagGraphics.speciman.Property(s => s.DeleteImageIndex), "*")
                {
                    ImageList = lstIcon,
                    IsHyperlinkStyleColumn = true,
                    HyperlinkActiveCursor = Cursors.Hand,
                    ColumnWidth = 23
                });
            gridTickers.UserHitCell += GridTickersOnUserHitCell;
            gridTickers.CalcSetTableMinWidth();
        }

        private void GridTickersOnUserHitCell(object sender, MouseEventArgs mouseEventArgs, int rowIndex, FastColumn col)
        {
            if (mouseEventArgs.Button != MouseButtons.Left) return;
            var sets = (TagGraphics) gridTickers.rows[rowIndex].ValueObject;

            // добавить строку
            if (sets.IsNewRowDummy)
            {
                var newSets = new TagGraphics
                    {
                        Symbol = DalSpot.Instance.GetTickerNames().First(),
                        BarSettings = BarSettingsStorage.Instance.GetCollection().First()
                    };

                // копировать существующую?
                var setsSpec = gridTickers.rows.Select(r => (TagGraphics)r.ValueObject).FirstOrDefault(r => r.IsNewRowDummy == false); 
                if (setsSpec != null)
                {
                    newSets.Symbol = setsSpec.Symbol;
                    newSets.BarSettings = setsSpec.BarSettings;
                }

                var setsList = gridTickers.rows.Select(r => (TagGraphics) r.ValueObject).ToList();
                setsList.Add(newSets);

                gridTickers.DataBind(setsList);
                AnchorDummyRow();
                gridTickers.Invalidate();
                return;
            }

            // всплывающий диалог
            if (col.PropertyName == TagGraphics.speciman.Property(s => s.Symbol) ||
                col.PropertyName == TagGraphics.speciman.Property(s => s.TimeframeString))
            {
                // показать combobox - выбор тикера / таймфрейма
                var coords = gridTickers.GetCellCoords(col, rowIndex);

                var list = col.PropertyName == TagGraphics.speciman.Property(s => s.Symbol) ? listTickers : listTimeframes;
                list.Width = col.ResultedWidth;

                var popup = new DropDownListPopup(list);
                popup.Show(gridTickers, coords.X, coords.Y + gridTickers.CellHeight);
                list.cellClicked = (obj, text) =>
                    {
                        if (list == listTickers)
                            sets.Symbol = text;
                        else
                            sets.TimeframeString = text;
                        gridTickers.UpdateRow(rowIndex, sets);
                        gridTickers.InvalidateCell(col, rowIndex);
                    };
                return;
            }

            // удаление
            gridTickers.rows.RemoveAt(rowIndex);
            gridTickers.Invalidate();
        }
    }
    
    class TagGraphics
    {
        public static readonly TagGraphics speciman = new TagGraphics();

        public bool IsNewRowDummy { get; set; }

        private string symbol;
        public string Symbol
        {
            get { return IsNewRowDummy ? "..." : symbol; }
            set { symbol = value; }
        }
        
        public BarSettings BarSettings { get; set; }
        
        public string TimeframeString
        {
            get
            {
                return IsNewRowDummy ? "..." : BarSettingsStorage.Instance.GetBarSettingsFriendlyName(BarSettings);
            }
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                BarSettings = BarSettingsStorage.Instance.GetBarSettingsByName(value);
            }
        }

        public int DeleteImageIndex
        {
            get
            {
                return IsNewRowDummy ? 100 : 0;
            }
        }
    }
}
