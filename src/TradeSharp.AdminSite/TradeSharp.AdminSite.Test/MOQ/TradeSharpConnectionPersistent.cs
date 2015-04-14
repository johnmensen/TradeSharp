using System;
using System.Data.EntityClient;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Effort.DataLoaders;
using TradeSharp.Linq;
using TradeSharp.Util;

namespace TradeSharp.AdminSite.Test.MOQ
{
    /// <summary>
    /// Внимание! Класс дублируется в Test\TradeSharp.Test\Moq
    /// </summary>
    public class TradeSharpConnectionPersistent : TradeSharpConnection
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

        /// <summary>
        /// инициализирует файловую БД
        /// </summary>
        /// <remarks>Код дублируется в аналогичном классе основного решения</remarks>
        public static TradeSharpConnectionPersistent InitializeTradeSharpConnection(out EntityConnection connection)
        {
            var path = GetCsvFilesFolder();
            var loader = new CsvDataLoader(path);
            connection = Effort.EntityConnectionFactory.CreateTransient("name=TradeSharpConnection", loader);
            var connectionPersistent = new TradeSharpConnectionPersistent(connection);
            DatabaseContext.InitializeFake(connectionPersistent);
            return connectionPersistent;
        }

        /// <summary>
        /// Получает путь к файловой БД
        /// </summary>
        /// <remarks>Код дублируется в аналогичном классе основного решения</remarks>
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

