using System;
using static System.Math;
using static System.Console;
using System.IO;
using System.Globalization;

public static class MC
{
    public static (double result, double error) PlainIntegrate(Func<double[], double> f, double[] a, double[] b, int N)
    {
        int dim = a.Length;
        double volume = 1.0;
        for (int i = 0; i < dim; i++)
            volume *= b[i] - a[i];

        double sum = 0.0, sumSq = 0.0;
        var x = new double[dim];
        var rnd = new Random();

        for (int i = 0; i < N; i++)
        {
            for (int k = 0; k < dim; k++)
                x[k] = a[k] + rnd.NextDouble() * (b[k] - a[k]);
            double fx = f(x);
            sum += fx;
            sumSq += fx * fx;
        }
        double mean = sum / N;
        double sigma = Sqrt(sumSq / N - mean * mean);

        return (mean * volume, sigma * volume / Sqrt(N));
    }

    public static double Corput(int n, int b = 2)
    {
        double q = 0, bk = 1.0 / b;
        while (n > 0)
        {
            q += (n % b) * bk;
            n /= b;
            bk /= b;
        }
        return q;
    }

    public static readonly int[] Primes1 = { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37 };
    public static readonly int[] Primes2 = { 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89 };

    public static void Halton(int n, int dim, double[] k, int[] primes, double[] a, double[] b)
    {
        for (int i = 0; i < dim; i++)
            k[i] = a[i] + Corput(n, primes[i]) * (b[i] - a[i]);
    }

    public static (double result, double error) QuasiIntegrate(Func<double[], double> f, double[] a, double[] b, int N)
    {
        int dim = a.Length;
        double volume = 1.0;
        for (int i = 0; i < dim; i++)
            volume *= b[i] - a[i];

        double seq1 = 0.0, seq2 = 0.0;
        var k = new double[dim];

        for (int i = 0; i < N; i++)
        {
            Halton(i, dim, k, Primes1, a, b);
            seq1 += f(k);

            Halton(i, dim, k, Primes2, a, b);
            seq2 += f(k);
        }

        double result = seq1 / N * volume;
        double error = Abs(seq1 - seq2) / N * volume;

        return (result, error);
    }
}

class Program
{
    static double Norm(double[] x)
    {
        double sum = 0;
        foreach (var xi in x) sum += xi * xi;
        return Sqrt(sum);
    }

    static void Main()
    {
        Func<double[], double> f = x => Norm(x) <= 1 ? 1 : 0;
        double[] a = { -1, -1 };
        double[] b = { 1, 1 };
        int N = 100000;

        (double result, double error) = MC.PlainIntegrate(f, a, b, N);
        WriteLine($"Unit circle area (plain MC): {result} ± {error}");

        (double result2, double error2) = MC.QuasiIntegrate(f, a, b, N);
        WriteLine($"Unit circle area (quasi MC): {result2} ± {error2}");

        Func<double[], double> g = k => Pow(PI, -3) * Pow(1 - Cos(k[0]) * Cos(k[1]) * Cos(k[2]), -1);
        double[] a2 = { 0, 0, 0 };
        double[] b2 = { PI, PI, PI };

        (double result3, double error3) = MC.PlainIntegrate(g, a2, b2, N);
        WriteLine($"Singular integral (plain MC): {result3} ± {error3}");

        (double result4, double error4) = MC.QuasiIntegrate(g, a2, b2, N);
        WriteLine($"Singular integral (quasi MC): {result4} ± {error4}");

        double exact = Math.PI;
        int[] Ns = {100, 200, 500, 1000, 2000, 5000, 10000, 20000};

        using(var estimatedFile = new StreamWriter("mc_estimated.txt"))
        using(var actualFile = new StreamWriter("mc_actual.txt"))
        using(var quasiFile = new StreamWriter("mc_quasi.txt"))
        {
            foreach(var n in Ns)
            {
                (double res, double err) = MC.PlainIntegrate(f, a, b, n);
                estimatedFile.WriteLine($"{n} {err.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
                actualFile.WriteLine($"{n} {Math.Abs(exact - res).ToString(System.Globalization.CultureInfo.InvariantCulture)}");

                (double qRes, double qErr) = MC.QuasiIntegrate(f, a, b, n);
                quasiFile.WriteLine($"{n} {qErr.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
            }
        }
        WriteLine("Results written as mc_estimated.txt, mc_actual.txt, mc_quasi.txt");
    }
}
