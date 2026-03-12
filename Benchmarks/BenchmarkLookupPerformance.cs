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
                          - --:  Random-access  :-- -
        | Method            | Mean     | Error     | StdDev    | Allocated |
        |------------------ |---------:|----------:|----------:|----------:|
        | SingleFirstQuick  | 2.499 ns | 0.0479 ns | 0.0448 ns |         - |
        | SingleLastQuick   | 2.553 ns | 0.0685 ns | 0.0641 ns |         - |
        |------------------ |---------:|----------:|----------:|----------:|
        | SingleFirstNative | 8.818 ns | 0.1136 ns | 0.1062 ns |         - |
        | SingleLastNative  | 8.623 ns | 0.1936 ns | 0.2152 ns |         - |
        |------------------ |---------:|----------:|----------:|----------:|
        

                         - --:  Frequent-access  :-- -
        | Method            | Mean     | Error     | StdDev    | Allocated |
        |------------------ |---------:|----------:|----------:|----------:|
        | RunFirstQuick     | 1.850 ns | 0.0352 ns | 0.0329 ns |         - |
        | RunLastQuick      | 1.939 ns | 0.0204 ns | 0.0181 ns |         - |
        |------------------ |---------:|----------:|----------:|----------:|
        | RunFirstNative    | 7.975 ns | 0.1333 ns | 0.1247 ns |         - |
        | RunLastNative     | 7.794 ns | 0.1227 ns | 0.1088 ns |         - |
        |------------------ |---------:|----------:|----------:|----------:|

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
            //Native.Add(typeof(ATransport), new ATransport());
            //Native.Add(typeof(BTransport), new BTransport());
            //Native.Add(typeof(CTransport), new CTransport());
            //Native.Add(typeof(DTransport), new DTransport());
            //Native.Add(typeof(ETransport), new ETransport());
            //Native.Add(typeof(FTransport), new FTransport());
            Quick = new(3);
            Quick.Add(new LoopbackTransport());
            Quick.Add(new TCPTransport());
            Quick.Add(new UDPTransport());
            //Quick.Add(new ATransport());
            //Quick.Add(new BTransport());
            //Quick.Add(new CTransport());
            //Quick.Add(new DTransport());
            //Quick.Add(new ETransport());
            //Quick.Add(new FTransport());
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

        [Benchmark]
        public void SingleFirstQuick()
        {
            if (Quick.TryGet(out LoopbackTransport? transport))
            {
                LoopbackConsumer = transport;
            }
        }

        [Benchmark]
        public void SingleLastQuick()
        {
            if (Quick.TryGet(out UDPTransport? transport))
            {
                UDPConsumer = transport;
            }
        }

        [Benchmark]
        public void SingleFirstNative()
        {
            if (Native.TryGetValue(typeof(LoopbackTransport), out ITransport? transport))
            {
                NativeConsumer = transport;
            }
        }

        [Benchmark]
        public void SingleLastNative()
        {
            if (Native.TryGetValue(typeof(UDPTransport), out ITransport? transport))
            {
                NativeConsumer = transport;
            }
        }

        const int Operations = 100_000;
        [Benchmark(OperationsPerInvoke = Operations)]
        public void RunFirstQuick()
        {
            for (int i = 0; i < Operations; i++)
                if (Quick.TryGet(out LoopbackTransport? transport))
                {
                    LoopbackConsumer = transport;
                }
        }

        [Benchmark(OperationsPerInvoke = Operations)]
        public void RunLastQuick()
        {
            for (int i = 0; i < Operations; i++)
                if (Quick.TryGet(out UDPTransport? transport))
                {
                    UDPConsumer = transport;
                }
        }

        [Benchmark(OperationsPerInvoke = Operations)]
        public void RunFirstNative()
        {
            for (int i = 0; i < Operations; i++)
                if (Native.TryGetValue(typeof(LoopbackTransport), out ITransport? transport))
                {
                    NativeConsumer = transport;
                }
        }

        [Benchmark(OperationsPerInvoke = Operations)]
        public void RunLastNative()
        {
            for (int i = 0; i < Operations; i++)
                if (Native.TryGetValue(typeof(UDPTransport), out ITransport? transport))
                {
                    NativeConsumer = transport;
                }
        }
    }
}
