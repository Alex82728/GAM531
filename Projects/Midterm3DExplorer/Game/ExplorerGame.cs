
using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Game.GLAbstractions;

namespace Game
{
    public class ExplorerGame : GameWindow
    {
        // Scene meshes & resources
        private Shader _shader = null!;
        private Texture _texture = null!;
        private Mesh _floor = null!;
        private Mesh _cube = null!;
        private Mesh _pyramid = null!;
        private Mesh _door = null!;
        private Camera _camera = null!;

        // Lights
        private Vector3 _torchPos = new(2f, 1.7f, -0.5f);
        private bool _torchOn = true;         // T to toggle
        private bool _flashlightOn = true;    // F to toggle (camera spotlight)

        // Interaction objects
        private Vector3 _gemPos = new(0f, 0.5f, -2f);
        private bool _gemCollected = false;

        // Door that unlocks after gem collection
        private float _doorOpen = 0f; // 0..1, 1 = fully open
        private Vector3 _doorPos = new(0f, 0f, -4f);
        private bool _nearDoor = false;

        // Motion / physics (simple)
        private Vector3 _velocity = Vector3.Zero;
        private bool _grounded = true;
        private const float GRAVITY = -12f;
        private const float JUMP_SPEED = 5.5f;

        // Timing / input
        private double _time;
        private bool _firstMove = true;
        private Vector2 _lastMouse;

        public ExplorerGame(int width, int height, string title)
            : base(GameWindowSettings.Default, new NativeWindowSettings
            {
                ClientSize = (width, height),
                Title = title
            })
        { }

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(0.06f, 0.06f, 0.09f, 1.0f);
            CursorState = CursorState.Grabbed;

            _shader = new Shader("Shaders/vertex.glsl", "Shaders/fragment.glsl");
            _texture = new Texture("Assets/texture.png");

            _camera = new Camera(new Vector3(0f, 1.2f, 3.5f), Size.X / (float)Size.Y);

            _floor = Mesh.CreateQuad(size: 20f, y: 0f);
            _cube = Mesh.CreateCube(size: 1f, center: new Vector3(-1.5f, 0.5f, -1f));
            _pyramid = Mesh.CreatePyramid(size: 1f, baseCenter: new Vector3(0f, 0f, 0f));
            _pyramid.ModelOffset = _gemPos;
            _door = Mesh.CreateCube(size: 1f, center: _doorPos + new Vector3(0f, 1f, 0f)); // door 2m tall

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(TriangleFace.Back);

            Title = "Mini 3D Explorer — WASD+Mouse | Space=Jump | F=Flashlight | T= Torch | E=Interact | Esc=Mouse";
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            _floor.Dispose();
            _cube.Dispose();
            _pyramid.Dispose();
            _door.Dispose();
            _texture.Dispose();
            _shader.Dispose();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);
            _camera.AspectRatio = Size.X / (float)Size.Y;
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            _time += args.Time;
            if (!IsFocused) return;

            var input = KeyboardState;

            if (input.IsKeyPressed(Keys.Escape))
                CursorState = CursorState == CursorState.Grabbed ? CursorState.Normal : CursorState.Grabbed;

            // Toggles
            if (input.IsKeyPressed(Keys.F)) _flashlightOn = !_flashlightOn; // flashlight
            if (input.IsKeyPressed(Keys.T)) _torchOn = !_torchOn;           // torch

            // Interaction: door open/close if unlocked and near
            if (input.IsKeyPressed(Keys.E) && _nearDoor && _gemCollected)
                _doorOpen = _doorOpen < 0.5f ? 1f : 0f;

            // Mouse look
            var mouse = MouseState;
            if (CursorState == CursorState.Grabbed)
            {
                if (_firstMove)
                {
                    _lastMouse = mouse.Position;
                    _firstMove = false;
                }
                else
                {
                    var delta = mouse.Position - _lastMouse;
                    _lastMouse = mouse.Position;
                    _camera.Yaw += delta.X * 0.1f;
                    _camera.Pitch -= delta.Y * 0.1f;
                }
            }

            // Movement (with gravity + jump)
            float baseSpeed = 4.0f;
            float speed = input.IsKeyDown(Keys.LeftShift) ? 7.0f : baseSpeed;
            Vector3 wish = Vector3.Zero;
            if (input.IsKeyDown(Keys.W)) wish += _camera.Front;
            if (input.IsKeyDown(Keys.S)) wish -= _camera.Front;
            if (input.IsKeyDown(Keys.A)) wish -= _camera.Right;
            if (input.IsKeyDown(Keys.D)) wish += _camera.Right;
            wish.Y = 0f;
            if (wish.LengthSquared > 0) wish = wish.Normalized() * speed;

            // gravity
            _velocity.Y += GRAVITY * (float)args.Time;
            _velocity.X = wish.X;
            _velocity.Z = wish.Z;

            if (_grounded && input.IsKeyPressed(Keys.Space)) _velocity.Y = JUMP_SPEED;

            Vector3 next = _camera.Position + _velocity * (float)args.Time;

            // ground plane at y=1.0 (eye height ~1.2)
            float groundY = 1.0f;
            if (next.Y < groundY)
            {
                next.Y = groundY;
                _velocity.Y = 0;
                _grounded = TrueIfLanded(_camera.Position.Y, groundY);
            }
            else
            {
                _grounded = false;
            }

