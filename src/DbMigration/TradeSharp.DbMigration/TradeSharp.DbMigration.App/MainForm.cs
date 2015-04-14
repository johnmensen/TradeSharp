using System;
using System.Diagnostics;
using System.Windows.Forms;
using TradeSharp.DbMigration.Core;

namespace TradeSharp.DbMigration.App
{
    public partial class MainForm : Form
    {
        private readonly BaseMigrator[] migrators = new BaseMigrator[]
            {
                new TradeSharpMigrator("TradeSharp"),
                new TradeSharpMigrator("Hub")
            };

        public MainForm()
        {
            InitializeComponent();

            cbDatabase.DataSource = migrators;
            cbDatabase.SelectedIndex = 0;
        }

        private void cbDatabase_SelectedIndexChanged(object sender, EventArgs e)
        {
            var migrator = (TradeSharpMigrator) cbDatabase.SelectedItem;
            tbConnectionString.Text = migrator.ConnectionString;
            lblCurVersion.Text = migrator.GetLatestVersion().ToString();
            var migrations = migrator.GetMigrations();
            lbRevision.DataSource = migrations;
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            var migrator = (TradeSharpMigrator)cbDatabase.SelectedItem;

            try
            {
                long i = -1;
                if (lbRevision.SelectedItem != null)
                {
                    var str = (string)lbRevision.SelectedItem;
                    string[] arrstr = str.Split(' ');
                    i = Convert.ToInt64(arrstr[0]);
                }
                migrator.Migrate(i);
                lblCurVersion.Text = migrator.GetLatestVersion().ToString();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void btnLog_Click(object sender, EventArgs e)
        {
            Process.Start("notepad.exe", "Logs\\Information.log");
        }
    }
}
