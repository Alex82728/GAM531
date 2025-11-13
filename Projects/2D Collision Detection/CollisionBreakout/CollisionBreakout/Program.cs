using System;
using System.Numerics;
using System.Collections.Generic;
using Raylib_cs;

namespace CollisionBreakout
{
    class GameObject
    {
        public Vector2 Position;
        public Vector2 Size;
        public bool Destroyed = false;

        public Rectangle GetRect()
        {
            return new Rectangle(Position.X, Position.Y, Size.X, Size.Y);
        }
    }

    class Ball
    {
        public Vector2 Position;
        public float Radius;
        public Vector2 Velocity;
    }

    static class Program
    {
        const int ScreenWidth = 800;
        const int ScreenHeight = 600;

        static void Main()
        {
            Raylib.InitWindow(ScreenWidth, ScreenHeight, "2D Collision Detection - Breakout Prototype");
            Raylib.SetTargetFPS(60);

            // Paddle
            GameObject paddle = new GameObject
            {
                Size = new Vector2(100, 20),
                Position = new Vector2(ScreenWidth / 2f - 50, ScreenHeight - 40)
            };

            // Ball
            Ball ball = new Ball
            {
                Radius = 8f,
                Position = new Vector2(ScreenWidth / 2f, ScreenHeight / 2f),
                Velocity = new Vector2(200f, -250f)
            };

            // Bricks
            List<GameObject> bricks = CreateLevel();

            while (!Raylib.WindowShouldClose())
            {
                float dt = Raylib.GetFrameTime();

                UpdateInput(paddle, dt);
                UpdateBall(ball, paddle, bricks, dt);

                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.DarkBlue);

                // Draw bricks
                foreach (var brick in bricks)
                {
                    if (brick.Destroyed) continue;
                    Raylib.DrawRectangleRec(brick.GetRect(), Color.Orange);
                }

                // Draw paddle
                Raylib.DrawRectangleRec(paddle.GetRect(), Color.LightGray);

                // Draw ball
                Raylib.DrawCircleV(ball.Position, ball.Radius, Color.Yellow);

                // HUD text
                Raylib.DrawText("Arrow keys: move paddle", 10, 10, 18, Color.White);
                Raylib.DrawText("Break the bricks using circle-AABB collisions", 10, 30, 18, Color.White);

                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }

        static List<GameObject> CreateLevel()
        {
            List<GameObject> bricks = new List<GameObject>();

            int rows = 5;
            int cols = 10;
            float brickWidth = 70;
            float brickHeight = 20;
            float offsetX = 35;
            float offsetY = 60;
            float gap = 5;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    GameObject brick = new GameObject();
                    brick.Size = new Vector2(brickWidth, brickHeight);

                    float x = offsetX + col * (brickWidth + gap);
                    float y = offsetY + row * (brickHeight + gap);
                    brick.Position = new Vector2(x, y);

                    bricks.Add(brick);
                }
            }

            return bricks;
        }

        static void UpdateInput(GameObject paddle, float dt)
        {
            float speed = 400f;

            if (Raylib.IsKeyDown(KeyboardKey.Left))
            {
                paddle.Position.X -= speed * dt;
            }
            if (Raylib.IsKeyDown(KeyboardKey.Right))
            {
                paddle.Position.X += speed * dt;
            }

            // Clamp paddle inside screen
            paddle.Position.X = Math.Clamp(paddle.Position.X, 0, ScreenWidth - paddle.Size.X);
        }

        static void UpdateBall(Ball ball, GameObject paddle, List<GameObject> bricks, float dt)
        {
            // Move the ball
            ball.Position += ball.Velocity * dt;

            // Screen border collisions (like AABB vs walls)
            if (ball.Position.X - ball.Radius <= 0)
            {
                ball.Position.X = ball.Radius;
                ball.Velocity.X *= -1;
            }
            if (ball.Position.X + ball.Radius >= ScreenWidth)
            {
                ball.Position.X = ScreenWidth - ball.Radius;
                ball.Velocity.X *= -1;
            }
            if (ball.Position.Y - ball.Radius <= 0)
            {
                ball.Position.Y = ball.Radius;
                ball.Velocity.Y *= -1;
            }

            // Ball falls below screen: reset
            if (ball.Position.Y - ball.Radius > ScreenHeight)
            {
                ball.Position = new Vector2(ScreenWidth / 2f, ScreenHeight / 2f);
                ball.Velocity = new Vector2(200f, -250f);
            }

            // Paddle collision (circle vs AABB)
            if (CheckCircleAABBCollision(ball, paddle))
            {
                // Simple bounce upwards
                ball.Velocity.Y = -Math.Abs(ball.Velocity.Y);

                // Slight control based on where ball hits the paddle
                float paddleCenter = paddle.Position.X + paddle.Size.X / 2f;
                float distanceFromCenter = ball.Position.X - paddleCenter;
                ball.Velocity.X += distanceFromCenter * 2.0f;
            }

            // Brick collisions
            foreach (var brick in bricks)
            {
                if (brick.Destroyed) continue;

                if (CheckCircleAABBCollision(ball, brick))
                {
                    brick.Destroyed = true;

                    // Basic bounce: invert Y
                    ball.Velocity.Y *= -1;

                    // Break after first hit to avoid multiple bounces
                    break;
                }
            }
        }

        // AABB-AABB collision (for reference)
        static bool CheckAABBCollision(GameObject a, GameObject b)
        {
            Rectangle ra = a.GetRect();
            Rectangle rb = b.GetRect();

            bool overlapX = ra.X + ra.Width >= rb.X && ra.X <= rb.X + rb.Width;
            bool overlapY = ra.Y + ra.Height >= rb.Y && ra.Y <= rb.Y + rb.Height;

            return overlapX && overlapY;
        }

        // Circle-AABB collision: ball vs rectangle
        static bool CheckCircleAABBCollision(Ball ball, GameObject rectObj)
        {
            Rectangle r = rectObj.GetRect();

            float rectLeft   = r.X;
            float rectRight  = r.X + r.Width;
            float rectTop    = r.Y;
            float rectBottom = r.Y + r.Height;

            float closestX = Math.Clamp(ball.Position.X, rectLeft, rectRight);
            float closestY = Math.Clamp(ball.Position.Y, rectTop, rectBottom);

            float dx = ball.Position.X - closestX;
            float dy = ball.Position.Y - closestY;

            float distanceSquared = dx * dx + dy * dy;

            return distanceSquared <= ball.Radius * ball.Radius;
        }
    }
}
