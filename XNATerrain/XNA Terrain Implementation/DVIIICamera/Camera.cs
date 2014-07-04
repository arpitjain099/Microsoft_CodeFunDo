using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace XNATerrainImplementation.DVIIICamera
{
    enum CameraMode
    {
        FREE_MODE,
        TRAJECTORY_MODE
    }

    class Camera
    {
        //from the main game system
        private GraphicsDeviceManager graphics;

        //Major properties, like the position and the target
        private Vector3 position;
        private Vector3 target = Vector3.Zero;
        private Matrix projectionMatrix;
        private Matrix viewMatrix;

        //also, the camera may have different modes, by default it is the free one
        private CameraMode mode = CameraMode.FREE_MODE;

        //Crosshair properties
        private Texture2D crosshairTexture;
        private Vector2 crosshairPosition;

        //the camera should be able to be manipulated with user input
        private MouseState previousMouseState;
        private KeyboardState previousKeyboardState;

        //for the mouse, this is also important
        private float mouseSensivity = 0.1f;

        //if the value is 1, the mouse is normal, if it is negative, it becomes inverted
        private float mouseInverted = 1;

        //depending on the mode, the accepted keys may vary
        private Keys[] freeModeKeys = {Keys.W, Keys.S,Keys.A,Keys.D, Keys.Space, Keys.C};

        //for the trajectory mode, there's the need to save the trajectories
        private Trajectory trajectory;

        public Camera(GraphicsDeviceManager newGraphics, Vector3 newPosition)
        {
            graphics = newGraphics;
            position = newPosition;
        }

        public Camera(GraphicsDeviceManager newGraphics, Vector3 newPosition, Vector3 newTarget)
        {
            graphics = newGraphics;
            position = newPosition;
            target = newTarget;
        }

        public void Initialize()
        {
            //sets up the initial view and projectionmatrix that will be constantly necessary 
            viewMatrix = Matrix.CreateLookAt(position, target, Vector3.Up);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), graphics.GraphicsDevice.Viewport.AspectRatio, 1.0f, 10000);

            //centers the mouse into the window
            Mouse.SetPosition(graphics.GraphicsDevice.Viewport.Width / 2, graphics.GraphicsDevice.Viewport.Height / 2);

            //save the current state of the mouse and keyboard to check for changes later
            previousMouseState = Mouse.GetState();
            previousKeyboardState = Keyboard.GetState();
        }

        public void LoadContent(ContentManager contentManager)
        {
            crosshairTexture = contentManager.Load<Texture2D>("Crosshair02");

            crosshairPosition = new Vector2(graphics.GraphicsDevice.Viewport.Width / 2.0f - crosshairTexture.Width / 2.0f,
                                            graphics.GraphicsDevice.Viewport.Height / 2.0f - crosshairTexture.Height / 2.0f);
        }

        public void Update(GameTime gameTime)
        {
            //get the vector indicating the direction we are looking at
            Vector3 direction = target - position;
            direction.Normalize();

            ProcessKeyboard(direction);

            ProcessMouse(direction);

            //position = trajectory.PositionCurve.GetPointOnCurve((float)time);
            //target = trajectory.TargetCurve.GetPointOnCurve((float)time);
            if (mode == CameraMode.TRAJECTORY_MODE)
            {
                trajectory.Update(ref position, ref target, gameTime.ElapsedGameTime.TotalMilliseconds);
                viewMatrix = Matrix.CreateLookAt(position, target, Vector3.Up);
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(crosshairTexture, crosshairPosition, Color.White);
        }

        private void ProcessMouse(Vector3 direction)
        {
            MouseState mouseState = Mouse.GetState();

            int mouseX = previousMouseState.X - mouseState.X;
            int mouseY = previousMouseState.Y - mouseState.Y;

            if(mouseX != 0 || mouseY != 0)
            {
                if (mode == CameraMode.FREE_MODE)
                {
                    //get the vector indicating the direction left to the one we are looking at (normal)
                    Vector3 cameraTargetNormalDirection;
                    cameraTargetNormalDirection.X = direction.Z;
                    cameraTargetNormalDirection.Z = -direction.X;
                    cameraTargetNormalDirection.Y = 0;

                    if (mouseX != 0)
                        target += cameraTargetNormalDirection*mouseX*mouseSensivity;

                    if (mouseY != 0)
                    {
                        Vector3 cameraTargetNormalDirectionUp = Vector3.Cross(direction, cameraTargetNormalDirection);

                        target += cameraTargetNormalDirectionUp*mouseY*mouseSensivity*mouseInverted;
                    }

                    viewMatrix = Matrix.CreateLookAt(position, target, Vector3.Up);
                    Mouse.SetPosition(graphics.GraphicsDevice.Viewport.Width/2,
                                      graphics.GraphicsDevice.Viewport.Height/2);
                }
            }
        }

        private void ProcessKeyboard(Vector3 direction)
        {
            KeyboardState keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.D1))
                mode = CameraMode.FREE_MODE;
            else if (keyboardState.IsKeyDown(Keys.D2))
                mode = CameraMode.TRAJECTORY_MODE;


            if (mode == CameraMode.FREE_MODE && IsAnyKeyDown(keyboardState,freeModeKeys))
            {
                //get the vector indicating the direction left to the one we are looking at (normal)
                Vector3 cameraNormalDirection;
                cameraNormalDirection.X = direction.Z;
                cameraNormalDirection.Z = -direction.X;
                cameraNormalDirection.Y = 0;

                //move the camera forward or backward
                if (keyboardState.IsKeyDown(Keys.W))
                {
                    position += direction;
                    target += direction;
                }
                else if (keyboardState.IsKeyDown(Keys.S))
                {
                    position -= direction;
                    target -= direction;
                }

                //move the camera left or right
                if (keyboardState.IsKeyDown(Keys.A))
                {
                    position += cameraNormalDirection;
                    target += cameraNormalDirection;
                }
                else if (keyboardState.IsKeyDown(Keys.D))
                {
                    position -= cameraNormalDirection;
                    target -= cameraNormalDirection;
                }

                //move the camera up or down
                if (keyboardState.IsKeyDown(Keys.Space))
                {
                    position += Vector3.Up;
                    target += Vector3.Up;
                }
                else if (keyboardState.IsKeyDown(Keys.C))
                {
                    position += Vector3.Down;
                    target += Vector3.Down;
                }

                viewMatrix = Matrix.CreateLookAt(position, target, Vector3.Up);
            }
        }

        private static bool IsAnyKeyDown(KeyboardState keyboardState, Keys[] keys)
        {
            foreach (Keys key in keys)
            {
                if(keyboardState.IsKeyDown(key))
                    return true;
            }

            return false;
        }

        public void AddTrajectory(Trajectory newTrajectory)
        {
            trajectory = newTrajectory;
        }

        #region Getters and Setters

        public Matrix ProjectionMatrix
        {
            get { return projectionMatrix; }
            set { projectionMatrix = value; }
        }

        public Matrix ViewMatrix
        {
            get { return viewMatrix; }
            set { viewMatrix = value; }
        }

        public float MouseSensivity
        {
            get { return mouseSensivity; }
            set { mouseSensivity = value; }
        }

        public bool MouseInverted
        {
            get { if(mouseInverted < 0) return true; return false;}
            set { if (value) mouseInverted = -1; else mouseInverted = 1; }
        }

        #endregion

    }
}