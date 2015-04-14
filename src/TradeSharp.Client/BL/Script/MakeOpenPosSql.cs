using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Candlechart;
using Candlechart.ChartMath;
using Candlechart.Series;
using Entity;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Client.BL.Script
{
    [DisplayName("SQL откр. поз.")]
    public class MakeOpenPosSql : TerminalScript
    {
        #region Параметры

        private int volumeMin = 10000;
        [DisplayName("Объем мин.")]
        [Category("Основные")]
        [Description("Минимальный объем сделки")]
        [PropertyXMLTag("VolumeMin")]
        public int VolumeMin
        {
            get { return volumeMin; }
            set { volumeMin = value; }
        }

        private int volumeMax = 10000;
        [DisplayName("Объем макс.")]
        [Category("Основные")]
        [Description("Максимальный объем сделки")]
        [PropertyXMLTag("VolumeMax")]
        public int VolumeMax
        {
            get { return volumeMax; }
            set { volumeMax = value; }
        }

        private int volumeStep = 10000;
        [DisplayName("Шаг объема")]
        [Category("Основные")]
        [Description("Шаг объема сделки")]
        [PropertyXMLTag("VolumeStep")]
        public int VolumeStep
        {
            get { return volumeStep; }
            set { volumeStep = value; }
        }
        
        #endregion

        public MakeOpenPosSql()
        {
            ScriptTarget = TerminalScriptTarget.График;
            ScriptName = "SQL откр. поз.";
        }

        public override string ActivateScript(CandleChartControl chart, PointD worldCoords)
        {
            if (!AccountStatus.Instance.isAuthorized)
            {
                MessageBox.Show("Не авторизован");
                return "Не авторизован";
            }

            var accountId = AccountStatus.Instance.accountID;
            var numStepsMax = (VolumeMax - VolumeMin) / VolumeStep;
            var rand = new Random();
            var sb = new StringBuilder();

            // закрытые сделки
            var closedOrders = MakeClosedOrders(chart, accountId, rand, numStepsMax);

            // SQL по закрытым сделкам
            MakeClosedOrdersSQL(closedOrders, accountId, sb);

            // найти все отметки на графике
            // и по ним сбацать "сделки"
            var positions = MakeMarketOrders(chart, accountId, rand, numStepsMax);

            // слепить SQL по открытым позам
            MakeMarketOrdersSQL(positions, accountId, sb);

            if (sb.Length > 0)
                Clipboard.SetText(sb.ToString());
            return (positions.Count + closedOrders.Count) + " сделок скопировано в буфер обмена";
        }

        private static void MakeMarketOrdersSQL(List<MarketOrder> positions, int accountId, StringBuilder sb)
        {
            sb.AppendLine();
            sb.AppendLine("-- Открытые позиции --");
            foreach (var pos in positions)
            {
                sb.AppendLine(string.Format(
                    "insert into POSITION ([State], Symbol, AccountID, Volume, PriceEnter, TimeEnter, Side) values (" +
                    "{0}, '{1}', {2}, {3}, {4}, '{5:yyyyMMdd HH:mm:ss}', {6})",
                    (int) pos.State,
                    pos.Symbol,
                    accountId,
                    pos.Volume,
                    pos.PriceEnter.ToStringUniformPriceFormat(),
                    pos.TimeEnter,
                    pos.Side));
            }
        }

        private List<MarketOrder> MakeMarketOrders(CandleChartControl chart, int accountId, Random rand, int numStepsMax)
        {
            var positions = new List<MarketOrder>();

            foreach (var mark in chart.seriesAsteriks.data)
            {
                var side = mark.Shape == AsteriskTooltip.ShapeType.СтрелкаВниз
                               ? -1
                               : mark.Shape == AsteriskTooltip.ShapeType.СтрелкаВверх ? 1 : 0;
                if (side == 0) continue;
                var pos = new MarketOrder
                              {
                                  AccountID = accountId,
                                  Side = side,
                                  TimeEnter = mark.DateStart.Value,
                                  State = PositionState.Opened,
                                  PriceEnter = mark.Price,
                                  Symbol = chart.Symbol,
                                  Volume = VolumeMin
                              };
                // посчитать объем
                if (numStepsMax > 0)
                    pos.Volume += rand.Next(numStepsMax + 1)*VolumeStep;
                positions.Add(pos);
            }
            return positions;
        }

        private List<MarketOrder> MakeClosedOrders(CandleChartControl chart, int accountId, Random rand, int numStepsMax)
        {
            var positions = new List<MarketOrder>();

            foreach (var mark in chart.seriesMarker.data)
            {
                var side = (int)mark.Side;
                if (mark.exitPair == null) continue;

                var pos = new MarketOrder
                {
                    AccountID = accountId,
                    Side = side,
                    TimeEnter = mark.DateStart.Value,
                    State = PositionState.Closed,
                    ExitReason = PositionExitReason.Closed,
                    PriceEnter = (float)mark.Price,
                    Symbol = chart.Symbol,
                    Volume = VolumeMin
                };

                // посчитать объем
                if (numStepsMax > 0)
                    pos.Volume += rand.Next(numStepsMax + 1) * VolumeStep;

                // найти отметку выхода
                var exitMarkId = mark.exitPair.Value;
                var exitMarker = chart.seriesMarker.data.First(m => m.id == exitMarkId);

                pos.PriceExit = (float) exitMarker.Price - DalSpot.Instance.GetDefaultSpread(pos.Symbol) * pos.Side;
                pos.TimeExit = exitMarker.DateStart.Value;

                // посчитать результат в пунктах, валюте депозита и тд.
                var profitCounter = pos.Side * (pos.PriceExit.Value - pos.PriceEnter) * pos.Volume;
                var profitDepo = profitCounter;
                if (pos.Symbol.StartsWith("USD")) profitDepo = profitCounter / pos.PriceExit.Value;
                var profitPoints = DalSpot.Instance.GetPointsValue(pos.Symbol, pos.Side*(pos.PriceExit.Value - pos.PriceEnter));

                pos.ResultDepo = profitDepo;
                pos.ResultPoints = profitPoints;
                pos.ResultDepo = profitDepo;
                pos.ResultBase = profitCounter;
                
                positions.Add(pos);
            }
            return positions;
        }

        private static void MakeClosedOrdersSQL(List<MarketOrder> positions, int accountId, StringBuilder sb)
        {
            float totalResult = 0;

            sb.AppendLine("-- Закрытые позиции --");
            foreach (var pos in positions)
            {
                totalResult += pos.ResultDepo;

                // добавить позу
                sb.AppendLine(string.Format(
                    "insert into POSITION ([State], Symbol, AccountID, Volume, PriceEnter, TimeEnter, Side) values (" +
                    "{0}, '{1}', {2}, {3}, {4}, '{5:yyyyMMdd HH:mm:ss}', {6})",
                    (int)PositionState.Opened,
                    pos.Symbol,
                    accountId,
                    pos.Volume,
                    pos.PriceEnter.ToStringUniformPriceFormat(),
                    pos.TimeEnter,
                    pos.Side));

                var selectIdStr = string.Format("(SELECT ID FROM POSITION WHERE AccountID = {0} AND Symbol = '{1}' " +
                                                "AND Side = {2} AND TimeEnter = '{3:yyyyMMdd HH:mm:ss}')",
                                                accountId, pos.Symbol, pos.Side, pos.TimeEnter);

                // добавить закрытую позу
                sb.AppendLine(string.Format(
                   "insert into POSITION_CLOSED (Symbol, AccountID, Volume, PriceEnter, TimeEnter, Side, ID, TimeExit, PriceExit, ExitReason, Swap, ResultPoints, ResultBase, ResultDepo) values (" +
                   "'{0}', {1}, {2}, {3}, '{4:yyyyMMdd HH:mm:ss}', {5}, {6}, '{7:yyyyMMdd HH:mm:ss}', {8}, {9}, 0, {10}, {11}, {12})",
                   pos.Symbol,
                   accountId,
                   pos.Volume,
                   pos.PriceEnter.ToStringUniformPriceFormat(),
                   pos.TimeEnter,
                   pos.Side,
                   selectIdStr, // ID закрываемой позы
                   pos.TimeExit,
                   pos.PriceExit.Value.ToStringUniformPriceFormat(),
                   (int)pos.ExitReason,
                   pos.ResultPoints.ToStringUniformPriceFormat(),
                   pos.ResultBase.ToStringUniformPriceFormat(),
                   pos.ResultDepo.ToStringUniformPriceFormat()
                   ));

                // удалить открытую позу
                sb.AppendLine(string.Format("DELETE FROM POSITION WHERE AccountID = {0} AND Symbol = '{1}' " +
                                            "AND Side = {2} AND TimeEnter = '{3:yyyyMMdd HH:mm:ss}'",
                                            accountId, pos.Symbol, pos.Side, pos.TimeEnter));

                // добавить изменение баланса
                sb.AppendLine(string.Format(
                    "INSERT INTO BALANCE_CHANGE(AccountID, Amount, ValueDate, Description, ChangeType) " + 
                    "VALUES({0}, {1}, '{2:yyyyMMdd HH:mm:ss}', '#{3}', {4})",
                    accountId, 
                    Math.Abs(pos.ResultDepo).ToStringUniform(),
                    pos.TimeExit,
                    pos.ID,
                    pos.ResultDepo > 0 ? (int)BalanceChangeType.Profit : (int)BalanceChangeType.Loss
                    ));
                sb.AppendLine();
            }

            // коррекция баланса по счету
            sb.AppendLine("-- коррекция баланса по счету --");
            sb.AppendLine(string.Format("UPDATE ACCOUNT SET Balance = (SELECT Balance FROM ACCOUNT WHERE ID = {0}) + {1} WHERE ID = {0}",
                                        accountId, totalResult.ToStringUniform()));
        }

        public override string ActivateScript(bool byTrigger)
        {
            throw new Exception("Неверный тип вызова скрипта \"MakeOpenPosSql\"");
        }        

        public override string ActivateScript(string ticker)
        {
            throw new Exception("Неверный тип вызова скрипта \"MakeOpenPosSql\"");            
        }        
    }
}
