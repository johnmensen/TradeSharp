using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.UI.Util.Update;
using TradeSharp.Util;

namespace TradeSharp.Test.Moq
{
    public static class TestDataGenerator
    {
        public static List<POSITION> GetOpenPosition()
        {
            return new List<POSITION>
                {
                    new POSITION
                        {
                            ID = 2599,
                            Magic = null,
                            Comment = "",
                            AccountID = 18,
                            MasterOrder = null,
                            ExpertComment = "634904164226131750",
                            Symbol = "USDJPY",
                            Takeprofit = null,
                            Side = -1,
                            Volume = 50000,
                            Stoploss = null,
                            PriceEnter = 80.51300m,
                            TimeEnter = "02.11.2012 20:41:20".ToDateTimeUniform(),
                            State = 1,
                            PendingOrderID = null,
                            PriceBest = null,
                            PriceWorst = null,
                            TrailLevel1 = null,
                            TrailLevel2 = null,
                            TrailLevel3 = null,
                            TrailLevel4 = null,
                            TrailTarget1 = null,
                            TrailTarget2 = null,
                            TrailTarget3 = null,
                            TrailTarget4 = null
                        },
                    new POSITION
                        {
                            ID = 2815,
                            Magic = null,
                            Comment = "",
                            AccountID = 29,
                            MasterOrder = null,
                            ExpertComment = "",
                            Symbol = "GBPUSD",
                            Takeprofit = null,
                            Side = 1,
                            Volume = 100000,
                            Stoploss = null,
                            PriceEnter = 1.61483m,
                            TimeEnter = "11.01.2013 17:28:44".ToDateTimeUniform(),
                            State = 1,
                            PendingOrderID = null,
                            PriceBest = null,
                            PriceWorst = null,
                            TrailLevel1 = null,
                            TrailLevel2 = null,
                            TrailLevel3 = null,
                            TrailLevel4 = null,
                            TrailTarget1 = null,
                            TrailTarget2 = null,
                            TrailTarget3 = null,
                            TrailTarget4 = null
                        },
                    new POSITION
                        {
                            ID = 2906,
                            Magic = null,
                            Comment = "",
                            AccountID = 29,
                            MasterOrder = null,
                            ExpertComment = "",
                            Symbol = "GBPUSD",
                            Takeprofit = null,
                            Side = 1,
                            Volume = 10000,
                            Stoploss = null,
                            PriceEnter = 1.61210m,
                            TimeEnter = "14.01.2013 13:44:42".ToDateTimeUniform(),
                            State = 1,
                            PendingOrderID = null,
                            PriceBest = null,
                            PriceWorst = null,
                            TrailLevel1 = null,
                            TrailLevel2 = null,
                            TrailLevel3 = null,
                            TrailLevel4 = null,
                            TrailTarget1 = null,
                            TrailTarget2 = null,
                            TrailTarget3 = null,
                            TrailTarget4 = null
                        },
                    new POSITION
                        {
                            ID = 2956,
                            Magic = null,
                            Comment = "",
                            AccountID = 22,
                            MasterOrder = null,
                            ExpertComment = "634946598573203501",
                            Symbol = "USDJPY",
                            Takeprofit = (decimal?) 85.00000,
                            Side = -1,
                            Volume = 250000,
                            Stoploss = null,
                            PriceEnter = 90.12000m,
                            TimeEnter = "24.01.2013 21:30:57".ToDateTimeUniform(),
                            State = 1,
                            PendingOrderID = null,
                            PriceBest = null,
                            PriceWorst = null,
                            TrailLevel1 = null,
                            TrailLevel2 = null,
                            TrailLevel3 = null,
                            TrailLevel4 = null,
                            TrailTarget1 = null,
                            TrailTarget2 = null,
                            TrailTarget3 = null,
                            TrailTarget4 = null
                        },
                    new POSITION
                        {
                            ID = 2986,
                            Magic = null,
                            Comment = "",
                            AccountID = 25,
                            MasterOrder = null,
                            ExpertComment = "",
                            Symbol = "USDJPY",
                            Takeprofit = (decimal?) 75.00000,
                            Side = -1,
                            Volume = 10000,
                            Stoploss = (decimal?) 0.00000,
                            PriceEnter = 78.95700m,
                            TimeEnter = "16.10.2012 19:21:00".ToDateTimeUniform(),
                            State = 1,
                            PendingOrderID = null,
                            PriceBest = null,
                            PriceWorst = null,
                            TrailLevel1 = null,
                            TrailLevel2 = null,
                            TrailLevel3 = null,
                            TrailLevel4 = null,
                            TrailTarget1 = null,
                            TrailTarget2 = null,
                            TrailTarget3 = null,
                            TrailTarget4 = null
                        },
                    new POSITION
                        {
                            ID = 2987,
                            Magic = null,
                            Comment = "",
                            AccountID = 25,
                            MasterOrder = null,
                            ExpertComment = "",
                            Symbol = "USDJPY",
                            Takeprofit = (decimal?) 75.00000,
                            Side = -1,
                            Volume = 10000,
                            Stoploss = (decimal?) 0.00000,
                            PriceEnter = 78.70400m,
                            TimeEnter = "17.10.2012 08:01:00".ToDateTimeUniform(),
                            State = 1,
                            PendingOrderID = null,
                            PriceBest = null,
                            PriceWorst = null,
                            TrailLevel1 = null,
                            TrailLevel2 = null,
                            TrailLevel3 = null,
                            TrailLevel4 = null,
                            TrailTarget1 = null,
                            TrailTarget2 = null,
                            TrailTarget3 = null,
                            TrailTarget4 = null
                        },
                    new POSITION
                        {
                            ID = 2988,
                            Magic = null,
                            Comment = "",
                            AccountID = 25,
                            MasterOrder = null,
                            ExpertComment = "",
                            Symbol = "USDJPY",
                            Takeprofit = (decimal?) 75.00000,
                            Side = -1,
                            Volume = 10000,
                            Stoploss = (decimal?) 0.00000,
                            PriceEnter = 78.91600m,
                            TimeEnter = "18.10.2012 01:08:00".ToDateTimeUniform(),
                            State = 1,
                            PendingOrderID = null,
                            PriceBest = null,
                            PriceWorst = null,
                            TrailLevel1 = null,
                            TrailLevel2 = null,
                            TrailLevel3 = null,
                            TrailLevel4 = null,
                            TrailTarget1 = null,
                            TrailTarget2 = null,
                            TrailTarget3 = null,
                            TrailTarget4 = null
                        },
                    new POSITION
                        {
                            ID = 2990,
                            Magic = null,
                            Comment = "",
                            AccountID = 25,
                            MasterOrder = null,
                            ExpertComment = "",
                            Symbol = "USDJPY",
                            Takeprofit = (decimal?) 75.00000,
                            Side = -1,
                            Volume = 10000,
                            Stoploss = (decimal?) 0.00000,
                            PriceEnter = 79.13500m,
                            TimeEnter = "18.10.2012 13:06:00".ToDateTimeUniform(),
                            State = 1,
                            PendingOrderID = null,
                            PriceBest = null,
                            PriceWorst = null,
                            TrailLevel1 = null,
                            TrailLevel2 = null,
                            TrailLevel3 = null,
                            TrailLevel4 = null,
                            TrailTarget1 = null,
                            TrailTarget2 = null,
                            TrailTarget3 = null,
                            TrailTarget4 = null
                        },
                    new POSITION
                        {
                            ID = 2992,
                            Magic = null,
                            Comment = "",
                            AccountID = 25,
                            MasterOrder = null,
                            ExpertComment = "",
                            Symbol = "USDJPY",
                            Takeprofit = (decimal?) 75.00000,
                            Side = -1,
                            Volume = 10000,
                            Stoploss = (decimal?) 0.00000,
                            PriceEnter = 79.32200m,
                            TimeEnter = "18.10.2012 15:19:00".ToDateTimeUniform(),
                            State = 1,
                            PendingOrderID = null,
                            PriceBest = null,
                            PriceWorst = null,
                            TrailLevel1 = null,
                            TrailLevel2 = null,
                            TrailLevel3 = null,
                            TrailLevel4 = null,
                            TrailTarget1 = null,
                            TrailTarget2 = null,
                            TrailTarget3 = null,
                            TrailTarget4 = null
                        },
                    new POSITION
                        {
                            ID = 2993,
                            Magic = null,
                            Comment = "",
                            AccountID = 25,
                            MasterOrder = null,
                            ExpertComment = "",
                            Symbol = "USDJPY",
                            Takeprofit = (decimal?) 75.00000,
                            Side = -1,
                            Volume = 10000,
                            Stoploss = (decimal?) 0.00000,
                            PriceEnter = 79.27500m,
                            TimeEnter = "18.10.2012 18:57:00".ToDateTimeUniform(),
                            State = 1,
                            PendingOrderID = null,
                            PriceBest = null,
                            PriceWorst = null,
                            TrailLevel1 = null,
                            TrailLevel2 = null,
                            TrailLevel3 = null,
                            TrailLevel4 = null,
                            TrailTarget1 = null,
                            TrailTarget2 = null,
                            TrailTarget3 = null,
                            TrailTarget4 = null
                        },
                    new POSITION
                        {
                            ID = 2994,
                            Magic = null,
                            Comment = "",
                            AccountID = 25,
                            MasterOrder = null,
                            ExpertComment = "",
                            Symbol = "USDJPY",
                            Takeprofit = (decimal?) 75.00000,
                            Side = -1,
                            Volume = 10000,
                            Stoploss = (decimal?) 0.00000,
                            PriceEnter = 79.28400m,
                            TimeEnter = "18.10.2012 18:58:00".ToDateTimeUniform(),
                            State = 1,
                            PendingOrderID = null,
                            PriceBest = null,
                            PriceWorst = null,
                            TrailLevel1 = null,
                            TrailLevel2 = null,
                            TrailLevel3 = null,
                            TrailLevel4 = null,
                            TrailTarget1 = null,
                            TrailTarget2 = null,
                            TrailTarget3 = null,
                            TrailTarget4 = null
                        },
                    new POSITION
                        {
                            ID = 2997,
                            Magic = null,
                            Comment = "",
                            AccountID = 25,
                            MasterOrder = null,
                            ExpertComment = "",
                            Symbol = "USDJPY",
                            Takeprofit = (decimal?) 75.00000,
                            Side = -1,
                            Volume = 10000,
                            Stoploss = (decimal?) 0.00000,
                            PriceEnter = 79.36600m,
                            TimeEnter = "19.10.2012 19:03:00".ToDateTimeUniform(),
                            State = 1,
                            PendingOrderID = null,
                            PriceBest = null,
                            PriceWorst = null,
                            TrailLevel1 = null,
                            TrailLevel2 = null,
                            TrailLevel3 = null,
                            TrailLevel4 = null,
                            TrailTarget1 = null,
                            TrailTarget2 = null,
                            TrailTarget3 = null,
                            TrailTarget4 = null
                        },
                    new POSITION
                        {
                            ID = 2999,
                            Magic = null,
                            Comment = "",
                            AccountID = 25,
                            MasterOrder = null,
                            ExpertComment = "",
                            Symbol = "USDJPY",
                            Takeprofit = (decimal?) 75.00000,
                            Side = -1,
                            Volume = 10000,
                            Stoploss = (decimal?) 0.00000,
                            PriceEnter = 79.33500m,
                            TimeEnter = "19.10.2012 20:55:00".ToDateTimeUniform(),
                            State = 1,
                            PendingOrderID = null,
                            PriceBest = null,
                            PriceWorst = null,
                            TrailLevel1 = null,
                            TrailLevel2 = null,
                            TrailLevel3 = null,
                            TrailLevel4 = null,
                            TrailTarget1 = null,
                            TrailTarget2 = null,
                            TrailTarget3 = null,
                            TrailTarget4 = null
                        },
                    new POSITION
                        {
                            ID = 3000,
                            Magic = null,
                            Comment = "",
                            AccountID = 25,
                            MasterOrder = null,
                            ExpertComment = "",
                            Symbol = "USDJPY",
                            Takeprofit = (decimal?) 75.00000,
                            Side = -1,
                            Volume = 10000,
                            Stoploss = (decimal?) 0.00000,
                            PriceEnter = 79.61900m,
                            TimeEnter = "22.10.2012 09:12:00".ToDateTimeUniform(),
                            State = 1,
                            PendingOrderID = null,
                            PriceBest = null,
                            PriceWorst = null,
                            TrailLevel1 = null,
                            TrailLevel2 = null,
                            TrailLevel3 = null,
                            TrailLevel4 = null,
                            TrailTarget1 = null,
                            TrailTarget2 = null,
                            TrailTarget3 = null,
                            TrailTarget4 = null
                        },
                    new POSITION
                        {
                            ID = 3013,
                            Magic = null,
                            Comment = "",
                            AccountID = 25,
                            MasterOrder = null,
                            ExpertComment = "",
                            Symbol = "USDJPY",
                            Takeprofit = (decimal?) 75.00000,
                            Side = -1,
                            Volume = 10000,
                            Stoploss = (decimal?) 0.00000,
                            PriceEnter = 80.20900m,
                            TimeEnter = "25.10.2012 16:43:00".ToDateTimeUniform(),
                            State = 1,
                            PendingOrderID = null,
                            PriceBest = null,
                            PriceWorst = null,
                            TrailLevel1 = null,
                            TrailLevel2 = null,
                            TrailLevel3 = null,
                            TrailLevel4 = null,
                            TrailTarget1 = null,
                            TrailTarget2 = null,
                            TrailTarget3 = null,
                            TrailTarget4 = null
                        },
                    new POSITION
                        {
                            ID = 3014,
                            Magic = null,
                            Comment = "",
                            AccountID = 25,
                            MasterOrder = null,
                            ExpertComment = "",
                            Symbol = "USDJPY",
                            Takeprofit = (decimal?) 75.00000,
                            Side = -1,
                            Volume = 10000,
                            Stoploss = (decimal?) 0.00000,
                            PriceEnter = 80.20000m,
                            TimeEnter = "25.10.2012 16:46:00".ToDateTimeUniform(),
                            State = 1,
                            PendingOrderID = null,
                            PriceBest = null,
                            PriceWorst = null,
                            TrailLevel1 = null,
                            TrailLevel2 = null,
                            TrailLevel3 = null,
                            TrailLevel4 = null,
                            TrailTarget1 = null,
                            TrailTarget2 = null,
                            TrailTarget3 = null,
                            TrailTarget4 = null
                        },
                    new POSITION
                        {
                            ID = 3015,
                            Magic = null,
                            Comment = "",
                            AccountID = 25,
                            MasterOrder = null,
                            ExpertComment = "",
                            Symbol = "USDJPY",
                            Takeprofit = (decimal?) 75.00000,
                            Side = -1,
                            Volume = 10000,
                            Stoploss = (decimal?) 0.00000,
                            PriceEnter = 79.79300m,
                            TimeEnter = "29.10.2012 21:14:00".ToDateTimeUniform(),
                            State = 1,
                            PendingOrderID = null,
                            PriceBest = null,
                            PriceWorst = null,
                            TrailLevel1 = null,
                            TrailLevel2 = null,
                            TrailLevel3 = null,
                            TrailLevel4 = null,
                            TrailTarget1 = null,
                            TrailTarget2 = null,
                            TrailTarget3 = null,
                            TrailTarget4 = null
                        },
                    new POSITION
                        {
                            ID = 3019,
                            Magic = null,
                            Comment = "",
                            AccountID = 25,
                            MasterOrder = null,
                            ExpertComment = "",
                            Symbol = "USDJPY",
                            Takeprofit = (decimal?) 75.00000,
                            Side = -1,
                            Volume = 10000,
                            Stoploss = (decimal?) 0.00000,
                            PriceEnter = 79.44100m,
                            TimeEnter = "09.11.2012 22:53:00".ToDateTimeUniform(),
                            State = 1,
                            PendingOrderID = null,
                            PriceBest = null,
                            PriceWorst = null,
                            TrailLevel1 = null,
                            TrailLevel2 = null,
                            TrailLevel3 = null,
                            TrailLevel4 = null,
                            TrailTarget1 = null,
                            TrailTarget2 = null,
                            TrailTarget3 = null,
                            TrailTarget4 = null
                        },
                    new POSITION
                        {
                            ID = 3056,
                            Magic = null,
                            Comment = "",
                            AccountID = 22,
                            MasterOrder = null,
                            ExpertComment = "634951840710648622",
                            Symbol = "USDJPY",
                            Takeprofit = (decimal?) 85.00000,
                            Side = -1,
                            Volume = 400000,
                            Stoploss = null,
                            PriceEnter = 91.09000m,
                            TimeEnter = "30.01.2013 23:07:51".ToDateTimeUniform(),
                            State = 1,
                            PendingOrderID = null,
                            PriceBest = null,
                            PriceWorst = null,
                            TrailLevel1 = 109.00000m,
                            TrailLevel2 = null,
                            TrailLevel3 = null,
                            TrailLevel4 = null,
                            TrailTarget1 = 25.00000m,
                            TrailTarget2 = null,
                            TrailTarget3 = null,
                            TrailTarget4 = null
                        },
                    new POSITION
                        {
                            ID = 3310,
                            Magic = null,
                            Comment = "",
                            AccountID = 25,
                            MasterOrder = null,
                            ExpertComment = "634974330220565853",
                            Symbol = "EURUSD",
                            Takeprofit = null,
                            Side = 1,
                            Volume = 10000,
                            Stoploss = null,
                            PriceEnter = 1.30997m,
                            TimeEnter = "25.02.2013 23:50:22".ToDateTimeUniform(),
                            State = 1,
                            PendingOrderID = null,
                            PriceBest = null,
                            PriceWorst = null,
                            TrailLevel1 = null,
                            TrailLevel2 = null,
                            TrailLevel3 = null,
                            TrailLevel4 = null,
                            TrailTarget1 = null,
                            TrailTarget2 = null,
                            TrailTarget3 = null,
                            TrailTarget4 = null
                        },
                    new POSITION
                        {
                            ID = 3581,
                            Magic = null,
                            Comment = "",
                            AccountID = 8,
                            MasterOrder = null,
                            ExpertComment = "",
                            Symbol = "EURUSD",
                            Takeprofit = null,
                            Side = -1,
                            Volume = 10000,
                            Stoploss = null,
                            PriceEnter = 1.30175m,
                            TimeEnter = "05.03.2013 20:44:45".ToDateTimeUniform(),
                            State = 1,
                            PendingOrderID = null,
                            PriceBest = null,
                            PriceWorst = null,
                            TrailLevel1 = null,
                            TrailLevel2 = null,
                            TrailLevel3 = null,
                            TrailLevel4 = null,
                            TrailTarget1 = null,
                            TrailTarget2 = null,
                            TrailTarget3 = null,
                            TrailTarget4 = null
                        },
                    new POSITION
                        {
                            ID = 3647,
                            Magic = null,
                            Comment = "",
                            AccountID = 28,
                            MasterOrder = null,
                            ExpertComment = "",
                            Symbol = "EURUSD",
                            Takeprofit = null,
                            Side = -1,
                            Volume = 10000,
                            Stoploss = null,
                            PriceEnter = 1.29991m,
                            TimeEnter = "11.03.2013 00:37:59".ToDateTimeUniform(),
                            State = 1,
                            PendingOrderID = null,
                            PriceBest = null,
                            PriceWorst = null,
                            TrailLevel1 = null,
                            TrailLevel2 = null,
                            TrailLevel3 = null,
                            TrailLevel4 = null,
                            TrailTarget1 = null,
                            TrailTarget2 = null,
                            TrailTarget3 = null,
                            TrailTarget4 = null
                        },
                    new POSITION
                        {
                            ID = 3833,
                            Magic = null,
                            Comment = "",
                            AccountID = 30,
                            MasterOrder = null,
                            ExpertComment = "",
                            Symbol = "USDCHF",
                            Takeprofit = null,
                            Side = -1,
                            Volume = 10000,
                            Stoploss = null,
                            PriceEnter = 0.94190m,
                            TimeEnter = "15.03.2013 15:09:02".ToDateTimeUniform(),
                            State = 1,
                            PendingOrderID = null,
                            PriceBest = null,
                            PriceWorst = null,
                            TrailLevel1 = null,
                            TrailLevel2 = null,
                            TrailLevel3 = null,
                            TrailLevel4 = null,
                            TrailTarget1 = null,
                            TrailTarget2 = null,
                            TrailTarget3 = null,
                            TrailTarget4 = null
                        },
                    new POSITION
                        {
                            ID = 3859,
                            Magic = null,
                            Comment = "",
                            AccountID = 18,
                            MasterOrder = null,
                            ExpertComment = "",
                            Symbol = "EURUSD",
                            Takeprofit = null,
                            Side = 1,
                            Volume = 10000,
                            Stoploss = null,
                            PriceEnter = 1.29059m,
                            TimeEnter = "18.03.2013 11:32:20".ToDateTimeUniform(),
                            State = 1,
                            PendingOrderID = null,
                            PriceBest = null,
                            PriceWorst = null,
                            TrailLevel1 = null,
                            TrailLevel2 = null,
                            TrailLevel3 = null,
                            TrailLevel4 = null,
                            TrailTarget1 = null,
                            TrailTarget2 = null,
                            TrailTarget3 = null,
                            TrailTarget4 = null
                        },
                    new POSITION
                        {
                            ID = 3893,
                            Magic = null,
                            Comment = "",
                            AccountID = 18,
                            MasterOrder = null,
                            ExpertComment = "",
                            Symbol = "GBPUSD",
                            Takeprofit = null,
                            Side = 1,
                            Volume = 10000,
                            Stoploss = null,
                            PriceEnter = 1.50871m,
                            TimeEnter = "20.03.2013 10:43:32".ToDateTimeUniform(),
                            State = 1,
                            PendingOrderID = null,
                            PriceBest = null,
                            PriceWorst = null,
                            TrailLevel1 = null,
                            TrailLevel2 = null,
                            TrailLevel3 = null,
                            TrailLevel4 = null,
                            TrailTarget1 = null,
                            TrailTarget2 = null,
                            TrailTarget3 = null,
                            TrailTarget4 = null
                        }
                };
        }

