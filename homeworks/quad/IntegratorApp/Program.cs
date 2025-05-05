using System;
using System.Collections.Generic;
using System.IO;

class Program
{
    static void Main()
    {
        // Testy podstawowych całek
        TestIntegral(x => Math.Sqrt(x), 0, 1, "∫₀¹ √x dx ≈ 2/3", 2.0 / 3);
        TestIntegral(x => 1 / Math.Sqrt(x), 0, 1, "∫₀¹ 1/√x dx ≈ 2", 2.0);
        TestIntegral(x => Math.Sqrt(1 - x * x), 0, 1, "∫₀¹ √(1-x²) dx ≈ π/2", Math.PI / 2);
        TestIntegral(x => Math.Log(x) / Math.Sqrt(x), 0, 1, "∫₀¹ ln(x)/√x dx ≈ -4", -4);

        // Test funkcji erf
        Console.WriteLine($"\nerf(1) = {Erf(1)} (oczekiwane ≈ 0.84270079294971486934)");

        // Porównanie błędu dla erf(1)
        ErfAccuracyPlot();

        // Test integracji z Clenshaw–Curtis
        Console.WriteLine("\nClenshaw–Curtis test:");
        TestCC(x => 1 / Math.Sqrt(x), 0, 1);
        TestCC(x => Math.Log(x) / Math.Sqrt(x), 0, 1);
    }

    static void TestIntegral(Func<double, double> f, double a, double b, string label, double expected)
    {
        var (result, error, evals) = AdaptiveIntegrator.Integrate(f, a, b, 1e-6, 1e-6);
        Console.WriteLine($"{label}: {result} ± {error}, evals: {evals}, Δ = {Math.Abs(result - expected)}");
    }

    static void TestCC(Func<double, double> f, double a, double b)
    {
        int calls = 0;
        Func<double, double> countedF = x => { calls++; return f(x); };
        var (result, error, evals) = AdaptiveIntegrator.IntegrateCC(countedF, a, b, 1e-6, 1e-6);
        Console.WriteLine($"Clenshaw–Curtis: result = {result}, evals = {calls}, error = {error}");
    }

    static void ErfAccuracyPlot()
    {
        double exact = 0.84270079294971486934;
        var accs = new List<double>();
        var errors = new List<double>();

        for (double acc = 1e-1; acc >= 1e-8; acc /= 10)
        {
            var (res, _, _) = AdaptiveIntegrator.Integrate(x => Math.Exp(-x * x), 0, 1, acc, 0);
            double erf = 2 / Math.Sqrt(Math.PI) * res;
            accs.Add(acc);
            errors.Add(Math.Abs(erf - exact));
            Console.WriteLine($"acc={acc}, error={Math.Abs(erf - exact)}");
        }

        File.WriteAllLines("erf_errors.csv", new[] { "acc,error" }.Concat(accs.Zip(errors, (a, e) => $"{a},{e}")));
    }

    public static double Erf(double z)
    {
        if (z < 0) return -Erf(-z);
        if (z <= 1)
        {
            Func<double, double> f = x => Math.Exp(-x * x);
            var (val, _, _) = AdaptiveIntegrator.Integrate(f, 0, z, 1e-6, 0);
            return 2 / Math.Sqrt(Math.PI) * val;
        }
        else
        {
            Func<double, double> f = t =>
            {
                double x = z + (1 - t) / t;
                return Math.Exp(-x * x) / (t * t);
            };
            var (val, _, _) = AdaptiveIntegrator.Integrate(f, 0, 1, 1e-6, 0);
            return 1 - 2 / Math.Sqrt(Math.PI) * val;
        }
    }
}

class AdaptiveIntegrator
{
    public static (double result, double error, int evals) Integrate(
        Func<double, double> f, double a, double b,
        double acc = 0.001, double eps = 0.001,
        double? f2 = null, double? f3 = null,
        int calls = 0)
    {
        if (double.IsInfinity(a) || double.IsInfinity(b))
            return IntegrateInfinite(f, a, b, acc, eps);

        double h = b - a;
        double x1 = a + h / 6;
        double x2 = a + 2 * h / 6;
        double x3 = a + 4 * h / 6;
        double x4 = a + 5 * h / 6;

        double f1 = f(x1);
        double F2 = f2 ?? f(x2);
        double F3 = f3 ?? f(x3);
        double f4 = f(x4);

        calls += (f2 == null ? 1 : 0) + (f3 == null ? 1 : 0) + 2;

        double Q = (2 * f1 + F2 + F3 + 2 * f4) / 6 * h;
        double q = (f1 + F2 + F3 + f4) / 4 * h;
        double err = Math.Abs(Q - q);
        double tol = acc + eps * Math.Abs(Q);

        if (err <= tol)
        {
            return (Q, err, calls);
        }
        else
        {
            var left = Integrate(f, a, (a + b) / 2, acc / Math.Sqrt(2), eps, f1, F2);
            var right = Integrate(f, (a + b) / 2, b, acc / Math.Sqrt(2), eps, F3, f4);
            double total = left.result + right.result;
            double totalError = Math.Sqrt(left.error * left.error + right.error * right.error);
            return (total, totalError, calls + left.evals + right.evals);
        }
    }

    public static (double result, double error, int evals) IntegrateCC(
        Func<double, double> f, double a, double b,
        double acc = 0.001, double eps = 0.001)
    {
        Func<double, double> g = theta =>
        {
            double x = (a + b) / 2 + (b - a) / 2 * Math.Cos(theta);
            double dx = (b - a) / 2 * Math.Sin(theta);
            return f(x) * dx;
        };
        return Integrate(g, 0, Math.PI, acc, eps);
    }

    public static (double result, double error, int evals) IntegrateInfinite(
        Func<double, double> f, double a, double b,
        double acc = 0.001, double eps = 0.001)
    {
        if (double.IsNegativeInfinity(a) && double.IsPositiveInfinity(b))
        {
            Func<double, double> g = t =>
            {
                double x = t / (1 - t * t);
                double dx = (1 + t * t) / Math.Pow(1 - t * t, 2);
                return f(x) + f(-x) * dx;
            };
            return Integrate(g, -1 + 1e-10, 1 - 1e-10, acc, eps);
        }
        else if (double.IsNegativeInfinity(a))
        {
            Func<double, double> g = t =>
            {
                double x = b - (1 - t) / t;
                return f(x) / (t * t);
            };
            return Integrate(g, 0, 1, acc, eps);
        }
        else if (double.IsPositiveInfinity(b))
        {
            Func<double, double> g = t =>
            {
                double x = a + (1 - t) / t;
                return f(x) / (t * t);
            };
            return Integrate(g, 0, 1, acc, eps);
        }
        throw new ArgumentException("Invalid infinite bounds");
    }
}
