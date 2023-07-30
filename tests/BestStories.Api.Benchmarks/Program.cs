using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BestStories.Api.Benchmarks.Benchmarks;

_ = BenchmarkRunner.Run(typeof(BestStoriesCache_Benchmarks));

Console.ReadLine();