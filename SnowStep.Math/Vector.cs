namespace SnowStep.Math
{
    public class Vector
    {
        public static Vector operator +(Vector v1, Vector v2) => new Vector(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        public static Vector operator -(Vector v1, Vector v2) => new Vector(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        public static Vector operator *(Vector v, double m) => new Vector(v.X * m, v.Y * m, v.Z * m);
        public static Vector operator /(Vector v, double m) => new Vector(v.X / m, v.Y / m, v.Z / m);
        public static Vector operator *(Vector v1, Vector v2) => new Vector(v1.Y * v2.Z - v1.Z * v2.Y, v1.Z * v2.X - v1.X * v2.Z, v1.X * v2.Y - v1.Y * v2.X);

        public static double InnerProduct(Vector v1, Vector v2) => v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Vector(double x, double y, double z) => (this.X, this.Y, this.Z) = (x, y, z);

        public Vector Rotate(Quarternion q) => q * this * q.Conj;
    }
}
