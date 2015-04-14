using System;
using System.IO;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;
using QuickFix;
using Account = QuickFix.Account;
using MarketOrder = TradeSharp.ProviderProxyContract.Entity.MarketOrder;
using System.Collections.Generic;

namespace TradeSharp.ProviderProxy.BL
{
    class FixMessageMaker
    {
        private static readonly FloodSafeLogger LoggerNoFlood = new FloodSafeLogger(1000);
        private const int MsgMagicNoSender = 1;
        private const int MsgMagicSymbolNotFound = 2;
        
        private static FixMessageMaker instance;
        public static FixMessageMaker Instance
        {
            get { return instance ?? (instance = new FixMessageMaker()); }
        }

        private bool? showMillisecond;
        public bool ShowMillisecond
        {
            get
            {
                if (showMillisecond.HasValue) return showMillisecond.Value;
                var ptrMil = SessionSettingsParser.Instance.GetParamInAllSections("ShowMillisecond", "0");
                if (ptrMil.Count > 0) showMillisecond = ptrMil[0].c == "1";
                else showMillisecond = false;
                return showMillisecond.Value;
            }
        }

        private readonly bool omitAccount = AppConfig.GetBooleanParam("FIX.OmitAccount", false);

        /// <summary>
        /// версия FIX-протокола
        /// м.б. прописана в ProviderSession sessionInfo
        /// </summary>
        //public FixVersion FixVersion { get; private set; }

        /// <summary>
        /// словарь сопоставляет каждому TargetCompID (ProviderSession) SenderCompID
        /// отправитель м.б. прописан в ProviderSession sessionInfo
        /// зачитывается из SessionSettings
        /// </summary>
        private readonly Dictionary<string, string> senderIdByTargetId =
            new Dictionary<string, string>();

        private FixMessageMaker()
        {
            //FixVersion = (FixVersion)Enum.Parse(typeof(FixVersion), 
            //    AppConfig.GetStringParam("FIX.Version", "Fix43"));
            ReadSenderIdByTargetId();
        }

        private void ReadSenderIdByTargetId()
        {
            var sessionSettingsFileName = string.Format("{0}\\{1}",
                                                        ExecutablePath.ExecPath, 
                                                        SessionSettingsParser.SessionSettingsFileName);
            if (!File.Exists(sessionSettingsFileName))
            {
                Logger.ErrorFormat("Файл [{0}] не найден", sessionSettingsFileName);
                return;
            }
            var ids = SessionSettingsParser.Instance.GetParamInAllSections("TargetCompID", string.Empty);
            foreach (var id in ids)
            {
                var key = string.IsNullOrEmpty(id.b) ? "" : id.b;
                if (!senderIdByTargetId.ContainsKey(key))
                    senderIdByTargetId.Add(key, id.a);
                Logger.DebugFormat("Target ID: [{0}], Sender ID: [{1}]",
                    key, id.a);
            }
        }

        private string GetSenderID(ProviderSession sessionInfo)
        {
            var targetID = sessionInfo.SessionName;
            return !string.IsNullOrEmpty(sessionInfo.SenderCompId)
                       ? sessionInfo.SenderCompId
                       : senderIdByTargetId.ContainsKey(targetID)
                             ? senderIdByTargetId[targetID]
                             : senderIdByTargetId.ContainsKey("")
                                   ? senderIdByTargetId[""]
                                   : string.Empty;
        }

