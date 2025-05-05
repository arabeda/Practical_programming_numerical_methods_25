using System;

namespace MinimizerProject
{
    public static class NewtonMinimizer
    {
        public static int IterationCount { get; private set; }

        public static Vec Minimize(Func<Vec, double> phi, Vec x0, double tol = 1e-3, int maxIter = 50, bool central = false)
        {
            IterationCount = 0;
            Vec x = x0.Copy();
            double phiVal = phi(x);

            while (IterationCount < maxIter)
            {
                Vec grad = central ? CentralGradient(phi, x) : Gradient(phi, x);
                if (grad.Norm() < tol)
                    break;

                Mat H = central ? CentralHessian(phi, x) : Hessian(phi, x);
                for (int i = 0; i < H.Rows; i++)
                    H[i, i] += 1e-8;

                Vec dx = H.SolveQR(-grad);

                double lambda = 1.0;
                while (lambda > 1.0 / 128.0)
                {
                    Vec trial = x + lambda * dx;
                    if (phi(trial) < phiVal)
                        break;
                    lambda /= 2.0;
                }

                x = x + lambda * dx;
                phiVal = phi(x);
                IterationCount++;
            }

            return x;
        }

        public static Vec Gradient(Func<Vec, double> phi, Vec x)
        {
            int n = x.Length;
            Vec grad = new Vec(n);
            double fx = phi(x);
            double dx, factor = Math.Pow(2, -26);

            for (int i = 0; i < n; i++)
            {
                dx = Math.Abs(x[i]) * factor;
                if (dx == 0) dx = factor;
                x[i] += dx;
                double fx_dx = phi(x);
                grad[i] = (fx_dx - fx) / dx;
                x[i] -= dx;
            }

            return grad;
        }

        public static Vec CentralGradient(Func<Vec, double> phi, Vec x)
        {
            int n = x.Length;
            Vec grad = new Vec(n);
            double factor = Math.Pow(2, -13);

            for (int i = 0; i < n; i++)
            {
                double orig = x[i];
                double dx = Math.Abs(orig) * factor;
                if (dx == 0) dx = factor;

                x[i] = orig + dx;
                double f_plus = phi(x);
                x[i] = orig - dx;
                double f_minus = phi(x);
                grad[i] = (f_plus - f_minus) / (2 * dx);
                x[i] = orig;
            }

            return grad;
        }

        public static Mat Hessian(Func<Vec, double> phi, Vec x)
        {
            int n = x.Length;
            Mat H = new Mat(n, n);
            Vec grad0 = Gradient(phi, x);
            double factor = Math.Pow(2, -13);

            for (int j = 0; j < n; j++)
            {
                double dx = Math.Abs(x[j]) * factor;
                if (dx == 0) dx = factor;

                x[j] += dx;
                Vec grad1 = Gradient(phi, x);
                Vec dgrad = grad1 - grad0;
                for (int i = 0; i < n; i++)
                    H[i, j] = dgrad[i] / dx;
                x[j] -= dx;
            }

            return H;
        }

        public static Mat CentralHessian(Func<Vec, double> phi, Vec x)
        {
            int n = x.Length;
            Mat H = new Mat(n, n);
            double factor = Math.Pow(2, -13);
            double f0 = phi(x);

            for (int i = 0; i < n; i++)
            {
                double orig = x[i];
                double dx = Math.Abs(orig) * factor;
                if (dx == 0) dx = factor;

                x[i] = orig + dx;
                double f_plus = phi(x);
                x[i] = orig - dx;
                double f_minus = phi(x);
                H[i, i] = (f_plus - 2 * f0 + f_minus) / (dx * dx);
                x[i] = orig;
            }

            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    double xi = x[i], xj = x[j];
                    double dxi = Math.Abs(xi) * factor;
                    double dxj = Math.Abs(xj) * factor;
                    if (dxi == 0) dxi = factor;
                    if (dxj == 0) dxj = factor;

                    x[i] = xi + dxi; x[j] = xj + dxj;
                    double fpp = phi(x);

                    x[i] = xi + dxi; x[j] = xj - dxj;
                    double fpm = phi(x);

                    x[i] = xi - dxi; x[j] = xj + dxj;
                    double fmp = phi(x);

                    x[i] = xi - dxi; x[j] = xj - dxj;
                    double fmm = phi(x);

                    double hij = (fpp - fpm - fmp + fmm) / (4 * dxi * dxj);
                    H[i, j] = hij;
                    H[j, i] = hij;

                    x[i] = xi; x[j] = xj;
                }
            }

            return H;
        }
    }
}
