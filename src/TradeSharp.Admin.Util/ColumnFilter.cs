using TradeSharp.Util;

namespace TradeSharp.Admin.Util
{
    public abstract class BaseColumnFilter
    {
        public abstract bool IsOff { get; }

        protected static bool CheckStringCriteria(ColumnFilterCriteria crit, string value, string specimen)
        {
            if (crit == ColumnFilterCriteria.Равно && value != specimen) return false;
            if (crit == ColumnFilterCriteria.НеРавно && value == specimen) return false;
            if (crit == ColumnFilterCriteria.Включает && value.Contains(specimen) == false) return false;
            if (crit == ColumnFilterCriteria.НачинаетсяС && value.StartsWith(specimen) == false) return false;
            if (crit == ColumnFilterCriteria.КончаетсяНа && value.EndsWith(specimen) == false) return false;
            return true;
        }
    }
}
