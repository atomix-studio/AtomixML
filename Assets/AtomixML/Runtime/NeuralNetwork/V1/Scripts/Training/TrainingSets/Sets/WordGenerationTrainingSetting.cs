﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Atom.MachineLearning.NeuralNetwork
{
    [CreateAssetMenu(menuName = "TrainingSets/WordGenerationTrainingSetting")]
    public class WordGenerationTrainingSetting : TrainingSettingBase
    {
        public RealFalseWordRecognitionTrainingSetting RealFalseWordRecognitionTrainingSetting;

        public NeuralNetwork DiscriminantNetwork;

        public int WordMinLenght;
        public int WordMaxLenght;

        public override void Init()
        {
        }

        public override void GetNextValues(out double[] x_val, out double[] t_val)
        {
            var test = new double[0];

            x_val = new double[WordMaxLenght];
            t_val = new double[WordMaxLenght];

            int word_length = UnityEngine.Random.Range(WordMinLenght, WordMaxLenght);

            for (int i = 0; i < WordMaxLenght; ++i)
            {
                if(i < word_length)
                {
                    x_val[i] = UnityEngine.Random.Range(0, 30);
                }
                else
                {
                    x_val[i] = -1;
                }
            }
        }

        public override bool ValidateRun(double[] y_val, double[] t_val)
        {
            return false;
        }
    }
}

