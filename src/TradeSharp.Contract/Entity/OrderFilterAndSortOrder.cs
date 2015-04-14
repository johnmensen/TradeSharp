using System;
using System.Linq;

namespace TradeSharp.Contract.Entity
{
    /// <summary>
    /// используется при запросе ордеров
    /// перечисляет условия фильтрации и сортировки для выборки
    /// </summary>
    [Serializable]
    public class OrderFilterAndSortOrder
    {
        public int? filterMagic;

        public string filterTicker;

        public int? filterSide;

        public string filterComment;

        public string filterExpertComment;

        public DateTime? filterTimeEnterStartsWith;

        public DateTime? filterTimeEnterEndsWith;

        public DateTime? filterTimeExitStartsWith;

        public DateTime? filterTimeExitEndsWith;

        public bool sortByTimeEnter;

        public bool sortAscending;

        public int takeCount;

        public IQueryable<MarketOrder> ApplyFilter(IQueryable<MarketOrder> query)
        {
            query = query.Where(pos =>
                               (filterMagic == null || filterMagic.Value == pos.Magic) &&
                               (filterSide == null || filterSide.Value == pos.Side) &&
                               (string.IsNullOrEmpty(filterComment) ||
                                pos.Comment.Contains(filterComment)) &&
                               (string.IsNullOrEmpty(filterExpertComment) ||
                                pos.ExpertComment.Contains(filterExpertComment)) &&
                               (string.IsNullOrEmpty(filterTicker) ||
                                pos.Symbol.Contains(filterTicker)) &&
                               (filterTimeEnterStartsWith == null ||
                                pos.TimeEnter >= filterTimeEnterStartsWith.Value) &&
                               (filterTimeEnterEndsWith == null ||
                                pos.TimeEnter <= filterTimeEnterEndsWith.Value) &&
                               (filterTimeExitStartsWith == null ||
                                pos.TimeExit >= filterTimeExitStartsWith) &&
                               (filterTimeExitEndsWith == null ||
                                pos.TimeExit <= filterTimeExitEndsWith));

            query = (sortAscending ? query.OrderBy(p => sortByTimeEnter ? p.TimeEnter : p.TimeExit)
                                 : query.OrderByDescending(p => sortByTimeEnter ? p.TimeEnter : p.TimeExit))
                                .Take(takeCount);
            return query;
        }
    }
}
