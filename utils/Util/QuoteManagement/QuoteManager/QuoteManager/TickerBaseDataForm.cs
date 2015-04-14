using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MTS.Live.Util;

namespace QuoteManager
{
    public partial class TickerBaseDataForm : Form
    {
        private readonly string ticker;
        private readonly int tickerId;

        public TickerBaseDataForm()
        {
            InitializeComponent();
        }

        public TickerBaseDataForm(string ticker, int tickerId)
        {
            InitializeComponent();
            this.ticker = ticker;
            this.tickerId = tickerId;
            Text = string.Format("{0} [ID:{1}] в БД", ticker, tickerId);
        }

        private void BtnReadStartEndClick(object sender, EventArgs e)
        {
            try
            {
                var connectionString = ConfigurationManager.ConnectionStrings["QuoteBase"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    var commandText = string.Format("select min(date) from QUOTE where trd_currency='{0}'",
                                                    tickerId);
                    var command = new SqlCommand(commandText, connection);
                    connection.Open();
                    var objTime = command.ExecuteScalar();
                    if (objTime != null)
                        dpStart.Value = (DateTime)objTime;
                    else
                        MessageBox.Show("Нет ответа от БД");

                    commandText = string.Format("select max(date) from QUOTE where trd_currency='{0}'",
                                                    tickerId);
                    command = new SqlCommand(commandText, connection);
                    objTime = command.ExecuteScalar();
                    if (objTime != null)
                        dpEnd.Value = (DateTime)objTime;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка обращения к БД", ex);
                MessageBox.Show(string.Format("Ошибка обращения к БД: {0}", ex.Message));
                return;
            }            
        }

        private void DeleteQuotesOnInterval(DateTime? start, DateTime? end)
        {
            if (MessageBox.Show("Удалить котировки в базе данных?", "Запрос",
                MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.No) return;

            var cmd = string.Format("delete from QUOTE where trd_currency='{0}'",
                                                    tickerId);
            if (start.HasValue && end.HasValue)
                cmd = string.Format(
                    "delete from QUOTE where trd_currency='{0}' and date between '{1:yyyyMMdd HH:mm:ss}' and '{2:yyyyMMdd HH:mm:ss}'",
                                                    tickerId, start.Value, end.Value);
            else if (start.HasValue)
                cmd = string.Format(
                    "delete from QUOTE where trd_currency='{0}' and date >= '{1:yyyyMMdd HH:mm:ss}'",
                                                    tickerId, start.Value);
            else if (end.HasValue)
                cmd = string.Format(
                    "delete from QUOTE where trd_currency='{0}' and date <= '{1:yyyyMMdd HH:mm:ss}'",
                                                    tickerId, end.Value);
            var connectionString = ConfigurationManager.ConnectionStrings["QuoteBase"].ConnectionString;
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    var command = new SqlCommand(cmd, connection);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка обращения к БД", ex);
                MessageBox.Show(string.Format("Ошибка обращения к БД: {0}", ex.Message));
                return;
            }
            MessageBox.Show("Котировки удалены");
        }

        private void BtnDeleteInterClick(object sender, EventArgs e)
        {
            DeleteQuotesOnInterval(dpStart.Value, dpEnd.Value);
        }

        private void BtnDeleteFromBeginClick(object sender, EventArgs e)
        {
            DeleteQuotesOnInterval(dpStart.Value, null);
        }

        private void BtnDeleteToEndClick(object sender, EventArgs e)
        {
            DeleteQuotesOnInterval(null, dpStart.Value);
        }

        private void BtnDeleteAllClick(object sender, EventArgs e)
        {
            DeleteQuotesOnInterval(null, null);
        }
    }
}