        public static List<POSITION_CLOSED> GetClosePosition()
        {
            return new List<POSITION_CLOSED>
                {
                    new POSITION_CLOSED
                        {
                            ID = 16036,
                            Symbol = "USDJPY",
                            AccountID = 76,
                            PendingOrderID = null,
                            Volume = 10000,
                            Comment = "",
                            ExpertComment = "MARobot",
                            Magic = 2,
                            PriceEnter = 104.46600m,
                            PriceExit = 104.69800m,
                            TimeEnter = "26.12.2013 03:00:03".ToDateTimeUniform(),
                            TimeExit = "26.12.2013 19:59:53".ToDateTimeUniform(),
                            Side = 1,
                            Stoploss = (decimal?) 101.92600,
                            Takeprofit = (decimal?) 106.92600,
                            PriceBest = null,
                            PriceWorst = null,
                            ExitReason = 4,
                            Swap = (decimal) 0.00000,
                            ResultPoints = (decimal) 23.19,
                            ResultBase = (decimal) 0.00,
                            ResultDepo = (decimal) 22.15
                        },
                    new POSITION_CLOSED
                        {
                            ID = 16037,
                            Symbol = "USDJPY",
                            AccountID = 79,
                            PendingOrderID = null,
                            Volume = 10000,
                            Comment = "",
                            ExpertComment = "MARobot",
                            Magic = 3,
                            PriceEnter = 104.45900m,
                            PriceExit = 104.69600m,
                            TimeEnter = "26.12.2013 03:00:04".ToDateTimeUniform(),
                            TimeExit = "26.12.2013 19:59:54".ToDateTimeUniform(),
                            Side = 1,
                            Stoploss = (decimal?) 101.92600,
                            Takeprofit = (decimal?) 106.92600,
                            PriceBest = null,
                            PriceWorst = null,
                            ExitReason = 4,
                            Swap = (decimal) 0.00000,
                            ResultPoints = (decimal) 23.69,
                            ResultBase = (decimal) 0.00,
                            ResultDepo = (decimal) 22.63
                        },
                    new POSITION_CLOSED
                        {
                            ID = 16038,
                            Symbol = "GBPUSD",
                            AccountID = 46,
                            PendingOrderID = null,
                            Volume = 10000,
                            Comment = "",
                            ExpertComment = "MARobot",
                            Magic = 2,
                            PriceEnter = 1.63501m,
                            PriceExit = 1.63874m,
                            TimeEnter = "26.12.2013 03:00:05".ToDateTimeUniform(),
                            TimeExit = "26.12.2013 07:00:06".ToDateTimeUniform(),
                            Side = -1,
                            Stoploss = (decimal?) 1.66009,
                            Takeprofit = (decimal?) 1.61009,
                            PriceBest = null,
                            PriceWorst = null,
                            ExitReason = 4,
                            Swap = (decimal) 0.00000,
                            ResultPoints = (decimal) -37.29,
                            ResultBase = (decimal) 0.00,
                            ResultDepo = (decimal) -37.29
                        },
                    new POSITION_CLOSED
                        {
                            ID = 16039,
                            Symbol = "GBPUSD",
                            AccountID = 80,
                            PendingOrderID = null,
                            Volume = 10000,
                            Comment = "",
                            ExpertComment = "DoubleTopRobot",
                            Magic = 3,
                            PriceEnter = 1.63504m,
                            PriceExit = 1.65313m,
                            TimeEnter = "26.12.2013 03:59:52".ToDateTimeUniform(),
                            TimeExit = "27.12.2013 15:35:06".ToDateTimeUniform(),
                            Side = -1,
                            Stoploss = (decimal?) 1.65312,
                            Takeprofit = (decimal?) 0.80856,
                            PriceBest = null,
                            PriceWorst = null,
                            ExitReason = 1,
                            Swap = (decimal) 0.00000,
                            ResultPoints = (decimal) -180.90,
                            ResultBase = (decimal) 0.00,
                            ResultDepo = (decimal) -180.90
                        },
                    new POSITION_CLOSED
                        {
                            ID = 16040,
                            Symbol = "USDJPY",
                            AccountID = 22,
                            PendingOrderID = null,
                            Volume = 2570000,
                            Comment = "103.22;101.70",
                            ExpertComment = "",
                            Magic = 8,
                            PriceEnter = 104.50400m,
                            PriceExit = 104.25700m,
                            TimeEnter = "26.12.2013 04:00:07".ToDateTimeUniform(),
                            TimeExit = "15.01.2014 00:06:17".ToDateTimeUniform(),
                            Side = -1,
                            Stoploss = (decimal?) 104.25400,
                            Takeprofit = (decimal?) 99.96712,
                            PriceBest = null,
                            PriceWorst = null,
                            ExitReason = 1,
                            Swap = (decimal) 0.00000,
                            ResultPoints = (decimal) 24.69,
                            ResultBase = (decimal) 0.00,
                            ResultDepo = (decimal) 6088.55
                        },
                    new POSITION_CLOSED
                        {
                            ID = 16041,
                            Symbol = "USDCHF",
                            AccountID = 163,
                            PendingOrderID = null,
                            Volume = 10000,
                            Comment = "",
                            ExpertComment = "",
                            Magic = 1,
                            PriceEnter = 0.89620m,
                            PriceExit = 0.89625m,
                            TimeEnter = "26.12.2013 04:59:51".ToDateTimeUniform(),
                            TimeExit = "26.12.2013 23:00:00".ToDateTimeUniform(),
                            Side = -1,
                            Stoploss = null,
                            Takeprofit = null,
                            PriceBest = null,
                            PriceWorst = null,
                            ExitReason = 4,
                            Swap = (decimal) 0.00000,
                            ResultPoints = (decimal) -0.50,
                            ResultBase = (decimal) 0.00,
                            ResultDepo = (decimal) -0.55
                        },
                    new POSITION_CLOSED
                        {
                            ID = 16042,
                            Symbol = "EURUSD",
                            AccountID = 46,
                            PendingOrderID = null,
                            Volume = 10000,
                            Comment = "",
                            ExpertComment = "MARobot",
                            Magic = 3,
                            PriceEnter = 1.36726m,
                            PriceExit = 1.36796m,
                            TimeEnter = "26.12.2013 05:00:06".ToDateTimeUniform(),
                            TimeExit = "26.12.2013 06:59:53".ToDateTimeUniform(),
                            Side = -1,
                            Stoploss = (decimal?) 1.39235,
                            Takeprofit = (decimal?) 1.34235,
                            PriceBest = null,
                            PriceWorst = null,
                            ExitReason = 4,
                            Swap = (decimal) 0.00000,
                            ResultPoints = (decimal) -6.99,
                            ResultBase = (decimal) 0.00,
                            ResultDepo = (decimal) -6.99
                        },
                    new POSITION_CLOSED
                        {
                            ID = 16043,
                            Symbol = "EURUSD",
                            AccountID = 79,
                            PendingOrderID = null,
                            Volume = 10000,
                            Comment = "",
                            ExpertComment = "MARobot",
                            Magic = 1,
                            PriceEnter = 1.36726m,
                            PriceExit = 1.36797m,
                            TimeEnter = "26.12.2013 05:00:09".ToDateTimeUniform(),
                            TimeExit = "26.12.2013 06:59:56".ToDateTimeUniform(),
                            Side = -1,
                            Stoploss = (decimal?) 1.39235,
                            Takeprofit = (decimal?) 1.34235,
                            PriceBest = null,
                            PriceWorst = null,
                            ExitReason = 4,
                            Swap = (decimal) 0.00000,
                            ResultPoints = (decimal) -7.10,
                            ResultBase = (decimal) 0.00,
                            ResultDepo = (decimal) -7.10
                        },
                    new POSITION_CLOSED
                        {
                            ID = 16044,
                            Symbol = "USDCHF",
                            AccountID = 46,
                            PendingOrderID = null,
                            Volume = 10000,
                            Comment = "",
                            ExpertComment = "MARobot",
                            Magic = 4,
                            PriceEnter = 0.89676m,
                            PriceExit = 0.89600m,
                            TimeEnter = "26.12.2013 06:00:22".ToDateTimeUniform(),
                            TimeExit = "26.12.2013 14:59:52".ToDateTimeUniform(),
                            Side = 1,
                            Stoploss = (decimal?) 0.87149,
                            Takeprofit = (decimal?) 0.92149,
                            PriceBest = null,
                            PriceWorst = null,
                            ExitReason = 4,
                            Swap = (decimal) 0.00000,
                            ResultPoints = (decimal) -7.59,
                            ResultBase = (decimal) 0.00,
                            ResultDepo = (decimal) -8.48
                        },
                    new POSITION_CLOSED
                        {
                            ID = 16045,
                            Symbol = "EURUSD",
                            AccountID = 46,
                            PendingOrderID = null,
                            Volume = 10000,
                            Comment = "",
                            ExpertComment = "MARobot",
                            Magic = 3,
                            PriceEnter = 1.36806m,
                            PriceExit = 1.36733m,
                            TimeEnter = "26.12.2013 06:59:53".ToDateTimeUniform(),
                            TimeExit = "26.12.2013 08:00:00".ToDateTimeUniform(),
                            Side = 1,
                            Stoploss = (decimal?) 1.34285,
                            Takeprofit = (decimal?) 1.39285,
                            PriceBest = null,
                            PriceWorst = null,
                            ExitReason = 4,
                            Swap = (decimal) 0.00000,
                            ResultPoints = (decimal) -7.30,
                            ResultBase = (decimal) 0.00,
                            ResultDepo = (decimal) -7.30
                        },
                    new POSITION_CLOSED
                        {
                            ID = 16046,
                            Symbol = "EURUSD",
                            AccountID = 75,
                            PendingOrderID = null,
                            Volume = 10000,
                            Comment = "",
                            ExpertComment = "DoubleTopRobot",
                            Magic = 1,
                            PriceEnter = 1.36806m,
                            PriceExit = 1.36785m,
                            TimeEnter = "26.12.2013 06:59:54".ToDateTimeUniform(),
                            TimeExit = "26.12.2013 06:59:54".ToDateTimeUniform(),
                            Side = 1,
                            Stoploss = (decimal?) 1.34285,
                            Takeprofit = (decimal?) 0.69642,
                            PriceBest = null,
                            PriceWorst = null,
                            ExitReason = 2,
                            Swap = (decimal) 0.00000,
                            ResultPoints = (decimal) -2.10,
                            ResultBase = (decimal) 0.00,
                            ResultDepo = (decimal) -2.10
                        },
                    new POSITION_CLOSED
                        {
                            ID = 16047,
                            Symbol = "EURUSD",
                            AccountID = 79,
                            PendingOrderID = null,
                            Volume = 10000,
                            Comment = "",
                            ExpertComment = "MARobot",
                            Magic = 1,
                            PriceEnter = 1.36807m,
                            PriceExit = 1.36733m,
                            TimeEnter = "26.12.2013 06:59:56".ToDateTimeUniform(),
                            TimeExit = "26.12.2013 08:00:03".ToDateTimeUniform(),
                            Side = 1,
                            Stoploss = (decimal?) 1.34285,
                            Takeprofit = (decimal?) 1.39285,
                            PriceBest = null,
                            PriceWorst = null,
                            ExitReason = 4,
                            Swap = (decimal) 0.00000,
                            ResultPoints = (decimal) -7.40,
                            ResultBase = (decimal) 0.00,
                            ResultDepo = (decimal) -7.40
                        },
                    new POSITION_CLOSED
                        {
                            ID = 16048,
                            Symbol = "EURUSD",
                            AccountID = 82,
                            PendingOrderID = null,
                            Volume = 10000,
                            Comment = "",
                            ExpertComment = "DoubleTopRobot",
                            Magic = 1,
                            PriceEnter = 1.36807m,
                            PriceExit = 1.36785m,
                            TimeEnter = "26.12.2013 06:59:57".ToDateTimeUniform(),
                            TimeExit = "26.12.2013 06:59:57".ToDateTimeUniform(),
                            Side = 1,
                            Stoploss = (decimal?) 1.34285,
                            Takeprofit = (decimal?) 0.69642,
                            PriceBest = null,
                            PriceWorst = null,
                            ExitReason = 2,
                            Swap = (decimal) 0.00000,
                            ResultPoints = (decimal) -2.20,
                            ResultBase = (decimal) 0.00,
                            ResultDepo = (decimal) -2.20
                        },
                    new POSITION_CLOSED
                        {
                            ID = 16049,
                            Symbol = "EURUSD",
                            AccountID = 153,
                            PendingOrderID = null,
                            Volume = 10000,
                            Comment = "",
                            ExpertComment = "",
                            Magic = 1,
                            PriceEnter = 1.36775m,
                            PriceExit = 1.36927m,
                            TimeEnter = "26.12.2013 06:59:57".ToDateTimeUniform(),
                            TimeExit = "26.12.2013 22:59:55".ToDateTimeUniform(),
                            Side = -1,
                            Stoploss = (decimal?) 1.37285,
                            Takeprofit = null,
                            PriceBest = null,
                            PriceWorst = null,
                            ExitReason = 4,
                            Swap = (decimal) 0.00000,
                            ResultPoints = (decimal) -15.19,
                            ResultBase = (decimal) 0.00,
                            ResultDepo = (decimal) -15.19
                        },
                    new POSITION_CLOSED
                        {
                            ID = 16050,
                            Symbol = "GBPUSD",
                            AccountID = 46,
                            PendingOrderID = null,
                            Volume = 10000,
                            Comment = "",
                            ExpertComment = "MARobot",
                            Magic = 2,
                            PriceEnter = 1.63884m,
                            PriceExit = 1.64709m,
                            TimeEnter = "26.12.2013 07:00:06".ToDateTimeUniform(),
                            TimeExit = "30.12.2013 05:00:01".ToDateTimeUniform(),
                            Side = 1,
                            Stoploss = (decimal?) 1.61361,
                            Takeprofit = (decimal?) 1.66361,
                            PriceBest = null,
                            PriceWorst = null,
                            ExitReason = 4,
                            Swap = (decimal) 0.00000,
                            ResultPoints = (decimal) 82.49,
                            ResultBase = (decimal) 0.00,
                            ResultDepo = (decimal) 82.49
                        },
                    new POSITION_CLOSED
                        {
                            ID = 16051,
                            Symbol = "EURUSD",
                            AccountID = 46,
                            PendingOrderID = null,
                            Volume = 10000,
                            Comment = "",
                            ExpertComment = "MARobot",
                            Magic = 3,
                            PriceEnter = 1.36723m,
                            PriceExit = 1.36808m,
                            TimeEnter = "26.12.2013 08:00:00".ToDateTimeUniform(),
                            TimeExit = "26.12.2013 09:59:51".ToDateTimeUniform(),
                            Side = -1,
                            Stoploss = (decimal?) 1.39233,
                            Takeprofit = (decimal?) 1.34233,
                            PriceBest = null,
                            PriceWorst = null,
                            ExitReason = 4,
                            Swap = (decimal) 0.00000,
                            ResultPoints = (decimal) -8.49,
                            ResultBase = (decimal) 0.00,
                            ResultDepo = (decimal) -8.49
                        },
                    new POSITION_CLOSED
                        {
                            ID = 16052,
                            Symbol = "EURUSD",
                            AccountID = 57,
                            PendingOrderID = null,
                            Volume = 10000,
                            Comment = "",
                            ExpertComment = "MARobot",
                            Magic = 3,
                            PriceEnter = 1.36723m,
                            PriceExit = 1.36857m,
                            TimeEnter = "26.12.2013 08:00:01".ToDateTimeUniform(),
                            TimeExit = "26.12.2013 11:59:51".ToDateTimeUniform(),
                            Side = -1,
                            Stoploss = (decimal?) 1.37533,
                            Takeprofit = (decimal?) 1.35233,
                            PriceBest = null,
                            PriceWorst = null,
                            ExitReason = 4,
                            Swap = (decimal) 0.00000,
                            ResultPoints = (decimal) -13.39,
                            ResultBase = (decimal) 0.00,
                            ResultDepo = (decimal) -13.39
                        },
                    new POSITION_CLOSED
                        {
                            ID = 16053,
                            Symbol = "EURUSD",
                            AccountID = 55,
                            PendingOrderID = null,
                            Volume = 10000,
                            Comment = "",
                            ExpertComment = "MARobot",
                            Magic = 2,
                            PriceEnter = 1.36723m,
                            PriceExit = 1.36857m,
                            TimeEnter = "26.12.2013 08:00:01".ToDateTimeUniform(),
                            TimeExit = "26.12.2013 11:59:52".ToDateTimeUniform(),
                            Side = -1,
                            Stoploss = (decimal?) 1.37533,
                            Takeprofit = (decimal?) 1.35233,
                            PriceBest = null,
                            PriceWorst = null,
                            ExitReason = 4,
                            Swap = (decimal) 0.00000,
                            ResultPoints = (decimal) -13.39,
                            ResultBase = (decimal) 0.00,
                            ResultDepo = (decimal) -13.39
                        },
                    new POSITION_CLOSED
                        {
                            ID = 16054,
                            Symbol = "EURUSD",
                            AccountID = 79,
                            PendingOrderID = null,
                            Volume = 10000,
                            Comment = "",
                            ExpertComment = "MARobot",
                            Magic = 1,
                            PriceEnter = 1.36723m,
                            PriceExit = 1.36807m,
                            TimeEnter = "26.12.2013 08:00:04".ToDateTimeUniform(),
                            TimeExit = "26.12.2013 09:59:55".ToDateTimeUniform(),
                            Side = -1,
                            Stoploss = (decimal?) 1.39233,
                            Takeprofit = (decimal?) 1.34233,
                            PriceBest = null,
                            PriceWorst = null,
                            ExitReason = 4,
                            Swap = (decimal) 0.00000,
                            ResultPoints = (decimal) -8.39,
                            ResultBase = (decimal) 0.00,
                            ResultDepo = (decimal) -8.39
                        },
                    new POSITION_CLOSED
                        {
                            ID = 16055,
                            Symbol = "EURUSD",
                            AccountID = 158,
                            PendingOrderID = null,
                            Volume = 10000,
                            Comment = "",
                            ExpertComment = "",
                            Magic = 1,
                            PriceEnter = 1.36765m,
                            PriceExit = 1.36894m,
                            TimeEnter = "26.12.2013 08:00:05".ToDateTimeUniform(),
                            TimeExit = "26.12.2013 14:59:54".ToDateTimeUniform(),
                            Side = 1,
                            Stoploss = (decimal?) 1.35245,
                            Takeprofit = null,
                            PriceBest = null,
                            PriceWorst = null,
                            ExitReason = 4,
                            Swap = (decimal) 0.00000,
                            ResultPoints = (decimal) 12.89,
                            ResultBase = (decimal) 0.00,
                            ResultDepo = (decimal) 12.89
                        },
                    new POSITION_CLOSED
                        {
                            ID = 16056,
                            Symbol = "USDJPY",
                            AccountID = 22,
                            PendingOrderID = null,
                            Volume = 2550000,
                            Comment = "103.22;101.70",
                            ExpertComment = "",
                            Magic = 8,
                            PriceEnter = 104.72300m,
                            PriceExit = 104.47400m,
                            TimeEnter = "26.12.2013 08:00:20".ToDateTimeUniform(),
                            TimeExit = "15.01.2014 08:21:06".ToDateTimeUniform(),
                            Side = -1,
                            Stoploss = (decimal?) 104.47300,
                            Takeprofit = (decimal?) 99.83178,
                            PriceBest = null,
                            PriceWorst = null,
                            ExitReason = 1,
                            Swap = (decimal) 0.00000,
                            ResultPoints = (decimal) 24.90,
                            ResultBase = (decimal) 0.00,
                            ResultDepo = (decimal) 6077.60
                        },
                    new POSITION_CLOSED
                        {
                            ID = 16057,
                            Symbol = "EURUSD",
                            AccountID = 46,
                            PendingOrderID = null,
                            Volume = 10000,
                            Comment = "",
                            ExpertComment = "MARobot",
                            Magic = 3,
                            PriceEnter = 1.36818m,
                            PriceExit = 1.37676m,
                            TimeEnter = "26.12.2013 09:59:51".ToDateTimeUniform(),
                            TimeExit = "27.12.2013 21:59:51".ToDateTimeUniform(),
                            Side = 1,
                            Stoploss = (decimal?) 1.34300,
                            Takeprofit = (decimal?) 1.39300,
                            PriceBest = null,
                            PriceWorst = null,
                            ExitReason = 4,
                            Swap = (decimal) 0.00000,
                            ResultPoints = (decimal) 85.79,
                            ResultBase = (decimal) 0.00,
                            ResultDepo = (decimal) 85.79
                        },
                    new POSITION_CLOSED
                        {
                            ID = 16058,
                            Symbol = "EURUSD",
                            AccountID = 77,
                            PendingOrderID = null,
                            Volume = 10000,
                            Comment = "",
                            ExpertComment = "MARobot",
                            Magic = 3,
                            PriceEnter = 1.36818m,
                            PriceExit = 1.37545m,
                            TimeEnter = "26.12.2013 09:59:53".ToDateTimeUniform(),
                            TimeExit = "30.12.2013 05:00:02".ToDateTimeUniform(),
                            Side = 1,
                            Stoploss = (decimal?) 1.34300,
                            Takeprofit = (decimal?) 1.39300,
                            PriceBest = null,
                            PriceWorst = null,
                            ExitReason = 4,
                            Swap = (decimal) 0.00000,
                            ResultPoints = (decimal) 72.69,
                            ResultBase = (decimal) 0.00,
                            ResultDepo = (decimal) 72.69
                        },
                    new POSITION_CLOSED
                        {
                            ID = 16059,
                            Symbol = "EURUSD",
                            AccountID = 78,
                            PendingOrderID = null,
                            Volume = 10000,
                            Comment = "",
                            ExpertComment = "MARobot",
                            Magic = 2,
                            PriceEnter = 1.36820m,
                            PriceExit = 1.37545m,
                            TimeEnter = "26.12.2013 09:59:54".ToDateTimeUniform(),
                            TimeExit = "30.12.2013 05:00:03".ToDateTimeUniform(),
                            Side = 1,
                            Stoploss = (decimal?) 1.34300,
                            Takeprofit = (decimal?) 1.39300,
                            PriceBest = null,
                            PriceWorst = null,
                            ExitReason = 4,
                            Swap = (decimal) 0.00000,
                            ResultPoints = (decimal) 72.50,
                            ResultBase = (decimal) 0.00,
                            ResultDepo = (decimal) 72.50
                        },
                    new POSITION_CLOSED
                        {
                            ID = 16060,
                            Symbol = "EURUSD",
                            AccountID = 79,
                            PendingOrderID = null,
                            Volume = 10000,
                            Comment = "",
                            ExpertComment = "MARobot",
                            Magic = 1,
                            PriceEnter = 1.36817m,
                            PriceExit = 1.37676m,
                            TimeEnter = "26.12.2013 09:59:55".ToDateTimeUniform(),
                            TimeExit = "27.12.2013 21:59:54".ToDateTimeUniform(),
                            Side = 1,
                            Stoploss = (decimal?) 1.34300,
                            Takeprofit = (decimal?) 1.39300,
                            PriceBest = null,
                            PriceWorst = null,
                            ExitReason = 4,
                            Swap = (decimal) 0.00000,
                            ResultPoints = (decimal) 85.89,
                            ResultBase = (decimal) 0.00,
                            ResultDepo = (decimal) 85.89
                        }
                };
        }

