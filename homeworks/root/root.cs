// Assignment: Newton, Rosenbrock, Himmelblau, Hydrogen (shooting method) - 2024-06-26

using System;
using System.Linq;
using System.IO;

class vector {
    public double[] data;
    public int size { get { return data.Length; } }
    public vector(int n) { data = new double[n]; }
    public vector(params double[] d) { data = d; }
    public double this[int i] { get => data[i]; set => data[i]=value; }
    public vector copy() => new vector((double[])data.Clone());
    public double norm() => Math.Sqrt(data.Sum(x => x*x));
    public static vector operator +(vector a, vector b) => new vector(a.data.Zip(b.data, (x,y)=>x+y).ToArray());
    public static vector operator -(vector a, vector b) => new vector(a.data.Zip(b.data, (x,y)=>x-y).ToArray());
    public static vector operator *(double s, vector v) => new vector(v.data.Select(x=>s*x).ToArray());
    public static vector operator *(vector v, double s) => s * v;
    public static vector operator /(vector v, double s) => new vector(v.data.Select(x=>x/s).ToArray());
    public vector map(Func<double,double> f) => new vector(data.Select(f).ToArray());
    public double max() => data.Max(x => Math.Abs(x));
    public override string ToString() => $"({string.Join(", ", data.Select(d => d.ToString("F6")))} )";
}

class matrix {
    public double[,] data;
    public int n;
    public matrix(int n) { this.n = n; data = new double[n, n]; }
    public double this[int i, int j] { get => data[i, j]; set => data[i, j] = value; }
}

// Numerical Jacobian (finite differences)
static class NumMethods
{
    public static matrix jacobian(Func<vector, vector> f, vector x, vector fx = null, vector dx = null)
    {
        int n = x.size;
        if (dx == null)
            dx = x.map(xi => Math.Max(Math.Abs(xi), 1) * Math.Pow(2, -26));
        if (fx == null)
            fx = f(x);
        matrix J = new matrix(n);
        for (int j = 0; j < n; j++)
        {
            double saved = x[j];
            x[j] += dx[j];
            vector df = f(x) - fx;
            for (int i = 0; i < n; i++)
                J[i, j] = df[i] / dx[j];
            x[j] = saved;
        }
        return J;
    }

    // Simple 2x2 solver (Gauss); replace with any QR or your own if needed.
    public static vector solve2x2(matrix A, vector b)
    {
        double a = A[0, 0], b1 = A[0, 1], c = A[1, 0], d = A[1, 1];
        double det = a * d - b1 * c;
        if (Math.Abs(det) < 1e-14) throw new Exception("Singular matrix");
        double x0 = (d * b[0] - b1 * b[1]) / det;
        double x1 = (-c * b[0] + a * b[1]) / det;
        return new vector(x0, x1);
    }

    // Newton with simple backtracking line search
    public static vector newton(Func<vector, vector> f, vector start, double acc = 1e-6, vector dx = null)
    {
        vector x = start.copy();
        vector fx = f(x), z, fz;
        double λmin = 1.0 / 1024;
        int iter = 0;
        do
        {
            if (fx.norm() < acc) break;
            matrix J = jacobian(f, x, fx, dx);
            vector Dx = solve2x2(J, -1 * fx);
            double λ = 1.0;
            do
            {
                z = x + λ * Dx;
                fz = f(z);
                if (fz.norm() < (1 - λ / 2) * fx.norm()) break;
                if (λ < λmin) break;
                λ /= 2;
            } while (true);
            x = z; fx = fz;
            if (Dx.norm() < (dx == null ? 1e-10 : dx.max())) break;
            iter++;
            if (iter > 50) { Console.WriteLine("Warning: Newton exceeded 50 iterations!"); break; }
        } while (true);
        return x;
    }
}

