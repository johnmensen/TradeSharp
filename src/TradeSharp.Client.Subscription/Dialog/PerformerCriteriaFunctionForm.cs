using System;
using System.Linq;
using System.Windows.Forms;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;
using TradeSharp.Contract.Util.BL;

namespace TradeSharp.Client.Subscription.Dialog
{
    public partial class PerformerCriteriaFunctionForm : Form
    {
        private PerformerCriteriaFunction lastSelectedItem;

        public PerformerCriteriaFunctionForm()
        {
            InitializeComponent();
            cbSortOrder.SelectedIndex = 0;
        }

        private void PerformerCriteriaFunctionFormLoad(object sender, EventArgs e)
        {
            ListParameters();
            FillFormulas();
        }

        /// <summary>
        /// в "справке" перечислить доступные параметры
        /// </summary>
        private void ListParameters()
        {
            var paramNames = PerformerStatField.fields.Where(f => 
                !string.IsNullOrEmpty(f.ExpressionParamName)).Select(f => f.ExpressionParamName +
                    ": " + f.ExpressionParamTitle).OrderBy(t => t);
            tbVariables.Text = string.Join(Environment.NewLine, paramNames);
        }

        /// <summary>
        /// заполнить комбо-бокс с формулами
        /// </summary>
        private void FillFormulas()
        {
            cbFunction.Items.Clear();
            var selFun = PerformerCriteriaFunctionCollection.Instance.SelectedFunction;
            var selIndex = 0;

            foreach (var crit in PerformerCriteriaFunctionCollection.Instance.criterias)
            {
                cbFunction.Items.Add(crit);
                if (crit == selFun)
                    selIndex = cbFunction.Items.Count - 1;
            }
            cbFunction.SelectedIndex = selIndex;
        }

        private void CbFunctionSelectedIndexChanged(object sender, EventArgs e)
        {
            var item = cbFunction.SelectedItem;
            if (item == null) return;
            var crit = (PerformerCriteriaFunction) item;
            lastSelectedItem = crit;
            tbComment.Text = crit.Description;
            cbMargin.Checked = crit.MarginValue.HasValue;
            tbMarginValue.Enabled = crit.MarginValue.HasValue;
            tbMarginValue.Text = crit.MarginValue.HasValue ? crit.MarginValue.Value.ToString("f2") : "0.00";
        }

        // "save" button
        private void BtnAcceptClick(object sender, EventArgs e)
        {
            // добавить новый критерий или удалить существующий
            var formula = (cbFunction.Text ?? "").Trim();
            if (lastSelectedItem == null && string.IsNullOrEmpty(formula)) return;

            // удалить?
            if (string.IsNullOrEmpty(formula))
            {
                if (MessageBox.Show("Удалить формулу \"" + lastSelectedItem.Function + "\"?", "Подтверждение",
                                    MessageBoxButtons.YesNo) == DialogResult.No) return;
                PerformerCriteriaFunctionCollection.Instance.criterias.Remove(lastSelectedItem);
                PerformerCriteriaFunctionCollection.Instance.WriteToFile();
                FillFormulas();
                return;
            }

            // проверить значение
            string error;
            if (!ExpressionResolver.CheckFunction(formula, out error, 
                PerformerCriteriaFunctionCollection.Instance.enabledParametersNames))
            {
                MessageBox.Show("Формула \"" + formula + "\" не распознана: " + error);
                return;
            }

            // переписать комментарий
            var existItem = PerformerCriteriaFunctionCollection.Instance.criterias.FirstOrDefault(
                c => c.FormulasEqual(formula));
            if (existItem != null)
            {
                existItem.MarginValue = cbMargin.Checked ? tbMarginValue.Text.ToFloatUniformSafe() : null;
                existItem.Description = tbComment.Text;
                PerformerCriteriaFunctionCollection.Instance.WriteToFile();                
                return;
            }

            // добавить новое значение
            var newFunc = new PerformerCriteriaFunction
                {
                    Function = formula,
                    Description = tbComment.Text,
                    PreferredSortOrder = cbSortOrder.SelectedIndex == 0 ? SortOrder.Descending : SortOrder.Ascending,
                    MarginValue =
                        cbMargin.Checked ? tbMarginValue.Text.Trim().Replace(",", ".").ToFloatUniformSafe() : null
                };
            PerformerCriteriaFunctionCollection.Instance.criterias.Add(newFunc);
            PerformerCriteriaFunctionCollection.Instance.SelectedFunction = newFunc;
            PerformerCriteriaFunctionCollection.Instance.WriteToFile();
            FillFormulas();
        }

        private void BtnOkClick(object sender, EventArgs e)
        {
            SaveLastSelectedCriteria();
            MessageBox.Show("Выбрана формула \"" + lastSelectedItem + "\"");
            DialogResult = DialogResult.OK;
        }

        public void SaveLastSelectedCriteria()
        {
            PerformerCriteriaFunctionCollection.Instance.SelectedFunction = lastSelectedItem;
        }

        private void BtnCheckFormulaClick(object sender, EventArgs e)
        {
            string error;
            if (!ExpressionResolver.CheckFunction(cbFunction.Text, out error,
                                                  PerformerCriteriaFunctionCollection.Instance.enabledParametersNames))
            {
                MessageBox.Show(error, "Ошибка в формуле", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // открыть окно проверки формулы подстановкой значений
            new FunctionCheckForm(cbFunction.Text).ShowDialog();
        }

        private void CbMarginCheckedChanged(object sender, EventArgs e)
        {
            tbMarginValue.Enabled = cbMargin.Checked;
        }

        private void BtnDeleteCriteriaClick(object sender, EventArgs e)
        {
            if (lastSelectedItem == null) return;
            // удалить выбранный критерий
            if (!PerformerCriteriaFunctionCollection.Instance.RemoveCriteria(lastSelectedItem.Description)) return;
            FillFormulas();
            PerformerCriteriaFunctionCollection.Instance.WriteToFile();
        }
    }
}
