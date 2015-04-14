using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TradeSharp.Util;

namespace TradeSharp.Client.Controls
{
    public partial class WebMoneyPursePanelControl : UserControl
    {
        public WebMoneyPursePanelControl()
        {
            InitializeComponent();
            Localizer.LocalizeControl(this);
        }

        private void BtnAddPurseClick(object sender, EventArgs e)
        {
            Controls.Add(new WebMoneyPurseControl
                {
                    Parent = this,
                    Dock = DockStyle.Top,
                    closeClicked = ctrl => Controls.Remove(ctrl)
                });
        }

        public void SetupPurses(List<string> purseIds)
        {
            var purseCtrls = Controls.Cast<Control>().Where(c => c is WebMoneyPurseControl).ToList();
            foreach (var ctrl in purseCtrls)
                Controls.Remove(ctrl);

            if (purseIds.Count == 0) purseIds.Add("");

            foreach (var purseId in purseIds)
            {
                Controls.Add(new WebMoneyPurseControl
                {
                    Parent = this,
                    Dock = DockStyle.Top,
                    closeClicked = ctrl => Controls.Remove(ctrl),
                    PurseName = purseId
                });
            }
        }

        public List<string> GetPurseIds(out bool error)
        {
            error = false;
            var purseCtrls = Controls.Cast<Control>().Where(c => c is WebMoneyPurseControl).Cast<WebMoneyPurseControl>().Where(c => 
                !c.IsEmpty).ToList();
            if (purseCtrls.Any(c => string.IsNullOrEmpty(c.PurseName)))
            {
                error = true;
                return null;
            }
            return purseCtrls.Count == 0 ? new List<string>() : purseCtrls.Select(c => c.PurseName).ToList();
        }
    }
}
