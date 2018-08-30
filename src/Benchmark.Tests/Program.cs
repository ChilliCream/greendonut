using BenchmarkDotNet.Running;

namespace GreenDonut.Benchmark.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<TaskCompletionBufferBenchmarks>();
        }
    }
}
