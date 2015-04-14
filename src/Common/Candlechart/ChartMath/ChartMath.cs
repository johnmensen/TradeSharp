using System;

namespace Candlechart.ChartMath
{
    public static class ChartMath
    {
        public static DataArray<double> Abs(DataArray<double> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data", "DataArray must not be null.");
            }
            var array = new DataArray<double>(data.Count);
            for (int i = 0; i < data.StartIndex; i++)
            {
                array.Add(0.0);
            }
            for (int j = data.StartIndex; j < data.Count; j++)
            {
                array.Add(Math.Abs(data[j]));
            }
            array.StartIndex = data.StartIndex;
            return array;
        }

        public static DataArray<double> Add(DataArray<double> data1, DataArray<double> data2)
        {
            return PerformArithmeticOperation(data1, data2, ArithmeticOperation.Addition);
        }

        public static DataArray<double> Add(DataArray<double> data, double value)
        {
            return PerformArithmeticOperation(data, value, ArithmeticOperation.Addition);
        }

        public static DataArray<double> And(DataArray<double> data1, DataArray<double> data2)
        {
            return PerformLogicalOperation(data1, data2, LogicalOperation.And);
        }

        public static DataArray<double> And(DataArray<double> data, double value)
        {
            return PerformLogicalOperation(data, value, LogicalOperation.And);
        }

