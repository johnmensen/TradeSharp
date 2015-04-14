using System;

namespace Entity
{
    public class Gaussian
    {
        private static bool uselast = true;
        private static double nextGaussian;
        private static readonly Random random = new Random();

        public static double BoxMuller()
        {
            if (uselast)
            {
                uselast = false;
                return nextGaussian;
            }
            double v1, v2, s;
            do
            {
                v1 = 2.0 * random.NextDouble() - 1.0;
                v2 = 2.0 * random.NextDouble() - 1.0;
                s = v1 * v1 + v2 * v2;
            } while (s >= 1.0 || s == 0);

            s = Math.Sqrt((-2.0 * Math.Log(s)) / s);

            nextGaussian = v2 * s;
            uselast = true;
            return v1 * s;
        }

        public static double BoxMuller(double mean, double standardDeviation)
        {
            return mean + BoxMuller() * standardDeviation;
        }

        public static int Next(int min, int max)
        {
            return (int)BoxMuller(min + (max - min) / 2.0, 1.0);
        }
    }
}
