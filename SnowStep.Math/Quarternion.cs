namespace SnowStep.Math
{
    public class Quarternion
    {
        public static implicit operator Vector(Quarternion q) => q.Imag;

        public static Quarternion operator +(Quarternion q1, Quarternion q2) => new Quarternion(q1.W + q2.W, q1.X + q2.X, q1.Y + q2.Y, q1.Z + q2.Z);
        public static Quarternion operator -(Quarternion q1, Quarternion q2) => new Quarternion(q1.W - q2.W, q1.X - q2.X, q1.Y - q2.Y, q1.Z - q2.Z);
        public static Quarternion operator *(Quarternion q, double m) => new Quarternion(q.W * m, q.X * m, q.Y * m, q.Z * m);
        public static Quarternion operator /(Quarternion q, double m) => new Quarternion(q.W / m, q.X / m, q.Y / m, q.Z / m);
        public static Quarternion operator *(Quarternion q1, Quarternion q2) => new Quarternion(q1.Real * q2.Real - Vector.InnerProduct(q1.Imag, q2.Imag), q1.Imag * q2.Real + q2.Imag * q1.Real + q1.Imag * q2.Imag);
        public static Quarternion operator *(Quarternion q, Vector v) => new Quarternion(-Vector.InnerProduct(q.Imag, v), v * q.Real + q.Imag * v);
        public static Quarternion operator *(Vector v, Quarternion q) => new Quarternion(-Vector.InnerProduct(q.Imag, v), v * q.Real + v * q.Imag);

        public static Quarternion From(in Vector v, double theta) => new Quarternion(System.Math.Cos(theta / 2), v * System.Math.Sin(theta / 2));

        public double W { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public double Real { get => this.W; set => this.W = value; }
        public Vector Imag { get => new Vector(this.X, this.Y, this.Z); set => (this.X, this.Y, this.Z) = (value.X, value.Y, value.Z); }
        public Quarternion Conj { get => new Quarternion(this.W, -this.X, -this.Y, -this.Z); }

        public Quarternion(double w, double x, double y, double z) => (this.W, this.X, this.Y, this.Z) = (w, x, y, z);
        public Quarternion(double w, Vector v) => (this.W, this.X, this.Y, this.Z) = (w, v.X, v.Y, v.Z);
    }
}
