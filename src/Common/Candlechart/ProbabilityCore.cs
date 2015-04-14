using System;

namespace Candlechart
{
    public static class ProbabilityCore
    {
        public const double R2 = 1.4142135623730950488016887242097;

        /// <summary>
        /// вычисляет вероятность того, что цена за некоторое время
        /// (количество свечей numCandles, м.б. дробным) станет
        /// выше (shouldBeLess = false) или ниже (shouldBeLess = true)
        /// цены targetPrice при том, что текущая цена равна currentPrice
        /// 
        /// candleAvgLen - средняя длина свечи
        /// </summary>
        public static double CalculateLevelProb(double candleAvgLen, double numCandles,
                                                double targetPrice, double currentPrice, bool shouldBeLess)
        {
            var targetDelta = !shouldBeLess
                                  ? currentPrice - targetPrice
                                  : targetPrice - currentPrice;
            return CalculatePriceDeltaWillBeLessThan(candleAvgLen, numCandles, targetDelta);
        }

        public static double CalculatePriceDeltaWillBeLessThan(double sigmaStep, double numSteps, double targetDelta)
        {
            const double r2 = 1.4142135623730950488016887242097;
            var sigma = Math.Sqrt(numSteps) * sigmaStep;
            var p = 0.5*(1 + Erf(targetDelta/(r2*sigma)));
            return p;
        }

        public static double IntegralFunc(double x, double sigma, double avg)
        {
            return 0.5*(1 + Erf((x - avg)/(R2*sigma)));
        }

        public static double Erf(double x)
        {
            // constants
            const double a1 = 0.254829592;
            const double a2 = -0.284496736;
            const double a3 = 1.421413741;
            const double a4 = -1.453152027;
            const double a5 = 1.061405429;
            const double p = 0.3275911;

            // Save the sign of x
            int sign = 1;
            if (x < 0)
                sign = -1;
            x = Math.Abs(x);

            // A&S formula 7.1.26
            double t = 1.0 / (1.0 + p * x);
            double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);

            return sign * y;
        }
    }
}
