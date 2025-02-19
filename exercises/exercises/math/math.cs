using System; 

public class GammaCalculator
{
    static double Gamma(double x)
    {
        if (x<0)
    {
            return Math.PI / (Math.Sin(Math.PI * x) * Gamma(1-x));
    }

        if (x<9)
    {
            return Gamma(x+1) / x;
    }

    double lnGamma = x * Math.Log(x + 1 / (12 * x -1 / (10 * x))) - x + Math.Log(2 * Math.PI / x) / 2 ;
    return Math.Exp(lnGamma);

    }   

public static void RunTest()
    {
        Console.Write("Please input x:");
        double x = Convert.ToDouble(Console.ReadLine());

        double solution = Gamma(x); 
        Console.WriteLine($"Gamma({x}) = {solution}"); 
    }

    public static int Main()
    {
        RunTest();
        return 0;
    }

}