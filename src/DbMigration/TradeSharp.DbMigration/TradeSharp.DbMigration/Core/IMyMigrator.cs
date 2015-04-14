using System.Collections.Generic;

namespace TradeSharp.DbMigration.Core
{
    public interface IMyMigrator
    {
        string ConnectionString { get; set; }
        bool CheckConnection();
        List<string> GetMigrations();
        long GetLatestVersion();
        void Migrate(long i);
    }
}
