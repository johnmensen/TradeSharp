using System;
using System.Collections.Generic;
using WebMoney.BasicObjects;
using WebMoney.Cryptography;
using WebMoney.XmlInterfaces;
using TradeSharp.Util;
using WebMoney.XmlInterfaces.Responses;

namespace TradeSharp.Processing.WebMoney.Server
{
    public class WebMoneyAccessor : IPaymentAccessor
    {
        /// <summary>
        /// Разница во времени с серверами WebMoney, выраженная в часах
        /// </summary>
        private readonly int wmServerTimeDifference = 0;
        
        /// <summary>
        /// XML-ключ, полученный из файла .kwm с помощью программы Key Extractor
        /// </summary>
        private readonly string keeperKeyValue;

        /// <summary>
        /// Номер целевого кошелька
        /// </summary>
        private readonly ulong targetPurseNumber;        
        
        /// <summary>
        /// WM кошелёк
        /// </summary>
        private readonly Purse purse;

        /// <summary>
        /// Уникальный идентификатор пользователя WM
        /// </summary>
        private readonly WmId wmId;

        private readonly Initializer initializer;

        /// <summary>
        /// Конструктор, использующийся для инициализации WebMoneyAccessor ключом Keeper Classic
        /// </summary>
        public WebMoneyAccessor()
        {
            var wmAccountDictionary = WebMoneyUtil.Instance.GetWmAccountSettings();
            if (wmAccountDictionary == null)
            {
                Logger.Error("WebMoneyAccessor() - не удалось прочитать необходимые для инициализировать WebMoney параметры (wmId, purseNumber и др.)");
                return;
            }
            
            keeperKeyValue = wmAccountDictionary["WmKeeperKeyValue"].ToString();
            targetPurseNumber = (ulong)wmAccountDictionary["WmTargetPurseNumber"];
            wmId = (WmId)(ulong)wmAccountDictionary["WmId"];
            var targetPurseType = WebMoneyUtil.StrToCurrency(wmAccountDictionary["WmPurseCurrency"].ToString());
            
            purse = new Purse
                {
                    Number = targetPurseNumber,
                    Type = targetPurseType
                };

            var keeperKey = new KeeperKey(keeperKeyValue);
            initializer = new Initializer(wmId, keeperKey)
                {
                    StartDate = new DateTime(1983, 1, 1).ToUniversalTime()
                };          
            initializer.Apply();

            wmServerTimeDifference = (WmDateTime.ServerTime2UtcTime(DateTime.Now).Hour - DateTime.Now.ToUniversalTime().Hour);
        }

        /// <summary>
        /// Проверяем пробным запросом к WebMoney, корректно ли введены учётные данные
        /// </summary>
        /// <returns></returns>
        public bool CheckInitial()
        {
            if (initializer == null) return false;
            try
            {
                const string message = "Проверка учётных данных";
                var signature = initializer.Sign(message);
                var signatureInspector = new SignatureInspector(wmId, (Message) message, (Description) signature);
                var signatureEvidence = signatureInspector.Submit();
                if (!signatureEvidence.VerificationResult)
                {
                    Logger.Error(
                        "WebMoneyAccessor - не удалось инициализировать WebMoney, указанными параметрами (wmId, purseNumber и др.)");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(
                    "WebMoneyAccessor - не удалось инициализировать WebMoney, указанными параметрами (wmId, purseNumber и др.)",ex);
                return false;
            }
        }

        /// <summary>
        /// Получить список транзакций кошелька, за указанный период времени. Период времени должен быть не больше трёх месяцев.
        /// </summary>
        public List<Transfer> GetTransfers(DateTime startTime, DateTime finishTime)
        {
            var transferFilter = new TransferFilter(purse, startTime.AddHours(wmServerTimeDifference), finishTime.AddHours(wmServerTimeDifference));
            try
            {
                
                var transferRegister = transferFilter.Submit();
                return transferRegister.TransferList;
            }
            catch (Exception ex)
            {
                Logger.Error(
                    string.Format("GetTransfers() - Не удалось получить список транзакций с сервера WebMoney кошелька {0} за период времени {1} от до {2}",
                    purse.ToString(), startTime, finishTime), ex);
                return null;
            }           
        }
    }
}
