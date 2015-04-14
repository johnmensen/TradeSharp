using System;
using System.Collections.Generic;

namespace TradeSharp.Contract.Entity
{
    public enum AccountEventCode
    {
        AccountModified = 0,
        TradeSignal = 1,
        UpdatePending = 2,
        GapFound = 3,
        DealOpened = 4,
        DealClosed = 5,
        DealModified = 6,
        DealCanceled = 7,
        ServerMessage = 8,
        PendingOrder = 9,
        PendingOrderModified = 10,
        WalletModified = 11,
        RobotMessage = 12
    }

    public enum AccountEventAction
    {
        DefaultAction = 0,
        AlertWindow = 1,
        DoNothing = 2,
        StatusPanelOnly = 3
    }

    /// <summary>
    /// некое сообщение пр
    /// </summary>
    [Serializable]
    public class UserEvent
    {
        public int User { get; set; }

        public DateTime Time { get; set; }

        public int? AccountId { get; set; }

        public AccountEventCode Code { get; set; }

        public AccountEventAction Action { get; set; }

        private string title;
        /// <summary>
        /// [50 chars max]
        /// </summary>
        public string Title
        {
            get { return title; }
            set 
            { 
                title = value;
                if (!string.IsNullOrEmpty(title) && title.Length > 50)
                    title = title.Substring(0, 50);
            }
        }

        private string text;
        /// <summary>
        /// [256 chars max]
        /// </summary>
        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                if (!string.IsNullOrEmpty(text) && text.Length > 256)
                    text = text.Substring(0, 256);
            }
        }

        public UserEvent Copy()
        {
            return new UserEvent
                       {
                           User = User,
                           Time = Time,
                           AccountId = AccountId,
                           Code = Code,
                           Action = Action,
                           title = title,
                           text = text
                       };
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Text)
                       ? (string.IsNullOrEmpty(Title) ? "Сообщение польз. " + User : Title)
                       : Text;
        }
    }
}
