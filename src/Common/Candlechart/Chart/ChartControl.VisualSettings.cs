using System;
using System.ComponentModel;
using System.Drawing;
using Candlechart.Theme;

namespace Candlechart.Chart
{    
    /// <summary>
    /// настройки рисования чарта
    /// </summary>
    public partial class ChartControl
    {
        public ChartVisualSettings visualSettings;

        public enum Themes
        {
            Standard = 0, Contrast = 1, Inverted = 3, Gray = 4
        }

        public class ChartVisualSettings
        {
            private readonly ChartControl owner;

            public const float DarkThreshold = 0.3f;

            public const float LightThreshold = 0.75f;

            #region Данные

            private string backgroundImageResourceName;
            public string BackgroundImageResourceName
            {
                get { return backgroundImageResourceName; }
                set
                {
                    backgroundImageResourceName = value;
                    if (owner != null && owner.Panes != null)
                        if (owner.StockPane != null)
                        {
                            owner.StockPane.BackgroundImageResourceName = BackgroundImageResourceName;
                            owner.StockPane.LoadBackImage(BackgroundImageResourceName);
                        }
                }
            }

            [Description("Specifies the background color of the chart control")]
            public Color ChartBackColor
            {
                get
                {
                    return owner == null ? Color.Empty : owner.BackColor;
                }
                set
                {
                    if (owner != null)
                        owner.BackColor = value;
                }
            }

            [Description("Specifies the border style of the chart control")]
            public FrameStyle ChartFrameStyle { get; set; }

            [Description("Specifies the background color of the panes")]
            public Color PaneBackColor { get; set; }

            [Description("Specifies the border color of the pane frame")]
            public Color PaneFrameBorderColor { get; set; }

            [Description("Specifies the border color of the pane frame (margins of linear gradient)")]
            public Color PaneFrameBorderMarginGradientColor { get; set; }

            [Description("Specifies the border color of the pane frame (center of linear gradient)")]
            public Color PaneFrameBorderCenterGradientColor { get; set; }

            [Description("Specifies the background color of the pane frame")]
            public Color PaneFrameBackColor { get; set; }

            [Description("Specifies the text color of the pane frame")]
            public Color PaneFrameTextColor { get; set; }

            [Description("Specifies the text color of the pane frame inside pane")]
            public Color PaneFrame2TextColor { get; set; }

            private Font paneFrameFont;
            [Description("Specifies the font used in the pane frame")]
            public Font PaneFrameFont
            {
                get { return paneFrameFont; }
                set
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException("PaneFrameFont", "Font cannot be null.");
                    }
                    paneFrameFont = value;
                }
            }

            [Description("Specifies the visibility of the pane frame title box")]
            public bool PaneFrameTitleBoxVisible { get; set; }

            [Description("Specifies the outline color used by PaneFrameTool")]
            public Color PaneFrameToolOutlineColor { get; set; }

            [Description("Specifies the background color of the Y axis")]
            public Color YAxisBackColor { get; set; }

            [Description("Specifies the foreground color of the Y axis")]
            public Color YAxisForeColor { get; set; }

            [Description("Specifies the text color of the Y axis")]
            public Color YAxisTextColor { get; set; }

