using BenchmarkDotNet.Attributes;
using NetCore;
using NetCore.Common;
using NetCore.Loopback;
using NetCore.TCP;
using NetCore.UDP;
using System;
using System.Collections.Generic;

namespace NetCore.Benchmarks
{
    [MemoryDiagnoser]
    public class BenchmarkLookupPerformance
    {
        private readonly Dictionary<Type, ITransport> Native = [];
        private ITransport? NativeConsumer;

        private QuickMap<ITransport> Quick;
        private LoopbackTransport? LoopbackConsumer;
        private UDPTransport? UDPConsumer;

        /*
        
        | Method    | Mean     | Error     | StdDev    | Allocated |
        |---------- |---------:|----------:|----------:|----------:|
        | RunNative | 9.316 ns | 0.0647 ns | 0.0605 ns |         - |
        | RunQuick  | 3.024 ns | 0.0235 ns | 0.0220 ns |         - |

        | Method         | Mean     | Error     | StdDev    | Allocated |
        |--------------- |---------:|----------:|----------:|----------:|
        | RunFirstNative | 8.711 ns | 0.1106 ns | 0.1034 ns |         - |
        | RunFirstQuick  | 3.072 ns | 0.0751 ns | 0.0703 ns |         - |
        | RunLastNative  | 7.975 ns | 0.0575 ns | 0.0510 ns |         - |
        | RunLastQuick   | 3.045 ns | 0.0301 ns | 0.0251 ns |         - |

        */

        [GlobalSetup]
        public void Setup()
        {
            Native.Add(typeof(LoopbackTransport), new LoopbackTransport());
            Native.Add(typeof(TCPTransport), new TCPTransport());
            Native.Add(typeof(UDPTransport), new UDPTransport());
            Quick = new(3);
            Quick.Add(new LoopbackTransport());
            Quick.Add(new TCPTransport());
            Quick.Add(new UDPTransport());
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            Native.Clear();
            NativeConsumer = null;
            Quick = default;
            LoopbackConsumer = null;
            UDPConsumer = null;
        }

        [Benchmark]
        public void RunFirstNative()
        {
            if (Native.TryGetValue(typeof(LoopbackTransport), out ITransport? transport))
            {
                NativeConsumer = transport;
            }
        }

        [Benchmark]
        public void RunFirstQuick()
        {
            if (Quick.TryGet(out LoopbackTransport? transport))
            {
                LoopbackConsumer = transport;
            }
        }

        [Benchmark]
        public void RunLastNative()
        {
            if (Native.TryGetValue(typeof(UDPTransport), out ITransport? transport))
            {
                NativeConsumer = transport;
            }
        }

        [Benchmark]
        public void RunLastQuick()
        {
            if (Quick.TryGet(out UDPTransport? transport))
            {
                UDPConsumer = transport;
            }
        }
    }
}
