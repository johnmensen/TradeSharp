using System;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using Candlechart.Series;

namespace Candlechart.Core
{
    /// <summary>
    /// интерфейс реализует каждый графический объект свечного графика,
    /// который выводится в общем списке объектов
    /// </summary>
    public interface IChartInteractiveObject
    {
        string ClassName { get; }

        bool Selected { get; set; }

        string Name { get; set; }

        DateTime? DateStart { get; set; }

        int IndexStart { get; }

        InteractiveObjectSeries Owner { get; set; }

        int Magic { get; set; }

        /// <summary>
        /// сохранить в XML-формате в указанный родительский узел
        /// </summary>
        /// <param name="parentNode">родительский узел (например, {Provections})</param>
        /// <param name="owner">график - "владелец" объекта</param>
// ReSharper disable InconsistentNaming
        void SaveInXML(XmlElement parentNode, CandleChartControl owner);
// ReSharper restore InconsistentNaming

        /// <summary>
        /// загрузить из узла, описывающего элемент
        /// </summary>
        /// <param name="itemNode">узел, описывающий элемент</param>
        /// <param name="owner">график - "владелец" объекта</param>
// ReSharper disable InconsistentNaming
        void LoadFromXML(XmlElement itemNode, CandleChartControl owner);
// ReSharper restore InconsistentNaming

        ChartObjectMarker IsInMarker(int screenX, int screenY, Keys modifierKeys);
        
        void OnMarkerMoved(ChartObjectMarker marker);

        void AjustColorScheme(CandleChartControl chart);

        Image CreateSample(Size sizeHint);
    }
}
