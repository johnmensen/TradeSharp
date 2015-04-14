using System.Windows.Forms;

namespace TradeSharp.UI.Util.Control
{
    public class GrayStyledButton : Button
    {
        private bool skipPaint;

        public bool GrayStyle { get; set; }

        public GrayStyledButton()
        {
            Cursor = Cursors.Hand;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (skipPaint)
            {
                skipPaint = false;
                return;
            }

            var flickEnabled = false;
            if (GrayStyle && Enabled)
            {
                flickEnabled = true;
                Enabled = false;
                skipPaint = true;
            }
            base.OnPaint(e);
            if (flickEnabled)
                Enabled = true;
        }
    }
}
