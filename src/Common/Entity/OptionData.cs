using System;
using System.Collections.Generic;
using System.Linq;
using TradeSharp.Util;

namespace Entity
{
    public enum OptionType { CALL = 1, PUT = -1 }
    public enum OptionStyle { American = 0, European }
    public enum OptionLevelType
    {
        Close = 0,
        High,
        Low
    }

    [Serializable]
    public class OptionData
    {
        public OptionType Type { get; set; } // put, call, 
        public OptionStyle Style { get; set; } // american, european
        public String BaseActive { get; set; }
        public DateTime DatePublished { get; set; }
        public DateTime DateExpiration { get; set; }
        public float StrikePrice { get; set; }
        public float ContractHigh { get; set; }
        public float ContractLow { get; set; }
        public float ContractClose { get; set; }
        public int OpenInterest { get; set; }
        public int Volume { get; set; }

        public float OptionLevelHigh
        {
            get { return Type == OptionType.CALL ? StrikePrice + ContractHigh : StrikePrice - ContractHigh; }
        }
        public float OptionLevelLow
        {
            get { return Type == OptionType.CALL ? StrikePrice + ContractLow : StrikePrice - ContractLow; }
        }
        public float OptionLevelClose
        {
            get { return Type == OptionType.CALL ? StrikePrice + ContractClose : StrikePrice - ContractClose; }
        }                

        private const string PartsSeparator = "#&";

        public OptionData() { }
        public OptionData(OptionType optType, OptionStyle optStyle, String baseActive, 
            DateTime countDate, DateTime dateExpiration,
            float strikePrice,
            float contractHigh, float contractLow, float contractClose,
            int openInterest, int volume)
        {
            Type = optType;
            Style = optStyle;
            BaseActive = baseActive;
            DatePublished = countDate;
            DateExpiration = dateExpiration;
            StrikePrice = strikePrice;
            ContractHigh = contractHigh;
            ContractLow = contractLow;
            ContractClose = contractClose;
            OpenInterest = openInterest;
            Volume = volume;
        }

        public OptionData(OptionData op)
        {
            Type = op.Type;
            Style = op.Style;
            BaseActive = op.BaseActive;
            DatePublished = op.DatePublished;
            DateExpiration = op.DateExpiration;
            StrikePrice = op.StrikePrice;
            ContractHigh = op.ContractHigh;
            ContractLow = op.ContractLow;
            ContractClose = op.ContractClose;
            OpenInterest = op.OpenInterest;
            Volume = op.Volume;
        }

        public float GetOptionLevel(OptionLevelType levelType)
        {
            return levelType == OptionLevelType.Close
                       ? OptionLevelClose
                       : levelType == OptionLevelType.High ? OptionLevelHigh : OptionLevelLow;
        }

        public override int GetHashCode()
        {
            return (int)(StrikePrice + (ContractClose * 10000));
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is OptionData == false) return false;
            var op = (OptionData)obj;
            return Equals(op);
        }

        public bool Equals(OptionData op)
        {
            if (op == null) return false;
            return Type == op.Type && Style == op.Style
                && BaseActive == op.BaseActive
                && DatePublished == op.DatePublished
                && DateExpiration == op.DateExpiration
                && StrikePrice == op.StrikePrice
                && ContractHigh == op.ContractHigh
                && ContractLow == op.ContractLow
                && ContractClose == op.ContractClose
                && OpenInterest == op.OpenInterest
                && Volume == op.Volume;
        }

        /// <summary>
        /// Формат строки "[#fmt]#;newstype=option#;type=val#;style=val#;baseactive=val#;countdate=val#;
        /// dateexpiration=val#;strikeprice=val#;delta=val#;openinterest=val"
        /// </summary>
        /// <param name="op"></param>
        public static OptionData Parse(String op)
        {
            var od = new OptionData();
            try
            {
                var separator = new[] { PartsSeparator };
                var str = op.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                if (str.Length != 13) return null;
                if (str[0] != "[#fmt]") return null;
                
                // строка отформатирована, проверим, что это опцион
                var members = new Dictionary<string, string>();
                var sep = new [] { '=' };
                foreach (var a in str)
                {
                    var mem = a.Split(sep);
                    if (mem.Count() == 1)
                        continue;
                    members.Add(mem[0], mem[1]);
                }
                if (members["newstype"] == null || members["newstype"] != "option")
                    throw new Exception("OptionData.Parse: тип новости отличается от опциона");

                var typeStr = members["type"];
                if (!string.IsNullOrEmpty(typeStr))
                    od.Type = (OptionType)Enum.Parse(typeof(OptionType), typeStr);
                var styleStr = members["style"];
                if (!string.IsNullOrEmpty(styleStr))
                    od.Style = (OptionStyle)Enum.Parse(typeof(OptionStyle), styleStr);
                

                od.BaseActive = members["baseactive"];
                od.DatePublished = members["publishdate"].ToDateTimeUniform();
                od.DateExpiration = DateTime.ParseExact(members["dateexpiration"],
                    "dd.MM.yyyy", CultureProvider.Common);                    
                od.StrikePrice = members["strikeprice"].ToFloatUniform();
                od.ContractHigh = members["high"].ToFloatUniform();
                od.ContractLow = members["low"].ToFloatUniform();
                od.ContractClose = members["close"].ToFloatUniform();
                od.OpenInterest = members["oi"].ToInt();
                od.Volume = members["volume"].ToInt();
            }
            catch (InvalidCastException ex)
            {
                Logger.Error("OptionData.Parse ошибка ", ex);
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error("OptionData.Parse ошибка ", ex);
                return null;
            }
            return od;
        }

        /// <summary>
        /// Формат строки "[#fmt]#;newstype=option#;type=val#;style=val#;baseactive=val#;countdate=val#;
        /// dateexpiration=val#;strikeprice=val#;delta=val#;openinterest=val"
        /// </summary>
        /// <returns></returns>
        public override String ToString()
        {
            var opData = new string[12];
            opData[0] = "[#fmt]";
            opData[1] = "newstype=option";
            opData[2] = "type=" + Type;
            opData[3] = "style=" + Style;
            opData[4] = "baseactive=" + BaseActive;
            opData[5] = "publishdate=" + DatePublished.ToStringUniform();
            opData[6] = "dateexpiration=" + DateExpiration.ToStringUniform();
            opData[7] = "strikeprice=" + StrikePrice.ToStringUniform(5);
            opData[8] = "high=" + ContractHigh.ToStringUniform(5);
            opData[9] = "low=" + ContractLow.ToStringUniform(5);
            opData[10] = "close=" + ContractClose.ToStringUniform(5);            
            opData[11] = "oi=" + OpenInterest;
            opData[11] = "volume=" + Volume;
            return String.Join(PartsSeparator, opData);
        }
    }
}