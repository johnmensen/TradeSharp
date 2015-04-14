using System;
using System.Drawing;
using System.Windows.Forms;
using Entity;
using TradeSharp.Client.BL;
using TradeSharp.Client.Controls.QuoteTradeControls.ButtonClass;
using TradeSharp.Client.Controls.QuoteTradeControls.DropDownListControls;


namespace TradeSharp.Client.Controls.QuoteTradeControls
{
    public class QuoteTradeControl : Control
    {
        /// <summary>
        /// Изменение значения выбранного объёма торгов
        /// </summary>
        public Action<int> selectedVolumeChanged;

        /// <summary>
        /// делегат для изменении возможных значений объёмов торгов т.е. при нажатии кнопкок "применить" или "применить ко всем" 
        /// </summary>
        public Action volumeByTickerChangedDelegate;

        /// <summary>
        /// Событие осуществления покупки или продажи кнопками "SELL" и "BUY". В качестве параметров передаются тип команды (купить/продать), 
        /// объём операции выбранный в выпадающем списке "QuoteTradeDropDownList" и наименование текущей валютной пары
        /// </summary>
        public event Action<DealType, float, string> OnQuoteTradeClick;

        /// <summary>
        /// Наступает при попытке закрытия контрола
        /// </summary>
        public event Action<Control> OnClosing;    
    
        // отступы в условных единицах
        private const int PaddingTopProportion = 7;
        private const int PaddingBottonProportion = 4;
        private const int PaddingLeftRightProportion = 4;
        private const int ButtonCloseSizeProportion = 4;
        private const int PaddingRightButtonCloseProportion = 2;

        public static Image imageCloseLight;
        public static Image imageCloseNomal;

        /// <summary>
        /// Текущая валютная пара
        /// </summary>
        public string Ticker { get; set; }

        public int Precision { get { return precision; } }

        /// <summary>
        /// Размер контрола
        /// </summary>
        public int CellSize
        {
            get { return cellSize; }
            set
            {
                if (cellSize == value) return;
                cellSize = value;
                SetSize(cellSize);
            }
        }

        /// <summary>
        /// Размер шапки, который расчитан на текущий момент
        /// </summary>
        public float CalculatedHaderHeight
        {
            get { return (PaddingBottonProportion + 1.5f)* scaleY; }
        }

        /// <summary>
        /// Изменение состояния при перетаскивании
        /// </summary>
        public DragDropState CurrentDragDropState
        {
            set
            {
                if (currentDragDropState == value) return;
                currentDragDropState = value;
                Invalidate(new Rectangle(Convert.ToInt32(PaddingLeftRightProportion / 2 * scaleX) - 1,
                                         PaddingBottonProportion - 1,
                                         Convert.ToInt32(Width - PaddingLeftRightProportion * scaleX) + 3, (int)CalculatedHaderHeight + 3));
            }
            get { return currentDragDropState; }
        }


        /// <summary>
        /// Текущий bid
        /// </summary>
        private float Bid
        {
            get { return bid ?? 0; }
            set
            {
                bid = value;
                if (leftButton == null) return;
                leftButton.Value = value;
            }
        }

        /// <summary>
        /// Текущий ask
        /// </summary>
        private float Ask
        {
            get { return ask ?? 0; }
            set
            {
                ask = value;
                if (rightButton == null) return;
                rightButton.Value = value;
            }
        }

        private float scaleX;
        private float scaleY;
        private float? bid;
        private float? ask;
        private int cellSize;
        private readonly int precision;
        private DragDropState currentDragDropState = DragDropState.Normal;
        private readonly BrushesStorage brushes = new BrushesStorage();
        private readonly PenStorage pens = new PenStorage();

        //контролы
        private QuotePartButton leftButton;
        private QuotePartButton rightButton;
        private SpreadIndicator spreadIndicator;
        private PictureBox btnClose;
        private QuoteTradeDropDownList volumeQuoteDropDownList; // Выпадающий список с объёмом торгов 

        public QuoteTradeControl(QuoteTableCellSettings sets, int cellSize, Action<int> selectedVolumeChanged, Action volumeByTickerChangedDelegate)
        {
            this.selectedVolumeChanged = selectedVolumeChanged;
            this.volumeByTickerChangedDelegate = volumeByTickerChangedDelegate;

            Location = new Point(sets.X, sets.Y);
            Name = string.Format("{0}{1}", "quoteTradeControl", sets.Ticker) ;

            SetSize(cellSize);
            Text = Ticker = sets.Ticker;
            precision = sets.Precision;
        }

