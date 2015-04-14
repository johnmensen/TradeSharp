using System;
using System.Collections.Generic;
using System.Linq;

namespace Entity
{
    /// <summary>
    /// методом обратной функции генерирует корректно распределенную СВ -
    /// ценовое приращение
    /// </summary>
    public class PriceModel
    {
        public static readonly Random random = new Random(DateTime.Now.Millisecond);

        private readonly double tailPercent = 0.1;

        private const int StepsPerTail = 50;

        private const int StepsPerBody = 200;

        private readonly ValueRange tailLeft, tailRight, body;

        public PriceModel(List<double> deltas) : this (deltas, 0.1)
        {            
        }

        public PriceModel(List<double> deltas, double tailPercent)
        {
            this.tailPercent = tailPercent;
            // отсечь 0.2% + 0.2% результатов "справа" и "слева" как "хвосты"
            var tailCount = (int)(deltas.Count * this.tailPercent / 100);
            var bodyCount = deltas.Count - tailCount * 2;

            tailLeft = new ValueRange(deltas.Take(tailCount).ToArray(), StepsPerTail);
            body = new ValueRange(deltas.Skip(tailCount).Take(bodyCount).ToArray(), StepsPerBody);
            tailRight = new ValueRange(deltas.Skip(tailCount + bodyCount).ToArray(), StepsPerTail);
        }

        public double GetRandomDelta()
        {
            var isTail = random.Next(int.MaxValue) < (int)(int.MaxValue * tailPercent / 100);
            if (!isTail)
                return body.GetRandomValue();
            var isLeft = random.Next(int.MaxValue) <= (int.MaxValue / 2);
            var tail = isLeft ? tailLeft : tailRight;
            return tail.GetRandomValue();
        }
    }

    /// <summary>
    /// массив вида интервал / попаданий на интервал (с накоплением)
    /// </summary>
    class ValueRange
    {
        private readonly double left, right;

        private readonly double[] intgrFunc;

        public ValueRange(double[] values, int stepsCount)
        {
            left = values[0];
            right = values.Last();

            if (left == right)
            {
                intgrFunc = new double[0];
                return;
            }

            var step = (right - left) / stepsCount;

            var valIndex = 0;
            var countTotal = 0;

            var countOnInterval = new int[stepsCount];
            for (var funcIndex = 0; funcIndex < stepsCount; funcIndex++)
            {
                var l = funcIndex * step + left;
                var r = l + step;
                while (valIndex < values.Length)
                {
                    if (values[valIndex] >= r)
                        break;
                    countTotal++;
                    valIndex++;
                }
                countOnInterval[funcIndex] = countTotal;
            }

            intgrFunc = new double[stepsCount];
            for (var i = 0; i < stepsCount; i++)
                intgrFunc[i] = countOnInterval[i] / (double)countTotal;
        }

        public double GetRandomValue()
        {
            if (intgrFunc.Length < 2)
                return left;

            var er = PriceModel.random.NextDouble();
            double l = intgrFunc[0], r = intgrFunc.Last();
            if (er <= l) return left;
            if (er >= r) return right;
            int iL = 0, iR = intgrFunc.Length - 1;

            for (var i = 0; i < intgrFunc.Length; i++)
            {
                if (intgrFunc[i] > er)
                {
                    r = intgrFunc[i];
                    if (i > 0)
                    {
                        l = intgrFunc[i - 1];
                        iL = i - 1;
                    }
                    break;
                }
            }
            var relIndex = iL + (er - l) / (r - l);

            var absVal = left + (right - left) * relIndex / intgrFunc.Length;
            return absVal;
        }
    }
}
