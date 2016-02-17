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
using VikingXNAGraphics;
using RoundLineCode;
using RoundCurve;
using Geometry;
using VikingXNA;

namespace XNATestbed
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class XNATestBedMain : Microsoft.Xna.Framework.Game
    {
        public GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;
        public RoundLineManager lineManager = new RoundLineCode.RoundLineManager();
        public CurveManager curveManager = new CurveManager();
        public VikingXNA.Scene Scene;
        public VikingXNA.Camera Camera;
        public SpriteFont fontArial;
        public BasicEffect basicEffect;
        public AnnotationOverBackgroundLumaEffect overlayEffect;

        CurveTest curveTest = new CurveTest();
        CurveViewTest curveViewTest = new CurveViewTest();


        public XNATestBedMain()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

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

            fontArial = Content.Load<SpriteFont>(@"Arial");

            // TODO: use this.Content to load your game content here
            Camera = new VikingXNA.Camera();
            Camera.LookAt = new Vector2(0, 0);
            Camera.Downsample = 0.5;
            Scene = new VikingXNA.Scene(graphics.GraphicsDevice.Viewport, Camera);


            lineManager.Init(GraphicsDevice, Content);
            curveManager.Init(GraphicsDevice, Content);

            RasterizerState state = new RasterizerState();
            state.CullMode = CullMode.None;
            //state.FillMode = FillMode.WireFrame;

            GraphicsDevice.RasterizerState = state;

            InitializeEffects();

            curveTest.Init(this);
            curveViewTest.Init(this);
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
        /// Initializes the basic effect (parameter setting and technique selection)
        /// used for the 3D model.
        /// </summary>
        private void InitializeEffects()
        {
            basicEffect = new BasicEffect(this.GraphicsDevice);
            //   basicEffect.DiffuseColor = new Vector3(0.1f, 0.1f, 0.1f);
            //   basicEffect.SpecularColor = new Vector3(0.25f, 0.25f, 0.25f);
            //   basicEffect.SpecularPower = 5.0f;
            basicEffect.AmbientLightColor = new Vector3(1f, 1f, 1f);
            /*
            basicEffect.Projection = projectionMatrix;
            basicEffect.World = worldMatrix;
            basicEffect.View = camera.View;
            basicEffect.Projection = projectionMatrix;
            */

            Matrix WorldViewProj = Scene.WorldViewProj;
             
            Effect AnnotationOverlayShader = Content.Load<Effect>("AnnotationOverlayShader");
            this.overlayEffect = new AnnotationOverBackgroundLumaEffect(AnnotationOverlayShader);
            this.overlayEffect.WorldViewProjMatrix = WorldViewProj;

            //this.channelEffect.WorldMatrix = worldMatrix;
            //this.channelEffect.ProjectionMatrix = projectionMatrix;
            //this.channelEffect.ViewMatrix = viewMatrix;
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

            ProcessGamepad();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        private void ProcessGamepad()
        {
            Camera.Downsample += -GamePad.GetState(PlayerIndex.One).ThumbSticks.Right.Y;
            Camera.LookAt += GamePad.GetState(PlayerIndex.One).ThumbSticks.Left;

            curveTest.ProcessGamepad();
        }

        private void UpdateEffectMatricies(Scene drawnScene)
        {
            basicEffect.Projection = drawnScene.Projection;
            basicEffect.View = drawnScene.Camera.View;
            basicEffect.World = drawnScene.World;
             
            overlayEffect.WorldViewProjMatrix = drawnScene.WorldViewProj;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            RasterizerState state = new RasterizerState();
            state.CullMode = CullMode.None;
            
            UpdateEffectMatricies(this.Scene);

            GraphicsDevice.RasterizerState = state;

            Matrix ViewProjMatrix = Scene.ViewProj;
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //curveTest.Draw(this);
            curveViewTest.Draw(this);
            
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

    }

    public class CurveTest
    {
        public Texture2D labelTexture;

        double CurveAngle = 3.14159 / 4.0;

        public void Init(XNATestBedMain window)
        {
            labelTexture = CreateTextureForLabel("The quick brown fox jumps over the lazy dog", window.GraphicsDevice, window.spriteBatch, window.fontArial);
        }

        public void ProcessGamepad()
        {
            CurveAngle += GamePad.GetState(PlayerIndex.One).ThumbSticks.Right.X;
        }

        public void Draw(XNATestBedMain window)
        {
            VikingXNA.Scene scene = window.Scene;
            Matrix ViewProjMatrix = scene.ViewProj;
            string TechniqueName = "AnimatedLinear";
            float time = DateTime.Now.Millisecond / 1000.0f;
            
            RoundLine line = new RoundLine(new Vector2((float)(-50.0f * Math.Cos(CurveAngle)), (float)(-50.0f * Math.Sin(CurveAngle)) + 50.0f),
                                           new Vector2((float)(50.0f * Math.Cos(CurveAngle)), (float)(50.0f * Math.Sin(CurveAngle)) + 50.0f));
            window.lineManager.Draw(new RoundLine[] { line }, 16, Color.Red, ViewProjMatrix, time, labelTexture);

            GridVector2[] cps = CreateTestCurve2(CurveAngle, 100);
            RoundCurve.RoundCurve curve = new RoundCurve.RoundCurve(cps);
            window.curveManager.Draw(new RoundCurve.RoundCurve[] { curve }, 16, Color.Blue, ViewProjMatrix, time, labelTexture);
            window.curveManager.Draw(new RoundCurve.RoundCurve[] { curve }, 16, Color.Blue, ViewProjMatrix, time, TechniqueName);
        }
         
        public Texture2D CreateTextureForLabel(string label, GraphicsDevice device,
                              SpriteBatch spriteBatch,
                              SpriteFont font)
        {
            Vector2 labelDimensions = font.MeasureString(label);
            RenderTarget2D target = new RenderTarget2D(device, (int)labelDimensions.X * 2, (int)labelDimensions.Y * 2);

            RenderTargetBinding[] oldRenderTargets = device.GetRenderTargets();
            device.SetRenderTarget(target);
            device.Clear(Color.Transparent);

            spriteBatch.Begin();
            spriteBatch.DrawString(font, label, new Vector2(0, 0), Color.Yellow, 0, new Vector2(0, 0), 2, SpriteEffects.None, 0);
            spriteBatch.End();

            device.SetRenderTargets(oldRenderTargets);

            return target;
        }

        private static GridVector2[] CreateTestCurve(double angle, double width)
        {
            GridVector2[] cps = new GridVector2[] {new GridVector2(-width,width),
                                                   new GridVector2(-width * Math.Cos(angle), -width * Math.Sin(angle)),
                                                   new GridVector2(0,0),
                                                   new GridVector2(width,0) };
            return cps;
        }

        private static GridVector2[] CreateTestCurve2(double angle, double width)
        {
            GridVector2[] cps = new GridVector2[] {new GridVector2(width,width),
                                                   new GridVector2(0, width),
                                                   new GridVector2(0,0),
                                                   new GridVector2(width,0) };
            return Geometry.Lagrange.FitCurve(cps, 30);
        }
    }


    public class CurveViewTest
    {
        CurveView curveView;
        CurveLabel leftCurveLabel;
        CurveLabel rightCurveLabel;

        public void Init(XNATestBedMain window)
        {
            GridVector2[] cps = CreateTestCurve3(0, 100);
            curveView = new CurveView(cps, Color.Red, false);
            leftCurveLabel = new CurveLabel("The quick brown fox jumps over the lazy dog", cps, Color.Black, false);
            rightCurveLabel = new CurveLabel("C 1485", cps, Color.PaleGoldenrod, false);
        }

        public void ProcessGamepad()
        {
        }

        public void Draw(XNATestBedMain window)
        {
            VikingXNA.Scene scene = window.Scene;
            Matrix ViewProjMatrix = scene.ViewProj;
            string TechniqueName = "AnimatedLinear";
            float time = DateTime.Now.Millisecond / 1000.0f;

            double totalLabelLength = (double)(leftCurveLabel.Label.Length + rightCurveLabel.Label.Length);
            leftCurveLabel.Alignment = HorizontalAlignment.Left;
            rightCurveLabel.Alignment = HorizontalAlignment.Right;
            leftCurveLabel.Max_Curve_Length_To_Use_Normalized = (float)(leftCurveLabel.Label.Length / totalLabelLength);
            rightCurveLabel.Max_Curve_Length_To_Use_Normalized = (float)(rightCurveLabel.Label.Length / totalLabelLength);


            CurveView.Draw(window.GraphicsDevice, scene, window.curveManager, window.basicEffect, window.overlayEffect, time, new CurveView[] { curveView });
            CurveLabel.Draw(window.GraphicsDevice, scene, window.spriteBatch, window.fontArial, window.curveManager, window.basicEffect, new CurveLabel[] { leftCurveLabel, rightCurveLabel });

        } 

        private static GridVector2[] CreateTestCurve(double angle, double width)
        {
            GridVector2[] cps = new GridVector2[] {new GridVector2(-width,width),
                                                   new GridVector2(-width * Math.Cos(angle), -width * Math.Sin(angle)),
                                                   new GridVector2(0,0),
                                                   new GridVector2(width,0) };
            return cps;
        }

        private static GridVector2[] CreateTestCurve2(double angle, double width)
        {
            GridVector2[] cps = new GridVector2[] {new GridVector2(width,width),
                                                   new GridVector2(0, width),
                                                   new GridVector2(0,0),
                                                   new GridVector2(width,0) };
            return cps;
        }

        private static GridVector2[] CreateTestCurve3(double angle, double width)
        {
            GridVector2[] cps = new GridVector2[] {new GridVector2(-100,100),
                                                   new GridVector2(-50, 0),
                                                   new GridVector2(0,100),
                                                   new GridVector2(100,0) };
            return cps;
        }
    }
}