        private void SetSize(int size)
        {
            Size = new Size(120 + 120 * size / 100, 90 + 90 * size / 100);
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            leftButton = new QuotePartButton(scaleX, scaleY, brushes, pens) { Value = ask };
            rightButton = new QuotePartButton(scaleX, scaleY, brushes, pens, false) { Value  = bid};

            btnClose = new PictureBox
                           {
                               Image = imageCloseNomal,
                               SizeMode = PictureBoxSizeMode.Zoom
                           };
            btnClose.Click += BtnCloseClick;
            btnClose.MouseEnter += BtnCloseMouseEnter;
            btnClose.MouseLeave += BtnCloseMouseLeave;
            Controls.Add(btnClose);

            spreadIndicator = new SpreadIndicator(new Rectangle(QuotePartButtonFigure.SpreadPosition.X,
                                                                QuotePartButtonFigure.SpreadPosition.Y, 
                                                                QuotePartButtonFigure.SpreadWidth,
                                                                QuotePartButtonFigure.SpreadHeight),
                                                                Ticker, Ask - Bid);

            volumeQuoteDropDownList = new QuoteTradeDropDownList(brushes);
            Controls.Add(volumeQuoteDropDownList);
            volumeQuoteDropDownList.MouseEnter += VolumeQuoteDropDownListMouseEnter;
            volumeQuoteDropDownList.CurrentSelectedItemChanged += VolumeQuoteDropDownListCurrentSelectedItemChanged;
            volumeQuoteDropDownList.VolumeByTickerChanged += QuoteDropDownListVolumeByTickerChanged;
            SetDimensions();
        }

        /// <summary>
        /// Изменение списка возможных значений объёма торгов
        /// </summary>
        private void QuoteDropDownListVolumeByTickerChanged()
        {
            if (volumeByTickerChangedDelegate != null) volumeByTickerChangedDelegate();
        }

        /// <summary>
        /// Обновить выпадающий список возможных объёмов торгов новыми значениями из файла settings.xml
        /// </summary>
        public void VolumeByTickerUpdate()
        {
            volumeQuoteDropDownList.VolumeByTickerUpdate();
        }

        /// <summary>
        /// Обработчик события изменения текущего объёма торгов. 
        /// </summary>
        /// <param name="obj">текущий выбранный объём торгов</param>
        private void VolumeQuoteDropDownListCurrentSelectedItemChanged(int obj)
        {
            selectedVolumeChanged(obj); // Просто вызываем делегат (анонимный метод), переданный в конструкторе
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            spreadIndicator.Draw(e.Graphics, brushes);
            leftButton.Draw(e.Graphics);
            rightButton.Draw(e.Graphics);


            Brush tickerBrush = null;
            //Отрисовываем шапку контрола
            switch (currentDragDropState)
            {
                case DragDropState.Normal:
                    tickerBrush = brushes.GetBrush(Color.Black);
                    break;
                case DragDropState.Washy:
                    tickerBrush = brushes.GetBrush(Color.FromArgb(100, Color.DarkGray));
                    break;
                case DragDropState.InFrame:
                    e.Graphics.DrawRectangle(pens.GetPen(Color.Black), PaddingLeftRightProportion / 2 * scaleX, PaddingBottonProportion,
                                                               Width - PaddingLeftRightProportion * scaleX, CalculatedHaderHeight);
                    tickerBrush = brushes.GetBrush(Color.Black);
                    break;
                default:
                    tickerBrush = brushes.GetBrush(Color.Black);
                    break;
            }
            
            //Отрисовываем заголовок валютной пары
            if (tickerBrush != null)
                e.Graphics.DrawString(Ticker, new Font("Times New Roman", Height < Width ? 1 + Height /9 : Width / 9, FontStyle.Bold), tickerBrush, PaddingBottonProportion, 0);

            //Отрисовываем кнопку закрытия
            var buttonCloseScale = scaleX < scaleY ? scaleX : scaleY;
            btnClose.Location = new Point(Convert.ToInt32((QuotePartButtonFigure.OriginalWidth * 2 - PaddingLeftRightProportion / 2 - PaddingRightButtonCloseProportion) * scaleX), Convert.ToInt32(2f * scaleY));
            btnClose.Size = new Size(Convert.ToInt32(ButtonCloseSizeProportion * buttonCloseScale), Convert.ToInt32(ButtonCloseSizeProportion * buttonCloseScale));
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            scaleX = (float)Width / (QuotePartButtonFigure.OriginalWidth * 2 + PaddingLeftRightProportion);
            scaleY = (float)Height / (QuotePartButtonFigure.OriginalHeight + PaddingTopProportion + PaddingBottonProportion);

            if (leftButton != null && rightButton != null)
            {
                leftButton.ScaleX = rightButton.ScaleX = scaleX;
                leftButton.ScaleY = rightButton.ScaleY = scaleY;
                leftButton.Value = rightButton.Value = null;
            }

            SetDimensions();
            Invalidate();
        }

