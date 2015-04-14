using System;
using System.Linq;
using Castle.Components.DictionaryAdapter;
using Moq;
using TradeSharp.Processing.WebMoney.Server;
using TradeSharp.Test.Processing;
using WebMoney.BasicObjects;
using WebMoney.XmlInterfaces.Responses;

namespace TradeSharp.Test.Moq
{
    internal class MoqPaymentAccessor
    {

        private const ulong MoqCompanyPurseId = 111111111111;
        private static uint lastTransferId = 959076445;

        private static readonly EditableList<Transfer> transfers = new EditableList<Transfer>
                            {
                                #region MyRegion
                                new NuWmTransfer(472.63m, DateTime.Now.AddMonths(-5),
                                                 "Зачисление N: 1102413870 / 3142-3 (Элекснет), номер чека: 8362 от 13/11/2013.",  //! платёж последних 5-х месяцев
                                                 948396383, new Purse(WmCurrency.R, MoqCompanyPurseId), 948396383,
                                                 DateTime.Now.AddMonths(-5), 0m, 0, false, 0,
                                                 (WmId) 161045341777,
                                                 0, 508.11m, new Purse(WmCurrency.R, 353227462352), 45578593,
                                                 TransferType.Normal),

                                new NuWmTransfer(499.0m, DateTime.Now.AddMonths(-5),
                                                 "Blizzard-000000546800473658760000100001-04007638260047365876",
                                                 948408358, new Purse(WmCurrency.R, 406531156062), 948408358,
                                                 DateTime.Now.AddMonths(-5), 4m, 423766849, false, 0,
                                                 (WmId) 362311291686,
                                                 0, 5.11m, new Purse(WmCurrency.R, 231891752284), 0,
                                                 TransferType.Normal),

                                new NuWmTransfer(0.9m, DateTime.Now.AddMonths(-1),
                                                 "Дополнительная комиссия за проведение SMS платежа номер 19140893 (оплата без авторизации - только через SMS на Мерчант.Вебмани)",
                                                 948408359, new Purse(WmCurrency.R, 719259572978), 948408359,
                                                 DateTime.Now.AddMonths(-1), 0m, 0, false, 0,
                                                 (WmId) 212609606184,
                                                 0, 4.21m, new Purse(WmCurrency.R, 231891752284), 0,
                                                 TransferType.Normal),

                                new NuWmTransfer(1890.5m, DateTime.Now.AddMonths(-2),
                                                 "Зачисление N: 1102427099 / 3142-3 (Элекснет), номер чека: 9192 от 04/12/2013.",  //! платёж последних 2-х месяцев
                                                 959076445, new Purse(WmCurrency.R, MoqCompanyPurseId), 959076445,
                                                 DateTime.Now.AddMonths(-2), 0m, 0, false, 0,
                                                 (WmId) 161045341777,
                                                 0, 1894.71m, new Purse(WmCurrency.R, 353227462352), 46456842,
                                                 TransferType.Normal),

                                new NuWmTransfer(1879.0m, DateTime.Now.AddMonths(-1), "TRADE#",
                                                 959077642, new Purse(WmCurrency.R, 266171490357), 959077642,
                                                 DateTime.Now.AddMonths(-1), 15.04m, 0, false, 0,
                                                 (WmId) 238661951555,
                                                 0, 0.67m, new Purse(WmCurrency.R, 231891752284), 0,
                                                 TransferType.Normal)
                                #endregion
                            };

        public static int TransferCount 
        {
            get { return transfers.Count; }
        }

        /// <summary>
        /// Для моделирования неудачного чтения параметров wmId, purseNumber и т.п.
        /// </summary>
        public static bool successInitial = true;

        /// <summary>
        /// Моделируем поступление нового платежа в последние 30 секунд
        /// </summary>
        public static void AddNewTransfer()
        {
            var newTrasferId = ++lastTransferId;
            transfers.Add(new NuWmTransfer(1890.5m, DateTime.Now.AddMilliseconds(-1),
                                            "Зачисление N: 1102427100 / 3142-3 (Элекснет), номер чека: 9193.",
                                            newTrasferId, new Purse(WmCurrency.R, MoqCompanyPurseId), newTrasferId,
                                            DateTime.Now.AddMilliseconds(-1), 0m, 0, false, 0,
                                            (WmId) 161045341777,
                                            0, 1094.71m, new Purse(WmCurrency.R, 353227462352), 46456842,
                                            TransferType.Normal)); 
        }

        public static Mock<IPaymentAccessor> MakeMoq()
        {
            WebMoneyUtil.companyPurseId = MoqCompanyPurseId;

            var moq = new Mock<IPaymentAccessor>();

            // для моделирования исключения нужно передать startData большую 2900 года
            // (в случае исключения метод GetTransfers возвращает null)
            moq.Setup(s => s.GetTransfers(It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(
                (DateTime startData, DateTime finishData) =>
                    {
                        if (startData > new DateTime(2900, 1, 1))
                            return null;

                        return transfers.Where(x => startData.CompareTo(x.CreateTime) <= 0 && finishData.CompareTo(x.CreateTime) >= 0).ToList();
                    }
                );

            moq.Setup(s => s.CheckInitial()).Returns(() => successInitial);
            return moq;        
        }
    }
}
