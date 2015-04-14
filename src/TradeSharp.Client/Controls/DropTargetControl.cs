using System;
using System.Drawing;
using System.Windows.Forms;

namespace TradeSharp.Client.Controls
{
    public partial class DropTargetControl : UserControl
    {
        public enum DropTargetAction
        {
            Tab = 0
        };

        public DropTargetAction DropTarget { get; private set; }

        public Action<Form> OnDrop { get; set; }

        private DockStyle cursorTarget;

        public DropTargetControl()
        {
            InitializeComponent();
        }

        public DropTargetControl(DropTargetAction dropTarget, DockStyle cursorTarget, Action<Form> onDrop)
            : this()
        {
            this.cursorTarget = cursorTarget;
            DropTarget = dropTarget;
            OnDrop = onDrop;
            pictureBox.Image = imageList.Images[(int) DropTarget];
            Visible = false;
        }

        public bool HasCapturedMouse()
        {
            return ClientRectangle.Contains(PointToClient(Cursor.Position));
        }

        public bool CursorIsDocked(Rectangle clientRect)
        {
            const int margin = 50;

            var relPos = Parent.PointToClient(Cursor.Position);
            var isIn = cursorTarget == DockStyle.Left
                           ? relPos.X < margin
                           : cursorTarget == DockStyle.Right
                                 ? relPos.X > clientRect.Width - margin
                                 : cursorTarget == DockStyle.Top
                                       ? relPos.Y < margin
                                       : relPos.Y > clientRect.Height - margin;

            return isIn;
        }

        public void PlaceAuto(Rectangle clientRect)
        {
            if (cursorTarget == DockStyle.Left)
            {
                Location = new Point(clientRect.Left + 2, clientRect.Top + clientRect.Height / 2 - Height / 2);
                return;
            }
            if (cursorTarget == DockStyle.Right)
            {
                Location = new Point(clientRect.Left + clientRect.Width - Width - 2, clientRect.Top + clientRect.Height / 2 - Height / 2);
                return;
            }
            if (cursorTarget == DockStyle.Bottom)
            {
                Location = new Point(clientRect.Left + clientRect.Width / 2 - Width / 2, clientRect.Top + clientRect.Height - Height - 2);
                return;
            }
            if (cursorTarget == DockStyle.Top)
            {
                Location = new Point(clientRect.Left + clientRect.Width / 2 - Width / 2, clientRect.Top + 2);
                //return;
            }
        }
    }
}
