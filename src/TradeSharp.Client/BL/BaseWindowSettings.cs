using System.Drawing;
using Entity;

namespace TradeSharp.Client.BL
{
    /// <summary>
    /// базовые настройки окна - размеры, позиция, состояние
    /// 
    /// от него наследует описание окна графика, от него наследуют описания прочих окон
    /// </summary>
    public class BaseWindowSettings
    {
        private Size windowSize;
        [PropertyXMLTag("Chart.Size")]
        public Size WindowSize
        {
            get { return windowSize; }
            set
            {
                UserSettings.Instance.lastTimeModified.Touch();
                windowSize = value;
            }
        }

        private Point windowPos;
        [PropertyXMLTag("Chart.Pos")]
        public Point WindowPos
        {
            get { return windowPos; }
            set { windowPos = value; UserSettings.Instance.lastTimeModified.Touch(); }
        }

        private string windowState;
        [PropertyXMLTag("Chart.WindowState")]
        public string WindowState
        {
            get { return windowState; }
            set { windowState = value; UserSettings.Instance.lastTimeModified.Touch(); }
        }
    }
}