        public Message MakeMessage(MarketOrder order, ProviderSession sessionInfo)
        {
            var senderId = GetSenderID(sessionInfo);
            if (string.IsNullOrEmpty(senderId))
            {
                LoggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Debug,
                    MsgMagicNoSender, 1000 * 60 * 15, 
                    "MakeMessage: код отправителя для [{0}] не найден", sessionInfo.SessionName);                
                return null;
            }
            var symbol = 
                TickerCodeDictionary.Instance.GetTickerNameFormatted(order.brokerOrder.Ticker);
            if (!ConvertToProviderSymbolNaming(ref symbol)) return null;
            //var isInstantOrder = order.EnabledAbsoluteSlippage.HasValue ? order.EnabledAbsoluteSlippage > 0 : false;
            var msg = FixMessage.FixVersion == FixVersion.Fix42
                            ? (Message) new QuickFix42.NewOrderSingle(
                                            new ClOrdID(order.brokerOrder.RequestId.ToString()),
                                            new HandlInst(FixMessageConstant.VALUE_HANDLE_INST_AUTO_NOINTERVENT[0]),
                                            new Symbol(symbol),
                                            new Side((order.brokerOrder.Side > 0
                                                        ? FixMessageConstant.VALUE_SIDE_BUY
                                                        : FixMessageConstant.VALUE_SIDE_SELL)[0]),
                                            new TransactTime(DateTime.Now.ToUniversalTime(), ShowMillisecond),
                                            new OrdType(FixMessageConstant.VALUE_ORDTYPE_MARKET[0]))
                            : FixMessage.FixVersion == FixVersion.Fix43
                                ? (Message) new QuickFix43.NewOrderSingle(
                                                new ClOrdID(order.brokerOrder.RequestId.ToString()),
                                                new HandlInst(FixMessageConstant.VALUE_HANDLE_INST_AUTO_NOINTERVENT[0]),
                                                new Side((order.brokerOrder.Side > 0
                                                                ? FixMessageConstant.VALUE_SIDE_BUY
                                                                : FixMessageConstant.VALUE_SIDE_SELL)[0]),
                                                new TransactTime(DateTime.Now.ToUniversalTime(), ShowMillisecond),
                                                new OrdType(FixMessageConstant.VALUE_ORDTYPE_MARKET[0]))                                                    
                                : new QuickFix44.NewOrderSingle(
                                        new ClOrdID(order.brokerOrder.RequestId.ToString()),
                                        new Side((order.brokerOrder.Side > 0
                                                    ? FixMessageConstant.VALUE_SIDE_BUY
                                                    : FixMessageConstant.VALUE_SIDE_SELL)[0]),
                                        new TransactTime(DateTime.Now.ToUniversalTime(), ShowMillisecond),
                                        new OrdType(FixMessageConstant.VALUE_ORDTYPE_MARKET[0]));

            msg.setField(new Symbol(symbol));            
            msg.setField(new OrderQty(order.brokerOrder.Volume));
            // !! пока торгуемый "продукт" вбит гвоздями
            var product = FixMessageConstant.VALUE_PRODUCT_CURRENCY.ToInt();
            msg.setField(new Product(product));
            if (!omitAccount) msg.setField(new Account(sessionInfo.HedgingAccount));
            if (order.brokerOrder.OrderPricing == OrderPricing.Instant && order.brokerOrder.RequestedPrice.HasValue)
                if (order.brokerOrder.RequestedPrice.Value > 0)
                    msg.setField(new Price((double)order.brokerOrder.RequestedPrice));

            msg.setField(new TimeInForce(FixMessageConstant.VALUE_TIME_IN_FORCE_FILL_OR_KILL[0]));
            // !! ExecInst - на TradingFloor - да бог его знает
            // msg.setField(new ExecInst(FixMessageConstant.));
            // msg.setField(new FutSettDate(DateTime.Now.ToUniversalTime().ToString("yyyyMMdd")));
            // !! onBehalfID не заполняется
            // msg.getHeader().setField(new OnBehalfOfCompID(onBehalfId));
            // сформировать заголовок
            msg.getHeader().setField(new SenderCompID(senderId));
            msg.getHeader().setField(new TargetCompID(sessionInfo.SessionName));
            // !! формируется непосредственно при отправке
            // msg.getHeader().setField(new SendingTime(DateTime.Now.ToUniversalTime(), false));            
            return msg;
        }

        private static bool ConvertToProviderSymbolNaming(ref string symbol)
        {
            if (FixApplication.ProviderNaming != TickerNamingStyle.Системный)
            {
                string providerSymbol;
                if (!DalSpot.Instance.ConvertTickerNaming(symbol, out providerSymbol,
                    TickerNamingStyle.Системный, FixApplication.ProviderNaming))
                {
                    LoggerNoFlood.LogMessageFormatCheckFlood(LogEntryType.Debug,
                        MsgMagicSymbolNotFound, 1000 * 60 * 20,
                        "Не найден системный символ \"{0}\" (стиль именования провайдера - {1})",
                        symbol, FixApplication.ProviderNaming);
                    return false;
                }
                symbol = providerSymbol;
            }
            return true;
        }
    }
}
