using System;

/// <summary>
/// Static class for Newton's method-based minimization, with numerical gradients and Hessians.
/// </summary>
public static class Newton
{
    /// <summary>
    /// Minimizes a function f starting from point x, using Newton's method with optional accuracy and step limits.
    /// </summary>
    /// <param name="f">Function to minimize</param>
    /// <param name="x">Initial point (will be modified)</param>
    /// <param name="acc">Accuracy for gradient norm</param>
    /// <param name="maxSteps">Maximum number of steps</param>
    /// <returns>Tuple: minimum point, number of steps</returns>
    public static (vector, int) Minimize(Func<vector, double> f, vector x, double acc = 1e-3, int maxSteps = 1000)
    {
        int steps = 0;
        while (steps < maxSteps)
        {
            var g = Gradient(f, x);
            Console.Error.WriteLine($"step {steps}, |grad| = {g.norm():F4}, f = {f(x):F4}");
            if (g.norm() < acc) break;

            var H = Hessian(f, x);

            vector dx;
            try
            {
                var qr = new QRdecomp(H);
                dx = qr.solve(-g);
            }
            catch
            {
                dx = -g; // fallback to steepest descent direction
            }

            // Optionally limit step size:
            // if (dx.norm() > 10.0) dx = dx * (10.0 / dx.norm());

            double lambda = 1;
            double fx = f(x);
            vector xNew = null;

            // Backtracking line search (Armijo condition)
            while (lambda >= 1.0 / 1024)
            {
                xNew = x + lambda * dx;
                if (f(xNew) < fx + 1e-3 * lambda * g.dot(dx)) break;
                lambda /= 2;
            }

            x = xNew;
            steps++;
        }
        return (x, steps);
    }

    /// <summary>
    /// Computes the numerical gradient using forward difference.
    /// </summary>
    public static vector Gradient(Func<vector, double> f, vector x)
    {
        var grad = new vector(x.size);
        double fx = f(x);
        for (int i = 0; i < x.size; i++)
        {
            double dx = (1 + Math.Abs(x[i])) * Math.Pow(2, -26);
            x[i] += dx;
            grad[i] = (f(x) - fx) / dx;
            x[i] -= dx;
        }
        return grad;
    }

    /// <summary>
    /// Computes the numerical Hessian using forward difference.
    /// </summary>
    public static matrix Hessian(Func<vector, double> f, vector x)
    {
        int n = x.size;
        var H = new matrix(n, n);
        var g0 = Gradient(f, x);
        for (int j = 0; j < n; j++)
        {
            double dx = (1 + Math.Abs(x[j])) * Math.Pow(2, -13);
            x[j] += dx;
            var dg = Gradient(f, x) - g0;
            for (int i = 0; i < n; i++) H[i, j] = dg[i] / dx;
            x[j] -= dx;
        }
        return H;
    }

    // ======================== TASK C: Central differences =========================

    /// <summary>
    /// Computes the numerical gradient using central difference.
    /// </summary>
    public static vector GradientCentral(Func<vector, double> f, vector x)
    {
        var grad = new vector(x.size);
        for (int i = 0; i < x.size; i++)
        {
            double dx = (1 + Math.Abs(x[i])) * Math.Pow(2, -26);
            double xi = x[i];
            x[i] = xi + dx;
            double fxPlus = f(x);
            x[i] = xi - dx;
            double fxMinus = f(x);
            x[i] = xi;
            grad[i] = (fxPlus - fxMinus) / (2 * dx);
        }
        return grad;
    }

    /// <summary>
    /// Computes a diagonal Hessian using central difference.
    /// </summary>
    public static matrix HessianCentral(Func<vector, double> f, vector x)
    {
        int n = x.size;
        var H = new matrix(n, n);
        var fx = f(x);
        for (int j = 0; j < n; j++)
        {
            double dx = (1 + Math.Abs(x[j])) * Math.Pow(2, -13);
            double xj = x[j];

            x[j] = xj + dx;
            double fPlus = f(x);
            x[j] = xj - dx;
            double fMinus = f(x);
            x[j] = xj;
            double f0 = fx;

            for (int i = 0; i < n; i++) H[i, j] = 0;
            H[j, j] = (fPlus - 2 * f0 + fMinus) / (dx * dx);
        }
        return H;
    }

    // ====================== End of central differences for TASK C =================

    /// <summary>
    /// QR decomposition and solver for linear systems.
    /// </summary>
    public class QRdecomp
    {
        private matrix Q, R;

        /// <summary>
        /// Constructs the QR decomposition of matrix A.
        /// </summary>
        public QRdecomp(matrix A)
        {
            int n = A.size1, m = A.size2;
            Q = new matrix(n, m);
            R = new matrix(m, m);
            for (int j = 0; j < m; j++)
            {
                vector v = A.column(j);
                for (int i = 0; i < j; i++)
                {
                    R[i, j] = Q.column(i).dot(v);
                    v -= Q.column(i) * R[i, j];
                }
                R[j, j] = v.norm();
                Q.set_col(j, v / R[j, j]);
            }
        }

        /// <summary>
        /// Solves QRx = b for x.
        /// </summary>
        public vector solve(vector b)
        {
            vector y = Q.transpose() * b;
            int n = R.size2;
            vector x = new vector(n);
            for (int i = n - 1; i >= 0; i--)
            {
                double s = y[i];
                for (int k = i + 1; k < n; k++)
                    s -= R[i, k] * x[k];
                x[i] = s / R[i, i];
            }
            return x;
        }
    }
}
