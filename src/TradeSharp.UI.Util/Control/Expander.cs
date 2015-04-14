using System;
using System.Collections.Generic;
using ns = System.Windows.Forms;

namespace TradeSharp.UI.Util.Control
{
    public partial class Expander : ns.UserControl
    {
        public string Title
        {
            set { txtHader.Text = value; }
        }

        private int originalHeight;

        private readonly int margins;

        public Expander()
        {
            InitializeComponent();
            margins = pnlHader.Margin.Bottom + pnlHader.Margin.Top + pnlContainer.Margin.Bottom + pnlContainer.Margin.Top + 3;
        }

        public void SetContent(ns.Control content)
        {
            content.Dock = ns.DockStyle.Fill;
            pnlContainer.Controls.Clear();
            pnlContainer.Controls.Add(content);

            originalHeight = Height;
        }

        public void SetContent(List<ns.Control> content)
        {
            pnlContainer.Controls.Clear();
            for (var i = 0; i < content.Count; i++)
            {
                content[i].Dock = ns.DockStyle.Top;
                pnlContainer.Controls.Add(content[i]);
            }

            originalHeight = Height;
        }

        public void Expanding(bool isExpand)
        {
            pnlContainer.Visible = isExpand;
            if (isExpand)
            {
                btnCollaps.Text = ">>";
                Height = originalHeight;
            }
            else
            {
                btnCollaps.Text = "<<";
                Height = pnlHader.Height + pnlHader.Margin.Bottom + pnlHader.Margin.Top;
            }
        }


        private void BtnCollapsClick(object sender, EventArgs e)
        {
            Expanding(!pnlContainer.Visible);
        }


        private void ExpanderResize(object sender, EventArgs e)
        {
            pnlHader.Width = Width - pnlHader.Margin.Left - pnlHader.Margin.Right;
            btnCollaps.Left = pnlHader.Width - btnCollaps.Width - btnCollaps.Margin.Right;
            pnlContainer.Width = Width - pnlContainer.Margin.Left - pnlContainer.Margin.Right;

            if (pnlContainer.Visible)
                pnlContainer.Height = Height > pnlHader.Height - margins ? Height - pnlHader.Height - margins : Height - pnlHader.Height;
        }
    }
}