        /// <summary>
        /// Внимание! Не тестировать по параметру isDemoAccount
        /// </summary>
        public override List<GetPositionList_Result> GetPositionListWrapped(int? countItemShow, int? accountId, int? isRealAccount, string symbol, 
            int? status, int? side, DateTime? timeOpenFrom, DateTime? timeOpenTo, DateTime? timeExitFrom, DateTime? timeExitTo, out int totalItemCount)
        {
            var parameters =
            new[]
                {
                   new Tuple<string, object>("accountId", accountId),
                   new Tuple<string, object>("isRealAccount", isRealAccount),
                   new Tuple<string, object>("symbol", symbol),
                   new Tuple<string, object>("status", status),
                   new Tuple<string, object>("side", side),
                   new Tuple<string, object>("timeOpenFrom", timeOpenFrom),
                   new Tuple<string, object>("timeOpenTo", timeOpenTo),
                   new Tuple<string, object>("timeExitFrom", timeExitFrom),
                   new Tuple<string, object>("timeExitTo", timeExitTo)
                };


            var position = new List<GetPositionList_Result>();

            #region Формируем списк всех сделок
            try
            {
                foreach (var pos in POSITION)
                {
                    position.Add(new GetPositionList_Result
                        {
                            AccountID = pos.AccountID,
                            ID = pos.ID,
                            IsClosed = pos.State,
                            PriceEnter = pos.PriceEnter,
                            PriceExit = null,
                            ResultDepo = null,
                            ResultPoints = null,
                            Side = pos.Side,
                            Symbol = pos.Symbol,
                            TimeEnter = pos.TimeEnter,
                            TimeExit = null
                        });
                }

                foreach (var closePos in POSITION_CLOSED)
                {
                    position.Add(new GetPositionList_Result
                        {
                            AccountID = closePos.AccountID,
                            ID = closePos.ID,
                            IsClosed = 0,
                            PriceEnter = closePos.PriceEnter,
                            PriceExit = null,
                            ResultDepo = null,
                            ResultPoints = null,
                            Side = closePos.Side,
                            Symbol = closePos.Symbol,
                            TimeEnter = closePos.TimeEnter,
                            TimeExit = null
                        });
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("GetPositionListWrapped(). Не удалось получить список сделок из тестовой БД.", ex);
                totalItemCount = -1;
                return null;
            }

            #endregion

            #region Фильтруем сделки в соответствии с указанными фильтрами
            foreach (var param in parameters)
            {
                if (param.Item2 == null) continue;
                switch (param.Item1)
                {
                    case "accountId":
                        position = position.Where(x => x.AccountID == ((int?)param.Item2).Value).ToList();
                        break;
                    case "isRealAccount":
                        var param1 = param;
                        var accGroup = ACCOUNT_GROUP.Where(x => x.IsReal == (((int?)param1.Item2).Value == 1));
                        var accId = new List<int>();
                        foreach (var accountGroup in accGroup)
                        {
                            accId.AddRange(ACCOUNT.Where(x => x.AccountGroup == accountGroup.Code).Select(x => x.ID).ToList());
                        }
                        position = position.Where(x => accId.Contains(x.AccountID)).ToList();
                        break;
                    case "symbol":
                        position = position.Where(x => x.Symbol == (string)param.Item2).ToList();
                        break;
                    case "status":
                        position = position.Where(x => x.IsClosed ==((int?)param.Item2).Value).ToList();
                        break;
                    case "side":
                        position = position.Where(x => x.Side == ((int?)param.Item2).Value).ToList();
                        break;
                    case "timeOpenFrom":
                        position = position.Where(x => x.TimeEnter >= ((DateTime?)param.Item2).Value).ToList();
                        break;
                    case "timeOpenTo":
                        position = position.Where(x => x.TimeEnter <= ((DateTime?)param.Item2).Value).ToList();
                        break;
                    case "timeExitFrom":
                        position = position.Where(x => x.TimeExit >= ((DateTime?)param.Item2).Value).ToList();
                        break;
                    case "timeExitTo":
                        position = position.Where(x => x.TimeExit >= ((DateTime?)param.Item2).Value).ToList();
                        break;
                }
            }

            if (countItemShow.HasValue)
                position = position.Take(countItemShow.Value).ToList();
            #endregion

            totalItemCount = position.Count;
            return position;
        }


        public override IEnumerable<GetGroupsWithAccounts_Result> GetGroupsWithAccountsWrapped(string accountGroupCode)
        {
            var result = new List<GetGroupsWithAccounts_Result>
                {
                    #region 
                    new GetGroupsWithAccounts_Result
                        {
                            Accounts = 1,
                            BrokerLeverage = 500,
                            Code = "CFHABD",
                            DealerCode = "Demo",
                            DealerEnabled = true,
                            DefaultMarkupPoints = 0.0,
                            DefaultVirtualDepo = 0,
                            FileName = "DemoDealer.dll",
                            HedgingAccount = "ACC4813",
                            IsReal = true,
                            MarginCallPercentLevel = 150,
                            MarkupType = 0,
                            MessageQueue = "ts.abd.in",
                            Name = "CFH Alexey B",
                            SenderCompId = "Alexei_Denisov",
                            SessionName = "CfhLiveOrders",
                            StopoutPercentLevel = 90,
                            SwapFree = false
                        },
                    new GetGroupsWithAccounts_Result
                        {
                            Accounts = 1,
                            BrokerLeverage = 500,
                            Code = "CFHABR",
                            DealerCode = "Demo",
                            DealerEnabled = true,
                            DefaultMarkupPoints = 0.0,
                            DefaultVirtualDepo = 0,
                            FileName = "DemoDealer.dll",
                            HedgingAccount = "ACC4709",
                            IsReal = true,
                            MarginCallPercentLevel = 50,
                            MarkupType = 0,
                            MessageQueue = "ts.abr.in",
                            Name = "CFH Abrosimov",
                            SenderCompId = "Alexei_Denisov",
                            SessionName = "CfhLiveOrders",
                            StopoutPercentLevel = 90,
                            SwapFree = false
                        },
                    new GetGroupsWithAccounts_Result
                        {
                            Accounts = 1,
                            BrokerLeverage = 500,
                            Code = "CfhAPopov",
                            DealerCode = "Demo",
                            DealerEnabled = true,
                            DefaultMarkupPoints = 10.0,
                            DefaultVirtualDepo = 0,
                            FileName = "DemoDealer.dll",
                            HedgingAccount = "101",
                            IsReal = false,
                            MarginCallPercentLevel = 50,
                            MarkupType = 0,
                            MessageQueue = "Arkasha",
                            Name = "CFH A Popov",
                            SenderCompId = "Arkasha",
                            SessionName = "LMXBD",
                            StopoutPercentLevel = 90,
                            SwapFree = false
                        },
                    new GetGroupsWithAccounts_Result
                        {
                            Accounts = 3,
                            BrokerLeverage = 100,
                            Code = "CFHDemoNew",
                            DealerCode = "Demo",
                            DealerEnabled = true,
                            DefaultMarkupPoints = 0.0,
                            DefaultVirtualDepo = 0,
                            FileName = "DemoDealer.dll",
                            HedgingAccount = "",
                            IsReal = false,
                            MarginCallPercentLevel = 75,
                            MarkupType = 0,
                            MessageQueue = "",
                            Name = "CFH Demo New",
                            SenderCompId = "",
                            SessionName = "",
                            StopoutPercentLevel = 90,
                            SwapFree = false
                        }
                    #endregion
                };


            return string.IsNullOrEmpty(accountGroupCode) ? result : result.Where(x => x.Code == accountGroupCode);
        }      
    }
}
