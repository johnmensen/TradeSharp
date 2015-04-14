using System;
using System.Collections.Generic;
using System.Drawing;
using Entity;
using TradeSharp.Util;

namespace Candlechart.ChartIcon
{
    /// <summary>
    /// иконка, рисуемая поверх графика, реагирующая на наведение / нажатие,
    /// в том числе, как вариант, выводящая выпадающий список
    /// </summary>
    public class ChartIcon
    {
        public enum ImageState { Normal = 0, Highlighted = 1, Pressed = 2 }

        private static readonly Dictionary<string, ButtonImageCollection> 
            buttonImage = new Dictionary<string, ButtonImageCollection>();

        public static readonly string chartButtonIndexAutoScroll = "a";
        
        public static readonly string chartButtonIndicators = "i";

        public static readonly string chartButtonNewOrder = "o";

        public static readonly string chartButtonQuoteArchive = "q";

        public static readonly string chartButtonFavIndicators = "f";

        public static readonly string chartButtonFastSell = "fs";

        public static readonly string chartButtonFastBuy = "fb";

        public static readonly string chartButtonPatchQuotes = "pq";

        public static readonly string chartButtonDealByTicker = "d";

        public static readonly string chartButtonStatByTicker = "s";

        public static readonly string chartVisualSettings = "v";

        public static readonly string chartProfitByTicker = "p";

        public static readonly string chartRobot = "ro";
        
        public string key;

        public EventHandler click;

        public Point Position { get; set; }

        public virtual Size Size
        {
            get; set; 
        }

        public ImageState State { get; set; }

        public bool IsIn(int x, int y)
        {
            return (x >= Position.X && x <= (Position.X + Size.Width)) &&
                   (y >= Position.Y && y <= (Position.Y + Size.Height));
        }

        /// <summary>
        /// 0 - обычная (светлая подложка), 1 - темная подложка
        /// </summary>
        public int ColorThemeIndex { get; set; }

        protected int imageIndex;

        public void SetThemeByBackColor(Color backColor)
        {
            var isBlackBack = (backColor.R + backColor.G + backColor.B) < 128 * 3;
            ColorThemeIndex = isBlackBack ? 1 : 0;
        }

        public ChartIcon() {}

        public ChartIcon(EventHandler click)
        {
            this.click = click;
        }

        public static void AddButtonImages(string key, ButtonImageCollection images)
        {
            buttonImage.Add(key, images);
        }

        /// <summary>
        /// NB! click handler is not copied
        /// </summary>        
        public virtual ChartIcon MakeCopy()
        {
            var cpy = new ChartIcon(null) {key = key, State = State, Size = Size, Position = Position};
            return cpy;
        }

        public virtual bool OnMouseDown(int x, int y)
        {
            if (!IsIn(x, y)) return false;
            return true;
        }

        public virtual void OnMouseClick()
        {
            if (click != null)
                click(this, EventArgs.Empty);
        }

        public virtual void Draw(Graphics g, BrushesStorage brushes, PenStorage pens)
        {
            var images = buttonImage[key];
            
            // нарисовать подложку и обводку
            if (State == ImageState.Pressed)
            {
                var brush =
                    brushes.GetBrush(Color.FromArgb(128, ColorThemeIndex == 0 ? Color.Black : Color.White));
                g.FillRectangle(brush, Position.X, Position.Y, Size.Width - 1, Size.Height - 1);
            }
            if (State == ImageState.Highlighted || State == ImageState.Pressed)
            {
                var pen = pens.GetPen(ColorThemeIndex == 0 ? Color.Black : Color.White);
                g.DrawRectangle(pen, Position.X, Position.Y, Size.Width - 1, Size.Height - 1);
            }

            // нарисовать картинку
            var img = images.images[ColorThemeIndex, imageIndex];
            var padding = (Size.Width - img.Width) / 2;
            g.DrawImage(img, Position.X + padding, Position.Y + padding, img.Width, img.Height);
        }

        public static string GetNameByKey(string key)
        {
            if (key == chartButtonStatByTicker) return Localizer.GetString("TitleInstrumentStatistics");
            if (key == chartButtonDealByTicker) return Localizer.GetString("TitleDealsWithInstrument");
            if (key == chartButtonIndexAutoScroll) return Localizer.GetString("TitleAutoscrollChart");
            if (key == chartButtonIndicators) return Localizer.GetString("TitleIndicators");
            if (key == chartButtonNewOrder) return Localizer.GetString("TitleNewOrder");
            if (key == chartButtonQuoteArchive) return Localizer.GetString("TitleQuoteArchive");
            if (key == chartButtonFavIndicators) return Localizer.GetString("TitleChosenIndicators");
            if (key == chartButtonFastBuy) return Localizer.GetString("TitleBuyQuick");
            if (key == chartButtonFastSell) return Localizer.GetString("TitleSellQuick");
            if (key == chartButtonPatchQuotes) return Localizer.GetString("TitleSynchronizeHistory");
            if (key == chartVisualSettings) return Localizer.GetString("TitleVisualSettings");
            if (key == chartProfitByTicker) return Localizer.GetString("TitleProfitWithInstrument");
            if (key == chartRobot) return Localizer.GetString("TitleRobots");
            return "-";
        }

        public class ButtonImageCollection
        {
            /// <summary>
            /// картинки. первый индекс - индекс темы (строго 2 массива)
            /// в каждом из двух массивов - картинки для определенного состояния кнопки
            /// </summary>
            public Bitmap[,] images;
        }
    }
}
