﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atom.MachineLearning.Core.Maths
{
    public static class MLRandom
    {
        private static Random _shared;

        public static Random Shared
        {
            get
            {
                if (_shared == null)
                {
                    _shared = new System.Random(Guid.NewGuid().GetHashCode());
                }

                return _shared;
            }
        }

        public static void SeedShared(int seed)
        {
            _shared = new System.Random(seed);
        }

        public static double GaussianNoise(this Random random, double mean = 0, double stdDev = 1.0)
        {
            double u1 = 1.0 - random.NextDouble();
            double u2 = 1.0 - random.NextDouble();

            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

            return mean + stdDev * randStdNormal;
        }

        public static double Range(this Random random, double minValue, double maxValue)
        {
            return random.NextDouble() * (maxValue - minValue) + minValue;
        }

        public static float Range(this Random random, float minValue, float maxValue)
        {
            return (float)(random.NextDouble() * (maxValue - minValue) + minValue);
        }

        public static int Range(this Random random, int minValue, int maxValue)
        {
            if (minValue > maxValue)
                return minValue;

            return random.Next(minValue, maxValue);
        }

        public static bool Chances(this Random random, double chances, double maxValue)
        {
            return Range(random, 0, maxValue) >= maxValue - chances;
        }

        public static int WeightedIndex(this Random random, double[] weights)
        {
            // Normalize weights to sum to 1 (in case they're not already percentages)
            double totalWeight = 0;
            foreach (var weight in weights)
                totalWeight += weight;

            // Create cumulative probabilities
            double[] cumulative = new double[weights.Length];
            double cumulativeSum = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                cumulativeSum += weights[i] / totalWeight;
                cumulative[i] = cumulativeSum;
            }

            // Generate a random number between 0 and 1
            double randomValue = random.NextDouble();

            // Determine the index based on the random value
            for (int i = 0; i < cumulative.Length; i++)
            {
                if (randomValue <= cumulative[i])
                    return i;
            }

            // Fallback (should not reach here if weights are correct)
            return -1;
        }

    }
}
