using BenchmarkDotNet.Attributes;
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

        From the recent time:
        | Method         | Mean     | Error     | StdDev    | Allocated |
        |--------------- |---------:|----------:|----------:|----------:|
        | RunFirstNative | 7.926 ns | 0.1484 ns | 0.1389 ns |         - |
        | RunFirstQuick  | 6.042 ns | 0.1062 ns | 0.0993 ns |         - |
        | RunLastNative  | 7.737 ns | 0.1634 ns | 0.1528 ns |         - |
        | RunLastQuick   | 6.087 ns | 0.1469 ns | 0.2107 ns |         - |
        So system is under rework.

        Before introducing raw fields:
        | Method        | Mean     | Error     | StdDev    |
        |-------------- |---------:|----------:|----------:|
        | RunLastNative | 8.646 ns | 0.1916 ns | 0.3783 ns |
        | RunLastQuick  | 5.935 ns | 0.1369 ns | 0.1406 ns |

        After introducing raw fields:
        ...

        Full:
        | Method        | Mean     | Error     | StdDev    |
        |-------------- |---------:|----------:|----------:|
        | RunLastQuick  | 6.403 ns | 0.1503 ns | 0.1955 ns |
        | RunLastNative | 7.975 ns | 0.1858 ns | 0.2139 ns |

        First half:
        | Method        | Mean     | Error     | StdDev    |
        |-------------- |---------:|----------:|----------:|
        | RunLastQuick  | 3.134 ns | 0.0874 ns | 0.0858 ns |
        | RunLastNative | 8.082 ns | 0.1766 ns | 0.1652 ns |

        Second half (+ struct creation):


        */

        private sealed class ATransport : Transport
        {
            public override bool HasConnection(ConnectionID connection) => throw new NotImplementedException();
        }
        private sealed class BTransport : Transport
        {
            public override bool HasConnection(ConnectionID connection) => throw new NotImplementedException();
        }
        private sealed class CTransport : Transport
        {
            public override bool HasConnection(ConnectionID connection) => throw new NotImplementedException();
        }
        private sealed class DTransport : Transport
        {
            public override bool HasConnection(ConnectionID connection) => throw new NotImplementedException();
        }
        private sealed class ETransport : Transport
        {
            public override bool HasConnection(ConnectionID connection) => throw new NotImplementedException();
        }
        private sealed class FTransport : Transport
        {
            public override bool HasConnection(ConnectionID connection) => throw new NotImplementedException();
        }

        [GlobalSetup]
        public void Setup()
        {
            Native.Add(typeof(LoopbackTransport), new LoopbackTransport());
            Native.Add(typeof(TCPTransport), new TCPTransport());
            Native.Add(typeof(UDPTransport), new UDPTransport());
            Native.Add(typeof(ATransport), new ATransport());
            Native.Add(typeof(BTransport), new BTransport());
            Native.Add(typeof(CTransport), new CTransport());
            Native.Add(typeof(DTransport), new DTransport());
            Native.Add(typeof(ETransport), new ETransport());
            Native.Add(typeof(FTransport), new FTransport());
            Quick = new(3);
            Quick.Add(new LoopbackTransport());
            Quick.Add(new TCPTransport());
            Quick.Add(new UDPTransport());
            Quick.Add(new ATransport());
            Quick.Add(new BTransport());
            Quick.Add(new CTransport());
            Quick.Add(new DTransport());
            Quick.Add(new ETransport());
            Quick.Add(new FTransport());
            //int r = 10;
            //for (int i = 0; i < 1000000000; i++)
            //{
            //    r += i;
            //}

            //Console.WriteLine(r);
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

        //[Benchmark]
        //public void RunFirstNative()
        //{
        //    if (Native.TryGetValue(typeof(LoopbackTransport), out ITransport? transport))
        //    {
        //        NativeConsumer = transport;
        //    }
        //}

        //[Benchmark]
        //public void RunFirstQuick()
        //{
        //    if (Quick.TryGet(out LoopbackTransport? transport))
        //    {
        //        LoopbackConsumer = transport;
        //    }
        //}

        [Benchmark(OperationsPerInvoke = 100_000)]
        public void RunLastQuick()
        {
            for (int i = 0; i < 100_000; i++)
            {
                if (Quick.TryGet(out UDPTransport? transport))
                {
                    UDPConsumer = transport;
                }
            }
        }

        [Benchmark(OperationsPerInvoke = 100_000)]
        public void RunLastNative()
        {
            for (int i = 0; i < 100_000; i++)
            {
                if (Native.TryGetValue(typeof(UDPTransport), out ITransport? transport))
                {
                    NativeConsumer = transport;
                }
            }
        }
    }
}
