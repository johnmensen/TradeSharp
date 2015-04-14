using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TradeSharp.Util;

namespace TradeSharp.UI.Util.Forms
{
    public partial class TooltipForm : Form
    {
        private readonly Assembly resxAssembly;
        private int tooltipIndex;
        private int tooltipsCount;
        private readonly Regex decoratorBold = new Regex(@"<b>.+?</b>", RegexOptions.IgnoreCase);
        private Font fontBold;
        private readonly Action<int> saveCurrentTooltip;

        public bool ShowOnNextStart
        {
            get { return cbShowOnNextStart.Checked; }
        }

        public int CurrentTooltip
        {
            get
            {
                return cbShowOnNextStart.Checked ? tooltipIndex : 0;
            }
        }
        
        public TooltipForm()
        {
            InitializeComponent();
        }

        public TooltipForm(int tooltipIndex, string resourceFileName, Action<int> saveCurrentTooltip)
            : this()
        {
            this.saveCurrentTooltip = saveCurrentTooltip;
            this.tooltipIndex = tooltipIndex;
            if (tooltipIndex < 1)
                this.tooltipIndex = 1;
            try
            {
                resxAssembly = Assembly.LoadFile(ExecutablePath.ExecPath + "\\" +
                    resourceFileName);
            }
            catch
            {
                return;
            }
            fontBold = new Font(Font, FontStyle.Bold);
            ShowTooltip();
        }

        private void BtnCloseClick(object sender, EventArgs e)
        {
            Close();
        }

        public void ShowTooltip()
        {
            if (resxAssembly == null)
            {
                Close();
                return;
            }
            
            // загрузить картинку из ресурса вида
            // TradeSharp.Client.Tooltip.image.tip001.png
            var imageResxName = string.Format("TradeSharp.Client.Tooltip.image.tip{0}.png",
                tooltipIndex.ToString("000"));
            try
            {
                using (var imageStream = resxAssembly.GetManifestResourceStream(imageResxName))
                {
                    pictureBox.Image = new Bitmap(imageStream);
                }
            }
            catch
            {
                tooltipIndex--;
                tooltipsCount = tooltipIndex;
            }

            // загрузить подпись
            try
            {
                var rm = new ResourceManager("TradeSharp.Client.Tooltip.tooltip", resxAssembly);
                var str = rm.GetString("tip" + tooltipIndex.ToString("000"));
                DecorateText(str);
            }
            catch
            {
            }

            SaveSettings(CurrentTooltip);
        }

        private void DecorateText(string str)
        {
            var fragments = new List<Cortege2<string, FontStyle>>();
            var lenOpen = "<b>".Length;
            var lenClose = "</b>".Length;

            while (true)
            {
                var match = decoratorBold.Match(str);
                if (!match.Success)
                {
                    if (str.Length > 0)
                        fragments.Add(new Cortege2<string, FontStyle>(str, FontStyle.Regular));
                    break;
                }

                if (match.Index > 0)
                    fragments.Add(new Cortege2<string, FontStyle>(str.Substring(0, match.Index), FontStyle.Regular));

                fragments.Add(new Cortege2<string, FontStyle>(match.Value.Substring(lenOpen, match.Length - lenOpen - lenClose), 
                    FontStyle.Bold));

                str = str.Substring(match.Index + match.Length);
            }

            tbComment.Text = "";
            foreach (var fragment in fragments)
            {
                tbComment.AppendText(fragment.a);
                var font = fragment.b == FontStyle.Bold ? fontBold : Font;

                tbComment.SelectionStart = tbComment.Text.Length - fragment.a.Length;
                tbComment.SelectionLength = fragment.a.Length;
                tbComment.SelectionFont = font;
            }
        }

        private void BtnNextClick(object sender, EventArgs e)
        {
            if (tooltipIndex == tooltipsCount) return;
            tooltipIndex++;
            ShowTooltip();
        }

        private void BtnPrevClick(object sender, EventArgs e)
        {
            if (tooltipIndex == 1) return;
            tooltipIndex--;
            ShowTooltip();
        }
    
        private void SaveSettings(int currentTip)
        {
            if (saveCurrentTooltip != null)
                saveCurrentTooltip(currentTip);
        }

        private void CbShowOnNextStartCheckedChanged(object sender, EventArgs e)
        {
            SaveSettings(CurrentTooltip);
        }
    }
}
