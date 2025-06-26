using static System.Math;
using System.IO;
using System;


public class Program
{
    public static (Vec, Vec) RKStep12(
        Func<double, Vec, Vec> f,
        double x,
        Vec y,
        double h)
    {
        Vec k0 = f(x, y);
        Vec k1 = f(x + h / 2, y + k0 * (h / 2));
        Vec yh = y + k1 * h;
        Vec dy = (k1 - k0) * h;
        return (yh, dy);
    }

    public static (genlist<double>, genlist<Vec>) Driver(
        Func<double, Vec, Vec> f,
        (double, double) interval,
        Vec yInit,
        double h = 0.125,
        double acc = 0.01,
        double eps = 0.01,
        double adj = 1.5,
        bool writeH = false)
    {
        var (a, b) = interval;
        double x = a;
        Vec y = yInit.Copy();

        var xList = new genlist<double>();
        var yList = new genlist<Vec>();
        xList.add(x);
        yList.add(y);

        double hMax = (b - a) / 500;

        do
        {
            if (x >= b) return (xList, yList);
            if (x + h > b) h = b - x;

            var (yh, dy) = RKStep12(f, x, y, h);
            double tol = (acc + eps * yh.Norm()) * Sqrt(h / (b - a));
            double err = dy.Norm();

            if (err <= tol)
            {
                x += h;
                y = yh;
                xList.add(x);
                yList.add(y);
            }

            if (err > 0)
            {
                double factor = Min(Pow(tol / err, 0.25) * 0.95, adj);
                h = Min(h * factor, hMax);
            }
            else
            {
                h = Min(h * 2, hMax);
            }

            if (writeH)
                Console.WriteLine($"{h}");

        } while (true);
    }

    public static int Main()
    {
        Func<double, Vec, Vec> F = (x, y) => new Vec(y[1], -y[0]);
        Vec yStart = new Vec(0, 1);
        var (xList, yList) = Driver(F, (0, 10.0), yStart);

        using (var writer = new StreamWriter("Test.dat"))
        {
            writer.WriteLine("# x_n y_n");
            for (int i = 0; i < xList.size(); i++)
                writer.WriteLine($"{xList[i]} {yList[i][0]}");
        }

        Func<double, Vec, Vec> Pend = (x, y) => new Vec(y[1], -0.25 * y[1] - 5.0 * Sin(y[0]));
        Vec pendY0 = new Vec(PI - 0.1, 0.0);
        var (pendXList, pendYList) = Driver(Pend, (0, 10.0), pendY0);

        using (var writer = new StreamWriter("Pendu.dat"))
        {
            writer.WriteLine("# x_n y_n");
            for (int i = 0; i < pendXList.size(); i++)
                writer.WriteLine($"{pendXList[i]} {pendYList[i][0]}");
        }

        double eps1 = 0, eps2 = 0, eps3 = 0.01;
        Vec yInit1 = new Vec(1, 0);
        Vec yInit2 = new Vec(1, -0.5);
        Vec yInit3 = new Vec(1, -0.5);

        Func<double, Vec, Vec> OrbitEps1 = (x, y) => new Vec(y[1], 1 - y[0] + eps1 * y[0] * y[0]);
        Func<double, Vec, Vec> OrbitEps2 = (x, y) => new Vec(y[1], 1 - y[0] + eps2 * y[0] * y[0]);
        Func<double, Vec, Vec> OrbitEps3 = (x, y) => new Vec(y[1], 1 - y[0] + eps3 * y[0] * y[0]);

        var (orbitEps1XList, orbitEps1YList) = Driver(OrbitEps1, (0, 10 * 2 * PI), yInit1, eps: 1e-6, h: 10 * 2 * PI / 500, acc: 1e-6, writeH: true);
        var (orbitEps2XList, orbitEps2YList) = Driver(OrbitEps2, (0, 10 * 2 * PI), yInit2);
        var (orbitEps3XList, orbitEps3YList) = Driver(OrbitEps3, (0, 10 * 2 * PI), yInit3);

        using (var writer = new StreamWriter("Orbit.dat"))
        {
            writer.WriteLine("# x1 y1 x2 y2 x3 y3");

            for (int i = 0; i < orbitEps1XList.size(); i++)
            {
                double x1t = orbitEps1XList[i];
                double y1t = orbitEps1YList[i][0];
                double x1r = y1t != 0 ? Cos(x1t) / y1t : double.NaN;
                double y1r = y1t != 0 ? Sin(x1t) / y1t : double.NaN;

                writer.WriteLine($"{x1r} {y1r} {double.NaN} {double.NaN} {double.NaN} {double.NaN}");
            }

            for (int i = 0; i < orbitEps2XList.size(); i++)
            {
                double x2t = orbitEps2XList[i];
                double y2t = orbitEps2YList[i][0];
                double x2r = y2t != 0 ? Cos(x2t) / y2t : double.NaN;
                double y2r = y2t != 0 ? Sin(x2t) / y2t : double.NaN;

                writer.WriteLine($"{double.NaN} {double.NaN} {x2r} {y2r} {double.NaN} {double.NaN}");
            }

            for (int i = 0; i < orbitEps3XList.size(); i++)
            {
                double x3t = orbitEps3XList[i];
                double y3t = orbitEps3YList[i][0];
                double x3r = y3t != 0 ? Cos(x3t) / y3t : double.NaN;
                double y3r = y3t != 0 ? Sin(x3t) / y3t : double.NaN;

                writer.WriteLine($"{double.NaN} {double.NaN} {double.NaN} {double.NaN} {x3r} {y3r}");
            }
        }

        return 0;
    }
}