        public static List<BALANCE_CHANGE> GetBalanceChange()
        {
            return new List<BALANCE_CHANGE>
                {
                    new BALANCE_CHANGE
                        {
                            ID = 5,
                            AccountID = 1,
                            ChangeType = 1,
                            Amount = 20000.00m,
                            ValueDate = "26.12.2013 19:59:53".ToDateTimeUniform(),
                            Description = "Начальный взнос",
                            Position = 16036,
                        },
                    new BALANCE_CHANGE
                        {
                            ID = 13,
                            AccountID = 1,
                            ChangeType = 1,
                            Amount = 50750.00m,
                            ValueDate = "26.12.2013 19:59:54".ToDateTimeUniform(),
                            Description = "Начальный депозит",
                            Position = 16037,
                        },
                    new BALANCE_CHANGE
                        {
                            ID = 17,
                            AccountID = 1,
                            ChangeType = 4,
                            Amount = 0.00m,
                            ValueDate = "26.12.2013 07:00:06".ToDateTimeUniform(),
                            Description = "результат сделки #21",
                            Position = 16038,
                        },
                    new BALANCE_CHANGE
                        {
                            ID = 18,
                            AccountID = 1,
                            ChangeType = 3,
                            Amount = 0.00m,
                            ValueDate = "27.12.2013 15:35:06".ToDateTimeUniform(),
                            Description = "результат сделки #20",
                            Position = 16039,
                        },
                    new BALANCE_CHANGE
                        {
                            ID = 38,
                            AccountID = 1,
                            ChangeType = 1,
                            Amount = 100000.00m,
                            ValueDate = "15.01.2014 00:06:17".ToDateTimeUniform(),
                            Description = "Начальный депозит",
                            Position = 16040,
                        },
                    new BALANCE_CHANGE
                        {
                            ID = 57,
                            AccountID = 1,
                            ChangeType = 1,
                            Amount = 15000.00m,
                            ValueDate = "26.12.2013 23:00:00".ToDateTimeUniform(),
                            Description = "Initial deposit (test)",
                            Position = 16041,
                        },
                    new BALANCE_CHANGE
                        {
                            ID = 78,
                            AccountID = 1,
                            ChangeType = 2,
                            Amount = 3.00m,
                            ValueDate = "26.12.2013 06:59:53".ToDateTimeUniform(),
                            Description = "результат сделки #77",
                            Position = 16042,
                        },
                    new BALANCE_CHANGE
                        {
                            ID = 82,
                            AccountID = 1,
                            ChangeType = 2,
                            Amount = 50.70m,
                            ValueDate = "26.12.2013 06:59:56".ToDateTimeUniform(),
                            Description = "результат сделки #85",
                            Position = 16043,
                        },
                    new BALANCE_CHANGE
                        {
                            ID = 339,
                            AccountID = 1,
                            ChangeType = 1,
                            Amount = 500000.00m,
                            ValueDate = "26.12.2013 14:59:52".ToDateTimeUniform(),
                            Description = "Initial depo",
                            Position = 16044,
                        },
                    new BALANCE_CHANGE
                        {
                            ID = 371,
                            AccountID = 1,
                            ChangeType = 4,
                            Amount = 3.30m,
                            ValueDate = "26.12.2013 08:00:00".ToDateTimeUniform(),
                            Description = "результат сделки #389",
                            Position = 16045,
                        },
                    new BALANCE_CHANGE
                        {
                            ID = 377,
                            AccountID = 1,
                            ChangeType = 4,
                            Amount = 72.20m,
                            ValueDate = "26.12.2013 06:59:54".ToDateTimeUniform(),
                            Description = "результат сделки #390",
                            Position = 16046,
                        },
                    new BALANCE_CHANGE
                        {
                            ID = 378,
                            AccountID = 1,
                            ChangeType = 3,
                            Amount = 0.80m,
                            ValueDate = "26.12.2013 08:00:03".ToDateTimeUniform(),
                            Description = "результат сделки #396",
                            Position = 16047,
                        },
                    new BALANCE_CHANGE
                        {
                            ID = 417,
                            AccountID = 1,
                            ChangeType = 1,
                            Amount = 500000.00m,
                            ValueDate = "26.12.2013 06:59:57".ToDateTimeUniform(),
                            Description = "Deposit payment",
                            Position = 16048,
                        },
                    new BALANCE_CHANGE
                        {
                            ID = 429,
                            AccountID = 1,
                            ChangeType = 3,
                            Amount = 730.00m,
                            ValueDate = "26.12.2013 22:59:55".ToDateTimeUniform(),
                            Description = "результат сделки #425",
                            Position = 16049,
                        },
                    new BALANCE_CHANGE
                        {
                            ID = 430,
                            AccountID = 1,
                            ChangeType = 3,
                            Amount = 101.80m,
                            ValueDate = "30.12.2013 05:00:01".ToDateTimeUniform(),
                            Description = "результат сделки #424",
                            Position = 16050,
                        },
                    new BALANCE_CHANGE
                        {
                            ID = 434,
                            AccountID = 1,
                            ChangeType = 1,
                            Amount = 110000.00m,
                            ValueDate = "26.12.2013 09:59:51".ToDateTimeUniform(),
                            Description = "Внесение средств",
                            Position = 16051,
                        },
                    new BALANCE_CHANGE
                        {
                            ID = 436,
                            AccountID = 1,
                            ChangeType = 3,
                            Amount = 57.40m,
                            ValueDate = "26.12.2013 11:59:51".ToDateTimeUniform(),
                            Description = "результат сделки #418",
                            Position = 16052,
                        },
                    new BALANCE_CHANGE
                        {
                            ID = 437,
                            AccountID = 1,
                            ChangeType = 4,
                            Amount = 102.00m,
                            ValueDate = "26.12.2013 11:59:52".ToDateTimeUniform(),
                            Description = "результат сделки #506",
                            Position = 16053,
                        },
                    new BALANCE_CHANGE
                        {
                            ID = 438,
                            AccountID = 1,
                            ChangeType = 4,
                            Amount = 63.72m,
                            ValueDate = "26.12.2013 09:59:55".ToDateTimeUniform(),
                            Description = "результат сделки #507",
                            Position = 16054,
                        },
                    new BALANCE_CHANGE
                        {
                            ID = 439,
                            AccountID = 1,
                            ChangeType = 4,
                            Amount = 108.00m,
                            ValueDate = "26.12.2013 14:59:54".ToDateTimeUniform(),
                            Description = "результат сделки #509",
                            Position = 16055,
                        },
                    new BALANCE_CHANGE
                        {
                            ID = 440,
                            AccountID = 1,
                            ChangeType = 4,
                            Amount = 99.00m,
                            ValueDate = "15.01.2014 08:21:06".ToDateTimeUniform(),
                            Description = "результат сделки #508",
                            Position = 16056,
                        },
                    new BALANCE_CHANGE
                        {
                            ID = 441,
                            AccountID = 1,
                            ChangeType = 4,
                            Amount = 814.00m,
                            ValueDate = "27.12.2013 21:59:51".ToDateTimeUniform(),
                            Description = "результат сделки #510",
                            Position = 16057,
                        },
                    new BALANCE_CHANGE
                        {
                            ID = 442,
                            AccountID = 1,
                            ChangeType = 4,
                            Amount = 61.70m,
                            ValueDate = "30.12.2013 05:00:02".ToDateTimeUniform(),
                            Description = "результат сделки #490",
                            Position = 16058,
                        },
                    new BALANCE_CHANGE
                        {
                            ID = 443,
                            AccountID = 1,
                            ChangeType = 3,
                            Amount = 152.00m,
                            ValueDate = "30.12.2013 05:00:03".ToDateTimeUniform(),
                            Description = "результат сделки #513",
                            Position = 16059,
                        },
                    new BALANCE_CHANGE
                        {
                            ID = 444,
                            AccountID = 1,
                            ChangeType = 3,
                            Amount = 1275.00m,
                            ValueDate = "27.12.2013 21:59:54".ToDateTimeUniform(),
                            Description = "результат сделки #514",
                            Position = 16060,
                        },
                    new BALANCE_CHANGE
                        {
                            ID = 445,
                            AccountID = 1,
                            ChangeType = 1,
                            Amount = 20000m,
                            ValueDate = "04.04.2014 17:54:42".ToDateTimeUniform(),
                            Description = "Initial depo",
                            Position = null,
                        },
                    new BALANCE_CHANGE
                        {
                            ID = 446,
                            AccountID = 1,
                            ChangeType = 1,
                            Amount = 20000m,
                            ValueDate = "03.04.2014 17:54:42".ToDateTimeUniform(),
                            Description = "Initial depo",
                            Position = null,
                        },
                    new BALANCE_CHANGE
                        {
                            ID = 447,
                            AccountID = 1,
                            ChangeType = 1,
                            Amount = 20000m,
                            ValueDate = "02.04.2014 17:54:42".ToDateTimeUniform(),
                            Description = "Initial depo",
                            Position = null,
                        },
                    new BALANCE_CHANGE
                        {
                            ID = 448,
                            AccountID = 1,
                            ChangeType = 1,
                            Amount = 20000m,
                            ValueDate = "01.04.2014 17:54:42".ToDateTimeUniform(),
                            Description = "Initial depo",
                            Position = null,
                        },
                    new BALANCE_CHANGE
                        {
                            ID = 449,
                            AccountID = 1,
                            ChangeType = 1,
                            Amount = 20000m,
                            ValueDate = "31.03.2014 17:54:42".ToDateTimeUniform(),
                            Description = "Initial depo",
                            Position = null,
                        }
                };
        }