        public static DataArray<double> Ceiling(DataArray<double> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data", "DataArray must not be null.");
            }
            var array = new DataArray<double>(data.Count);
            for (int i = 0; i < data.StartIndex; i++)
            {
                array.Add(0.0);
            }
            for (int j = data.StartIndex; j < data.Count; j++)
            {
                array.Add(Math.Ceiling(data[j]));
            }
            array.StartIndex = data.StartIndex;
            return array;
        }

        public static DataArray<double> Cumulate(DataArray<double> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data", "DataArray must not be null.");
            }
            var array = new DataArray<double>(data.Count);
            for (int i = 0; i < data.StartIndex; i++)
            {
                array.Add(0.0);
            }
            if (data.StartIndex < data.Count)
            {
                array.Add(data[data.StartIndex]);
                for (int j = data.StartIndex + 1; j < data.Count; j++)
                {
                    array.Add(data[j] + array[j - 1]);
                }
            }
            array.StartIndex = data.StartIndex;
            return array;
        }

        public static DataArray<double> Divide(DataArray<double> data1, DataArray<double> data2)
        {
            return PerformArithmeticOperation(data1, data2, ArithmeticOperation.Division);
        }

        public static DataArray<double> Divide(DataArray<double> data, double value)
        {
            return PerformArithmeticOperation(data, value, ArithmeticOperation.Division);
        }

        public static DataArray<double> Exp(DataArray<double> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data", "DataArray must not be null.");
            }
            var array = new DataArray<double>(data.Count);
            for (int i = 0; i < data.StartIndex; i++)
            {
                array.Add(0.0);
            }
            for (int j = data.StartIndex; j < data.Count; j++)
            {
                array.Add(Math.Exp(data[j]));
            }
            array.StartIndex = data.StartIndex;
            return array;
        }

        public static DataArray<double> Floor(DataArray<double> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data", "DataArray must not be null.");
            }
            var array = new DataArray<double>(data.Count);
            for (int i = 0; i < data.StartIndex; i++)
            {
                array.Add(0.0);
            }
            for (int j = data.StartIndex; j < data.Count; j++)
            {
                array.Add(Math.Floor(data[j]));
            }
            array.StartIndex = data.StartIndex;
            return array;
        }

        public static DataArray<double> Fraction(DataArray<double> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data", "DataArray must not be null.");
            }
            var array = new DataArray<double>(data.Count);
            for (int i = 0; i < data.StartIndex; i++)
            {
                array.Add(0.0);
            }
            for (int j = data.StartIndex; j < data.Count; j++)
            {
                array.Add(data[j] - ((int)data[j]));
            }
            array.StartIndex = data.StartIndex;
            return array;
        }

        public static DataArray<double> Integer(DataArray<double> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data", "DataArray must not be null.");
            }
            var array = new DataArray<double>(data.Count);
            for (int i = 0; i < data.StartIndex; i++)
            {
                array.Add(0.0);
            }
            for (int j = data.StartIndex; j < data.Count; j++)
            {
                array.Add(((int)data[j]));
            }
            array.StartIndex = data.StartIndex;
            return array;
        }

        public static DataArray<double> IsEqual(DataArray<double> data1, DataArray<double> data2)
        {
            return PerformRelationalOperation(data1, data2, RelationalOperation.Equal);
        }

        public static DataArray<double> IsEqual(DataArray<double> data, double value)
        {
            return PerformRelationalOperation(data, value, RelationalOperation.Equal);
        }

        public static DataArray<double> IsGreater(DataArray<double> data1, DataArray<double> data2)
        {
            return PerformRelationalOperation(data1, data2, RelationalOperation.Greater);
        }

        public static DataArray<double> IsGreater(DataArray<double> data, double value)
        {
            return PerformRelationalOperation(data, value, RelationalOperation.Greater);
        }

        public static DataArray<double> IsGreaterEqual(DataArray<double> data1, DataArray<double> data2)
        {
            return PerformRelationalOperation(data1, data2, RelationalOperation.GreaterEqual);
        }

        public static DataArray<double> IsGreaterEqual(DataArray<double> data, double value)
        {
            return PerformRelationalOperation(data, value, RelationalOperation.GreaterEqual);
        }

        public static DataArray<double> IsLess(DataArray<double> data1, DataArray<double> data2)
        {
            return PerformRelationalOperation(data1, data2, RelationalOperation.Less);
        }

        public static DataArray<double> IsLess(DataArray<double> data, double value)
        {
            return PerformRelationalOperation(data, value, RelationalOperation.Less);
        }

        public static DataArray<double> IsLessEqual(DataArray<double> data1, DataArray<double> data2)
        {
            return PerformRelationalOperation(data1, data2, RelationalOperation.LessEqual);
        }

        public static DataArray<double> IsLessEqual(DataArray<double> data, double value)
        {
            return PerformRelationalOperation(data, value, RelationalOperation.LessEqual);
        }

        public static DataArray<double> IsNotEqual(DataArray<double> data1, DataArray<double> data2)
        {
            return PerformRelationalOperation(data1, data2, RelationalOperation.NotEqual);
        }

        public static DataArray<double> IsNotEqual(DataArray<double> data, double value)
        {
            return PerformRelationalOperation(data, value, RelationalOperation.NotEqual);
        }

        public static DataArray<double> Log(DataArray<double> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data", "DataArray must not be null.");
            }
            var array = new DataArray<double>(data.Count);
            for (int i = 0; i < data.StartIndex; i++)
            {
                array.Add(0.0);
            }
            for (int j = data.StartIndex; j < data.Count; j++)
            {
                array.Add(Math.Log(data[j]));
            }
            array.StartIndex = data.StartIndex;
            return array;
        }

        public static double Maximum(DataArray<double> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data", "DataArray must not be null.");
            }
            if (data.BarCount <= 0)
            {
                return 0.0;
            }
            double minValue = double.MinValue;
            for (int i = data.StartIndex; i < data.Count; i++)
            {
                if (data[i] > minValue)
                {
                    minValue = data[i];
                }
            }
            return minValue;
        }

        public static DataArray<double> Maximum(DataArray<double> data1, DataArray<double> data2)
        {
            if (data1 == null)
            {
                throw new ArgumentNullException("data1", "DataArray must not be null.");
            }
            if (data2 == null)
            {
                throw new ArgumentNullException("data2", "DataArray must not be null.");
            }
            if (data1.Count != data2.Count)
            {
                throw new ArgumentException("Array sizes do not match.", "data1");
            }
            var array = new DataArray<double>(data1.Count);
            DataArray<double> array2 = data2;
            if (data1.StartIndex > data2.StartIndex)
            {
                array2 = data1;
            }
            for (int i = 0; i < array2.StartIndex; i++)
            {
                array.Add(0.0);
            }
            for (int j = array2.StartIndex; j < array2.Count; j++)
            {
                array.Add((data1[j] > data2[j]) ? data1[j] : data2[j]);
            }
            array.StartIndex = array2.StartIndex;
            return array;
        }

        public static DataArray<double> Maximum(StockSeriesData data, double value)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data", "DataArray must not be null.");
            }
            var array = new DataArray<double>(data.Count);
            for (int i = 0; i < data.StartIndex; i++)
            {
                array.Add(0.0);
            }
            for (int j = data.StartIndex; j < data.Count; j++)
            {
                array.Add(((double)data[j].close > value) ? (double)data[j].close : value);
            }
            array.StartIndex = data.StartIndex;
            return array;
        }

        public static double Minimum(DataArray<double> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data", "DataArray must not be null.");
            }
            if (data.BarCount <= 0)
            {
                return 0.0;
            }
            double maxValue = double.MaxValue;
            for (int i = data.StartIndex; i < data.Count; i++)
            {
                if (data[i] < maxValue)
                {
                    maxValue = data[i];
                }
            }
            return maxValue;
        }

        public static DataArray<double> Minimum(DataArray<double> data1, DataArray<double> data2)
        {
            if (data1 == null)
            {
                throw new ArgumentNullException("data1", "DataArray must not be null.");
            }
            if (data2 == null)
            {
                throw new ArgumentNullException("data2", "DataArray must not be null.");
            }
            if (data1.Count != data2.Count)
            {
                throw new ArgumentException("Array sizes do not match.", "data1");
            }
            var array = new DataArray<double>(data1.Count);
            DataArray<double> array2 = data2;
            if (data1.StartIndex > data2.StartIndex)
            {
                array2 = data1;
            }
            for (int i = 0; i < array2.StartIndex; i++)
            {
                array.Add(0.0);
            }
            for (int j = array2.StartIndex; j < array2.Count; j++)
            {
                array.Add((data1[j] < data2[j]) ? data1[j] : data2[j]);
            }
            array.StartIndex = array2.StartIndex;
            return array;
        }

        public static DataArray<double> Minimum(DataArray<double> data, double value)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data", "DataArray must not be null.");
            }
            var array = new DataArray<double>(data.Count);
            for (int i = 0; i < data.StartIndex; i++)
            {
                array.Add(0.0);
            }
            for (int j = data.StartIndex; j < data.Count; j++)
            {
                array.Add((data[j] < value) ? data[j] : value);
            }
            array.StartIndex = data.StartIndex;
            return array;
        }

        public static DataArray<double> Mod(DataArray<double> data1, DataArray<double> data2)
        {
            return PerformArithmeticOperation(data1, data2, ArithmeticOperation.Modulus);
        }

        public static DataArray<double> Mod(DataArray<double> data, double value)
        {
            return PerformArithmeticOperation(data, value, ArithmeticOperation.Modulus);
        }

        public static DataArray<double> Multiply(DataArray<double> data1, DataArray<double> data2)
        {
            return PerformArithmeticOperation(data1, data2, ArithmeticOperation.Multiplication);
        }

        public static DataArray<double> Multiply(DataArray<double> data, double value)
        {
            return PerformArithmeticOperation(data, value, ArithmeticOperation.Multiplication);
        }

        public static DataArray<double> Neg(DataArray<double> data)
        {
            return PerformArithmeticOperation(data, 0.0, ArithmeticOperation.Negation);
        }

        public static DataArray<double> Not(DataArray<double> data)
        {
            return PerformLogicalOperation(data, 0.0, LogicalOperation.Not);
        }

        public static DataArray<double> Or(DataArray<double> data1, DataArray<double> data2)
        {
            return PerformLogicalOperation(data1, data2, LogicalOperation.Or);
        }

        public static DataArray<double> Or(DataArray<double> data, double value)
        {
            return PerformLogicalOperation(data, value, LogicalOperation.Or);
        }

        private static DataArray<double> PerformArithmeticOperation(DataArray<double> data1, DataArray<double> data2,
                                                            ArithmeticOperation operation)
        {
            if (data1 == null)
            {
                throw new ArgumentNullException("data1", "DataArray<double> must not be null.");
            }
            if (data2 == null)
            {
                throw new ArgumentNullException("data2", "DataArray<double> must not be null.");
            }
            if (data1.Count != data2.Count)
            {
                throw new ArgumentException("Array sizes do not match.", "data1");
            }
            var array = new DataArray<double>(data1.Count);
            DataArray<double> array2 = data2;
            if (data1.StartIndex > data2.StartIndex)
            {
                array2 = data1;
            }
            for (int i = 0; i < array2.StartIndex; i++)
            {
                array.Add(0.0);
            }
            switch (operation)
            {
                case ArithmeticOperation.Addition:
                    for (int j = array2.StartIndex; j < array2.Count; j++)
                    {
                        array.Add(data1[j] + data2[j]);
                    }
                    break;

                case ArithmeticOperation.Subtraction:
                    for (int k = array2.StartIndex; k < array2.Count; k++)
                    {
                        array.Add(data1[k] - data2[k]);
                    }
                    break;

                case ArithmeticOperation.Multiplication:
                    for (int m = array2.StartIndex; m < array2.Count; m++)
                    {
                        array.Add(data1[m] * data2[m]);
                    }
                    break;

                case ArithmeticOperation.Division:
                    for (int n = array2.StartIndex; n < array2.Count; n++)
                    {
                        array.Add(data1[n] / data2[n]);
                    }
                    break;

                case ArithmeticOperation.Modulus:
                    for (int num6 = array2.StartIndex; num6 < array2.Count; num6++)
                    {
                        array.Add(data1[num6] % data2[num6]);
                    }
                    break;

                case ArithmeticOperation.Power:
                    for (int num7 = array2.StartIndex; num7 < array2.Count; num7++)
                    {
                        array.Add(Math.Pow(data1[num7], data2[num7]));
                    }
                    break;
            }
            array.StartIndex = array2.StartIndex;
            return array;
        }

        private static DataArray<double> PerformArithmeticOperation(DataArray<double> data, double value, ArithmeticOperation operation)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data", "DataArray<double> must not be null.");
            }
            var array = new DataArray<double>(data.Count);
            for (int i = 0; i < data.StartIndex; i++)
            {
                array.Add(0.0);
            }
            switch (operation)
            {
                case ArithmeticOperation.Negation:
                    for (int j = data.StartIndex; j < data.Count; j++)
                    {
                        array.Add(-data[j]);
                    }
                    break;

                case ArithmeticOperation.Addition:
                    for (int k = data.StartIndex; k < data.Count; k++)
                    {
                        array.Add(data[k] + value);
                    }
                    break;

                case ArithmeticOperation.Subtraction:
                    for (int m = data.StartIndex; m < data.Count; m++)
                    {
                        array.Add(data[m] - value);
                    }
                    break;

                case ArithmeticOperation.Multiplication:
                    for (int n = data.StartIndex; n < data.Count; n++)
                    {
                        array.Add(data[n] * value);
                    }
                    break;

                case ArithmeticOperation.Division:
                    for (int num6 = data.StartIndex; num6 < data.Count; num6++)
                    {
                        array.Add(data[num6] / value);
                    }
                    break;

                case ArithmeticOperation.Modulus:
                    for (int num7 = data.StartIndex; num7 < data.Count; num7++)
                    {
                        array.Add(data[num7] % value);
                    }
                    break;

                case ArithmeticOperation.Power:
                    for (int num8 = data.StartIndex; num8 < data.Count; num8++)
                    {
                        array.Add(Math.Pow(data[num8], value));
                    }
                    break;
            }
            array.StartIndex = data.StartIndex;
            return array;
        }

        private static DataArray<double> PerformLogicalOperation(DataArray<double> data1, DataArray<double> data2, LogicalOperation operation)
        {
            if (data1 == null)
            {
                throw new ArgumentNullException("data1", "DataArray<double> must not be null.");
            }
            if (data2 == null)
            {
                throw new ArgumentNullException("data2", "DataArray<double> must not be null.");
            }
            if (data1.Count != data2.Count)
            {
                throw new ArgumentException("Array sizes do not match.", "data1");
            }
            var array = new DataArray<double>(data1.Count);
            DataArray<double> array2 = data2;
            if (data1.StartIndex > data2.StartIndex)
            {
                array2 = data1;
            }
            for (int i = 0; i < array2.StartIndex; i++)
            {
                array.Add(0.0);
            }
            switch (operation)
            {
                case LogicalOperation.And:
                    for (int j = array2.StartIndex; j < array2.Count; j++)
                    {
                        array.Add(Convert.ToDouble((data1[j] != 0.0) && (data2[j] != 0.0)));
                    }
                    break;

                case LogicalOperation.Or:
                    for (int k = array2.StartIndex; k < array2.Count; k++)
                    {
                        array.Add(Convert.ToDouble((data1[k] != 0.0) || (data2[k] != 0.0)));
                    }
                    break;

                case LogicalOperation.Xor:
                    for (int m = array2.StartIndex; m < array2.Count; m++)
                    {
                        array.Add(Convert.ToDouble((data1[m] != 0.0) ^ (data2[m] != 0.0)));
                    }
                    break;
            }
            array.StartIndex = array2.StartIndex;
            return array;
        }

        private static DataArray<double> PerformLogicalOperation(DataArray<double> data, double value, LogicalOperation operation)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data", "DataArray<double> must not be null.");
            }
            var array = new DataArray<double>(data.Count);
            for (int i = 0; i < data.StartIndex; i++)
            {
                array.Add(0.0);
            }
            switch (operation)
            {
                case LogicalOperation.Not:
                    for (int j = data.StartIndex; j < data.Count; j++)
                    {
                        array.Add(Convert.ToDouble(data[j] == 0.0));
                    }
                    break;

                case LogicalOperation.And:
                    for (int k = data.StartIndex; k < data.Count; k++)
                    {
                        array.Add(Convert.ToDouble((data[k] != 0.0) && (value != 0.0)));
                    }
                    break;

                case LogicalOperation.Or:
                    for (int m = data.StartIndex; m < data.Count; m++)
                    {
                        array.Add(Convert.ToDouble((data[m] != 0.0) || (value != 0.0)));
                    }
                    break;

                case LogicalOperation.Xor:
                    for (int n = data.StartIndex; n < data.Count; n++)
                    {
                        array.Add(Convert.ToDouble((data[n] != 0.0) ^ (value != 0.0)));
                    }
                    break;
            }
            array.StartIndex = data.StartIndex;
            return array;
        }

        private static DataArray<double> PerformRelationalOperation(DataArray<double> data1, DataArray<double> data2,
                                                            RelationalOperation operation)
        {
            if (data1 == null)
            {
                throw new ArgumentNullException("data1", "DataArray<double> must not be null.");
            }
            if (data2 == null)
            {
                throw new ArgumentNullException("data2", "DataArray<double> must not be null.");
            }
            if (data1.Count != data2.Count)
            {
                throw new ArgumentException("Array sizes do not match.", "data1");
            }
            var array = new DataArray<double>(data1.Count);
            DataArray<double> array2 = data2;
            if (data1.StartIndex > data2.StartIndex)
            {
                array2 = data1;
            }
            for (int i = 0; i < array2.StartIndex; i++)
            {
                array.Add(0.0);
            }
            switch (operation)
            {
                case RelationalOperation.Equal:
                    for (int j = array2.StartIndex; j < array2.Count; j++)
                    {
                        array.Add(Convert.ToDouble(data1[j] == data2[j]));
                    }
                    break;

                case RelationalOperation.NotEqual:
                    for (int k = array2.StartIndex; k < array2.Count; k++)
                    {
                        array.Add(Convert.ToDouble(data1[k] != data2[k]));
                    }
                    break;

                case RelationalOperation.Greater:
                    for (int m = array2.StartIndex; m < array2.Count; m++)
                    {
                        array.Add(Convert.ToDouble(data1[m] > data2[m]));
                    }
                    break;

                case RelationalOperation.GreaterEqual:
                    for (int n = array2.StartIndex; n < array2.Count; n++)
                    {
                        array.Add(Convert.ToDouble(data1[n] >= data2[n]));
                    }
                    break;

                case RelationalOperation.Less:
                    for (int num6 = array2.StartIndex; num6 < array2.Count; num6++)
                    {
                        array.Add(Convert.ToDouble(data1[num6] < data2[num6]));
                    }
                    break;

                case RelationalOperation.LessEqual:
                    for (int num7 = array2.StartIndex; num7 < array2.Count; num7++)
                    {
                        array.Add(Convert.ToDouble(data1[num7] <= data2[num7]));
                    }
                    break;
            }
            array.StartIndex = array2.StartIndex;
            return array;
        }

        private static DataArray<double> PerformRelationalOperation(DataArray<double> data, double value, RelationalOperation operation)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data", "DataArray<double> must not be null.");
            }
            var array = new DataArray<double>(data.Count);
            for (int i = 0; i < data.StartIndex; i++)
            {
                array.Add(0.0);
            }
            switch (operation)
            {
                case RelationalOperation.Equal:
                    for (int j = data.StartIndex; j < data.Count; j++)
                    {
                        array.Add(Convert.ToDouble(data[j] == value));
                    }
                    break;

                case RelationalOperation.NotEqual:
                    for (int k = data.StartIndex; k < data.Count; k++)
                    {
                        array.Add(Convert.ToDouble(data[k] != value));
                    }
                    break;

                case RelationalOperation.Greater:
                    for (int m = data.StartIndex; m < data.Count; m++)
                    {
                        array.Add(Convert.ToDouble(data[m] > value));
                    }
                    break;

                case RelationalOperation.GreaterEqual:
                    for (int n = data.StartIndex; n < data.Count; n++)
                    {
                        array.Add(Convert.ToDouble(data[n] >= value));
                    }
                    break;

                case RelationalOperation.Less:
                    for (int num6 = data.StartIndex; num6 < data.Count; num6++)
                    {
                        array.Add(Convert.ToDouble(data[num6] < value));
                    }
                    break;

                case RelationalOperation.LessEqual:
                    for (int num7 = data.StartIndex; num7 < data.Count; num7++)
                    {
                        array.Add(Convert.ToDouble(data[num7] <= value));
                    }
                    break;
            }
            array.StartIndex = data.StartIndex;
            return array;
        }

        public static DataArray<double> Pow(DataArray<double> data1, DataArray<double> data2)
        {
            return PerformArithmeticOperation(data1, data2, ArithmeticOperation.Power);
        }

        public static DataArray<double> Pow(DataArray<double> data, double value)
        {
            return PerformArithmeticOperation(data, value, ArithmeticOperation.Power);
        }

        public static DataArray<double> Round(DataArray<double> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data", "DataArray<double> must not be null.");
            }
            var array = new DataArray<double>(data.Count);
            for (int i = 0; i < data.StartIndex; i++)
            {
                array.Add(0.0);
            }
            for (int j = data.StartIndex; j < data.Count; j++)
            {
                array.Add(Math.Round(data[j]));
            }
            array.StartIndex = data.StartIndex;
            return array;
        }

        public static DataArray<double> Sign(DataArray<double> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data", "DataArray<double> must not be null.");
            }
            var array = new DataArray<double>(data.Count);
            for (int i = 0; i < data.StartIndex; i++)
            {
                array.Add(0.0);
            }
            for (int j = data.StartIndex; j < data.Count; j++)
            {
                array.Add(Math.Sign(data[j]));
            }
            array.StartIndex = data.StartIndex;
            return array;
        }

        public static DataArray<double> Sqrt(DataArray<double> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data", "DataArray<double> must not be null.");
            }
            var array = new DataArray<double>(data.Count);
            for (int i = 0; i < data.StartIndex; i++)
            {
                array.Add(0.0);
            }
            for (int j = data.StartIndex; j < data.Count; j++)
            {
                array.Add(Math.Sqrt(data[j]));
            }
            array.StartIndex = data.StartIndex;
            return array;
        }

        public static DataArray<double> StdDev(StockSeriesData data, int periods)
        {
            return Sqrt(Variance(data, periods));
        }

        public static DataArray<double> Subtract(DataArray<double> data1, DataArray<double> data2)
        {
            return PerformArithmeticOperation(data1, data2, ArithmeticOperation.Subtraction);
        }

        public static DataArray<double> Subtract(DataArray<double> data, double value)
        {
            return PerformArithmeticOperation(data, value, ArithmeticOperation.Subtraction);
        }

        public static DataArray<double> Sum(StockSeriesData data, int periods)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data", "DataArray<double> must not be null.");
            }
            if (periods <= 0)
            {
                throw new ArgumentOutOfRangeException("periods", "Periods must not be negative or zero.");
            }
            var array = new DataArray<double>(data.Count);
            int num = (((data.StartIndex + periods) - 1) < data.Count) ? ((data.StartIndex + periods) - 1) : data.Count;
            for (int i = 0; i < num; i++)
            {
                array.Add(0.0);
            }
            if (num < data.Count)
            {
                double num3 = 0.0;
                for (int j = data.StartIndex; j < (data.StartIndex + periods); j++)
                {
                    num3 += (double)data[j].close;
                }
                array.Add(num3);
                for (int k = num + 1; k < data.Count; k++)
                {
                    num3 = (num3 - (double)data[k - periods].close) + (double)data[k].close;
                    array.Add(num3);
                }
            }
            array.StartIndex = num;
            return array;
        }

        public static DataArray<double> Variance(StockSeriesData data, int periods)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data", "DataArray<double> must not be null.");
            }
            if (periods <= 0)
            {
                throw new ArgumentOutOfRangeException("periods", "Periods must not be negative or zero.");
            }
            var array = new DataArray<double>(data.Count);
            int num = (((data.StartIndex + periods) - 1) < data.Count) ? ((data.StartIndex + periods) - 1) : data.Count;
            for (int i = 0; i < num; i++)
            {
                array.Add(0.0);
            }
            if (num < data.Count)
            {
                double num3 = 0.0;
                double num4 = 0.0;
                for (int j = data.StartIndex; j < (data.StartIndex + periods); j++)
                {
                    num3 += (double)data[j].close;
                    num4 += (double)(data[j].close * data[j].close);
                }
                array.Add((num4 / (periods)) - ((num3 / (periods)) * (num3 / (periods))));
                for (int k = num + 1; k < data.Count; k++)
                {
                    num3 = (num3 - (double)data[k - periods].close) + (double)data[k].close;
                    num4 = (num4 - (double)(data[k - periods].close * data[k - periods].close)) + (double)(data[k].close * data[k].close);
                    array.Add((num4 / (periods)) - ((num3 / (periods)) * (num3 / (periods))));
                }
            }
            array.StartIndex = num;
            return array;
        }

        public static DataArray<double> Xor(DataArray<double> data1, DataArray<double> data2)
        {
            return PerformLogicalOperation(data1, data2, LogicalOperation.Xor);
        }

        public static DataArray<double> Xor(DataArray<double> data, double value)
        {
            return PerformLogicalOperation(data, value, LogicalOperation.Xor);
        }
    }    
}
