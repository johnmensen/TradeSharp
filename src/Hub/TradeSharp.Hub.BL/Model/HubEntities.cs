using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace TradeSharp.Hub.BL.Model
{
    public class HubEntities : DbContext
    {
        public HubEntities()
            : base("name=HubEntities")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Entity<ServerInstance>().HasRequired(a => a.Currency);
            modelBuilder.Entity<Ticker>().HasRequired(w => w.BaseCurrency).WithMany().Map(m => m.MapKey("Currency_Code"));
        }

        public DbSet<Currency> Currency { get; set; }

        public DbSet<Ticker> Ticker { get; set; } 

        public DbSet<ServerInstance> ServerInstance { get; set; }

        public DbSet<BrokerVolume> BrokerVolume { get; set; }

        public DbSet<TickerAlias> TickerAlias { get; set; }
    }
}