            private Font yAxisFont;
            [Description("Specifies the font used in the Y axis")]
            public Font YAxisFont
            {
                get { return yAxisFont; }
                set
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException("YAxisFont", "Font cannot be null.");
                    }
                    yAxisFont = value;
                }
            }

            [Description("Specifies whether the Y axis grid bands are visible")]
            public bool YAxisGridBandVisible { get; set; }

            [Description("Specifies the grid band color of the Y axis")]
            public Color YAxisGridBandColor { get; set; }

            [Description("Specifies whether the Y axis grid lines are visible")]
            public bool YAxisGridLineVisible { get; set; }

            [Description("Specifies the type of grid line style for the Y axis")]
            public GridLineStyle YAxisGridLineStyle { get; set; }

            [Description("Specifies the grid line color of the Y axis")]
            public Color YAxisGridLineColor { get; set; }

            [Description("Specifies whether the major ticks are visible on the Y axis")]
            public bool YAxisMajorTickVisible { get; set; }

            [Description("Specifies whether the minor ticks are visible on the Y axis")]
            public bool YAxisMinorTickVisible { get; set; }

            [Description("Specifies the background color of the exponent label")]
            public Color ExponentLabelBackColor { get; set; }

            [Description("Specifies the foreground color of the exponent label")]
            public Color ExponentLabelForeColor { get; set; }

            [Description("Specifies the text color of the exponent label")]
            public Color ExponentLabelTextColor { get; set; }

            private Font exponentLabelFont;
            [Description("Specifies the font used in the exponent label")]
            public Font ExponentLabelFont
            {
                get { return exponentLabelFont; }
                set
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException("ExponentLabelFont", "Font cannot be null.");
                    }
                    exponentLabelFont = value;
                }
            }

            [Description("Specifies the background color of the X axis")]
            public Color XAxisBackColor { get; set; }

            [Description("Specifies the foreground color of the X axis")]
            public Color XAxisForeColor { get; set; }

            [Description("Specifies the text color of the X axis")]
            public Color XAxisTextColor { get; set; }

            [Description("Specifies the font used in the X axis")]
            public Font XAxisFont
            {
                get { return xAxisFont; }
                set
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException("XAxisFont", "Font cannot be null.");
                    }
                    xAxisFont = value;
                }
            }

            [Description("Specifies whether the X axis grid bands are visible")]
            public bool XAxisGridBandVisible { get; set; }

            [Description("Specifies the grid band color of the X axis")]
            public Color XAxisGridBandColor { get; set; }

            [Description("Specifies whether the X axis grid lines are visible")]
            public bool XAxisGridLineVisible { get; set; }

            [Description("Specifies the type of grid line style for the X axis")]
            public GridLineStyle XAxisGridLineStyle { get; set; }

            [Description("Specifies the grid line color of the X axis")]
            public Color XAxisGridLineColor { get; set; }

            [Description("Specifies whether to turn on anti-aliasing for the series")]
            public bool SeriesAntiAlias { get; set; }

            [Description("Specifies the background color of the series")]
            public Color SeriesBackColor { get; set; }

            [Description("Specifies the foreground color of the series")]
            public Color SeriesForeColor { get; set; }

            [Description("Specifies the line width of the series")]
            public float SeriesLineWidth { get; set; }

            [Description("Specifies the line style of the series")]
            public LineStyle SeriesLineStyle { get; set; }

            [Description("Specifies the fill color of up-closing bars of the stock series")]
            public Color StockSeriesUpFillColor { get; set; }

            [Description("Specifies the line color of the up-closing bars of the stock series")]
            public Color StockSeriesUpLineColor { get; set; }

            [Description("Specifies the fill color of down-closing bars of the stock series")]
            public Color StockSeriesDownFillColor { get; set; }

            [Description("Specifies the line color of down-closing bars of the stock series")]
            public Color StockSeriesDownLineColor { get; set; }

            [Description("Specifies the outline color for growing bars")]
            public Color BarShadowColorUp { get; set; }

            [Description("Specifies the outline color for falling bars")]
            public Color BarShadowColorDown { get; set; }

            [Description("Specifies the outline color for bars (not candles)")]
            public Color BarNeutralColor { get; set; }

            [Description("Color theme")]
            public Themes Theme { get; set; }

            private Font xAxisFont;
            #endregion

            public ChartVisualSettings()
            {                
            }

            public ChartVisualSettings(ChartControl owner)
            {
                this.owner = owner;
                ApplyTheme();
            }

            /// <summary>
            /// если цвета совпадают - сделать их контрастными (изменить цвет cl)
            /// </summary>            
            public static Color AdjustColor(Color cl, Color back)
            {
                var ownR = cl.R;
                var ownG = cl.G;
                var ownB = cl.B;

                var deltaCl = (ownR - back.R) * (ownR - back.R) +
                              (ownB - back.B) * (ownB - back.B) + (ownG - back.G) * (ownG - back.G);
                if (deltaCl > 1000) return cl;

                if (ownR > 115 && ownR < 141 && ownB > 115 && ownB < 141 && ownG > 115 && ownG < 141)
                    return Color.Black;
                
                return Color.FromArgb(255 - cl.R, 255 - cl.G, 255 - cl.B);
            }

            #region Темы
            public void ApplyTheme()
            {
                switch (Theme)
                {
                    case Themes.Standard:
                        ApplyStandardTheme();
                        break;
                    case Themes.Contrast:
                        ApplyContrastTheme();
                        break;
                    case Themes.Inverted:
                        ApplyContrastInverseTheme();
                        break;
                    case Themes.Gray:
                        ApplyGreyTheme();
                        break;
                }
            }

            private void ApplyStandardTheme()
            {
                var forecl = Color.FromArgb(255, 255, 255);
                var fadecl = Color.FromArgb(240, 240, 240);
                var lightcl = Color.FromArgb(255, 255, 255);
                var dimcl = Color.FromArgb(220, 220, 220);
                var pencl = Color.FromArgb(38, 38, 38);

                BackgroundImageResourceName = "Candlechart.images.terminal_logo_main.png";
                ChartBackColor = SystemColors.Control;
                ChartFrameStyle = FrameStyle.ThreeD;
                PaneBackColor = forecl;
                PaneFrameBorderColor = fadecl;
                PaneFrameBorderMarginGradientColor = dimcl;
                PaneFrameBorderCenterGradientColor = fadecl;
                PaneFrameBackColor = lightcl;
                PaneFrameTextColor = Color.Gray;
                PaneFrame2TextColor = Color.Black;
                PaneFrameFont = new Font(FontFamily.GenericSansSerif, 9.5f, FontStyle.Regular);
                PaneFrameTitleBoxVisible = true;
                PaneFrameToolOutlineColor = Color.Black;
                YAxisBackColor = lightcl;
                YAxisForeColor = Color.Black;
                YAxisTextColor = pencl;
                YAxisFont = new Font(FontFamily.GenericSansSerif, 9.5f);
                YAxisGridBandVisible = false;
                YAxisGridBandColor = Color.FromArgb(30, fadecl);
                YAxisGridLineVisible = true;
                YAxisGridLineStyle = GridLineStyle.Dot;
                YAxisGridLineColor = fadecl;
                YAxisMajorTickVisible = true;
                YAxisMinorTickVisible = true;
                ExponentLabelBackColor = fadecl;
                ExponentLabelForeColor = Color.Black;
                ExponentLabelTextColor = Color.Brown;
                ExponentLabelFont = new Font(FontFamily.GenericSansSerif, 9.5f);
                XAxisBackColor = fadecl;
                XAxisForeColor = Color.Black;
                XAxisTextColor = pencl;
                XAxisFont = new Font(FontFamily.GenericSansSerif, 9.5f);
                XAxisGridBandVisible = false;
                XAxisGridBandColor = Color.FromArgb(30, fadecl);
                XAxisGridLineVisible = false;
                XAxisGridLineStyle = GridLineStyle.Dot;
                XAxisGridLineColor = forecl;
                SeriesAntiAlias = true;
                SeriesBackColor = dimcl;
                SeriesForeColor = pencl;
                SeriesLineWidth = 1f;
                SeriesLineStyle = LineStyle.Solid;
                StockSeriesUpFillColor = Color.Empty;
                StockSeriesUpLineColor = Color.Black;
                StockSeriesDownFillColor = Color.Gray;
                StockSeriesDownLineColor = Color.Black;
                BarNeutralColor = Color.Gray;
            }

            private void ApplyContrastTheme()
            {
                var forecl = Color.FromArgb(0, 0, 0);
                var fadecl = Color.FromArgb(180, 180, 180);
                var lightcl = Color.FromArgb(33, 33, 33);
                var dimcl = Color.FromArgb(55, 55, 55);
                var pencl = Color.FromArgb(220, 220, 220);

                BackgroundImageResourceName = "Candlechart.images.terminal_logo_main_contrast.png";
                ChartBackColor = Color.Black;
                ChartFrameStyle = FrameStyle.ThreeD;
                PaneBackColor = forecl;
                PaneFrameBorderColor = lightcl;
                PaneFrameBackColor = lightcl;
                PaneFrameBorderMarginGradientColor = dimcl;
                PaneFrameBorderCenterGradientColor = Color.FromArgb(80, 80, 80);
                PaneFrameTextColor = Color.Gray;
                PaneFrame2TextColor = Color.Gray;
                PaneFrameFont = new Font(FontFamily.GenericSansSerif, 9.5f, FontStyle.Regular);
                PaneFrameTitleBoxVisible = true;
                PaneFrameToolOutlineColor = Color.White;
                YAxisBackColor = dimcl;
                YAxisForeColor = Color.White;
                YAxisTextColor = pencl;
                YAxisFont = new Font(FontFamily.GenericSansSerif, 9.5f);
                YAxisGridBandVisible = false;
                YAxisGridBandColor = Color.FromArgb(30, fadecl);
                YAxisGridLineVisible = true;
                YAxisGridLineStyle = GridLineStyle.Dot;
                YAxisGridLineColor = dimcl;
                YAxisMajorTickVisible = true;
                YAxisMinorTickVisible = true;
                ExponentLabelBackColor = fadecl;
                ExponentLabelForeColor = Color.White;
                ExponentLabelTextColor = Color.Olive;
                ExponentLabelFont = new Font(FontFamily.GenericSansSerif, 9.5f);
                XAxisBackColor = dimcl;
                XAxisForeColor = Color.White;
                XAxisTextColor = pencl;
                XAxisFont = new Font(FontFamily.GenericSansSerif, 9.5f);
                XAxisGridBandVisible = false;
                XAxisGridBandColor = Color.FromArgb(30, fadecl);
                XAxisGridLineVisible = false;
                XAxisGridLineStyle = GridLineStyle.Dot;
                XAxisGridLineColor = forecl;
                SeriesAntiAlias = true;
                SeriesBackColor = dimcl;
                SeriesForeColor = pencl;
                SeriesLineWidth = 1f;
                SeriesLineStyle = LineStyle.Solid;
                StockSeriesUpFillColor = Color.Empty;
                StockSeriesUpLineColor = Color.White;
                StockSeriesDownFillColor = Color.LightGray;
                StockSeriesDownLineColor = Color.White;
                BarNeutralColor = Color.LightGray;
            }

            private void ApplyContrastInverseTheme()
            {
                var forecl = Color.FromArgb(0, 0, 0);
                var fadecl = Color.FromArgb(180, 180, 180);
                var lightcl = Color.FromArgb(33, 33, 33);
                var dimcl = Color.FromArgb(55, 55, 55);
                var pencl = Color.FromArgb(220, 220, 220);

                BackgroundImageResourceName = "Candlechart.images.terminal_logo_main_contrast.png";
                ChartBackColor = Color.Black;
                ChartFrameStyle = FrameStyle.ThreeD;
                PaneBackColor = forecl;
                PaneFrameBorderColor = lightcl;
                PaneFrameBackColor = lightcl;
                PaneFrameBorderMarginGradientColor = dimcl;
                PaneFrameBorderCenterGradientColor = Color.FromArgb(80, 80, 80);
                PaneFrameTextColor = Color.Gray;
                PaneFrame2TextColor = Color.Gray;
                PaneFrameFont = new Font(FontFamily.GenericSansSerif, 9.5f, FontStyle.Regular);
                PaneFrameTitleBoxVisible = true;
                PaneFrameToolOutlineColor = Color.White;
                YAxisBackColor = dimcl;
                YAxisForeColor = Color.White;
                YAxisTextColor = pencl;
                YAxisFont = new Font(FontFamily.GenericSansSerif, 9.5f);
                YAxisGridBandVisible = false;
                YAxisGridBandColor = Color.FromArgb(30, fadecl);
                YAxisGridLineVisible = false;
                YAxisGridLineStyle = GridLineStyle.Dot;
                YAxisGridLineColor = fadecl;
                YAxisMajorTickVisible = true;
                YAxisMinorTickVisible = true;
                ExponentLabelBackColor = fadecl;
                ExponentLabelForeColor = Color.White;
                ExponentLabelTextColor = Color.Olive;
                ExponentLabelFont = new Font(FontFamily.GenericSansSerif, 9.5f);
                XAxisBackColor = dimcl;
                XAxisForeColor = Color.White;
                XAxisTextColor = pencl;
                XAxisFont = new Font(FontFamily.GenericSansSerif, 9.5f);
                XAxisGridBandVisible = false;
                XAxisGridBandColor = Color.FromArgb(30, fadecl);
                XAxisGridLineVisible = false;
                XAxisGridLineStyle = GridLineStyle.Dot;
                XAxisGridLineColor = forecl;
                SeriesAntiAlias = true;
                SeriesBackColor = dimcl;
                SeriesForeColor = pencl;
                SeriesLineWidth = 1f;
                SeriesLineStyle = LineStyle.Solid;
                StockSeriesUpFillColor = Color.Red;
                StockSeriesUpLineColor = Color.Red;
                StockSeriesDownFillColor = Color.Blue;
                StockSeriesDownLineColor = Color.Blue;
                BarNeutralColor = Color.LightGray;
            }

            private void ApplyGreyTheme()
            {
                var forecl = Color.FromArgb(192, 192, 192);
                var fadecl = Color.FromArgb(166, 166, 166);
                var lightcl = Color.FromArgb(220, 220, 220);
                var dimcl = Color.FromArgb(170, 170, 170);
                var pencl = Color.FromArgb(38, 38, 38);

                BackgroundImageResourceName = "Candlechart.images.terminal_logo_main.png";
                ChartBackColor = SystemColors.Control;
                ChartFrameStyle = FrameStyle.ThreeD;
                PaneBackColor = forecl;
                PaneFrameBorderColor = fadecl;
                PaneFrameBorderMarginGradientColor = dimcl;
                PaneFrameBorderCenterGradientColor = fadecl;
                PaneFrameBackColor = lightcl;
                PaneFrameTextColor = Color.Gray;
                PaneFrame2TextColor = Color.Black;
                PaneFrameFont = new Font(FontFamily.GenericSansSerif, 9.5f, FontStyle.Regular);
                PaneFrameTitleBoxVisible = true;
                PaneFrameToolOutlineColor = Color.Black;
                YAxisBackColor = lightcl;
                YAxisForeColor = Color.Black;
                YAxisTextColor = pencl;
                YAxisFont = new Font(FontFamily.GenericSansSerif, 9.5f);
                YAxisGridBandVisible = false;
                YAxisGridBandColor = Color.FromArgb(30, fadecl);
                YAxisGridLineVisible = true;
                YAxisGridLineStyle = GridLineStyle.Dot;
                YAxisGridLineColor = fadecl;
                YAxisMajorTickVisible = true;
                YAxisMinorTickVisible = true;
                ExponentLabelBackColor = fadecl;
                ExponentLabelForeColor = Color.Black;
                ExponentLabelTextColor = Color.Brown;
                ExponentLabelFont = new Font(FontFamily.GenericSansSerif, 9.5f);
                XAxisBackColor = fadecl;
                XAxisForeColor = Color.Black;
                XAxisTextColor = pencl;
                XAxisFont = new Font(FontFamily.GenericSansSerif, 9.5f);
                XAxisGridBandVisible = false;
                XAxisGridBandColor = Color.FromArgb(30, fadecl);
                XAxisGridLineVisible = false;
                XAxisGridLineStyle = GridLineStyle.Dot;
                XAxisGridLineColor = forecl;
                SeriesAntiAlias = true;
                SeriesBackColor = dimcl;
                SeriesForeColor = pencl;
                SeriesLineWidth = 1f;
                SeriesLineStyle = LineStyle.Solid;
                StockSeriesUpFillColor = Color.Empty;
                StockSeriesUpLineColor = Color.Black;
                StockSeriesDownFillColor = Color.Gray;
                StockSeriesDownLineColor = Color.Black;
                BarNeutralColor = Color.Gray;
            }
            #endregion
        }
    }
}