using System;
using System.Collections.Generic;

namespace TradeSharp.Util
{
    [Flags]
    public enum ColumnFilterCriteria
    {
        Нет = 0,
        Равно = 1, НеРавно = 2, Включает = 4, НеВключает = 8, Больше = 16, Меньше = 32, НачинаетсяС = 64, КончаетсяНа = 128
    }
    
    [AttributeUsage(AttributeTargets.Property)]
    public class EntityFilterAttribute : Attribute
    {
        public string Title { get; set; }
        
        public string Category { get; set; }
        
        public ColumnFilterCriteria EnabledCriterias { get; set; }
        
        private List<object > enabledValues = new List<object>();
        public Object[] EnabledValues
        {
            get { return enabledValues.ToArray(); }
            set { enabledValues.Clear(); enabledValues.AddRange(value); }
        }
        public bool MatchCase { get; set; }

        public bool CheckAuto { get; set; }

        #region Constructors
        public EntityFilterAttribute(string title)
        {
            Title = title;
        }
        #endregion
    }
}
