using System;
using System.Collections.Generic;
using System.IO;

public class Program
{
    // Rosenbrock function: commonly used to test optimization algorithms
    public static double Rosenbrock(Vec v)
    {
        double x = v[0];
        double y = v[1];
        return Math.Pow(1 - x, 2) + 100 * Math.Pow(y - x * x, 2);
    }

    // Himmelblau function: has multiple minima, useful for testing
    public static double Himmelblau(Vec v)
    {
        double x = v[0];
        double y = v[1];
        return Math.Pow(x * x + y - 11, 2) + Math.Pow(x + y * y - 7, 2);
    }

    // Breit-Wigner distribution: used to model resonance peaks in physics
    public static double BreitWigner(double energy, double mass, double width, double amplitude)
    {
        return amplitude / (Math.Pow(energy - mass, 2) + Math.Pow(width, 2) / 4.0);
    }

    // Calculates how far the model is from the experimental data (least-squares)
    public static double Deviation(Vec parameters, double[] energies, double[] signals, double[] uncertainties)
    {
        double mass = parameters[0];
        double width = parameters[1];
        double amplitude = parameters[2];

        double total = 0.0;
        for (int i = 0; i < energies.Length; i++)
        {
            double modelValue = BreitWigner(energies[i], mass, width, amplitude);
            double residual = modelValue - signals[i];
            total += Math.Pow(residual / uncertainties[i], 2);
        }

        return total;
    }

    public static void Main(string[] args)
    {
        Vec x0;
        Vec result;

        // Try to find minimum of Rosenbrock function using forward differences
        x0 = new Vec(new double[] { -1, 1 });
        result = NewtonMinimizer.Minimize(Rosenbrock, x0, 1e-3, 100, useCentralDifference: false);
        Console.WriteLine("Rosenbrock minimum:");
        Console.WriteLine($"  x = {result[0]}");
        Console.WriteLine($"  y = {result[1]}");
        Console.WriteLine($"  Iterations: {NewtonMinimizer.IterationCount}\n");

        // Same Rosenbrock test but with central differences for better accuracy
        x0 = new Vec(new double[] { -1, 1 });
        result = NewtonMinimizer.Minimize(Rosenbrock, x0, 1e-3, 100, useCentralDifference: true);
        Console.WriteLine("Rosenbrock minimum (central differences):");
        Console.WriteLine($"  x = {result[0]}");
        Console.WriteLine($"  y = {result[1]}");
        Console.WriteLine($"  Iterations: {NewtonMinimizer.IterationCount}\n");

        // Try minimizing Himmelblau function using forward differences
        x0 = new Vec(new double[] { 2, 2 });
        result = NewtonMinimizer.Minimize(Himmelblau, x0, 1e-3, 100, useCentralDifference: false);
        Console.WriteLine("Himmelblau minimum:");
        Console.WriteLine($"  x = {result[0]}");
        Console.WriteLine($"  y = {result[1]}");
        Console.WriteLine($"  Iterations: {NewtonMinimizer.IterationCount}\n");

        // Repeat Himmelblau with central differences
        x0 = new Vec(new double[] { 2, 2 });
        result = NewtonMinimizer.Minimize(Himmelblau, x0, 1e-3, 100, useCentralDifference: true);
        Console.WriteLine("Himmelblau minimum (central differences):");
        Console.WriteLine($"  x = {result[0]}");
        Console.WriteLine($"  y = {result[1]}");
        Console.WriteLine($"  Iterations: {NewtonMinimizer.IterationCount}\n");

        // Load experimental data from a file (energy, signal, uncertainty)
        const string dataFile = "higgs.data.txt";
        if (!File.Exists(dataFile))
        {
            Console.WriteLine($"Data file \"{dataFile}\" not found.");
            return;
        }

        var energyList = new List<double>();
        var signalList = new List<double>();
        var errorList = new List<double>();

        foreach (var line in File.ReadAllLines(dataFile))
        {
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                continue;

            var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
                continue;

            energyList.Add(double.Parse(parts[0]));
            signalList.Add(double.Parse(parts[1]));
            errorList.Add(double.Parse(parts[2]));
        }

        double[] energies = energyList.ToArray();
        double[] signals = signalList.ToArray();
        double[] errors = errorList.ToArray();

        // Wrap deviation function to use in optimizer
        Func<Vec, double> deviationFunction = p => Deviation(p, energies, signals, errors);

        // Initial guess: [mass ~125, width ~6, amplitude ~43]
        Vec initialGuess = new Vec(new double[] { 125.0, 6.0, 43.0 });
        Vec fitResult = NewtonMinimizer.Minimize(deviationFunction, initialGuess, 1e-3, 50, useCentralDifference: false);

        double massFit = fitResult[0];
        double widthFit = fitResult[1];
        double amplitudeFit = fitResult[2];

        // Display fitted parameters
        Console.WriteLine("Best-fit parameters for Breit-Wigner:");
        Console.WriteLine($"  Mass m         = {massFit} GeV/c²");
        Console.WriteLine($"  Width Γ        = {widthFit} GeV/c²");
        Console.WriteLine($"  Scale factor A = {amplitudeFit}");
        Console.WriteLine($"  Iterations     = {NewtonMinimizer.IterationCount}");

        // Create a plot file: includes both the data and the fitted curve
        const string outputFile = "plot.dat";
        using (StreamWriter writer = new StreamWriter(outputFile))
        {
            writer.WriteLine("# Experimental data: Energy [GeV]   Signal   Uncertainty");
            for (int i = 0; i < energies.Length; i++)
            {
                writer.WriteLine($"{energies[i]} {signals[i]} {errors[i]}");
            }

            writer.WriteLine(); // blank line to separate datasets
            writer.WriteLine("# Fitted curve: Energy [GeV]   Fitted Signal");

            double E_min = energies[0];
            double E_max = energies[energies.Length - 1];
            int numPoints = 200;
            double step = (E_max - E_min) / (numPoints - 1);

            for (int i = 0; i < numPoints; i++)
            {
                double E = E_min + i * step;
                double value = BreitWigner(E, massFit, widthFit, amplitudeFit);
                writer.WriteLine($"{E} {value}");
            }
        }

        Console.WriteLine($"Plot data written to {outputFile}");
    }
}
