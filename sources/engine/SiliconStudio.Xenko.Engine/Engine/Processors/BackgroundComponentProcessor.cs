// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Collections.Generic;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Sprites;

namespace SiliconStudio.Xenko.Engine.Processors
{
    /// <summary>
    /// The processor in charge of updating and drawing the entities having background components.
    /// </summary>
    internal class BackgroundComponentProcessor : EntityProcessor<BackgroundComponent>
    {
        public List<BackgroundComponent> Backgrounds { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundComponentProcessor"/> class.
        /// </summary>
        public BackgroundComponentProcessor()
        {
            Backgrounds = new List<BackgroundComponent>();
        }

        public override void Draw(RenderContext gameTime)
        {
            Backgrounds.Clear();
            foreach (var backgroundKeyPair in ComponentDatas)
            {
                var background = backgroundKeyPair.Key;
                if (background.Enabled)
                {
                    Backgrounds.Add(background);
                }
            }
        }
    }
}
