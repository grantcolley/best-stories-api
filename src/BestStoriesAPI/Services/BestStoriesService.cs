using BestStories.Core.Models;
using BestStories.Core.Static;
using BestStoriesAPI.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace BestStoriesAPI.Services
{
    /// <summary>
    /// The <see cref="BestStoriesService"/> class is responsible  
    /// for fetching the top n stories from the cache.
    /// </summary>
    internal class BestStoriesService : IBestStoriesService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IBestStoriesCacheAPIService _bestStoriesCacheAPIService;
        private readonly ILogger<BestStoriesService> _logger;

        public BestStoriesService(
            IDistributedCache distributedCache,
            IBestStoriesCacheAPIService bestStoriesCacheAPIService,
            ILogger<BestStoriesService> logger) 
        {
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _bestStoriesCacheAPIService = bestStoriesCacheAPIService ?? throw new ArgumentNullException(nameof(distributedCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the cached stories and returns the top `n`
        /// stories as specified in the count argument.
        /// </summary>
        /// <param name="count">The number or stories to return.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The top `n` stories as specified in the count argument.</returns>
        public async Task<IEnumerable<Story>?> GetBestStoriesAsync(int count, CancellationToken cancellationToken)
        {
            try
            {
                byte[]? stories = await _distributedCache.GetAsync(Constants.DISTRIBUTED_CACHE_BEST_STORIES, cancellationToken)
                    .ConfigureAwait(false);

                if (stories != null
                    && stories.Length > 0)
                {
                    // stories are cached in descending order of their score.
                    // just take the first `n` stories from the list.

                    return JsonSerializer.Deserialize<IEnumerable<Story>>(stories)?
                        .Take(count)
                        .ToList();
                }

                // call BestStoriesCacheAPI to recycle the cache

                return await _bestStoriesCacheAPIService.GetBestStoriesAsync(count, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetBestStoriesAsync({count})");

                return await Task.FromException<IEnumerable<Story>>(ex);
            }
        }
    }
}
