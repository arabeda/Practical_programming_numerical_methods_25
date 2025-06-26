using System;

class Program
{
    static bool Approx(double a, double b, double acc = 1e-9, double eps = 1e-9)
    {
        if (Math.Abs(a - b) <= acc) return true;
        if (Math.Abs(a - b) <= Math.Max(Math.Abs(a), Math.Abs(b)) * eps) return true;
        return false;
    }

    static void Main()
    {
        Console.WriteLine("The machine epsilon is the difference between 1.0 and the next representable floating point number.");
        
        // Machine epsilon for double
        double x = 1.0;
        while (1.0 + x != 1.0)
        {
            x /= 2.0;
        }
        x *= 2.0;

        Console.WriteLine("Machine epsilon for double: {0:E}", x);

        // Machine epsilon for float
        float y = 1.0f;
        while (1.0f + y != 1.0f)
        {
            y /= 2.0f;
        }
        y *= 2.0f;

        Console.WriteLine("Machine epsilon for float: {0:E}", y);

        // Porównanie z poprawnymi wartościami epsilona
        double d = Math.Pow(2, -52);
        double f = Math.Pow(2, -23);
        Console.WriteLine("Proper value for double should be: {0:E}", d);
        Console.WriteLine("Proper value for float should be: {0:E}", f);

        // Testowanie wpływu epsilona na operacje zmiennoprzecinkowe
        double epsilon = Math.Pow(2, -52);
        double tiny = epsilon / 2;
        double a = 1 + tiny + tiny;
        double b = tiny + tiny + 1;

        Console.WriteLine($"a == b ? {a == b}");
        Console.WriteLine($"a > 1  ? {a > 1}");
        Console.WriteLine($"b > 1  ? {b > 1}");

        // Testowanie funkcji Approx
        double d1 = 0.1 * 7;
        double d2 = 0.7;

        Console.WriteLine($"d1: {d1}, d2: {d2}");
        Console.WriteLine($"Using Approx function: d1 and d2 are approximately equal? {Approx(d1, d2)}");
    }
}
