using System.Collections.Generic;
using System.ComponentModel;
using Candlechart.Chart;
using Candlechart.Core;
using Entity;

namespace Candlechart.Indicator
{
    public interface IChartIndicator
    {
        string Name { get; }

        /// <summary>
        /// Генерируется самим индикатором уникальное имя
        /// </summary>
        string UniqueName { get; set; }
        /// <summary>
        /// вернуть что-то вида RSI14.RSI ...
        /// </summary>        
        string GetFullyQualifiedPaneName();
        /// <summary>
        /// построить - собственные серии, на свечном графике...
        /// </summary>        
        void BuildSeries(ChartControl chart);
        /// <summary>
        /// добавить на чарт
        /// </summary>
        void Add(ChartControl chart, Pane ownerPane);
        /// <summary>
        /// убрать с чарта
        /// </summary>
        void Remove();
        /// <summary>
        /// применить новые настройки
        /// </summary>
        void AcceptSettings();
        /// <summary>
        /// свеча добавилась - перерисовать индюк
        /// (возможно, не весь)
        /// </summary>        
        void OnCandleUpdated(CandleData updatedCandle, List<CandleData> newCandles);
        /// <summary>
        /// Вернуть подсказку по уровню / объекту под курсором
        /// на входе - координаты в CandleChartControl
        /// </summary>
        /// <param name="x">координата Х курсора</param>
        /// <param name="y">координата У курсора</param>
        /// <param name="index">индекс свечи под курсором</param>
        /// <param name="price">цена под курсором</param>
        /// <param name="tolerance">точность попадания, пикс</param>
        /// <returns>подсказку по уровню индюка, объекту под курсором и т.д.</returns>
        string GetHint(int x, int y, double index, double price, int tolerance);
        /// <param name="screenX">экранные координаты курсора</param>
        /// <param name="screenY">экранные координаты курсора</param>
        /// <param name="tolerance">точность попадания, пикс</param>
        /// <returns>список объектов, м.б. пустой</returns>
        List<IChartInteractiveObject> GetObjectsUnderCursor(int screenX, int screenY, int tolerance);

        /// <summary>
        /// серии родительских индикаторов отображение в property grid
        /// </summary>
        string SeriesSourcesDisplay { get; set; }

        /// серии родительских индикаторов
        [Browsable(false)]
        List<Series.Series> SeriesSources { get; set; }
        [Browsable(false)]
        List<Series.Series> SeriesResult { get; }

        /// <summary>
        /// панель отображения индикатора
        /// </summary>
        [Browsable(false)]
        Pane DrawPane { get; set; }

        string DrawPaneDisplay { get; set; }

        bool CreateOwnPanel { get; set; }

        bool IsPanelVisible { get; set; }
    }    
}
