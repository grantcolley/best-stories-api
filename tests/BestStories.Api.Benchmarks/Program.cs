using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BestStories.Api.Benchmarks;

BenchmarkRunner.Run(typeof(BestStoriesLockedCacheBenchmarks));
BenchmarkRunner.Run(typeof(BestStoriesInterlockedCacheBenchmarks));

Console.ReadLine();