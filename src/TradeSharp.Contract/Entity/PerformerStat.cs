using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using TradeSharp.Util;

namespace TradeSharp.Contract.Entity
{
    [Serializable]
    public class PerformerStat
    {
        public const string FeeCurrency = "USD";

        [DisplayName("Счет")]
        [LocalizedDisplayName("TitleAccount")]
        [Description("№ счета")]
        [LocalizedExpressionParameterName("Account", DefaultSortOrder = SortOrder.Descending)]
        [DefaultExpressionValuesAttribute(ExpressionOperator.Equal, 1)]
        public int Account { get; set; }

        /// <summary>
        /// имя пользователя-владельца счета
        /// </summary>
        [DisplayName("Логин")]
        [LocalizedDisplayName("TitleLogin")]
        [Description("Логин трейдера")]
        public string Login { get; set; }

        [DisplayName("Эл. почта")]
        [LocalizedDisplayName("TitleEmail")]
        [Description("E-mail адрес")]
        public string Email { get; set; }

        [DisplayName("Группа")]
        [LocalizedDisplayName("TitleGroup")]
        [Description("Группа счета трейдера")]
        public string Group { get; set; }

        [DisplayName("Валюта")]
        [LocalizedDisplayName("TitleCurrency")]
        [Description("Валюта счета трейдера")]
        [PropertyOrder(4)]
        public string DepoCurrency { get; set; }

        [DisplayName("Реальный")]
        [LocalizedDisplayName("TitleReal")]
        [Description("Реальный счет")]
        [LocalizedExpressionParameterName("Real", DefaultSortOrder = SortOrder.Descending)]
        [DefaultExpressionValuesAttribute(ExpressionOperator.Equal, 1)]
        [PropertyOrder(9)]
        public bool IsRealAccount { get; set; }

        [DisplayName("№ сигнала")]
        [LocalizedDisplayName("TitleSignalNumber")]
        [Description("№ торгового сигнала")]
        public int Service { get; set; }

        [DisplayName("Тип сервиса")]
        [LocalizedDisplayName("TitleServiceType")]
        [Description("Тип сервиса (0 - сигнал, 1 - ПАММ)")]
        //[ExpressionParameterName("ServiceType", "Тип сервиса (0 - сигнал, 1 - ПАММ)")]
        public int ServiceType { get; set; }

        [DisplayName("Сигнал")]
        [LocalizedDisplayName("TitleSignal")]
        [Description("Название торгового сигнала")]
        public string TradeSignalTitle { get; set; }

        [DisplayName("Подписчиков")]
        [LocalizedDisplayName("TitleSubscriberCount")]
        [Description("Количество подписчиков")]
        [LocalizedExpressionParameterName("Sub", DefaultSortOrder = SortOrder.Descending)]
        [DefaultExpressionValuesAttribute(ExpressionOperator.Greater, 0)]
        [PropertyOrder(6)]
        public int SubscriberCount { get; set; }

        [PropertyOrder(1)]
        public byte[] Chart { get; set; }

        [DisplayName("Рейтинг")]
        [LocalizedDisplayName("TitleRating")]
        [Description("Рейтинг TRADE#")]
        [LocalizedExpressionParameterName("Score", DefaultSortOrder = SortOrder.Descending)]
        [DisplayFormat(DataFormatString = "f3", ApplyFormatInEditMode = true)]
        public float Score { get; set; }

        [DisplayName("Критерий")]
        [LocalizedDisplayName("TitleUserRating")]
        [Description("Пользовательский критерий")]
        [DisplayFormat(DataFormatString = "f3", ApplyFormatInEditMode = true)]
        [PropertyOrder(2)]
        public float UserScore { get; set; }

        [DisplayName("Торгует")]
        [LocalizedDisplayName("TitleTradeTime")]
        [Description("Дней торгует")]
        [LocalizedExpressionParameterName("Days", "TitleTradeTimeInDays", "TitleDaysUnits", true, DefaultSortOrder = SortOrder.Descending)]
        [DisplayFormat(DataFormatString = "f0", ApplyFormatInEditMode = true)]
        [DefaultExpressionValuesAttribute(ExpressionOperator.Greater, 30)]
        [PropertyOrder(8)]
        public int DaysTraded { get; set; }

        [DisplayName("Комиссия (USD)")]
        [LocalizedDisplayName("TitleFeeInUSD")]
        [Description("Комиссия за торговые сигналы (USD)")]
        [LocalizedExpressionParameterName("Fee", DefaultSortOrder = SortOrder.Ascending)]
        [DisplayFormat(DataFormatString = "f2", ApplyFormatInEditMode = true)]
        [DefaultExpressionValuesAttribute(ExpressionOperator.Lower, 2)]
        public decimal FeeUSD { get; set; }

        [DisplayName("Прибыль")]
        [LocalizedDisplayName("TitleProfit")]
        [Description("Прибыль, %")]
        [LocalizedExpressionParameterName("P", "TitleProfitInPercents", "%", DefaultSortOrder = SortOrder.Descending)]
        [DisplayFormat(DataFormatString = "f1", ApplyFormatInEditMode = true)]
        [DefaultExpressionValuesAttribute(ExpressionOperator.Greater, 15)]
        [PropertyOrder(3)]
        public float Profit { get; set; }

