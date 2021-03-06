// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// An interface that represents an asset factory.
    /// </summary>
    /// <typeparam name="T">The type of asset this factory can create.</typeparam>
    [AssemblyScan]
    public interface IAssetFactory<out T> where T : Asset
    {
        /// <summary>
        /// Retrieve the asset type associated to this factory.
        /// </summary>
        /// <returns>The asset type associated to this factory.</returns>
        [NotNull]
        Type AssetType { get; }

        /// <summary>
        /// Creates a new instance of the asset type associated to this factory.
        /// </summary>
        /// <returns>A new instance of the asset type associated to this factory.</returns>
        T New();
    }
}
