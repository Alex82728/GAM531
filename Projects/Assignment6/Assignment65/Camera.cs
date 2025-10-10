using OpenTK.Mathematics;

namespace Assignment6
{
    public class Camera
    {
        public Vector3 Position { get; set; }
        public float Pitch { get; set; } = 0f;
        public float Yaw { get; set; } = -90f;
        public float Fov { get; set; } = 45f;

        private float _aspectRatio;

        public Camera(Vector3 position, float aspectRatio)
        {
            Position = position;
            _aspectRatio = aspectRatio;
        }

        public Vector3 Front
        {
            get
            {
                var front = new Vector3(
                    MathF.Cos(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch)),
                    MathF.Sin(MathHelper.DegreesToRadians(Pitch)),
                    MathF.Sin(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch))
                );
                return Vector3.Normalize(front);
            }
        }

        public Vector3 Right => Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
        public Vector3 Up => Vector3.Normalize(Vector3.Cross(Right, Front));

        public void UpdateAspectRatio(float aspect) => _aspectRatio = aspect;

        public Matrix4 GetViewMatrix() => Matrix4.LookAt(Position, Position + Front, Vector3.UnitY);
        public Matrix4 GetProjectionMatrix() =>
            Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Fov), _aspectRatio, 0.1f, 100f);
    }
}
