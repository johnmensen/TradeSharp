using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Candlechart.Series;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Controls
{
    public partial class ChartCommentEditDialog : Form
    {
        private readonly ChartComment comment;

        public ChartCommentEditDialog()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
        }

        public ChartCommentEditDialog(ChartComment obj, CandleChartControl chart) : this()
        {
            comment = obj;//new ChartComment(); ChartComment.Copy(comment, obj);
            
            // отобразить содержание полей
            tbText.Text = comment.Text;
            btnColorPicker.BackColor = comment.ColorText;
            tbText.ForeColor = comment.ColorText;
            tbText.BackColor = chart.chart.BackColor;
            cbShowArrow.Checked = !comment.HideArrow;
            cbShowFrame.Checked = comment.DrawFrame;
            btnColorLine.ForeColor = comment.Color;
            btnColorFill.ForeColor = comment.ColorFill;
            tbMagic.Text = comment.Magic.ToString();
            transparencyControl.Transparency = comment.FillTransparency;

            // настройки выпадающих списков
            //cbTextStyle.SelectedIndex = SeriesComment.FontBold
            var patterns = new CommentaryPatternsList().GetStandardValues();
            if (patterns == null)
                return;
            var items = new object[patterns.Count];
            patterns.CopyTo(items, 0);
            cbTemplates.Items.AddRange(items);
        }

        private void BtnColorPickerClick(object sender, System.EventArgs e)
        {
            colorDialog.Color = btnColorPicker.BackColor;
            if (colorDialog.ShowDialog() != DialogResult.OK) return;
            btnColorPicker.BackColor = colorDialog.Color;

            // назначить цвет строке
            SwitchLineStyle(FontStyle.Regular, colorDialog.Color, true);
        }

        private void BtnColorLineClick(object sender, System.EventArgs e)
        {
            var btn = (Button) sender;
            colorDialogLineFill.Color = btn.ForeColor;
            if (colorDialogLineFill.ShowDialog() == DialogResult.OK)
                btn.ForeColor = colorDialogLineFill.Color;
        }

        private void BtnOkClick(object sender, System.EventArgs e)
        {
            comment.Text = tbText.Text;
            comment.ColorText = btnColorPicker.BackColor;
            comment.ColorText = tbText.ForeColor;
            comment.HideArrow = !cbShowArrow.Checked;
            comment.DrawFrame = cbShowFrame.Checked;
            comment.Color = btnColorLine.ForeColor;
            comment.ColorFill = btnColorFill.ForeColor;
            comment.Magic = tbMagic.Text.ToIntSafe() ?? 0;
            comment.FillTransparency = transparencyControl.Transparency;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void CbTemplatesSelectedIndexChanged(object sender, System.EventArgs e)
        {
            var selText = (string) cbTemplates.SelectedItem;
            if (!string.IsNullOrEmpty(selText))
                tbText.Text = selText;
        }

        private bool SwitchLineStyle(FontStyle style, Color? colorNew, bool changeColor)
        {
            tbText.SelectionChanged -= TbTextSelectionChanged;
            
            var cursorPosition = tbText.SelectionStart;
            var lineIndex = tbText.GetLineFromCharIndex(cursorPosition);
            if (lineIndex < 0 || lineIndex >= tbText.Lines.Length) return false;
            var lineText = tbText.Lines[lineIndex];
            
            FontStyle oldStyle;
            Color? color;
            var linePure = GraphicsExtensions.GetLineModifiers(lineText, out oldStyle, out color);
            var hadThisStyle = (oldStyle & style) == style;

            // вернуть линии все модификаторы, но добавить/убрать указанный
            if (hadThisStyle)
                oldStyle = oldStyle & ~style;
            else
                oldStyle = oldStyle | style;

            // форматировать линию обратно
            var line = GraphicsExtensions.FormatLine(linePure, oldStyle, 
                changeColor ? colorNew : color);

            // запихнуть линию в текст
            var oldLines = tbText.Lines.ToArray();
            oldLines[lineIndex] = line;
            tbText.Lines = oldLines;
            
            tbText.SelectionChanged += TbTextSelectionChanged;
            
            return hadThisStyle;
        }
        
        private void CbStyleBoldCheckedChanged(object sender, System.EventArgs e)
        {
            var senderBtn = (CheckBox) sender;
            var targetStyle = senderBtn == cbStyleBold
                                  ? FontStyle.Bold
                                  : senderBtn == cbStyleItalic ? FontStyle.Italic : FontStyle.Underline;

            senderBtn.CheckedChanged -= CbStyleBoldCheckedChanged;
            senderBtn.Checked = !SwitchLineStyle(targetStyle, null, false);
            senderBtn.CheckedChanged += CbStyleBoldCheckedChanged;

            tbText.Focus();
        }

        /// <summary>
        /// обновить статус кнопок цвета и стиля
        /// </summary>
        private void TbTextSelectionChanged(object sender, System.EventArgs e)
        {
            var cursorPosition = tbText.SelectionStart;
            var lineIndex = tbText.GetLineFromCharIndex(cursorPosition);
            if (lineIndex < 0 || lineIndex >= tbText.Lines.Length)
                return;
            var lineText = tbText.Lines[lineIndex];
            FontStyle oldStyle;
            Color? color;
            GraphicsExtensions.GetLineModifiers(lineText, out oldStyle, out color);

            var buttons = new [] {cbStyleBold, cbStyleItalic, cbStyleUnderline};
            foreach (var btn in buttons)
                btn.CheckedChanged -= CbStyleBoldCheckedChanged;

            cbStyleBold.Checked = (oldStyle & FontStyle.Bold) == FontStyle.Bold;
            cbStyleItalic.Checked = (oldStyle & FontStyle.Italic) == FontStyle.Italic;
            cbStyleUnderline.Checked = (oldStyle & FontStyle.Underline) == FontStyle.Underline;
            btnColorPicker.BackColor = color ?? SystemColors.ButtonFace;

            foreach (var btn in buttons)
                btn.CheckedChanged += CbStyleBoldCheckedChanged;
        }

        private void BtnClearColorClick(object sender, System.EventArgs e)
        {
            // убрать цвет
            SwitchLineStyle(FontStyle.Regular, null, true);
        }
    }
}
