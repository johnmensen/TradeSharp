using System;

namespace TradeSharp.Contract.Entity
{
    [Serializable]
    public class PerformerSearchCriteria
    {
        public string propertyName;

        public string compradant;

        public bool ignoreCase;

        public bool checkWholeWord;
    }
}
