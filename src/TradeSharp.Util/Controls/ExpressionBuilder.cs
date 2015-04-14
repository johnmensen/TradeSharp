using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace TradeSharp.Util.Controls
{
    public partial class ExpressionBuilder : UserControl
    {
        public delegate void ExpressionChangedDel(ExpressionBuilder sender);

        public ExpressionChangedDel ExpressionChanged;

        private int panelMargin = 5;
        public int PanelMargin
        {
            get { return panelMargin; }
            set
            {
                panelMargin = value;
                ArrangePanels();
            }
        }

        private readonly List<Control> panels = new List<Control>();
        public List<Control> Panels
        {
            get { return panels; }
        }

        public ExpressionBuilder()
        {
            InitializeComponent();
        }

        public void AddPanel(Control panel)
        {
            var container = new Panel
                {
                    BorderStyle = BorderStyle.FixedSingle,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    BackColor = SystemColors.Control,
                    Width = Width - panelMargin * 2,
                    Height = panel.Height
                };
            
            panel.Dock = DockStyle.Fill;
            container.Controls.Add(panel);

            var closeImage = imageList.Images[0];
            var button = new Button { FlatStyle = FlatStyle.Popup, Image = closeImage, Size = closeImage.Size };
            button.Click += delegate
            {
                panels.Remove(panel);
                filterPanel.Controls.Remove(container);
                ArrangePanels();

                if (ExpressionChanged != null)
                    ExpressionChanged(this);
            };

            var buttonPanel = new FlowLayoutPanel
                {
                    AutoSize = true,
                    FlowDirection = FlowDirection.TopDown,
                    Dock = DockStyle.Right
                };
            buttonPanel.Controls.Add(button);

            container.Controls.Add(buttonPanel);

            panels.Add(panel);
            filterPanel.Controls.Add(container);
            ArrangePanels();

            if (ExpressionChanged != null)
                ExpressionChanged(this);
        }

        //public void RemovePanel()

        public void ClearPanels()
        {
            panels.Clear();
            filterPanel.Controls.Clear();

            if (ExpressionChanged != null)
                ExpressionChanged(this);
        }

        private void ArrangePanels()
        {
            var top = panelMargin;
            foreach (Control panel in filterPanel.Controls)
            {
                panel.Left = panelMargin;
                panel.Top = top;
                panel.Width = Width - panelMargin * 2;
                top += panel.Height + panelMargin;
            }
        }

        private void ExpressionBuilderResize(object sender, EventArgs e)
        {
            foreach (Control panel in filterPanel.Controls)
                panel.Width = Width - panelMargin * 2;
        }
    }
}
