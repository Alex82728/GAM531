namespace OpenTKRectangleDemo.MathLib
{
    public class Matrix4x4D
    {
        private double[,] _m = new double[4,4];

        private Matrix4x4D() { }

        public static Matrix4x4D Identity()
        {
            var m = new Matrix4x4D();
            m._m[0,0] = 1; m._m[1,1] = 1; m._m[2,2] = 1; m._m[3,3] = 1;
            return m;
        }

        public static Matrix4x4D CreateScale(double sx, double sy, double sz)
        {
            var m = Identity();
            m._m[0,0] = sx;
            m._m[1,1] = sy;
            m._m[2,2] = sz;
            return m;
        }

        public static Matrix4x4D CreateRotationZ(double radians)
        {
            var m = Identity();
            var c = Math.Cos(radians);
            var s = Math.Sin(radians);
            m._m[0,0] = c; m._m[0,1] = -s;
            m._m[1,0] = s; m._m[1,1] = c;
            return m;
        }

        public static Vector3D operator *(Matrix4x4D mat, Vector3D vec)
        {
            double x = mat._m[0,0]*vec.X + mat._m[0,1]*vec.Y + mat._m[0,2]*vec.Z;
            double y = mat._m[1,0]*vec.X + mat._m[1,1]*vec.Y + mat._m[1,2]*vec.Z;
            double z = mat._m[2,0]*vec.X + mat._m[2,1]*vec.Y + mat._m[2,2]*vec.Z;
            return new Vector3D(x, y, z);
        }

        public static Matrix4x4D operator *(Matrix4x4D a, Matrix4x4D b)
        {
            var result = new Matrix4x4D();
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    for (int k = 0; k < 4; k++)
                        result._m[i,j] += a._m[i,k] * b._m[k,j];
            return result;
        }
    }
}