        public static List<EquityOnTime> GetEquityOnTime()
        {
            return new List<EquityOnTime>
                {
                    new EquityOnTime(1043849, "16.03.2014 00:00:00".ToDateTimeUniform()),
                    new EquityOnTime(1139572, "17.03.2014 00:00:00".ToDateTimeUniform()),
                    new EquityOnTime(1161234, "18.03.2014 00:00:00".ToDateTimeUniform()),
                    new EquityOnTime(1049382, "19.03.2014 00:00:00".ToDateTimeUniform()),
                    new EquityOnTime(1082618, "20.03.2014 00:00:00".ToDateTimeUniform()),
                    new EquityOnTime(1190249, "21.03.2014 00:00:00".ToDateTimeUniform()),
                    new EquityOnTime(1084332, "22.03.2014 00:00:00".ToDateTimeUniform()),
                    new EquityOnTime(1048688, "23.03.2014 00:00:00".ToDateTimeUniform()),
                    new EquityOnTime(1159354, "24.03.2014 00:00:00".ToDateTimeUniform()),
                    new EquityOnTime(1141686, "25.03.2014 00:00:00".ToDateTimeUniform()),
                    new EquityOnTime(1044186, "26.03.2014 00:00:00".ToDateTimeUniform()),
                    new EquityOnTime(1100568, "27.03.2014 00:00:00".ToDateTimeUniform()),
                    new EquityOnTime(1186729, "28.03.2014 00:00:00".ToDateTimeUniform()),
                    new EquityOnTime(1069047, "29.03.2014 00:00:00".ToDateTimeUniform()),
                    new EquityOnTime(1057036, "30.03.2014 00:00:00".ToDateTimeUniform()),
                    new EquityOnTime(1175799, "31.03.2014 00:00:00".ToDateTimeUniform()),
                    new EquityOnTime(1140872, "01.04.2014 00:00:00".ToDateTimeUniform()),
                    new EquityOnTime(1082450, "02.04.2014 00:00:00".ToDateTimeUniform()),
                    new EquityOnTime(1180657, "03.04.2014 00:00:00".ToDateTimeUniform()),
                    new EquityOnTime(1255943, "04.04.2014 00:00:00".ToDateTimeUniform()),
                    new EquityOnTime(1157141, "05.04.2014 00:00:00".ToDateTimeUniform()),
                    new EquityOnTime(1168905, "06.04.2014 00:00:00".ToDateTimeUniform()),
                    new EquityOnTime(1286652, "07.04.2014 00:00:00".ToDateTimeUniform()),
                    new EquityOnTime(1200767, "08.04.2014 00:00:00".ToDateTimeUniform()),
                    new EquityOnTime(1144150, "09.04.2014 00:00:00".ToDateTimeUniform()),
                    new EquityOnTime(1241473, "10.04.2014 00:00:00".ToDateTimeUniform()),
                    new EquityOnTime(1259545, "11.04.2014 00:00:00".ToDateTimeUniform()),
                    new EquityOnTime(1148757, "12.04.2014 00:00:00".ToDateTimeUniform()),
                    new EquityOnTime(1184157, "13.04.2014 00:00:00".ToDateTimeUniform()),
                    new EquityOnTime(1290253, "14.04.2014 00:00:00".ToDateTimeUniform())
                };
        }

