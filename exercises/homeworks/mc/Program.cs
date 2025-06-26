public static class MonteCarlo
{
    private static readonly Random rand = new();

    public static (double estimatedIntegral, double errorEstimate) PlainMC(
        Func<double[], double> f, double[] a, double[] b, int N)
    {
        int dim = a.Length;
        double volume = 1.0;
        for (int i = 0; i < dim; i++) volume *= (b[i] - a[i]);

        double sum = 0, sum2 = 0;
        for (int i = 0; i < N; i++)
        {
            double[] x = new double[dim];
            for (int j = 0; j < dim; j++)
                x[j] = a[j] + rand.NextDouble() * (b[j] - a[j]);

            double fx = f(x);
            sum += fx;
            sum2 += fx * fx;
        }

        double mean = sum / N;
        double sigma = Math.Sqrt((sum2 / N - mean * mean) / N);
        return (mean * volume, sigma * volume);
    }

    public static (double estimatedIntegral, double errorEstimate) QMC(
        Func<double[], double> f, double[] a, double[] b, int N)
    {
        int dim = a.Length;
        double volume = 1.0;
        for (int i = 0; i < dim; i++) volume *= (b[i] - a[i]);

        double sum1 = 0.0, sum2 = 0.0;
        for (int i = 0; i < N; i++)
        {
            double[] x1 = new double[dim];
            double[] x2 = new double[dim];
            for (int d = 0; d < dim; d++)
            {
                x1[d] = a[d] + (b[d] - a[d]) * Halton(i + 1, Prime(d));
                x2[d] = a[d] + (b[d] - a[d]) * Halton(i + 1, Prime(d + 7)); // different primes
            }

            sum1 += f(x1);
            sum2 += f(x2);
        }

        double mean = (sum1 + sum2) / (2 * N);
        double sigma = Math.Abs(sum1 - sum2) / N / Math.Sqrt(2); // crude error estimate
        return (mean * volume, sigma * volume);
    }

    private static double Halton(int index, int b)
    {
        double f = 1.0, r = 0.0;
        while (index > 0)
        {
            f /= b;
            r += f * (index % b);
            index /= b;
        }
        return r;
    }

    private static int Prime(int n)
    {
        int[] primes = { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29 };
        return primes[n % primes.Length];
    }
}
