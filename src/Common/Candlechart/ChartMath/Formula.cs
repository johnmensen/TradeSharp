using System;

namespace Candlechart.ChartMath
{
    internal enum ArithmeticOperation
    {
        Negation,
        Addition,
        Subtraction,
        Multiplication,
        Division,
        Modulus,
        Power
    }

    internal enum LogicalOperation
    {
        Not,
        And,
        Or,
        Xor
    } 

    public enum MovingAverageMethod
    {
        Simple,
        Weighted,
        Exponential
    }

    internal enum RelationalOperation
    {
        Equal,
        NotEqual,
        Greater,
        GreaterEqual,
        Less,
        LessEqual
    }

    public static class Formula
    {
        public static DataArray<double> BollingerBandBottom(StockSeriesData data, int periods, MovingAverageMethod method, double stddev)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data", "Data must not be null.");
            }
            if (periods <= 0)
            {
                throw new ArgumentOutOfRangeException("periods", "Periods must not be negative or zero.");
            }
            if (stddev <= 0.0)
            {
                throw new ArgumentOutOfRangeException("stddev", "Standard deviation must not be negative or zero.");
            }
            DataArray<double> array = null;
            switch (method)
            {
                case MovingAverageMethod.Simple:
                    array = SimpleMovingAverage(data, periods);
                    break;

                case MovingAverageMethod.Weighted:
                    array = WeightedMovingAverage(data, periods);
                    break;

                case MovingAverageMethod.Exponential:
                    array = ExponentialMovingAverage(data, periods);
                    break;
            }
            DataArray<double> array2 = ChartMath.StdDev(data, periods);
            return ChartMath.Subtract(array, ChartMath.Multiply(array2, stddev));
        }

        public static DataArray<double> BollingerBandTop(StockSeriesData data, int periods, MovingAverageMethod method, double stddev)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data", "Data must not be null.");
            }
            if (periods <= 0)
            {
                throw new ArgumentOutOfRangeException("periods", "Periods must not be negative or zero.");
            }
            if (stddev <= 0.0)
            {
                throw new ArgumentOutOfRangeException("stddev", "Standard deviation must not be negative or zero.");
            }
            DataArray<double> array = null;
            switch (method)
            {
                case MovingAverageMethod.Simple:
                    array = SimpleMovingAverage(data, periods);
                    break;

                case MovingAverageMethod.Weighted:
                    array = WeightedMovingAverage(data, periods);
                    break;

                case MovingAverageMethod.Exponential:
                    array = ExponentialMovingAverage(data, periods);
                    break;
            }
            DataArray<double> array2 = ChartMath.StdDev(data, periods);
            return ChartMath.Add(array, ChartMath.Multiply(array2, stddev));
        }

        public static DataArray<double> ExponentialMovingAverage(StockSeriesData data, int periods)
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
                double num4 = 2.0 / (1 + periods);
                double num5 = 1.0 - num4;
                for (int j = data.StartIndex; j < (data.StartIndex + periods); j++)
                {
                    num3 += (double)data[j].close;
                }
                array.Add(num3 / periods);
                for (int k = num + 1; k < data.Count; k++)
                {
                    array.Add((num4 * (double)data[k].close) + (num5 * array[k - 1]));
                }
            }
            array.StartIndex = num;
            return array;
        }

        public static DataArray<double> Highest(DataArray<double> data, int periods)
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
                double minValue = 0.0;
                int num4 = num - periods;
                for (int j = num; j < data.Count; j++)
                {
                    if (num4 <= (j - periods))
                    {
                        minValue = double.MinValue;
                        for (int k = (j - periods) + 1; k <= j; k++)
                        {
                            if (data[k] > minValue)
                            {
                                minValue = data[k];
                                num4 = k;
                            }
                        }
                    }
                    else if (data[j] > minValue)
                    {
                        minValue = data[j];
                        num4 = j;
                    }
                    array.Add(minValue);
                }
            }
            array.StartIndex = num;
            return array;
        }

        public static DataArray<double> Lowest(DataArray<double> data, int periods)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data", "DataArray<double> must not be null.");
            }
            if (periods <= 0)
            {
                throw new ArgumentOutOfRangeException("periods", "Periods must not be negative or zero.");
            }
            DataArray<double> array = new DataArray<double>(data.Count);
            int num = (((data.StartIndex + periods) - 1) < data.Count) ? ((data.StartIndex + periods) - 1) : data.Count;
            for (int i = 0; i < num; i++)
            {
                array.Add(0.0);
            }
            if (num < data.Count)
            {
                double maxValue = 0.0;
                int num4 = num - periods;
                for (int j = num; j < data.Count; j++)
                {
                    if (num4 <= (j - periods))
                    {
                        maxValue = double.MaxValue;
                        for (int k = (j - periods) + 1; k <= j; k++)
                        {
                            if (data[k] < maxValue)
                            {
                                maxValue = data[k];
                                num4 = k;
                            }
                        }
                    }
                    else if (data[j] < maxValue)
                    {
                        maxValue = data[j];
                        num4 = j;
                    }
                    array.Add(maxValue);
                }
            }
            array.StartIndex = num;
            return array;
        }

        public static DataArray<double> MACD(StockSeriesData data, int shortPeriods, int longPeriods)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data", "DataArray<double> must not be null.");
            }
            if (shortPeriods <= 0)
            {
                throw new ArgumentOutOfRangeException("shortPeriods", "Periods must not be negative or zero.");
            }
            if (longPeriods <= 0)
            {
                throw new ArgumentOutOfRangeException("longPeriods", "Periods must not be negative or zero.");
            }
            return ChartMath.Subtract(ExponentialMovingAverage(data, shortPeriods), ExponentialMovingAverage(data, longPeriods));
        }

        public static DataArray<double> Momentum(DataArray<double> data, int periods)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data", "DataArray<double> must not be null.");
            }
            if (periods <= 0)
            {
                throw new ArgumentOutOfRangeException("periods", "Periods must not be negative or zero.");
            }
            return ChartMath.Multiply(ChartMath.Divide(data, Reference(data, -periods)), 100.0);
        }

        public static DataArray<double> RateOfChange(DataArray<double> data, int periods)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data", "DataArray<double> must not be null.");
            }
            if (periods <= 0)
            {
                throw new ArgumentOutOfRangeException("periods", "Periods must not be negative or zero.");
            }
            DataArray<double> array = Reference(data, -periods);
            return ChartMath.Multiply(ChartMath.Divide(ChartMath.Subtract(data, array), array), 100.0);
        }

        public static DataArray<double> Reference(DataArray<double> data, int index)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data", "DataArray<double> must not be null.");
            }
            DataArray<double> array = new DataArray<double>(data.Count);
            if (index <= 0)
            {
                index *= -1;
                int num = ((data.StartIndex + index) < data.Count) ? (data.StartIndex + index) : data.Count;
                for (int m = 0; m < num; m++)
                {
                    array.Add(0.0);
                }
                for (int n = num; n < data.Count; n++)
                {
                    array.Add(data[n - index]);
                }
                array.StartIndex = num;
                return array;
            }
            int num4 = ((data.StartIndex - index) >= 0) ? (data.StartIndex - index) : 0;
            int num5 = (((data.Count - 1) - index) >= 0) ? ((data.Count - 1) - index) : -1;
            for (int i = 0; i < num4; i++)
            {
                array.Add(0.0);
            }
            for (int j = num4; j <= num5; j++)
            {
                array.Add(data[j + index]);
            }
            for (int k = num5 + 1; k < data.Count; k++)
            {
                array.Add(0.0);
            }
            array.StartIndex = num4;
            return array;
        }

        public static DataArray<double> SimpleMovingAverage(StockSeriesData data, int periods)
        {
            return ChartMath.Divide(ChartMath.Sum(data, periods), periods);
        }

        public static DataArray<double> WeightedMovingAverage(StockSeriesData data, int periods)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data", "DataArray<double> must not be null.");
            }
            if (periods <= 0)
            {
                throw new ArgumentOutOfRangeException("periods", "Periods must not be negative or zero.");
            }
            DataArray<double> array = new DataArray<double>(data.Count);
            int num = (((data.StartIndex + periods) - 1) < data.Count) ? ((data.StartIndex + periods) - 1) : data.Count;
            for (int i = 0; i < num; i++)
            {
                array.Add(0.0);
            }
            if (num < data.Count)
            {
                double num3 = 0.0;
                double num4 = 0.0;
                double num5 = (periods * (periods + 1)) / 2;
                for (int j = data.StartIndex; j < (data.StartIndex + periods); j++)
                {
                    num4 += ((j - data.StartIndex) + 1) * (double)data[j].close;
                    num3 += (double)data[j].close;
                }
                array.Add(num4 / num5);
                for (int k = num + 1; k < data.Count; k++)
                {
                    num4 = (num4 - num3) + (periods * (double)data[k].close);
                    num3 = (num3 - (double)data[k - periods].close) + (double)data[k].close;
                    array.Add(num4 / num5);
                }
            }
            array.StartIndex = num;
            return array;
        }
    } 
}
