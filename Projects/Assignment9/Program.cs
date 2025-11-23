using System;
using System.Collections.Generic;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Collision3D
{
    public static class Program
    {
        public static void Main()
        {
            var nativeSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(1280, 720),
                Title = "Assignment 9 - 3D Collision (OpenTK)",
                APIVersion = new Version(3, 3),
                Flags = ContextFlags.ForwardCompatible
            };

            using var window = new Game(nativeSettings);
            window.Run();
        }
    }

    // ---------------------------
    // Game Window
    // ---------------------------
    public class Game : GameWindow
    {
        private int _shaderProgram;
        private int _vao;
        private int _vbo;

        private Matrix4 _projection;
        private PlayerController _player;
        private List<GameObject> _sceneObjects = new();
        private CollisionSystem _collisionSystem = new();

        private float _time;

        public Game(NativeWindowSettings nativeWindowSettings)
            : base(GameWindowSettings.Default, nativeWindowSettings)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.1f, 0.1f, 0.2f, 1.0f);
            GL.Enable(EnableCap.DepthTest);

            _shaderProgram = CreateBasicShader();
            GL.UseProgram(_shaderProgram);

            // Create cube geometry
            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

            float[] cubeVertices = CubeVertices();
            GL.BufferData(BufferTarget.ArrayBuffer, cubeVertices.Length * sizeof(float),
                cubeVertices, BufferUsageHint.StaticDraw);

            // Position (vec3) + Normal (vec3)
            int stride = 6 * sizeof(float);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));

            // Projection (simple perspective)
            float aspect = Size.X / (float)Size.Y;
            _projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60f), aspect, 0.1f, 100f);

            // Create scene + player
            CreateScene();
        }

        private void CreateScene()
        {
            // Player
            _player = new PlayerController
            {
                Transform = new Transform(
                    position: new Vector3(0, 1, 5),
                    scale: Vector3.One,
                    yawDegrees: 180f
                ),
                Collider = new AABBCollider(new Vector3(0, 1, 5), new Vector3(0.5f, 1f, 0.5f)),
                MoveSpeed = 5f
            };

            // WALLS (static colliders)
            // Back wall
            _sceneObjects.Add(new GameObject(
                name: "BackWall",
                transform: new Transform(new Vector3(0, 1.5f, -5), new Vector3(10, 3, 0.5f), 0),
                collider: new AABBCollider(new Vector3(0, 1.5f, -5), new Vector3(5, 1.5f, 0.25f)),
                isTrigger: false
            ));

            // Left wall
            _sceneObjects.Add(new GameObject(
                name: "LeftWall",
                transform: new Transform(new Vector3(-5, 1.5f, 0), new Vector3(0.5f, 3, 10), 0),
                collider: new AABBCollider(new Vector3(-5, 1.5f, 0), new Vector3(0.25f, 1.5f, 5)),
                isTrigger: false
            ));

            // Right wall
            _sceneObjects.Add(new GameObject(
                name: "RightWall",
                transform: new Transform(new Vector3(5, 1.5f, 0), new Vector3(0.5f, 3, 10), 0),
                collider: new AABBCollider(new Vector3(5, 1.5f, 0), new Vector3(0.25f, 1.5f, 5)),
                isTrigger: false
            ));

            // Extra back wall segment for more structure
            _sceneObjects.Add(new GameObject(
                name: "BackWallUpper",
                transform: new Transform(new Vector3(0, 4.0f, -5), new Vector3(10, 1, 0.5f), 0),
                collider: new AABBCollider(new Vector3(0, 4.0f, -5), new Vector3(5, 0.5f, 0.25f)),
                isTrigger: false
            ));

            // BOX / PILLAR (static obstacle in the middle)
            _sceneObjects.Add(new GameObject(
                name: "CenterBox",
                transform: new Transform(new Vector3(0, 1, 0), new Vector3(1, 2, 1), 0),
                collider: new AABBCollider(new Vector3(0, 1, 0), new Vector3(0.5f, 1f, 0.5f)),
                isTrigger: false
            ));

            // Second pillar on the right
            _sceneObjects.Add(new GameObject(
                name: "RightPillar",
                transform: new Transform(new Vector3(3, 1, 2), new Vector3(1, 3, 1), 0),
                collider: new AABBCollider(new Vector3(3, 1.5f, 2), new Vector3(0.5f, 1.5f, 0.5f)),
                isTrigger: false
            ));

            // DOOR object (solid, with a trigger in front of it)
            var door = new GameObject(
                name: "Door",
                transform: new Transform(new Vector3(0, 1, -4.5f), new Vector3(1.5f, 2.5f, 0.3f), 0),
                collider: new AABBCollider(new Vector3(0, 1, -4.5f), new Vector3(0.75f, 1.25f, 0.15f)),
                isTrigger: false
            );
            _sceneObjects.Add(door);

            // "Door trigger" zone in front of the door (touch detection)
            var doorTrigger = new GameObject(
                name: "DoorTrigger",
                transform: new Transform(new Vector3(0, 1, -2.5f), new Vector3(2, 2, 1), 0),
                collider: new AABBCollider(new Vector3(0, 1, -2.5f), new Vector3(1f, 1f, 0.5f)),
                isTrigger: true
            );
            doorTrigger.OnTriggerEnter = (player, obj) =>
            {
                Console.WriteLine("[INFO] You are near the door. Press 'E' to interact (placeholder).");
            };
            _sceneObjects.Add(doorTrigger);

            // NPC placeholder with a trigger area
            var npc = new GameObject(
                name: "NPC_1",
                transform: new Transform(new Vector3(3, 1, -1), new Vector3(1, 2, 1), 0),
                collider: new AABBCollider(new Vector3(3, 1, -1), new Vector3(0.5f, 1f, 0.5f)),
                isTrigger: false
            );
            _sceneObjects.Add(npc);

            var npcTrigger = new GameObject(
                name: "NPC_1_Trigger",
                transform: new Transform(new Vector3(3, 1, 0.5f), new Vector3(2, 2, 2), 0),
                collider: new AABBCollider(new Vector3(3, 1, 0.5f), new Vector3(1f, 1f, 1f)),
                isTrigger: true
            );
            npcTrigger.OnTriggerEnter = (player, obj) =>
            {
                Console.WriteLine("[INFO] You are near NPC 1. Press 'E' to talk (placeholder).");
            };
            _sceneObjects.Add(npcTrigger);

            // Second NPC on the left
            var npc2 = new GameObject(
                name: "NPC_2",
                transform: new Transform(new Vector3(-3, 1, 1), new Vector3(1, 2, 1), 0),
                collider: new AABBCollider(new Vector3(-3, 1, 1), new Vector3(0.5f, 1f, 0.5f)),
                isTrigger: false
            );
            _sceneObjects.Add(npc2);

            var npc2Trigger = new GameObject(
                name: "NPC_2_Trigger",
                transform: new Transform(new Vector3(-3, 1, 2.5f), new Vector3(2, 2, 2), 0),
                collider: new AABBCollider(new Vector3(-3, 1, 2.5f), new Vector3(1f, 1f, 1f)),
                isTrigger: true
            );
            npc2Trigger.OnTriggerEnter = (player, obj) =>
            {
                Console.WriteLine("[INFO] You are near NPC 2. Press 'E' to talk (placeholder).");
            };
            _sceneObjects.Add(npc2Trigger);

            // Register scene colliders in collision system
            _collisionSystem.SceneObjects = _sceneObjects;
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            _time += (float)args.Time;

            if (!IsFocused)
                return;

            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
                Close();

            // Update player movement with collision
            _player.Update((float)args.Time, input, _collisionSystem);

            // Optional interaction key
            if (input.IsKeyPressed(Keys.E))
            {
                Console.WriteLine("[INFO] Interaction key pressed (E) - interaction logic can be implemented here.");
            }
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.UseProgram(_shaderProgram);
            GL.BindVertexArray(_vao);

            // First-person camera based on player position + yaw
            var playerPos = _player.Transform.Position;
            float yawRad = MathHelper.DegreesToRadians(_player.Transform.YawDegrees);
            Vector3 forward = new((float)Math.Sin(yawRad), 0, (float)Math.Cos(yawRad));

            Vector3 cameraPos = playerPos + new Vector3(0, 1.7f, 0); // eye height
            Matrix4 view = Matrix4.LookAt(cameraPos, cameraPos + forward * 10f, Vector3.UnitY);

            int mvpLocation = GL.GetUniformLocation(_shaderProgram, "u_MVP");
            int colorLocation = GL.GetUniformLocation(_shaderProgram, "u_Color");

            // Draw scene objects (each one is a cube scaled by its transform)
            foreach (var obj in _sceneObjects)
            {
                Matrix4 model = obj.Transform.GetModelMatrix();
                Matrix4 mvp = model * view * _projection;

                GL.UniformMatrix4(mvpLocation, false, ref mvp);

                // Slightly different colors based on type
                Vector3 color = new(0.6f, 0.6f, 0.6f);

                if (obj.Name.Contains("Wall"))
                    color = new Vector3(0.4f, 0.4f, 0.8f);
                else if (obj.Name.Contains("Box") || obj.Name.Contains("Pillar"))
                    color = new Vector3(0.6f, 0.4f, 0.2f);
                else if (obj.Name.Contains("Door"))
                    color = new Vector3(0.2f, 0.8f, 0.2f);
                else if (obj.Name.Contains("NPC"))
                    color = new Vector3(0.8f, 0.8f, 0.2f);
                else if (obj.IsTrigger)
                    color = new Vector3(0.2f, 0.8f, 0.8f); // triggers tinted cyan-ish

                GL.Uniform3(colorLocation, color);

                GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            }

            // In first-person mode we don't draw the player cube itself (it would overlap camera).

            SwapBuffers();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);

            float aspect = Size.X / (float)Size.Y;
            _projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60f), aspect, 0.1f, 100f);
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            GL.DeleteBuffer(_vbo);
            GL.DeleteVertexArray(_vao);
            GL.DeleteProgram(_shaderProgram);
        }

        // ---------------------------
        // Helpers
        // ---------------------------
        private int CreateBasicShader()
        {
            string vertexShaderSource = @"
                #version 330 core

                layout (location = 0) in vec3 a_Position;
                layout (location = 1) in vec3 a_Normal;

                uniform mat4 u_MVP;

                out vec3 v_Normal;

                void main()
                {
                    gl_Position = u_MVP * vec4(a_Position, 1.0);
                    v_Normal = a_Normal;
                }
            ";

            string fragmentShaderSource = @"
                #version 330 core

                in vec3 v_Normal;
                out vec4 FragColor;

                uniform vec3 u_Color;

                void main()
                {
                    vec3 lightDir = normalize(vec3(1.0, 1.0, 0.5));
                    float diff = max(dot(normalize(v_Normal), lightDir), 0.1);
                    vec3 color = u_Color * diff;
                    FragColor = vec4(color, 1.0);
                }
            ";

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.CompileShader(vertexShader);
            GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out int vStatus);
            if (vStatus == 0)
                Console.WriteLine(GL.GetShaderInfoLog(vertexShader));

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);
            GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out int fStatus);
            if (fStatus == 0)
                Console.WriteLine(GL.GetShaderInfoLog(fragmentShader));

            int program = GL.CreateProgram();
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
            GL.LinkProgram(program);
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int linkStatus);
            if (linkStatus == 0)
                Console.WriteLine(GL.GetProgramInfoLog(program));

            GL.DetachShader(program, vertexShader);
            GL.DetachShader(program, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            return program;
        }

        private float[] CubeVertices()
        {
            // Position (x,y,z) + Normal (nx,ny,nz)
            return new float[]
            {
                // Front face
                -0.5f, -0.5f,  0.5f,   0, 0, 1,
                 0.5f, -0.5f,  0.5f,   0, 0, 1,
                 0.5f,  0.5f,  0.5f,   0, 0, 1,
                -0.5f, -0.5f,  0.5f,   0, 0, 1,
                 0.5f,  0.5f,  0.5f,   0, 0, 1,
                -0.5f,  0.5f,  0.5f,   0, 0, 1,

                // Back face
                -0.5f, -0.5f, -0.5f,   0, 0, -1,
                 0.5f,  0.5f, -0.5f,   0, 0, -1,
                 0.5f, -0.5f, -0.5f,   0, 0, -1,
                -0.5f, -0.5f, -0.5f,   0, 0, -1,
                -0.5f,  0.5f, -0.5f,   0, 0, -1,
                 0.5f,  0.5f, -0.5f,   0, 0, -1,

                // Left face
                -0.5f, -0.5f, -0.5f,  -1, 0, 0,
                -0.5f, -0.5f,  0.5f,  -1, 0, 0,
                -0.5f,  0.5f,  0.5f,  -1, 0, 0,
                -0.5f, -0.5f, -0.5f,  -1, 0, 0,
                -0.5f,  0.5f,  0.5f,  -1, 0, 0,
                -0.5f,  0.5f, -0.5f,  -1, 0, 0,

                // Right face
                 0.5f, -0.5f, -0.5f,   1, 0, 0,
                 0.5f,  0.5f,  0.5f,   1, 0, 0,
                 0.5f, -0.5f,  0.5f,   1, 0, 0,
                 0.5f, -0.5f, -0.5f,   1, 0, 0,
                 0.5f,  0.5f, -0.5f,   1, 0, 0,
                 0.5f,  0.5f,  0.5f,   1, 0, 0,

                // Top face
                -0.5f,  0.5f, -0.5f,   0, 1, 0,
                -0.5f,  0.5f,  0.5f,   0, 1, 0,
                 0.5f,  0.5f,  0.5f,   0, 1, 0,
                -0.5f,  0.5f, -0.5f,   0, 1, 0,
                 0.5f,  0.5f,  0.5f,   0, 1, 0,
                 0.5f,  0.5f, -0.5f,   0, 1, 0,

                // Bottom face
                -0.5f, -0.5f, -0.5f,   0, -1, 0,
                 0.5f, -0.5f,  0.5f,   0, -1, 0,
                -0.5f, -0.5f,  0.5f,   0, -1, 0,
                -0.5f, -0.5f, -0.5f,   0, -1, 0,
                 0.5f, -0.5f, -0.5f,   0, -1, 0,
                 0.5f, -0.5f,  0.5f,   0, -1, 0,
            };
        }
    }

    // ---------------------------
    // Transform
    // ---------------------------
    public struct Transform
    {
        public Vector3 Position;
        public Vector3 Scale;
        public float YawDegrees;

        public Transform(Vector3 position, Vector3 scale, float yawDegrees)
        {
            Position = position;
            Scale = scale;
            YawDegrees = yawDegrees;
        }

        public Matrix4 GetModelMatrix()
        {
            float yawRad = MathHelper.DegreesToRadians(YawDegrees);
            Matrix4 translation = Matrix4.CreateTranslation(Position);
            Matrix4 rotation = Matrix4.CreateRotationY(yawRad);
            Matrix4 scale = Matrix4.CreateScale(Scale);
            return scale * rotation * translation;
        }
    }

    // ---------------------------
    // Collider (AABB)
    // ---------------------------
    public class AABBCollider
    {
        public Vector3 Center;
        public Vector3 HalfSize;
        public bool IsTrigger;

        public AABBCollider(Vector3 center, Vector3 halfSize, bool isTrigger = false)
        {
            Center = center;
            HalfSize = halfSize;
            IsTrigger = isTrigger;
        }

        public Vector3 Min => Center - HalfSize;
        public Vector3 Max => Center + HalfSize;

        public bool Intersects(AABBCollider other)
        {
            if (Max.X < other.Min.X || Min.X > other.Max.X) return false;
            if (Max.Y < other.Min.Y || Min.Y > other.Max.Y) return false;
            if (Max.Z < other.Min.Z || Min.Z > other.Max.Z) return false;
            return true;
        }
    }

    // ---------------------------
    // GameObject
    // ---------------------------
    public class GameObject
    {
        public string Name;
        public Transform Transform;
        public AABBCollider Collider;
        public bool IsTrigger => Collider.IsTrigger;

        // Called when player touches this trigger
        public Action<PlayerController, GameObject>? OnTriggerEnter;

        public GameObject(string name, Transform transform, AABBCollider collider, bool isTrigger)
        {
            Name = name;
            Transform = transform;
            collider.IsTrigger = isTrigger;
            Collider = collider;
        }
    }

    // ---------------------------
    // Player Controller
    // ---------------------------
    public class PlayerController
    {
        public Transform Transform;
        public AABBCollider Collider;
        public float MoveSpeed = 5f;
        public float RotationSpeed = 90f; // degrees per second

        /// <summary>
        /// Handles input, computes desired movement, and lets CollisionSystem
        /// block movement when a collision is predicted.
        /// </summary>
        public void Update(float deltaTime, KeyboardState input, CollisionSystem collisionSystem)
        {
            // Rotate player using left/right arrow keys (yaw)
            float yawDelta = 0f;
            if (input.IsKeyDown(Keys.Left))
                yawDelta += RotationSpeed * deltaTime;
            if (input.IsKeyDown(Keys.Right))
                yawDelta -= RotationSpeed * deltaTime;

            Transform.YawDegrees += yawDelta;

            float yawRad = MathHelper.DegreesToRadians(Transform.YawDegrees);
            Vector3 forward = new((float)Math.Sin(yawRad), 0, (float)Math.Cos(yawRad));
            Vector3 right = new(forward.Z, 0, -forward.X); // perpendicular in XZ plane

            Vector3 moveDir = Vector3.Zero;

            if (input.IsKeyDown(Keys.W)) moveDir += forward;
            if (input.IsKeyDown(Keys.S)) moveDir -= forward;
            if (input.IsKeyDown(Keys.A)) moveDir -= right;
            if (input.IsKeyDown(Keys.D)) moveDir += right;

            if (moveDir.LengthSquared > 0.0001f)
            {
                moveDir.Normalize();
                Vector3 displacement = moveDir * MoveSpeed * deltaTime;

                // Try to move with collision
                var newPos = collisionSystem.ResolveMovement(Transform.Position, Collider, displacement);

                Transform.Position = newPos;
                Collider.Center = newPos; // keep collider centered on player
            }

            // After moving, check trigger overlaps (e.g., door / NPC zones)
            collisionSystem.CheckTriggers(this);
        }
    }

    // ---------------------------
    // Collision System
    // ---------------------------
    public class CollisionSystem
    {
        public List<GameObject> SceneObjects = new();

        // Track which triggers were already touched this frame to avoid spamming
        private readonly HashSet<GameObject> _alreadyTriggered = new();

        /// <summary>
        /// Predictive movement + collision response:
        /// - Move along X, then Z (axis by axis) to avoid corner jitter.
        /// - If a move would overlap a solid AABB, cancel that axis movement.
        /// </summary>
        public Vector3 ResolveMovement(Vector3 currentPosition, AABBCollider playerCollider, Vector3 displacement)
        {
            Vector3 newPos = currentPosition;

            // Move in X
            if (Math.Abs(displacement.X) > 0.0001f)
            {
                Vector3 testPos = newPos + new Vector3(displacement.X, 0, 0);
                if (!WouldCollide(testPos, playerCollider))
                    newPos = testPos;
            }

            // Move in Z
            if (Math.Abs(displacement.Z) > 0.0001f)
            {
                Vector3 testPos = newPos + new Vector3(0, 0, displacement.Z);
                if (!WouldCollide(testPos, playerCollider))
                    newPos = testPos;
            }

            // (No vertical movement in this simple example, but same idea for Y)

            return newPos;
        }

        private bool WouldCollide(Vector3 testPosition, AABBCollider playerCollider)
        {
            var testCollider = new AABBCollider(testPosition, playerCollider.HalfSize);

            foreach (var obj in SceneObjects)
            {
                if (obj.Collider.IsTrigger)
                    continue; // triggers don't block movement

                if (testCollider.Intersects(obj.Collider))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Trigger detection for interaction zones (e.g., door / NPC).
        /// </summary>
        public void CheckTriggers(PlayerController player)
        {
            _alreadyTriggered.Clear();

            foreach (var obj in SceneObjects)
            {
                if (!obj.IsTrigger)
                    continue;

                if (player.Collider.Intersects(obj.Collider))
                {
                    if (!_alreadyTriggered.Contains(obj))
                    {
                        obj.OnTriggerEnter?.Invoke(player, obj);
                        _alreadyTriggered.Add(obj);
                    }
                }
            }
        }
    }
}
