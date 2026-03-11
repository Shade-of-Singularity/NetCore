using BenchmarkDotNet.Running;
using NetCore.Benchmarks;

var instance = new BenchmarkLookupPerformance();
instance.Setup();
instance.RunLastQuick();

BenchmarkRunner.Run<BenchmarkLookupPerformance>();