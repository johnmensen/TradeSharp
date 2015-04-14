using System;
using System.Windows.Forms;
using TradeSharp.Client.BL;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Client
{
    public partial class MainForm
    {
        #region Статусная строка
        private const int StatusHeightClosed = 35;
        private const int StatusHeightOpened = 80;
        private const int StatusHeightSizeStep = 50;
        public void SwitchStatusPanel()
        {
            StatusPanelHeight = IsStatusPanelVisible ? StatusHeightClosed : StatusHeightOpened;
        }

        public bool IsStatusPanelVisible
        {
            get { return panelStatus.Height > StatusHeightClosed; }
        }

        public int StatusPanelHeight
        {
            get { return panelStatus.Height; }
            set
            {
                if (value < StatusHeightClosed)
                    value = StatusHeightClosed;
                if (panelStatus.Height == value) return;
                panelStatus.Height = value;
                UserSettings.Instance.StatusBarHeight = value;
                menuitemStatusBar.Checked = IsStatusPanelVisible;
                // снять подсветку?
                if (IsStatusPanelVisible) HighlightStatusPanelExpandButton(false);
            }
        }
        #endregion

        private void MenuitemStatusBarClick(object sender, EventArgs e)
        {
            SwitchStatusPanel();
        }

        private void BtnExpandStatusPanelClick(object sender, EventArgs e)
        {
            ChangeStatusPanelHeight(1);
        }

        private void BtnShrinkStatusPanelClick(object sender, EventArgs e)
        {
            ChangeStatusPanelHeight(-1);
        }

        private void ChangeStatusPanelHeight(int side)
        {
            var ht = StatusPanelHeight;
            var numSteps = (int)Math.Round((ht - StatusHeightClosed) / (float)StatusHeightSizeStep);
            numSteps += side;
            if (numSteps < 0) numSteps = 0;
            StatusPanelHeight = numSteps * StatusHeightSizeStep + StatusHeightClosed;
        }

        private void AddMessageToStatusPanelUnsafe(DateTime time, string messageText)
        {
            messageText = "[" + time.ToString("dd MMM HH:mm") + "] " + messageText;
            tbStatus.Select(0, 0);
            tbStatus.SelectedText = messageText + Environment.NewLine;
            if (!IsStatusPanelVisible) HighlightStatusPanelExpandButton(true);
        }

        /// <summary>
        /// обработать сообщение от робота - просто вывести в окошко ("лог") или показать желтое
        /// окно
        /// </summary>
        public void ProcessRobotMessage(DateTime time, string messageText)
        {
            var showWindow = UserSettings.Instance.ShowRobotMessageInYellowWindow;
            if (!showWindow)
            {
                AddMessageToStatusPanelSafe(time, messageText);
                return;
            }
            ShowMsgWindowSafe(new AccountEvent(Localizer.GetString("MessageFromRobot"), 
                messageText, AccountEventCode.RobotMessage));
        }

        public void AddMessageToStatusPanelSafe(DateTime time, string messageText)
        {
            if (IsDisposed) return;
            try
            {
                Invoke(new Action<DateTime, string>(AddMessageToStatusPanelUnsafe), time, messageText);
            }
            catch (InvalidOperationException)
            {
            }            
        }

        private void AddUrlToStatusPanelUnsafe(DateTime time, string linkText, string linkParam)
        {
            linkText = "[" + time.ToString("dd MMM HH:mm") + "] " + linkText;
            var oldLength = tbStatus.TextLength;
            tbStatus.Select(0, 0);
            tbStatus.InsertLink(linkText, linkParam);
            tbStatus.Select(tbStatus.TextLength - oldLength, 0);
            tbStatus.SelectedText = Environment.NewLine;

            if (!IsStatusPanelVisible) HighlightStatusPanelExpandButton(true);
        }

        public void AddUrlToStatusPanelSafe(DateTime time, string linkText, string linkParam)
        {
            if (IsDisposed) return;
            Invoke(new Action<DateTime, string, string>(AddUrlToStatusPanelUnsafe), time, linkText, linkParam);
        }

        /// <summary>
        /// пользователь кликнул гиперссылку - например, торговый сигнал
        /// </summary>  
        private void TbStatusLinkClicked(object sender, LinkClickedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.LinkText)) return;
            var posSharp = e.LinkText.IndexOf('#');
            if (posSharp < 0) return;
            var linkValue = e.LinkText.Substring(posSharp + 1);
            if (string.IsNullOrEmpty(linkValue)) return;

            // торговый сигнал?
            if (ProcessUserClickOnFillGapLink(linkValue)) return;
            if (ProcessUserClickOnTradeSignalStatusLink(linkValue)) return;
            
            // рестарт?
            if (ProcessUserClickOnRestartLink(linkValue)) return;
        }
    
        /// <summary>
        /// "подсветить" кнопку "развернуть" статусной строки
        /// либо снять "подсветку"
        /// 
        /// кнопка подсвечивается, если в окне есть сообщение, а окно свернуто
        /// </summary>
        private void HighlightStatusPanelExpandButton(bool isHighlighted)
        {
            const int icoIndexExpand = 70, icoIndexExpandHl = 73;
            btnExpandStatusPanel.ImageIndex = isHighlighted ? icoIndexExpandHl : icoIndexExpand;
        }
    }
}