using BenchmarkDotNet.Running;
using System;

namespace BenchmarkPlayground
{
    class Program
    {
        static void Main(string[] args)
        {
            new BenchmarkSwitcher(typeof(Program).Assembly).Run(args);

            //#if DEBUG
            Console.ReadKey();
            //#endif
        }
    }
}
