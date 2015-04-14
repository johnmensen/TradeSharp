using Candlechart;
using Entity;
using NUnit.Framework;

namespace TradeSharp.Test.Robot
{
    [TestFixture]
    public class OptionTraderRobotTest
    {
        //private readonly List<float> prices = new List<float>(5000);
        private const double StdDev = 0.005;

        [TestFixtureSetUp]
        public void Setup()
        {
            //var startPrice = 100f;
            //for (var i = 0; i < 5000; i++)
            //{
            //    var delta = Gaussian.BoxMuller(0, StdDev);
            //    startPrice += (float)delta;
            //    prices.Add(startPrice);
            //}
        }

        [Test]
        public void TestCalcProbTouch()
        {
            //var sumSigma = 0.0;
            //for (var i = 1; i < prices.Count; i++)
            //{
            //    var delta = (prices[i] - prices[i - 1]);
            //    sumSigma += (delta * delta);
            //}
            //var sigma = Math.Sqrt(sumSigma / prices.Count);
            var sigma = StdDev;
            var prob = GetMonteCarloProbHighTouchesM(sigma, 60, 0.05, 15000);
            Assert.IsTrue(prob >= 0 && prob <= 1, "вероятность Монте-Карло вне границ 0 - 1 (" + prob + ")");
            var probT = GetTheoreticalProbHighTouchesM(sigma, 60, 0.05);
        }

        private double GetMonteCarloProbHighTouchesM(double sigma, int numSteps, double m, int iterations)
        {
            var countAbove = 0;
            for (var i = 0; i < iterations; i++)
            {
                var hasAbove = false;

                var startPrice = 0.0;
                for (var j = 0; j < numSteps; j++)
                {
                    startPrice += Gaussian.BoxMuller(0, sigma);
                    if (startPrice >= m)
                    {
                        hasAbove = true;
                        break;
                    }
                }

                if (hasAbove)
                    countAbove++;
            }

            return (double) countAbove / iterations;
        }

        private double GetTheoreticalProbHighTouchesM(double sigma, int numSteps, double m)
        {
            return 1 - ProbabilityCore.CalculatePriceDeltaWillBeLessThan(sigma * numSteps, 1, m);
        }
    }
}
