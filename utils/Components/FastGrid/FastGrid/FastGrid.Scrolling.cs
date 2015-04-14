using System.Windows.Forms;

namespace FastGrid
{
    public partial class FastGrid
    {
        #region Scrolling
        public void ScrollTo(int rowIndex)
        {
            if (!scrollBarV.Visible)
                return;
            if (rowIndex > scrollBarV.Maximum) 
                rowIndex = scrollBarV.Maximum;
            if (rowIndex < scrollBarV.Minimum)
                rowIndex = scrollBarV.Minimum;
            scrollBarV.Value = rowIndex;
        }

        public bool ScrollUpDown(int delta)
        {
            if (!scrollBarV.Visible)
                return false;
            var rowIndex = scrollBarV.Value + delta;
            if (rowIndex > (scrollBarV.Maximum - scrollBarV.LargeChange))
                rowIndex = scrollBarV.Maximum - scrollBarV.LargeChange;
            if (rowIndex < scrollBarV.Minimum)
                rowIndex = scrollBarV.Minimum;
            if (scrollBarV.Value == rowIndex)
                return false;
            scrollBarV.Value = rowIndex;
            return true;
        }
        
        private void ScrollBarVScroll(object sender, ScrollEventArgs e)
        {
            Invalidate();
        }

        private void ScrollBarScrollH(object sender, ScrollEventArgs e)
        {
            Invalidate();
        }

        private void RecalcScrollBounds(int itemsLeft)
        {
            var largeChange = rows.Count - itemsLeft;
            if (largeChange < 0) largeChange = 0;
            scrollBarV.LargeChange = largeChange;
            if (scrollBarV.LargeChange < 1) scrollBarV.LargeChange = 1;
            scrollBarV.Maximum = rows.Count;
            if (scrollBarV.Value > scrollBarV.Maximum)
                scrollBarV.Value = scrollBarV.Maximum;
        }
        #endregion        
    }
}