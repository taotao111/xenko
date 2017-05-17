﻿// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Streaming
{
    /// <summary>
    /// Texture streaming object.
    /// </summary>
    public class StreamingTexture : StreamableResource
    {
        protected Texture _texture;
        protected ImageDescription _desc;
        protected int _residentMips;
        protected Task _streamingTask;

        internal StreamingTexture(StreamingManager manager, [NotNull] Texture texture)
            : base(manager)
        {
            _texture = texture;
            this.DisposeBy(_texture);
            _residentMips = 0;
        }

        /// <summary>
        /// Gets the texture object.
        /// </summary>
        public Texture Texture => _texture;

        /// <summary>
        /// Gets the texture image description (available in the storage container).
        /// </summary>
        public ImageDescription Description => _desc;

        /// <summary>
        /// Gets the total amount of mip levels.
        /// </summary>
        public int TotalMipLevels => _desc.MipLevels;

        /// <summary>
        /// Gets the width of maximum texture mip.
        /// </summary>
        public int TotalWidth => _desc.Width;

        /// <summary>
        /// Gets the height of maximum texture mip.
        /// </summary>
        public int TotalHeight => _desc.Height;

        /// <summary>
        /// Gets the number of textures in an array.
        /// </summary>
        public int ArraySize => _desc.ArraySize;

        /// <summary>
        /// Gets a value indicating whether this texture is a cube map.
        /// </summary>
        /// <value><c>true</c> if this texture is a cube map; otherwise, <c>false</c>.</value>
        public bool IsCubeMap => _desc.Dimension == TextureDimension.TextureCube;
        
        /// <summary>	
        /// Gets the texture texels format
        /// </summary>	
        public PixelFormat Format => _desc.Format;

        /// <summary>
        /// Gets index of the highest resident mip map (may be equal to MipLevels if no mip has been uploaded). Note: mip=0 is the highest (top quality)
        /// </summary>
        /// <returns>Mip index</returns>
        public int HighestResidentMipIndex => TotalMipLevels - _residentMips;

        /// <inheritdoc />
        public override object Resource => _texture;

        /// <inheritdoc />
        public override int CurrentResidency => _residentMips;

        /// <inheritdoc />
        public override int AllocatedResidency => Texture.MipLevels;

        /// <inheritdoc />
        public override int MaxResidency => _desc.MipLevels;

        /// <inheritdoc />
        internal override bool CanBeUpdated => _streamingTask == null || _streamingTask.IsCompleted;

        internal void Init([NotNull] ContentStorage storage, ref ImageDescription imageDescription)
        {
            if(imageDescription.Depth != 1)
                throw new NotSupportedException("Texture streaming supports only 2D textures and 2D texture arrays.");

            Init(storage);
            _desc = imageDescription;
            _residentMips = 0;

            if (Texture.GraphicsDevice != null)
                Texture.OnDestroyed();
        }
        
        private void StreamingTask(int residency)
        {
            // Cache data
            var texture = Texture;
            int mipsChange = residency - CurrentResidency;
            int mipsCount = residency;
            Debug.Assert(mipsChange != 0);
            
            // TODO: allocation task should dispose texture or merge those tasks?
            if (residency == 0)
            {
                texture.OnDestroyed();
                return;
            }

            try
            {
                Storage.LockChunks();

                // Setup texture description
                TextureDescription newDesc = _desc;
                int newHighestResidentMipIndex = TotalMipLevels - mipsCount;
                newDesc.MipLevels = mipsCount;
                newDesc.Width = TotalWidth >> (_desc.MipLevels - newDesc.MipLevels);
                newDesc.Height = TotalHeight >> (_desc.MipLevels - newDesc.MipLevels);
                var dataBoxes = new DataBox[newDesc.MipLevels * newDesc.ArraySize];
                bool isBlockCompressed =
                    (Format >= PixelFormat.BC1_Typeless && Format <= PixelFormat.BC5_SNorm) ||
                    (Format >= PixelFormat.BC6H_Typeless && Format <= PixelFormat.BC7_UNorm_SRgb);
                int dataBoxIndex = 0;

                // Get data boxes data
                for (int arrayIndex = 0; arrayIndex < newDesc.ArraySize; arrayIndex++)
                {
                    for (int mipIndex = 0; mipIndex < newDesc.MipLevels; mipIndex++)
                    {
                        int totalMipIndex = newHighestResidentMipIndex + mipIndex;
                        int mipWidth = TotalWidth >> totalMipIndex;
                        int mipheight = TotalHeight >> totalMipIndex;

                        if (isBlockCompressed && ((mipWidth % 4) != 0 || (mipheight % 4) != 0))
                        {
                            mipWidth = unchecked((int)(((uint)(mipWidth + 3)) & ~(uint)3));
                            mipheight = unchecked((int)(((uint)(mipheight + 3)) & ~(uint)3));
                        }

                        int rowPitch, slicePitch;
                        int widthPacked;
                        int heightPacked;
                        Image.ComputePitch(Format, mipWidth, mipheight, out rowPitch, out slicePitch, out widthPacked, out heightPacked);

                        var chunk = Storage.GetChunk(totalMipIndex);
                        if (chunk == null || chunk.Size != slicePitch * newDesc.ArraySize)
                            throw new DataException("Data chunk is missing or has invalid size.");
                        var data = chunk.GetData(fileProvider);
                        if (!chunk.IsLoaded)
                            throw new DataException("Data chunk is not loaded.");

                        unsafe
                        {
                            fixed (byte* p = data)
                                dataBoxes[dataBoxIndex].DataPointer = (IntPtr)p + slicePitch * arrayIndex;
                            dataBoxes[dataBoxIndex].RowPitch = rowPitch;
                            dataBoxes[dataBoxIndex].SlicePitch = slicePitch;
                            dataBoxIndex++;
                        }

                        if (IsDisposed) // TODO: use cancellation token
                            return;
                    }
                }

                // Recreate texture
                texture.OnDestroyed();
                texture.InitializeFrom(newDesc, new TextureViewDescription(), dataBoxes);
                _residentMips = newDesc.MipLevels;
            }
            finally
            {
                Storage.UnlockChunks();
            }
        }

        internal override Task StreamAsync(int residency)
        {
            Debug.Assert(CanBeUpdated && residency <= MaxResidency);

            return _streamingTask = new Task(() => StreamingTask(residency));
        }

        /// <inheritdoc />
        protected override void Destroy()
        {
            // Stop streaming
            // TODO: stop steaming using cancellationToken

            base.Destroy();
        }
    }
}