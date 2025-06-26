using System;
using System.Collections.Generic;
using System.IO;
using MinimizerProject;

namespace MinimizerProject
{
    public class Program
    {
        // Rosenbrock function: f(x, y) = (1 - x)^2 + 100 * (y - x^2)^2
        public static double Rosenbrock(Vec v)
        {
            double x = v[0], y = v[1];
            return Math.Pow(1 - x, 2) + 100 * Math.Pow(y - x * x, 2);
        }

        // Himmelblau function: f(x, y) = (x^2 + y - 11)^2 + (x + y^2 - 7)^2
        public static double Himmelblau(Vec v)
        {
            double x = v[0], y = v[1];
            return Math.Pow(x * x + y - 11, 2) + Math.Pow(x + y * y - 7, 2);
        }

        // Breit-Wigner function
        public static double BreitWigner(double E, double m, double Gamma, double A)
        {
            return A / (Math.Pow(E - m, 2) + Gamma * Gamma / 4.0);
        }

        // Deviation function for fit quality
        public static double Deviation(Vec p, double[] energy, double[] signal, double[] error)
        {
            double m = p[0], Gamma = p[1], A = p[2];
            double sum = 0;
            for (int i = 0; i < energy.Length; i++)
            {
                double diff = BreitWigner(energy[i], m, Gamma, A) - signal[i];
                sum += Math.Pow(diff / error[i], 2);
            }
            return sum;
        }

        public static void Main()
        {
            // --- Test Rosenbrock forward ---
            var x0 = new Vec(new double[] { -1, 1 });
            var solRosen = NewtonMinimizer.Minimize(Rosenbrock, x0, 1e-3, 100, false);
            Console.WriteLine("Rosenbrock minimum (forward):");
            solRosen.Print("  ");

            // --- Test Rosenbrock central ---
            x0 = new Vec(new double[] { -1, 1 });
            solRosen = NewtonMinimizer.Minimize(Rosenbrock, x0, 1e-3, 100, true);
            Console.WriteLine("Rosenbrock minimum (central):");
            solRosen.Print("  ");

            // --- Test Himmelblau ---
            x0 = new Vec(new double[] { 2, 2 });
            var solHimmel = NewtonMinimizer.Minimize(Himmelblau, x0, 1e-3, 100, false);
            Console.WriteLine("Himmelblau minimum:");
            solHimmel.Print("  ");

            // --- Load experimental data ---
            string file = "higgs.data.txt";
            if (!File.Exists(file))
            {
                Console.WriteLine("Missing data file: " + file);
                return;
            }

            var energyList = new List<double>();
            var signalList = new List<double>();
            var errorList = new List<double>();

            foreach (var line in File.ReadAllLines(file))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) continue;
                energyList.Add(double.Parse(parts[0]));
                signalList.Add(double.Parse(parts[1]));
                errorList.Add(double.Parse(parts[2]));
            }

            var energy = energyList.ToArray();
            var signal = signalList.ToArray();
            var error = errorList.ToArray();

            Func<Vec, double> deviation = p => Deviation(p, energy, signal, error);
            Vec initialGuess = new Vec(new double[] { 125.0, 6.0, 43.0 });

            var bestFit = NewtonMinimizer.Minimize(deviation, initialGuess, 1e-3, 50, false);
            Console.WriteLine("Breit-Wigner fit:");
            Console.WriteLine($"  Mass   = {bestFit[0]:F4}");
            Console.WriteLine($"  Width  = {bestFit[1]:F4}");
            Console.WriteLine($"  Scale  = {bestFit[2]:F4}");

            // --- Generate plot.dat ---
            using (var writer = new StreamWriter("plot.dat"))
            {
                for (int i = 0; i < energy.Length; i++)
                    writer.WriteLine($"{energy[i]} {signal[i]} {error[i]}");

                writer.WriteLine();
                double E_min = energy[0], E_max = energy[^1];
                int N = 200;
                double step = (E_max - E_min) / (N - 1);
                for (int i = 0; i < N; i++)
                {
                    double E = E_min + i * step;
                    double BW = BreitWigner(E, bestFit[0], bestFit[1], bestFit[2]);
                    writer.WriteLine($"{E} {BW}");
                }
            }

            Console.WriteLine("Saved output to plot.dat.");
        }
    }
}
