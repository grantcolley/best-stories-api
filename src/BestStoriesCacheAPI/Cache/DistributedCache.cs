using BestStories.Core.Models;
using BestStories.Core.Static;
using BestStoriesCacheAPI.Interfaces;
using BestStoriesCacheAPI.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace BestStoriesCacheAPI.Cache
{
    /// <summary>
    /// The <see cref="DistributedCache"/> class responsible for maintaining 
    /// the <see cref="IDistributedCache"/>, including fetching the latest 
    /// best stories from HackerNewsAPI and setting them in the cache
    /// with an AbsoluteExpiration specified as the CacheExpiryInSeconds
    /// in <see cref="BestStoriesCacheConfiguration"/>.
    /// </summary>
    internal class DistributedCache : IBestStoriesCache
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly IDistributedCache _distributedCache;
        private readonly IHackerNewsAPIService _hackerNewsAPIService;
        private readonly BestStoriesCacheConfiguration _bestStoriesCacheConfiguration;
        private readonly ILogger<DistributedCache> _logger;

        public DistributedCache(
            IDistributedCache distributedCache,
            IHackerNewsAPIService hackerNewsAPIService,
            IOptions<BestStoriesCacheConfiguration> bestStoriesCacheConfiguration,
            ILogger<DistributedCache> logger)
        {
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _hackerNewsAPIService = hackerNewsAPIService ?? throw new ArgumentNullException(nameof(hackerNewsAPIService));
            _bestStoriesCacheConfiguration = bestStoriesCacheConfiguration?.Value ?? throw new ArgumentNullException(nameof(bestStoriesCacheConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the cached stories from the distributed cache. 
        /// 
        /// If there are no stories in the cache, fetch them 
        /// directly from HackerNewsAPI and recycle the cache.
        /// 
        /// The latest best stories are cached with an 
        /// AbsoluteExpiration specified as the 
        /// CacheExpiryInSeconds in <see cref="BestStoriesCacheConfiguration"/>.
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns the cached stories.</returns>
        public async Task<IEnumerable<Story>?> GetStoryCacheAsync(CancellationToken cancellationToken)
        {
            try
            {
                byte[]? stories = await _distributedCache.GetAsync(Constants.DISTRIBUTED_CACHE_BEST_STORIES, cancellationToken)
                    .ConfigureAwait(false);

                if (stories != null
                    && stories.Length > 0)
                {
                    return JsonSerializer.Deserialize<IEnumerable<Story>>(stories);
                }

                return await RecycleTheCacheAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                return await Task.FromException<IEnumerable<Story>?>(ex);
            }
        }

        /// <summary>
        /// Recycle the cache by fetching the latest best stories from HackerNewsApi
        /// and replacing the current set, specifying a new AbsoluteExpiration that is
        /// set to CacheExpiryInSeconds from <see cref="BestStoriesCacheConfiguration"/>.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns the cached stories.</returns>
        private async Task<IEnumerable<Story>?> RecycleTheCacheAsync(CancellationToken cancellationToken)
        {
            try
            {
                // asynchronously wait allowing one request at a time.

                await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

                // on entering the semaphore double check the cache still needs to be built.

                byte[]? newStoriesAvailable = await _distributedCache.GetAsync(Constants.DISTRIBUTED_CACHE_BEST_STORIES, cancellationToken)
                    .ConfigureAwait(false);

                if (newStoriesAvailable != null
                    && newStoriesAvailable.Length > 0)
                {
                    return JsonSerializer.Deserialize<IEnumerable<Story>>(newStoriesAvailable);
                }

                // fetch the best stories from the HackerNewsAPI.

                var newStoriesToCache = await _hackerNewsAPIService.GetBestStoryiesAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (cancellationToken.IsCancellationRequested)
                {
                    return await Task.FromCanceled<IEnumerable<Story>>(cancellationToken);
                }

                // persist the top best stories in the distributed cache.

                return await PersistStoriesToCacheAsync(newStoriesToCache)
                    .ConfigureAwait(false);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Save the stories to the distributed cache, with a new AbsoluteExpiration 
        /// set to CacheExpiryInSeconds from <see cref="BestStoriesCacheConfiguration"/>.
        /// 
        /// Stories are ordered descending of their score and then the top `n` stories 
        /// up to the CacheMaxSize are persisted to the cache. 
        /// </summary>
        /// <param name="stories">The stories to cache.</param>
        /// <returns>Returns the stories that has been cached.</returns>
        private async Task<IEnumerable<Story>> PersistStoriesToCacheAsync(IEnumerable<Story> stories)
        {
            IEnumerable<Story> rankedStoriesToCache = 
                stories.OrderByDescending(s => s.score)
                .Take(_bestStoriesCacheConfiguration.CacheMaxSize)
                .ToList();

            DateTimeOffset expires = DateTimeOffset.Now.Add(TimeSpan.FromSeconds(_bestStoriesCacheConfiguration.CacheExpiryInSeconds));

            byte[] storiesToCache = UTF8Encoding.UTF8.GetBytes(JsonSerializer.Serialize(rankedStoriesToCache));

            await _distributedCache.SetAsync(
                Constants.DISTRIBUTED_CACHE_BEST_STORIES,
                storiesToCache,
                new DistributedCacheEntryOptions { AbsoluteExpiration = expires })
                .ConfigureAwait(false);

            return rankedStoriesToCache;
        }
    }
}