        // новее \\fastgrid.dll  \\fastmultichart.dll  \\terminal.chm  \\tradesharp.client.exe  
        //       \\tradesharp.client.exe.config  \\mt4\\udpcommandtranslator.mq4  \\sounds\\error.wav
        // имеют другой размер и хэш \\mt4\\udpcommandtranslator.mq4  \\sounds\\error.wav
        public static List<FileVersion> GetServerFiles()
        {
            return new List<FileVersion>
                {
                    new FileVersion
                        {
                            FileName = "\\fastgrid.dll",
                            Date = "17.04.2014 13:29:32".ToDateTimeUniform(),
                            Length = 70976,
                            HashCode = "abcdefghijklmnopqrstuvwxyz"
                        },
                    new FileVersion
                        {
                            FileName = "\\fastmultichart.dll",
                            Date = "17.04.2014 13:29:32".ToDateTimeUniform(),
                            Length = 34624,
                            HashCode = "abcdefghijklmnopqrstuvwxyz"
                        },
                    new FileVersion
                        {
                            FileName = "\\icsharpcode.sharpziplib.dll",
                            Date = "15.04.2014 13:29:32".ToDateTimeUniform(),
                            Length = 202048,
                            HashCode = "abcdefghijklmnopqrstuvwxyz"
                        },
                    new FileVersion
                        {
                            FileName = "\\terminal.chm",
                            Date = "17.04.2014 13:29:21".ToDateTimeUniform(),
                            Length = 4318613,
                            HashCode = "abcdefghijklmnopqrstuvwxyz"
                        },
                    new FileVersion
                        {
                            FileName = "\\tradesharp.client.exe",
                            Date = "17.04.2014 13:29:30".ToDateTimeUniform(),
                            Length = 3068736,
                            HashCode = "abcdefghijklmnopqrstuvwxyz"
                        },
                    new FileVersion
                        {
                            FileName = "\\tradesharp.client.exe.config",
                            Date = "27.02.2014 13:54:20".ToDateTimeUniform(),
                            Length = 7770,
                            HashCode = "abcdefghijklmnopqrstuvwxyz"
                        },
                    new FileVersion
                        {
                            FileName = "\\tradesharp.util.dll",
                            Date = "15.04.2014 13:29:37".ToDateTimeUniform(),
                            Length = 524608,
                            HashCode = "abcdefghijklmnopqrstuvwxyz"
                        },
                    new FileVersion
                        {
                            FileName = "\\mt4\\udpcommandtranslator.mq4",
                            Date = "25.11.2013 17:34:52".ToDateTimeUniform(),
                            Length = 9657,
                            HashCode = "abcdefghijklmnopqrstuvwxyz1"
                        },
                    new FileVersion
                        {
                            FileName = "\\mt4\\udpqueuelib.dll",
                            Date = "05.03.2013 11:17:06".ToDateTimeUniform(),
                            Length = 65024,
                            HashCode = "abcdefghijklmnopqrstuvwxyz"
                        },
                    new FileVersion
                        {
                            FileName = "\\sounds\\error.wav",
                            Date = "01.04.2013 12:05:15".ToDateTimeUniform(),
                            Length = 63371,
                            HashCode = "abcdefghijklmnopqrstuvwxyz1"
                        }
                };
        }

