using BenchmarkDotNet.Running;
using NetCore.Benchmarks;

BenchmarkRunner.Run<NativeOperatorPerformance>();

//var instance = new BenchmarkLookupPerformance();
//instance.Setup();
//instance.RunLastQuick();

//BenchmarkRunner.Run<BenchmarkLookupPerformance>();