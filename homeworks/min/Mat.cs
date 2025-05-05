// Mat.cs
namespace MinimizerProject
{
    public class Mat
    {
        private double[,] data;
        public int Rows { get; }
        public int Cols { get; }

        public Mat(int rows, int cols)
        {
            if (rows <= 0 || cols <= 0)
                throw new ArgumentException("Invalid matrix dimensions.");
            Rows = rows;
            Cols = cols;
            data = new double[rows, cols];
        }

        public Mat(double[,] array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            Rows = array.GetLength(0);
            Cols = array.GetLength(1);
            data = (double[,])array.Clone();
        }

        public double this[int i, int j]
        {
            get => data[i, j];
            set => data[i, j] = value;
        }

        public Vec GetCol(int col)
        {
            var v = new Vec(Rows);
            for (int i = 0; i < Rows; i++)
                v[i] = data[i, col];
            return v;
        }

        public void SetCol(int col, Vec v)
        {
            for (int i = 0; i < Rows; i++)
                data[i, col] = v[i];
        }

        public Mat Transpose()
        {
            var t = new Mat(Cols, Rows);
            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Cols; j++)
                    t[j, i] = data[i, j];
            return t;
        }

        public static Vec operator *(Mat A, Vec v)
        {
            var r = new Vec(A.Rows);
            for (int i = 0; i < A.Rows; i++)
                for (int j = 0; j < A.Cols; j++)
                    r[i] += A[i, j] * v[j];
            return r;
        }

        public void QRDecomposition(out Mat Q, out Mat R)
        {
            Q = new Mat(Rows, Cols);
            R = new Mat(Cols, Cols);
            var A = new Mat((double[,])data.Clone());

            for (int j = 0; j < Cols; j++)
            {
                var v = A.GetCol(j);
                for (int i = 0; i < j; i++)
                {
                    var q = Q.GetCol(i);
                    R[i, j] = Vec.Dot(q, v);
                    v = v - q * R[i, j];
                }
                R[j, j] = v.Norm();
                Q.SetCol(j, v / R[j, j]);
            }
        }

        public Vec SolveQR(Vec b)
        {
            QRDecomposition(out var Q, out var R);
            var y = Q.Transpose() * b;
            var x = new Vec(Cols);
            for (int i = Cols - 1; i >= 0; i--)
            {
                double sum = 0;
                for (int j = i + 1; j < Cols; j++)
                    sum += R[i, j] * x[j];
                x[i] = (y[i] - sum) / R[i, i];
            }
            return x;
        }
    }
}
