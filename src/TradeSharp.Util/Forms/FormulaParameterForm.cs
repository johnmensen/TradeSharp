using System.Collections.Generic;
using System.Windows.Forms;

namespace TradeSharp.Util.Forms
{
    public partial class FormulaParameterForm : Form
    {
        public BaseCodeTemplate CodeTemplate { get; set; }
        private readonly List<Control> paramEditors = new List<Control>();
        /// <summary>
        /// последний из выбранных пользователем шаблонов
        /// применяется для именования, например, индикатора "Пользовательский индекс"
        /// </summary>
        public static string lastUsedTemplate;

        public FormulaParameterForm()
        {
            InitializeComponent();
        }

        public FormulaParameterForm(BaseCodeTemplate template)
        {
            InitializeComponent();
            CodeTemplate = template;
            InitEditors();
        }

        public void InitEditors()
        {
            const int startX = 20, labelWd = 170, labelHt = 18, editorWd = 100, editorLeft = 198;
            const int startY = 20, deltaY = 22;
            var row = 0;
            
            foreach (var ptr in CodeTemplate.parameters.Values)
            {
                var top = startY + (row++)*deltaY;
                // подпись
                var lbl = new Label {Text = ptr.Title, Parent = panelControls};
                lbl.SetBounds(startX, top, labelWd, labelHt);
                panelControls.Controls.Add(lbl);

                // редактор значения
                if (ptr.IsNumber || ptr.IsString)
                {
                    var textBox = new TextBox {Parent = panelControls};
                    textBox.SetBounds(editorLeft, top, editorWd, labelHt + 2);
                    panelControls.Controls.Add(textBox);
                    paramEditors.Add(textBox);
                }
                if (ptr.IsBoolean)
                {
                    var checkBox = new CheckBox {Parent = panelControls, Left = editorLeft, Top = top};
                    panelControls.Controls.Add(checkBox);
                    paramEditors.Add(checkBox);
                }
            }
        }

        private void BtnOkClick(object sender, System.EventArgs e)
        {
            // инициализировать значения параметров
            int i = 0;            
            foreach (var ptrKey in CodeTemplate.parameters.Keys)
            {
                var ptr = CodeTemplate.parameters[ptrKey];
                var editor = paramEditors[i++];
                
                if (ptr.IsString) CodeTemplate.parameters[ptrKey].ValueStr = editor.Text;
                if (ptr.IsNumber) CodeTemplate.parameters[ptrKey].ValueNum = editor.Text.ToDoubleUniformSafe() ?? 0;
                if (ptr.IsBoolean) CodeTemplate.parameters[ptrKey].ValueBit = ((CheckBox) editor).Checked;                                
            }
            lastUsedTemplate = CodeTemplate.MakeTitleByParams();
        }

        private void BtnCancelClick(object sender, System.EventArgs e)
        {
            lastUsedTemplate = string.Empty;
        }
    }
}
