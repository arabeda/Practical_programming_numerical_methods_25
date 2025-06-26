using System;
using System.IO;
using System.Globalization;

public partial class Program
{
    static void Main()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        // Signal parameters
        int n = 100;
        double[] t = new double[n];
        double[] clean = new double[n];
        double[] noisy = new double[n];

        // Generate a clean signal (sum of sine and cosine) and add random noise
        Random rand = new Random(42);
        for (int i = 0; i < n; i++)
        {
            t[i] = i * 1.0 / (n - 1);
            clean[i] = 2.0 * Math.Sin(2 * Math.PI * t[i]) + Math.Cos(4 * Math.PI * t[i]);
            noisy[i] = clean[i] + rand.NextDouble() * 1.5 - 0.75; // noise in [-0.75, +0.75]
        }

        // Try different lambda values for smoothing
        double[] lambdas = { 0.01, 0.1, 1, 10, 100 };
        for (int li = 0; li < lambdas.Length; li++)
        {
            double lambda = lambdas[li];
            double[] smoothed = LeastSquaresSmoothing.Smooth(noisy, lambda);
            string filename = $"output_lambda_{lambda:0.00}.txt";
            using (var sw = new StreamWriter(filename))
            {
                for (int i = 0; i < n; i++)
                {
                    // time, clean, noisy, smoothed
                    sw.WriteLine($"{t[i]}\t{clean[i]}\t{noisy[i]}\t{smoothed[i]}");
                }
            }
            Console.WriteLine($"Saved to {filename}");
        }
        Console.WriteLine("Ready to plot with Gnuplot!");
    }
}

// --- Least-squares signal smoothing implementation ---
class LeastSquaresSmoothing
{
    public static double[] Smooth(double[] y, double lambda)
    {
        int n = y.Length;
        double[,] D = BuildSecondDifferenceMatrix(n);
        double[,] DTD = MultiplyTranspose(D);
        double[,] I = IdentityMatrix(n);

        // Compute (I + lambda * D^T D)
        double[,] A = new double[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                A[i, j] = I[i, j] + lambda * DTD[i, j];

        // Solve the linear system A x = y
        double[] x = SolveLinearSystem(A, y);
        return x;
    }

    // Builds the second-difference matrix (n-2 rows, n columns)
    private static double[,] BuildSecondDifferenceMatrix(int n)
    {
        int rows = n - 2;
        double[,] D = new double[rows, n];
        for (int i = 0; i < rows; i++)
        {
            D[i, i] = 1;
            D[i, i + 1] = -2;
            D[i, i + 2] = 1;
        }
        return D;
    }

    // Calculates D^T * D (returns n x n square matrix)
    private static double[,] MultiplyTranspose(double[,] D)
    {
        int rows = D.GetLength(0);
        int n = D.GetLength(1);
        double[,] DTD = new double[n, n];
        for (int i = 0; i < n; i++)
        for (int j = 0; j < n; j++)
            for (int k = 0; k < rows; k++)
                DTD[i, j] += D[k, i] * D[k, j];
        return DTD;
    }

    // Returns identity matrix (n x n)
    private static double[,] IdentityMatrix(int n)
    {
        double[,] I = new double[n, n];
        for (int i = 0; i < n; i++)
            I[i, i] = 1.0;
        return I;
    }

    // Solves Ax = b using Gaussian elimination (for small n, this is fine)
    private static double[] SolveLinearSystem(double[,] A, double[] b)
    {
        int n = b.Length;
        double[,] M = (double[,])A.Clone();
        double[] x = (double[])b.Clone();

        // Gaussian elimination with partial pivoting
        for (int k = 0; k < n; k++)
        {
            // Pivot
            double max = Math.Abs(M[k, k]);
            int maxRow = k;
            for (int i = k + 1; i < n; i++)
            {
                if (Math.Abs(M[i, k]) > max)
                {
                    max = Math.Abs(M[i, k]);
                    maxRow = i;
                }
            }
            // Swap rows
            for (int j = 0; j < n; j++)
            {
                double tmp = M[maxRow, j];
                M[maxRow, j] = M[k, j];
                M[k, j] = tmp;
            }
            double tmp2 = x[maxRow];
            x[maxRow] = x[k];
            x[k] = tmp2;

            // Elimination
            for (int i = k + 1; i < n; i++)
            {
                double f = M[i, k] / M[k, k];
                for (int j = k; j < n; j++)
                    M[i, j] -= f * M[k, j];
                x[i] -= f * x[k];
            }
        }
        // Back substitution
        double[] sol = new double[n];
        for (int i = n - 1; i >= 0; i--)
        {
            sol[i] = x[i];
            for (int j = i + 1; j < n; j++)
                sol[i] -= M[i, j] * sol[j];
            sol[i] /= M[i, i];
        }
        return sol;
    }
}