        [DisplayName("Пункты")]
        [LocalizedDisplayName("TitlePoints")]
        [Description("Прибыль, ценовые пункты")]
        [LocalizedExpressionParameterName("Points", "TitleProfitInPoints", "TitlePointsUnits", true, DefaultSortOrder = SortOrder.Descending)]
        [DisplayFormat(DataFormatString = "f0", ApplyFormatInEditMode = true)]
        [DefaultExpressionValuesAttribute(ExpressionOperator.Greater, 0)]
        [PropertyOrder(5)]
        public float SumProfitPoints { get; set; }

        [DisplayName("Макс. плечо")]
        [LocalizedDisplayName("TitleMaximumLeverage")]
        [Description("Максимальное торговое плечо")]
        [LocalizedExpressionParameterName("ML", DefaultSortOrder = SortOrder.Ascending)]
        [DisplayFormat(DataFormatString = "f2", ApplyFormatInEditMode = true)]
        [DefaultExpressionValuesAttribute(ExpressionOperator.Lower, 10)]
        public float MaxLeverage { get; set; }

        /// <summary>
        /// среднее плечо - без учета 0-х значений
        /// </summary>
        [DisplayName("Среднее плечо")]
        [LocalizedDisplayName("TitleAverageLeverage")]
        [Description("Среднее торговое плечо")]
        [LocalizedExpressionParameterName("AL", DefaultSortOrder = SortOrder.Ascending)]
        [DisplayFormat(DataFormatString = "f2", ApplyFormatInEditMode = true)]
        [DefaultExpressionValuesAttribute(ExpressionOperator.Lower, 5)]
        public float AvgLeverage { get; set; }

        [DisplayName("Макс. проседание")]
        [LocalizedDisplayName("TitleMaximumDrawdownShort")]
        [Description("Макс. относительное проседание, %")]
        [LocalizedExpressionParameterName("DD", "TitleMaximumRelativeDrawdownInPercents", "%", DefaultSortOrder = SortOrder.Ascending)]
        [DisplayFormat(DataFormatString = "f2", ApplyFormatInEditMode = true)]
        [DefaultExpressionValuesAttribute(ExpressionOperator.Lower, 30)]
        [PropertyOrder(9)]
        public float MaxRelDrawDown { get; set; }

        [DisplayName("К. Шарпа")]
        [LocalizedDisplayName("TitleSharpeRatioShort")]
        [Description("Коэфф. Шарпа")]
        [LocalizedExpressionParameterName("Sharp", "TitleSharpeRatio", DefaultSortOrder = SortOrder.Descending)]
        [DisplayFormat(DataFormatString = "f2", ApplyFormatInEditMode = true)]
        [DefaultExpressionValuesAttribute(ExpressionOperator.Greater, 0)]
        public float Sharp { get; set; }

        [DisplayName("Ср. геом. год. доход")]
        [LocalizedDisplayName("TitleGAAnnualProfitShort")]
        [Description("Среднегеомтрический годовой доход, %")]
        [LocalizedExpressionParameterName("AYP", "TitleGAAnnualProfitInPercentsShort", "%", DefaultSortOrder = SortOrder.Descending)]
        [DisplayFormat(DataFormatString = "f1", ApplyFormatInEditMode = true)]
        [DefaultExpressionValuesAttribute(ExpressionOperator.Greater, 15)]
        public float AvgYearProfit { get; set; }

        /// <summary>
        /// отношение среднего профита к среднему убытку
        /// </summary>
        [DisplayName("Ср. профит/убыток")]
        [LocalizedDisplayName("TitleRatioOfAverageProfitToAverageLossShort")]
        [Description("Отношение среднего профита к среднему убытку")]
        [LocalizedExpressionParameterName("GR", "TitleRatioOfAverageProfitToAverageLoss")]
        [DisplayFormat(DataFormatString = "f2", ApplyFormatInEditMode = true)]
        [DefaultExpressionValuesAttribute(ExpressionOperator.Greater, 1)]
        public float GreedyRatio { get; set; }

        /// <summary>
        /// профит за последние N месяцев
        /// </summary>
        [DisplayName("Профит N мес.%")]
        [LocalizedDisplayName("TitleProfitForNMonthsInPercentsShort")]
        [Description("Профит за последние N месяцев, %")]
        [LocalizedExpressionParameterName("Pn", "TitleProfitForNMonthsInPercents", "%", DefaultSortOrder = SortOrder.Descending)]
        [DisplayFormat(DataFormatString = "f1", ApplyFormatInEditMode = true)]
        [DefaultExpressionValuesAttribute(ExpressionOperator.Greater, 5)]
        public float ProfitLastMonths { get; set; }

