using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using BestStories.Api.Cache;
using BestStories.Api.Core.Interfaces;
using BestStories.Api.Core.Models;

namespace BestStories.Api.Benchmarks
{
    [RankColumn]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [MemoryDiagnoser]
    public class BestStoriesLockedCacheBenchmarks
    {
        private IBestStoriesCache? bestStoriesCache;

        [GlobalSetup]
        public void GlobalSetup()
        {
            bestStoriesCache = new BestStoriesLockedCache();

            List<Story> stories = new();

            for(int i = 0; i < 200; i++) 
            {
                stories.Add(new Story());
            }

            bestStoriesCache.RecycleCache(stories);
        }

        [Benchmark]
        public async Task BestStoriesLockedCache_100_Requests()
        {
            List<int> requests = new();

            for (int i = 0; i < 100; i++)
            {
                requests.Add(i);
            }

            Task<IEnumerable<Story>?>[] bestStories = requests.Select(id =>
            {
                return Task.FromResult(bestStoriesCache?.GetStoryCache());
            }).ToArray();

            IEnumerable<Story>?[] stories = await Task.WhenAll(bestStories);
        }

        [Benchmark]
        public async Task BestStoriesLockedCache_500_Requests()
        {
            List<int> requests = new();

            for (int i = 0; i < 500; i++)
            {
                requests.Add(i);
            }

            Task<IEnumerable<Story>?>[] bestStories = requests.Select(id =>
            {
                return Task.FromResult(bestStoriesCache?.GetStoryCache());
            }).ToArray();

            IEnumerable<Story>?[] stories = await Task.WhenAll(bestStories);
        }

        [Benchmark]
        public async Task BestStoriesLockedCache_1000_Requests()
        {
            List<int> requests = new();

            for (int i = 0; i < 1000; i++)
            {
                requests.Add(i);
            }

            Task<IEnumerable<Story>?>[] bestStories = requests.Select(id =>
            {
                return Task.FromResult(bestStoriesCache?.GetStoryCache());
            }).ToArray();

            IEnumerable<Story>?[] stories = await Task.WhenAll(bestStories);
        }
    }
}
