using System;
using System.IO;
using static System.Math;
using static System.Console;

class Program
{
    static int Main(string[] args)
    {
        string inputFile = null, outputFile = null;
        string numberList = null;

        // Parsing command-line arguments
        foreach (var arg in args)
        {
            if (arg.StartsWith("-input:"))
                inputFile = arg.Substring(7); // Extract file name after "-input:"
            else if (arg.StartsWith("-output:"))
                outputFile = arg.Substring(8); // Extract file name after "-output:"
            else if (arg.StartsWith("-numbers:"))
                numberList = arg.Substring(9); // Extract numbers after "-numbers:"
        }

        // If input and output files are provided, process file-based input
        if (inputFile != null && outputFile != null)
        {
            if (!File.Exists(inputFile))
            {
                Error.WriteLine($"Error: Input file '{inputFile}' does not exist.");
                return 1;
            }

            try
            {
                using (var reader = new StreamReader(inputFile))
                using (var writer = new StreamWriter(outputFile, append: false))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (double.TryParse(line, out double x))
                        {
                            writer.WriteLine($"{x,-10:F6} {Sin(x),-15:F6} {Cos(x),-15:F6}");
                        }
                        else
                        {
                            Error.WriteLine($"Error: '{line}' is not a valid number.");
                        }
                    }
                }

                WriteLine($"Data successfully written to: {outputFile}");
                return 0;
            }
            catch (Exception ex)
            {
                Error.WriteLine($"Error: {ex.Message}");
                return 1;
            }
        }

        // If "-numbers:" argument is provided, process inline numbers
        if (numberList != null)
        {
            var numbers = numberList.Split(',');
            foreach (var number in numbers)
            {
                if (double.TryParse(number, out double x))
                {
                    WriteLine($"{x,-10:F6} {Sin(x),-15:F6} {Cos(x),-15:F6}");
                }
                else
                {
                    Error.WriteLine($"Error: '{number}' is not a valid number.");
                }
            }
            return 0;
        }

        // If no valid arguments were provided, show usage instructions
        Error.WriteLine("Error: Incorrect arguments. Usage:");
        Error.WriteLine("mono main.exe -input:input.txt -output:output.txt");
        Error.WriteLine("mono main.exe -numbers:1,2,3,4,5");
        return 1;
    }
}