        /// <summary>
        /// профит за последние N месяцев
        /// </summary>
        [DisplayName("Профит N мес.")]
        [LocalizedDisplayName("TitleProfitForNMonthsShort")]
        [Description("Профит за последние N месяцев, в валюте депозита")]
        [LocalizedExpressionParameterName("PnA", "TitleProfitForNMonthsInDepositCurrency", DefaultSortOrder = SortOrder.Descending)]
        [DisplayFormat(DataFormatString = "f2", ApplyFormatInEditMode = true)]
        [DefaultExpressionValuesAttribute(ExpressionOperator.Greater, 5)]
        public float ProfitLastMonthsAbs { get; set; }

        /// <summary>
        /// выведено со счета N месяцев
        /// </summary>
        [DisplayName("Вывел N мес.")]
        [LocalizedDisplayName("TitleWithdrewForLastNMonthsShort")]
        [Description("Выведено средств за последние N месяцев, валюта депозита")]
        [LocalizedExpressionParameterName("WN", "TitleWithdrewForLastNMonthsInDepositCurrency")]
        [DisplayFormat(DataFormatString = "f2", ApplyFormatInEditMode = true)]
        [DefaultExpressionValuesAttribute(ExpressionOperator.Lower, 30)]
        public float WithdrawalLastMonths { get; set; }

        [DisplayName("Объем торгов")]
        [LocalizedDisplayName("TitleTradeVolume")]
        [Description("Суммарный объем всех совершенных сделок, валюта депозита")]
        [LocalizedExpressionParameterName("V", "TitleSummaryVolumeOfAllDealsInDepositCurrency", DefaultSortOrder = SortOrder.Descending)]
        [DefaultExpressionValuesAttribute(ExpressionOperator.Greater, 50000)]
        public long TotalTradedInDepoCurrency { get; set; }

        [DisplayName("Ср. взвеш. прибыль/убыток")]
        [LocalizedDisplayName("TitleWeightedProfitToLossShort")]
        [Description("Средневзвешенный профит по сделке по отношению к сумме модулей результатов, %")]
        [LocalizedExpressionParameterName("WPtL", "TitleWeightedProfitToLossInPercents", "%")]
        [DisplayFormat(DataFormatString = "f2", ApplyFormatInEditMode = true)]
        [DefaultExpressionValuesAttribute(ExpressionOperator.Greater, 0)]
        public float AvgWeightedDealProfitToLoss { get; set; }

        [DisplayName("Сделок")]
        [LocalizedDisplayName("TitleDealsTotal")]
        [Description("Общее количество сделок")]
        [LocalizedExpressionParameterName("ND", DefaultSortOrder = SortOrder.Descending)]
        [DefaultExpressionValuesAttribute(ExpressionOperator.Greater, 25)]
        public int DealsCount { get; set; }

        [DisplayName("Средства")]
        [LocalizedDisplayName("TitleFunds")]
        [Description("Текущий баланс (средства), в валюте депозита")]
        [LocalizedExpressionParameterName("Eq", "TitleCurrentBalanceInDepositCurrency", DefaultSortOrder = SortOrder.Descending)]
        [DisplayFormat(DataFormatString = "f2", ApplyFormatInEditMode = true)]
        [DefaultExpressionValuesAttribute(ExpressionOperator.Greater, 500)]
        [PropertyOrder(7)]
        public float Equity { get; set; }

        public string FullName { get; set; }

        //[DisplayName("Пользователь")]
        public int UserId { get; set; }

        public PerformerStat()
        {
        }

        public PerformerStat(PerformerStat performer)
        {
            Account = performer.Account;
            Login = performer.Login;
            Group = performer.Group;
            DepoCurrency = performer.DepoCurrency;
            Service = performer.Service;
            ServiceType = performer.ServiceType;
            TradeSignalTitle = performer.TradeSignalTitle;
            SubscriberCount = performer.SubscriberCount;
            if (performer.Chart != null)
            {
                Chart = new byte[performer.Chart.Length];
                Array.Copy(performer.Chart, Chart, performer.Chart.Length);
            }
            Score = performer.Score;
            Profit = performer.Profit;
            MaxLeverage = performer.MaxLeverage;
            AvgLeverage = performer.AvgLeverage;
            MaxRelDrawDown = performer.MaxRelDrawDown;
            Sharp = performer.Sharp;
            AvgYearProfit = performer.AvgYearProfit;
            GreedyRatio = performer.GreedyRatio;
            UserScore = performer.UserScore;
            ProfitLastMonths = performer.ProfitLastMonths;
            TotalTradedInDepoCurrency = performer.TotalTradedInDepoCurrency;
            AvgWeightedDealProfitToLoss = performer.AvgWeightedDealProfitToLoss;
            DealsCount = performer.DealsCount;
            WithdrawalLastMonths = performer.WithdrawalLastMonths;
            Equity = performer.Equity;
            ProfitLastMonthsAbs = performer.ProfitLastMonthsAbs;
            UserId = performer.UserId;
            FeeUSD = performer.FeeUSD;
            DaysTraded = performer.DaysTraded;
            SumProfitPoints = performer.SumProfitPoints;
            IsRealAccount = performer.IsRealAccount;
            FullName = performer.FullName;
        }
    }
}
