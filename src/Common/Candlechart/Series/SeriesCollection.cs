using System.Collections;
using Candlechart.Core;

namespace Candlechart.Series
{
    public class SeriesCollection : CollectionBase
    {
        // Fields
        private readonly Pane _owner;

        // Methods
        internal SeriesCollection(Pane owner)
        {
            _owner = owner;
        }

        public Series this[string name]
        {
            get
            {
                foreach (Series series in InnerList)
                {
                    if (series.Name == name)
                    {
                        return series;
                    }
                }
                return null;
            }
        }

        public void Add(Series series)
        {
            series.Owner = _owner;
            InnerList.Add(series);
        }

        public new void Clear()
        {
            base.Clear();
        }

        public void Remove(Series series)
        {
            InnerList.Remove(series);            
        }

        public bool ContainsSeries(Series series)
        {
            return InnerList.Contains(series);
        }

        public void Remove(string name)
        {
            Remove(this[name]);
        }

        //private void RemoveAt(int index)
        //{
        //}
    }    
}
