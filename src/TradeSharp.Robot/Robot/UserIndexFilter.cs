using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using Entity;
using TradeSharp.Util;

namespace TradeSharp.Robot.Robot
{
    /// <summary>
    /// описывает условия фильтрации входов по пользовательскому индексу
    /// условия вида: "запретить покупки если значение индекса > 0.05"
    /// </summary>
    public class UserIndexFilter
    {
        [PropertyXMLTag("UserIndexFilter.IndexFormula"), 
        DisplayName("Валютный индекс"), 
        Category("Основные"), 
        Description("Валютный индекс, значение которого проверяется"), 
        Editor(typeof(FormulaUIEditor), typeof(UITypeEditor))]
        public string IndexFormula { get; set; }

        private List<UserIndexFilterCondition> conditions = new List<UserIndexFilterCondition>();

        [PropertyXMLTag("UserIndexFilter.Conditions"),
        DisplayName("Условия"),
        Category("Основные"),
        Description("Условия фильтрации")]
        public List<UserIndexFilterCondition> Conditions
        {
            get { return conditions; }
            set { conditions = value; }
        }

        public IndexCalculator indexCalculator;

        [Browsable(false)]
        public double? IndexValue
        {
            get { return indexCalculator == null ? null : indexCalculator.lastIndexValue; }
        }

        public UserIndexFilter() {}

        public UserIndexFilter(UserIndexFilter filter)
        {
            IndexFormula = filter.IndexFormula;
            foreach (var con in filter.conditions)
            {
                conditions.Add(new UserIndexFilterCondition(con));
            }
        }

        public void Initialize()
        {
            indexCalculator = new IndexCalculator(IndexFormula);
        }

        public bool IsEnterProhibided(DealType side)
        {
            if (!indexCalculator.lastIndexValue.HasValue) return false;
            foreach (var con in conditions)
            {
                if ((con.ProhibitionType == UserIndexFilterCondition.DealProhibitionType.ЗапретПокупок &&
                    side == DealType.Sell) ||
                    (con.ProhibitionType == UserIndexFilterCondition.DealProhibitionType.ЗапретПродаж &&
                    side == DealType.Buy)) continue;
                if (con.IsProhibided(indexCalculator.lastIndexValue.Value)) return true;
            }
            return false;
        }
    }

    public class UserIndexFilterCondition
    {
        public enum ConditionType { Равно = 0, Больше, Меньше, БольшеРавно, МеньшеРавно, НеРавно }

        public enum DealProhibitionType { ЗапретВходов = 0, ЗапретВходовВыходов, ЗапретПокупок, ЗапретПродаж }

        [PropertyXMLTag("UserIndexFilter.Condition"),
        DisplayName("Условие"),
        Category("Основные"),
        Description("Условие")]
        public ConditionType Condition { get; set; }

        [PropertyXMLTag("UserIndexFilter.Value"),
        DisplayName("Значение"),
        Category("Основные"),
        Description("Проверяемое значение")]
        public double CompValue { get; set; }

        [PropertyXMLTag("UserIndexFilter.ProhibitionType"),
        DisplayName("Фильтр"),
        Category("Основные"),
        Description("Фильтр")]
        public DealProhibitionType ProhibitionType { get; set; }

        public UserIndexFilterCondition() { }
        public UserIndexFilterCondition(UserIndexFilterCondition filter)
        {
            Condition = filter.Condition;
            CompValue = filter.CompValue;
            ProhibitionType = filter.ProhibitionType;
        }

        public bool IsProhibided(double index)
        {
            if (Condition == ConditionType.Равно) return index == CompValue;
            if (Condition == ConditionType.НеРавно) return index != CompValue;
            if (Condition == ConditionType.МеньшеРавно) return index <= CompValue;
            if (Condition == ConditionType.Меньше) return index < CompValue;
            if (Condition == ConditionType.БольшеРавно) return index >= CompValue;
            /*if (Condition == ConditionType.Больше) */return index > CompValue;
        }
    }
}
