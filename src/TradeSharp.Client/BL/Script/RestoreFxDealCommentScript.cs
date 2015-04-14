using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Candlechart;
using Candlechart.ChartMath;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Robot.Robot;

namespace TradeSharp.Client.BL.Script
{
    [DisplayName("Восстановить сделки FX")]
    public class RestoreFxDealCommentScript : TerminalScript
    {
        public RestoreFxDealCommentScript()
        {
            ScriptTarget = TerminalScriptTarget.Терминал;
            ScriptName = "Восстановить сделки FX";
        }

        public override string ActivateScript(CandleChartControl chart, PointD worldCoords)
        {
            return ActivateScript(false);
        }

        public override string ActivateScript(string ticker)
        {
            return ActivateScript(false);
        }

        public override string ActivateScript(bool byTrigger)
        {
            // получить всех роботов
            var robots = MainForm.Instance.RobotFarm.GetRobotCopies();
            // и все ордера
            var orders = MarketOrdersStorage.Instance.MarketOrders;
            var orderWithComment = new List<MarketOrder>();

            foreach (FiboLevelRobot robot in robots.Where(r => r is FiboLevelRobot))
            {
                if (robot.Graphics.Count == 0 || robot.Magic == 0
                    || robot.PriceA == 0 || robot.PriceB == 0) continue;

                // найти уровни A - B, сделка должна преодолеть "расширение"
                var level = robot.PriceB + (robot.PriceA - robot.PriceB) * robot.KoefEnter;
                var side = robot.PriceA > robot.PriceB ? -1 : 1;
                var ticker = robot.Graphics[0].a;
                var magic = robot.Magic;

                // отобрать сделки
                var deals = orders.Where(o => o.Symbol == ticker && o.Side == side && o.Magic == magic);
                foreach (var deal in deals)
                {
                    // установить новый комментарий
                    var comment = DalSpot.Instance.FormatPrice(ticker, (float)robot.PriceA, true) + ";" +
                        DalSpot.Instance.FormatPrice(ticker, (float)robot.PriceB, true);
                    deal.Comment = comment;
                    orderWithComment.Add(deal);
                }
            }

            if (orderWithComment.Count == 0) return string.Empty;

            // установить комментарии по подтверждению
            var prompt = string.Format("Установить для {0} ордеров комментарии вида \"{1}\"?",
                                       orderWithComment.Count, orderWithComment[0].Comment);
            if (MessageBox.Show(prompt, "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) !=
                DialogResult.Yes) return string.Empty;

            foreach (var order in orderWithComment)
                MainForm.Instance.SendEditMarketRequestSafe(order);

            return orderWithComment.Count.ToString() + " ордеров обновлено";
        }
    }
}