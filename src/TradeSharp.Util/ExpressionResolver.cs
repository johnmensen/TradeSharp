using System;
using System.Collections.Generic;
using System.Linq;

namespace TradeSharp.Util
{
    public class ExpressionResolver
    {
        public readonly ExpressionOperand rootOperand;

        #region Метаданные

        private static readonly List<Cortege2<string, string>> constants = new
            List<Cortege2<string, string>>
            {
                new Cortege2<string, string>("_pi", "3.1415926536"),
                new Cortege2<string, string>("_e", "2.718281828459"),
                new Cortege2<string, string>("_r2", "1.414213562373"),
                new Cortege2<string, string>("_true", "1"),
                new Cortege2<string, string>("_false", "0"),
            };

        private static readonly Dictionary<ExpressionOperator, int>
            operatorPriority = new Dictionary<ExpressionOperator, int>
                                   {
                                       { ExpressionOperator.Equal, 0 },
                                       { ExpressionOperator.NotEqual, 0 },                                       
                                       { ExpressionOperator.Greater, 0 },
                                       { ExpressionOperator.Lower, 0 },
                                       { ExpressionOperator.And, 1 },    
                                       { ExpressionOperator.Or, 1 },                                       
                                       { ExpressionOperator.Plus, 2 },
                                       { ExpressionOperator.Minus, 2 },
                                       { ExpressionOperator.Mult, 3 },
                                       { ExpressionOperator.Div, 3 },
                                       { ExpressionOperator.Pow, 4 },
                                       { ExpressionOperator.ReplaceNil, 4 },
                                       { ExpressionOperator.SignInversion, 5 }, /* этот и последующие - унарные операторы */
                                       { ExpressionOperator.Negate, 5 },
                                       { ExpressionOperator.Sign, 5 }, /* у них - высший приоритет */
                                       { ExpressionOperator.Sin, 5 },
                                       { ExpressionOperator.Cos, 5 },
                                       { ExpressionOperator.Tan, 5 },
                                       { ExpressionOperator.Log10, 5 },
                                       { ExpressionOperator.Log, 5 }
                                   };

        private static readonly Dictionary<ExpressionOperator, string>
            operatorString = new Dictionary<ExpressionOperator, string>
                                 {
                                     { ExpressionOperator.Plus, "+" },
                                     { ExpressionOperator.Minus, "-" },
                                     { ExpressionOperator.Mult, "*" },
                                     { ExpressionOperator.Div, "/" },
                                     { ExpressionOperator.Pow, "^" },
                                     { ExpressionOperator.Sign, "sign" },
                                     { ExpressionOperator.Sin, "sin" },
                                     { ExpressionOperator.Cos, "cos" },
                                     { ExpressionOperator.Tan, "tan" },
                                     { ExpressionOperator.Log, "ln" },
                                     { ExpressionOperator.Log10, "lg" },
                                     { ExpressionOperator.And, "&" },    
                                     { ExpressionOperator.Or, "|" },
                                     { ExpressionOperator.Equal, "=" },
                                     { ExpressionOperator.NotEqual, "@" },
                                     { ExpressionOperator.Greater, ">" },
                                     { ExpressionOperator.Lower, "<" },
                                     
                                     { ExpressionOperator.SignInversion, "-" },
                                     { ExpressionOperator.Negate, "~" },
                                     { ExpressionOperator.ReplaceNil, "??" }
                                 };

        public static string GetOperatorString(ExpressionOperator oper)
        {
            if (!operatorString.ContainsKey(oper))
                return null;
            return operatorString[oper];
        }

        #endregion

        private readonly string formula;
        public string Formula
        {
            get { return formula; }
        }

        /// <param name="formula">3.14*sin(x + y^(-2))</param>
        public ExpressionResolver(string formula)
        {
            formula = formula.ToLower().Replace(" ", "").Replace("{", "(").Replace("}", ")").Replace("!=", "@");
            formula = constants.Aggregate(formula, (current, cst) => current.Replace(cst.a, cst.b));

            this.formula = formula;
            var entries = GetOperatorEntries(formula);
            BuildOperandTree(ref rootOperand, formula, entries);
        }

        /// <summary>
        /// вернуть имена всех объявленных в формуле переменных, без повторов
        /// </summary>        
        public List<string> GetVariableNames()
        {
            var vars = new List<string>();
            GetVariableNames(rootOperand, vars);
            vars = vars.Distinct().ToList();
            return vars;
        }

