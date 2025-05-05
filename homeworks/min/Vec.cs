namespace MinimizerProject
{
    using System;
    using System.Text;

    public class Vec
    {
        private double[] data;
        public int Length => data.Length;

        public Vec() : this(3) { }
        public Vec(int dim)
        {
            if (dim <= 0) throw new ArgumentException("Invalid dimension.");
            data = new double[dim];
        }
        public Vec(double[] values)
        {
            if (values == null || values.Length == 0) throw new ArgumentException("Values cannot be null or empty.");
            data = (double[])values.Clone();
        }
        public Vec(double x, double y, double z) : this(new double[] { x, y, z }) { }

        public double this[int i]
        {
            get => data[i];
            set => data[i] = value;
        }

        public double Norm()
        {
            double sum = 0;
            for (int i = 0; i < Length; i++) sum += data[i] * data[i];
            return Math.Sqrt(sum);
        }

        public Vec Copy() => new Vec(data);

        public static Vec operator +(Vec a, Vec b)
        {
            if (a.Length != b.Length) throw new ArgumentException("Vectors must be same length.");
            var r = new Vec(a.Length);
            for (int i = 0; i < a.Length; i++) r[i] = a[i] + b[i];
            return r;
        }

        public static Vec operator -(Vec a, Vec b)
        {
            if (a.Length != b.Length) throw new ArgumentException("Vectors must be same length.");
            var r = new Vec(a.Length);
            for (int i = 0; i < a.Length; i++) r[i] = a[i] - b[i];
            return r;
        }

        public static Vec operator -(Vec v) => v * -1;
        public static Vec operator *(Vec v, double c)
        {
            var r = new Vec(v.Length);
            for (int i = 0; i < v.Length; i++) r[i] = v[i] * c;
            return r;
        }
        public static Vec operator *(double c, Vec v) => v * c;
        public static Vec operator /(Vec v, double c)
        {
            if (c == 0) throw new DivideByZeroException();
            var r = new Vec(v.Length);
            for (int i = 0; i < v.Length; i++) r[i] = v[i] / c;
            return r;
        }

        public double Dot(Vec other)
        {
            if (Length != other.Length) throw new ArgumentException("Lengths must match.");
            double sum = 0;
            for (int i = 0; i < Length; i++) sum += this[i] * other[i];
            return sum;
        }
        public static double Dot(Vec a, Vec b) => a.Dot(b);

        public override string ToString()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < Length; i++)
            {
                sb.Append(this[i].ToString("G"));
                if (i < Length - 1) sb.Append(" ");
            }
            return sb.ToString();
        }

        public void Print(string prefix = "")
        {
            Console.Write(prefix);
            Console.WriteLine(ToString());
        }
    }
}
