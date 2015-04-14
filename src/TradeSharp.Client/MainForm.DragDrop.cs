using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TradeSharp.Client.Controls;

namespace TradeSharp.Client
{
    /// <summary>
    /// Реализация Drag & Drop (обработчиками Move - ResizeEnd)
    /// </summary>
    public partial class MainForm
    {
        /// <summary>
        /// пятачки с пиктограммами - появляются при перетаскивании окошек
        /// </summary>
        private DropTargetControl[] dropTargets;

        private void SetupDragTargets()
        {
            dropTargets = new []
                {
                    new DropTargetControl(DropTargetControl.DropTargetAction.Tab, DockStyle.Bottom, MoveChildToAnotherTab)
                        { Parent = this }
                };
            foreach (var target in dropTargets)
                Controls.Add(target);
        }

        /// <summary>
        /// спрятать панель (цель перемещения),
        /// инициировать процедуру
        /// </summary>
        private void OnChildResizeEnd(Form senderChild)
        {
            foreach (var dropTarget in dropTargets.Where(t => t.Visible))
            {
                var cought = dropTarget.HasCapturedMouse();
                dropTarget.Hide();
                // сделать что-то с отправителем
                if (cought)
                    dropTarget.OnDrop(senderChild);
            }
        }

        /// <summary>
        /// показать / спрятать панель - цель перемещения
        /// </summary>
        private void OnChildMove(Form senderChild)
        {
            if ((MouseButtons & MouseButtons.Left) != MouseButtons.Left)
                return;
            var rectWoPanel = new Rectangle(ClientRectangle.Left, ClientRectangle.Top,
                ClientRectangle.Width, ClientRectangle.Height - panelStatus.Height);
            var rectWithPanel = new Rectangle(rectWoPanel.Left + (panelNavi.Visible ? panelNavi.Width : 0),
                                              rectWoPanel.Top,
                                              rectWoPanel.Width - (panelNavi.Visible ? panelNavi.Width : 0),
                                              rectWoPanel.Height);

            foreach (var dropTarget in dropTargets)
            {
                var isDocked = dropTarget.CursorIsDocked(rectWithPanel);
                var isVisible = dropTarget.Visible;
                if (isDocked == isVisible) continue;
                // показать или спрятать контрол
                if (!isDocked)
                {
                    dropTarget.Hide();
                    continue;
                }
                dropTarget.PlaceAuto(rectWithPanel);
                dropTarget.Show();
            }
        }
    }
}