        public static bool CheckFunction(string function, out string error, IEnumerable<string> validParamNames)
        {
            if (string.IsNullOrEmpty(function))
            {
                error = "Функция не может быть пустой";
                return false;
            }
            ExpressionResolver resv;
            try
            {
                resv = new ExpressionResolver(function);
            }
            catch
            {
                error = "Функция не распознана";
                return false;
            }
            var lackVars =
                resv.GetVariableNames()
                    .Where(n => !validParamNames.Any(p => p.Equals(n, StringComparison.OrdinalIgnoreCase)));
            error = string.Join(", ", lackVars);
            if (string.IsNullOrEmpty(error)) return true;
            error = "Переменные " + error;
            return false;
        }

        private static void GetVariableNames(ExpressionOperand root, List<string> vars)
        {
            if (root == null) return;
            if (root.IsVariable) vars.Add(root.VariableName);
            else
            {
                if (root.OperandLeft != null)
                    GetVariableNames(root.OperandLeft, vars);
                if (root.OperandRight != null)
                    GetVariableNames(root.OperandRight, vars);
            }
        }

        public bool Calculate(Dictionary<string, double> varValues, out double result)
        {
            result = 0;
            if (rootOperand == null) return false;
            try
            {
                result = Calculate(rootOperand, varValues);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private static double Calculate(ExpressionOperand root, Dictionary<string, double> varValues)
        {
            if (root.IsConstant) return root.ConstantValue;
            if (root.IsVariable)
            {
                double result;
                if (!varValues.TryGetValue(root.VariableName, out result))
                    throw new Exception(string.Format("Переменная \"{0}\" не определена", root.VariableName));
                return result;
            }
            var valLeft = root.OperandLeft == null ? 0 : Calculate(root.OperandLeft, varValues);
            var valRight = root.OperandRight == null ? 0 : Calculate(root.OperandRight, varValues);
            return CalculateOperator(root.Operator, valLeft, valRight);
        }

        private static double CalculateOperator(ExpressionOperator op, double a, double b)
        {
            switch (op)
            {
                case ExpressionOperator.Equal:
                    return a == b ? 1 : 0;
                case ExpressionOperator.NotEqual:
                    return a != b ? 1 : 0;
                case ExpressionOperator.And:
                    return a != 0 && b != 0 ? 1 : 0;                
                case ExpressionOperator.Div:
                    {
                        if (b == 0) throw new Exception("Division by 0");
                        return a / b;
                    }
                case ExpressionOperator.Greater:
                    return a > b ? 1 : 0;
                case ExpressionOperator.Log:
                    return Math.Log(b);
                case ExpressionOperator.Log10:
                    return Math.Log10(b);
                case ExpressionOperator.Lower:
                    return a < b ? 1 : 0;
                case ExpressionOperator.Minus:
                    return a - b;
                case ExpressionOperator.Mult:
                    return a * b;
                case ExpressionOperator.SignInversion:
                    return -b;
                case ExpressionOperator.Or:
                    return a != 0 || b != 0 ? 1 : 0;
                case ExpressionOperator.Plus:
                    return a + b;
                case ExpressionOperator.Pow:
                    return Math.Pow(a, b);
                case ExpressionOperator.Sign:
                    return Math.Sign(b);
                case ExpressionOperator.Cos:
                    return Math.Cos(b);
                case ExpressionOperator.Sin:
                    return Math.Sin(b);
                case ExpressionOperator.Tan:
                    return Math.Tan(b);
                case ExpressionOperator.ReplaceNil:
                    return a == 0 ? b : a;
                case ExpressionOperator.Negate:
                    return b == 0 ? 1 : 0;
            }
            return 0;
        }

        private static void BuildOperandTree(ref ExpressionOperand root, string formula, List<ExpressionOperatorEntry> operators)
        {
            if (string.IsNullOrEmpty(formula))
                throw new ArgumentException("BuildOperandTree() - formula пуста", "formula");
            // освободить формулу от обрамляющих скобок, если имеются
            while (true)
            {
                if (HasRedundantBraces(formula))
                {
                    formula = formula.Substring(1, formula.Length - 2);
                    foreach (var op in operators)
                    {
                        op.start -= 1;
                        op.end -= 1;
                    }
                }
                else break;
            }
            if (string.IsNullOrEmpty(formula))
                throw new ArgumentException("BuildOperandTree() - formula пуста", "formula");

            if (operators.Count == 0)
            {// либо константа, либо - переменная
                var constVal = formula.ToDoubleUniformSafe();
                if (constVal.HasValue)
                {
                    root = new ExpressionOperand { IsConstant = true, ConstantValue = constVal.Value };
                    return;
                }
                // будем считать переменной
                root = new ExpressionOperand { IsVariable = true, VariableName = formula };
                return;
            }

            root = new ExpressionOperand();

            // найти самый низко-приоритетный оператор
            var minPrior = operators.Min(op => op.priority);
            var firstOper = operators.Last(op => op.priority == minPrior);
            root.Operator = firstOper.oper;

            // получить левый и правый (если есть) операнды
            // есть левый операнд
            if (firstOper.start > 0)
            {
                var leftOpers = new List<ExpressionOperatorEntry>();
                foreach (var op in operators)
                {
                    if (op.start >= firstOper.start) continue;
                    leftOpers.Add(new ExpressionOperatorEntry(op.oper, op.start, op.end) { priority = op.priority });
                }
                var partLeft = formula.Substring(0, firstOper.start);
                ExpressionOperand leftRoot = null;
                BuildOperandTree(ref leftRoot, partLeft, leftOpers);
                root.OperandLeft = leftRoot;
            }
            // есть правый операнд
            if (firstOper.end < formula.Length)
            {
                var rightOpers = new List<ExpressionOperatorEntry>();
                foreach (var op in operators)
                {
                    if (op.start <= firstOper.start) continue;
                    rightOpers.Add(new ExpressionOperatorEntry(op.oper, op.start - firstOper.end,
                        op.end - firstOper.end) { priority = op.priority });
                }
                var partRight = formula.Substring(firstOper.end);
                ExpressionOperand rightRoot = null;
                BuildOperandTree(ref rightRoot, partRight, rightOpers);
                root.OperandRight = rightRoot;
            }
        }

        private static bool HasRedundantBraces(string formula)
        {
            if (string.IsNullOrEmpty(formula)) return false;
            if (formula[0] != '(' || formula[formula.Length - 1] != ')') return false;
            var level = 1;
            for (var i = 1; i < formula.Length - 1; i++)
            {
                if (formula[i] == '(') level++;
                else
                    if (formula[i] == ')')
                    {
                        level--;
                        if (level == 0) return false;
                    }
            }
            return true;
        }

        private static List<ExpressionOperatorEntry> GetOperatorEntries(string formula)
        {
            var formulaRem = formula;
            var entries = new List<ExpressionOperatorEntry>();
            var index = 0;
            // найти вхождения операторов
            while (!string.IsNullOrEmpty(formulaRem))
            {
                var shiftChar = 1;
                foreach (var opName in operatorString)
                {
                    if (formulaRem.StartsWith(opName.Value))
                    {
                        entries.Add(new ExpressionOperatorEntry(opName.Key, index, index + opName.Value.Length));
                        shiftChar = opName.Value.Length;
                        break;
                    }
                }
                index += shiftChar;
                formulaRem = formulaRem.Substring(shiftChar);
            }
            // заменить вычитание на отрицание
            // если слева от оператора "минус" другой оператор, скобка или ничего - это отрицание
            for (var i = 0; i < entries.Count; i++)
            {
                var oper = entries[i];
                if (oper.oper != ExpressionOperator.Minus) continue;
                if (oper.start == 0)
                {
                    oper.oper = ExpressionOperator.SignInversion;
                    continue;
                }
                if (i > 0)
                    if (entries[i - 1].end == oper.start)
                    {
                        oper.oper = ExpressionOperator.SignInversion;
                        continue;
                    }
                if (formula[oper.start - 1] == '(')
                    oper.oper = ExpressionOperator.SignInversion;
            }

            // расставить операторам приоритеты
            foreach (var oper in entries)
            {
                var basePriority = operatorPriority[oper.oper];
                // найти уровень вложенности в скобки
                var level = GetBracketLevel(formula, oper.start);
                oper.priority = level * 100 + basePriority;
            }
            return entries;
        }

        private static int GetBracketLevel(string formula, int index)
        {
            var level = 0;
            for (var i = 0; i < index; i++)
            {
                if (formula[i] == '(') level++;
                else
                    if (formula[i] == ')') level--;
            }
            return level;
        }            
    }

    public enum ExpressionOperator
    {
        Equal, NotEqual, Or, And,
        Greater, Lower, Plus, Minus, Mult, Div, Pow, 
        SignInversion, Negate, Sin, Cos, Tan, Log, Log10, Sign, ReplaceNil
    }

    public class ExpressionOperatorEntry
    {
        public int start, end;
        public int priority;
        public ExpressionOperator oper;

        public ExpressionOperatorEntry(ExpressionOperator oper, int start, int end)
        {
            this.oper = oper;
            this.start = start;
            this.end = end;
            priority = -1;
        }
    }

    public class ExpressionOperand
    {
        public bool IsConstant { get; set; }
        public bool IsVariable { get; set; }
        public double ConstantValue { get; set; }
        public string VariableName { get; set; }
        public ExpressionOperator Operator { get; set; }
        public ExpressionOperand OperandLeft { get; set; }
        public ExpressionOperand OperandRight { get; set; }
    }
}