        public static List<FileVersion> GetOwnFiles()
        {
            return new List<FileVersion>
                {
                    new FileVersion
                        {
                            FileName = "\\fastgrid.dll",
                            Date = "15.04.2014 13:29:32".ToDateTimeUniform(),
                            Length = 70976,
                            HashCode = "abcdefghijklmnopqrstuvwxyz"
                        },
                    new FileVersion
                        {
                            FileName = "\\fastmultichart.dll",
                            Date = "15.04.2014 13:29:32".ToDateTimeUniform(),
                            Length = 34624,
                            HashCode = "abcdefghijklmnopqrstuvwxyz"
                        },
                    new FileVersion
                        {
                            FileName = "\\icsharpcode.sharpziplib.dll",
                            Date = "15.04.2014 13:29:32".ToDateTimeUniform(),
                            Length = 202048,
                            HashCode = "abcdefghijklmnopqrstuvwxyz"
                        },
                    new FileVersion
                        {
                            FileName = "\\terminal.chm",
                            Date = "15.04.2014 13:29:21".ToDateTimeUniform(),
                            Length = 4318613,
                            HashCode = "abcdefghijklmnopqrstuvwxyz"
                        },
                    new FileVersion
                        {
                            FileName = "\\tradesharp.client.exe",
                            Date = "15.04.2014 13:29:30".ToDateTimeUniform(),
                            Length = 3068736,
                            HashCode = "abcdefghijklmnopqrstuvwxyz"
                        },
                    new FileVersion
                        {
                            FileName = "\\tradesharp.client.exe.config",
                            Date = "25.02.2014 13:54:20".ToDateTimeUniform(),
                            Length = 7770,
                            HashCode = "abcdefghijklmnopqrstuvwxyz"
                        },
                    new FileVersion
                        {
                            FileName = "\\tradesharp.util.dll",
                            Date = "15.04.2014 13:29:37".ToDateTimeUniform(),
                            Length = 524608,
                            HashCode = "abcdefghijklmnopqrstuvwxyz"
                        },
                    new FileVersion
                        {
                            FileName = "\\mt4\\udpcommandtranslator.mq4",
                            Date = "25.11.2013 17:34:52".ToDateTimeUniform(),
                            Length = 9643,
                            HashCode = "abcdefghijklmnopqrstuvwxyz2"
                        },
                    new FileVersion
                        {
                            FileName = "\\mt4\\udpqueuelib.dll",
                            Date = "05.03.2013 11:17:06".ToDateTimeUniform(),
                            Length = 65024,
                            HashCode = "abcdefghijklmnopqrstuvwxyz"
                        },
                    new FileVersion
                        {
                            FileName = "\\sounds\\error.wav",
                            Date = "01.04.2013 12:05:15".ToDateTimeUniform(),
                            Length = 63354,
                            HashCode = "abcdefghijklmnopqrstuvwxyz2"
                        }
                };
        }

