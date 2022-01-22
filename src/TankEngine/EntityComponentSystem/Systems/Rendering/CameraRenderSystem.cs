﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TankEngine.Adapter;
using TankEngine.DataStructures.Pools;
using TankEngine.EntityComponentSystem.Components.Rendering;
using TankEngine.EntityComponentSystem.Components.World;
using TankEngine.EntityComponentSystem.Validator;
using TankEngine.EntityComponentSystem.Validator.Base;

namespace TankEngine.EntityComponentSystem.Systems.Rendering
{
    /// <summary>
    /// Create a render system based on camera entites
    /// </summary>
    public class CameraRenderSystem : SimpleRenderSystem
    {
        /// <summary>
        /// The validator to use for a camera entity
        /// </summary>
        private IValidatable cameraValidator;

        /// <summary>
        /// Was a camera found if not do not render the game objects
        /// </summary>
        private bool cameraFound;

        /// <summary>
        /// The position of the first found camera
        /// </summary>
        private Vector2 cameraPosition;

        /// <summary>
        /// The post processing effects to use
        /// </summary>
        private Effect[] postProcessingEffects;

        /// <summary>
        /// Rendertarget to use for postprocessing
        /// </summary>
        private RenderTarget2D postProcessingRenderTarget;

        /// <summary>
        /// The folder to save screenshots in
        /// </summary>
        private string screenshotFolder;

        /// <summary>
        /// The pool for render texture containers
        /// </summary>
        protected IObjectPool<CameraContainer> cameraContainerPool;


        /// <summary>
        /// Create a new instance of this render system
        /// </summary>
        /// <param name="spriteBatch">The spritebatch to use</param>
        /// <param name="defaultEffekt">The default effekt to use</param>
        /// <param name="viewportAdapter">The viewport adapter to use</param>
        /// <param name="graphicsDevice">The graphics device used to generate render targets</param>
        /// <param name="screenshotFolder">The base folder to save screenshots in</param>
        public CameraRenderSystem(SpriteBatch spriteBatch, Effect defaultEffect, IViewportAdapter viewportAdapter, GraphicsDevice graphicsDevice, string screenshotFolder)
            : base(spriteBatch, defaultEffect, viewportAdapter, graphicsDevice)
        {
            cameraFound = false;
            postProcessingEffects = new Effect[0];
            cameraValidator = new CameraEntityValidator();
            cameraContainerPool = new ConcurrentObjectPool<CameraContainer>(() => new CameraContainer(), 2);
            validators.Add(cameraValidator);
            postProcessingRenderTarget = new RenderTarget2D(graphicsDevice, viewportAdapter.Viewport.Width, viewportAdapter.Viewport.Height);
            this.screenshotFolder = screenshotFolder;
        }

        /// <inheritdoc/>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            CameraContainer cameraToUse = watchedEntities.Where(entityId => cameraValidator.IsValidEntity(entityId, entityManager))
                                                         .Select(entityId => CreateCameraContainer(entityId))
                                                         .Where(container => container.Camera.Active)
                                                         .OrderBy(container => container.Camera.Priority)
                                                         .FirstOrDefault();
            cameraFound = false;
            if (cameraToUse == null)
            {
                return;
            }
            cameraFound = true;
            cameraPosition = cameraToUse.WorldPosition.Position;
            postProcessingEffects = cameraToUse.Camera.PostProcessingEffects;
            if (cameraToUse.Camera.TakeScreenshot)
            {
                cameraToUse.Camera.TakeScreenshot = false;
                TakeScreenshot(postProcessingEffects.Length == 0 ? sceneRenderTarget : postProcessingRenderTarget, screenshotFolder);
            }

        }

        /// <inheritdoc/>
        protected override void DrawGameObjects()
        {
            if (!cameraFound)
            {
                //return;
            }
            base.DrawGameObjects();
        }

        /// <inheritdoc/>
        protected override Vector2 GetBaseDrawPosition(PositionComponent positionComponent)
        {
            return WorldPositionToScreenPosition(positionComponent.Position);
        }

        /// <summary>
        /// Convert the world position of an object into a screen position
        /// </summary>
        /// <param name="worldPosition">The world position of an object</param>
        /// <returns>The screen position of the object</returns>
        private Vector2 WorldPositionToScreenPosition(Vector2 worldPosition)
        {
            Vector2 cameraWorldTopLeftPosition = cameraPosition - viewportAdapter.Center.ToVector2();
            return (cameraWorldTopLeftPosition * -1 + worldPosition);
        }

