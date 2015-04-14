using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using TradeSharp.Util;

namespace TradeSharp.Contract.Entity
{
    /// <summary>
    /// описывает свойство класса PerformerStat
    /// </summary>
    public class PerformerStatField
    {
        class ExpressionToken
        {
            public string name;

            public int pos;

            public int len;

            public string replacement;
        }

        public string PropertyName { get; set; }

        public string Title { get; set; }

        public string ExpressionParamName { get; set; }

        public string ExpressionParamTitle { get; set; }

        public string FormatString { get; set; }

        public string FormatSuffix { get; set; }

        public ExpressionOperator? DefaultOperator { get; set; }

        public double? DefaultValue { get; set; }

        public static List<PerformerStatField> fields;

        public ExpressionParameterNameAttribute.StatFieldType FieldType { get; set; }

        private SortOrder defaultSortOrder = SortOrder.Unspecified;
        /// <summary>
        /// кому - вершки, кому - корешки
        /// скажем, если упорядочиваем по AYP (среднегодовой доходности) нам интересно выбрать сначала потолще
        /// а вот если по DD или ML (макс. плечо) - сортируем по-возрастанию, отбираем сначала меньшие значения
        /// </summary>
        public SortOrder DefaultSortOrder
        {
            get { return defaultSortOrder; }
            set { defaultSortOrder = value; }
        }
        
        public string Description { get; set; }

        static PerformerStatField()
        {
            var unsortedFields = new List<Cortege2<PerformerStatField, int>>();            
            
            foreach (var prop in typeof(PerformerStatEx).GetProperties())
            {
                var title = string.Empty;
                var paramTitle = string.Empty;

                // title из ExpressionParameterNameAttribute имеет больший приоритет
                var attrs = prop.GetCustomAttributes(true);
                var exPtrAttr = attrs.FirstOrDefault(a => a is LocalizedExpressionParameterNameAttribute) as ExpressionParameterNameAttribute;
                if (exPtrAttr == null)
                    exPtrAttr = attrs.FirstOrDefault(a => a is ExpressionParameterNameAttribute) as ExpressionParameterNameAttribute;
                if (exPtrAttr != null)
                    paramTitle = exPtrAttr.ParamTitle;
                var dispNameAttr = attrs.FirstOrDefault(a => a is LocalizedDisplayNameAttribute) as DisplayNameAttribute;
                if (dispNameAttr == null)
                    dispNameAttr = attrs.FirstOrDefault(a => a is DisplayNameAttribute) as DisplayNameAttribute;
                if (dispNameAttr != null)
                    title = dispNameAttr.DisplayName;

                if (string.IsNullOrEmpty(title))
                    continue;

                var record = new PerformerStatField { Title = title, PropertyName = prop.Name };
                if (exPtrAttr != null)
                {
                    record.ExpressionParamName = exPtrAttr.ParamName;
                    record.ExpressionParamTitle = string.IsNullOrEmpty(paramTitle) ? title : paramTitle;
                    record.FieldType = exPtrAttr.FieldType;
                    record.FormatSuffix = exPtrAttr.Suffix;
                    record.DefaultSortOrder = exPtrAttr.DefaultSortOrder;
                }

                var formatAttr = attrs.FirstOrDefault(a => a is DisplayFormatAttribute) as DisplayFormatAttribute;
                if (formatAttr != null)
                    record.FormatString = formatAttr.DataFormatString;

                var descrAttr = attrs.FirstOrDefault(a => a is DescriptionAttribute) as DescriptionAttribute;
                if (descrAttr != null)
                    record.Description = descrAttr.Description;

                var defaultAttr = attrs.FirstOrDefault(a => a is DefaultExpressionValuesAttribute) as DefaultExpressionValuesAttribute;
                if (defaultAttr != null)
                {
                    record.DefaultOperator = defaultAttr.Operator;
                    record.DefaultValue = defaultAttr.Value;
                }

                var sortOrder = int.MaxValue;
                var sortAttr = attrs.FirstOrDefault(a => a is PropertyOrderAttribute) as PropertyOrderAttribute;
                if (sortAttr != null)
                    sortOrder = sortAttr.Order;

                unsortedFields.Add(new Cortege2<PerformerStatField, int>(record, sortOrder));
            }

            fields = unsortedFields.OrderBy(f => f.b).Select(f => f.a).ToList();
        }

        public static bool ParseSimpleFormula(string expression,
                                              out List<Cortege3<PerformerStatField, ExpressionOperator, double>> filters,
                                              out PerformerStatField sortField)
        {
            ExpressionResolver resv;
            filters = new List<Cortege3<PerformerStatField, ExpressionOperator, double>>();
            sortField = null;
            try
            {
                resv = new ExpressionResolver(expression);
            }
            catch
            {
                return false;
            }

            // одно из поддеревьев должно быть листом - PerformerStatField
            if (resv.rootOperand == null) return false;
            if (resv.rootOperand.OperandLeft == null &&
                resv.rootOperand.OperandRight == null)
            {
                // частный случай - формула вида "1", "AYP" ...
                var sortVal = resv.rootOperand.IsConstant 
                    ? resv.rootOperand.ConstantValue.ToStringUniform() : resv.rootOperand.VariableName;
                sortField = fields.FirstOrDefault(f => f.ExpressionParamName != null &&
                    f.ExpressionParamName.Equals(sortVal, StringComparison.OrdinalIgnoreCase));
                return sortField != null;
            }

            if (resv.rootOperand.OperandLeft == null ||
                resv.rootOperand.OperandRight == null)
            {
                filters = null;
                return false;
            }

            var root = resv.rootOperand.OperandRight;
            var sorter = resv.rootOperand.OperandLeft;
            if (!sorter.IsVariable)
            {
                sorter = resv.rootOperand.OperandRight;
                root = resv.rootOperand.OperandLeft;
            }
            if (!sorter.IsVariable)
            {
                filters = null;
                return false;
            }
            sortField = fields.FirstOrDefault(f => f.ExpressionParamName != null &&
                f.ExpressionParamName.Equals(sorter.VariableName, StringComparison.OrdinalIgnoreCase));
            if (sortField == null)
            {
                filters = null;
                return false;
            }
            
            // разобрать другое поддерево (root)
            if (root.IsConstant || root.IsVariable)
            {
                filters = null;
                sortField = null;
                return false;
            }
            return ParseSimpleFormulaBranch(root, filters);
        }

