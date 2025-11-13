// Program.cs — Movement + simple follow camera + PARALLAX BACKGROUND (mountains, clouds, glow)
// Controls: ←/→ (A/D) walk, Shift run, Space jump, ↓ (S) crouch,
//           ↑/↓ on a ladder to climb (↓ climbs only if already off ground).

using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using System;
using System.IO;
using System.Collections.Generic;
using ImageSharp = SixLabors.ImageSharp.Image;
using SixLabors.ImageSharp.PixelFormats;

namespace OpenTK_Sprite_Animation
{
    public class SpriteAnimationGame : GameWindow
    {
        private Player? _player;

        // GL objects
        private int _spriteProg, _colorProg;
        private int _vaoSprite, _vboSprite;
        private int _vaoRect, _vboRect;
        private int _texture;

        // Uniforms
        private int _uProjSprite, _uViewSprite;
        private int _uProjColor,  _uViewColor;

        // Screen & world
        private const float ScreenW = 800f, ScreenH = 600f;
        private const float WorldW  = 2400f, WorldH = 1400f;

        // Simple follow camera (centered on player)
        private Vector2 _cam = new Vector2(400, 300);
        private const float CamSmooth = 10f; // higher = snappier

        // ------- SCENE: parallax background -------
        private struct Cloud { public float x,y,w,h,speed; public Cloud(float x,float y,float w,float h,float s){this.x=x;this.y=y;this.w=w;this.h=h;this.speed=s;} }
        private readonly List<Rect> _mountainsFar  = new();
        private readonly List<Rect> _mountainsNear = new();
        private readonly List<Cloud> _clouds = new();

        // Ladders (bottoms on ground; tall)
        private static Rect MakeLadder(float x, float height)
            => new Rect(x, Player.GroundY + height * 0.5f, 44f, height);

        private readonly Rect[] _ladders = new[]
        {
            MakeLadder( 650f, 900f),
            MakeLadder(1200f, 900f),
            MakeLadder(1850f, 900f),
        };

        public SpriteAnimationGame()
            : base(new GameWindowSettings(),
                   new NativeWindowSettings { ClientSize = (800, 600), Title = "Sprite Game — Parallax Scene" })
        { }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.11f, 0.12f, 0.15f, 1f);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // --- Sprite shader + quad ---
            _spriteProg = CreateSpriteProgram();
            _texture = LoadTexture("Sprite_Character.png");

