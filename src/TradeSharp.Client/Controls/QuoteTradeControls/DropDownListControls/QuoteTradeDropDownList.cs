using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Candlechart.ChartIcon;
using Entity;
using TradeSharp.Client.BL;
using TradeSharp.Client.Controls.QuoteTradeControls.ButtonClass;
using TradeSharp.Util;
using TradeSharp.Util.Forms;

namespace TradeSharp.Client.Controls.QuoteTradeControls.DropDownListControls
{
    /// <summary>
    /// Выпадающий список объёмов торгов
    /// </summary>
    public class QuoteTradeDropDownList : Control
    {
        /// <summary>
        /// Изменение выбранного объёма торгов 
        /// </summary>
        public event Action<int> CurrentSelectedItemChanged;

        /// <summary>
        /// Событие наступает при изменении возможных значений объёмов торгов т.е. при нажатии кнопкок "применить" или "применить ко всем" 
        /// </summary>
        public event Action VolumeByTickerChanged;

        /// <summary>
        /// Объём торгов, который выбран в текущий момент в выпадающем списке
        /// </summary>
        public QuoteTradeListItem CurrentSelectedItem { get; private set; }

        /// <summary>
        /// Текущий масштаб контрола
        /// </summary>
        public float Sx { get; set; }
        public float Sy { get; set; }

        #region Массивы точек контрола в оригинальном размере
        /// <summary>
        /// Точки треугольничка в оригинальном виде и размере
        /// </summary>
        private static readonly PointF[] triangleClosePointArray = new[]
                                                           {
                                                               new PointF(0, 0.8f), 
                                                               new PointF(1.0f, -0.8f), 
                                                               new PointF(-1.0f, -0.8f)
                                                           };
        /// <summary>
        /// точки треугольника отмасштабированные и развёрнутые в текушее положение - вверх/вниз
        /// </summary>
        private PointF[] trianglePointArray;

        /// <summary>
        /// Точки прямоугольника контрола в оригинальном виде и размере
        /// </summary>
        private static readonly PointF[] indicatorPointArray = new[]
                                                           {
                                                               new PointF(29.9f, 0.1f),
                                                               new PointF(29.9f, 9.9f),
                                                               new PointF(0.1f, 9.9f),
                                                               new PointF(0.1f, 0.1f)
                                                           };
        #endregion

        /// <summary>
        /// Размер шрифта
        /// </summary>
        public int TextSize { get; set; }
        
        /// <summary>
        /// Указывает, был ли изменён размер элемента. Это нужно для изменения размеров "выпадающих" элементов при клике.
        /// </summary>
        private bool mustUpdateSize;

        /// <summary>
        /// отступы треугольника от краёв контрола
        /// </summary>
        private Point TrianglePadding { get; set; }

        /// <summary>
        /// Ссылка на хранилище кистей
        /// </summary>
        public BrushesStorage Brushes { get; set; }
        private SolidBrush ForeGroundBrush { get; set; }
        private SolidBrush TriangleBrush { get; set; }
        private Pen TrianglePen { get; set; }
        private SolidBrush TextBrush { get; set; }      
        private DropDownList dropList;
        //private static List<QuoteTradeListItem> DefaultVolumeByTicker { get; set; }
        //private static Dictionary<string, int[]> CustomVolumeByTicker { get; set; }  

        private readonly Dictionary<DropDownListColor, Color> colors = new Dictionary<DropDownListColor, Color>
            {
                {DropDownListColor.ForeGroundBrushNormal, Color.FromArgb(190, 240, 190)},
                {DropDownListColor.ForeGroundBrushCovered, Color.FromArgb(170, 210, 170)},
                {DropDownListColor.TriangleBrushNormal, Color.FromArgb(25, 117, 48)},
                {DropDownListColor.TriangleBrushCovered, Color.FromArgb(255, 12, 237, 68)},
                {DropDownListColor.TrianglePen, Color.FromArgb(180, 45, 157, 48)},
                {DropDownListColor.TextBrushNormal, Color.Black},
                {DropDownListColor.TextBrushCovered, Color.DarkBlue}
            }; 

        public QuoteTradeDropDownList(BrushesStorage brushes)
        {
            Brushes = brushes;           
        }

