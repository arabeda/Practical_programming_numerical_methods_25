using System;
using static System.Math;
using static System.Console;
using System.IO;

class SpecialFunctions {
    static double ComputeErf(double x) {
        if (x < 0) return -ComputeErf(-x);
        double[] coefficients = {0.254829592, -0.284496736, 1.421413741, -1.453152027, 1.061405429};
        double t = 1 / (1 + 0.3275911 * x);
        double sum = t * (coefficients[0] + t * (coefficients[1] + t * (coefficients[2] + t * (coefficients[3] + t * coefficients[4]))));
        return 1 - sum * Exp(-x * x);
    }

    static double ComputeGamma(double x) {
        if (x < 0) return PI / Sin(PI * x) / ComputeGamma(1 - x);
        if (x < 9) return ComputeGamma(x + 1) / x;
        double logApprox = Log(2 * PI) / 2 + (x - 0.5) * Log(x) - x
                         + (1.0 / 12) / x - (1.0 / 360) / Pow(x, 3) + (1.0 / 1260) / Pow(x, 5);
        return Exp(logApprox);
    }

    static double ComputeLnGamma(double x) {
        if (x <= 0) throw new ArgumentException("ComputeLnGamma: x<=0");
        if (x < 9) return ComputeLnGamma(x + 1) - Log(x);
        return x * Log(x + 1 / (12 * x - 1 / x / 10)) - x + Log(2 * PI / x) / 2;
    }

    static void Main() {
        using (var erfWriter = new StreamWriter("erf_results.txt"))
        using (var gammaWriter = new StreamWriter("gamma_results.txt"))
        using (var lngammaWriter = new StreamWriter("lngamma_results.txt")) {
            for (double x = -3; x <= 3; x += 1.0 / 64)
                erfWriter.WriteLine($"{x} {ComputeErf(x)}");
            for (double x = 0.1; x <= 10; x += 1.0 / 32)
                gammaWriter.WriteLine($"{x} {ComputeGamma(x)}");
            for (double x = 0.1; x <= 10; x += 1.0 / 32)
                lngammaWriter.WriteLine($"{x} {ComputeLnGamma(x)}");
        }
    }
}
