﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atom.MachineLearning.Core
{
    public struct NVector : IMLInOutData
    {
        public double[] Data { get; set; }

        public int Length => Data.Length;

        public double this[int index] => Data[index];

        public static NVector operator +(NVector a, NVector b)
        {
            if (a.Length != b.Length) throw new ArgumentException($"Vector dimensions aren't equals. A is {a.Length} and B is {b.Length}");

            double[] temp = new double[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                temp[i] = a[i] + b[i];
            }

            return new NVector(temp);
        }

        public static NVector operator -(NVector a, NVector b)
        {
            if (a.Length != b.Length) throw new ArgumentException($"Vector dimensions aren't equals. A is {a.Length} and B is {b.Length}");

            double[] temp = new double[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                temp[i] = a[i] - b[i];
            }

            return new NVector(temp);
        }

        public static NVector operator *(NVector a, double b)
        {
            double[] temp = new double[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                temp[i] = a[i] * b;
            }

            return new NVector(temp);
        }

        public static NVector operator /(NVector a, double b)
        {
            double[] temp = new double[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                temp[i] = a[i] / b;
            }

            return new NVector(temp);
        }

        public NVector(int dimensions)
        {
            Data = new double[dimensions];
        }

        public NVector(double[] arr)
        {
            Data = new double[arr.Length];

            for (int i = 0; i < arr.Length; ++i)
                Data[i] = arr[i];
        }

        public NVector(double x, double y)
        {
            Data = new double[] { x, y };
        }

        public NVector(double x, double y, double z)
        {
            Data = new double[] { x, y, z };
        }


        public static NVector Mean(NVector[] vectors)
        {
            int dimensions = vectors[0].Length;

            var mean = new NVector(dimensions);
            for (int i = 0; i < vectors.Length; ++i)
            {
                mean += vectors[i];
            }

            return mean /= vectors.Length;
        }

        public static double FeatureMean(NVector[] vectors, int featureIndex)
        {
            double sum = 0.0;
            for (int i = 0; i < vectors.Length; ++i)
            {
                sum += vectors[i][featureIndex];
            }

            return sum / vectors.Length;
        }

        public static double FeatureStandardDeviation(NVector[] vectors, double feature_mean, int featureIndex)
        {
            var sum = 0.0;
            for (int i = 0; i < vectors.Length; ++i)
            {
                sum += Math.Pow(vectors[i][featureIndex] - feature_mean, 2);
            }

            return Math.Sqrt((sum / vectors.Length));
        }


        public static double SampleCovariance(NVector a, NVector b)
        {
            if (a.Length != b.Length) throw new ArgumentException($"Vector dimensions aren't equals. A is {a.Length} and B is {b.Length}");

            double mean_a = a.Average();
            double mean_b = b.Average();

            double sum = 0.0;
            for (int i = 0; i < a.Length; i++)
            {
                sum += (a[i] - mean_a) * (b[i] - mean_b);
            }

            return sum / (a.Length - 1); // Use n-1 for sample covariance
        }

        public static double Covariance(NVector a, NVector b)
        {
            if (a.Length != b.Length) throw new ArgumentException($"Vector dimensions aren't equals. A is {a.Length} and B is {b.Length}");

            double mean_a = a.Average();
            double mean_b = b.Average();

            double sum = 0.0;
            for (int i = 0; i < a.Length; i++)
            {
                sum += (a[i] - mean_a) * (b[i] - mean_b);
            }

            return sum / a.Length; // Use n-1 for sample covariance
        }

    }

    public static class NVectorExtensions
    {
        /// <summary>
        /// Euclidian distance between two multidimensionnal vectors represented by float arrays
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double EuclidianDistanceTo(this NVector a, NVector b)
        {
            if (a.Length != b.Length) throw new ArgumentException($"Vector dimensions aren't equals. A is {a.Length} and B is {b.Length}");

            double result = 0;
            for (int i = 0; i < a.Length; ++i)
            {
                result += Math.Pow(a[i] - b[i], 2);
            }

            return Math.Sqrt(result);
        }

        public static double Average(this NVector vector)
        {
            double val = 0;
            for (int i = 0; i < vector.Length; ++i)
            {
                val += vector[i];
            }

            return val / vector.Length;
        }

    }
}
