using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BestStoriesApi.Benchmarks;

BenchmarkRunner.Run(typeof(BestStoriesLockedCacheBenchmarks));
BenchmarkRunner.Run(typeof(BestStoriesInterlockedCacheBenchmarks));

Console.ReadLine();