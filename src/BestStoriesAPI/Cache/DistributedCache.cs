using BestStoriesAPI.Interfaces;
using BestStoriesAPI.Models;
using BestStoriesAPI.Static;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace BestStoriesApi.Cache
{
    /// <summary>
    /// The <see cref="DistributedCache"/> class responsible for maintaining 
    /// the <see cref="IDistributedCache"/>, including fetching the latest 
    /// best stories from HackerNewsAPI and setting them in the cache
    /// with an AbsoluteExpiration specified as the CacheExpiryInSeconds
    /// in <see cref="BestStoriesConfiguration"/>.
    /// </summary>
    internal class DistributedCache : IBestStoriesCache
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly IDistributedCache _distributedCache;
        private readonly IHackerNewsAPIService _hackerNewsAPIService;
        private readonly BestStoriesConfiguration _bestStoriesConfiguration;
        private readonly ILogger<DistributedCache> _logger;

        public DistributedCache(
            IDistributedCache distributedCache,
            IHackerNewsAPIService hackerNewsAPIService,
            IOptions<BestStoriesConfiguration> bestStoriesConfiguration,
            ILogger<DistributedCache> logger)
        {
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _hackerNewsAPIService = hackerNewsAPIService ?? throw new ArgumentNullException(nameof(hackerNewsAPIService));
            _bestStoriesConfiguration = bestStoriesConfiguration?.Value ?? throw new ArgumentNullException(nameof(bestStoriesConfiguration));
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
        /// CacheExpiryInSeconds in <see cref="BestStoriesConfiguration"/>.
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns the cached stories.</returns>
        public async Task<IEnumerable<Story>?> GetStoryCacheAsync(CancellationToken cancellationToken)
        {
            try
            {
                byte[]? stories = await _distributedCache.GetAsync(Constants.DISTRIBUTED_CACHE)
                    .ConfigureAwait(false);

                if (stories != null)
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
        /// set to CacheExpiryInSeconds from <see cref="BestStoriesConfiguration"/>.
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

                byte[]? newStoriesAvailable = await _distributedCache.GetAsync(Constants.DISTRIBUTED_CACHE)
                    .ConfigureAwait(false);

                if (newStoriesAvailable != null)
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
        /// set to CacheExpiryInSeconds from <see cref="BestStoriesConfiguration"/>.
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
                .Take(_bestStoriesConfiguration.CacheMaxSize)
                .ToList();

            DateTimeOffset expires = DateTimeOffset.Now.Add(TimeSpan.FromSeconds(_bestStoriesConfiguration.CacheExpiryInSeconds));

            byte[] storiesToCache = UTF8Encoding.UTF8.GetBytes(JsonSerializer.Serialize(rankedStoriesToCache));

            await _distributedCache.SetAsync(
                Constants.DISTRIBUTED_CACHE,
                storiesToCache,
                new DistributedCacheEntryOptions { AbsoluteExpiration = expires })
                .ConfigureAwait(false);

            await _distributedCache.RefreshAsync(Constants.DISTRIBUTED_CACHE);

            return rankedStoriesToCache;
        }
    }
}
