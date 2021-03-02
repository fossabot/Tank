﻿namespace Tank.Interfaces.EntityComponentSystem
{
    /// <summary>
    /// This game object can be cleared
    /// </summary>
    interface IClearable
    {        
        /// <summary>
        /// Clear the event manager
        /// </summary>
        void Clear();
    }
}
