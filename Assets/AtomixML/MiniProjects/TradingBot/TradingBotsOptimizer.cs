﻿using Atom.MachineLearning.Core.Maths;
using Atom.MachineLearning.Core.Optimization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Atom.MachineLearning.MiniProjects.TradingBot
{
    [Serializable]
    public class TradingBotsOptimizer : GeneticOptimizerBase<TradingBotEntity, decimal, int>
    {
        [Header("Training Bot Optimizer Parameters")]
        [SerializeField, Range(0f, 1f)] private float _batchSizeRatio = .02f;
        [SerializeField] private float _profitScoreBonusMultiplier = 3;
        [SerializeField] private float _gainLossRatioMultiplier = 2;
        //[SerializeField] private float _learningRate = .01f;
        [Space]
        [SerializeField] float[] _minMaxLearningRate = new float[] { 1.5f, .1f };
        [SerializeField] float[] _minMaxBatchesSize = new float[] { .05f, .50f };

        private TradingBotManager _manager;
        private Func<TradingBotEntity> _tradingBotCreateDelegate;

        public double adaptiveLearningRate => MLMath.InverseLerp(_minMaxLearningRate[0], _minMaxLearningRate[1], (float)CurrentIteration / (float)MaxIterations);
        public double adaptiveBatchSizeRatio => MLMath.InverseLerp(_minMaxBatchesSize[0], _minMaxBatchesSize[1], (float)CurrentIteration / (float)MaxIterations);

        public void Initialize(TradingBotManager manager, CancellationToken cancellationToken, Func<TradingBotEntity> tradingBotCreateDelegate)
        {
            _manager = manager;
            _tradingBotCreateDelegate = tradingBotCreateDelegate;
            this.cancellationToken = cancellationToken;
        }

        public override TradingBotEntity CreateEntity()
        {
            return _tradingBotCreateDelegate();
        }

        public override async Task ComputeGeneration()
        {
            // run a complete epoch on market datas with all entities
            await _manager.RunEpochParallel2(CurrentGenerationEntities, cancellationToken, true, adaptiveBatchSizeRatio);
        }

        public override double GetEntityScore(TradingBotEntity entity)
        {
            /*// we do transactionCount - 1 because we do automatic closing transaction on end epoch but we don't want to take this in account
            if (entity.sellTransactionsCount <= 0)
                return 0;
           
            // to do prise en compte des stocks sur la valeur à la fin ? 
            // on pousse la marge moyenne (par transaction) et le volume aussi
            var score = Convert.ToDouble(entity.meanMargin * Math.Abs(entity.totalMargin)) * _profitScoreBonusMultiplier;

            if (_transactionsScoreMalusMultiplier != 0)
                if (entity.sellTransactionsCount == 1)
                    score *= .05;
                else
                    score *= Math.Log(entity.sellTransactionsCount * _transactionsScoreMalusMultiplier, 2);

            score += entity.totalHoldingTime * _transactionHoldingTimeMultiplier;*/

            var score = Convert.ToDouble(entity.totalBalance) * _profitScoreBonusMultiplier - entity.sellTransactionsCount - entity.buyTransactionsCount;
                //Math.Log(Math.Pow(1 + entity.gainLossRatio * _gainLossRatioMultiplier, 2));

            return score;
        }

        public override void OnObjectiveReached(TradingBotEntity bestEntity)
        {
            Debug.Log($"Best entity on training. Amout {bestEntity.walletAmount} $, Transactions Done : {bestEntity.sellTransactionsCount}");
        }

        protected override void ClearPreviousGeneration(List<TradingBotEntity> previousGenerationEntities)
        {

        }
    }
}
