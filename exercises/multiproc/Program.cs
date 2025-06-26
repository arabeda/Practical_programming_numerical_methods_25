using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

public class Program
{
    public class PartialSum
    {
        public int Start { get; set; }
        public int Stop { get; set; }
        public double Sum { get; set; }
    }

    public static void HarmonicManual(object obj)
    {
        var data = (PartialSum)obj;
        data.Sum = 0;
        for (int i = data.Start + 1; i <= data.Stop; i++)
            data.Sum += 1.0 / i;
    }

    public static void HarmonicParallelFlawed(int N)
    {
        double sum = 0;
        Parallel.For(1, N + 1, i => sum += 1.0 / i);
        Console.WriteLine("Flawed Parallel.For computed sum = " + sum);
    }

    public static void HarmonicParallelThreadLocal(int N)
    {
        var threadLocalSum = new ThreadLocal<double>(() => 0, true);
        Parallel.For(1, N + 1, i => threadLocalSum.Value += 1.0 / i);
        double totalSum = threadLocalSum.Values.Sum();
        Console.WriteLine("ThreadLocal Parallel.For computed sum = " + totalSum);
    }


    public static void RunManual(int nTerms, int nThreads)
    {
        Console.WriteLine($"Using {nThreads} thread{(nThreads == 1 ? "" : "s")}:");
        Console.WriteLine("Computing harmonic sum using manual threading:");
        Thread[] threads = new Thread[nThreads];
        PartialSum[] partialSums = new PartialSum[nThreads];

        for (int i = 0; i < nThreads; i++)
        {
            partialSums[i] = new PartialSum
            {
                Start = i * nTerms / nThreads,
                Stop = (i + 1) * nTerms / nThreads
            };
        }

        var sw = Stopwatch.StartNew();

        for (int i = 0; i < nThreads; i++)
        {
            threads[i] = new Thread(HarmonicManual);
            threads[i].Start(partialSums[i]);
        }

        foreach (var thread in threads)
            thread.Join();

        sw.Stop();

        double sum = partialSums.Sum(d => d.Sum);
        Console.WriteLine($"Manual threading computed sum = {sum:R}");
        Console.WriteLine($"real {sw.Elapsed.TotalSeconds:F2}\n");
    }

    public static int Main(string[] args)
    {
        int nTerms = 1_000_000; 
        int nThreads = 1;
        bool useParallel = false;
        bool useThreadLocal = false;


        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-threads" when i + 1 < args.Length:
                    int.TryParse(args[++i], out nThreads);
                    break;
                case "-terms" when i + 1 < args.Length:
                    int.TryParse(args[++i], out nTerms);
                    break;
                case "-parallel":
                    useParallel = true;
                    break;
                case "-threadlocal":
                    useThreadLocal = true;
                    break;
            }
        }

        if (useThreadLocal)
        {
            Console.WriteLine("Computing harmonic sum using Parallel.For with ThreadLocal:");
            HarmonicParallelThreadLocal(nTerms);
        }
        else if (useParallel)
        {
            Console.WriteLine("Computing harmonic sum using flawed Parallel.For (race condition):");
            HarmonicParallelFlawed(nTerms);
        }
        else
        {
            if (args.Contains("-threads"))
            {
                RunManual(nTerms, nThreads);
            }
            else 
            {
                foreach (int t in new[] { 1, 2, 3, 4 })
                    RunManual(nTerms, t);
            }
        }

        return 0;
    }
}
