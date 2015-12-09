﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Xenko.Shaders;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Particles.VertexLayouts;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Particles
{
    [Flags]
    public enum ParticleEffectVariation
    {
        None = 0x00,
        IsSrgb = 0x01,
        HasTex0 = 0x02,
        HasColor = 0x04,
    }

    public partial class ParticleBatch
    {
        // TODO We need static bytecode to initialize the ParticleBatch, but they are not actually being used
        // After the ParticleBatch has been refactored to not use BatchBase, we can remove the static bytecode as well

        private const int MaxEffectVariations =
            (int)(ParticleEffectVariation.IsSrgb |
                  ParticleEffectVariation.HasTex0 |
                  ParticleEffectVariation.HasColor) + 1;

        private static Effect[] effect = new Effect[MaxEffectVariations];
        private static EffectBytecode[] effectBytecode = new EffectBytecode[MaxEffectVariations];

        private static EffectBytecode Bytecode(ParticleEffectVariation variation)
        {
            if (variation.HasFlag(ParticleEffectVariation.IsSrgb))
            {
                if (variation.HasFlag(ParticleEffectVariation.HasTex0))
                    return effectBytecode[(int)(ParticleEffectVariation.IsSrgb | ParticleEffectVariation.HasTex0)] ??
                          (effectBytecode[(int)(ParticleEffectVariation.IsSrgb | ParticleEffectVariation.HasTex0)] = EffectBytecode.FromBytes(binaryBytecodeSRgbTex0));

                return effectBytecode[(int)ParticleEffectVariation.IsSrgb] ??
                      (effectBytecode[(int)ParticleEffectVariation.IsSrgb] = EffectBytecode.FromBytes(binaryBytecodeSRgb));
            }

            if (variation.HasFlag(ParticleEffectVariation.HasTex0))
                return effectBytecode[(int)ParticleEffectVariation.HasTex0] ??
                          (effectBytecode[(int)ParticleEffectVariation.HasTex0] = EffectBytecode.FromBytes(binaryBytecodeTex0));

            return effectBytecode[(int)ParticleEffectVariation.None] ??
                  (effectBytecode[(int)ParticleEffectVariation.None] = EffectBytecode.FromBytes(binaryBytecode));
        }

        public static Effect GetEffect(GraphicsDevice device, ParticleEffectVariation variation)
        {
            return effect[(int)variation] ?? (effect[(int)variation] = new Effect(device, Bytecode(variation)));
        }

        public static ParticleVertexLayout GetVertexLayout(ParticleEffectVariation variation)
        {
            if (variation.HasFlag(ParticleEffectVariation.HasTex0))
                return new ParticleVertexLayoutTextured();

            return new ParticleVertexLayoutPlain();
        }
    }
}