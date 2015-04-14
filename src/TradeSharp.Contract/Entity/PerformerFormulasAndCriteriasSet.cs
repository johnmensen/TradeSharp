using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TradeSharp.Contract.Entity
{
    [Serializable]
    public class PerformerFormulasAndCriteriasSet
    {
        /// <summary>
        /// формулы
        /// </summary>
        public string[] formulaValues;

        /// <summary>
        /// описания формул
        /// </summary>
        public string[] formulaTitles;

        /// <summary>
        /// имена параметров формул
        /// </summary>
        public string[] parameterNames;

        /// <summary>
        /// названия параметров формул
        /// </summary>
        public string[] parameterTitles;

        

        public PerformerFormulasAndCriteriasSet()
        {
        }
    }
}
