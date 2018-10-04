using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace GreenDonut.Benchmark.Tests
{
    [CoreJob]
    [RPlotExporter, MemoryDiagnoser]
    public class TaskCompletionBufferBenchmarks
    {
        private TaskCompletionBuffer<string, int> _buffer;
        private string[] _keys;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _keys = new string[1000];

            for (var i = 0; i < _keys.Length; i++)
            {
                _keys[i] = Guid.NewGuid().ToString("N");
            }
        }

        [IterationSetup]
        public void IterationSetup()
        {
            _buffer = new TaskCompletionBuffer<string, int>();
        }

        [Benchmark]
        public bool TryAddSingle()
        {
            return _buffer.TryAdd(_keys[0], new TaskCompletionSource<int>(
                TaskCreationOptions.RunContinuationsAsynchronously));
        }

        [Benchmark]
        public bool TryAddSingleTwice()
        {
            var result = false;

            result = _buffer.TryAdd(_keys[0], new TaskCompletionSource<int>(
                TaskCreationOptions.RunContinuationsAsynchronously));
            result = _buffer.TryAdd(_keys[0], new TaskCompletionSource<int>(
                TaskCreationOptions.RunContinuationsAsynchronously));

            return result;
        }

        [Benchmark]
        public bool TryAdd1000()
        {
            var result = false;

            for (var i = 0; i < _keys.Length; i++)
            {
                result = _buffer.TryAdd(_keys[i],
                    new TaskCompletionSource<int>(
                        TaskCreationOptions.RunContinuationsAsynchronously));
            }

            return result;
        }
    }
}
