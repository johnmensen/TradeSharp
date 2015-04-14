using System.Windows.Forms;

namespace TradeSharp.Util.Forms
{
    public partial class FormulaEditorForm : Form
    {
        public FormulaEditorForm()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
            InitTemplates();
        }

        public FormulaEditorForm(string formula)
        {
            InitializeComponent();
            InitTemplates();
            codeEditor.Text = formula;
            //using (var streamCfg = ScintillaConfigurator.GetSyntaxStreamForIndexFormula("formula"))
            //{
            //    var cfg = new Configuration(streamCfg, "formula", true);
            //    codeEditor.ConfigurationManager.Configure(cfg);
            //}
            //codeEditor.ConfigurationManager.Language = "cs";
        }

        public string Formula
        {
            get { return codeEditor.Text; }
        }

        public void InitTemplates()
        {
            cbTemplates.Items.Add(new CodeTemplateIndexSumDelta());
            cbTemplates.Items.Add(new CodeTemplateCurrencyIndex());
            cbTemplates.Items.Add(new CodeTemplateCurrencyIndexROC());
            cbTemplates.Items.Add(new CodeTemplateDetrendedPrice());
            cbTemplates.SelectedIndex = 0;
        }

        private void BtnTemplateStartClick(object sender, System.EventArgs e)
        {
            var dlg = new FormulaParameterForm((BaseCodeTemplate) cbTemplates.SelectedItem);            
            if (dlg.ShowDialog() != DialogResult.OK) return;
            var template = dlg.CodeTemplate;
            codeEditor.Text = template.MakeFormula();
        }
    }    
}
