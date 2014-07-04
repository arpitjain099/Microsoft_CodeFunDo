using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using XNATerrainHeightmap;
using XNATerrainImplementation.DVIIICamera;

namespace XNATerrainImplementation
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private Camera camera;

        SpriteFont spriteFont;
        XNATerrain xnaTerrain;
        Vector3 localClick;
        
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            //IsMouseVisible = true;
            localClick = Vector3.Zero;
            graphics.SynchronizeWithVerticalRetrace = true;
            camera = new Camera(graphics, new Vector3(35.0f));
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            camera.Initialize();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            spriteFont = Content.Load<SpriteFont>("Verdana");
            
            // Load the class heightmap with info about terrain height map e picking.
            xnaTerrain = Content.Load<XNATerrain>("HeightMap");

            camera.LoadContent(Content);

            //display everything in wireframe
            //GraphicsDevice.RenderState.FillMode = FillMode.WireFrame;

            //create a path for the camera to follow
            Trajectory trajectory01 = new Trajectory(CurveLoopType.Cycle);
            trajectory01.AddStep(new Vector3(45f, 45, 45f), Vector3.Zero, 0);
            trajectory01.AddStep(new Vector3(45f,45, -45), Vector3.Zero, 2000);
            trajectory01.AddStep(new Vector3(-45f, 45, -45), Vector3.Zero, 4000);
            trajectory01.AddStep(new Vector3(-45f, 45, 45), Vector3.Zero, 6000);
            trajectory01.AddStep(new Vector3(45f, 45, 45), Vector3.Zero, 8000);

            trajectory01.BuildTrajectory();

            camera.AddTrajectory(trajectory01);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                localClick = xnaTerrain.Pick(GraphicsDevice, camera.ViewMatrix, camera.ProjectionMatrix);
            }

            camera.Update(gameTime);

            base.Update(gameTime);
        }

        

        

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);            

            graphics.GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            graphics.GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Wrap;

            xnaTerrain.Draw(camera.ViewMatrix, camera.ProjectionMatrix);

            
            // Print position of the pick in terrain
            spriteBatch.Begin();
            if (localClick != Vector3.Zero)
            {
                spriteBatch.DrawString(
                    spriteFont,
                    @"
                X: " + localClick.X + @"
                Y: " + localClick.Y + @"
                Z: " + localClick.Z + @"
                ",
                    Vector2.Zero,
                    Color.Yellow);
                              
            }

            camera.Draw(gameTime, spriteBatch);  
            spriteBatch.End();

            ResetStates();            

            base.Draw(gameTime);

            
        }

        private void ResetStates()
        {
            GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
            GraphicsDevice.RenderState.DepthBufferEnable = false;

            GraphicsDevice.RenderState.AlphaBlendEnable = true;
            GraphicsDevice.RenderState.AlphaBlendOperation = BlendFunction.Add;
            GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
            GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
            GraphicsDevice.RenderState.SeparateAlphaBlendEnabled = false;

            GraphicsDevice.RenderState.AlphaTestEnable = true;
            GraphicsDevice.RenderState.AlphaFunction = CompareFunction.Greater;
            GraphicsDevice.RenderState.ReferenceAlpha = 0;

            GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Clamp;
            GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Clamp;

            GraphicsDevice.SamplerStates[0].MagFilter = TextureFilter.Linear;
            GraphicsDevice.SamplerStates[0].MinFilter = TextureFilter.Linear;
            GraphicsDevice.SamplerStates[0].MipFilter = TextureFilter.Linear;

            GraphicsDevice.SamplerStates[0].MipMapLevelOfDetailBias = 0.0f;
            GraphicsDevice.SamplerStates[0].MaxMipLevel = 0;

            GraphicsDevice.RenderState.DepthBufferEnable = true;
            GraphicsDevice.RenderState.AlphaBlendEnable = false;
            GraphicsDevice.RenderState.AlphaTestEnable = false;
        }
    }
}
