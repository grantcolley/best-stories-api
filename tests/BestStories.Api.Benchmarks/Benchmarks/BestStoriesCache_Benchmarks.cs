using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using BestStories.Api.Cache;
using BestStories.Api.Core.Interfaces;
using BestStories.Api.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace BestStories.Api.Benchmarks.Benchmarks
{
    [RankColumn]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [MemoryDiagnoser]
    public class BestStoriesCache_Benchmarks
    {
        private IBestStoriesCache? _lockedCache;
        private IBestStoriesCache? _volatileCache;
        private IBestStoriesCache? _semaphoreSlimCache;

        [GlobalSetup]
        public async Task GlobalSetup()
        {
            List<Story> stories = new();

            for (int i = 0; i < 200; i++)
            {
                stories.Add(new Story { id = i });
            }

            ILoggerFactory factory = new NullLoggerFactory();

            _lockedCache = new LockedCache();
            await _lockedCache.RecycleCacheAsync(stories);

            ILogger<VolatileCache> VolatileCacheLogger = factory.CreateLogger<VolatileCache>();
            _volatileCache = new VolatileCache(VolatileCacheLogger);
            await _volatileCache.RecycleCacheAsync(stories);

            ILogger<SemaphoreSlimCache> SemaphoreSlimCachelLogger = factory.CreateLogger<SemaphoreSlimCache>();
            _semaphoreSlimCache = new SemaphoreSlimCache(SemaphoreSlimCachelLogger);
            await _semaphoreSlimCache.RecycleCacheAsync(stories);
        }

        [Benchmark]
        public async Task LockedCache_GetStoryCacheAsync()
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            _ = await _lockedCache.GetStoryCacheAsync();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        [Benchmark]
        public async Task VolatileCache_GetStoryCacheAsync()
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            _ = await _volatileCache.GetStoryCacheAsync();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        [Benchmark]
        public async Task SemaphoreSlimCache_GetStoryCacheAsync()
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            _ = await _semaphoreSlimCache.GetStoryCacheAsync();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }
    }
}