        #region генерация C# кода

        /// <summary>
        /// Возвращает в виде строки код C# с инициализацией списка открытых позиций
        /// </summary>
        public static string GetOpenPositionStr(int skipCount, int takeCount = 25, int id = 1)
        {
            var ctx = TradeSharpConnectionPersistent.InitializeTradeSharpConnection();
            var pos = ctx.POSITION.OrderBy(x => x.ID).Skip(skipCount).Take(takeCount).ToList();
            var positionString = new StringBuilder();
            foreach (var po in pos)
            {
                positionString.AppendLine(string.Format("new POSITION \n" +
                                                        "   {{ \n" +
                                                        "       ID = {0}, \n" +
                                                        "       Magic = {1}, \n" +
                                                        "       Comment = {2}, \n" +
                                                        "       AccountID = {3}, \n" +
                                                        "       MasterOrder = {4}, \n" +
                                                        "       ExpertComment = {5}, \n" +
                                                        "       Symbol = {6}, \n" +
                                                        "       Takeprofit = {7}, \n" +
                                                        "       Side = {8}, \n" +
                                                        "       Volume = {9}, \n" +
                                                        "       Stoploss = {10}, \n" +
                                                        "       PriceEnter = {11}, \n" +
                                                        "       TimeEnter = {12}, \n" +
                                                        "       State = {13}, \n" +
                                                        "       PendingOrderID = {14}, \n" +
                                                        "       PriceBest = {15}, \n" +
                                                        "       PriceWorst = {16}, \n" +
                                                        "       TrailLevel1 = {17}, \n" +
                                                        "       TrailLevel2 = {18}, \n" +
                                                        "       TrailLevel3 = {19}, \n" +
                                                        "       TrailLevel4  = {20}, \n" +
                                                        "       TrailTarget1 = {21}, \n" +
                                                        "       TrailTarget2 = {22}, \n" +
                                                        "       TrailTarget3 = {23}, \n" +
                                                        "       TrailTarget4 = {24} \n" +
                                                        "   }},",
                                                        po.ID,
                                                        po.Magic.HasValue ? po.Magic.Value.ToString() : "null",
                                                        string.IsNullOrEmpty(po.Comment)
                                                            ? "\"\""
                                                            : "\"" + po.Comment + "\"",
                                                        po.AccountID,
                                                        po.MasterOrder.HasValue
                                                            ? po.MasterOrder.Value.ToString()
                                                            : "null",
                                                        string.IsNullOrEmpty(po.ExpertComment)
                                                            ? "\"\""
                                                            : "\"" + po.ExpertComment + "\"",
                                                        string.IsNullOrEmpty(po.Symbol)
                                                            ? "\"\""
                                                            : "\"" + po.Symbol + "\"",
                                                        po.Takeprofit.HasValue
                                                            ? "(decimal?)" + po.Takeprofit.Value.ToStringUniform()
                                                            : "null",
                                                        po.Side,
                                                        po.Volume,
                                                        po.Stoploss.HasValue
                                                            ? "(decimal?)" + po.Stoploss.Value.ToStringUniform()
                                                            : "null",
                                                        po.PriceEnter.ToString() + "m",
                                                        "\"" + po.TimeEnter.ToStringUniform() + "\"" +
                                                        ".ToDateTimeUniform()",
                                                        po.State,
                                                        po.PendingOrderID.HasValue
                                                            ? po.PendingOrderID.Value.ToString()
                                                            : "null",
                                                        po.PriceBest.HasValue
                                                            ? "(decimal?)" + po.PriceBest.Value.ToStringUniform() + "m"
                                                            : "null",
                                                        po.PriceWorst.HasValue
                                                            ? po.PriceWorst.Value.ToStringUniform() + "m"
                                                            : "null",
                                                        po.TrailLevel1.HasValue
                                                            ? po.TrailLevel1.Value.ToStringUniform() + "m"
                                                            : "null",
                                                        po.TrailLevel2.HasValue
                                                            ? po.TrailLevel2.Value.ToStringUniform() + "m"
                                                            : "null",
                                                        po.TrailLevel3.HasValue
                                                            ? po.TrailLevel3.Value.ToStringUniform() + "m"
                                                            : "null",
                                                        po.TrailLevel4.HasValue
                                                            ? po.TrailLevel4.Value.ToStringUniform() + "m"
                                                            : "null",
                                                        po.TrailTarget1.HasValue
                                                            ? po.TrailTarget1.Value.ToStringUniform() + "m"
                                                            : "null",
                                                        po.TrailTarget2.HasValue
                                                            ? po.TrailTarget2.Value.ToStringUniform() + "m"
                                                            : "null",
                                                        po.TrailTarget3.HasValue
                                                            ? po.TrailTarget3.Value.ToStringUniform() + "m"
                                                            : "null",
                                                        po.TrailTarget4.HasValue
                                                            ? po.TrailTarget4.Value.ToStringUniform() + "m"
                                                            : "null"));

            }
            ctx.Cleanup();
            return positionString.ToString();
        }

