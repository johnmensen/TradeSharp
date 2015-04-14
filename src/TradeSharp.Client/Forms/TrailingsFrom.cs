using System;
using System.Windows.Forms;
using Entity;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;


namespace TradeSharp.Client.Forms
{
    public partial class TrailingsFrom : Form
    {
        public MarketOrder deal;
        private volatile bool editFlag;
        public TrailingsFrom()
        {
            InitializeComponent();
        }
        
        public TrailingsFrom(MarketOrder d)
        {
            InitializeComponent();
            deal = d;
        }

        private void cbLevel1_CheckedChanged(object sender, EventArgs e)
       { 
            cbLevel2.Enabled = cbLevel1.Checked;
            tbLevel1.Enabled = cbLevel1.Checked;
            tbLevel1Price.Enabled = cbLevel1.Checked;
            tbGoal1.Enabled = cbLevel1.Checked;
            tbGoal1Price.Enabled = cbLevel1.Checked;
        }

        private void cbLevel2_CheckedChanged(object sender, EventArgs e)
        {
            cbLevel3.Enabled = cbLevel2.Checked;
            tbLevel2.Enabled = cbLevel2.Checked;
            tbLevel2Price.Enabled = cbLevel2.Checked;
            tbGoal2.Enabled = cbLevel2.Checked;
            tbGoal2Price.Enabled = cbLevel2.Checked;
            cbLevel1.Enabled = !cbLevel2.Checked;
        }

        private void cbLevel3_CheckedChanged(object sender, EventArgs e)
        {
            cbLevel4.Enabled = cbLevel3.Checked;
            tbLevel3.Enabled = cbLevel3.Checked;
            tbLevel3Price.Enabled = cbLevel3.Checked;
            tbGoal3.Enabled = cbLevel3.Checked;
            tbGoal3Price.Enabled = cbLevel3.Checked;
            cbLevel2.Enabled = !cbLevel3.Checked;
        }

        private void cbLevel4_CheckedChanged(object sender, EventArgs e)
        {
            tbLevel4.Enabled = cbLevel4.Checked;
            tbLevel4Price.Enabled = cbLevel4.Checked;
            tbGoal4.Enabled = cbLevel4.Checked;
            tbGoal4Price.Enabled = cbLevel4.Checked;
            cbLevel3.Enabled = !cbLevel4.Checked;
        }

        private void TrailingsFrom_Load(object sender, EventArgs e)
        {
            if (deal == null) return;
            Text = Text + " на позицию №" + deal.ID;
            if (deal.TrailLevel1 != null && deal.TrailTarget1 != null)
            {
                cbLevel1.Checked = true;
                tbLevel1.Text = deal.TrailLevel1.ToString();
                tbGoal1.Text = deal.TrailTarget1.ToString();
            }
            if (deal.TrailLevel2 != null && deal.TrailTarget2 != null)
            {
                cbLevel2.Checked = true;
                tbLevel2.Text = deal.TrailLevel2.ToString();
                tbGoal2.Text = deal.TrailTarget2.ToString();
            }

            if (deal.TrailLevel3 != null && deal.TrailTarget3 != null)
            {
                cbLevel3.Checked = true;
                tbLevel3.Text = deal.TrailLevel3.ToString();
                tbGoal3.Text = deal.TrailTarget3.ToString();
            }

            if (deal.TrailLevel4 != null && deal.TrailTarget4 != null)
            {
                cbLevel4.Checked = true;
                tbLevel4.Text = deal.TrailLevel4.ToString();
                tbGoal4.Text = deal.TrailTarget4.ToString();
            }


        }

        private void tbLevel1_TextChanged(object sender, EventArgs e)
        {
            PerformPointEdit(tbLevel1Price, tbLevel1);
        }

        private void tbGoal1_TextChanged(object sender, EventArgs e)
        {
            PerformPointEdit(tbGoal1Price, tbGoal1);
        }

        private void tbLevel2_TextChanged(object sender, EventArgs e)
        {
            PerformPointEdit(tbLevel2Price, tbLevel2);
        }

        private void tbGoal2_TextChanged(object sender, EventArgs e)
        {
            PerformPointEdit(tbGoal2Price, tbGoal2);
        }

        private void tbLevel3_TextChanged(object sender, EventArgs e)
        {
            PerformPointEdit(tbLevel3Price, tbLevel3);
        }

        private void tbGoal3_TextChanged(object sender, EventArgs e)
        {
            PerformPointEdit(tbGoal3Price, tbGoal3);
        }

        private void tbLevel4_TextChanged(object sender, EventArgs e)
        {
            PerformPointEdit(tbLevel4Price, tbLevel4);
        }

