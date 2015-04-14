using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using FastGrid;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Client.Subscription.Dialog
{
    public partial class FunctionCheckForm : Form
    {
        class ExpressionParameter
        {
            public string Name { get; set; }

            public double Value { get; set; }

            public string Description { get; set; }
        }

        private readonly ExpressionResolver resolver;

        public FunctionCheckForm()
        {
            InitializeComponent();
        }

        public FunctionCheckForm(string function) : this()
        {
            lblFormula.Text = function;
            resolver = new ExpressionResolver(function);
        }

        private void FunctionCheckFormLoad(object sender, EventArgs e)
        {
            SetupGrid();
            
            var ptrs = new List<ExpressionParameter>();
            // ReSharper disable LoopCanBeConvertedToQuery
            foreach (var var in resolver.GetVariableNames())
            // ReSharper restore LoopCanBeConvertedToQuery
            {
                var varName = var;
                var field = PerformerStatField.fields.FirstOrDefault(f => 
                    !string.IsNullOrEmpty(f.ExpressionParamName) && f.ExpressionParamName.Equals(
                    varName, StringComparison.OrdinalIgnoreCase));
                var fieldTitle = field == null ? "" : field.ExpressionParamTitle;
                var ptr = new ExpressionParameter
                    {
                        Name = var,
                        Description = fieldTitle
                    };
                ptrs.Add(ptr);
            }
            grid.DataBind(ptrs);
        }

        private void SetupGrid()
        {
            grid.Columns.Add(new FastColumn("Name", "Парам.")
                {
                    ColumnWidth = 50,
                    SortOrder = FastColumnSort.Ascending
                });
            grid.Columns.Add(new FastColumn("Value", "Значение")
            {
                ColumnWidth = 65,
                IsEditable = true                
            });
            grid.Columns.Add(new FastColumn("Description", "Описание"));
            grid.CalcSetTableMinWidth();
        }

        private void BtnResultClick(object sender, EventArgs e)
        {
            // посчитать результат
            var ptrVal = grid.GetRowValues<ExpressionParameter>(false).ToDictionary(p => p.Name, p => p.Value);

            try
            {
                double rst;
                if (!resolver.Calculate(ptrVal, out rst))
                {
                    lblResult.Text = "?";
                    return;
                }
                lblResult.Text = rst.ToString();
            }
            catch (Exception ex)
            {
                lblResult.Text = ex.Message;
            }
        }
    }
}
