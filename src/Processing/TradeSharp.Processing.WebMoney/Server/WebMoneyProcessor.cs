using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Xml.Linq;
using System.Xml.Serialization;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Processing.Lib;
using TradeSharp.Util;
using WM = WebMoney.XmlInterfaces.Responses;

namespace TradeSharp.Processing.WebMoney.Server
{
    /// <summary>
    /// Управляет циклом опроса серверов WebMoney
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class WebMoneyProcessor : PaymentProcessor
    {
        /// <summary>
        /// Флаг попытки остановить сервис в штатном режиме 
        /// </summary>
        private volatile bool isStopping;

        private WebMoneyTransferCache wmTransferCache;

        private const int ThreadSleepMilliseconds = 1000;

        /// <summary>
        /// Поток с бесконечным циклом опросы кошелька WebMoney
        /// </summary>
        private Thread threadRequestWallet;

        #region Singleton

        private static readonly Lazy<WebMoneyProcessor> instance =
            new Lazy<WebMoneyProcessor>(() => new WebMoneyProcessor());

        public static WebMoneyProcessor Instance
        {
            get { return instance.Value; }
        }

        private WebMoneyProcessor()
        {
        }

        #endregion
        
        /// <summary>
        /// Метод вызывается при старте службы и запускает в отдельном потоке бесконечный цикл опроса серверов WebMoney
        /// </summary>
        public void Start()
        {
            if (threadRequestWallet != null && threadRequestWallet.IsAlive) return;
            wmTransferCache = new WebMoneyTransferCache(ThreadSleepMilliseconds);
            

            isStopping = false;
            threadRequestWallet = new Thread(RequestWalletLoop);
            threadRequestWallet.Start();

            Logger.Info("Поток опроса WebMoney службы WebMoneyService запущен");
        }

        public void Stop()
        {
            isStopping = true;
            if (threadRequestWallet == null) return;

            threadRequestWallet.Join();
            Logger.Info("Поток опроса WebMoney службы WebMoneyService остановлен");
        }

        /// <summary>
        /// запрос к серверам WebMoney
        /// </summary>
        private void RequestWalletLoop()
        {
            var currentIteration = 0;
            while (!isStopping)
            {
                Thread.Sleep(ThreadSleepMilliseconds);

                currentIteration++;
                if (currentIteration < wmTransferCache.countIterationToRequest) continue;
                currentIteration = 0;

                var transfers = wmTransferCache.GetActualTransaction();

                if (transfers != null && transfers.Count > 0)
                    foreach (var upTransfer in transfers)
                    {
                        Logger.Info(string.Format(
                                "Поступил платёж с WebMoney, в размере {3} {5}. WmId плательщика {2}. Wm кошелёк плательщика {1}. № транзакции {0}." +
                                "Дополнительные сведения: {4}",
                                upTransfer.Id, upTransfer.SourcePurse, upTransfer.Partner, upTransfer.Amount, upTransfer.Description, WebMoneyUtil.CurrencyToStr(upTransfer.SourcePurse.Type)));

                        
                        var commentString = upTransfer.Description.ToString().Trim();
                        
                        if (!string.IsNullOrEmpty(commentString))
                        {
                            var wId = GetTradeSharpeWalletIdByLogin(commentString);
                            if (wId.HasValue &&
                                TradeSharpServer.Instance.proxy.DepositOnWallet(wId.Value, WebMoneyUtil.CurrencyToStr(upTransfer.TargetPurse.Type),
                                                                                upTransfer.Amount, DateTime.Now))
                            {
                                Logger.Info(string.Format("Кошелёк {0} пополнен через реквизиты комментария", upTransfer.SourcePurse));
                                continue;
                            }
                        }
                        
                        #region Сериализация для передачи в параметре "strRequisites"
                        string strRequisites;
                        var serializer = new XmlSerializer(upTransfer.GetType());
                        using (var writer = new StringWriter())
                        {
                            serializer.Serialize(writer, upTransfer);
                            strRequisites = writer.ToString();
                        }
                        #endregion
                        
                        // то что пришло в комментарии, не удалось применить для опознавания кошелька T#
                        // Начинаем действовать исходя из реквизитов транзакции
                        var walletId = GetTradeSharpeWalletIdByPaySysRequisite(PaymentSystem.WebMoney, upTransfer.Partner.ToString(), upTransfer.SourcePurse.ToString());
                        if (walletId != null)
                        {
                            if (TradeSharpServer.Instance.proxy.DepositOnWallet(walletId.Value, WebMoneyUtil.CurrencyToStr(upTransfer.TargetPurse.Type), upTransfer.Amount, DateTime.Now))
                            {
                                Logger.Info(string.Format("T# кошелёк {0} пополнен через поиск по БД", walletId.Value));
                            }
                            else
                            {
                                Logger.Error(string.Format("T# кошелёк плательщика опознан как {0}, но провести платёж не удалось.", walletId.Value));

                                ReportOnFailPayment(WebMoneyUtil.CurrencyToStr(upTransfer.TargetPurse.Type),upTransfer.Amount, 
                                                    DateTime.Now, PaymentSystem.WebMoney, strRequisites);
                            }
                        }
                        else
                        {
                            Logger.Info("Не удалось опознать кошелёк T# никакими способами");
                            ReportOnFailPayment(WebMoneyUtil.CurrencyToStr(upTransfer.TargetPurse.Type),upTransfer.Amount, 
                                                DateTime.Now, PaymentSystem.WebMoney, strRequisites);
                        }
                    }
            }
        }

        /// <summary>
        /// Парсинг строки с реквизитами. Обычно эта страка передаётся если проводится "неопознанный" платёж
        /// </summary>
        /// <param name="requisites">сериализованный в XML-строку объект транзакции</param>
        /// <returns>Словарь со строготипизированными ключами-реквизитами</returns>
        protected override Dictionary<RequisitesDictionaryKey, string> ParsRequisites(string requisites)
        {
            var result = new Dictionary<RequisitesDictionaryKey, string>
                {
                    {RequisitesDictionaryKey.SourcePaySysAccount, string.Empty},
                    {RequisitesDictionaryKey.SourcePaySysPurse, string.Empty},
                    {RequisitesDictionaryKey.Comment, string.Empty}
                };
            try
            {
                var doc = XElement.Parse(requisites);
                var pursesrc = doc.Elements("pursesrc").FirstOrDefault();
                if (pursesrc != null) result[RequisitesDictionaryKey.SourcePaySysPurse] = pursesrc.Value;

                var corrwm = doc.Elements("corrwm").FirstOrDefault();
                if (corrwm != null) result[RequisitesDictionaryKey.SourcePaySysAccount] = corrwm.Value;

                var desc = doc.Elements("desc").FirstOrDefault();
                if (desc != null) result[RequisitesDictionaryKey.Comment] = desc.Value;
            }
            catch (Exception ex)
            {
                Logger.Error("ReportOnFailPayment() - не удалось распарсить строку 'requisites', в которую сериализован объект типа Transfer", ex);
            }
            return result;
        }

        public override bool MakePayment(Wallet wallet, decimal amount, string targetPurse)
        {
            throw new NotImplementedException();
        }
    }
}
