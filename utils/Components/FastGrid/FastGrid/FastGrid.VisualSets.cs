using System.Drawing;

namespace FastGrid
{
    public partial class FastGrid
    {
        #region Grid visual settings
        /// <summary>
        /// column's header font
        /// </summary>
        public Font FontHeader { get; set; }
        
        /// <summary>
        /// cell's font
        /// </summary>
        public Font FontCell { get; set; }
        
        /// <summary>
        /// selected row font
        /// </summary>
        public Font FontSelectedCell { get; set; }
        
        /// <summary>
        /// anchored (stick to top or bottom) font
        /// </summary>
        public Font FontAnchoredRow { get; set; }

        private int cellPadding = 5;
        /// <summary>
        /// cell's padding
        /// </summary>
        public int CellPadding
        {
            get { return cellPadding; }
            set { cellPadding = value; }
        }

        private int captionHeight = 20;
        /// <summary>
        /// column's caption height
        /// </summary>
        public int CaptionHeight
        {
            get { return captionHeight; }
            set { captionHeight = value; }
        }

        private int cellHeight = 18;
        /// <summary>
        /// cell's height
        /// </summary>
        public int CellHeight
        {
            get { return cellHeight; }
            set { cellHeight = value; }
        }

        private Color colorCellOutlineUpper = Color.DarkGray;
        /// <summary>
        /// cell upper outline color
        /// </summary>
        public Color ColorCellOutlineUpper
        {
            get { return colorCellOutlineUpper; }
            set { colorCellOutlineUpper = value; }
        }

        private Color colorCellOutlineLower = Color.White;
        /// <summary>
        /// cell lower outline color
        /// </summary>
        public Color ColorCellOutlineLower
        {
            get { return colorCellOutlineLower; }
            set { colorCellOutlineLower = value; }
        }

        private Color colorCellFont = Color.Black;
        /// <summary>
        /// cell font color
        /// </summary>
        public Color ColorCellFont
        {
            get { return colorCellFont; }
            set { colorCellFont = value; }
        }

        private Color colorCellBackground = Color.FromArgb(250, 250, 250);
        /// <summary>
        /// cell background color
        /// </summary>
        public Color ColorCellBackground
        {
            get { return colorCellBackground; }
            set { colorCellBackground = value; }
        }

        private Color colorAltCellBackground = Color.FromArgb(235, 235, 235);
        /// <summary>
        /// alternating row cell backgr color
        /// </summary>
        public Color ColorAltCellBackground
        {
            get { return colorAltCellBackground; }
            set { colorAltCellBackground = value; }
        }

        private Color colorSelectedCellFont = Color.Black;
        /// <summary>
        /// selected row font color
        /// </summary>
        public Color ColorSelectedCellFont
        {
            get { return colorSelectedCellFont; }
            set { colorSelectedCellFont = value; }
        }

        private Color colorSelectedCellBackground = Color.FromArgb(219, 242, 228);
        /// <summary>
        /// selected row cell backgr color
        /// </summary>
        public Color ColorSelectedCellBackground
        {
            get { return colorSelectedCellBackground; }
            set { colorSelectedCellBackground = value; }
        }

        private Color colorAnchorCellBackground = Color.FromArgb(250, 250, 250);
        /// <summary>
        /// anchored cell backgr color
        /// </summary>
        public Color ColorAnchorCellBackground
        {
            get { return colorAnchorCellBackground; }
            set { colorAnchorCellBackground = value; }
        }

        private bool fitWidth;
        /// <summary>
        /// try to occupy by sizable columns all free space inside table
        /// when it is on, column resizing is restricted
        /// </summary>
        public bool FitWidth
        {
            get { return fitWidth; }
            set { fitWidth = value; CheckSize(); }
        }

        #endregion 
    }
}