        /// <summary>
        /// Установка размеров и положения контрола при его масштабировании
        /// </summary>
        /// <param name="scaleX">Новый масшаб по оси X</param>
        /// <param name="scaleY">Новый масшаб по оси Y</param>
        /// <param name="paddingLeftRight">Отступ родительского контрола (типа QuoteTradeControl) от левого края формы</param>
        /// <param name="paddingTop">Отступ родительского контрола (типа QuoteTradeControl) от верхнего края формы</param>
        public void SetDimensions(float scaleX, float scaleY, int paddingLeftRight, int paddingTop)
        {
            Sx = scaleX;
            Sy = scaleY;

            Width = Convert.ToInt32(2 * (QuotePartButtonFigure.OriginalVolumeSize.Width - 0.1) * Sx); //  "+ 0.1" Это поправка на округление Convert.ToInt32
            Height = Convert.ToInt32((QuotePartButtonFigure.OriginalVolumeSize.Height) * Sy);
            Left = Convert.ToInt32((QuotePartButtonFigure.OriginalVolumePadding.X + 0.17 + paddingLeftRight / 2f) * Sx);
            Top = Convert.ToInt32((QuotePartButtonFigure.OriginalVolumePadding.Y + 0.17 + paddingTop) * Sy);
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            TrianglePadding = new Point(25, 3);
            trianglePointArray = triangleClosePointArray;

            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;

            ForeGroundBrush = Brushes.GetBrush(colors[DropDownListColor.ForeGroundBrushNormal]) as SolidBrush;
            TriangleBrush = Brushes.GetBrush(colors[DropDownListColor.TriangleBrushNormal]) as SolidBrush;
            TrianglePen = new Pen(colors[DropDownListColor.TrianglePen], 1);
            TextBrush = Brushes.GetBrush(colors[DropDownListColor.TextBrushNormal]) as SolidBrush;

            var quoteTradeControl = Parent as QuoteTradeControl; //Получаем родительский контрол, что бы узнать текущую валютную пару
            var currentTicker = quoteTradeControl != null ? quoteTradeControl.Ticker : string.Empty;

            // Задаём возможные значения объёма торгов
            dropList = new DropDownList
                           {
                               CalcHeightAuto = true,
                               MaxLines = 15,
                               Values = GetQuoteTradeListItemsFromXml(currentTicker).Cast<object>().ToList(),
                               ClCellBack = Color.FromArgb(200, 128, 215, 128),
                               ClCellBackHl = Color.FromArgb(250, 128, 155, 128),
                               Tag = currentTicker
                           };
            dropList.cellClicked += DropListMouseUp;       

            // В случае ошибки приведения, присваиваем текущему значению первое из списка 
            if (quoteTradeControl == null)
            {
                CurrentSelectedItem = dropList.Values[0] as QuoteTradeListItem;
                return;
            }

            // Если нормально получили ссылку на родительский контрол то сохранённое значение для валютной пары этого контрола  
            CurrentSelectedItem = dropList.Values.FirstOrDefault(x =>
                {
                    var quoteTradeListItem = x as QuoteTradeListItem;
                    return quoteTradeListItem != null && quoteTradeListItem.VolumeTrade == 
                        UserSettings.Instance.FastDealSelectedVolumeDict.FirstOrDefault(y => y.Key == currentTicker).Value;
                }) as QuoteTradeListItem ?? dropList.Values[0] as QuoteTradeListItem;
        }

        /// <summary>
        /// Проверяем, есть ли кастомные и дефолтные значения объёмов торгов - если есть то грузим (причём только один раз т.к. переменная статическая)
        /// </summary>
        /// <param name="currentTicker">Текуший инструмент (валютная пара)</param>
        /// <returns>Коллкция объёмов торгов для выпадающего списка</returns>
        private static IEnumerable<QuoteTradeListItem> GetQuoteTradeListItemsFromXml(string currentTicker)
        {
            var customVolumeByTicker = UserSettings.Instance.VolumeByTicker;
            var tradeVolumes = customVolumeByTicker.FirstOrDefault(x => x.Key == currentTicker).Value != null
                                   ? customVolumeByTicker.FirstOrDefault(x => x.Key == currentTicker).Value.
                                                          Select(x => new QuoteTradeListItem(x)).ToList()
                                   : new List<QuoteTradeListItem>();
            if (tradeVolumes.Count <= 0)
            {
                var defaultVolumeByTicker = UserSettings.Instance.FastDealVolumes.Select(x => new QuoteTradeListItem(x)).Any()
                            ? UserSettings.Instance.FastDealVolumes.Select(x => new QuoteTradeListItem(x)).ToList()
                            : new List<QuoteTradeListItem> {new QuoteTradeListItem(10000)};
                        // Это на всякий случай (скорее всего это значение никогда не подставиться)
                tradeVolumes = defaultVolumeByTicker.Select(item => (QuoteTradeListItem) item.Clone()).ToList();
            }

            tradeVolumes.Add(new QuoteTradeListItem("..."));
            return tradeVolumes;
        }

