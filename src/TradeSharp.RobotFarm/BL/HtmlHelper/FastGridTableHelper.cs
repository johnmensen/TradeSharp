using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace TradeSharp.RobotFarm.BL.HtmlHelper
{
    /// <summary>
    /// генерируют html разметку этой таблици для FastGrid
    /// </summary>
    public class FastGridTableHelper : HtmlHelper
    {
        #region Поля
        /// <summary>
        /// Имя JavaScript функции заполнения таблици
        /// </summary>
        public string FillTableFuncName { get; private set; }

        /// <summary>
        /// Имя JavaScript функции
        /// </summary>
        public string GetRowValuesFuncName { get; private set; }

        /// <summary>
        /// Имя JavaScript функции
        /// </summary>
        public string GetUrlEncodedRowValuesFuncName { get; private set; }

        /// <summary>
        /// Имя JavaScript массива, значениями которого заполненяется таблици
        /// </summary>
        public string TableArrayName { get; private set; }

        /// <summary>
        /// ссылка на объект типа FastGrid по образу которого генерируется Html разметка таблици
        /// </summary>
        private FastGrid.FastGrid PrototypeGrid { get; set; }

        /// <summary>
        /// Уникальное имя таблици
        /// </summary>
        private readonly string tableName;

        /// <summary>
        /// Следует ли отрисовывать заголовок таблици
        /// </summary>
        private readonly bool renderHeader;    
        #endregion

        /// <param name="tableId">Уникальное имя генерируемой таблци</param>
        /// <param name="grid">Таблица, html разметку которой нужно сгенерировать</param>
        /// <param name="renderHeader">Отрисовывать ли заголовок таблици</param>
        public FastGridTableHelper(string tableId, FastGrid.FastGrid grid, bool renderHeader)
        {
            PrototypeGrid = grid;
            tableName = tableId;
            this.renderHeader = renderHeader;

            // Задаём названия функций
            FillTableFuncName = "Fill" + tableId;
            TableArrayName = tableId + "Items";
            GetRowValuesFuncName = "GetRowValues" + tableId;
            GetUrlEncodedRowValuesFuncName = "GetUrlEncodedRowValues" + tableId;
        }

        /// <summary>
        /// Метод формирует из таблици prototypeGrid массив для Javascript
        /// </summary>
        public string GetFormatItemArray()
        {
            var result = new StringBuilder();
            var source = GetTableItemArray();
            foreach (var strChar in source)
            {
                switch (strChar)
                {
                    case '\'':
                        result.Append("\"");
                        break;
                    case '\"':
                        result.Append("\'");
                        break;
                    default:
                        result.Append(strChar);
                        break;
                }
            }
            return "[" + result + "]";
        }

        protected override void MakeHtmlMarkup()
        {
            throw new ArgumentNullException(); //TODO возможо тут нужно сделать как то более изящно
        }

        protected override void MakeHtmlMarkup(IList source)
        {
            PrototypeGrid.DataBind(source);
            
            // Генерируем таблицу
            markupSB.AppendLine("<table id=\"" + tableName + "\" ");
            MakeExtendedHtmlMarkup();
            markupSB.AppendLine(" >");

            // Генерируем разметку "шапки" талици счетов (на этом пока таблица заканчивается). Дальше JavaScript функцией таблица заполниться
            if (renderHeader)
            {
                markupSB.AppendLine("<tr>");
                foreach (var col in PrototypeGrid.Columns)
                    markupSB.AppendFormat("<th>{0}</th>", col.Title); // minWidth, fixedWidth, sorting etc
                markupSB.Append("</tr>");
            }
            markupSB.AppendLine("</table>");
            //=================================

            GetJaveScriptFunc();
        }

        /// <summary>
        /// В зависимости от additionalAttribute генерирует дополнительные атрибуты разметки
        /// </summary>
        private void MakeExtendedHtmlMarkup()
        {
            if (additionalAttribute.Count == 0) return;
            foreach (var attribute in additionalAttribute)
            {
                switch (attribute.Key)
                {
                    // Id уже задан в самом начале
                    case HtmlAttribute.Style:
                        markupSB.Append(" style=\"" + attribute.Value + "\" ");
                        break;
                    case HtmlAttribute.Class:
                        markupSB.Append(" class=\"" + attribute.Value + "\" ");
                        break;
                    case HtmlAttribute.Name:
                        markupSB.Append(" name=\"" + attribute.Value + "\" ");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Генерирует JavaScript для таблици (функции заполнение и т.п.)
        /// </summary>
        private void GetJaveScriptFunc()
        {
            markupSB.AppendLine("         \r\n<script type=\"text/javascript\">");

            // Генерируем javaScript массив с содержанием столбцов, генерируемой таблици
            markupSB.AppendLine("         var " + TableArrayName + " = [");
            markupSB.Append(GetTableItemArray());
            markupSB.Append("];");
            
            // добавляем в разметку функции для заполнения и управления таблицой
            JScrFillAccountTable();
            JScrGetRowValues();
            JScrEncodedRowValues();

            // Вызов функции заполнения таблици из сгенерированного массива
            markupSB.AppendLine(FillTableFuncName + "();");

            markupSB.AppendLine("         </script>");
        }

        /// <summary>
        /// вспомогательный метод. генерирующий массив для JavaScript
        /// </summary>
        private string GetTableItemArray()
        {
            var result = new StringBuilder();
            if (PrototypeGrid == null || PrototypeGrid.rows.Count == 0) return string.Empty;

            foreach (var row in PrototypeGrid.rows)
            {
                result.Append("[");
                result.Append(string.Join(", ", row.cells.Select(c => "\'" + c.CellString + "\'")));
                result.Append("]");
            }
            result.Replace("][", "], [");
            return result.ToString();
        }

        #region Генерирование JavaScript функций
        /// <summary>
        /// Генерирование JavaScript функции заполнения таблици
        /// </summary>
        private void JScrFillAccountTable()
        {
            markupSB.AppendLine("         \r\n\r\n function " + FillTableFuncName + "() {"+
                              "         $(\"#" + tableName + "\").find(\"tr:gt(0)\").remove();" +
                              "           var table = document.getElementById('" + tableName + "');\r\n" +
                              "           for (var i = 0; i < " + TableArrayName + ".length; i++) {\r\n" +
                              "             var cells = " + TableArrayName + "[i];\r\n  " +
                              "             var row = table.insertRow(i + 1); \r\n" +
                              "             for (var j = 0; j < cells.length; j++) {\r\n" +
                              "                var cell = row.insertCell(j);\r\n  " +
                              "                cell.innerHTML = cells[j];\r\n" +
                              "             }\r\n" +
                              "           }\r\n" +
                              "         }\r\n\r\n");
        }

        /// <summary>
        /// генерирует текст функции JavaScript, которая возвращает в виде массива значения в контролах 'input' переданной строки
        /// (только в первом контроле в каждой ячейки)
        /// </summary>
        private void JScrGetRowValues()
        {
            markupSB.AppendLine("         \r\n\r\n function " + GetRowValuesFuncName + "(row) {");
            markupSB.AppendLine("         var cells = row.getElementsByTagName('td');\r\n" +
                              "         var valueArray = [cells.length];\r\n" +
                              "         for (var ic = 0, it = cells.length; ic < it; ic++) {\r\n" +
                              "             var cell = cells[ic];\r\n" +
                              "             var input = cell.getElementsByTagName('input');\r\n" +
                              "             if (input.length)\r\n" +
                              "             if (input[0])\r\n" +
                              "                 valueArray[ic] = input[0].value;\r\n" +
                              "         }" +
                              "         return valueArray;" +
                              "}\r\n");
        }

        /// <summary>
        /// генерирует текст функции JavaScript, которая возвращает в виде строки (зашифрованной в escep последвательность)
        /// значения в контролах 'input' переданной строки
        /// </summary>
        private void JScrEncodedRowValues()
        {
            markupSB.AppendLine("    \r\n\r\n function " + GetUrlEncodedRowValuesFuncName + "(row) {");
            markupSB.AppendLine("    var vals = " + GetRowValuesFuncName + "(row);\r\n" +
                              "         var str = '';\r\n" +
                              "         if (!vals || vals.length == 0) return str;\r\n" +
                              "         for (var i = 0; i < vals.length; i++) {\r\n" +
                              "             str = str + 'col' + i + '=' + encodeURIComponent(vals[i]) + '&';\r\n" +
                              "         }\r\n" +
                              "         return str;\r\n}\r\n");
        }
        #endregion
    }
}