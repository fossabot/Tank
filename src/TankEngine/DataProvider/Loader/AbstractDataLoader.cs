﻿using System;
using System.Threading.Tasks;

namespace TankEngine.DataProvider.Loader
{
    /// <summary>
    /// Abstract class of data loader which impletements the second load data method
    /// </summary>
    /// <typeparam name="T">The type of the loader</typeparam>
    public abstract class AbstractDataLoader<T> : IDataLoader<T>
    {
        /// <summary>
        /// Path to the base folder
        /// </summary>
        protected readonly string baseFolder;

        /// <summary>
        /// Create a new instance of this class without a base folder
        /// </summary>
        public AbstractDataLoader() : this(string.Empty) { }

        /// <summary>
        /// Create a new instance of this class with given basefolder
        /// </summary>
        /// <param name="baseFolder">The base folder to use</param>
        public AbstractDataLoader(string baseFolder)
        {
            this.baseFolder = baseFolder;
        }

        /// <inheritdoc/>
        public abstract T LoadData(string fileName);

        /// <inheritdoc/>
        public virtual async Task<T> LoadDataAsync(string fileName)
        {
            return await Task.Run(() => LoadData(fileName));
        }

        /// <inheritdoc/>
        public virtual C LoadData<C>(string fileName, Func<T, C> dataConversion)
        {
            return dataConversion(LoadData(fileName));
        }

        /// <inheritdoc/>
        public virtual async Task<C> LoadDataAsync<C>(string fileName, Func<T, C> dataConversion)
        {
            return await Task.Run(() => LoadData(fileName, dataConversion));
        }
    }
}
