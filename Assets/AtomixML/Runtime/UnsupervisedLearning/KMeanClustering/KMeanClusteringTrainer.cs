﻿using Atom.MachineLearning.Core;
using Atom.MachineLearning.Core.Training;
using Atom.MachineLearning.IO;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Atom.MachineLearning.Unsupervised.KMeanClustering
{
    public class KMeanClusteringTrainer : MonoBehaviour, IMLTrainer<KMeanClusteringModel, NVector, KMeanClusteringOutputData>
    {
        [HyperParameter, SerializeField] private int _epochs;

        [ShowInInspector, ReadOnly] private int _currentEpoch;
        public int currentEpoch => _currentEpoch;

        public KMeanClusteringModel trainedModel { get; set ; }

        /// <summary>
        /// Euclidian distance computed for each point and classified by label
        /// </summary>
        private List<List<(NVector, double)>> _epoch_results = new List<List<(NVector, double)>>();
        private List<double[]> _clusters_barycenter = new List<double[]>();
        private NVector[] _x_datas;

        #region test / to be moved in implementation scripts
        [Button]
        public async void TestFit_SimpleKMC2D(int minClusters = 2, int maxClusters = 6, int parallelRuns = 3)
        {
            var set = new SimpleKMCTwoDimensionalTrainingSet();

            int delta = maxClusters - minClusters;

            for (int i = 0; i < delta; ++i)
            {
                int clusterCount = minClusters + i;

                for (int j = 0; j < parallelRuns; ++j)
                {
                    trainedModel = new KMeanClusteringModel(clusterCount, new double[] { 10, 10 });
                    var result = await Fit(set.Features);

                    Debug.Log($"Parallel run {j} > {clusterCount} clusters, total variance = {result.Accuracy}");

                }
            }
        }

        [Button]
        private async void TestFitFlowers(int minClusters = 2, int maxClusters = 6, int parallelRuns = 3)
        {
            var datas = Datasets.Flowers_All();

            DatasetRWUtils.SplitLastColumn(datas, out var features, out var labels);

            var vectorized_labels = TransformationUtils.Encode(labels, 3, new Dictionary<string, double[]>()
            {
                { "Iris-setosa", new double[] { 0, 0, 1 } },
                { "Iris-versicolor", new double[] { 0, 1, 0 } },
                { "Iris-virginica", new double[] { 1, 0, 0 } },
            });

            var vectorized_features = NVector.Standardize(TransformationUtils.StringMatrix2DToDoubleMatrix2D(features).ToNVectorRowsArray(), out var means, out var stdDeviations, out _);            

            int delta = maxClusters - minClusters;

            for (int i = 0; i < delta; ++i)
            {
                int clusterCount = minClusters + i;

                for (int j = 0; j < parallelRuns; ++j)
                {
                    trainedModel = new KMeanClusteringModel(clusterCount, new double[] { 2, 2, 2, 2 });
                    var result = await Fit(vectorized_features);
                    Debug.Log($"Parallel run {j} > {clusterCount} clusters, total variance = {result.Accuracy}");
                }
            }
        }

        [Button]
        public void TestRunFlower()
        {
            var datas = Datasets.Flowers_All();

            DatasetRWUtils.SplitLastColumn(datas, out var features, out var labels);

            var vectorized_labels = TransformationUtils.Encode(labels, 3, new Dictionary<string, double[]>()
            {
                { "Iris-setosa", new double[] { 0, 0, 1 } },
                { "Iris-versicolor", new double[] { 0, 1, 0 } },
                { "Iris-virginica", new double[] { 1, 0, 0 } },
            });

            var vectorized_features = NVector.Standardize(TransformationUtils.StringMatrix2DToDoubleMatrix2D(features).ToNVectorRowsArray(), out var means, out var stdDeviations, out _);

            for(int i = 0; i < vectorized_features.Length; ++i)
            {
                var output = trainedModel.Predict(vectorized_features[i]);

                Debug.Log($"Output class : {output.ClassLabel}. Real expected label : {labels[i]}");
            }
        }
        #endregion

        public async Task<ITrainingResult> Fit(NVector[] trainingDatas)
        {
            return FitSynchronously(trainingDatas);  
        }

        public ITrainingResult FitSynchronously(NVector[] x_datas)
        {
            _x_datas = x_datas;
            _epoch_results.Clear();
            _clusters_barycenter.Clear();

            for (int i = 0; i < trainedModel.clustersCount; ++i)
            {
                _epoch_results.Add(new List<(NVector, double)>());
                _clusters_barycenter.Add(new double[_x_datas[0].Data.Length]);
            }

            // run epochs
            for (_currentEpoch = 0; _currentEpoch < _epochs; _currentEpoch++)
            {
                // run the batch 
                for (int j = 0; j < _x_datas.Length; ++j)
                {
                    // predict
                    var result = trainedModel.Predict(_x_datas[j]);

                    // save the result of each prediction in a cluster list
                    _epoch_results[result.ClassLabel].Add(new(_x_datas[j], result.Euclidian));
                }

                // compute average
                ComputeClusterBarycenters();

                // update centroids
                trainedModel.UpdateCentroids(_clusters_barycenter);

                // clear buffers
                for (int i = 0; i < trainedModel.clustersCount; ++i)
                {
                    _epoch_results[i].Clear();

                    for (int j = 0; j < _clusters_barycenter[i].Length; ++j)
                        _clusters_barycenter[i][j] = 0;
                }
            }

            // test run
            for (int j = 0; j < _x_datas.Length; ++j)
            {
                // predict
                var result = trainedModel.Predict(_x_datas[j]);

                // save the result of each prediction in a cluster list
                _epoch_results[result.ClassLabel].Add(new(_x_datas[j], result.Euclidian));
            }

            // variance compute
            double total_variance = 0;
            double[] clusters_variance = new double[trainedModel.clustersCount];

            for (int classIndex = 0; classIndex < _epoch_results.Count; ++classIndex)
            {
                // summing all results for each cluster in a mean vector (array)
                for (int j = 0; j < _epoch_results[classIndex].Count; ++j)
                {
                    clusters_variance[classIndex] += _epoch_results[classIndex][j].Item2;
                    total_variance += _epoch_results[classIndex][j].Item2;
                }
            }

            return new TrainingResult()
            {
                Accuracy = (float)total_variance
            };
        }

        public Task<double> Score()
        {
            throw new NotImplementedException();
        }

        public double ScoreSynchronously()
        {
            throw new NotImplementedException();
        }

        private void ComputeClusterBarycenters()
        {
            for (int i = 0; i < _clusters_barycenter.Count; ++i)
                for (int j = 0; j < _clusters_barycenter[i].Length; ++j)
                    _clusters_barycenter[i][j] = 0;

            for (int classIndex = 0; classIndex < _epoch_results.Count; ++classIndex)
            {
                // summing all results for each cluster in a mean vector (array)
                for (int j = 0; j < _epoch_results[classIndex].Count; ++j)
                {
                    for (int k = 0; k < _epoch_results[classIndex][j].Item1.Data.Length; ++k)
                        _clusters_barycenter[classIndex][k] += _epoch_results[classIndex][j].Item1.Data[k];
                }

                // barycenter compute by divided the sum by the elements count
                for (int j = 0; j < _clusters_barycenter[classIndex].Length; ++j)
                {
                    _clusters_barycenter[classIndex][j] /= _epoch_results[classIndex].Count;
                    // now we have the new cluster position
                }
            }
        }

        // TODO Elbow Function / Automatic BestFit with ranged min-max cluster 
        // TODO Save
        // TODO proper visualization

        #region Tests 

        Color[] _epochs_colors;


        void OnDrawGizmos()
        {
            if (_x_datas == null)
                return;

            if (trainedModel == null)
                return;

            foreach (var item in _x_datas)
                Gizmos.DrawSphere(new Vector3((float)item.Data[0], (float)item.Data[1], 0), .3f);

            if (_epoch_results == null)
                return;

            if (_epochs_colors == null || _epochs_colors.Length != _epoch_results.Count)
            {
                _epochs_colors = new Color[_epoch_results.Count];
                for(int i = 0; i < _epochs_colors.Length; ++i)
                    _epochs_colors[i] = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
            }

            for (int i = 0; i < trainedModel.centroids.Count; ++i)
            {
                Gizmos.color = _epochs_colors[i];
                Gizmos.DrawSphere(new Vector3((float)trainedModel.centroids[i][0], (float)trainedModel.centroids[i][1], 0), .5f);

                foreach (var item in _epoch_results[i])
                {
                    Gizmos.DrawSphere(new Vector3((float)item.Item1.Data[0], (float)item.Item1.Data[1], 0), .2f);
                }
            }
        }

        #endregion
    }
}
