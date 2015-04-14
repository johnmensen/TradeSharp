using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
using FastGrid;
using TradeSharp.Chat.Client.BL;
using TradeSharp.Chat.Contract;
using TradeSharp.Client.Subscription.Dialog;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.BL;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.SiteBridge.Lib.Contract;
using TradeSharp.SiteBridge.Lib.Distribution;
using TradeSharp.Util;
using TradeSharp.Util.Forms;
using FontStyle = System.Drawing.FontStyle;

namespace TradeSharp.Client.Subscription.Control
{
    public delegate void SavePerformersGridSelectedColumnsDel(List<string> colNames);
    public delegate List<string> LoadPerformersGridSelectedColumnsDel();

    public partial class PerformersFastGrid : UserControl
    {
        public SavePerformersGridSelectedColumnsDel SavePerformersGridSelectedColumns;
        public LoadPerformersGridSelectedColumnsDel LoadPerformersGridSelectedColumns;
        public event ChatControlBackEnd.EnterRoomDel EnterRoomRequested;

        // исп. для переключения вкладки SubscriptionControl
        public Action<SubscriptionControl.ActivePage> PageTargeted;

        public FastGrid.FastGrid Grid { get { return grid; } }

        public bool FitWidth { get { return grid.FitWidth; } set { grid.FitWidth = value; } }

        public Pen ChartPen;

        public Brush ChartBrush = null;

        public Size MiniChartSize
        {
            get { return imgListChartMini.ImageSize; }
            set { imgListChartMini.ImageSize = value; }
        }

        public bool ShowLabelsInMiniChart { get; set; }

        private ChatControlBackEnd chat;

        // TODO: make content strongly typed
        private static readonly List<string> defaultProperties = new List<string>
            {
                "IsSubscribed", "ChartIndex", "TradeSignalTitle", "UserScore", "Profit", "DepoCurrency", "SubscriberCount"
            };
        
        public PerformersFastGrid()
        {
            InitializeComponent();

            grid.UserHitCell += GridUserHitCell;
            grid.ColumnSettingsChanged += () =>
                {
                    if (SavePerformersGridSelectedColumns != null)
                        SavePerformersGridSelectedColumns(grid.Columns.Select(c => c.PropertyName).ToList());
                }; 
            grid.CalcSetTableMinWidth();
        }

        public void SetupGrid()
        {
            var stat = new PerformerStatEx();
            try
            {
                var metadataSettings = TradeSharpDictionary.Instance.proxy.GetMetadataByCategory("UserInfoEx");
                if (metadataSettings != null)
                    imgListAvatar.ImageSize = new Size((int) metadataSettings["SmallAvatarMaxSize"],
                                                       (int) metadataSettings["SmallAvatarMaxSize"]);
            }
            catch (Exception ex)
            {
                Logger.Info("PerformersFastGrid.SetupGrid", ex);
            }
            try
            {
                grid.Columns.Clear();
                var cols = LoadColumns();
                var fontBold = new Font(Font, FontStyle.Bold);

                foreach (var col in cols)
                {
                    var column = new FastColumn(col.PropertyName, col.Title)
                    {
                        FormatString = col.FormatString
                        //ColumnMinWidth = 65
                    };
                    if (!string.IsNullOrEmpty(col.FormatSuffix))
                    {
                        column.Tag = col.FormatSuffix;
                        column.CellFormatting += args =>
                            {
                                args.resultedString += (string) column.Tag;
                            };
                    }
                    // определенные колонки имеют определенные настройки
                    if (col.PropertyName == stat.Property(t => t.IsSubscribed))
                    {
                        column.IsHyperlinkStyleColumn = true;
                        column.HyperlinkActiveCursor = Cursors.Hand;
                        column.ImageList = imageListGrid;
                    }
                    else if (col.PropertyName == stat.Property(t => t.TradeSignalTitle) ||
                        col.PropertyName == stat.Property(t => t.Login))
                    {
                        if (col.PropertyName == stat.Property(t => t.TradeSignalTitle))
                            column.ColumnMinWidth = 70;
                        column.IsHyperlinkStyleColumn = true;
                        column.HyperlinkActiveCursor = Cursors.Hand;
                        column.ColorHyperlinkTextActive = Color.Blue;
                        column.HyperlinkFontActive = fontBold;
                        column.ShowClippedContent = true;
                    }
                    else if (col.PropertyName == stat.Property(t => t.ChartIndex))
                    {
                        column.ColumnWidth = imgListChartMini.ImageSize.Width + grid.CellPadding * 2;
                        column.ImageList = imgListChartMini;
                        column.IsHyperlinkStyleColumn = true;
                        column.HyperlinkActiveCursor = Cursors.Hand;
                    }
                    else if (col.PropertyName == stat.Property(t => t.UserScore))
                    {
                        column.IsHyperlinkStyleColumn = true;
                        column.HyperlinkActiveCursor = Cursors.Hand;
                        column.ColorHyperlinkTextActive = Color.Blue;
                        column.HyperlinkFontActive = fontBold;
                    }
                    else if (col.PropertyName == stat.Property(t => t.IsRealAccount))
                    {
                        column.ImageList = imageListGrid;
                    }
                    else if (col.PropertyName == stat.Property(t => t.AvatarSmallIndex))
                    {
                        column.ImageList = imgListAvatar;
                        column.ColumnWidth = imgListAvatar.ImageSize.Width + grid.CellPadding * 2;
                    }
                    else if (col.PropertyName == stat.Property(t => t.Rooms))
                    {
                        column.IsHyperlinkStyleColumn = true;
                        column.HyperlinkActiveCursor = Cursors.Hand;
                        column.ColorHyperlinkTextActive = Color.Blue;
                        column.HyperlinkFontActive = fontBold;
                    }

                    column.Visible = true;
                    grid.Columns.Add(column);
                }
                
                // установка минимальных размеров таблицы (а зачем?)
                //grid.CalcSetTableMinWidth();
                grid.DataBind(grid.rows.Select(r => r.ValueObject).ToList());
                grid.CheckSize(true);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
                // мб в режиме дизайнера, о чем контрол никогда не узнает
            }
        }