            float w = 32f, h = 64f; // 64x128 sprite quad
            float[] spriteVerts =
            {
                -w,-h, 0f,0f,
                 w,-h, 1f,0f,
                 w, h, 1f,1f,
                -w, h, 0f,1f
            };
            _vaoSprite = GL.GenVertexArray();
            GL.BindVertexArray(_vaoSprite);
            _vboSprite = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboSprite);
            GL.BufferData(BufferTarget.ArrayBuffer, spriteVerts.Length*sizeof(float), spriteVerts, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4*sizeof(float), 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4*sizeof(float), 2*sizeof(float));

            GL.UseProgram(_spriteProg);
            GL.Uniform1(GL.GetUniformLocation(_spriteProg, "uTexture"), 0);
            _uProjSprite = GL.GetUniformLocation(_spriteProg, "projection");
            _uViewSprite = GL.GetUniformLocation(_spriteProg, "view");

            // --- Color shader + unit quad ---
            _colorProg = CreateColorProgram();
            _vaoRect = GL.GenVertexArray();
            GL.BindVertexArray(_vaoRect);
            _vboRect = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboRect);
            float[] rectVerts = { -0.5f,-0.5f, 0.5f,-0.5f, 0.5f,0.5f, -0.5f,0.5f };
            GL.BufferData(BufferTarget.ArrayBuffer, rectVerts.Length*sizeof(float), rectVerts, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2*sizeof(float), 0);

            GL.UseProgram(_colorProg);
            _uProjColor = GL.GetUniformLocation(_colorProg, "projection");
            _uViewColor = GL.GetUniformLocation(_colorProg, "view");

            // Projection: window pixel coords (0..W, 0..H)
            Matrix4 proj = Matrix4.CreateOrthographicOffCenter(0, ScreenW, 0, ScreenH, -1, 1);
            GL.UseProgram(_spriteProg); GL.UniformMatrix4(_uProjSprite, false, ref proj);
            GL.UseProgram(_colorProg ); GL.UniformMatrix4(_uProjColor , false, ref proj);

            // Player
            _player = new Player(_spriteProg, new Vector2(200f, Player.GroundY), worldWidth: WorldW);
            _player.SetLadders(_ladders);

            // --- Build background content ---
            BuildParallaxScene();

            // Initial view upload
            UploadView();
        }

        private void BuildParallaxScene()
        {
            // Mountains: blocks with varied widths/heights (stylized)
            var rng = new Random(42);
            float horizonY = Player.GroundY + 120f;

            // Far mountains
            float x = 0f;
            while (x < WorldW + 400f)
            {
                float w = rng.Next(220, 360);
                float h = rng.Next(140, 220);
                _mountainsFar.Add(new Rect(x + w*0.5f, horizonY + h*0.5f, w, h));
                x += w * 0.7f;
            }

            // Near mountains
            x = -200f;
            while (x < WorldW + 400f)
            {
                float w = rng.Next(180, 300);
                float h = rng.Next(200, 320);
                _mountainsNear.Add(new Rect(x + w*0.5f, horizonY + 40f + h*0.5f, w, h));
                x += w * 0.8f;
            }

            // Clouds
            for (int i = 0; i < 10; i++)
            {
                float cx = (float)rng.NextDouble() * (WorldW + 1200f) - 600f;
                float cy = horizonY + 220f + rng.Next(-60, 160);
                float cw = rng.Next(140, 220);
                float ch = rng.Next(50, 80);
                float speed = 12f + (float)rng.NextDouble() * 18f; // slow drift
                _clouds.Add(new Cloud(cx, cy, cw, ch, speed));
            }
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            if (_player is null) return;

            var k = KeyboardState;
            bool left   = k.IsKeyDown(Keys.Left)  || k.IsKeyDown(Keys.A);
            bool right  = k.IsKeyDown(Keys.Right) || k.IsKeyDown(Keys.D);
            bool sprint = k.IsKeyDown(Keys.LeftShift) || k.IsKeyDown(Keys.RightShift);
            bool crouch = k.IsKeyDown(Keys.Down) || k.IsKeyDown(Keys.S);
            bool up     = k.IsKeyDown(Keys.Up)   || k.IsKeyDown(Keys.W);
            bool down   = k.IsKeyDown(Keys.Down) || k.IsKeyDown(Keys.S);
            bool jumpPressed = k.IsKeyPressed(Keys.Space);

            _player.Update((float)e.Time, left, right, sprint, crouch, up, down, jumpPressed);

            // --- Camera: center on player, clamp to world, mild smoothing ---
            Vector2 target = _player.Position;
            _cam = Vector2.Lerp(_cam, target, MathF.Min(1f, CamSmooth * (float)e.Time));

            float halfW = ScreenW * 0.5f;
            float halfH = ScreenH * 0.5f;
            _cam.X = MathHelper.Clamp(_cam.X, halfW, Math.Max(halfW, WorldW - halfW));
            _cam.Y = MathHelper.Clamp(_cam.Y, halfH, Math.Max(halfH, WorldH - halfH));

            // Clouds drift (wrap)
            for (int i = 0; i < _clouds.Count; i++)
            {
                var c = _clouds[i];
                c.x += c.speed * (float)e.Time;
                float wrap = WorldW + 1600f;
                if (c.x - c.w*0.5f > WorldW + 800f) c.x -= wrap;
                _clouds[i] = c;
            }

            UploadView();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Colors
            Vector4 skyTop   = new Vector4(0.52f, 0.76f, 0.98f, 1f);
            Vector4 skyBase  = new Vector4(0.46f, 0.70f, 0.96f, 1f);
            Vector4 farMount = new Vector4(0.58f, 0.66f, 0.78f, 1f);
            Vector4 nearMount= new Vector4(0.42f, 0.50f, 0.64f, 1f);
            Vector4 cloudCol = new Vector4(0.95f, 0.97f, 1.00f, 0.90f);
            Vector4 glowCol  = new Vector4(1.00f, 0.92f, 0.55f, 0.12f);

            float horizonY = Player.GroundY + 120f;

            // World background
            GL.UseProgram(_colorProg);
            GL.BindVertexArray(_vaoRect);

            // Sky base + top tint (fake gradient)
            DrawRect(new Rect(WorldW*0.5f, WorldH*0.5f, WorldW, WorldH), skyBase);
            DrawRect(new Rect(WorldW*0.5f, horizonY + 400f, WorldW, WorldH), skyTop * new Vector4(1,1,1,0.35f));

            // Far mountains (slow parallax)
            foreach (var r in _mountainsFar)
                DrawParallaxRect(r, farMount, parallax: 0.35f);

            // Near mountains
            foreach (var r in _mountainsNear)
                DrawParallaxRect(r, nearMount, parallax: 0.55f);

            // Clouds
            foreach (var c in _clouds)
                DrawParallaxRect(new Rect(c.x, c.y, c.w, c.h), cloudCol, parallax: 0.70f);

            // Horizon glow to make the ground line pop
            DrawRect(new Rect(WorldW*0.5f, Player.GroundY + 70f, WorldW, 160f), glowCol);

            // Ground + ladders (gameplay layer)
            DrawRect(new Rect(WorldW*0.5f, Player.GroundY-10f, WorldW, 20f),
                     new Vector4(0.22f, 0.28f, 0.20f, 1f)); // ground

            foreach (var L in _ladders)
            {
                DrawRect(L, new Vector4(0.55f, 0.34f, 0.18f, 1f)); // posts
                for (float y = L.y - L.h*0.45f; y <= L.y + L.h*0.45f; y += 28f)
                    DrawRect(new Rect(L.x, y, L.w+36f, 6f), new Vector4(0.35f, 0.22f, 0.10f, 1f)); // rungs
            }

            // Player
            if (_player is not null)
            {
                GL.UseProgram(_spriteProg);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, _texture);
                GL.BindVertexArray(_vaoSprite);
                _player.Render();
            }

            SwapBuffers();
        }

        // --- drawing helpers -------------------------------------------------------------------
        private void UploadView()
        {
            float halfW = ScreenW * 0.5f;
            float halfH = ScreenH * 0.5f;
            Matrix4 view = Matrix4.CreateTranslation(-_cam.X + halfW, -_cam.Y + halfH, 0);

            GL.UseProgram(_spriteProg); GL.UniformMatrix4(_uViewSprite, false, ref view);
            GL.UseProgram(_colorProg ); GL.UniformMatrix4(_uViewColor , false, ref view);
        }

        private void DrawRect(Rect r, Vector4 color)
        {
            int uModel = GL.GetUniformLocation(_colorProg, "model");
            int uColor = GL.GetUniformLocation(_colorProg, "uColor");
            Matrix4 model = Matrix4.CreateScale(r.w, r.h, 1) * Matrix4.CreateTranslation(r.x, r.y, 0);
            GL.UniformMatrix4(uModel, false, ref model);
            GL.Uniform4(uColor, color);
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);
        }

        // Parallax: apply extra X shift proportional to camera offset so layer moves slower
        private void DrawParallaxRect(Rect r, Vector4 color, float parallax)
        {
            float halfW = ScreenW * 0.5f;
            float camOffset = (_cam.X - halfW) * (1f - parallax);
            Rect shifted = new Rect(r.x + camOffset, r.y, r.w, r.h);
            DrawRect(shifted, color);
        }

        protected override void OnUnload()
        {
            GL.DeleteProgram(_spriteProg);
            GL.DeleteProgram(_colorProg);
            GL.DeleteTexture(_texture);
            GL.DeleteBuffer(_vboSprite);
            GL.DeleteVertexArray(_vaoSprite);
            GL.DeleteBuffer(_vboRect);
            GL.DeleteVertexArray(_vaoRect);
            base.OnUnload();
        }

        // -------------------- Shaders / Texture -------------------------------------------------

        private int CreateSpriteProgram()
        {
            string vs = @"
#version 330 core
layout(location=0) in vec2 aPosition;
layout(location=1) in vec2 aTexCoord;
out vec2 vTexCoord;
uniform mat4 projection, view, model;
void main(){
    gl_Position = projection * view * model * vec4(aPosition,0.0,1.0);
    vTexCoord = vec2(aTexCoord.x, 1.0 - aTexCoord.y);
}";
            string fs = @"
#version 330 core
in vec2 vTexCoord;
out vec4 color;
uniform sampler2D uTexture;
uniform vec2 uOffset, uSize;
void main(){
    vec2 uv = uOffset + vTexCoord * uSize;
    color = texture(uTexture, uv);
}";
            return CompileLink(vs, fs);
        }

        private int CreateColorProgram()
        {
            string vs = @"
#version 330 core
layout(location=0) in vec2 aPos;
uniform mat4 projection, view, model;
void main(){
    gl_Position = projection * view * model * vec4(aPos,0.0,1.0);
}";
            string fs = @"
#version 330 core
out vec4 color;
uniform vec4 uColor;
void main(){ color = uColor; }";
            return CompileLink(vs, fs);
        }

        private int CompileLink(string vsSrc, string fsSrc)
        {
            int v = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(v, vsSrc); GL.CompileShader(v);
            GL.GetShader(v, ShaderParameter.CompileStatus, out int ok);
            if (ok == 0) throw new Exception("VERTEX SHADER: " + GL.GetShaderInfoLog(v));
            int f = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(f, fsSrc); GL.CompileShader(f);
            GL.GetShader(f, ShaderParameter.CompileStatus, out ok);
            if (ok == 0) throw new Exception("FRAGMENT SHADER: " + GL.GetShaderInfoLog(f));
            int p = GL.CreateProgram();
            GL.AttachShader(p, v); GL.AttachShader(p, f); GL.LinkProgram(p);
            GL.GetProgram(p, GetProgramParameterName.LinkStatus, out ok);
            if (ok == 0) throw new Exception("PROGRAM LINK: " + GL.GetProgramInfoLog(p));
            GL.DetachShader(p, v); GL.DetachShader(p, f); GL.DeleteShader(v); GL.DeleteShader(f);
            return p;
        }

        private int LoadTexture(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Texture not found: {path}", path);
            using var img = ImageSharp.Load<Rgba32>(path);
            int tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, tex);
            var pixels = new byte[4*img.Width*img.Height];
            img.CopyPixelDataTo(pixels);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, img.Width, img.Height, 0,
                          PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            return tex;
        }
    }

    // ------------------------------- Geometry (ONE definition) ---------------------------------
    public readonly struct Rect
    {
        public readonly float x, y, w, h;   // center (x,y), size (w,h)
        public Rect(float cx, float cy, float w, float h){ x=cx; y=cy; this.w=w; this.h=h; }
        public bool Contains(Vector2 p) =>
            MathF.Abs(p.X - x) <= w*0.5f && MathF.Abs(p.Y - y) <= h*0.5f;
    }

    // ---------------------------------- Player / Animation -------------------------------------
    public enum MoveState { Idle, Walk, Run, Jump, Fall, Crouch, Climb }
    public enum Facing { Right=0, Left=1 }

    public class Player
    {
        private readonly int _shader;
        private readonly int _uModel, _uOffset, _uSize;

        public Vector2 Position;
        private Vector2 _velocity;
        private bool _onGround = true;
        private Rect[] _ladders = Array.Empty<Rect>();
        private readonly float _worldWidth;

        // Movement tuning
        private const float WalkSpeed   = 160f;
        private const float RunSpeed    = 260f;
        private const float JumpVel     = 380f;
        private const float Gravity     = 1000f;
        private const float ClimbSpeed  = 150f;

        // Sprite half-height
        private const float SpriteHalfH = 64f;

        // Crouch squash
        private const float CrouchScaleY = 0.75f;

        public const float GroundY = 300f;

        // State
        private MoveState _state = MoveState.Idle;
        private Facing _face = Facing.Right;

        // Anim timing
        private float _timer; private int _frame;

        // Atlas geometry (adjust if your sheet differs)
        private const float FrameW = 64f, FrameH = 128f, GapX = 60f;
        private const float TotalW = FrameW + GapX;
        private const float SheetW = 4*TotalW - GapX;
        private const float SheetH = 256f;

        private readonly Anim IdleAnim   = new(0,1,1,0.20f);
        private readonly Anim WalkAnim   = new(0,1,4,0.15f);
        private readonly Anim RunAnim    = new(0,1,4,0.09f);
        private readonly Anim JumpAnim   = new(0,1,1,0.20f);
        private readonly Anim FallAnim   = new(0,1,1,0.20f);
        private readonly Anim CrouchAnim = new(0,1,1,0.20f);
        private readonly Anim ClimbAnim  = new(0,1,2,0.18f);

        private struct Anim
        {
            public readonly int RowRight, RowLeft, Frames; public readonly float FrameTime;
            public Anim(int rr, int rl, int f, float t){ RowRight=rr; RowLeft=rl; Frames=f; FrameTime=t; }
            public int Row(Facing f) => f==Facing.Right ? RowRight : RowLeft;
        }

        public Player(int shader, Vector2 startPos, float worldWidth)
        {
            _shader = shader; Position = startPos; _worldWidth = worldWidth;
            _uModel  = GL.GetUniformLocation(_shader, "model");
            _uOffset = GL.GetUniformLocation(_shader, "uOffset");
            _uSize   = GL.GetUniformLocation(_shader, "uSize");
            ApplyFrame(0, IdleAnim.Row(_face));
        }

        public void SetLadders(Rect[] ladders) => _ladders = ladders;

        public void Update(float dt, bool left, bool right, bool sprint, bool crouchKey, bool upKey, bool downKey, bool jumpPressed)
        {
            float dir = 0f;
            if (left)  { dir -= 1f; _face = Facing.Left; }
            if (right) { dir += 1f; _face = Facing.Right; }

            bool onLadder = IsOnAnyLadder(Position, padX: 10f);
            bool applyGravity = true;

            switch (_state)
            {
                case MoveState.Climb:
                    _velocity.X = 0f;
                    _velocity.Y = (upKey ? +ClimbSpeed : downKey ? -ClimbSpeed : 0f);
                    applyGravity = false;
                    if (jumpPressed){ _velocity.Y = JumpVel; _onGround=false; ChangeState(MoveState.Jump); applyGravity=true; }
                    else if (!onLadder){ _onGround=false; ChangeState(MoveState.Fall); }
                    break;

                default:
                    float maxSpeed = sprint ? RunSpeed : WalkSpeed;
                    _velocity.X = dir * maxSpeed;

                    bool wantsCrouch = crouchKey && _onGround;

                    if (jumpPressed && _onGround && !wantsCrouch)
                    {
                        _onGround = false; _velocity.Y = JumpVel; ChangeState(MoveState.Jump);
                    }

                    // climb only with UP, or DOWN when off ground
                    if (!wantsCrouch && onLadder && (upKey || (downKey && !_onGround)))
                    {
                        ChangeState(MoveState.Climb); _onGround=false; applyGravity=false;
                        _velocity.X = 0f; _velocity.Y = (upKey ? +ClimbSpeed : -ClimbSpeed);
                    }
                    else
                    {
                        if (!_onGround) ChangeState(_velocity.Y >= 0 ? MoveState.Jump : MoveState.Fall);
                        else if (wantsCrouch){ _velocity.X = 0; ChangeState(MoveState.Crouch); }
                        else if (Math.Abs(_velocity.X) > 1f) ChangeState(sprint ? MoveState.Run : MoveState.Walk);
                        else ChangeState(MoveState.Idle);
                    }
                    break;
            }

            if (applyGravity && !_onGround) _velocity.Y -= Gravity * dt;

            // integrate
            Position += _velocity * dt;

            // ground collision (feet)
            float feetY = Position.Y - SpriteHalfH;
            if (feetY <= GroundY)
            {
                Position.Y = GroundY + SpriteHalfH; _velocity.Y = 0; _onGround = true;
                if (_state is MoveState.Jump or MoveState.Fall)
                    ChangeState(Math.Abs(_velocity.X) > 1f ? MoveState.Walk : MoveState.Idle);
            }

            // clamp X
            Position.X = MathHelper.Clamp(Position.X, 40f, _worldWidth - 40f);

            // animate
            var anim = GetAnim(_state);
            if (anim.Frames > 1)
            {
                _timer += dt;
                if (_timer >= anim.FrameTime)
                {
                    _timer -= anim.FrameTime;
                    _frame = (_frame + 1) % anim.Frames;
                    ApplyFrame(_frame, anim.Row(_face));
                }
            }

            // model with crouch squash (feet pinned)
            float scaleY = (_state == MoveState.Crouch) ? CrouchScaleY : 1f;
            float feetComp = (1f - scaleY) * SpriteHalfH;
            Matrix4 model = Matrix4.CreateScale(1f, scaleY, 1f)
                          * Matrix4.CreateTranslation(Position.X, Position.Y - feetComp, 0);
            GL.UseProgram(_shader);
            GL.UniformMatrix4(_uModel, false, ref model);
        }

        public void Render() => GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);

        private void ChangeState(MoveState s)
        {
            if (s == _state) return;
            _state = s; _timer = 0f; _frame = 0;
            var anim = GetAnim(_state); ApplyFrame(_frame, anim.Row(_face));
        }

        private Anim GetAnim(MoveState s) => s switch
        {
            MoveState.Idle   => IdleAnim,
            MoveState.Walk   => WalkAnim,
            MoveState.Run    => RunAnim,
            MoveState.Jump   => JumpAnim,
            MoveState.Fall   => FallAnim,
            MoveState.Crouch => CrouchAnim,
            MoveState.Climb  => ClimbAnim,
            _ => IdleAnim
        };

        public bool IsVertical() => _state is MoveState.Climb or MoveState.Jump or MoveState.Fall;

        private bool IsOnAnyLadder(Vector2 p, float padX)
        {
            foreach (var L in _ladders)
                if (MathF.Abs(p.X - L.x) <= L.w*0.5f + padX && MathF.Abs(p.Y - L.y) <= L.h*0.5f)
                    return true;
            return false;
        }

        private void ApplyFrame(int col, int row)
        {
            float u = (col * TotalW) / SheetW;
            float v = (row * FrameH) / SheetH;
            float w = FrameW / SheetW;
            float h = FrameH / SheetH;
            GL.UseProgram(_shader);
            GL.Uniform2(_uOffset, u, v);
            GL.Uniform2(_uSize, w, h);
        }
    }

    internal class Program
    {
        private static void Main()
        {
            using var game = new SpriteAnimationGame();
            game.Run();
        }
    }
}
