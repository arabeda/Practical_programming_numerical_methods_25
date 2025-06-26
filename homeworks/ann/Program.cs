using System;

class ann
{
    public int n;
    public Func<double, double> f = x => x * Math.Exp(-x * x);
    public double[] p;

    public ann(int n)
    {
        this.n = n;
        p = new double[3 * n];
        Random rnd = new Random();
        for (int i = 0; i < n; i++)
        {
            p[3 * i] = -1.0 + 2.0 * i / (n - 1);
            p[3 * i + 1] = 0.1 + rnd.NextDouble() * 0.5;
            p[3 * i + 2] = rnd.NextDouble() * 0.1;
        }
    }

    public double response(double x)
    {
        double sum = 0;
        for (int i = 0; i < n; i++)
        {
            double a = p[3 * i];
            double b = p[3 * i + 1];
            double w = p[3 * i + 2];
            double arg = (x - a) / b;
            sum += f(arg) * w;
        }
        return sum;
    }

    double df(double x)
    {
        return Math.Exp(-x * x) * (1 - 2 * x * x);
    }

    double d2f(double x)
    {
        return Math.Exp(-x * x) * (4 * x * x * x - 6 * x);
    }

    double integralF(double x)
    {
        return -0.5 * Math.Exp(-x * x);
    }

    public double responseDerivative(double x)
    {
        double sum = 0;
        for (int i = 0; i < n; i++)
        {
            double a = p[3 * i];
            double b = p[3 * i + 1];
            double w = p[3 * i + 2];
            double arg = (x - a) / b;
            sum += df(arg) * (1.0 / b) * w;
        }
        return sum;
    }

    public double responseSecondDerivative(double x)
    {
        double sum = 0;
        for (int i = 0; i < n; i++)
        {
            double a = p[3 * i];
            double b = p[3 * i + 1];
            double w = p[3 * i + 2];
            double arg = (x - a) / b;
            sum += d2f(arg) * (1.0 / (b * b)) * w;
        }
        return sum;
    }

    public double responseIntegral(double x)
    {
        double sum = 0;
        for (int i = 0; i < n; i++)
        {
            double a = p[3 * i];
            double b = p[3 * i + 1];
            double w = p[3 * i + 2];
            double arg = (x - a) / b;
            sum += w * b * integralF(arg);
        }
        return sum;
    }

    double cost(double[] x, double[] y)
    {
        double c = 0;
        for (int k = 0; k < x.Length; k++)
        {
            double r = response(x[k]) - y[k];
            c += r * r;
        }
        return c;
    }

    public void train(double[] x, double[] y, int maxIter = 10000, double lr = 0.001)
    {
        for (int iter = 0; iter < maxIter; iter++)
        {
            double[] grad = gradient(x, y);
            for (int i = 0; i < p.Length; i++)
                p[i] -= lr * grad[i];

            if (iter % 1000 == 0)
                Console.WriteLine($"Iter {iter}, Cost = {cost(x, y):F6}");
        }
    }

    double[] gradient(double[] x, double[] y)
    {
        double[] grad = new double[p.Length];
        for (int k = 0; k < x.Length; k++)
        {
            double diff = response(x[k]) - y[k];
            for (int i = 0; i < n; i++)
            {
                double a = p[3 * i];
                double b = p[3 * i + 1];
                double w = p[3 * i + 2];
                double arg = (x[k] - a) / b;

                double fval = f(arg);
                double df_darg = df(arg);

                double dC_da = 2 * diff * df_darg * (-1.0 / b) * w;
                double dC_db = 2 * diff * df_darg * (-(x[k] - a) / (b * b)) * w;
                double dC_dw = 2 * diff * fval;

                grad[3 * i] += dC_da;
                grad[3 * i + 1] += dC_db;
                grad[3 * i + 2] += dC_dw;
            }
        }
        return grad;
    }
}

class Program
{
    static void Main()
    {
        int nNeurons = 10;
        ann net = new ann(nNeurons);

        int N = 50;
        double[] xs = new double[N];
        double[] ys = new double[N];

        // Target function g(x) = cos(5x - 1)*exp(-x^2)
        for (int i = 0; i < N; i++)
        {
            xs[i] = -1.0 + 2.0 * i / (N - 1);
            ys[i] = Math.Cos(5 * xs[i] - 1) * Math.Exp(-xs[i] * xs[i]);
        }

        net.train(xs, ys, maxIter: 10000, lr: 0.001);

        Console.WriteLine("\nTesting trained network at x=0.3");
        double xTest = 0.3;
        Console.WriteLine($"Fp({xTest}) = {net.response(xTest)}");
        Console.WriteLine($"Fp'({xTest}) = {net.responseDerivative(xTest)}");
        Console.WriteLine($"Fp''({xTest}) = {net.responseSecondDerivative(xTest)}");
        Console.WriteLine($"Integral Fp({xTest}) = {net.responseIntegral(xTest)}");
    }
}