        public void DataBind(List<PerformerStatEx> performerStats, ChatControlBackEnd chat)
        {
            var images = imgListChartMini.Images.Cast<Image>().ToList();
            imgListChartMini.Images.Clear();
            foreach (var img in images)
                img.Dispose();
            var data = new List<PerformerStatEx>();

            // UserScore вычисляется на сервере
            // RecalcUserScore(performerStats);

            var userInfoExSource = new UserInfoExCache(TradeSharpAccountStatistics.Instance.proxy);
            var usersInfoEx = userInfoExSource.GetUsersInfo(performerStats.Select(p => p.UserId).Distinct().ToList());

            var rowColors = new[] { Color.Red, Color.ForestGreen, Color.Black };
            var stringFormat = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center
                };
            using (var font = new Font(Font.FontFamily, 7))
            foreach (var performerStat in performerStats)
            {
                // создать картинку с графиком
                var bmp = new Bitmap(imgListChartMini.ImageSize.Width, imgListChartMini.ImageSize.Height);
                if (performerStat.Chart != null)
                using (var gr = Graphics.FromImage(bmp))
                {
                    var leftValue = performerStat.AvgYearProfit;
                    var leftText = string.Format("{0:f2}%", leftValue);
                    
                    var leftTextWidth = gr.MeasureString(leftText, font).ToSize().Width;
                    var rightValue = performerStat.ProfitLastMonthsAbs;
                    var rightText = rightValue.ToStringUniformMoneyFormat(false);
                    var textWidth = leftTextWidth + gr.MeasureString(rightText, font).ToSize().Width;
                    if (!ShowLabelsInMiniChart)
                        textWidth = 0;
                    PointF[] points;
                    if (ChartBrush != null)
                        points = MiniChartPacker.MakePolygon(performerStat.Chart,
                                                                 imgListChartMini.ImageSize.Width - textWidth,
                                                                 imgListChartMini.ImageSize.Height, 1, 1);
                    else
                        points = MiniChartPacker.MakePolyline(performerStat.Chart,
                                                                  imgListChartMini.ImageSize.Width - textWidth,
                                                                  imgListChartMini.ImageSize.Height, 1, 1);
                    gr.SmoothingMode = SmoothingMode.AntiAlias;
                    gr.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    if (ShowLabelsInMiniChart)
                    {
                        for (var i = 0; i < points.Length; i++)
                            points[i] = new PointF(points[i].X + leftTextWidth, points[i].Y);
                        gr.DrawString(leftText, font,
                                      new SolidBrush(leftValue >= 0 ? rowColors[1] : rowColors[0]), 0,
                                      imgListChartMini.ImageSize.Height / 2, stringFormat);
                        gr.DrawString(rightText, font,
                                      new SolidBrush(rightValue >= 0 ? rowColors[1] : rowColors[0]),
                                      imgListChartMini.ImageSize.Width - textWidth + leftTextWidth,
                                      imgListChartMini.ImageSize.Height / 2, stringFormat);
                    }
                    
                    if (ChartBrush != null)
                        gr.FillPolygon(ChartBrush, points);
                    else
                        gr.DrawLines(ChartPen ?? new Pen(leftValue >= 0 ? rowColors[1] : rowColors[0], 2), points);
                }
                var item = new PerformerStatEx(performerStat) {ChartIndex = imgListChartMini.Images.Count};
                imgListChartMini.Images.Add(bmp);
                imgListChartMini.Images.SetKeyName(item.ChartIndex, item.ChartIndex.ToString());

                // создать фотку
                UserInfoEx userInfoEx = null;
                if (usersInfoEx != null)
                    userInfoEx = usersInfoEx.Find(ui => ui != null && ui.Id == performerStat.UserId);
                if (userInfoEx != null && userInfoEx.AvatarSmall != null)
                {
                    item.AvatarSmallIndex = imgListAvatar.Images.Count;
                    imgListAvatar.Images.Add(userInfoEx.AvatarSmall);
                    imgListAvatar.Images.SetKeyName(item.AvatarSmallIndex, item.AvatarSmallIndex.ToString());
                }
                else
                    item.AvatarSmallIndex = -1;

                data.Add(item);
            }

