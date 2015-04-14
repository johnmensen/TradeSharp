using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using Entity;
using TradeSharp.Robot.Robot;
using TradeSharp.Util;

namespace TradeSharp.Robot.BL
{
    // ReSharper disable LocalizableElement
    /// <summary>
    /// описывает валютный индекс и настройки дивергенций индекса
    /// </summary>
    public class IndexDivergencyInfo
    {
        public string indexFormula = 
            "close-(close+close#1+close#2+close#3+close#4+close#5+close#6+close#7+close#8+close#9+close#10+close#11)/12";

        public double? LastIndexValue { get { return indexCalculator == null ? null : indexCalculator.lastIndexValue; } }

        public List<double> lastIndicies;

        /// <summary>
        /// массив "пиков" индикатора: индекс, знак (1 - перекупленность), цена
        /// </summary>
        public List<Cortege3<decimal, int, decimal>> indexPeaks;

        [PropertyXMLTag("Robot.IndexFormula")]
        [DisplayName("Валютный индекс")]
        [Category("Основные")]
        [Description("Валютный индекс, с которым ищутся дивергенции")]
        [Editor(typeof(FormulaUIEditor), typeof(UITypeEditor))]
        public string IndexFormulaL
        {
            get { return indexFormula; }
            set { indexFormula = value; }
        }

        private decimal indexMarginUp = 0.03M;
        [PropertyXMLTag("Robot.IndexMarginUp")]
        [DisplayName("Верх зоны п/п")]
        [Category("Основные")]
        [Description("Верхняя граница - зона условной перекупленности")]
        public decimal IndexMarginUp
        {
            get { return indexMarginUp; }
            set { indexMarginUp = value; }
        }

        private decimal indexMarginDn = -0.03M;
        [PropertyXMLTag("Robot.IndexMarginDn")]
        [DisplayName("Низ зоны п/п")]
        [Category("Основные")]
        [Description("Нижняя граница - зона условной перепроданности")]
        public decimal IndexMarginDn
        {
            get { return indexMarginDn; }
            set { indexMarginDn = value; }
        }

        [DisplayName("Бар подтверждения")]
        [Category("Основные")]
        [PropertyXMLTag("Robot.WaitOneBar")]
        [Description("Ждать один бар в сторону дивергенции")]
        public bool WaitOneBar { get; set; }

        private decimal isNaN;// = 0;
        [PropertyXMLTag("Robot.IsNaN")]
        [DisplayName("Замена IsNaN")]
        [Category("Основные")]
        [Description("Замена неопределенности при вычислениях на число")]
        public decimal IsNaN
        {
            get { return isNaN; }
            set { isNaN = value; }
        }

        #region Настройки дивергов
        public enum DivergenceType { Классические = 0, ОтКвазиЭкстремумов = 1 }

        [DisplayName("Тип дивергенций")]
        [Category("Дивергенции")]
        [PropertyXMLTag("Robot.DiverType")]
        [Description("Правила определения вершин диверов")]
        public DivergenceType DiverType { get; set; }

        private int periodExtremum = 6;
        [DisplayName("Экстрем., бар")]
        [Category("Дивергенции")]
        [PropertyXMLTag("Robot.PeriodExtremum")]
        [Description("Период экстремума на свечах, свечей")]
        public int PeriodExtremum
        {
            get { return periodExtremum; }
            set { periodExtremum = value; }
        }

        private int maxPastExtremum = 20;
        [DisplayName("Свеч до экстр.")]
        [Category("Дивергенции")]
        [PropertyXMLTag("Robot.MaxPastExtremum")]
        [Description("Макс. свечей до экстремума")]
        public int MaxPastExtremum
        {
            get { return maxPastExtremum; }
            set { maxPastExtremum = value; }
        }

        #endregion

        public IndexCalculator indexCalculator;

        public IndexDivergencyInfo() {}

        public IndexDivergencyInfo(IndexDivergencyInfo ind)
        {
            IndexFormulaL = ind.IndexFormulaL;
            IndexMarginUp = ind.IndexMarginUp;
            IndexMarginDn = ind.IndexMarginDn;
            IsNaN = ind.IsNaN;
            DiverType = ind.DiverType;
            PeriodExtremum = ind.PeriodExtremum;
            MaxPastExtremum = ind.MaxPastExtremum;
        }

        public void Initialize()
        {
            indexCalculator = new IndexCalculator(indexFormula);
        }

        /// <summary>
        /// располагая очередями candles и lastIndicies
        /// определить наличие бычьей (1) или медвежьей дивергенции (-1)
        /// в противном случае - 0
        /// </summary>        
        public int GetDivergenceSign(List<CandleData> candles, out string comment)
        {
            comment = string.Empty;
            if (candles.Count < 2) return 0;
            
            // расчет разный по типам экстремумов
            if (DiverType == DivergenceType.ОтКвазиЭкстремумов)           
                return FindDivPointsQuasi(candles);
            
            // классические диверы
            return FindDivPointsClassic(candles);
        }

        private int FindDivPointsClassic(List<CandleData> candles)
        {
            var spans = Divergency.FindDivergencePointsClassic(candles.Count, periodExtremum,
                                                   maxPastExtremum,
                                                   i => (double) candles[i].close,
                                                   i => lastIndicies[i],
                                                   WaitOneBar);

            return spans.Sum(s => s.end == candles.Count - (WaitOneBar ? 2 : 1) ? s.sign : 0);
        }

        private int FindDivPointsQuasi(List<CandleData> candles)
        {
            var spans = Divergency.FindDivergencePointsQuasi(
                candles.Count,
                (double)IndexMarginUp, 
                (double)IndexMarginDn,
                i => (double)candles[i].close,
                i => lastIndicies[i]);

            return spans.Sum(s => s.end == candles.Count - (WaitOneBar ? 2 : 1) ? s.sign : 0);
        }

        public void CalculateValue(string[] tickerNames, List<CandleData> candles,
            Dictionary<string, List<double>> lastBidLists, 
            DateTime curTime, Random randomGener)
        {
            if (indexCalculator != null) // посчитать индекс
            {
                indexCalculator.CalculateValue(tickerNames, candles, lastBidLists, curTime,
                    randomGener);
                lastIndicies.Add(indexCalculator.lastIndexValue ?? 0);
            }
        }
        /// <summary>
        /// подготовить словарь Имя-Значение для 
        /// </summary>        
        public Dictionary<string, double> GetVariableValues(string[] tickerNames,
            List<CandleData> candles, Dictionary<string, List<double>> lastBidLists, 
            DateTime curTime, Random randomGener)
        {
            if (indexCalculator == null) return new Dictionary<string, double>();
            return indexCalculator.GetVariableValues(tickerNames, candles, lastBidLists,
                                                     curTime, randomGener);            
        }        
    }
    // ReSharper restore LocalizableElement
}