        /// <summary>
        /// Обработчик выбора элемента списка объёмов торгов. Запоминаем текущий выбранный объём торгов
        /// </summary>
        void DropListMouseUp(object selObj, string selText) //object sender, MouseEventArgs e)
        {
            if (selText == "...")
            {
                var setCustomValueDialogForm = new TickerUpdateDialog
                {
                    CustomVolume = string.Join(", ", dropList.Values.Where(y =>
                    {
                        var quoteTradeListItem = y as QuoteTradeListItem;
                        return quoteTradeListItem != null && quoteTradeListItem.IsHaveValue();
                    }).Select(x =>
                    {
                        var quoteTradeListItem = x as QuoteTradeListItem;
                        return quoteTradeListItem != null ? quoteTradeListItem.VolumeTrade.ToString(CultureInfo.InvariantCulture) : null;
                    }))
                };
                if (setCustomValueDialogForm.ShowDialog() == DialogResult.OK)
                {
                    var volumes = setCustomValueDialogForm.CustomVolume.ToIntArrayUniform();
                    if (setCustomValueDialogForm.acceptAll)
                    {
                        UserSettings.Instance.FastDealVolumes = volumes;
                        UserSettings.Instance.VolumeByTicker = new Dictionary<string, int[]>();
                    }
                    else
                    {
                        var customVolumes = UserSettings.Instance.VolumeByTicker;
                        if (customVolumes.FirstOrDefault(x => x.Key == dropList.Tag.ToString()).Value != null)
                            customVolumes[dropList.Tag.ToString()] = volumes;
                        else
                            customVolumes.Add(dropList.Tag.ToString(), volumes);
                        UserSettings.Instance.VolumeByTicker = customVolumes;
                    }
                    UserSettings.Instance.SaveSettings();
                    if (VolumeByTickerChanged != null) VolumeByTickerChanged();
                }
                return;
            }

            CurrentSelectedItem = (QuoteTradeListItem) selObj;
            var volm = CurrentSelectedItem.VolumeTrade;
            if (CurrentSelectedItemChanged != null)
                CurrentSelectedItemChanged(volm);
            Invalidate();            
        }

        /// <summary>
        /// Прочитать ещё раз из xml файла значения возможных объёмов торгов и перезаполнить выпадающий список.
        /// </summary>
        public void VolumeByTickerUpdate()
        {
            dropList.Values = GetQuoteTradeListItemsFromXml(dropList.Tag.ToString()).Cast<object>().ToList();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var points = indicatorPointArray.Select(p => new PointF(p.X * Sx, p.Y * Sy)).ToArray();
            e.Graphics.FillPolygon(ForeGroundBrush, points);

            points = trianglePointArray.Select(p => new PointF((p.X + TrianglePadding.X) * Sx, (p.Y + TrianglePadding.Y) * Sy)).ToArray();
            e.Graphics.FillPolygon(TriangleBrush, points);
            e.Graphics.DrawPolygon(TrianglePen, points);

            e.Graphics.DrawString(CurrentSelectedItem.Text, new Font("Open Sans", TextSize == 0 ? 1 : TextSize, FontStyle.Bold), TextBrush, Sx, Sy * 1.5f);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            var containerPoints = indicatorPointArray.Select(p => new PointF(p.X * Sx, p.Y * Sy)).ToArray();
            var containerPath = new GraphicsPath();
            containerPath.AddPolygon(containerPoints);
            if (!containerPath.IsVisible(e.Location)) return;


            for (var i = 0; i < trianglePointArray.Length; i++) // Переварачиваем треугольник
                trianglePointArray[i] = new PointF(triangleClosePointArray[i].X, -1 * triangleClosePointArray[i].Y);

            // Если размеры выпадающего списка обновились, то нужно обновить размеры выпадающих элементов и размеры контейнера
            if (mustUpdateSize)
            {
                dropList.Width = Width;
                mustUpdateSize = false;
            }

            var volumeQuoteDropDownList = new DropDownListPopup(dropList);  
            
            volumeQuoteDropDownList.Show(new Point(PointToScreen(ClientRectangle.Location).X, PointToScreen(ClientRectangle.Location).Y + ClientRectangle.Height));
            volumeQuoteDropDownList.Closed += VolumeQuoteDropDownListClosed;
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            mustUpdateSize = true; // при следующей перерисовке в методе "OnPaint" размеры элементов контрола будут пересчитаны
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            ForeGroundBrush = Brushes.GetBrush(colors[DropDownListColor.ForeGroundBrushCovered]) as SolidBrush;
            TriangleBrush = Brushes.GetBrush(colors[DropDownListColor.TriangleBrushCovered]) as SolidBrush;
            TextBrush = Brushes.GetBrush(colors[DropDownListColor.TextBrushCovered]) as SolidBrush;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            ForeGroundBrush = Brushes.GetBrush(colors[DropDownListColor.ForeGroundBrushNormal]) as SolidBrush;
            TriangleBrush = Brushes.GetBrush(colors[DropDownListColor.TriangleBrushNormal]) as SolidBrush;
            TextBrush = Brushes.GetBrush(colors[DropDownListColor.TextBrushNormal]) as SolidBrush;
            Invalidate();
        }

        private void VolumeQuoteDropDownListClosed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            for (var i = 0; i < trianglePointArray.Length; i++) // Переварачиваем треугольник в начальное положение
                trianglePointArray[i] = new PointF(triangleClosePointArray[i].X, -1 * triangleClosePointArray[i].Y);
            Invalidate();
        }
    }
}