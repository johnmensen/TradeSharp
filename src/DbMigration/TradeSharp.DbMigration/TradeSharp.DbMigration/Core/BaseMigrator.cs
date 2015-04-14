using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using ECM7.Migrator;
using TradeSharp.Util;

namespace TradeSharp.DbMigration.Core
{
    public abstract class BaseMigrator : IMyMigrator
    {
        public string MigratorFriendlyName { get; set; }
        
        private string connectionString;
        private readonly string iniPath = ExecutablePath.ExecPath + "\\settings.ini";

        public string ConnectionString
        {
            get
            {
                if (!string.IsNullOrEmpty(connectionString)) return connectionString;
                var values = new IniFile(iniPath).ReadSection(MigratorFriendlyName);
                values.TryGetValue("connectionString", out connectionString);
                return connectionString;
            }
            set
            {
                connectionString = value;
                new IniFile(iniPath).WriteSection(MigratorFriendlyName, new Dictionary<string, string>
                    {
                        {"connectionString", connectionString}
                    });
            }
        }

        protected BaseMigrator(string migratorName)
        {
            MigratorFriendlyName = migratorName;
        }

        public abstract bool CheckConnection();

        public List<string> GetMigrations()
        {
            var res = new List<string>();
            try
            {
                var migrator = GetMigrator();
                res.AddRange(migrator.AvailableMigrations
                    .Select(migration => string.Format("{0} :     {1} WithTransaction:{2}",
                        migration.Version, migration.Type.Name, !migration.WithoutTransaction)));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                //MessageBox.Show(ex.Message);
            }
            return res;
        }

        public long GetLatestVersion()
        {
            try
            {
                var migrator = GetMigrator();
                var list = migrator.GetAppliedMigrations();
                return list.Count == 0 ? -1 : list.Last();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                MessageBox.Show(ex.Message);
            }
            return -1;
        }

        public void Migrate(long i)
        {
            try
            {
                var migrator = GetMigrator();
                migrator.Migrate(i);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                MessageBox.Show(ex.Message);
            }
        }

        protected abstract Migrator GetMigrator();

        public override string ToString()
        {
            return MigratorFriendlyName;
        }
    }
}