class Program
{
    // Gradient of Rosenbrock function
    static vector rosenbrock_grad(vector v)
    {
        double x = v[0], y = v[1];
        return new vector(-2 * (1 - x) - 400 * x * (y - x * x), 200 * (y - x * x));
    }
    // Gradient of Himmelblau's function
    static vector himmelblau_grad(vector v)
    {
        double x = v[0], y = v[1];
        double dx = 4 * x * (x * x + y - 11) + 2 * (x + y * y - 7);
        double dy = 2 * (x * x + y - 11) + 4 * y * (x + y * y - 7);
        return new vector(dx, dy);
    }
    // Shooting method for hydrogen atom
    static double shoot(double E, double rmin, double rmax, double dr)
    {
        double f = rmin - rmin * rmin;
        double g = 1 - 2 * rmin;
        for (double r = rmin; r < rmax; r += dr)
        {
            // RK4 integrator
            double k1f = dr * g;
            double k1g = dr * (-2 * (E + 1 / r) * f);

            double k2f = dr * (g + 0.5 * k1g);
            double k2g = dr * (-2 * (E + 1 / (r + 0.5 * dr)) * (f + 0.5 * k1f));

            double k3f = dr * (g + 0.5 * k2g);
            double k3g = dr * (-2 * (E + 1 / (r + 0.5 * dr)) * (f + 0.5 * k2f));

            double k4f = dr * (g + k3g);
            double k4g = dr * (-2 * (E + 1 / (r + dr)) * (f + k3f));

            f += (k1f + 2 * k2f + 2 * k3f + k4f) / 6;
            g += (k1g + 2 * k2g + 2 * k3g + k4g) / 6;
        }
        return f; // f(rmax)
    }
    // Bisection root finder
    static double find_root(Func<double, double> M, double E1, double E2, double tol)
    {
        double f1 = M(E1), f2 = M(E2);
        if (f1 * f2 > 0) throw new Exception("Root not bracketed");
        int iter = 0;
        while (Math.Abs(E2 - E1) > tol)
        {
            double Em = 0.5 * (E1 + E2);
            double fm = M(Em);
            if (f1 * fm < 0) { E2 = Em; f2 = fm; }
            else { E1 = Em; f1 = fm; }
            iter++;
            if (iter > 100) { Console.WriteLine("Warning: Bisection exceeded 100 iterations!"); break; }
        }
        return 0.5 * (E1 + E2);
    }

    static void Main(string[] args)
    {
        Console.WriteLine("--- Newton: Rosenbrock minimum ---");
        vector start1 = new vector(1.2, 1.2);
        vector minR = NumMethods.newton(rosenbrock_grad, start1);
        Console.WriteLine($"Rosenbrock minimum: {minR}");

        Console.WriteLine("\n--- Newton: Himmelblau minimum ---");
        vector start2 = new vector(1.0, 1.0);
        vector minH = NumMethods.newton(himmelblau_grad, start2);
        Console.WriteLine($"Himmelblau minimum: {minH}");

        // Other minima for Himmelblau (the function has 4!)
        vector[] himStarts = {
            new vector(3, 2), new vector(-2.8, 3.1), new vector(-3.8, -3.3), new vector(3.6, -1.8)
        };
        for(int i=0; i<himStarts.Length; i++)
        {
            vector res = NumMethods.newton(himmelblau_grad, himStarts[i]);
            Console.WriteLine($"Himmelblau minimum from point {himStarts[i]}: {res}");
        }

        Console.WriteLine("\n--- Shooting method: hydrogen atom ground state ---");
        double rmin = 1e-3, rmax = 8, dr = 1e-2;
        Func<double, double> M = (E) => shoot(E, rmin, rmax, dr);
        double E0 = find_root(M, -1.0, -0.1, 1e-6);
        Console.WriteLine($"Numerical E0: {E0} (should be close to -0.5)");

        // Write data to file for plotting in gnuplot
        using (StreamWriter sw = new StreamWriter("hydrogen.dat"))
        {
            sw.WriteLine("# r   f_num   f_exact");
            for (double r = 0.1; r < 5.1; r += 0.05)
            {
                // RK4 integration for f_num
                double f = rmin - rmin * rmin;
                double g = 1 - 2 * rmin;
                double rr;
                for (rr = rmin; rr < r; rr += dr)
                {
                    double k1f = dr * g;
                    double k1g = dr * (-2 * (E0 + 1 / rr) * f);

                    double k2f = dr * (g + 0.5 * k1g);
                    double k2g = dr * (-2 * (E0 + 1 / (rr + 0.5 * dr)) * (f + 0.5 * k1f));

                    double k3f = dr * (g + 0.5 * k2g);
                    double k3g = dr * (-2 * (E0 + 1 / (rr + 0.5 * dr)) * (f + 0.5 * k2f));

                    double k4f = dr * (g + k3g);
                    double k4g = dr * (-2 * (E0 + 1 / (rr + dr)) * (f + k3f));

                    f += (k1f + 2 * k2f + 2 * k3f + k4f) / 6;
                    g += (k1g + 2 * k2g + 2 * k3g + k4g) / 6;
                }
                double f_exact = r * Math.Exp(-r);
                sw.WriteLine($"{r:F4}\t{f:F8}\t{f_exact:F8}");
            }
        }
        Console.WriteLine("Data written to hydrogen.dat - ready for gnuplot!");

        // How to plot in gnuplot
        Console.WriteLine("\nTo plot in gnuplot, type for example:");
        Console.WriteLine("plot 'hydrogen.dat' using 1:2 with lines title 'numerical', \\");
        Console.WriteLine("     'hydrogen.dat' using 1:3 with lines title 'exact'");
    }
}
