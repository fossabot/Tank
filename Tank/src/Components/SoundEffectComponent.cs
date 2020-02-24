﻿using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tank.src.Components
{
    class SoundEffectComponent : BaseComponent
    {
        /// <summary>
        /// Name of the sound effect
        /// </summary>
        public string Name;   

        /// <summary>
        /// Readonly access to the sound effect to use
        /// </summary>
        private readonly SoundEffect soundEffect;

        /// <summary>
        /// Public readonly access to the sound effect
        /// </summary>
        public SoundEffect SoundEffect => soundEffect;

        /// <summary>
        /// Create a new instance of this container
        /// </summary>
        /// <param name="soundEffect">The sound effect to use</param>
        public SoundEffectComponent(SoundEffect soundEffect)
        {
            this.soundEffect = soundEffect;
        }

    }
}
