﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NeuralNetwork
{
    public abstract class TrainingSettingBase : ScriptableObject
    {
        public int TrainingDataLenght = 15000;
        public int TestingDataLenght = 15000;

        protected double[][] x_datas;
        protected double[][] t_datas;

        public abstract void Init();

        public virtual void GetTrainDatas(out double[][] x_datas, out double[][] t_datas) { x_datas = null; t_datas = null; }

        public abstract void GetNextValues(out double[] x_val, out double[] t_val);
        
        public abstract bool ValidateRun(double[] y_val, double[] t_val);        
    }
}
