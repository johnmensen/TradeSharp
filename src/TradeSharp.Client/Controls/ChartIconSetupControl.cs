using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Candlechart;
using Candlechart.ChartIcon;
using Entity;
using FastGrid;
using TradeSharp.Client.BL;
using TradeSharp.Util;
using BrushesStorage = Entity.BrushesStorage;

namespace TradeSharp.Client.Controls
{
    public partial class ChartIconSetupControl : UserControl
    {
        class ChartIconTableItem
        {
            public string IconKey { get; set; }

            public string Title
            {
                get { return ChartIcon.GetNameByKey(IconKey); }
            }

            public bool IsShown { get; set; }
        }

        public ChartIconSetupControl()
        {
            InitializeComponent();

            try
            {
                SetupGrid();
                FillGrid();
            }
            catch (Exception ex)
            {
                // может ругаться в дезайн-моде
                return;
            }
        }

        private void SetupGrid()
        {
            const int imageSize = 22;

            // сформировать ImageList
            var lst = new ImageList { ImageSize = new Size(imageSize, imageSize), ColorDepth = ColorDepth.Depth32Bit };
            foreach (var icon in CandleChartControl.allChartIcons)
            {
                var cpy = icon.MakeCopy();
                cpy.Position = new Point(1, 1);
                cpy.Size = new Size(imageSize, imageSize);

                // нарисовать квадратик
                var bmp = MakeIconBackgr(imageSize + 2);
                // на нем иконку
                using (var g = Graphics.FromImage(bmp))
                {
                    cpy.Draw(g, new BrushesStorage(), new PenStorage());
                }
                // добавить в список
                lst.Images.Add(cpy.key, bmp);
            }

            var blank = new ChartIconTableItem();
            grid.Columns.Add(new FastColumn(blank.Property(p => p.IconKey), "*")
                {
                    ImageList = lst,
                    ColumnWidth = 28
                });
            grid.Columns.Add(new FastColumn(blank.Property(p => p.Title), Localizer.GetString("TitleAction"))
                {
                    ColumnMinWidth = 65,
                    SortOrder = FastColumnSort.Ascending
                });
            grid.Columns.Add(new FastColumn(blank.Property(p => p.IsShown), "*")
                {
                    ColumnWidth = 23,
                    IsHyperlinkStyleColumn = true,
                    HyperlinkActiveCursor = Cursors.Hand,
                    ImageList = imageListSetsGrid
                });

            grid.CalcSetTableMinWidth();
        }

        private void GridOnUserHitCell(object sender, MouseEventArgs e, int rowIndex, FastColumn col)
        {
            var item = (ChartIconTableItem) grid.rows[rowIndex].ValueObject;
            if (col.PropertyName == item.Property(p => p.IsShown) && e.Button == MouseButtons.Left)
            {
                item.IsShown = !item.IsShown;
                grid.UpdateRow(rowIndex, item);
                grid.InvalidateRow(rowIndex);

                // проверить, должна ли быть активна кнопка Принять
                CheckAcceptButtonEnabled();
            }
        }

        private void CheckAcceptButtonEnabled()
        {
            var selectedButtons =
                grid.GetRowValues<ChartIconTableItem>(false).Where(i => i.IsShown).Select(i => i.IconKey).ToList();
            // сравнить выбор пользователя с настройками
            var settingsButtons = UserSettings.Instance.SelectedChartIcons;
            var areDiff = true;
            if (settingsButtons.Length == selectedButtons.Count)
            {
                areDiff = !settingsButtons.HasSameContentsAs(selectedButtons);
            }
            btnAccept.Enabled = areDiff;
        }

        private void FillGrid()
        {
            var selectedIcons = UserSettings.Instance.SelectedChartIcons;

            var items = (from icon in CandleChartControl.allChartIcons
                         let iconKey = icon.key
                         select new ChartIconTableItem
                                    {
                                        IconKey = icon.key, IsShown = selectedIcons.Contains(iconKey)
                                    }).ToList();
            grid.DataBind(items);
        }

        private static Bitmap MakeIconBackgr(int sz)
        {
            var bmp = new Bitmap(sz, sz);
            using (var g = Graphics.FromImage(bmp))
            {
                //using (var b = new SolidBrush(Color.White))
                //{
                //    g.FillRectangle(b, 0, 0, sz - 1, sz - 1);
                //}
                //using (var p = new Pen(Color.Gainsboro))
                //{
                //    for (var i = 2; i < sz; i += 4)
                //    {
                //        g.DrawLine(p, i, 0, i, sz - 1);
                //        g.DrawLine(p, 0, i, sz - 1, i);
                //    }
                //}
            }
            return bmp;
        }

        private void BtnAcceptClick(object sender, EventArgs e)
        {
            AcceptSettings();
        }

        public void AcceptSettings()
        {
            var items =
                grid.rows.Select(r => (ChartIconTableItem) r.ValueObject).Where(i => i.IsShown).Select(i => i.IconKey).
                    ToArray();
            UserSettings.Instance.SelectedChartIcons = items;
            MainForm.Instance.UpdateChartIconSet();
            btnAccept.Enabled = false;
        }
    }
}