        /// <inheritdoc/>
        protected virtual CameraContainer CreateCameraContainer(uint entityId)
        {
            PositionComponent position = entityManager.GetComponent<PositionComponent>(entityId);
            CameraComponent camera = entityManager.GetComponent<CameraComponent>(entityId);
            if (position == null || camera == null)
            {
                return null;
            }
            CameraContainer returnContainer = cameraContainerPool.Get();
            returnContainer.WorldPosition = position;
            returnContainer.Camera = camera;
            return returnContainer;
        }

        /// <summary>
        /// Copy the render target
        /// </summary>
        /// <param name="source">The source of the render target</param>
        /// <param name="renderTarget">The target to render to</param>
        protected void CopyRenderTarget(RenderTarget2D source, RenderTarget2D renderTarget)
        {
            graphicsDevice.SetRenderTarget(renderTarget);
            spriteBatch.Begin();
            spriteBatch.Draw(
                source,
                new Rectangle(0, 0, viewportAdapter.VirtualWidth, viewportAdapter.VirtualHeight),
                Color.White
                );
            spriteBatch.End();
        }

        /// <inheritdoc/>
        protected override void DrawCompletedScene(RenderTarget2D scene)
        {
            graphicsDevice.SetRenderTarget(null);
            PostProcessingSteps(scene);
            base.DrawCompletedScene(postProcessingEffects.Length == 0 ? scene : postProcessingRenderTarget);
        }

        /// <summary>
        /// Do all the post processing steps for the image
        /// </summary>
        /// <param name="scene">The scene to use as base</param>
        private void PostProcessingSteps(RenderTarget2D scene)
        {
            for (int i = 0; i < postProcessingEffects.Length; i++)
            {
                graphicsDevice.SetRenderTarget(postProcessingRenderTarget);
                spriteBatch.Begin(
                    SpriteSortMode.Immediate,
                    BlendState.AlphaBlend,
                    null,
                    null,
                    null,
                    postProcessingEffects[i],
                    viewportAdapter.GetScaleMatrix()
                    );

                spriteBatch.Draw(scene, new Rectangle(0, 0, viewportAdapter.VirtualWidth, viewportAdapter.VirtualHeight), Color.White);
                spriteBatch.End();
                CopyRenderTarget(postProcessingRenderTarget, scene);
            }
        }


        /// <summary>
        /// Create a screenshot
        /// </summary>
        /// <param name="renderTarget">The rendertarget to save the screenshot from</param>
        /// <param name="path">The path to save to</param>
        /// <returns>true after saving was done</returns>
        private bool TakeScreenshot(RenderTarget2D renderTarget, string path)
        {
            if (path == null || path == string.Empty)
            {
                return false;
            }

            MemoryStream memoryStream = new MemoryStream();
            TaskAwaiter<bool> screenshotSaver;
            try
            {

                renderTarget.SaveAsPng(memoryStream, renderTarget.Width, renderTarget.Height);
                screenshotSaver = SaveScreenshotAsync(memoryStream, path).GetAwaiter();
                screenshotSaver.OnCompleted(() =>
                {
                    if (memoryStream != null)
                    {
                        memoryStream.Dispose();
                        memoryStream = null;
                    }
                });

            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (screenshotSaver.IsCompleted)
                {
                    if (memoryStream != null)
                    {
                        memoryStream.Dispose();
                        memoryStream = null;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Write screenshot async to disc
        /// </summary>
        /// <param name="dataStream">The datastream with the data to save</param>
        /// <param name="path">The path to save the data to</param>
        /// <returns>True if saving was successful</returns>
        private async Task<bool> SaveScreenshotAsync(MemoryStream dataStream, string path)
        {
            return await Task.Run(() =>
                        {
                            if (!dataStream.CanRead)
                            {
                                return false;
                            }
                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }
                            path = Path.Combine(path, DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_GameScreenshot.png");
                            try
                            {

                                using (FileStream fs = new FileStream(path, FileMode.Create))
                                {
                                    dataStream.Position = 0;
                                    dataStream.CopyTo(fs);
                                    fs.Flush();
                                }
                            }
                            catch (Exception)
                            {
                                return false;
                            }
                            return true;
                        });
        }
    }
}