            // Very simple collision against door when closed
            _nearDoor = false;
            var doorOpenAngle = MathHelper.Lerp(0f, -95f, _doorOpen);
            bool doorBlocking = _doorOpen < 0.9f; // almost closed
            // door world AABB (approx around doorway)
            var doorCenter = _doorPos + new Vector3(0f, 1f, 0f);
            var doorHalf = new Vector3(0.6f, 1.0f, 0.15f);
            var aabbMin = doorCenter - doorHalf;
            var aabbMax = doorCenter + doorHalf;

            // Near check for interaction tooltip
            _nearDoor = (new Vector2(_camera.Position.X - doorCenter.X, _camera.Position.Z - doorCenter.Z).Length <= 1.5f);

            if (doorBlocking && PointInsideXZ(next, aabbMin, aabbMax))
            {
                // block forward progress roughly along Z
                next.Z = _camera.Position.Z;
            }

            _camera.Position = next;

            // Gem collect
            if (!_gemCollected && (_camera.Position - _gemPos).LengthFast < 0.9f)
                _gemCollected = true;
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _shader.Use();

            // Camera matrices
            _shader.SetMatrix4("uView", _camera.GetViewMatrix());
            _shader.SetMatrix4("uProjection", _camera.GetProjectionMatrix());
            _shader.SetVector3("uViewPos", _camera.Position);

            // Texture & material
            _texture.Bind(TextureUnit.Texture0);
            _shader.SetInt("uMaterial.diffuse", 0);
            _shader.SetVector3("uMaterial.specular", new Vector3(0.5f));
            _shader.SetFloat("uMaterial.shininess", 32f);

            // Lights setup
            // 1) Torch (point light near cube)
            _shader.SetInt("uPointOn", _torchOn ? 1 : 0);
            _shader.SetVector3("uPoint.position", _torchPos);
            _shader.SetVector3("uPoint.ambient", new Vector3(0.12f));
            _shader.SetVector3("uPoint.diffuse", new Vector3(0.8f, 0.7f, 0.6f));
            _shader.SetVector3("uPoint.specular", new Vector3(1f));

            // 2) Flashlight (spot on camera)
            _shader.SetInt("uSpotOn", _flashlightOn ? 1 : 0);
            _shader.SetVector3("uSpot.position", _camera.Position);
            _shader.SetVector3("uSpot.direction", _camera.Front);
            _shader.SetFloat("uSpot.cutOff", MathF.Cos(MathHelper.DegreesToRadians(12.5f)));
            _shader.SetFloat("uSpot.outerCutOff", MathF.Cos(MathHelper.DegreesToRadians(17.5f)));
            _shader.SetFloat("uSpot.constant", 1.0f);
            _shader.SetFloat("uSpot.linear", 0.09f);
            _shader.SetFloat("uSpot.quadratic", 0.032f);
            _shader.SetVector3("uSpot.ambient", new Vector3(0.05f));
            _shader.SetVector3("uSpot.diffuse", new Vector3(1.0f));
            _shader.SetVector3("uSpot.specular", new Vector3(1.0f));

            // Draw floor
            var model = Matrix4.Identity;
            _shader.SetMatrix4("uModel", model);
            _floor.Draw();

            // Rotating cube near torch
            model = Matrix4.CreateRotationY((float)_time * 0.7f) * Matrix4.CreateTranslation(_cube.ModelOffset);
            _shader.SetMatrix4("uModel", model);
            _cube.Draw();

            // Spinning pyramid (gem): sink when collected
            float bob = (float)Math.Sin(_time * 2.0) * 0.12f;
            var gemOffset = _gemCollected ? new Vector3(0f, -1.6f, 0f) : new Vector3(0f, bob, 0f);
            model = Matrix4.CreateRotationY((float)_time) * Matrix4.CreateTranslation(gemOffset) *
                    Matrix4.CreateTranslation(_pyramid.ModelOffset);
            _shader.SetMatrix4("uModel", model);
            _pyramid.Draw();

            // Door: rotate around left edge as it opens (hinge)
            // Build transform: translate to hinge, rotate, translate back, then place at door pos and scale thin
            var hingeLocal = new Vector3(-0.5f, 0f, 0f); // left edge in unit cube space
            float angle = MathHelper.Lerp(0f, -95f, _doorOpen) * MathF.PI / 180f;

            model = Matrix4.CreateTranslation(-hingeLocal) *
                    Matrix4.CreateRotationY(angle) *
                    Matrix4.CreateTranslation(hingeLocal) *
                    Matrix4.CreateScale(1f, 2f, 0.1f) *   // tall & thin
                    Matrix4.CreateTranslation(_doorPos + new Vector3(0f, 0f, 0f));

            _shader.SetMatrix4("uModel", model);
            _door.Draw();

            SwapBuffers();
        }

        private static bool PointInsideXZ(Vector3 p, Vector3 min, Vector3 max)
        {
            return p.X >= min.X && p.X <= max.X && p.Z >= min.Z && p.Z <= max.Z;
        }
        private static bool TrueIfLanded(float prevY, float groundY) => prevY >= groundY - 0.05f;
    }
}
