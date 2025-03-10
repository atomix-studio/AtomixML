﻿using Atom.MachineLearning.Core;
using Atom.MachineLearning.Core.Maths;
using Atom.MachineLearning.Core.Training;
using Atom.MachineLearning.IO;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Atom.MachineLearning.Unsupervised.BoltzmanMachine
{
    [Serializable]
    /// <summary>
    /// Contrastive divergence / sampling in action
    /// </summary>
    public class BooleanRBMTrainer : IMLTrainer<BooleanRBMModel, NVector, NVector>, IEpochIteratable, ITrainIteratable, IBatchedTrainIteratable
    {
        public BooleanRBMModel trainedModel { get; set; }

        [HyperParameter, SerializeField] private int _epochs = 10000;

        /*
         It is possible to update the weights after estimating the gradient on a single training case, but it is
            often more ecient to divide the training set into small “mini-batches” of 10 to 100 cases
         - Hinton, 2010
        */
        /// <summary>
        /// Number of batches 
        /// </summary>
        [HyperParameter, SerializeField] private int _batchSize = 5;

        [HyperParameter, SerializeField, Range(.01f, .99f)] private double _learningRate = .5f;
        [HyperParameter, SerializeField, Range(.01f, .99f)] private double _biasRate = .1f;
        [HyperParameter, SerializeField, Range(.001f, .99f)] private double _momentum = .01f;
        [HyperParameter, SerializeField, Range(.01f, .99f)] private double _weightDecay = .001f;
        /// <summary>
        /// Number of gibbs sample per training data negative phase
        /// </summary>
        [HyperParameter, SerializeField, Range(1, 10)] private int _k_steps = 1;
        [HyperParameter, SerializeField] private int _testRunsOnEndEpoch = 10;
        [HyperParameter, SerializeField] private int _averageWeightAndBiasesComputationInterval = 10;
        [Space]
        [ShowInInspector, ReadOnly] private int _currentEpoch;
        [Space]
        [ShowInInspector, ReadOnly] private double _mse;
        [ShowInInspector, ReadOnly] private double _freeVisibleEnergy;

        [ShowInInspector, ReadOnly] private double _weightAverage;
        [ShowInInspector, ReadOnly] private double _vBiasAverage;
        [ShowInInspector, ReadOnly] private double _hBiasAverage;

        private ITrainingSupervisor _trainingSupervisor;
        private NVector[] _x_datas;
        private NVector[] _t_datas;

        public BooleanRBMTrainer(BooleanRBMModel model)
        {
            trainedModel = model;
        }

        public async Task<ITrainingResult> Fit(NVector[] x_datas)
        {
            int split_index = (int)Math.Round((x_datas.Length * .8f));
            DatasetRWUtils.Split_TrainTest_NVector(x_datas, split_index, out _x_datas, out _t_datas);

            _trainingSupervisor = new StandardTrainingSupervisor();
            _trainingSupervisor.SetEpochIteration(this);
            _trainingSupervisor.SetTrainIteration(this);
            _trainingSupervisor.SetTrainBatchIteration(this);

            if (_batchSize == 1)
                await _trainingSupervisor.RunOnlineAsync(_epochs, _x_datas.Length, true);
            else
                await _trainingSupervisor.RunBatchedAsync(_epochs, _x_datas.Length, _batchSize, true);

            return new TrainingResult();
        }

        public ITrainingResult FitSynchronously(NVector[] x_datas)
        {
            throw new NotImplementedException();
        }

        public Task<double> Score()
        {
            throw new NotImplementedException();
        }

        public double ScoreSynchronously()
        {
            throw new NotImplementedException();
        }

        public void OnBeforeEpoch(int epochIndex)
        {
            _currentEpoch = epochIndex;
        }

        public void OnTrainNextBatch(int[] indexes)
        {
            NVector v, vP, h, hP;

            NVector vT = new NVector(trainedModel.visibleUnits);
            NVector vPT = new NVector(trainedModel.visibleUnits);
            NVector hT = new NVector(trainedModel.hiddenUnits);
            NVector hPT = new NVector(trainedModel.hiddenUnits);

            foreach (var index in indexes)
            {
                var next = _x_datas[index];
                trainedModel.Sample(next, _k_steps, out v, out vP, out h, out hP);

                vT += v;
                vPT += vP;
                hT += h;
                hPT += hP;

                if (index % _averageWeightAndBiasesComputationInterval == 0)
                {
                    _weightAverage = trainedModel.GetAverageWeights();
                    _hBiasAverage = trainedModel.GetAverageHiddenBias();
                    _vBiasAverage = trainedModel.GetAverageVisibleBias();
                }
            }

            vT /= indexes.Length;
            vPT /= indexes.Length;
            hT /= indexes.Length;
            hPT /= indexes.Length;

            trainedModel.UpdateWeightsAndBiases(vT, vPT, hT, hPT, _learningRate, _biasRate, _momentum, _weightDecay);
        }

        public void OnTrainNext(int index)
        {
            var next = _x_datas[index];
            trainedModel.Train(next, _k_steps, _learningRate, _biasRate, _momentum, _weightDecay);

            if (index % _averageWeightAndBiasesComputationInterval == 0)
            {
                _weightAverage = trainedModel.GetAverageWeights();
                _hBiasAverage = trainedModel.GetAverageHiddenBias();
                _vBiasAverage = trainedModel.GetAverageVisibleBias();
            }
        }

        public void OnAfterEpoch(int epochIndex)
        {
            /*
             It is easy to compute the squared error between the data and the reconstructions, so this quantity
            is often printed out during learning. The reconstruction error on the entire training set should fall
            rapidly and consistently at the start of learning and then more slowly. Due to the noise in the
            gradient estimates, the reconstruction error on the individual mini-batches will fluctuate gently after
            the initial rapid descent. It may also oscillate gently with a period of a few mini-batches when using
            high momentum (see section 9).
             */
            if (_t_datas == null || _t_datas.Length <= 1)
                return;

            _mse = 0.0;
            _freeVisibleEnergy = 0.0;
            for (int i = 0; i < _testRunsOnEndEpoch; ++i)
            {
                // todo test on test train 
                var next = MLRandom.Shared.Range(0, _t_datas.Length);
                var prediction = trainedModel.Predict(_t_datas[next]);
                _mse += MLCostFunctions.MSE(_t_datas[next], prediction);

                _freeVisibleEnergy = trainedModel.FreeVisibleEnergy(_t_datas[next]);
            }

            _mse /= _testRunsOnEndEpoch;
            _freeVisibleEnergy /= _testRunsOnEndEpoch;

        }

        public void Cancel()
        {
            _trainingSupervisor.Cancel();
        }

    }
}
