using System;
using System.Linq;
using ScottPlot;

class LeastSquaresQR
{
    // QR Decomposition using Gram-Schmidt Process
    public static (double[,], double[,]) QRDecomposition(double[,] A)
    {
        int rows = A.GetLength(0);
        int cols = A.GetLength(1);
        double[,] Q = new double[rows, cols];
        double[,] R = new double[cols, cols];

        for (int k = 0; k < cols; k++)
        {
            for (int i = 0; i < rows; i++)
                Q[i, k] = A[i, k];

            for (int j = 0; j < k; j++)
            {
                double dot = 0;
                for (int i = 0; i < rows; i++)
                    dot += Q[i, j] * A[i, k];
                R[j, k] = dot;
                for (int i = 0; i < rows; i++)
                    Q[i, k] -= R[j, k] * Q[i, j];
            }

            double norm = 0;
            for (int i = 0; i < rows; i++)
                norm += Q[i, k] * Q[i, k];
            norm = Math.Sqrt(norm);

            R[k, k] = norm;
            for (int i = 0; i < rows; i++)
                Q[i, k] /= norm;
        }
        return (Q, R);
    }

    // Solve Rx = Qt * y
    public static double[] BackSubstitution(double[,] R, double[] QtY)
    {
        int n = R.GetLength(0);
        double[] x = new double[n];

        for (int i = n - 1; i >= 0; i--)
        {
            x[i] = QtY[i];
            for (int j = i + 1; j < n; j++)
                x[i] -= R[i, j] * x[j];
            x[i] /= R[i, i];
        }
        return x;
    }

    // Least squares fitting function
    public static double[] LSFit(Func<double, double>[] fs, double[] x, double[] y, double[] dy)
    {
        int n = x.Length;
        int m = fs.Length;
        double[,] A = new double[n, m];
        double[] b = new double[n];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
                A[i, j] = fs[j](x[i]) / dy[i];
            b[i] = y[i] / dy[i];
        }

        var (Q, R) = QRDecomposition(A);
        double[] QtY = new double[m];

        for (int i = 0; i < m; i++)
        {
            double sum = 0;
            for (int j = 0; j < n; j++)
                sum += Q[j, i] * b[j];
            QtY[i] = sum;
        }

        return BackSubstitution(R, QtY);
    }
}

class Program
{
    static void Main()
    {
        // 📌 Dane pomiarowe
        double[] t = { 1, 2, 3, 4, 6, 9, 10, 13, 15 };
        double[] y = { 117, 100, 88, 72, 53, 29.5, 25.2, 15.2, 11.1 };
        double[] dy = { 6, 5, 4, 4, 4, 3, 3, 2, 2 };

        // 📌 Logarytmizacja danych: ln(y) = ln(a) - λt
        double[] lnY = new double[y.Length];
        double[] dLnY = new double[dy.Length];
        for (int i = 0; i < y.Length; i++)
        {
            lnY[i] = Math.Log(y[i]);
            dLnY[i] = dy[i] / y[i];
        }

        // 📌 Dopasowanie modelu ln(y) = ln(a) - λt
        Func<double, double>[] basis = { x => 1, x => -x };
        double[] coefficients = LeastSquaresQR.LSFit(basis, t, lnY, dLnY);

        double lnA = coefficients[0];
        double lambda = coefficients[1];
        double T_half = Math.Log(2) / lambda;

        Console.WriteLine($"ln(a): {lnA}, lambda: {lambda}, T_half: {T_half} days");

        // 📌 Generowanie wykresu dopasowanej funkcji
        int N = 100;
        double[] t_fit = new double[N];
        double[] y_fit = new double[N];

        double minT = t.Min();
        double maxT = t.Max();
        double step = (maxT - minT) / (N - 1);

        for (int i = 0; i < N; i++)
        {
            t_fit[i] = minT + i * step;
            y_fit[i] = Math.Exp(lnA - lambda * t_fit[i]);
        }

               var plt = new Plot();
        double[] x = { 1, 2, 3, 4, 6, 9, 10, 13, 15 };

        plt.Add.Scatter(x, y);
        plt.SavePng("fit_plot.png", 600, 400);

        Console.WriteLine("Plot saved as fit_plot.png");
    }
}