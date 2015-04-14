using System.Collections.Generic;
using System.Text;

namespace TradeSharp.Util
{
    public class TemplateParameter
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public bool IsNumber { get; set; }
        public bool IsString { get; set; }
        public bool IsBoolean { get; set; }

        public string ValueStr { get; set; }
        public double ValueNum { get; set; }
        public bool ValueBit { get; set; }

        public TemplateParameter(string name, string title, bool isNumber, bool isString, bool isBoolean)
        {
            Name = name;
            Title = title;
            IsNumber = isNumber;
            IsString = isString;
            IsBoolean = isBoolean;
        }
    }

    public abstract class BaseCodeTemplate
    {
        public abstract string Title { get; }
        public Dictionary<string, TemplateParameter> parameters;

        public abstract string MakeFormula();

        public override string ToString()
        {
            return Title;
        }

        public virtual string MakeTitleByParams()
        {
            return Title;
        }
    }

    public class CodeTemplateIndexSumDelta : BaseCodeTemplate
    {
        public override string Title { get { return "Бивалютный индекс"; } }

        public CodeTemplateIndexSumDelta()
        {
            parameters = new Dictionary<string, TemplateParameter>
                             {{"period", new TemplateParameter("period", "Период индекса", true, false, false)},
                              {"pair", new TemplateParameter("pair", "Валютная пара", false, true, false)},
                              {"type", new TemplateParameter("type", "Вид индекса (1 - сумма приращений, 2 - произведение)", 
                                  true, false, false)}};
        }

        public override string MakeFormula()
        {
            var period = parameters["period"].ValueNum;
            if (period < 1) return "Ошибка задания параметров: период должен быть > 0";
            var pair = parameters["pair"].ValueStr;
            if (string.IsNullOrEmpty(pair)) return "Ошибка задания параметров: не определена валютная пара";

            pair = pair.ToLower();
            var typeIndex = (int)parameters["type"].ValueNum;
            if (typeIndex < 1 || typeIndex > 2)
                return "Ошибка задания параметров: вид индекса должен быть 1 или 2";

            // расписать формулу
            var formulaPlaceholder = "";
            if (pair == "eurusd" && typeIndex == 1)
                formulaPlaceholder = "100*((eurusd-eurusd#i)/eurusd#i + (eurjpy-eurjpy#i)/eurjpy#i + " +
                                     "(eurchf-eurchf#i)/eurchf#i) - 100*((usdchf-usdchf#i)/usdchf#i + (usdjpy-usdjpy#i)/usdjpy#i - " +
                                     "(eurusd-eurusd#i)/eurusd#i - (gbpusd-gbpusd#i)/gbpusd#i)";
            if (pair == "eurusd" && typeIndex == 2)
                formulaPlaceholder =
                    "(i<@period)??(1+" +
                    "eurusd^0.6504*eurgbp^0.3056*eurjpy^0.1891*eursek^0.0785*eurchf^0.1113/(usdjpy^0.1652*gbpusd^-0.082*usdchf^0.027*usdsek^0.0189)-" +
                    "eurusd#i^0.6504*eurgbp#i^0.3056*eurjpy#i^0.1891*eursek#i^0.0785*eurchf#i^0.1113/(usdjpy#i^0.1652*gbpusd#i^-0.082*usdchf#i^0.027*usdsek#i^0.0189))-1";
            
            if (pair == "gbpusd" && typeIndex == 1)
                formulaPlaceholder = "100*((gbpjpy-gbpjpy#i)/gbpjpy#i + " +
                                     "(gbpchf-gbpchf#i)/gbpchf#i) - (eurgbp-eurgbp#i)/eurgbp#i - " +
                                     "100*((usdchf-usdchf#i)/usdchf#i + (usdjpy-usdjpy#i)/usdjpy#i - " +
                                     "(eurusd-eurusd#i)/eurusd#i)";
            if (pair == "gbpusd" && typeIndex == 2)
                formulaPlaceholder =
                    "(i<@period)??(1+" +
                    "gbpusd^0.249*eurgbp^-0.743*gbpjpy^0.043*gbpcad^0.024*gbpchf^0.023/(eurusd^-0.3349*usdjpy^0.1652*usdchf^0.027*usdsek^0.0189)-" +
                    "gbpusd#i^0.249*eurgbp#i^-0.743*gbpjpy#i^0.043*gbpcad#i^0.024*gbpchf#i^0.023/(eurusd#i^-0.3349*usdjpy#i^0.1652*usdchf#i^0.027*usdsek#i^0.0189))-1";

            if (pair == "usdjpy" && typeIndex == 1)
                formulaPlaceholder = "100*((usdchf-usdchf#i)/usdchf#i - " +
                                     "(eurusd-eurusd#i)/eurusd#i - (gbpusd-gbpusd#i)/gbpusd#i) + 100*" +
                                     "((eurjpy-eurjpy#i)/eurjpy#i + (gbpjpy-gbpjpy#i)/gbpjpy#i + " +
                                     "(chfjpy-chfjpy#i)/chfjpy#i)";
            if (pair == "usdjpy" && typeIndex == 2)
                formulaPlaceholder =
                    "(i<@period)??(1+" +
                    "eurusd^-0.3349*usdjpy^0.7432*gbpusd^-0.082*usdchf^0.027*usdsek^0.0189/(eurjpy^-0.264*gbpjpy^-0.06^audjpy^-0.055*cadjpy^-0.043)-" +
                    "eurusd#i^-0.3349*usdjpy#i^0.7432*gbpusd#i^-0.082*usdchf#i^0.027*usdsek#i^0.0189/(eurjpy#i^-0.264*gbpjpy#i^-0.06^audjpy#i^-0.055*cadjpy#i^-0.043))-1";

            if (pair == "usdchf" && typeIndex == 1)
                formulaPlaceholder = "100*((usdjpy-usdjpy#i)/usdjpy#i - " +
                                     "(eurusd-eurusd#i)/eurusd#i - (gbpusd-gbpusd#i)/gbpusd#i) - 100*" +
                                     "((chfjpy-chfjpy#i)/chfjpy#i - (gbpchf-gbpchf#i)/gbpchf#i - " +
                                     "(eurchf-eurchf#i)/eurchf#i)";
            if (pair == "usdchf" && typeIndex == 2)
                formulaPlaceholder =
                    "(i<@period)??(1+" +
                    "eurusd^-0.3349*usdjpy^0.1652*gbpusd^-0.082*usdchf^0.142*usdsek^0.0189/(eurchf^-0.69*gbpchf^-0.195)-" +
                    "eurusd#i^-0.3349*usdjpy#i^0.1652*gbpusd#i^-0.082*usdchf#i^0.142*usdsek#i^0.0189/(eurchf#i^-0.69*gbpchf#i^-0.195))-1";

            if (pair == "audusd" && typeIndex == 1)
                formulaPlaceholder = "100*((audcad-audcad#i)/audcad#i??1 + " +
                    "(audchf-audchf#i)/audchf#i??1 + (audeur-audeur#i)/audeur#i??1 + " +
                    "(audjpy-audjpy#i)/audjpy#i??1 - (gbpaud-gbpaud#i)/gbpaud#i??1 + " +
                    "(audnzd-audnzd#i)/audnzd#i??1) - 100*((usdchf-usdchf#i)/usdchf#i??1 + " +
                    "(usdjpy-usdjpy#i)/usdjpy#i??1 - (eurusd-eurusd#i)/eurusd#i??1)";            
            if (pair == "audusd" && typeIndex == 2)
                formulaPlaceholder = "audcad*audchf*audeur*audjpy*audnzd*audusd*gbpusd*eurusd/" +
                    "(gbpaud*usdchf*usdjpy)??1 - " +
                    "audcad#i*audchf#i*audeur#i*audjpy#i*audnzd#i*audusd#i*gbpusd#i*eurusd#i/" +
                    "(gbpaud#i*usdchf#i*usdjpy#i)??1";

            if (pair == "usdcad" && typeIndex == 1)
                formulaPlaceholder = "100*((usdchf-usdchf#i)/usdchf#i??1 + " +
                                     "(usdjpy-usdjpy#i)/usdjpy#i??1 - (eurusd-eurusd#i)/eurusd#i??1) - " +
                                     "100*((cadchf-cadchf#i)/cadchf#i??1 + (cadjpy-cadjpy#i)/cadjpy#i??1 - " +
                                     "(gbpcad-gbpcad#i)/gbpcad#i??1 - (usdcad-usdcad#i)/usdcad#i??1 - " +
                                     "(eurcad-eurcad#i)/eurcad#i??1)";
            // USD eurusd^-0.3349*usdjpy^0.1652*gbpusd^-0.082*usdcad^0.2887*usdchf^0.027
            // CAD usdcad^-0.7618*eurcad^-0.0931*(cadjpy??1)^-0.0527*(gbpcad??1)^-0.0271
            // eurusd^-0.3349*usdjpy^0.1652*gbpusd^-0.082*usdcad^1.0505*usdchf^0.027*eurcad^0.0931*(cadjpy??1)^-0.0527*(gbpcad??1)^0.0271
            if (pair == "usdcad" && typeIndex == 2)
                formulaPlaceholder =
                    "(usdcad*gbpcad*nzdcad*usdcad*eurcad*usdchf)/(cadchf??1*cadjpy??1*audusd??1*eurusd??1*gbpusd??1) - " +
                    "(usdcad#i*gbpcad#i*nzdcad#i*usdcad#i*eurcad#i*usdchf#i)/(cadchf#i??1*cadjpy#i??1*audusd#i??1*eurusd#i??1*gbpusd#i??1)";
            
            if (pair == "nzdusd" && typeIndex == 1)
                formulaPlaceholder = "100*((nzdchf-nzdchf#i)/nzdchf#i??1 + (nzdgbp-nzdgbp#i)/nzdgbp#i??1 + " +
                                     "(nzdjpy-nzdjpy#i)/nzdjpy#i??1 + (nzdusd-nzdusd#i)/nzdusd#i??1) -  100*((usdchf-usdchf#i)/usdchf#i??1 + " +
                                     "(usdjpy-usdjpy#i)/usdjpy#i??1 - (eurusd-eurusd#i)/eurusd#i??1)";
            if (pair == "nzdusd" && typeIndex == 2)
                formulaPlaceholder = "nzdchf*nzdgbp*nzdjpy*nzdusd*gbpusd*eurusd/(usdchf??1*usdjpy??1) - " +
                    "nzdchf#i*nzdgbp#i*nzdjpy#i*nzdusd#i*gbpusd#i*eurusd#i/(usdchf#i??1*usdjpy#i??1)";

            if (pair == "eurjpy" && typeIndex == 1)
                formulaPlaceholder = "100*((eurusd-eurusd#i)/eurusd#i + (eurjpy-eurjpy#i)/eurjpy#i + " +
                                     "(eurchf-eurchf#i)/eurchf#i) + 100*" +
                                     "((usdjpy-usdjpy#i)/usdjpy#i + (eurjpy-eurjpy#i)/eurjpy#i + (gbpjpy-gbpjpy#i)/gbpjpy#i + " +
                                     "(chfjpy-chfjpy#i)/chfjpy#i)";
            if (pair == "eurjpy" && typeIndex == 2)
                formulaPlaceholder = "(i<@period)??(1+" +
                    "eurusd^0.6504*eurgbp^0.3056*eurjpy^0.1891*eursek^0.0785*eurchf^0.1113/(usdjpy^-0.258*eurjpy^-0.124*gbpjpy^-0.029*audjpy^-0.021)-" +
                    "eurusd#i^0.6504*eurgbp#i^0.3056*eurjpy#i^0.1891*eursek#i^0.0785*eurchf#i^0.1113/(usdjpy#i^-0.258*eurjpy#i^-0.124*gbpjpy#i^-0.029*audjpy#i^-0.021))-1";

            if (pair == "eurchf" && typeIndex == 1)
                formulaPlaceholder = "100*((eurusd-eurusd#i)/eurusd#i + (eurjpy-eurjpy#i)/eurjpy#i + " +
                                     "(eurchf-eurchf#i)/eurchf#i) - 100*" +
                                     "((chfjpy-chfjpy#i)/chfjpy#i - (gbpchf-gbpchf#i)/gbpchf#i - " +
                                     "(eurchf-eurchf#i)/eurchf#i)";
            if (pair == "eurchf" && typeIndex == 2)
                formulaPlaceholder = "(i<@period)??(1+" +
                                     "eurusd^0.6504*eurgbp^0.3056*eurjpy^0.1891*eursek^0.0785*eurchf^0.1113/(usdchf^-0.115*eurchf^-0.69*gbpchf^-0.195)-" +
                                     "eurusd#i^0.6504*eurgbp#i^0.3056*eurjpy#i^0.1891*eursek#i^0.0785*eurchf#i^0.1113/(usdchf#i^-0.115*eurchf#i^-0.69*gbpchf#i^-0.195))-1";

            if (pair == "eurgbp" && typeIndex == 1)
                formulaPlaceholder = "100*((eurusd-eurusd#i)/eurusd#i + (eurjpy-eurjpy#i)/eurjpy#i + " +
                                     "(eurchf-eurchf#i)/eurchf#i) - 100*" +
                                     "((gbpjpy-gbpjpy#i)/gbpjpy#i + ((gbpusd-gbpusd#i)/gbpusd#i" +
                                     "(gbpchf-gbpchf#i)/gbpchf#i)";
            if (pair == "eurgbp" && typeIndex == 2)
                formulaPlaceholder = "(i<@period)??(1+" +
                                     "eurusd^0.6504*eurgbp^0.3056*eurjpy^0.1891*eursek^0.0785*eurchf^0.1113/" +
                                     "(gbpusd^0.167*eurgbp^-0.743*gbpjpy^0.043*gbpcad^0.024*gbpchf^0.023)-" +
                                     "eurusd#i^0.6504*eurgbp#i^0.3056*eurjpy#i^0.1891*eursek#i^0.0785*eurchf#i^0.1113/" +
                                     "(gbpusd#i^0.167*eurgbp#i^-0.743*gbpjpy#i^0.043*gbpcad#i^0.024*gbpchf#i^0.023))-1";

            if (pair == "gbpjpy" && typeIndex == 1)
                formulaPlaceholder = "100*((gbpusd-gbpusd#i)/gbpusd#i + " +
                                     "(gbpchf-gbpchf#i)/gbpchf#i) - (eurgbp-eurgbp#i)/eurgbp#i + " +
                                     "100*((usdjpy-usdjpy#i)/usdjpy#i + (eurjpy-eurjpy#i)/eurjpy#i + " +
                                     "(chfjpy-chfjpy#i)/chfjpy#i)";
            if (pair == "gbpjpy" && typeIndex == 2)
                formulaPlaceholder = "(i<@period)??(1+" +
                    "gbpusd^0.167*eurgbp^-0.743*gbpjpy^0.043*gbpcad^0.024*gbpchf^0.023/(usdjpy^-0.258*eurjpy^-0.124*gbpjpy^-0.029)-" +
                    "gbpusd#i^0.167*eurgbp#i^-0.743*gbpjpy#i^0.043*gbpcad#i^0.024*gbpchf#i^0.023/(usdjpy#i^-0.258*eurjpy#i^-0.124*gbpjpy#i^-0.029))-1";

            if (pair == "chfjpy" && typeIndex == 1)
                formulaPlaceholder = "100*((usdchf#i-usdchf)/usdchf#i - (gbpchf-gbpchf#i)/gbpchf#i - (eurchf-eurchf#i)/eurchf#i)" +
                    "100*((usdjpy-usdjpy#i)/usdjpy#i + (eurjpy-eurjpy#i)/eurjpy#i + (gbpjpy-gbpjpy#i)/gbpjpy#i)";
            
            if (pair == "chfjpy" && typeIndex == 2)
                formulaPlaceholder = "(i<@period)??(1+" +
                    "usdchf^-0.115*eurchf^-0.69*gbpchf^-0.195/(usdjpy^-0.258*eurjpy^-0.124*gbpjpy^-0.029)-" +
                    "usdchf#i^-0.115*eurchf#i^-0.69*gbpchf#i^-0.195/(usdjpy#i^-0.258*eurjpy#i^-0.124*gbpjpy#i^-0.029))-1";


            if (string.IsNullOrEmpty(formulaPlaceholder)) return string.Format("Индекс не определен для пары \"{0}\"", pair);

            // заменить #i на #{Period}
            var indexPart = string.Format("#{0}", period);
            formulaPlaceholder = formulaPlaceholder.Replace("#i", indexPart);
            formulaPlaceholder = formulaPlaceholder.Replace("@period", period.ToString());
            // заменить валютную пару графика-владельца на close
            formulaPlaceholder = formulaPlaceholder.Replace(pair, "close");
            return formulaPlaceholder;
        }

        public override string MakeTitleByParams()
        {
            var smb = parameters["pair"].ValueStr;
            var period = parameters["period"].ValueNum;
            return string.Format("{0} {1}[{2}]", Title, smb, period);
        }
    }

    /// <summary>
    /// валютный индекс, с устаревшими либо актуализированными весами
    /// </summary>
    public class CodeTemplateCurrencyIndex : BaseCodeTemplate
    {
        public override string Title { get { return "Валютный индекс"; } }

        public CodeTemplateCurrencyIndex()
        {
            parameters = new Dictionary<string, TemplateParameter>
                             {{"currency", new TemplateParameter("currency", "Валюта", false, true, false)},
                              {"ownerPair", new TemplateParameter("ownerPair", "Валютная пара графика", false, true, false)}, 
                              {"type", new TemplateParameter("type", "Вид индекса (1 - классический, 2 - актуализированный)", 
                                  true, false, false)}};
        }

        public override string MakeFormula()
        {
            var curx = parameters["currency"].ValueStr;
            if (string.IsNullOrEmpty(curx)) return "Ошибка задания параметров: не определена валюта";
            curx = curx.ToLower();

            var typeIndex = (int)parameters["type"].ValueNum;
            if (typeIndex < 1 || typeIndex > 2)
                return "Ошибка задания параметров: вид индекса должен быть 1 или 2";

            // расписать формулу
            var formulaPlaceholder = "";
            if (curx == "usd" && typeIndex == 1)
                formulaPlaceholder = "50.14348112*eurusd^-0.576*usdjpy^0.136*gbpusd^-0.119*" +
                    "usdcad^0.091*usdchf^0.036"; // *usdsek^0.042";
            if (curx == "usd" && typeIndex == 2)
                formulaPlaceholder = "50.14348112*eurusd^-0.3349*usdjpy^0.1652*gbpusd^-0.082*" +
                    "usdcad^0.2887*usdchf^0.027"; // *usdsek^0.0189";

            if (curx == "eur" && typeIndex == 1)
                formulaPlaceholder = "34.38805726*eurusd^0.3155*eurgbp^0.3056*eurjpy^0.1891*" +
                    "eurchf^0.1113"; // *eursek^0.0785";
            if (curx == "eur" && typeIndex == 2)
                formulaPlaceholder = "34.38805726*eurusd^0.3121*eurgbp^0.285*eurjpy^0.1322*" +
                    "eurchf^0.1019"; // *eursek^0.0764";

            if (curx == "usd" && typeIndex == 1)
                formulaPlaceholder = "50.14348112*eurusd^-0.576*usdjpy^0.136*gbpusd^-0.119*" +
                    "usdcad^0.091*usdchf^0.036"; // *usdsek^0.042";

            if (curx == "jpy")
            {
                const double jpycny = 12.26, jpyhkd = 0.09838, jpykrw = 13.35, jpytwd = 0.3645, 
                    jpythb = 0.3802, hkdjpy = 10.16;
                formulaPlaceholder = "798*usdjpy^-0.258*eurjpy^-0.124*gbpjpy^-0.029*jpycny^0.154*jpykrw^0.089*" +
                                     "jpytwd^0.084*(hkdjpy??_hkdjpy_)^-0.069*jpythb^0.043";

                formulaPlaceholder = formulaPlaceholder.Replace("jpycny", jpycny.ToStringUniform());
                formulaPlaceholder = formulaPlaceholder.Replace("jpyhkd", jpyhkd.ToStringUniform());
                formulaPlaceholder = formulaPlaceholder.Replace("jpykrw", jpykrw.ToStringUniform());
                formulaPlaceholder = formulaPlaceholder.Replace("jpytwd", jpytwd.ToStringUniform());
                formulaPlaceholder = formulaPlaceholder.Replace("jpytwd", jpytwd.ToStringUniform());
                formulaPlaceholder = formulaPlaceholder.Replace("jpythb", jpythb.ToStringUniform());
                formulaPlaceholder = formulaPlaceholder.Replace("_hkdjpy_", hkdjpy.ToStringUniform());
            }

            if (curx == "gbp")
                formulaPlaceholder = "gbpusd^0.167*eurgbp^-0.743*gbpjpy^0.043*gbpcad^0.024*gbpchf^0.023";

            if (curx == "chf")
                formulaPlaceholder = "usdchf^-0.115*eurchf^-0.69*gbpchf^-0.195";

            if (curx == "cad")
                formulaPlaceholder = "usdcad^-0.7618*eurcad^-0.0931*(cadjpy??1)^0.0527*(gbpcad??1)^-0.0271";

            if (curx == "nzd")
                formulaPlaceholder = "0.248633927*nzdusd^0.665*nzdjpy^0.335";

            if (curx == "aud")
                formulaPlaceholder = "0.527687*audusd^0.335*audnzd^0.436*audjpy^0.137*audchf^0.027*audcad^0.065";

            if (string.IsNullOrEmpty(formulaPlaceholder)) return string.Format("Индекс не определен для валюты \"{0}\"", curx);

            // заменить валютную пару графика-владельца на close
            var ownerPair = parameters["ownerPair"].ValueStr;
            if (!string.IsNullOrEmpty(ownerPair))
                formulaPlaceholder = formulaPlaceholder.Replace(ownerPair.ToLower(), "close");
            return formulaPlaceholder;
        }

        public override string MakeTitleByParams()
        {
            var smb = parameters["currency"].ValueStr;
            var typeStr = parameters["type"].ValueNum == 1 ? "класс." : "акт.";
            return string.Format("{0} ({1} {2})", Title, smb, typeStr);
        }
    }

    /// <summary>
    /// ROC stands for Rate Of Changes
    /// классический индекс берется в приращении
    /// опционально переворачивается (USDX -> 1/USDX)
    /// пример
    /// (i знак_меньше 4)??(1+close-(close+close#1+close#2+close#3)/4)-1
    /// </summary>
    public class CodeTemplateCurrencyIndexROC : BaseCodeTemplate
    {
        public override string Title { get { return "Приращение индекса"; } }

        public CodeTemplateCurrencyIndexROC()
        {
            parameters = new Dictionary<string, TemplateParameter>
                             {{"currency", new TemplateParameter("currency", "Валюта", false, true, false)},
                              {"period", new TemplateParameter("period", "Период индекса", true, false, false)},
                              {"ownerPair", new TemplateParameter("ownerPair", "Валютная пара графика", false, true, false)}, 
                              {"invert", new TemplateParameter("invert", "Обратить (1/XXX)", false, false, true)}};
        }

        public override string MakeFormula()
        {
            var curx = parameters["currency"].ValueStr;
            if (string.IsNullOrEmpty(curx)) return "Ошибка задания параметров: не определена валюта";
            curx = curx.ToLower();

            var period = parameters["period"].ValueNum;
            if (period < 1) return "Ошибка задания параметров: период должен быть > 0";

            var invert = parameters["invert"].ValueBit;
            
            // расписать формулу
            var formulaPlaceholder = "";
            if (curx == "usd" && invert)
                formulaPlaceholder = "(i<#i)??(1+(1/(eurusd^-0.3349*usdjpy^0.1652*gbpusd^-0.082*" +
                    "usdcad^0.2887*usdchf^0.027) - 1/(eurusd##i^-0.3349*usdjpy##i^0.1652*" +
                    "gbpusd##i^-0.082*usdcad##i^0.2887*usdchf##i^0.027)))-1";
            if (curx == "usd" && invert == false)
                formulaPlaceholder = "(i<#i)??(1+(eurusd^-0.3349*usdjpy^0.1652*gbpusd^-0.082*" +
                    "usdcad^0.2887*usdchf^0.027 - eurusd##i^-0.3349*usdjpy##i^0.1652*" +
                    "gbpusd##i^-0.082*usdcad##i^0.2887*usdchf##i^0.027))-1";
            if (curx == "eur" && invert)
                formulaPlaceholder = "(i<#i)??(1+(1/(eurusd^0.3121*eurgbp^0.285*eurjpy^0.1322*" +
                    "eurchf^0.1019) - 1/(eurusd##i^0.3121*eurgbp##i^0.285*eurjpy##i^0.1322*" +
                    "eurchf##i^0.1019)))-1";
            if (curx == "eur" && invert == false)
                formulaPlaceholder = "(i<#i)??(1+(eurusd^0.3121*eurgbp^0.285*eurjpy^0.1322*" +
                    "eurchf^0.1019 - eurusd##i^0.3121*eurgbp##i^0.285*eurjpy##i^0.1322*" +
                    "eurchf##i^0.1019))-1";
            if (curx == "gbp" && invert == false)
                formulaPlaceholder = "(i<#i)??(1+(gbpusd^0.167*eurgbp^-0.743*gbpjpy^0.043*gbpcad^0.024*gbpchf^0.023-" +
                    "gbpusd##i^0.167*eurgbp##i^-0.743*gbpjpy##i^0.043*gbpcad##i^0.024*gbpchf##i^0.023" +
                    "))-1";
            if (curx == "gbp" && invert)
                formulaPlaceholder = "(i<#i)??(1+(gbpusd^-0.167*eurgbp^0.743*gbpjpy^-0.043*gbpcad^-0.024*gbpchf^-0.023-" +
                    "gbpusd##i^-0.167*eurgbp##i^0.743*gbpjpy##i^-0.043*gbpcad##i^-0.024*gbpchf##i^-0.023" +
                    "))-1";
            if (curx == "chf" && invert == false)
                formulaPlaceholder = "(i<#i)??(1+(usdchf^-0.115*eurchf^-0.69*gbpchf^-0.195 - " +
                                     "usdchf##i^-0.115*eurchf##i^-0.69*gbpchf##i^-0.195))-1";
            if (curx == "chf" && invert)
                formulaPlaceholder = "(i<#i)??(1+(usdchf^0.115*eurchf^0.69*gbpchf^0.195 - " +
                                     "usdchf##i^0.115*eurchf##i^0.69*gbpchf##i^0.195))-1";
            if (curx == "jpy" && invert == false)
                formulaPlaceholder = "(i<#i)??(1+(usdjpy^-0.258*eurjpy^-0.124*gbpjpy^-0.029 -" +
                                     "usdjpy##i^-0.258*eurjpy##i^-0.124*gbpjpy##i^-0.029))-1";
            if (curx == "jpy" && invert)
                formulaPlaceholder = "(i<#i)??(1+(usdjpy^0.258*eurjpy^0.124*gbpjpy^0.029 -" +
                                     "usdjpy##i^0.258*eurjpy##i^0.124*gbpjpy##i^0.029))-1";
            
            if (curx == "cad" && !invert)
                formulaPlaceholder = "(i<#i)??(1+(usdcad^-0.7618*eurcad^-0.0931*(cadjpy??1)^0.0527*(gbpcad??1)^-0.0271 -" +
                "usdcad##i^-0.7618*eurcad##i^-0.0931*(cadjpy##i??1)^0.0527*(gbpcad##i??1)^-0.0271))-1";

            if (curx == "cad" && invert)
                formulaPlaceholder = "(i<#i)??(1+(usdcad^0.7618*eurcad^0.0931*(cadjpy??1)^-0.0527*(gbpcad??1)^0.0271 -" +
                "usdcad##i^0.7618*eurcad##i^0.0931*(cadjpy##i??1)^-0.0527*(gbpcad##i??1)^0.0271))-1";

            if (string.IsNullOrEmpty(formulaPlaceholder)) 
                return string.Format("Индекс не определен для валюты \"{0}\"", curx);

            // заменить #i на период
            var periodStr = period.ToString();
            formulaPlaceholder = formulaPlaceholder.Replace("#i", periodStr);

            // заменить валютную пару графика-владельца на close
            var ownerPair = parameters["ownerPair"].ValueStr;
            if (!string.IsNullOrEmpty(ownerPair))
                formulaPlaceholder = formulaPlaceholder.Replace(ownerPair.ToLower(), "close");
            return formulaPlaceholder;
        }

        public override string MakeTitleByParams()
        {
            var smb = parameters["currency"].ValueStr;
            var period = parameters["period"].ValueNum;
            var invert = parameters["invert"].ValueBit;
            return string.Format("{0} ({1} {2}{3})", Title, smb, period, invert ? ", инв." : "");
        }
    }

    /// <summary>
    /// из цены вычитается ее тренд в виде скользящей средней
    /// </summary>
    public class CodeTemplateDetrendedPrice : BaseCodeTemplate
    {
        public override string Title { get { return "Внутритрендовое приращение"; } }

        public CodeTemplateDetrendedPrice()
        {
            parameters = new Dictionary<string, TemplateParameter>
                {
                    {"ticker", new TemplateParameter("ticker", "Параметр (close по-умолчению)", false, true, false)},
                    {"period", new TemplateParameter("period", "Период цикла", true, false, false)}
                };
        }

        public override string MakeFormula()
        {
            var ticker = parameters["ticker"].ValueStr;
            if (string.IsNullOrEmpty(ticker)) ticker = "close";
            
            var period = parameters["period"].ValueNum;
            if (period < 1) return "Ошибка задания параметров: период должен быть > 0";            

            // расписать формулу
            var formula = new StringBuilder(string.Format("(i<{0})??(1+{1}-({1}", period, ticker));            
            for (var i = 1; i < period; i++)
            {
                formula.AppendFormat("+close#{0}", i);
            }
            formula.AppendFormat(")/{0})-1", period);

            return formula.ToString();
        }

        public override string MakeTitleByParams()
        {
            var period = parameters["period"].ValueNum;            
            return string.Format("{0} ({1})", Title, period);
        }
    }
}
