using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using TradeSharp.Hub.BL.BL;
using TradeSharp.Hub.BL.Model;

namespace TradeSharp.Hub.BL.Repository
{
    public class EntityRepository<T> where T : class, new()
    {
        private readonly Dictionary<string, string> navFieldColumn = new Dictionary<string, string>();

        protected HubEntities context;

        protected readonly T speciman;

        public EntityRepository()
        {
            context = new HubEntities();
            context.Configuration.LazyLoadingEnabled = false;
            speciman = new T();

            foreach (var prop in typeof (T).GetProperties())
            {
                if (!prop.PropertyType.IsClass) continue;
                var prmKeyType = prop.PropertyType;
                var primKey = prmKeyType.GetProperties().Select(p =>
                    {
                        var attrs = p.GetCustomAttributes(typeof (KeyAttribute), true);
                        return attrs.Length > 0 ? p.Name : string.Empty;
                    }).FirstOrDefault(name => !string.IsNullOrEmpty(name));
                
                if (!string.IsNullOrEmpty(primKey))
                    navFieldColumn.Add(prop.Name, primKey);
            }
        }

        public PagedListResult<T> GetItemsPaged(string propertiesToInclude,
            string sortBy, bool ascending, int skip, int take)
        {
            var model = new PagedListResult<T>
                {
                    totalRecordsCount = context.Set<T>().Count()
                };
            string primKeyColumnName;
            navFieldColumn.TryGetValue(sortBy, out primKeyColumnName);

            var query = context.Set<T>().OrderByField(sortBy, ascending, primKeyColumnName)
                               .Skip(skip)
                               .Take(take);
            model.items = string.IsNullOrEmpty(propertiesToInclude) 
                ? query.Cast<T>().ToList()
                : query.Cast<T>().Include(propertiesToInclude).ToList();

            return model;
        }
    }
}