        private static bool ParseSimpleFormulaBranch(ExpressionOperand root,
                                                     List<Cortege3<PerformerStatField, ExpressionOperator, double>> filters)
        {
            if (root.Operator == ExpressionOperator.And)
            {
                if (root.OperandLeft.IsConstant || root.OperandLeft.IsConstant) return false;
                if (root.OperandRight.IsConstant || root.OperandRight.IsConstant) return false;
                if (!ParseSimpleFormulaBranch(root.OperandLeft, filters)) return false;
                if (!ParseSimpleFormulaBranch(root.OperandRight, filters)) return false;
                return true;
            }
            if (root.Operator == ExpressionOperator.Greater ||
                root.Operator == ExpressionOperator.Lower ||
                root.Operator == ExpressionOperator.Equal)
            {
                if (!root.OperandLeft.IsVariable) return false;
                if (!root.OperandRight.IsConstant) return false;

                var field = fields.FirstOrDefault(f => f.ExpressionParamName != null &&
                    f.ExpressionParamName.Equals(root.OperandLeft.VariableName, StringComparison.OrdinalIgnoreCase));
                if (field == null) return false;

                filters.Add(new Cortege3<PerformerStatField, ExpressionOperator, double>(field, root.Operator, root.OperandRight.ConstantValue));
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            //return string.IsNullOrEmpty(Description) ? Title : Title + " - " + Description;
            return Title;
        }

        public static string GetFormulaHighlightedHtml(string formula)
        {
            const string header =
                "<html>\n<head>\n" +
                "<meta charset=\"utf-8\" />\n" +
                "<style type=\"text/css\">\n" +
                "    div.code { font-family: 'Courier New'; } \n" +
                " </style>\n" +
                "</head>\n" +
                "<body>\n" +
                "  <div class=\"code\">\n";

            const string footer = "  </div>\n</body>\n</html>";

            if (string.IsNullOrEmpty(formula))
                return header + footer;

            // для каждой переменной формулы создать гиперссылку вида
            // <a href="javascript:void(0);" title="Доход
//Суммарная доходность по счету, %">P</a>+1)*2.5)
            // именно так!

            var resolver = new ExpressionResolver(formula);
            var varNames = resolver.GetVariableNames().OrderByDescending(v => v.Length).ToList();
            var tokens = new List<ExpressionToken>();
            foreach (var varName in varNames)
            {
                var pos = -1;
                while (true)
                {
                    pos = formula.IndexOf(varName, pos + 1, StringComparison.InvariantCultureIgnoreCase);
                    if (pos < 0) break;

                    if (tokens.Any(t => t.pos <= pos && (t.pos + t.len) >= pos)) continue;

                    tokens.Add(new ExpressionToken
                        {
                            pos = pos,
                            name = varName,
                            len = varName.Length,
                            replacement = MakeExpressionParameterHyperlink(varName).Replace("<", "#-").Replace(">", "#+")
                        });
                }
            }

            foreach (var token in tokens)
            {
                var tokenPos = token.pos;
                var deltaLen = token.replacement.Length - token.len;
                formula = formula.Substring(0, token.pos) + token.replacement + formula.Substring(token.pos + token.len);
                foreach (var t in tokens.Where(x => x.pos > tokenPos))
                    t.pos += deltaLen;
            }

            formula = formula.Replace("#-", "<").Replace("#+", ">");
            return formula;
        }

        private static string MakeExpressionParameterHyperlink(string variableName)
        {
            return "<a href=\"#\" title=\"" + MakeExpressionParameterHyperlinkText(variableName) + "\">" + 
                variableName + "</a>";
        }

        private static string MakeExpressionParameterHyperlinkText(string variableName)
        {
            var field = fields.FirstOrDefault(f =>
                    !string.IsNullOrEmpty(f.ExpressionParamName) && f.ExpressionParamName.Equals(
                    variableName, StringComparison.OrdinalIgnoreCase));
            if (field == null)
                return variableName;

            return field.ExpressionParamName + " - " + field.ExpressionParamTitle + "\n" + field.Description;
        }

        public static string DecorateString(string s, bool undecorate)
        {
            if (!undecorate)
                return string.Join("", s.Select(c => "0x0" + ((int)c).ToString("X") + ";"));
            var regex = new Regex("0x0[A-Fa-f0-9]+;");
            while (true)
            {
                var match = regex.Match(s);
                if (!match.Success) break;

                var numPart = Convert.ToUInt32(match.Value/*.Replace("0x0", "")*/.Replace(";", ""), 16);
                var alpha = ((char) numPart).ToString();
                var head = s.Substring(0, match.Index);
                var trail = s.Substring(match.Index + match.Length);
                s = head + alpha + trail;
            }
            return s;
        }
    }
}
