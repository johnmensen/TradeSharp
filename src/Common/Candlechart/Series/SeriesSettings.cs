using System.Collections.Generic;
using Entity;

namespace Candlechart.Series
{
    /// <summary>
    /// входит в состав UserSettings->ChartWindowSettigns в единственном экземпляре, хранит
    /// настройки серий
    /// </summary>
    public class SeriesSettings
    {
        #region Серии Фибоначчи (бары разворота)

        [PropertyXMLTag("Series.TurnBar.FiboDontSumDegrees")]
        public bool FiboDontSumDegrees { get; set; }

        private List<int> fiboLevels = new List<int>();
        [PropertyXMLTag("Series.TurnBar.FiboLevels")]
        public List<int> FiboLevels
        {
            get { return fiboLevels; }
            set { fiboLevels = value; }
        }

        private int fiboFilter = 2;
        [PropertyXMLTag("Series.TurnBar.Filter")]
        public int FiboFilter
        {
            get { return fiboFilter; }
            set { fiboFilter = value; }
        }

        /// <summary>
        /// 2,1 - бар отсчета получит 2 очка, соседние - по одному
        /// 4,2,1 - бар отсчета получит 4 очка, соседние - по 2, через 1 бар - по одному очку
        /// </summary>
        private List<int> fiboMarks = new List<int>();
        [PropertyXMLTag("Series.TurnBar.Marks")]
        public List<int> FiboMarks
        {
            get { return fiboMarks; }
            set { fiboMarks = value; }
        }

        #endregion
    }
}