            grid.DataBind(data);
            grid.Invalidate();

            if (this.chat != null)
                this.chat.RoomsReceived -= RoomsReceived;
            this.chat = chat;

            if (chat == null)
                return;

            chat.RoomsReceived += RoomsReceived;
            chat.GetRooms();
        }

        // создание строки с комнатами, которыми владеет каждый сигнальщик
        private void RoomsReceived(List<Room> rooms)
        {
            try
            {
                Invoke(new Action<List<Room>>(OnRoomReceive), rooms);
            }
            catch
            {
            }
        }

        private void OnRoomReceive(List<Room> rooms)
        {
            for (var rowIndex = 0; rowIndex < grid.rows.Count; rowIndex++)
            {
                var row = grid.rows[rowIndex];
                var performer = (PerformerStatEx) row.ValueObject;
                performer.Rooms = string.Join(", ", rooms.Where(r => r.Owner == performer.UserId).Select(r => r.Name));
                grid.UpdateRow(rowIndex, performer);
            }
            var blank = new PerformerStatEx();
            var roomsColumnIndex = grid.Columns.FindIndex(col => col.PropertyName == blank.Property(p => p.Rooms));
            grid.InvalidateColumns(roomsColumnIndex, roomsColumnIndex);
            chat.RoomsReceived -= RoomsReceived;
        }

        private void GridUserHitCell(object sender, MouseEventArgs e, int rowIndex, FastColumn col)
        {
            if (e.Button != MouseButtons.Left)
                return;
            var selPerformer = (PerformerStatEx) grid.rows[rowIndex].ValueObject;
            if (col.PropertyName == selPerformer.Property(t => t.IsSubscribed))
            {
                PerformerStatistic.SubscribeOrUnsubscribe(selPerformer, true);
                if (PageTargeted != null)
                    PageTargeted(SubscriptionControl.ActivePage.Subscription);
            }
            else if ((col.PropertyName == selPerformer.Property(t => t.TradeSignalTitle) ||
                 col.PropertyName == selPerformer.Property(t => t.ChartIndex) ||
                 col.PropertyName == selPerformer.Property(t => t.Login)))
            {
                var form = new SubscriberStatisticsForm(selPerformer);
                form.EnterRoomRequested += OnEnterRoomRequested;
                form.pageTargeted += p => { if (PageTargeted != null) PageTargeted(p); };
                form.Show(this);
            }
            else if (col.PropertyName == selPerformer.Property(t => t.UserScore))
            {
                //ChangeCriteria();
            }
            else if (col.PropertyName == selPerformer.Property(t => t.Rooms))
            {
                var performer = (PerformerStatEx) grid.rows[rowIndex].ValueObject;
                if (EnterRoomRequested != null && !string.IsNullOrEmpty(performer.Rooms))
                {
                    var rooms = performer.Rooms.Split(new[] {", "}, StringSplitOptions.None);
                    if (rooms.Length == 1)
                        EnterRoomRequested(rooms[0]);
                    else
                    {
                        var form = new ListSelectDialog();
                        form.Initialize(rooms.Select(o => o as object), "Выберите комнату чата:");
                        if (form.ShowDialog(this) == DialogResult.OK)
                            EnterRoomRequested(form.SelectedItem.ToString());
                    }
                }
            }
        }

        private void OnEnterRoomRequested(string name, string password = "")
        {
            if (EnterRoomRequested != null)
                EnterRoomRequested(name, password);
        }

