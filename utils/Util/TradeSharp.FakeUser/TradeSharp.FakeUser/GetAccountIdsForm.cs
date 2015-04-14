using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TradeSharp.Linq;

namespace TradeSharp.FakeUser
{
    public partial class GetAccountIdsForm : Form
    {
        private List<int> accountIds = new List<int>();

        public List<int> AccountIds
        {
            get { return accountIds; }
        }

        public GetAccountIdsForm()
        {
            InitializeComponent();
        }

        public GetAccountIdsForm(string[] accountGroups) : this()
        {
            cbGroup.DataSource = accountGroups;
            if (cbGroup.Items.Count > 0)
                cbGroup.SelectedIndex = 0;
            dpTimeStart.Value = DateTime.Now.Date.AddMonths(-1);
            dpTimeEnd.Value = DateTime.Now;
        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
            using (var ctx = DatabaseContext.Instance.Make())
            {
                accountIds = (from u in ctx.PLATFORM_USER
                                  join pa in ctx.PLATFORM_USER_ACCOUNT on u.ID equals pa.PlatformUser
                                  join a in ctx.ACCOUNT on pa.Account equals a.ID
                                  where a.AccountGroup == (string)cbGroup.SelectedItem && 
                                    a.TimeCreated >= dpTimeStart.Value && a.TimeCreated <= dpTimeEnd.Value &&
                                    (!cbCheckPassword.Checked || u.Password == tbPassword.Text)
                                  select a.ID).ToList();
            }
            MessageBox.Show("Найдено " + accountIds.Count + " счетов");
        }

        private void btnCopyAndClose_Click(object sender, EventArgs e)
        {
            if (accountIds.Count == 0) return;
            Clipboard.SetText(string.Join(", ", accountIds));
            DialogResult = DialogResult.OK;
        }
    }
}
