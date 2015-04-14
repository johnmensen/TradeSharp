using System;
using System.Collections.Specialized;
using TradeSharp.Util.Serialization;

namespace TradeSharp.Contract.Entity
{
    /*      Тест сериализации
     * 
     * 
            SerializationWriter.TypeSurrogates.Add(new MarketOrderSerializer());

            var rnd = new Random();
            var orders = new List<MarketOrder>();
            const int ordersCount = 2000;
            var tickers = DalSpot.Instance.GetTickerNames();

            for (var i = 0; i < ordersCount; i++)
            {
                var order = new MarketOrder
                                {
                                    ID = i,
                                    AccountID = rnd.Next(20),
                                    Volume = (rnd.Next(100) + 1)*10000,
                                    TimeEnter = DateTime.Now.AddMinutes(-rnd.Next(500000)),
                                    Symbol = tickers[rnd.Next(tickers.Length)],
                                    Trailing = "50 5 75 10",
                                    Comment = "test",
                                    ExitReason = PositionExitReason.Stopout
                                };
                order.TimeExit = rnd.Next(5) < 2 ? (DateTime?) null : order.TimeEnter.AddMinutes(rnd.Next(400000));
                orders.Add(order);
            }
            var writer = new SerializationWriter();
            writer.Write(orders);
            writer.Flush();

            var reader = new SerializationReader(writer.ToArray());
            var ordersRead = reader.ReadObjectArray(typeof (MarketOrder)).Cast<MarketOrder>().ToList();

            var countSame = 0;
            for (var i = 0; i < orders.Count; i++)
            {
                if (orders[i].CompareOrders(ordersRead[i])) countSame++;
            }
     * 
     */
    public class MarketOrderSerializer : IFastSerializationTypeSurrogate
    {
        static readonly int magicIsValued = BitVector32.CreateMask();
        static readonly int pendingOrderIdIsValued = BitVector32.CreateMask(magicIsValued);
        static readonly int priceBestIsValued = BitVector32.CreateMask(pendingOrderIdIsValued);
        static readonly int priceExitIsValued = BitVector32.CreateMask(priceBestIsValued);
        static readonly int priceWorstIsValued = BitVector32.CreateMask(priceExitIsValued);
        static readonly int stopLossIsValued = BitVector32.CreateMask(priceWorstIsValued);
        static readonly int swapIsValued = BitVector32.CreateMask(stopLossIsValued);
        static readonly int takeProfitIsValued = BitVector32.CreateMask(swapIsValued);
        static readonly int timeExitIsValued = BitVector32.CreateMask(takeProfitIsValued);
        
        public bool SupportsType(Type type)
        {
            return type == typeof(MarketOrder);
        }

        public void Serialize(SerializationWriter writer, object value)
        {
            var type = value.GetType();

            if (type == typeof(MarketOrder))
            {
                Serialize(writer, (MarketOrder)value);
            }
            else
            {
                throw new InvalidOperationException(string.Format("{0} does not support Type: {1}", GetType(), type));
            }
        }

        public object Deserialize(SerializationReader reader, Type type)
        {
            if (type == typeof(MarketOrder)) return DeserializeOrder(reader);

            throw new InvalidOperationException(string.Format("{0} does not support Type: {1}", GetType(), type));
        }

        public static void Serialize(SerializationWriter writer, MarketOrder deal)
        {
            writer.Write(deal.ID);
            writer.Write(deal.AccountID);
            writer.Write(deal.Comment);
            writer.Write((Int16)(int)deal.ExitReason);
            writer.Write(deal.ExpertComment);
            writer.Write(deal.PriceEnter);
            writer.Write(deal.ResultDepo);
            writer.Write(deal.ResultPoints);
            writer.Write((sbyte)deal.Side);
            writer.Write((Int16)(int)deal.State);
            writer.Write(deal.Symbol);
            writer.Write(deal.TimeEnter);
            writer.Write(deal.Trailing);
            writer.Write(deal.Volume);
            writer.Write(deal.VolumeInDepoCurrency);
            
            // nullable values
            var flags = new BitVector32();

            if (deal.Magic.HasValue)
                flags[magicIsValued] = true;
            if (deal.PendingOrderID.HasValue)
                flags[pendingOrderIdIsValued] = true;
            if (deal.PriceBest.HasValue)
                flags[priceBestIsValued] = true;
            if (deal.PriceExit.HasValue)
                flags[priceExitIsValued] = true;
            if (deal.PriceWorst.HasValue)
                flags[priceWorstIsValued] = true;
            if (deal.StopLoss.HasValue)
                flags[stopLossIsValued] = true;
            if (deal.Swap.HasValue)
                flags[swapIsValued] = true;
            if (deal.TakeProfit.HasValue)
                flags[takeProfitIsValued] = true;
            if (deal.TimeExit.HasValue)
                flags[timeExitIsValued] = true;


            writer.WriteOptimized(flags);

            if (deal.Magic.HasValue)
                writer.Write(deal.Magic.Value);
            if (deal.PendingOrderID.HasValue)
                writer.Write(deal.PendingOrderID.Value);
            if (deal.PriceBest.HasValue)
                writer.Write(deal.PriceBest.Value);
            if (deal.PriceExit.HasValue)
                writer.Write(deal.PriceExit.Value);
            if (deal.PriceWorst.HasValue)
                writer.Write(deal.PriceWorst.Value);
            if (deal.StopLoss.HasValue)
                writer.Write(deal.StopLoss.Value);
            if (deal.Swap.HasValue)
                writer.Write(deal.Swap.Value);
            if (deal.TakeProfit.HasValue)
                writer.Write(deal.TakeProfit.Value);
            if (deal.TimeExit.HasValue)
                writer.Write(deal.TimeExit.Value);
        }

        public static MarketOrder DeserializeOrder(SerializationReader reader)
        {
            var deal = new MarketOrder
            {
                ID = reader.ReadInt32(),
                AccountID = reader.ReadInt32(),
                Comment = reader.ReadString(),
                ExitReason = (PositionExitReason)reader.ReadInt16(),
                ExpertComment = reader.ReadString(),
                PriceEnter = reader.ReadSingle(),
                ResultDepo = reader.ReadSingle(),
                ResultPoints = reader.ReadSingle(),
                Side = reader.ReadSByte(),
                State = (PositionState) reader.ReadInt16(),
                Symbol = reader.ReadString(),
                TimeEnter = reader.ReadDateTime(),
                Trailing = reader.ReadString(),
                Volume = reader.ReadInt32(),
                VolumeInDepoCurrency = reader.ReadSingle()                
            };
         
            // nullable values
            var flags = reader.ReadOptimizedBitVector32();
            
            if (flags[magicIsValued])
                deal.Magic = reader.ReadInt32();
            
            if (flags[pendingOrderIdIsValued])
                deal.PendingOrderID = reader.ReadInt32();
            
            if (flags[priceBestIsValued])
                deal.PriceBest = reader.ReadSingle();
            
            if (flags[priceExitIsValued])
                deal.PriceExit = reader.ReadSingle();
            
            if (flags[priceWorstIsValued])
                deal.PriceWorst = reader.ReadSingle();
            
            if (flags[stopLossIsValued])
                deal.StopLoss = reader.ReadSingle();
            
            if (flags[swapIsValued])
                deal.Swap = reader.ReadSingle();
            
            if (flags[takeProfitIsValued])
                deal.TakeProfit = reader.ReadSingle();
            
            if (flags[timeExitIsValued])
                deal.TimeExit = reader.ReadDateTime();
            
            return deal;
        }
    }
}