        // установка минимальной ширины колонки неперед (имеет смысл только для числовых данных),
        // исходя из необходимости хранить данные всего диапазона значений данной колонки;
        // такая установка исключает какие-либо иные установки ширины в дальнейшем;
        // исключена из SetupGrid
        private void DetermineColumnDisplaySettings(FastColumn col, PerformerStatField field, Graphics graphics)
        {
            // определить минимальную ширину колонки из размеров ее заголовка
            var font = grid.FontHeader ?? grid.Font;
            var textWd = (int) Math.Ceiling(graphics.MeasureString(col.Title, font).Width);
            // добавить ширину значка сортировки
            textWd += 8;
            col.ColumnMinWidth = textWd;
            // если колонка содержит число, задать ей соответствующую ширину фиксированной
            var prop = typeof (PerformerStatEx).GetProperties().FirstOrDefault(p => p.Name == field.PropertyName);
            if (prop == null) return;
            var propType = prop.PropertyType;
            var nulType = Nullable.GetUnderlyingType(propType);
            propType = nulType ?? propType;
            // для целочисленных
            if (propType == typeof (int) || propType == typeof (short) || propType == typeof (long) ||
                propType == typeof (uint) || propType == typeof (ushort) || propType == typeof (ulong))
            {
                var wd = (int)Math.Ceiling(graphics.MeasureString("-19 999 999", font).Width);
                col.ColumnWidth = Math.Max(wd, col.ColumnMinWidth);
                return;
            }
            // для вещественных
            if (propType == typeof(float) || propType == typeof(double) || propType == typeof(decimal))
            {
                const double number = -9000500.25;
                var str = number.ToStringUniformMoneyFormat();
                try
                {
                    if (!string.IsNullOrEmpty(field.FormatString))
                        str = number.ToString(field.FormatString);
                }
                catch
                {
                }

                var wd = (int)Math.Ceiling(graphics.MeasureString(str, font).Width);
                col.ColumnWidth = Math.Max(wd, col.ColumnMinWidth);
                //return;
            }
        }

        private List<PerformerStatField> LoadColumns()
        {
            var columnsToShow = GetColumnsToShow();
            return columnsToShow.Select(propName => 
                PerformerStatField.fields.FirstOrDefault(f => f.PropertyName == propName)).Where(field => field != null).ToList();
        }

        private List<string> GetColumnsToShow()
        {
            var columnsToShow = LoadPerformersGridSelectedColumns();
            if (columnsToShow.Count == 0)
                columnsToShow = defaultProperties;
            // обязательность колонок отключена
            //columnsToShow = columnsToShow.Union(obligatoryProperties).Distinct().ToList();
            return columnsToShow;
        }

        private void MenuitemChooseColumnsClick(object sender, EventArgs e)
        {
            if (SavePerformersGridSelectedColumns == null)
                return;

            var columnsToShow = GetColumnsToShow();
            // взять все поля
            // и упорядочить их по порядку следования в таблице...
            var optionsOrdered =
                PerformerStatField.fields.Select(o => new Cortege2<object, int>(o, grid.Columns.FindIndex(
                    c => c.PropertyName == o.PropertyName))).Select(o =>
                        new Cortege2<object, int>(o.a, o.b == -1 ? int.MaxValue : o.b)).OrderBy(o => o.b).Select(o => o.a).ToList();
            
            var checkState = optionsOrdered.Select(o => columnsToShow.Contains(((PerformerStatField) o).PropertyName)).ToList();
            
            if (!CheckedOrderedListBoxDialog.ShowDialog(ref optionsOrdered, ref checkState, "Выберите столбцы"))
                return;

            SaveColumnSettings(optionsOrdered, checkState);

            SetupGrid();
            grid.DataBind(grid.GetRowValues<PerformerStatEx>(false).ToList());
        }

        private void SaveColumnSettings(List<object> options, List<bool> checkState)
        {
            var selOptions = options.Where((t, i) => checkState[i]).Select(t => ((PerformerStatField) t).PropertyName).ToList();
            SavePerformersGridSelectedColumns(selOptions);
        }

        private void MenuitemChooseCriteriaClick(object sender, EventArgs e)
        {
            ChangeCriteria();
        }

        private void ChangeCriteria()
        {
            if (new PerformerCriteriaFunctionForm().ShowDialog() != DialogResult.OK) return;
            var performers = grid.GetRowValues<PerformerStatEx>(false).ToList();
            RecalcUserScore(performers);
            grid.DataBind(performers);
            grid.Invalidate();
        }

        private void RecalcUserScore(IEnumerable<PerformerStatEx> performerStats)
        {
            var crit = PerformerCriteriaFunctionCollection.Instance.SelectedFunction;
            ExpressionResolver resv;
            try
            {
                resv = new ExpressionResolver(crit.Function);
            }
            catch
            {
                resv = null;
            }

            foreach (var performerStat in performerStats)
            {
                // посчитать критерий
                if (resv != null)
                    performerStat.UserScore = (float)PerformerCriteriaFunction.Calculate(resv, performerStat);
            }
        }

        private void contextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            menuitemSelectedSummary.Visible = grid.rows.Any(r => r.Selected);
        }

        private void menuitemSelectedSummary_Click(object sender, EventArgs e)
        {
            var stats = grid.GetRowValues<PerformerStatEx>(true).ToList();
            if (stats.Count == 0) 
                return;
            new PerformersSummaryForm(stats).ShowDialog();
        }
    }
}
