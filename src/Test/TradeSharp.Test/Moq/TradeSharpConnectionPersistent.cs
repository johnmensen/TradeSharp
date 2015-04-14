using System;
using System.Collections.Generic;
using System.Data.EntityClient;
using System.IO;
using System.Linq;
using System.Reflection;
using Effort.DataLoaders;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.Util;

namespace TradeSharp.Test.Moq
{
    /// <summary>
    /// Внимание! Класс дублируется в TradeSharp.AdminSite.Test\MOQ
    /// </summary>
    class TradeSharpConnectionPersistent : TradeSharpConnection
    {
        public TradeSharpConnectionPersistent(EntityConnection ctx) : base(ctx) { }

        protected override void Dispose(bool disposing)
        {
        }

        public void Cleanup()
        {
            // ReSharper disable RedundantBaseQualifier
            base.Dispose();
            // ReSharper restore RedundantBaseQualifier
        }

        public static TradeSharpConnectionPersistent InitializeTradeSharpConnection()
        {
            var path = GetCsvFilesFolder();
            var loader = new CsvDataLoader(path);
            var connection = Effort.EntityConnectionFactory.CreateTransient("name=TradeSharpConnection", loader);
            var connectionPersistent = new TradeSharpConnectionPersistent(connection);
            DatabaseContext.InitializeFake(connectionPersistent);
            return connectionPersistent;
        }

        /// <summary>
        /// временно переименовать указанные CSV в каталоге загрузки,
        /// чтобы потом вернуть им имена в TearDown
        /// 
        /// Например, если в списке позиций требуется оставить N-е количество ордеров -
        /// альтернатива поочередному из удалению средствами EF
        /// 
        /// в TearDown вызывается RestoreCsvFilesInMoqDbFolder
        /// </summary>
        public static void RenameCsvFilesInMoqDbFolder(params Type[] efTypesToExclude)
        {
            var path = GetCsvFilesFolder();
            //var renamedFileNames = new List<string>();
            foreach (var type in efTypesToExclude)
            {
                var fileName = path + "\\" + type.Name + ".csv";
                var tempFileName = fileName + ".temp";
                if (!File.Exists(fileName)) continue;
                if (File.Exists(tempFileName)) continue;
                File.Move(fileName, tempFileName);
                //renamedFileNames.Add(fileName);
            }
            //return renamedFileNames;
        }

        public static void RenameCsvFilesContainingAccountDataInMoqDbFolder()
        {
            RenameCsvFilesInMoqDbFolder(typeof(POSITION), typeof(BALANCE_CHANGE),
                typeof(TRANSFER), typeof(POSITION_CLOSED), typeof(SUBSCRIPTION), typeof(SUBSCRIPTION_SIGNAL), typeof(SERVICE),
                typeof(PENDING_ORDER), typeof(PENDING_ORDER_CLOSED), typeof(USER_TOP_PORTFOLIO), typeof(TOP_PORTFOLIO), typeof(ORDER_BILL),
                typeof(PAYMENT_SYSTEM_TRANSFER));
        }

        public static void RestoreCsvFilesInMoqDbFolder()
        {
            var path = GetCsvFilesFolder();
            foreach (var fileName in Directory.GetFiles(path, "*.temp"))
            {
                var realName = fileName.Replace(".temp", "");
                if (File.Exists(realName)) continue;
                File.Move(fileName, realName);
            }            
            //foreach (var fileName in oldFileNames)
            //{
            //    var tempName = fileName + ".temp";
            //    if (!File.Exists(tempName)) continue;
            //    File.Move(tempName, fileName);
            //}
        }

        public static string GetCsvFilesFolder()
        {
            var directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            if (directoryName != null)
            {
               var path = Directory.GetParent(Directory.GetParent(directoryName.Replace("file:\\", "")).FullName).FullName;
               return Path.Combine(path, "ModelDB\\MTS_LIVE\\"); 
            }
            
            Logger.Error("UserEventStorage.Setup() не удалось сгенерировать путь у тестовой БД");
            return string.Empty;
        }

        public override IEnumerable<GetVolumesByTiker_Result> GetVolumesByTikerWrapped()
        {
            return new List<GetVolumesByTiker_Result>
                {
                    new GetVolumesByTiker_Result
                        {
                            Buy = 10000,
                            Sell = 50000,
                            Symbol = "EURUSD"
                        },
                    new GetVolumesByTiker_Result
                        {
                            Buy = 170000,
                            Sell = 0,
                            Symbol = "GBPUSD"
                        },
                    new GetVolumesByTiker_Result
                        {
                            Buy = 10000,
                            Sell = 100000,
                            Symbol = "EURJPY"
                        }
                };
        }
    
        public override IEnumerable<GetPositionsToSwap_Result> GetPositionsToSwapWrapped()
        {
            return (from pos in POSITION
                    join acc in ACCOUNT on pos.AccountID equals acc.ID
                    join gr in ACCOUNT_GROUP on acc.AccountGroup equals gr.Code
                    where gr.SwapFree == false
                    select new                    
                        {
                            pos = pos,
                            Currency = acc.Currency
                            // трейлинг опускаю
                        }).ToList().Select(x => new GetPositionsToSwap_Result
                            {
                                AccountID = x.pos.AccountID,
                                Comment = x.pos.Comment,
                                Currency = x.Currency,
                                ExpertComment = x.pos.ExpertComment,
                                ID = x.pos.ID,
                                Magic = x.pos.Magic,
                                MasterOrder = x.pos.MasterOrder,
                                PendingOrderID = x.pos.PendingOrderID,
                                PriceBest = x.pos.PriceBest,
                                PriceEnter = x.pos.PriceEnter,
                                PriceWorst = x.pos.PriceWorst,
                                Side = x.pos.Side,
                                State = x.pos.State,
                                Stoploss = x.pos.Stoploss,
                                Symbol = x.pos.Symbol,
                                Takeprofit = x.pos.Takeprofit,
                                TimeEnter = x.pos.TimeEnter,
                                Volume = x.pos.Volume,
                            }).ToList();
        }

        public override IEnumerable<GetTransfersSummary_Result> GetTransfersSummaryWrapped(int userId)
        {
            return new List<GetTransfersSummary_Result>
                {
                    new GetTransfersSummary_Result
                        {
                            TransferType = (int)TransfersByAccountSummary.AccountTransferType.PutOnAccount,
                            TotalCount = 2,
                            TotalAmount = 200.50M
                        },
                    new GetTransfersSummary_Result
                        {
                            TransferType = (int)TransfersByAccountSummary.AccountTransferType.DirectTransfer,
                            TotalCount = 2,
                            TotalAmount = 10.00M
                        }
                };
        }
    }
}
