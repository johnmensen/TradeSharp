using System;
using System.Linq;
using System.Windows.Forms;
using TradeSharp.Contract.Entity;
using TradeSharp.Robot.Robot;
using TradeSharp.Util;

namespace TradeSharp.Robot.BL
{
    public partial class Mt4RobotEditorForm : Form
    {
        public Func<Account[]> GetUserOwnedAccounts;

        private Mt4Robot.RobotMode Mode
        {
            get
            {
                return modeButton.ImageIndex == 0 ? Mt4Robot.RobotMode.КопироватьОрдераМТ4 : Mt4Robot.RobotMode.УправлятьМТ4;
            }
        }

        private Mt4Robot robot;
        public Mt4Robot Robot
        {
            get { return robot; }
            set
            {
                if (value == null)
                    return;
                robot = value;
                Reset();
            }
        }

        public Mt4RobotEditorForm()
        {
            InitializeComponent();
        }

        private void ModeButtonClick(object sender, EventArgs e)
        {
            if (Mode == Mt4Robot.RobotMode.УправлятьМТ4)
            {
                modeButton.TextImageRelation = TextImageRelation.ImageBeforeText;
                modeButton.ImageIndex = 0;
            }
            else
            {
                modeButton.TextImageRelation = TextImageRelation.TextBeforeImage;
                modeButton.ImageIndex = 1;
            }
        }

        private void Reset()
        {
            tickerTextBox.Text = BaseRobot.GetStringFromTickerTimeframe(robot.Graphics);
            if (robot.Mode == Mt4Robot.RobotMode.УправлятьМТ4)
            {
                modeButton.TextImageRelation = TextImageRelation.TextBeforeImage;
                modeButton.ImageIndex = 1;
            }
            else
            {
                modeButton.TextImageRelation = TextImageRelation.ImageBeforeText;
                modeButton.ImageIndex = 0;
            }
            tradeSharpAccountComboBox.Items.Clear();
            if (GetUserOwnedAccounts != null)
                tradeSharpAccountComboBox.Items.AddRange(GetUserOwnedAccounts().Select(a => a as object).ToArray());
            tradeSharpAccountComboBox.Text = robot.TradeSharpAccount.ToString();
            ignoreMagicCheckBox.Checked = robot.ControlOrdersDisregardMagic;
            mt4ToTradeSharpRateTextBox.Text = robot.Mt4ToTradeSharpRate.ToString();
            mt4TickerSuffixTextBox.Text = robot.Mt4TickerSuffix;
            ownPortNumericUpDown.Value = robot.portOwn;
            mt4AddressTextBox.Text = robot.HostMt4;
            mt4PortNumericUpDown.Value = robot.PortMt4;
            percentScaleNumericUpDown.Value = robot.PercentScale;
            fixedVolumeTextBox.Text = robot.FixedVolume.ToString();
        }

        private void OkButtonClick(object sender, EventArgs e)
        {
            robot.Graphics = BaseRobot.ParseTickerTimeframeString(tickerTextBox.Text);
            robot.Mode = Mode;
            var account = tradeSharpAccountComboBox.SelectedItem as Account;
            var tradeSharpAccount = account != null ? account.ID : tradeSharpAccountComboBox.Text.ToIntSafe();
            robot.TradeSharpAccount = tradeSharpAccount.HasValue ? tradeSharpAccount.Value : 0;
            robot.ControlOrdersDisregardMagic = ignoreMagicCheckBox.Checked;
            var mt4ToTradeSharpRate = mt4ToTradeSharpRateTextBox.Text.ToIntSafe();
            robot.Mt4ToTradeSharpRate = mt4ToTradeSharpRate.HasValue ? mt4ToTradeSharpRate.Value : 0;
            robot.Mt4TickerSuffix = mt4TickerSuffixTextBox.Text;
            robot.portOwn = (int) ownPortNumericUpDown.Value;
            robot.HostMt4 = mt4AddressTextBox.Text;
            robot.PortMt4 = (int) mt4PortNumericUpDown.Value;
            robot.PercentScale = (int) percentScaleNumericUpDown.Value;
            var fixeVolume = fixedVolumeTextBox.Text.ToIntSafe();
            robot.FixedVolume = fixeVolume.HasValue ? fixeVolume.Value : 0;
        }

        private void ShowHelp(object sender, EventArgs e)
        {
            var helpButton = sender as Label;
            if (helpButton == null)
                return;
            HelpManager.Instance.ShowHelp(this, (string) helpButton.Tag);
        }

        private void TickerButtonClick(object sender, EventArgs e)
        {
            var dlg = new RobotTimeframesForm(BaseRobot.ParseTickerTimeframeString(tickerTextBox.Text));
            if (dlg.ShowDialog() != DialogResult.OK)
                return;
            tickerTextBox.Text = BaseRobot.GetStringFromTickerTimeframe(dlg.UpdatedGraphics);
        }
    }
}
