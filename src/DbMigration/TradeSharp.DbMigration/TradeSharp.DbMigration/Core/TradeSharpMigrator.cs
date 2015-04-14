using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using ECM7.Migrator;
using SharpInitMigration = TradeSharp.DbMigration.TradeSharp.InitMigration;
using HubInitMigration = TradeSharp.DbMigration.Hub.InitMigration;

namespace TradeSharp.DbMigration.Core
{
    public class TradeSharpMigrator : BaseMigrator
    {
        private readonly string migratorName;
        private static readonly Dictionary<string, Assembly>
            assemblyByName = new Dictionary<string, Assembly>();

        static TradeSharpMigrator()
        {
            assemblyByName.Add("TradeSharp", typeof(SharpInitMigration).Assembly);
            assemblyByName.Add("Hub", typeof(HubInitMigration).Assembly);
        }

        public TradeSharpMigrator(string migratorName)
            : base(migratorName)
        {
            this.migratorName = migratorName;
        }       

        public override bool CheckConnection()
        {
            using (var connection = new SqlConnection())
            {
                try
                {
                    connection.ConnectionString = ConnectionString;
                    connection.Open();
                    var str = string.Format("Ok  State: {0}", connection.State);
                    Debug.WriteLine(str);
                    MessageBox.Show(str);
                    return true;               
                }
                catch (Exception ex)
                {
                    var str = string.Format("Error: {0}", ex.Message);
                    Debug.WriteLine(str);
                    MessageBox.Show(str);
                    return false;
                }
            }
        }

        protected override Migrator GetMigrator()
        {
            var migrator = new Migrator("SqlServer",
                ConnectionString,
                assemblyByName[migratorName]);
            return migrator;
        }
    }
}
