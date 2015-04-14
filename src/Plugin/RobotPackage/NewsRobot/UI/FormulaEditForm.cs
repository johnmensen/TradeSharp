using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FastGrid;
using TradeSharp.Util;
using System.Linq;

namespace NewsRobot.UI
{
    public partial class FormulaEditForm : Form
    {
        class FormulaWidthComment
        {
            public string Formula;
            public string Comment;
            public override string ToString()
            {
                return Formula;
            }
        }

        enum TrendDirection
        {
            Вниз = 0,
            Вверх = 1
        }

        class FormulaArgument
        {
            public string Name { get; set; }
            public TrendDirection Value { get; set; }
            public string Comment { get; set; }
        }

        public string Formula;

        public FormulaEditForm()
        {
            InitializeComponent();
            formulaComboBox.Items.Add(new FormulaWidthComment
                {
                    Formula = "(Tba = Tcb) & (Tba != Tn)",
                    Comment = "Тренд до выхода новости (Tba) равен тренду после выхода новости (Tcb).\r\n" +
                        "Новостной сигнал (Tn) отличается от текущего тренда"
                });
            formulaComboBox.Items.Add(new FormulaWidthComment
            {
                Formula = "_true",
                Comment = "Входим в рынок вне зависимости от тренда"
            });
            formulaComboBox.Items.Add(new FormulaWidthComment
            {
                Formula = "(Tba != Tcb) & (Tcb = Tn)",
                Comment = "Тренд до выхода новости (Tba) изменился после выхода новости (тренд Tcb имеет другое направление).\r\n" +
                    "Тренд после выхода новости (Tcb) определяется (равен) направлением новостного сигнала (Tn)"
            });
            argumentsFastGrid.CaptionHeight = 0;
            argumentsFastGrid.Columns.Add(new FastColumn("Name", "Название") { ColumnWidth = 80 });
            argumentsFastGrid.Columns.Add(new FastColumn("Value", "Значение")
                {
                    ColumnWidth = 80,
                    IsEditable = true,
                    IsHyperlinkStyleColumn = true,
                    HyperlinkActiveCursor = Cursors.Hand,
                    HyperlinkFontActive = new Font(Font, FontStyle.Bold),
                    ColorHyperlinkTextActive = Color.Blue,
                });
            argumentsFastGrid.Columns.Add(new FastColumn("Comment", "Комментарий"));
            var arguments = new List<FormulaArgument>();
            foreach (var argument in NewsRobot.formulaArguments)
                arguments.Add(new FormulaArgument { Name = argument.Key, Value = TrendDirection.Вверх, Comment = argument.Value });
            argumentsFastGrid.DataBind(arguments);
        }

        public FormulaEditForm(string formula) : this()
        {
            Formula = formula;
            formulaComboBox.Text = formula;
        }

        private bool CheckFormula()
        {
            Formula = formulaComboBox.Text;
            try
            {
                var resolver = new ExpressionResolver(Formula.ToLower());
                //string absentVars = ""
                string excessVars = "";
                var vars = resolver.GetVariableNames();
                var neededArgs = NewsRobot.formulaArguments.Select(argAndComm => argAndComm.Key.ToLower());
                /*foreach (var argument in neededArgs)
                    if (!vars.Contains(argument))
                    {
                        if (!string.IsNullOrEmpty(absentVars))
                            absentVars += ", ";
                        absentVars += argument;
                    }*/
                foreach (var argument in vars)
                {
                    if(!neededArgs.Contains(argument))
                    {
                        if (!string.IsNullOrEmpty(excessVars))
                            excessVars += ", ";
                        excessVars += argument;
                    }
                }
                if (/*!string.IsNullOrEmpty(absentVars) || */!string.IsNullOrEmpty(excessVars))
                {
                    MessageBox.Show(this,
                                    /*(!string.IsNullOrEmpty(absentVars)
                                         ? "В формуле отсутствуют необходимые параметры: " + absentVars + "\n"
                                         : "") +*/
                                    (!string.IsNullOrEmpty(excessVars)
                                         ? "В формуле присутствуют лишние параметры: " + excessVars
                                         : ""), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }
            }
            catch (Exception)
            {
                MessageBox.Show(this, "Синтаксическая ошибка в формуле", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
            return true;
        }

        private void FormulaComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            if (formulaComboBox.SelectedItem == null)
                return;
            var item = (FormulaWidthComment)formulaComboBox.SelectedItem;
            commentRichTextBox.Text = item.Comment;
        }

        private void FormulaComboBoxTextChanged(object sender, EventArgs e)
        {
            commentRichTextBox.Text = "";
        }

        private void CalcButtonClick(object sender, EventArgs e)
        {
            if (!CheckFormula())
            {
                resultLabel.Text = "Ошибка в формуле";
                return;
            }
            var resolver = new ExpressionResolver(Formula.ToLower());
            var values = new Dictionary<string, double>();
            foreach (var row in argumentsFastGrid.rows)
            {
                var arg = (FormulaArgument) row.ValueObject;
                values.Add(arg.Name.ToLower(), (double)arg.Value);
            }
            double result;
            var resultFlag = resolver.Calculate(values, out result);
            if (!resultFlag)
            {
                resultLabel.Text = "Ошибка в расчете по формуле";
                return;
            }
            resultLabel.Text = result == 0 ? "Нет входа в рынок" : "Вход в рынок";
        }

        private void OkButtonClick(object sender, EventArgs e)
        {
            if (CheckFormula())
                DialogResult = DialogResult.OK;
        }
    }
}
