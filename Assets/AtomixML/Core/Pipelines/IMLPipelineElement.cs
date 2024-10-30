﻿using System.Threading.Tasks;

namespace Atom.MachineLearning.Core
{
    /// <summary>
    /// A pipeline element can be either a model, a transformation layer, etc...
    /// </summary>
    public interface IMLPipelineElement<T, K> where T : IMLInputData where K : IMLOutputData
    {
        public Task<K> Predict(T inputData);
    }
}