        #region подсветка кнопок при наведении мыши и нажимании       
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            switch (e.Button)
            {
                case MouseButtons.Left:
                    var graphic = CreateGraphics();
                    if (leftButton.Shape.IsVisible(e.Location))
                    {
                        leftButton.MousePressed = true;
                        leftButton.Draw(graphic);
                    }
                    if (rightButton.Shape.IsVisible(e.Location))
                    {
                        rightButton.MousePressed = true;
                        rightButton.Draw(graphic);
                    }
                    break;
                case MouseButtons.Right:
                    break;
            }
        }
        
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            leftButton.MousePressed = rightButton.MousePressed = false;
            var graphic = CreateGraphics();
            if (leftButton.Shape.IsVisible(e.Location))
            {
                leftButton.Draw(graphic);
                if (OnQuoteTradeClick != null)
                    OnQuoteTradeClick(DealType.Sell, volumeQuoteDropDownList.CurrentSelectedItem.VolumeTrade, Ticker);
            }
            if (rightButton.Shape.IsVisible(e.Location))
            {
                rightButton.Draw(graphic);
                if (OnQuoteTradeClick != null)
                    OnQuoteTradeClick(DealType.Buy, volumeQuoteDropDownList.CurrentSelectedItem.VolumeTrade, Ticker);
            }
        }
        
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (leftButton == null || leftButton.Shape == null) return;
            if (leftButton.Shape.IsVisible(e.Location))
            {
                if (!leftButton.MouseAboveButton)
                {
                    leftButton.MouseAboveButton = true;
                    leftButton.Draw(CreateGraphics());
                }
            }
            else
            {
                if (leftButton.MouseAboveButton)
                {
                    leftButton.MouseAboveButton = false;
                    leftButton.Draw(CreateGraphics());
                }
            }

            if (rightButton == null || rightButton.Shape == null) return;
            if (rightButton.Shape.IsVisible(e.Location))
            {
                if (!rightButton.MouseAboveButton)
                {
                    rightButton.MouseAboveButton = true;
                    rightButton.Draw(CreateGraphics());
                }
            }
            else
            {
                if (rightButton.MouseAboveButton)
                {
                    rightButton.MouseAboveButton = false;
                    rightButton.Draw(CreateGraphics());
                }
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            leftButton.MouseAboveButton = false;
            leftButton.Draw(CreateGraphics());

            rightButton.MouseAboveButton = false;
            rightButton.Draw(CreateGraphics());

        }

        private void VolumeQuoteDropDownListMouseEnter(object sender, EventArgs e)
        {
            if (leftButton.MouseAboveButton)
            {
                leftButton.MouseAboveButton = false;
                leftButton.Draw(CreateGraphics());
            }
            if (rightButton.MouseAboveButton)
            {
                rightButton.MouseAboveButton = false;
                rightButton.Draw(CreateGraphics());
            }
        }

        /// <summary>
        /// Перерисовка кнопки закрытия
        /// </summary>
        private void BtnCloseMouseLeave(object sender, EventArgs e)
        {
            btnClose.Image = imageCloseNomal;
        }

        /// <summary>
        /// Перерисовка кнопки закрытия
        /// </summary>
        private void BtnCloseMouseEnter(object sender, EventArgs e)
        {
            btnClose.Image = imageCloseLight;
        }
        #endregion
        
        /// <summary>
        /// Установка размеров и выравнивание элементов.
        /// </summary>
        private void SetDimensions()
        {
            if (volumeQuoteDropDownList != null)
            {
                volumeQuoteDropDownList.TextSize = Height < Width ? Height / 15 : Width / 15;
                volumeQuoteDropDownList.SetDimensions(scaleX, scaleY, PaddingLeftRightProportion, PaddingTopProportion);
            }     
   
            if (spreadIndicator != null) spreadIndicator.SetDimensions(scaleX, scaleY, PaddingLeftRightProportion, PaddingTopProportion);
        }

        /// <summary>
        /// Перерисовка всего контрола
        /// </summary>
        private void Redraw()
        {
            spreadIndicator.Draw(CreateGraphics(), brushes);
            leftButton.Draw(CreateGraphics());
            rightButton.Draw(CreateGraphics());
        }

        /// <summary>
        /// Закрытие контрола
        /// </summary>
        private void BtnCloseClick(object sender, EventArgs e)
        {
            if (OnClosing != null) 
                OnClosing(this);
            Controls.Clear();
            brushes.Dispose();
        }

        public void SetPrice(float bid, float ask)
        {
            if (Ask == ask && Bid == bid) return;

            Ask = ask;
            Bid = bid;

            spreadIndicator.Value = Ask - Bid;
            Redraw();
        }
    }
}