        private void tbGoal4_TextChanged(object sender, EventArgs e)
        {
            PerformPointEdit(tbGoal4Price, tbGoal4);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (cbLevel1.Checked)
            {
                if (tbLevel1Price.Text != "" && tbGoal1Price.Text != "" && !tbLevel1.Text.Contains("-") && !tbGoal1.Text.Contains("-"))
                {
                    // есть уровень, надо его добавить
                    deal.TrailLevel1 = tbLevel1.Text.ToIntSafe();
                    deal.TrailTarget1 = tbGoal1.Text.ToIntSafe();
                }
                else
                {
                    MessageBox.Show(string.Format(Localizer.GetString("MessageTrailinfLevelNWrong"), 1), 
                        Localizer.GetString("TitleError"));
                    return;
                }
            }
            else
            {
                deal.TrailLevel1 = null;
                deal.TrailTarget1 = null;
            }

            if (cbLevel2.Checked)
            {
                if (tbLevel2Price.Text != "" && tbGoal2Price.Text != "" && !tbLevel2.Text.Contains("-") && !tbGoal2.Text.Contains("-"))
                {
                    // есть уровень, надо его добавить
                    deal.TrailLevel2 = tbLevel2.Text.ToIntSafe();
                    deal.TrailTarget2 = tbGoal2.Text.ToIntSafe();
                }
                else
                {
                    MessageBox.Show(string.Format(Localizer.GetString("MessageTrailinfLevelNWrong"), 2),
                        Localizer.GetString("TitleError"));
                    return;
                }
            }
            else
            {
                deal.TrailLevel2 = null;
                deal.TrailTarget2 = null;
            }

            if (cbLevel3.Checked)
            {
                if (tbLevel3Price.Text != "" && tbGoal3Price.Text != "" && !tbLevel3.Text.Contains("-") && !tbGoal3.Text.Contains("-"))
                {
                    // есть уровень, надо его добавить
                    deal.TrailLevel3 = tbLevel3.Text.ToIntSafe();
                    deal.TrailTarget3 = tbGoal3.Text.ToIntSafe();
                }
                else
                {
                    MessageBox.Show(string.Format(Localizer.GetString("MessageTrailinfLevelNWrong"), 3),
                        Localizer.GetString("TitleError"));
                    return;
                }
            }
            else
            {
                deal.TrailLevel3 = null;
                deal.TrailTarget3 = null;
            }

            if (cbLevel4.Checked)
            {
                if (tbLevel4Price.Text != "" && tbGoal4Price.Text != "" && !tbLevel4.Text.Contains("-") && !tbGoal4.Text.Contains("-"))
                {
                    // есть уровень, надо его добавить
                    deal.TrailLevel4 = tbLevel4.Text.ToIntSafe();
                    deal.TrailTarget4 = tbGoal4.Text.ToIntSafe();
                }
                else
                {
                    MessageBox.Show(string.Format(Localizer.GetString("MessageTrailinfLevelNWrong"), 4),
                        Localizer.GetString("TitleError"));
                    return;
                }
            }
            else
            {
                deal.TrailLevel4 = null;
                deal.TrailTarget4 = null;
            }

            try
            {
                MainForm.Instance.SendEditMarketRequestSafe(deal);
            }
            catch (Exception ex)
            {
                Logger.Error("Server proxy: SendEditMarketRequest error: ", ex);
                MessageBox.Show(Localizer.GetString("MessageUnableToDeliverRequest"),
                                Localizer.GetString("TitleError"), 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            DialogResult = DialogResult.OK;
        }

        private void PerformPointEdit(TextBox tbPrice, TextBox tbPoints)
        {
            try
            {
                if (editFlag) return;
                editFlag = true;
                var points = tbPoints.Text.ToIntSafe();
                if (points == null)
                {
                    tbPrice.Text = "";
                    editFlag = false;
                    return;
                }
                if (points < 0)
                {
                    points *= -1;
                    tbPoints.Text = points.ToString();
                }
                var price = deal.PriceEnter + DalSpot.Instance.GetAbsValue(deal.Symbol, (float)points) * deal.Side;
                tbPrice.Text = price.ToString();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                editFlag = false;
            }
        }
        private void PerformPriceEdit(TextBox tbPrice, TextBox tbPoints)
        {
            try
            {
                if (editFlag) return;
                editFlag = true;
                var price = tbPrice.Text.ToFloatUniformSafe();
                if (price == null)
                {
                    tbPoints.Text = "";
                    editFlag = false;
                    return;
                }
                var points = (int)DalSpot.Instance.GetPointsValue(deal.Symbol, (float)price - deal.PriceEnter) * deal.Side;
                //if (points < 0)
                //{
                //    tbPrice.Text = deal.PriceEnter.ToString();
                //    tbPoints.Text = "0";
                //    editFlag = false;
                //    return;
                //}
                tbPoints.Text = points.ToString();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                editFlag = false;
            }
        }

        private void tbLevel1Price_TextChanged(object sender, EventArgs e)
        {
            PerformPriceEdit(tbLevel1Price, tbLevel1);
        }

        private void tbLevel2Price_TextChanged(object sender, EventArgs e)
        {
            PerformPriceEdit(tbLevel2Price, tbLevel2);
        }

        private void tbLevel3Price_TextChanged(object sender, EventArgs e)
        {
            PerformPriceEdit(tbLevel3Price, tbLevel3);
        }

        private void tbLevel4Price_TextChanged(object sender, EventArgs e)
        {
            PerformPriceEdit(tbLevel4Price, tbLevel4);
        }

        private void tbGoal1Price_TextChanged(object sender, EventArgs e)
        {
            PerformPriceEdit(tbGoal1Price, tbGoal1);
        }

        private void tbGoal2Price_TextChanged(object sender, EventArgs e)
        {
            PerformPriceEdit(tbGoal2Price, tbGoal2);
        }

        private void tbGoal3Price_TextChanged(object sender, EventArgs e)
        {
            PerformPriceEdit(tbGoal3Price, tbGoal3);
        }

        private void tbGoal4Price_TextChanged(object sender, EventArgs e)
        {
            PerformPriceEdit(tbGoal4Price, tbGoal4);
        }
    }
}
