using System;
using System.Collections.Generic;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Linq
{
    public static class LinqToEntity
    {
        public static TradeTicker DecorateTicker(SPOT spot)
        {
            return new TradeTicker
                       {
                           ActiveBase = spot.ComBase,
                           ActiveCounter = spot.ComCounter,
                           CodeFXI = spot.CodeFXI,
                           Description = spot.Description,
                           Precision = spot.Precise,
                           SwapBuy = spot.SwapBuy,
                           SwapSell = spot.SwapSell,
                           Title = spot.Title
                       };
        }

        public static MarketOrder DecorateOrder(POSITION pos)
        {
            var mo = new MarketOrder
            {
                AccountID = pos.AccountID,
                Comment = pos.Comment,
                ExpertComment = pos.ExpertComment,
                ID = pos.ID,
                Magic = pos.Magic,
                PendingOrderID = pos.PendingOrderID,
                PriceBest = (float?)pos.PriceBest,
                PriceEnter = (float)pos.PriceEnter,
                PriceWorst = (float?)pos.PriceWorst,
                Side = pos.Side,
                State = ((PositionState)pos.State),
                StopLoss = (float?)(pos.Stoploss == 0 ? null : pos.Stoploss),
                TakeProfit = (float?)(pos.Takeprofit == 0 ? null : pos.Takeprofit),
                Symbol = pos.Symbol,
                TimeEnter = pos.TimeEnter,
                Volume = pos.Volume,
                TrailLevel1 = (float?)pos.TrailLevel1,
                TrailLevel2 = (float?)pos.TrailLevel2,
                TrailLevel3 = (float?)pos.TrailLevel3,
                TrailLevel4 = (float?)pos.TrailLevel4,
                TrailTarget1 = (float?)pos.TrailTarget1,
                TrailTarget2 = (float?)pos.TrailTarget2,
                TrailTarget3 = (float?)pos.TrailTarget3,
                TrailTarget4 = (float?)pos.TrailTarget4,
                MasterOrder = pos.MasterOrder
            };
            return mo;
        }

        public static MarketOrder DecorateOrder(POSITION_CLOSED pos)
        {
            var mo = new MarketOrder
            {
                AccountID = pos.AccountID,
                Comment = pos.Comment,
                ExpertComment = pos.ExpertComment,
                ID = pos.ID,
                Magic = pos.Magic,
                PendingOrderID = pos.PendingOrderID,
                PriceBest = (float?)pos.PriceBest,
                PriceEnter = (float)pos.PriceEnter,
                PriceWorst = (float?)pos.PriceWorst,
                PriceExit = (float?)pos.PriceExit,
                TimeExit = pos.TimeExit,
                Side = pos.Side,
                State = PositionState.Closed,
                StopLoss = (float?)(pos.Stoploss == 0 ? null : pos.Stoploss),
                TakeProfit = (float?)(pos.Takeprofit == 0 ? null : pos.Takeprofit),
                Swap = (float?)pos.Swap,
                Symbol = pos.Symbol,
                TimeEnter = pos.TimeEnter,
                Volume = pos.Volume,
                ResultDepo = (float)pos.ResultDepo,
                ResultPoints = (float)pos.ResultPoints,
                ResultBase = (float)pos.ResultBase,
                ExitReason = (PositionExitReason)pos.ExitReason
            };
            return mo;
        }

        public static PendingOrder DecoratePendingOrder(PENDING_ORDER ord)
        {
            var po = new PendingOrder
            {
                ID = ord.ID,
                AccountID = ord.AccountID,
                PriceSide = (PendingOrderType)ord.PriceSide,
                PriceFrom = (float)ord.PriceFrom,
                PriceTo = (float?)ord.PriceTo,
                TimeFrom = ord.TimeFrom,
                TimeTo = ord.TimeTo,
                Symbol = ord.Symbol,
                Volume = ord.Volume,
                Side = ord.Side,
                Magic = ord.Magic,
                Comment = ord.Comment,
                ExpertComment = ord.ExpertComment,
                StopLoss = (float?)(ord.Stoploss == 0 ? null : ord.Stoploss),
                TakeProfit = (float?)(ord.Takeprofit == 0 ? null : ord.Takeprofit),
                Status = PendingOrderStatus.Создан,
                PairOCO = ord.PairOCO,
                TrailLevel1 = (float?)ord.TrailLevel1,
                TrailLevel2 = (float?)ord.TrailLevel2,
                TrailLevel3 = (float?)ord.TrailLevel3,
                TrailLevel4 = (float?)ord.TrailLevel4,
                TrailTarget1 = (float?)ord.TrailTarget1,
                TrailTarget2 = (float?)ord.TrailTarget2,
                TrailTarget3 = (float?)ord.TrailTarget3,
                TrailTarget4 = (float?)ord.TrailTarget4
            };
            return po;
        }

        public static PendingOrder DecoratePendingOrder(PENDING_ORDER_CLOSED ord)
        {
            var po = new PendingOrder
            {
                ID = ord.OrderID,
                AccountID = ord.AccountID,
                PriceSide = (PendingOrderType)ord.PriceSide,
                PriceFrom = (float)ord.PriceFrom,
                PriceTo = (float?)ord.PriceTo,
                TimeFrom = ord.TimeFrom,
                TimeTo = ord.TimeTo,
                Symbol = ord.Symbol,
                Volume = ord.Volume,
                Side = ord.Side,
                Magic = ord.Magic,
                Comment = ord.Comment,
                ExpertComment = ord.ExpertComment,
                StopLoss = (float?)(ord.Stoploss == 0 ? null : ord.Stoploss),
                TakeProfit = (float?)(ord.Takeprofit == 0 ? null : ord.Takeprofit),
                Status = (PendingOrderStatus)ord.Status,
                PairOCO = ord.PairOCO,
                TimeClosed = ord.TimeClosed,
                PriceClosed = (float?)ord.PriceClosed,
                CloseReason = ord.CloseReason,
            };
            return po;
        }

        public static PENDING_ORDER UndecorateLiveActiveOrder(PendingOrder ord)
        {
            var po = new PENDING_ORDER
            {
                ID = ord.ID,
                AccountID = ord.AccountID,
                PriceSide = (int)ord.PriceSide,
                PriceFrom = (decimal)ord.PriceFrom,
                PriceTo = (decimal?)ord.PriceTo,
                TimeFrom = ord.TimeFrom,
                TimeTo = ord.TimeTo,
                Symbol = ord.Symbol,
                Volume = ord.Volume,
                Side = ord.Side,
                Magic = ord.Magic,
                Comment = ord.Comment,
                ExpertComment = ord.ExpertComment,
                Stoploss = (decimal?)(ord.StopLoss == 0 ? null : ord.StopLoss),
                Takeprofit = (decimal?)(ord.TakeProfit == 0 ? null : ord.TakeProfit),
                PairOCO = ord.PairOCO,
                TrailLevel1 = (decimal?)ord.TrailLevel1,
                TrailLevel2 = (decimal?)ord.TrailLevel2,
                TrailLevel3 = (decimal?)ord.TrailLevel3,
                TrailLevel4 = (decimal?)ord.TrailLevel4,
                TrailTarget1 = (decimal?)ord.TrailTarget1,
                TrailTarget2 = (decimal?)ord.TrailTarget2,
                TrailTarget3 = (decimal?)ord.TrailTarget3,
                TrailTarget4 = (decimal?)ord.TrailTarget4                
            };
            return po;
        }

        public static Account DecorateAccount(ACCOUNT ac)
        {
            var account = new Account
            {
                ID = ac.ID,
                Currency = ac.Currency,
                Group = ac.AccountGroup,
                MaxLeverage = (float)ac.MaxLeverage,
                Balance = ac.Balance,
                TimeCreated = ac.TimeCreated,
                TimeBlocked = ac.TimeBlocked,
                Status = (Account.AccountStatus)ac.Status
            };
            return account;
        }

        public static void DecorateAccount(Account dest, ACCOUNT src)
        {
            dest.ID = src.ID;
            dest.Currency = src.Currency;
            dest.Group = src.AccountGroup;
            dest.MaxLeverage = (float) src.MaxLeverage;
            dest.Balance = src.Balance;
            dest.TimeCreated = src.TimeCreated;
            dest.TimeBlocked = src.TimeBlocked;
            dest.Status = (Account.AccountStatus) src.Status;
        }

        public static AccountGroup DecorateAccountGroup(ACCOUNT_GROUP ag)
        {
            var group = new AccountGroup
            {
                Code = ag.Code,
                BrokerLeverage = (float)ag.BrokerLeverage,
                DefaultVirtualDepo = ag.DefaultVirtualDepo ?? 0,
                IsReal = ag.IsReal,
                MarginCallPercentLevel = (float)ag.MarginCallPercentLevel,
                Name = ag.Name,
                StopoutPercentLevel = (float)ag.StopoutPercentLevel,
                Markup = (AccountGroup.MarkupType)ag.MarkupType,
                DefaultMarkupPoints = (float)ag.DefaultMarkupPoints
            };
            return group;
        }

        public static AccountGroup DecorateAccountGroup(ACCOUNT_GROUP ag, DEALER_GROUP dealerGroup)
        {
            var group = DecorateAccountGroup(ag);
            if (dealerGroup == null) return group;
            group.MessageQueue = dealerGroup.MessageQueue;
            group.SessionName = dealerGroup.SessionName;
            group.HedgingAccount = dealerGroup.HedgingAccount;
            group.SenderCompId = dealerGroup.SenderCompId;
            group.Markup = (AccountGroup.MarkupType)ag.MarkupType;
            group.DefaultMarkupPoints = (float)ag.DefaultMarkupPoints;
            group.Dealer = new DealerDescription { Code = dealerGroup.Dealer };
            return group;
        }

        public static POSITION_CLOSED UndecorateClosedPosition(MarketOrder pos)
        {
            // ReSharper disable PossibleInvalidOperationException
            var p = new POSITION_CLOSED
            {
                ID = pos.ID,
                AccountID = pos.AccountID,
                Comment = pos.Comment,
                ExitReason = (int)pos.ExitReason,
                ExpertComment = pos.ExpertComment,
                Magic = pos.Magic,
                PendingOrderID = pos.PendingOrderID,
                PriceBest = (decimal?)pos.PriceBest,
                PriceWorst = (decimal?)pos.PriceWorst,
                PriceEnter = (decimal)pos.PriceEnter,
                ResultBase = (decimal)pos.ResultBase,
                ResultDepo = (decimal)pos.ResultDepo,
                ResultPoints = (decimal)pos.ResultPoints,
                Side = pos.Side,
                Stoploss = (decimal?)pos.StopLoss,
                Swap = (decimal)(pos.Swap ?? 0),
                Symbol = pos.Symbol,
                Takeprofit = (decimal?)pos.TakeProfit,
                TimeEnter = pos.TimeEnter,
                Volume = pos.Volume,
                PriceExit = (decimal)pos.PriceExit.Value,
                TimeExit = pos.TimeExit.Value
            };
            // ReSharper restore PossibleInvalidOperationException
            return p;
        }

        public static POSITION UndecorateOpenedPosition(MarketOrder pos)
        {
            // ReSharper disable PossibleInvalidOperationException
            var p = new POSITION
            {
                //ID = pos.ID,
                AccountID = pos.AccountID,
                Comment = pos.Comment,
                ExpertComment = pos.ExpertComment,
                Magic = pos.Magic,
                PendingOrderID = pos.PendingOrderID,
                PriceBest = (decimal?)pos.PriceBest,
                PriceWorst = (decimal?)pos.PriceWorst,
                PriceEnter = (decimal)pos.PriceEnter,
                Side = pos.Side,
                Stoploss = (decimal?)pos.StopLoss,
                Symbol = pos.Symbol,
                Takeprofit = (decimal?)pos.TakeProfit,
                TimeEnter = pos.TimeEnter,
                Volume = pos.Volume,
                State = (int)pos.State,
                MasterOrder = pos.MasterOrder
            };
            // ReSharper restore PossibleInvalidOperationException
            return p;
        }

        public static PlatformUser DecoratePlatformUser(PLATFORM_USER user)
        {
            var us = new PlatformUser
            {
                ID = user.ID,
                Title = user.Title,
                Login = user.Login,
                Name = user.Name,
                Password = user.Password,
                Patronym = user.Patronym,
                Phone1 = user.Phone1,
                Phone2 = user.Phone2,
                RoleMask = (UserRole)user.RoleMask,
                Surname = user.Surname,
                Description = user.Description,
                Email = user.Email,
                RegistrationDate = user.RegistrationDate
            };
            return us;
        }

        public static PLATFORM_USER UndecoratePlatformUser(PlatformUser user)
        {
            var us = new PLATFORM_USER
            {
                ID = user.ID,
                Title = user.Title,
                Login = user.Login,
                Name = user.Name,
                Password = user.Password,
                Patronym = user.Patronym,
                Phone1 = user.Phone1,
                Phone2 = user.Phone2,
                RoleMask = (int)user.RoleMask,
                Surname = user.Surname,
                Description = user.Description,
                Email = user.Email,
                RegistrationDate = user.RegistrationDate
            };
            return us;
        }

        public static BrokerOrder DecorateBrokerOrder(BROKER_ORDER ord)
        {
            var order = new BrokerOrder
            {
                RequestId = ord.RequestID,
                Id = ord.ID,
                Ticker = ord.Ticker,
                Instrument = (Instrument)ord.Instrument,
                Volume = ord.Volume,
                Side = ord.Side,
                OrderPricing = (OrderPricing)ord.OrderPricing,
                RequestedPrice = ord.RequestedPrice,
                Slippage = ord.Slippage,
                DealerCode = ord.Dealer,
                AccountID = ord.AccountID,
                ClosingPositionID = ord.ClosingPositionID,
                TimeCreated = ord.TimeCreated,
                Magic = ord.Magic,
                Comment = ord.Comment,
                ExpertComment = ord.ExpertComment,
                MarkupAbs = (float)ord.Markup
            };
            return order;
        }

        public static BROKER_ORDER UndecorateBrokerOrder(BrokerOrder ord)
        {
            var order = new BROKER_ORDER
            {
                RequestID = ord.RequestId,
                ID = ord.Id,
                Ticker = ord.Ticker,
                Instrument = (int)ord.Instrument,
                Volume = ord.Volume,
                Side = ord.Side,
                OrderPricing = (int)ord.OrderPricing,
                RequestedPrice = ord.RequestedPrice,
                Slippage = ord.Slippage,
                Dealer = ord.DealerCode,
                AccountID = ord.AccountID,
                ClosingPositionID = ord.ClosingPositionID,
                TimeCreated = ord.TimeCreated,
                Magic = ord.Magic,
                Comment = ord.Comment,
                ExpertComment = ord.ExpertComment,
                Markup = ord.MarkupAbs
            };
            return order;
        }

        public static BrokerResponse DecorateBrokerResponse(BROKER_RESPONSE resp)
        {
            return new BrokerResponse
            {
                Id = resp.ID,
                RequestId = resp.RequestID,
                Price = resp.Price,
                Swap = resp.Swap,
                Status = (OrderStatus)resp.Status,
                RejectReason = resp.RejectReason == null
                                   ? (OrderRejectReason?)null
                                   : (OrderRejectReason)resp.RejectReason,
                RejectReasonString = resp.RejectReasonString,
                ValueDate = resp.ValueDate
            };
        }

        public static BROKER_RESPONSE UndecorateBrokerResponse(BrokerResponse resp)
        {
            return new BROKER_RESPONSE
            {
                ID = resp.Id,
                RequestID = resp.RequestId,
                Price = resp.Price,
                Swap = resp.Swap,
                Status = (int)resp.Status,
                RejectReason = resp.RejectReason == null
                                   ? (int?)null
                                   : (int)resp.RejectReason,
                RejectReasonString = resp.RejectReasonString,
                ValueDate = resp.ValueDate
            };
        }

        public static DealerDescription DecorateDealerDescription(DEALER dealer)
        {
            return new DealerDescription
            {
                Code = dealer.Code,
                DealerEnabled = dealer.DealerEnabled,
                FileName = dealer.FileName
            };
        }

        public static BalanceChange DecorateBalanceChange(BALANCE_CHANGE bc)
        {
            return new BalanceChange
            {
                AccountID = bc.AccountID,
                Amount = bc.Amount,
                ChangeType = (BalanceChangeType)bc.ChangeType,
                CurrencyToDepoRate = 1,
                Description = bc.Description,
                ID = bc.ID,
                ValueDate = bc.ValueDate,
                PositionId = bc.Position
            };
        }

        public static AutoTradeSettings DecorateAutoTradeSettings(SUBSCRIPTION_SIGNAL cat)
        {
            return new AutoTradeSettings
            {
                FixedVolume = cat.FixedVolume,
                HedgingOrdersEnabled = cat.HedgingOrdersEnabled,
                MaxLeverage = cat.MaxLeverage,
                MaxVolume = cat.MaxVolume,
                MinVolume = cat.MinVolume,
                PercentLeverage = cat.PercentLeverage ?? 100,
                StepVolume = cat.StepVolume,
                TradeAuto = cat.AutoTrade ?? false,
                VolumeRound = (VolumeRoundType?)cat.VolumeRound,
                TargetAccount = cat.TargetAccount
            };
        }

        public static SUBSCRIPTION_SIGNAL UndecorateAutoTradeSettings(AutoTradeSettings cat)
        {
            return new SUBSCRIPTION_SIGNAL
            {
                FixedVolume = cat.FixedVolume,
                HedgingOrdersEnabled = cat.HedgingOrdersEnabled,
                MaxLeverage = cat.MaxLeverage,
                MaxVolume = cat.MaxVolume,
                MinVolume = cat.MinVolume,
                PercentLeverage = cat.PercentLeverage,
                StepVolume = cat.StepVolume,
                AutoTrade = cat.TradeAuto,
                VolumeRound = (int?)cat.VolumeRound
            };
        }

        public static UserEvent DecorateUserEvent(USER_EVENT evt)
        {
            return new UserEvent
                       {
                           User = evt.User,
                           Time = evt.Time,
                           Action = (AccountEventAction)evt.Action,
                           Code = (AccountEventCode)evt.Code,
                           Text = evt.Text,
                           Title = evt.Title,
                           AccountId = evt.Account
                       };
        }

        public static USER_EVENT UndecorateUserEvent(UserEvent evt)
        {
            return new USER_EVENT
            {
                User = evt.User,
                Time = evt.Time,
                Action = (short)evt.Action,
                Code = (short)evt.Code,
                Text = evt.Text,
                Title = evt.Title,
                Account = evt.AccountId
            };
        }

        public static OrderBill DecorateOrderBill(ORDER_BILL bill)
        {
            return new OrderBill
                {
                    Position = bill.Position,
                    MarkupEnter = (float)bill.MarkupEnter,
                    MarkupExit = (float)bill.MarkupExit,
                    MarkupBroker = (float)bill.MarkupBroker,
                    MarkupType = (AccountGroup.MarkupType)bill.MarkupType,
                    ProfitBroker = (float)bill.ProfitBroker
                };
        }

        public static ORDER_BILL UndecorateOrderBill(OrderBill bill)
        {
            return new ORDER_BILL
            {
                Position = bill.Position,
                MarkupEnter = bill.MarkupEnter,
                MarkupExit = bill.MarkupExit,
                MarkupBroker = bill.MarkupBroker,
                MarkupType = (int)bill.MarkupType,
                ProfitBroker = bill.ProfitBroker
            };
        }

        /// <summary>
        /// мапинг объекта Account в ACCOUNT, с использованием переданного контекста
        /// Это нужно для того, что бы потом была возможномть выполнить обновление (потому что обновление должно выполняться в этом же контексте) 
        /// </summary>
        /// <param name="dest">его значения обновятся</param>
        /// <param name="src">объек для мапинга</param>
        /// <returns></returns>
        public static void UndecorateAccount(ACCOUNT dest, Account src)
        {
            dest.Currency = src.Currency;
            dest.AccountGroup = src.Group;
            dest.MaxLeverage = (decimal)src.MaxLeverage;
            dest.UsedMargin = src.UsedMargin;
            dest.Balance = src.Balance;
            dest.Status = (int)src.Status;
            dest.TimeCreated = src.TimeCreated;
            dest.TimeBlocked = src.TimeBlocked;
        }

        public static ACCOUNT UndecorateAccount(Account src)
        {
            var dest = new ACCOUNT();
            UndecorateAccount(dest, src);
            return dest;
        }

        /// <summary>
        /// Мапинг объекта "группа счетов" в объект Entity FW. Возвращает новый объект ACCOUNT_GROUP
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static ACCOUNT_GROUP UndecorateAccountGroup(AccountGroup src)
        {
            var result = new ACCOUNT_GROUP
                {
                    Code = src.Code,
                    Name = src.Name,
                    IsReal = src.IsReal,
                    BrokerLeverage = (decimal)src.BrokerLeverage,
                    MarginCallPercentLevel = (decimal)src.MarginCallPercentLevel,
                    StopoutPercentLevel = (decimal)src.StopoutPercentLevel,
                    DefaultMarkupPoints = src.DefaultMarkupPoints,
                    DefaultVirtualDepo = src.DefaultVirtualDepo,
                    MarkupType = (int)src.Markup,
                    SwapFree = src.SwapFree
                };
            return result;
        }

        /// <summary>
        /// Мапинг объекта "группа счетов" в объект Entity FW. Изменяется переданный объект.
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="src"></param>
        public static void UndecorateAccountGroup(ACCOUNT_GROUP dest, AccountGroup src)
        {
            dest.Code = src.Code;
            dest.Name = src.Name;
            dest.IsReal = src.IsReal;
            dest.BrokerLeverage = (decimal)src.BrokerLeverage;
            dest.MarginCallPercentLevel = (decimal)src.MarginCallPercentLevel;
            dest.StopoutPercentLevel = (decimal)src.StopoutPercentLevel;
            dest.DefaultMarkupPoints = src.DefaultMarkupPoints;
            dest.DefaultVirtualDepo = src.DefaultVirtualDepo;
            dest.MarkupType = (int)src.Markup;
            dest.SwapFree = src.SwapFree;
        }


        /// <summary>
        /// Этот метод ещё нужно дорабатывать. DEALER_PARAMETER, DEALER_GROUP и т.п. тут не задаётся
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static DEALER UndecorateDealer(DealerDescription src)
        {
            var result = new DEALER
            {
                Code = src.Code,
                FileName = src.FileName,
                DealerEnabled = src.DealerEnabled
            };
            return result;
        }

        /// <summary>
        /// Этот метод ещё нужно дорабатывать. DEALER_PARAMETER, DEALER_GROUP и т.п. тут не задаётся
        /// </summary>
        public static void UndecorateDealer(DEALER dest, DealerDescription src)
        {
            dest.Code = src.Code;
            dest.FileName = src.FileName;
            dest.DealerEnabled = src.DealerEnabled;
        }

        public static Wallet DecorateWallet(WALLET walInf)
        {
            var wallet = new Wallet
                {
                    Balance = walInf.Balance,
                    Currency = walInf.Currency,
                    User = walInf.User,
                    Password = walInf.Password
                };
            return wallet;
        }

        public static Subscription DecorateSubscription(SUBSCRIPTION sub)
        {
            return new Subscription
                {
                    User = sub.User,
                    RenewAuto = sub.RenewAuto,
                    Service = sub.Service,
                    TimeEnd = sub.TimeEnd,
                    TimeStarted = sub.TimeStarted
                };
        }

        public static Subscription DecorateSubscription(SUBSCRIPTION_V sub)
        {
            var sb = new Subscription
            {
                User = sub.User,
                RenewAuto = sub.RenewAuto ?? true,
                Service = sub.Service,
                TimeEnd = sub.TimeEnd,
                TimeStarted = sub.TimeStarted
            };
            sb.PaidService = new PaidService
                {
                    Id = sub.Service,
                    Comment = sub.Comment,
                    FixedPrice = sub.FixedPrice ?? 0,
                    AccountId = sub.AccountId,
                    Currency = sub.Currency,
                    ServiceType = (PaidServiceType) sub.ServiceType
                };
            if (sub.AutoTrade.HasValue)
            {
                sb.AutoTradeSettings = new AutoTradeSettings
                    {
                        TradeAuto = sub.AutoTrade.Value,
                        VolumeRound = (VolumeRoundType?) sub.VolumeRound,
                        FixedVolume = sub.FixedVolume,
                        HedgingOrdersEnabled = sub.HedgingOrdersEnabled,
                        MaxLeverage = sub.MaxLeverage,
                        MaxVolume = sub.MaxVolume,
                        MinVolume = sub.MinVolume,
                        PercentLeverage = sub.PercentLeverage ?? 100,
                        StepVolume = sub.StepVolume,
                        TargetAccount = sub.TargetAccount
                    };
            }

            return sb;
        }

        public static PaidService DecoratePaidService(SERVICE srv)
        {
            return new PaidService
                {
                    User = srv.User,
                    Comment = srv.Comment,
                    FixedPrice = srv.FixedPrice,
                    Id = srv.ID,
                    ServiceType = (PaidServiceType)srv.ServiceType,
                    Currency = srv.Currency,
                    AccountId = srv.AccountId, 
                    serviceRates = new List<PaidServiceRate>()
                };
        }

        public static Transfer DecorateTransfer(TRANSFER trans)
        {
            return new Transfer
                {
                    Amount = trans.Amount,
                    BalanceChange = trans.BalanceChange,
                    Comment = trans.Comment,
                    Id = trans.ID,
                    RefWallet = trans.RefWallet,
                    Subscription = trans.Subscription,
                    TargetAmount = trans.TargetAmount,                    
                    User = trans.User,
                    ValueDate = trans.ValueDate
                };
        }

        public static AccountShare DecorateAccountShare(ACCOUNT_SHARE share)
        {
            return new AccountShare
            {
                SharePercent = share.Share,
                UserId = share.ShareOwner,
                HWM = share.HWM ?? 0
            };
        }

        public static UserPaymentSystem DecorateUserPaymentSystem(USER_PAYMENT_SYSTEM userPaySys)
        {
            if (userPaySys == null) return null;
            var sp = PaymentSystem.Unknown;
            try
            {
                sp = (PaymentSystem)Enum.ToObject(typeof(PaymentSystem), userPaySys.SystemPayment);
            }
            catch (Exception ex)
            {
                var message = string.Format("Не удалось распознать платёжную систему. " +
                                            "Проверьте в базе данных в таблице USER_PAYMENT_SYSTEM значение в столбце SystemPayment " +
                                            "для записи с UserId : {0}, PurseId {1}, RootId: {2}", 
                                            userPaySys.UserId, userPaySys.PurseId, userPaySys.RootId);
                Logger.Error(message, ex);
            }
                
            return new UserPaymentSystem
                {
                    Id = userPaySys.Id,
                    UserId = userPaySys.UserId,
                    PurseId = userPaySys.PurseId,
                    RootId = userPaySys.RootId,
                    SystemPayment = sp,
                    PurseConfirm = userPaySys.PurseConfirm,
                    FirstName = userPaySys.FirstName,
                    LastName = userPaySys.LastName,
                    Email = userPaySys.Email
                };
        }

        public static USER_PAYMENT_SYSTEM UndecorateUserPaymentSystem(UserPaymentSystem userPaySys)
        {
            if (userPaySys == null) return null;
            return new USER_PAYMENT_SYSTEM
            {
                Id = userPaySys.Id,
                UserId = userPaySys.UserId,
                PurseId = userPaySys.PurseId,
                RootId = userPaySys.RootId,
                SystemPayment = (byte)userPaySys.SystemPayment,
                PurseConfirm = userPaySys.PurseConfirm,
                FirstName = userPaySys.FirstName,
                LastName = userPaySys.LastName,
                Email = userPaySys.Email
            };
        }

        public static PaymentSystemTransfer DecoratePaymentSystemTransfer(PAYMENT_SYSTEM_TRANSFER paySysTransfer)
        {
            if (paySysTransfer == null) return null;
            return new PaymentSystemTransfer
                {
                    Id = paySysTransfer.Id,
                    UserPaymentSys = paySysTransfer.UserPaymentSys,
                    Ammount = paySysTransfer.Ammount,
                    Currency = paySysTransfer.Currency,
                    DateProcessed = paySysTransfer.DateProcessed,
                    DateValue = paySysTransfer.DateValue,
                    Comment = paySysTransfer.Comment,
                    Transfer = paySysTransfer.Transfer,
                    SourcePaySysAccount = paySysTransfer.SourcePaySysAccount,
                    SourcePaySysPurse = paySysTransfer.SourcePaySysPurse,
                    SourseFirstName = paySysTransfer.SourseFirstName,
                    SourseLastName = paySysTransfer.SourseLastName,
                    SourseEmail = paySysTransfer.SourseEmail,
                };
        }

        public static PAYMENT_SYSTEM_TRANSFER UndecoratePaymentSystemTransfer(PaymentSystemTransfer paySysTransfer)
        {
            if (paySysTransfer == null) return null;
            return new PAYMENT_SYSTEM_TRANSFER
            {
                Id = paySysTransfer.Id,
                UserPaymentSys = paySysTransfer.UserPaymentSys,
                Ammount = paySysTransfer.Ammount,
                Currency = paySysTransfer.Currency,
                DateProcessed = paySysTransfer.DateProcessed,
                DateValue = paySysTransfer.DateValue,
                Comment = paySysTransfer.Comment,
                Transfer = paySysTransfer.Transfer,
                SourcePaySysAccount = paySysTransfer.SourcePaySysAccount,
                SourcePaySysPurse = paySysTransfer.SourcePaySysPurse,
                SourseFirstName = paySysTransfer.SourseFirstName,
                SourseLastName = paySysTransfer.SourseLastName,
                SourseEmail = paySysTransfer.SourseEmail
            };
        }

        public static TopPortfolio DecoratePortfolio(TOP_PORTFOLIO p)
        {
            return new TopPortfolio
                {
                    Criteria = p.Criteria,
                    DescendingOrder = p.DescendingOrder,
                    Id = p.Id,
                    ManagedAccount = p.ManagedAccount,
                    MarginValue = (float?) p.MarginValue,
                    Name = p.Name,
                    ParticipantCount = p.ParticipantCount,
                    OwnerUser = p.OwnerUser
                };
        }

        public static TOP_PORTFOLIO UndecoratePortfolio(TopPortfolio p)
        {
            return new TOP_PORTFOLIO
            {
                Criteria = p.Criteria,
                DescendingOrder = p.DescendingOrder,
                Id = p.Id,
                ManagedAccount = p.ManagedAccount,
                MarginValue = p.MarginValue,
                Name = p.Name,
                ParticipantCount = p.ParticipantCount,
                OwnerUser = p.OwnerUser
            };
        }

        public static void UndecoratePortfolio(TOP_PORTFOLIO dest, TopPortfolio src)
        {
            dest.Criteria = src.Criteria;
            dest.DescendingOrder = src.DescendingOrder;
            dest.ManagedAccount = src.ManagedAccount;
            dest.MarginValue = src.MarginValue;
            dest.Name = src.Name;
            dest.OwnerUser = src.OwnerUser;
            dest.ParticipantCount = src.ParticipantCount;
        }
    
        public static AccountShareOnDate DecorateAccountShareHistory(ACCOUNT_SHARE_HISTORY record)
        {
            return new AccountShareOnDate
                {
                    date = record.Date,
                    newHWM = record.NewHWM,
                    newShare = record.NewShare,
                    oldHWM = record.OldHWM,
                    oldShare = record.OldShare,
                    shareAmount = record.ShareAmount
                };
        }
    }
}