        /// <summary>
        /// Возвращает в виде строки код C# с инициализацией списка закрытых позиций
        /// </summary>
        public static string GetClosePositionStr(int skipCount, int takeCount = 25, int id = 1)
        {
            var ctx = TradeSharpConnectionPersistent.InitializeTradeSharpConnection();
            var pos = ctx.POSITION_CLOSED.OrderBy(x => x.ID).Skip(skipCount).Take(takeCount).ToList();
            var positionString = new StringBuilder();
            foreach (var po in pos)
            {
                positionString.AppendLine(string.Format("new POSITION_CLOSED \n" +
                                                        "   {{ \n" +
                                                        "       ID = {0}, \n" +
                                                        "       Symbol = {1}, \n" +
                                                        "       AccountID = {2}, \n" +
                                                        "       PendingOrderID = {3}, \n" +
                                                        "       Volume = {4}, \n" +
                                                        "       Comment = {5}, \n" +
                                                        "       ExpertComment = {6}, \n" +
                                                        "       Magic = {7}, \n" +
                                                        "       PriceEnter = {8}, \n" +
                                                        "       PriceExit = {9}, \n" +
                                                        "       TimeEnter = {10}, \n" +
                                                        "       TimeExit = {11}, \n" +
                                                        "       Side = {12}, \n" +
                                                        "       Stoploss = {13}, \n" +
                                                        "       Takeprofit = {14}, \n" +
                                                        "       PriceBest = {15}, \n" +
                                                        "       PriceWorst = {16}, \n" +
                                                        "       ExitReason = {17}, \n" +
                                                        "       Swap = {18}, \n" +
                                                        "       ResultPoints = {19}, \n" +
                                                        "       ResultBase = {20}, \n" +
                                                        "       ResultDepo = {21} \n" +
                                                        "   }},",
                                                        po.ID,
                                                        string.IsNullOrEmpty(po.Symbol)
                                                            ? "\"\""
                                                            : "\"" + po.Symbol + "\"",
                                                        po.AccountID,
                                                        po.PendingOrderID.HasValue
                                                            ? po.PendingOrderID.Value.ToString()
                                                            : "null",
                                                        po.Volume,
                                                        string.IsNullOrEmpty(po.Comment)
                                                            ? "\"\""
                                                            : "\"" + po.Comment + "\"",
                                                        string.IsNullOrEmpty(po.ExpertComment)
                                                            ? "\"\""
                                                            : "\"" + po.ExpertComment + "\"",
                                                        po.Magic.HasValue ? po.Magic.Value.ToString() : "null",
                                                        po.PriceEnter.ToStringUniform() + "m",
                                                        po.PriceExit.ToStringUniform() + "m",
                                                        "\"" + po.TimeEnter.ToStringUniform() + "\"" +
                                                        ".ToDateTimeUniform()",
                                                        "\"" + po.TimeExit.ToStringUniform() + "\"" +
                                                        ".ToDateTimeUniform()",
                                                        po.Side,
                                                        po.Stoploss.HasValue
                                                            ? "(decimal?)" + po.Stoploss.Value.ToStringUniform()
                                                            : "null",
                                                        po.Takeprofit.HasValue
                                                            ? "(decimal?)" + po.Takeprofit.Value.ToStringUniform()
                                                            : "null",
                                                        po.PriceBest.HasValue
                                                            ? "(decimal?)" + po.PriceBest.Value.ToStringUniform()
                                                            : "null",
                                                        po.PriceWorst.HasValue
                                                            ? "(decimal?)" + po.PriceWorst.Value.ToStringUniform()
                                                            : "null",
                                                        po.ExitReason,
                                                        "(decimal)" + po.Swap.ToStringUniform(),
                                                        "(decimal)" + po.ResultPoints.ToStringUniform(),
                                                        "(decimal)" + po.ResultBase.ToStringUniform(),
                                                        "(decimal)" + po.ResultDepo.ToStringUniform()));

            }
            ctx.Cleanup();
            return positionString.ToString();

        }

        /// <summary>
        /// Возвращает в виде строки код C# с инициализацией списка изменений баланса
        /// </summary>
        public static string GetBalanceChangesStr(List<POSITION_CLOSED> closePosition, int skipCount, int accountID = 1)
        {
            const int initChange = 5; // количество записей в которых Position = null
            if (initChange >= closePosition.Count)
                throw new Exception(
                    "Количество запрашиваемых элементов не может быть меньше количества элементов, в которых будет Position = null");

            var ctx = TradeSharpConnectionPersistent.InitializeTradeSharpConnection();
            var pos =
                ctx.BALANCE_CHANGE.OrderBy(x => x.ID)
                   .Skip(skipCount)
                   .Take(closePosition.Count)
                   .Select(x => new BalanceChange
                       {
                           ID = x.ID,
                           AccountID = accountID,
                           ChangeType = (BalanceChangeType) x.ChangeType,
                           Amount = x.Amount,
                           Description = x.Description
                       }).ToList();

            for (var i = 0; i < pos.Count; i++)
            {
                pos[i].PositionId = closePosition[i].ID;
                pos[i].ValueDate = closePosition[i].TimeExit;
            }

            var lastId = pos.Last().ID + 1;

            for (var i = 0; i < initChange; i++)
            {
                pos.Add(new BalanceChange
                    {
                        ID = lastId + i,
                        AccountID = 1,
                        ChangeType = (BalanceChangeType) 1,
                        Amount = 20000m,
                        ValueDate = DateTime.Now.AddDays(-i),
                        Description = "Initial depo",
                        PositionId = null
                    });
            }

            var positionString = new StringBuilder();
            foreach (var po in pos)
            {
                positionString.AppendLine(string.Format("new BALANCE_CHANGE \n" +
                                                        "   {{ \n" +
                                                        "       ID = {0}, \n" +
                                                        "       AccountID = {1}, \n" +
                                                        "       ChangeType = {2}, \n" +
                                                        "       Amount = {3}, \n" +
                                                        "       ValueDate = {4}, \n" +
                                                        "       Description = {5}, \n" +
                                                        "       Position = {6}, \n" +
                                                        "   }},",
                                                        po.ID,
                                                        po.AccountID,
                                                        (int) po.ChangeType,
                                                        po.Amount.ToStringUniform() + "m",
                                                        "\"" + po.ValueDate.ToStringUniform() + "\"" +
                                                        ".ToDateTimeUniform()",
                                                        string.IsNullOrEmpty(po.Description)
                                                            ? "\"\""
                                                            : "\"" + po.Description + "\"",
                                                        po.PositionId.HasValue ? po.PositionId.Value.ToString() : "null"
                                              ));

            }
            ctx.Cleanup();
            return positionString.ToString();
        }
        #endregion
    }
}