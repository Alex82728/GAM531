using OpenTK.Mathematics;

namespace Game.GLAbstractions
{
    public class Camera
    {
        public Vector3 Position;
        public float Pitch;
        public float Yaw;
        public float Fov = 60f;
        public float AspectRatio { get; set; }

        public Vector3 Front => new(
            MathF.Cos(MathHelper.DegreesToRadians(Pitch)) * MathF.Cos(MathHelper.DegreesToRadians(Yaw)),
            MathF.Sin(MathHelper.DegreesToRadians(Pitch)),
            MathF.Cos(MathHelper.DegreesToRadians(Pitch)) * MathF.Sin(MathHelper.DegreesToRadians(Yaw))
        );

        public Vector3 Right => Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
        public Vector3 Up => Vector3.Normalize(Vector3.Cross(Right, Front));

        public Camera(Vector3 position, float aspect)
        {
            Position = position;
            AspectRatio = aspect;
            Yaw = -90f;
            Pitch = 0f;
        }

        public Matrix4 GetViewMatrix() => Matrix4.LookAt(Position, Position + Front, Up);
        public Matrix4 GetProjectionMatrix() =>
            Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Fov), AspectRatio, 0.1f, 100f);
    }
}
