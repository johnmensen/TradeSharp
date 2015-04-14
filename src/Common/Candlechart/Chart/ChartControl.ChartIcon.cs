using System;
using System.Drawing;
using System.Windows.Forms;

namespace Candlechart.Chart
{
    partial class ChartControl
    {
        private ToolTip chartIconToolTip;

        private void PaneButtonsOnMouseMove(int x, int y)
        {
            if (StockPane.customButtons == null) return;
            foreach (var btn in StockPane.customButtons)
            {
                var isIn = btn.IsIn(x, y);
                if ((!isIn && btn.State == ChartIcon.ChartIcon.ImageState.Normal)
                    || (isIn && btn.State != ChartIcon.ChartIcon.ImageState.Normal))
                    continue;
                // подсветить кнопку
                btn.State = isIn
                                ? ChartIcon.ChartIcon.ImageState.Highlighted
                                : ChartIcon.ChartIcon.ImageState.Normal;
                
                // показать подсказку
                if (btn.State == ChartIcon.ChartIcon.ImageState.Highlighted)
                    ShowTooltip(ChartIcon.ChartIcon.GetNameByKey(btn.key), 
                        btn.Position.X, btn.Position.Y + btn.Size.Height + 2);
                else
                    HideTooltip();
                
                // перерисовать кнопку
                RedrawButton(btn);
            }
        }

        private void OnMouseLeave(object sender, EventArgs eventArgs)
        {
            HideTooltip();
        }

        private void ShowTooltip(string text, int x, int y)
        {
            if (chartIconToolTip != null)
            {
                if (chartIconToolTip.Active && chartIconToolTip.ToolTipTitle == text) return;
                chartIconToolTip.Active = false;
                chartIconToolTip = null;
            }

            chartIconToolTip = new ToolTip { ToolTipTitle = text, AutoPopDelay = 1000 * 5, ShowAlways = false, AutomaticDelay = 0 };
            chartIconToolTip.Show(text, Owner, x, y);
        }

        private void HideTooltip()
        {
            if (chartIconToolTip == null) return;
            if (chartIconToolTip.Active)
                chartIconToolTip.Active = false;
            chartIconToolTip = null;
        }

        private void PaneButtonsOnLeftMouseDown(int x, int y)
        {
            if (StockPane.customButtons == null) return;
            foreach (var btn in StockPane.customButtons)
            {
                var isIn = btn.IsIn(x, y);
                if ((!isIn && btn.State != ChartIcon.ChartIcon.ImageState.Pressed)
                    || (isIn && btn.State == ChartIcon.ChartIcon.ImageState.Pressed))
                    continue;
                // подсветить кнопку (нажата)
                btn.State = isIn
                                ? ChartIcon.ChartIcon.ImageState.Pressed
                                : ChartIcon.ChartIcon.ImageState.Normal;
                // перерисовать кнопку
                RedrawButton(btn);
            }
        }

        private void PaneButtonsOnLeftMouseUp()
        {
            if (StockPane.customButtons == null) return;
            foreach (var btn in StockPane.customButtons)
            {
                if (btn.State != ChartIcon.ChartIcon.ImageState.Pressed) continue;
                btn.State = ChartIcon.ChartIcon.ImageState.Highlighted;
                // перерисовать кнопку
                RedrawButton(btn);
                // обработать клик
                btn.OnMouseClick();
            }
        }

        private void RedrawButton(ChartIcon.ChartIcon btn)
        {
            Invalidate(new Rectangle(btn.Position, btn.Size));
        }        
    }
}
