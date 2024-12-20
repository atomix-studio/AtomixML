﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atom.MachineLearning.NeuralNetwork
{
    public static class NeuralNetworkMathHelper
    {
        #region Miscellaneous
       
        public static float Norm01(int min, int max, float input)
        {
            return (input - min) / (max - min);
        }

        public static float Map(float value, float inputMin, float inputMax, float outputMin, float outputMax)
        {
            float deltaIn = inputMax - inputMin;
            float deltaOut = outputMax - outputMin;

            float ratio = (value - inputMin) / deltaIn;
            return ratio * deltaOut + outputMin;
        }

        public static double[][] NormalizeData(double[][] allData, int columnsToNorm)
        {
            double[][] normalizedData = allData;
            double minValue = int.MaxValue;
            double maxValue = 0;


            for (int i = 0; i < normalizedData.Length; ++i)
            {
                double minTemp = normalizedData[i].Min();
                minValue = minTemp < minValue ? minTemp : minValue;

                double maxTemp = normalizedData[i].Max();
                maxValue = maxTemp > maxValue ? maxTemp : maxValue;
            }

            double delta = maxValue - minValue;

            for (int i = 0; i < normalizedData.Length; ++i)
            {
                for (int j = 0; j < normalizedData[i].Length; ++j)
                {
                    if (j < columnsToNorm)
                    {
                        double nVal = (normalizedData[i][j] - minValue) / delta;
                        normalizedData[i][j] = nVal;
                    }
                }
            }

            return normalizedData;
        }

        public static int MaxIndex(double[] vector) // helper for Accuracy()
        {
            // index of largest value
            int bigIndex = 0;
            double biggestVal = vector[0];
            for (int i = 0; i < vector.Length; ++i)
            {
                if (vector[i] > biggestVal)
                {
                    biggestVal = vector[i]; 
                    bigIndex = i;
                }
            }
            return bigIndex;
        }

        public static double[,] MakeMatrix(int x, int y)
        {
            double[,] matrix = new double[x, y];
            return matrix;
        }

        #endregion

        #region ActivationFunctions

        public static double ComputeActivation(ActivationFunctions type, bool derivative, double x, double[] a = null)
        {
            double result = 0;
            switch (type)
            {
                case ActivationFunctions.None:
                    return x;
                case ActivationFunctions.Linear:
                    if (!derivative)
                    {
                        result = x;
                    }
                    else
                    {
                        result = 1;
                    }

                    break;
                case ActivationFunctions.Sigmoid:
                    if (!derivative)
                    {
                        result = Logistic(x);
                    }
                    else
                    {
                        result = DLogistic(x);
                    }

                    break;
                case ActivationFunctions.ReLU:
                    if (!derivative)
                    {
                        result = ReLU(x);
                    }
                    else
                    {
                        result = DReLU(x);
                    }

                    break;

                case ActivationFunctions.Tanh:
                    if (!derivative)
                    {
                        result = Tanh(x);
                    }
                    else
                    {
                        result = DTanh(x);
                    }

                    break;
                case ActivationFunctions.Sinusoid:
                    if (!derivative)
                    {
                        result = Sinusoid(x);
                    }
                    else
                    {
                        result = DSinusoid(x);
                    }

                    break;
                case ActivationFunctions.Gaussian:
                    if (!derivative)
                    {
                        result = Gaussian(x);
                    }
                    else
                    {
                        result = DGaussian(x);
                    }

                    break;
                case ActivationFunctions.PReLU:
                    if (!derivative)
                    {
                        result = PReLU(x, 0.1f);
                    }
                    else
                    {
                        result = DPReLU(x, 0.1f);
                    }

                    break;
                case ActivationFunctions.ELU:
                    if (!derivative)
                    {
                        result = ELU(x, 0.1f);
                    }
                    else
                    {
                        result = DELU(x, 0.1f);
                    }

                    break;
                case ActivationFunctions.Softmax:
                    if (derivative)
                    {
                        result = DLogistic(x);
                    }
                    break;
                case ActivationFunctions.Boolean:
                    if (!derivative)
                    {
                        result = x > 0 ? 1 : 0;
                    }
                    else
                    {
                        result = x != 0 ? 0 : 1; // ?? Cannot calculate derivative of boolean function

                    }
                    break;
            }
            return result;
        }

        public static double ReLU(double x)
        {
            return Math.Max(0, x);// x < 0 ? 0 : x;
        }

        public static double DReLU(double x)
        {
            return Math.Max(0, 1);// x < 0 ? 0 : x;
        }

        public static double Logistic(double x)
        {
            double result = 0;

            if (x > 45)
            {
                result = 1;
            }
            else
            {
                result = 1 / (1 + Math.Exp(-x));
            }
            return result;
        }

        public static double DLogistic(double x)
        {
            return x * (1 - x);
        }

        public static double Tanh(double x)
        {
            if (x < -20.0) return -1.0; // approximation is correct to 30 decimals
            else if (x > 20.0) return 1.0;
            else return Math.Tanh(x);
        }

        public static double DTanh(double x)
        {
            return (1 - x) * (1 + x);
        }

        public static double Sinusoid(double x)
        {
            return Math.Sin(x);
        }

        public static double DSinusoid(double x)
        {
            return Math.Cos(x);
        }

        public static double Gaussian(double x)
        {
            return Math.Pow(Math.E, Math.Pow(-x, 2));
        }

        public static double DGaussian(double x)
        {
            return -2 * x * Math.Pow(Math.E, Math.Pow(-x, 2));
        }

        public static double PReLU(double x, double a)
        {
            return x < 0 ? a * x : x;
        }

        public static double DPReLU(double x, double a)
        {
            return x < 0 ? a : 1;
        }

        //Exponential Linear Unit 
        public static double ELU(double x, double a)
        {
            return x < 0 ? a * (Math.Pow(Math.E, x) - 1) : x;
        }

        public static double DELU(double x, double a)
        {
            return x < 0 ? ELU(x, a) + a : 1;
        }

        public static double[] Softmax(double[] oSums)
        {
            // does all output nodes at once so scale doesn't have to be re-computed each time
            // 1. determine max output sum
            double max = oSums[0];
            for (int i = 0; i < oSums.Length; ++i)
                if (oSums[i] > max) max = oSums[i];

            // 2. determine scaling factor -- sum of exp(each val - max)
            double scale = 0.0;
            for (int i = 0; i < oSums.Length; ++i)
                scale += Math.Exp(oSums[i] - max);

            double[] result = new double[oSums.Length];
            for (int i = 0; i < oSums.Length; ++i)
                result[i] = Math.Exp(oSums[i] - max) / scale;

            return result; // now scaled so that xi sum to 1.0
        }
        #endregion
    }
}
