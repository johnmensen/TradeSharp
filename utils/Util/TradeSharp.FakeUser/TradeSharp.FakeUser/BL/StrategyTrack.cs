using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TradeSharp.FakeUser.BL
{
    public class DecimalFormatJsonConverter : JsonConverter
    {
        private readonly int _numberOfDecimals;

        public DecimalFormatJsonConverter(int numberOfDecimals)
        {
            _numberOfDecimals = numberOfDecimals;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var d = (decimal) value;
            var rounded = Math.Round(d, _numberOfDecimals);
            writer.WriteValue((decimal)rounded);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(decimal);
        }
    }
    
    public class TwoDigitsDecimalFormatConverter : DecimalFormatJsonConverter
    {
        public TwoDigitsDecimalFormatConverter() : base(2)
        {
        }
    }

    class StrategyTrackRecord
    {
        [JsonProperty(PropertyName = "equity", Order = 10)]
        [JsonConverter(typeof(TwoDigitsDecimalFormatConverter))]
        public decimal Equity { get; set; }

        [JsonProperty(PropertyName = "balance", Order = 20)]
        [JsonConverter(typeof(TwoDigitsDecimalFormatConverter))]
        public decimal Balance { get; set; }

        [JsonProperty(PropertyName = "volume_open_trades", Order = 30)]
        public int OpenedVolumes { get; set; }

        [JsonProperty(PropertyName = "profit_float", Order = 40)]
        [JsonConverter(typeof(TwoDigitsDecimalFormatConverter))]
        public decimal PeriodOpenProfit { get; set; }

        [JsonProperty(PropertyName = "profit_closed", Order = 50)]
        [JsonConverter(typeof(TwoDigitsDecimalFormatConverter))]
        public decimal PeriodClosedProfit { get; set; }

        [JsonProperty(PropertyName = "deposit", Order = 60)]
        [JsonConverter(typeof(TwoDigitsDecimalFormatConverter))]
        public decimal DepositWithdraw { get; set; }
    }

    class RecordOnDate
    {
        public DateTime Date { get; set; }

        public StrategyTrackRecord Record { get; set; }
    }

    class Strategy
    {
        public readonly List<RecordOnDate> records = new List<RecordOnDate>();

        public Strategy(DateTime time, int initialDepo)
        {
            records.Add(new RecordOnDate
            {
                Date = time,
                Record = new StrategyTrackRecord
                {
                    Balance = initialDepo,
                    Equity = initialDepo
                }
            });
        }

        public void AddRecord(DateTime time, 
            decimal balance, decimal equity, decimal volumes,
            decimal sumBalanceChanges, decimal closedProfit)
        {
            var record = new StrategyTrackRecord
            {
                Balance = balance,
                Equity = equity,
                OpenedVolumes = (int)volumes,
                PeriodClosedProfit = closedProfit,
                PeriodOpenProfit = equity - balance,
                DepositWithdraw = sumBalanceChanges
            };
            records.Add(new RecordOnDate
            {
                Date = time,
                Record = record
            });
        }

        public void Serialize(StreamWriter sw)
        {
            sw.Write("{");
            var first = true;
            foreach (var record in records)
            {
                if (!first) 
                    sw.Write("," + Environment.NewLine);
                first = false;
                sw.Write("\"{0:dd.MM.yyyy HH:mm:ss}\": {1}",
                    record.Date, JsonConvert.SerializeObject(record.Record));
            }
            sw.Write("}");
        }
    }
}
