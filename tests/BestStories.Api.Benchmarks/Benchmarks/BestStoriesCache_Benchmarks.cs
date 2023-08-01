using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using BestStories.Api.Cache;
using BestStories.Api.Core.Interfaces;
using BestStories.Api.Core.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BestStories.Api.Benchmarks.Benchmarks
{
    [RankColumn]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [MemoryDiagnoser]
    public class BestStoriesCache_Benchmarks
    {
        private IBestStoriesCache? _lockedCache;
        private IBestStoriesCache? _volatileCache;
        private IBestStoriesCache? _readerWriterLockCache;
        private IBestStoriesCache? _semaphoreSlimCache;
        private IBestStoriesCache? _distributedCache;

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

            ILogger<ReaderWriterLockSlimCache> readerWriterLockSlimLogger = factory.CreateLogger<ReaderWriterLockSlimCache>();
            _readerWriterLockCache = new ReaderWriterLockSlimCache(readerWriterLockSlimLogger);
            await _readerWriterLockCache.RecycleCacheAsync(stories);

            ILogger<SemaphoreSlimCache> SemaphoreSlimCachelLogger = factory.CreateLogger<SemaphoreSlimCache>();
            _semaphoreSlimCache = new SemaphoreSlimCache(SemaphoreSlimCachelLogger);
            await _semaphoreSlimCache.RecycleCacheAsync(stories);

            IOptions<MemoryDistributedCacheOptions> optionsAccessor = Options.Create(new MemoryDistributedCacheOptions());
            ILogger<DistributedCache> distributedCachelLogger = factory.CreateLogger<DistributedCache>();
            _distributedCache = new DistributedCache(new MemoryDistributedCache(optionsAccessor), distributedCachelLogger);
            await _distributedCache.RecycleCacheAsync(stories);
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
        public async Task ReaderWriterLockCache_GetStoryCacheAsync()
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            _ = await _readerWriterLockCache.GetStoryCacheAsync();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        [Benchmark]
        public async Task SemaphoreSlimCache_GetStoryCacheAsync()
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            _ = await _semaphoreSlimCache.GetStoryCacheAsync();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        [Benchmark]
        public async Task DistributedCache_GetStoryCacheAsync()
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            _ = await _distributedCache.GetStoryCacheAsync();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }
    }
}
