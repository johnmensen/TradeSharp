using System;
using System.Windows.Forms;

namespace TradeSharp.Client.BL
{
    /// <summary>
    /// интерфейс реализуют окна, ведущие себя как обычные MDI, сохраняемые
    /// в конфигурации, размещаемые в закладках... но это - не окна графиков
    /// 
    /// например, окно Счет, окно торговых роботов, окно чата, окно доходности и т.д.
    /// </summary>
    public interface IMdiNonChartWindow
    {
        NonChartWindowSettings.WindowCode WindowCode { get; }

        int WindowInnerTabPageIndex { get; set; }

        /// <summary>
        /// на перемещение формы - показать опции Drag & Drop
        /// </summary>
        event Action<Form> FormMoved;

        /// <summary>
        /// перемещение формы завершено - показать варианты Drop-a
        /// </summary>
        event Action<Form> ResizeEnded;
    }
}
