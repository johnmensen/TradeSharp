using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace FastGrid
{
    /// <summary>
    /// custom method delegate to format cell value as a sctring.
    /// might be provided for a column
    /// </summary>    
    public delegate string FormatCellValueDel(object cellValue);
    
    /// <summary>
    /// formats value for a cell taking the whole row's object value as an argument
    /// </summary>    
    public delegate string FormatObjectFieldDel(object rowValueObject);
    
    /// <summary>
    /// determines backgr and foregr (font) colors for a cell depending on the cell's value
    /// </summary>    
    public delegate void ColorColumnByValueDel(object cellValue, out Color? backColor, out Color? fontColor);
    
    public class CellFormattingEventArgs
    {
        public FastColumn column;

        public object cellValue;

        public object rowValue;

        public string resultedString;
    }

    /// <summary>
    /// describes grid's column
    /// </summary>
    [Serializable]
    public class FastColumn
    {
        public string PropertyName { get; set; }
        public string Title { get; set; }
        public string ToolTip { get; set; }
        public bool IsHyperlinkStyleColumn { get; set; }
        public bool ShowClippedContent { get; set; }
        
        /// <summary>
        /// filled from binding's code from a DisplayName attribute (if it exists)
        /// ingored if Title prop is specified (not empty)
        /// </summary>
        public string PropertyDisplayName { get; set; }

        /// <summary>
        /// Then ImageList is bound to column
        /// images are pick up from list by name (prop must be string)
        /// or index (prop must be int)
        /// </summary>
        public ImageList ImageList { get; set; }
        
        private int columnWidth = -1;
        /// <summary>
        /// fixed column width (should be > 0, overwise ignored)
        /// </summary>
        public int ColumnWidth
        {
            get { return columnWidth; }
            set { columnWidth = value; }
        }

        private int columnMinWidth = -1;
        /// <summary>
        /// minimum column width (should be > 0, overwise ignored)
        /// if = 0 then will be assigned by text width
        /// </summary>
        public int ColumnMinWidth
        {
            get { return columnMinWidth; }
            set { columnMinWidth = value; }
        }

        /// <summary>
        /// column relative width [0..1] (if = -1, then it will be assigned on first use)
        /// </summary>
        private double relativeWidth = -1;
        public double RelativeWidth
        {
            get { return relativeWidth; }
            set { relativeWidth = value; }
        }

        /// <summary>
        /// auto calculated column width
        /// </summary>
        public int ResultedWidth { get; set; }

        /// <summary>
        /// sort mode
        /// </summary>
        public FastColumnSort SortOrder { get; set; }
        
        /// <summary>
        /// column's caption alignment
        /// </summary>
        private readonly StringFormat headerTextFormat = new StringFormat 
            { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

        /// <summary>
        /// visibility
        /// </summary>
        private bool visible = true;
        public bool Visible
        {
            get { return visible; }
            set { visible = value; if (!value) relativeWidth = -1; }
        }

        public bool IsEditable { get; set; }

        #region Formatting
        
        private int fractionDigits = 2;
        /// <summary>
        /// default digits count for fractions
        /// </summary>
        public int FractionDigits
        {
            get { return fractionDigits; }
            set { fractionDigits = value; }
        }

        /// <summary>
        /// user specified custom format string
        /// </summary>
        public string FormatString { get; set; }

        public Action<CellFormattingEventArgs> cellFormatting;

        public event Action<CellFormattingEventArgs> CellFormatting
        {
            add { cellFormatting += value; }
            remove { cellFormatting -= value; }
        }

        /// <summary>
        /// sub for null values
        /// </summary>
        public string NullString { get; set; }

        /// <summary>
        /// custom method delegate to format cell value as a string.
        /// might be provided for a column
        /// </summary>
        public FormatCellValueDel formatter;
        
        /// <summary>
        /// formats value for a cell taking the whole row's object value as an argument
        /// </summary>
        public FormatObjectFieldDel rowFormatter;
        
        /// <summary>
        /// determines backgr and foregr (font) colors for a cell depending on the cell's value
        /// </summary>
        public ColorColumnByValueDel colorColumnFormatter;

        public object Tag { get; set; }
        
        #endregion

        #region Visual props
        private Color colorSorter = Color.DarkGray;
        /// <summary>
        /// sort label (small arrow) color
        /// </summary>
        public Color ColorSorter
        {
            get { return colorSorter; }
            set { colorSorter = value; }
        }

        private Color colorText = Color.Black;
        /// <summary>
        /// text color
        /// </summary>
        public Color ColorText
        {
            get { return colorText; }
            set { colorText = value; }
        }

        private int padding = 5;
        /// <summary>
        /// cell padding
        /// </summary>
        public int Padding
        {
            get { return padding; }
            set { padding = value;}
        }            

        private int sortOrderSignWidth = 9;
        /// <summary>
        /// sort label (small arrow) size
        /// </summary>
        public int SortOrderSignWidth
        {
            get { return sortOrderSignWidth; }
            set { sortOrderSignWidth = value; }
        }
        
        private Color colorOutlineUpper = Color.DarkGray;
        /// <summary>
        /// cell's outline color (upper line)
        /// </summary>
        public Color ColorOutlineUpper
        {
            get { return colorOutlineUpper; }
            set { colorOutlineUpper = value; }
        }

        private Color colorOutlineLower = Color.DarkGray;
        /// <summary>
        /// cell's outline color (lower line)
        /// </summary>
        public Color ColorOutlineLower
        {
            get { return colorOutlineLower; }
            set { colorOutlineLower = value; }
        }

        private Color colorFillUpper = Color.FromArgb(240, 240, 240);
        /// <summary>
        /// start gradient fill color (upper point)
        /// </summary>
        public Color ColorFillUpper
        {
            get { return colorFillUpper; }
            set { colorFillUpper = value;}
        }

        private Color colorFillLower = Color.LightGray;
        /// <summary>
        /// end gradient fill color (lower point)
        /// </summary>
        public Color ColorFillLower
        {
            get { return colorFillLower; }
            set { colorFillLower = value; }
        }

        /// <summary>
        /// hyperlink-style column inactive text color (no cursor over cell)
        /// </summary>
        public Color? ColorHyperlinkTextInactive { get; set; }

        private Color? colorHyperlinkTextActive = Color.DarkBlue;
        /// <summary>
        /// hyperlink-style column active text color (cursor is over cell)
        /// </summary>
        public Color? ColorHyperlinkTextActive
        {
            get { return colorHyperlinkTextActive; }
            set { colorHyperlinkTextActive = value; }
        }

        private StringAlignment cellHAlignment = StringAlignment.Near;
        /// <summary>
        /// cell's horizontal text alignment
        /// </summary>
        public StringAlignment CellHAlignment
        {
            set { cellHAlignment = value; }
            get { return cellHAlignment; }
        }

        /// <summary>
        /// column's font (migh be null)
        /// </summary>
        public Font ColumnFont { get; set; }

        /// <summary>
        /// column's font for an active hyperlink-style cell (mouse curs is over)
        /// </summary>
        public Font HyperlinkFontActive { get; set; }

        /// <summary>
        /// column's font for an inactive hyperlink-style cell (no mouse curs)
        /// </summary>
        public Font HyperlinkFontInactive { get; set; }

        /// <summary>
        /// mouse pointer for an active hyperlink-style cell (mouse curs is over)
        /// </summary>
        public Cursor HyperlinkActiveCursor { get; set; }

        #endregion

        public FastColumn(string propName)
        {
            PropertyName = propName;
            Title = propName;
        }

        public FastColumn(string propName, string title)
        {
            PropertyName = propName;
            Title = title;
        }
        
        /// <summary>
        /// draw column's header
        /// </summary>
        public void Draw(Graphics g, Font font, int height, int width, Point leftTop,
            Pen penUp, Pen penDn, BrushesStorage brushes)
        {
            // choose fill brush
            var brush = colorFillLower == colorFillUpper
                            ? brushes.GetBrush(colorFillUpper)
                            : (Brush)new LinearGradientBrush(new Point(0, leftTop.Y),
                                                      new Point(0, leftTop.Y + height - 1),
                                                      brushes.GetBrush(colorFillUpper).Color, brushes.GetBrush(colorFillLower).Color);
            var rect = new Rectangle(leftTop.X, leftTop.Y, width, height);
            g.FillRectangle(brush, leftTop.X, leftTop.Y, width, height);

            // outlines
            g.DrawLine(penUp, leftTop.X, leftTop.Y, leftTop.X + width, leftTop.Y);
            g.DrawLine(penUp, leftTop.X, leftTop.Y, leftTop.X, leftTop.Y + height);
            
            g.DrawLine(penDn, leftTop.X + width, leftTop.Y, leftTop.X + width, leftTop.Y + height);
            g.DrawLine(penDn, leftTop.X + width, leftTop.Y + height, leftTop.X, leftTop.Y + height);
            
            // text and sorting mode label
            var orderSignW = SortOrder == FastColumnSort.None ? 0 : SortOrderSignWidth;
            var maxTextW = width - Padding * 2 - orderSignW;
            var textCenterX = maxTextW/2 + Padding;
            var columnText = !string.IsNullOrEmpty(Title)
                                 ? Title
                                 : !string.IsNullOrEmpty(PropertyDisplayName) ? PropertyDisplayName : PropertyName;
            var brushText = brushes.GetBrush(ColorText);

            g.SetClip(rect);
            g.DrawString(columnText, font, brushText,
                    leftTop.X + textCenterX, leftTop.Y + height / 2, headerTextFormat);            
            g.ResetClip();

            // sorting label
            if (SortOrder != FastColumnSort.None)
                DrawSortLabel(g, height, width, leftTop, brushes);
        }

        private void DrawSortLabel(Graphics g, int height, int width, Point leftTop, BrushesStorage brushes)
        {
            var centerX = leftTop.X + width - Padding - sortOrderSignWidth/2;
            var centerY = leftTop.Y + height/2;

            var l = sortOrderSignWidth - 4;
            var l2 = l*0.5F;

            const float sq3 = 0.57735F;
            var r = sq3*l;
            var h = r / 2;

            var points = SortOrder == FastColumnSort.Ascending
                             ? new []
                                 {
                                     new PointF(centerX, centerY - r), 
                                     new PointF(centerX - l2, centerY + h), 
                                     new PointF(centerX + l2, centerY + h), 
                                 }
                             : new []
                                 {
                                     new PointF(centerX, centerY + r), 
                                     new PointF(centerX - l2, centerY - h), 
                                     new PointF(centerX + l2, centerY - h), 
                                 };
            var brush = brushes.GetBrush(ColorSorter);
            g.FillPolygon(brush, points);
        }
    }

    /// <summary>
    /// column's value sorting order mode
    /// </summary>
    public enum FastColumnSort
    {
        None = 0,
        Ascending,
        Descending
    }
}
