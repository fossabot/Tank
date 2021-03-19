﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Tank.Components;
using Tank.Components.Rendering;
using Tank.Components.Tags;
using Tank.DataStructure;
using Tank.GameStates.Data;
using Tank.Interfaces.EntityComponentSystem;

namespace Tank.Builders
{
    /// <summary>
    /// Create new tank object
    /// </summary>
    class TankObjectBuilder : BaseBuilder
    {

        /// <summary>
        /// The spritesheet to use
        /// </summary>
        private readonly Texture2D spriteSheet;

        /// <summary>
        /// The animation frames
        /// </summary>
        private readonly List<Rectangle> animationFrames;
        private readonly Rectangle colliderDestination;

        /// <summary>
        /// The shader effect to use
        /// </summary>
        private readonly Effect effect;

        /// <summary>
        /// Create a new instance of this class to spawn game objects
        /// </summary>
        /// <param name="spriteSheet"></param>
        /// <param name="animationFrames"></param>
        public TankObjectBuilder(Texture2D spriteSheet, List<Rectangle> animationFrames)
            : this(spriteSheet, animationFrames, null)
        {
        }

        /// <summary>
        /// Create a new instance of this class to spawn game objects
        /// </summary>
        /// <param name="spriteSheet"></param>
        /// <param name="animationFrames"></param>
        public TankObjectBuilder(Texture2D spriteSheet, List<Rectangle> animationFrames, Effect effect)
        {
            this.spriteSheet = spriteSheet;
            this.animationFrames = animationFrames;
            this.effect = effect;
            int textureWidth = animationFrames[0].Width;
            int textureHeight = animationFrames[0].Height;
            colliderDestination = new Rectangle(-textureWidth / 2, -textureHeight / 2, textureWidth, textureHeight);
        }

        /// <summary>
        /// Build all the game components
        /// </summary>
        /// <returns></returns>
        public override List<IComponent> BuildGameComponents(object parameter)
        {
            if (entityManager == null)
            {
                return new List<IComponent>();
            }

            if (parameter is Vector2 startPosition)
            {
                return CreateComponents(startPosition);
            }

            return new List<IComponent>();
        }

        protected List<IComponent> CreateComponents(Vector2 startPosition)
        {
            List<IComponent> returnComponents = new List<IComponent>();
            PlaceableComponent placeableComponent = entityManager.CreateComponent<PlaceableComponent>();
            placeableComponent.Position = startPosition + new Vector2(spriteSheet.Width, spriteSheet.Height) * -1;
            VisibleComponent visibleComponent = entityManager.CreateComponent<VisibleComponent>();
            visibleComponent.Color = Color.White;
            visibleComponent.Source = animationFrames[0];
            visibleComponent.Destination = animationFrames[0];
            visibleComponent.Texture = spriteSheet;
            visibleComponent.ShaderEffect = effect;
            visibleComponent.DrawMiddle = true;
            MoveableComponent moveable = entityManager.CreateComponent<MoveableComponent>();
            moveable.Mass = 15;
            moveable.ApplyPhysic = true;
            ColliderComponent collider = entityManager.CreateComponent<ColliderComponent>();
            collider.Collider = colliderDestination;

            PlayerControllableComponent controllableComponent = entityManager.CreateComponent<PlayerControllableComponent>();
            controllableComponent.Controller = new StaticKeyboardControls();
            GameObjectTag gameObjectTag = entityManager.CreateComponent<GameObjectTag>();

            returnComponents.Add(placeableComponent);
            returnComponents.Add(visibleComponent);
            returnComponents.Add(moveable);
            returnComponents.Add(collider);
            returnComponents.Add(controllableComponent);
            returnComponents.Add(gameObjectTag);

            return returnComponents;
        }
    }
}
