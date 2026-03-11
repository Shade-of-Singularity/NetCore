using BenchmarkDotNet.Attributes;

namespace NetCore.Benchmarks
{
    public class NativeOperatorPerformance
    {
        const int Repeats = 10_000_000;

        [Benchmark]
        [Arguments(10, 10)]
        public int AddIntInt(int a, int b)
        {
            return a + b;
        }

        [Benchmark]
        [Arguments(10, 10)]
        public int AddUIntInt(uint a, int b)
        {
            return (int)(a + b);
        }

        [Benchmark]
        [Arguments(10, 10)]
        public int AddUShortInt(ushort a, int b)
        {
            return a + b;
        }

        [Benchmark]
        [Arguments(10, 10)]
        public int SubtractIntInt(int a, int b)
        {
            return a - b;
        }

        [Benchmark]
        [Arguments(10, 10)]
        public int SubtractUIntInt(uint a, int b)
        {
            return (int)(a - b);
        }

        [Benchmark]
        [Arguments(10, 10)]
        public int SubtractUShortInt(ushort a, int b)
        {
            return a - b;
        }
    }
}
