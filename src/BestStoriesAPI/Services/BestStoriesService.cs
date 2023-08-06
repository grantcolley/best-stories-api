﻿using BestStoriesAPI.Interfaces;
using BestStoriesAPI.Models;
using Microsoft.Extensions.Options;

namespace BestStoriesAPI.Services
{
    /// <summary>
    /// The <see cref="BestStoriesService"/> class is responsible  
    /// for fetching the top n stories from the cache.
    /// </summary>
    internal class BestStoriesService : IBestStoriesService
    {
        private readonly IBestStoriesCache _bestStoriesCache;
        private readonly ILogger<BestStoriesService> _logger;
        private readonly BestStoriesConfiguration _bestStoriesConfiguration;

        public BestStoriesService(
            IBestStoriesCache bestStoriesCache, 
            IOptions<BestStoriesConfiguration> bestStoriesConfiguration,
            ILogger<BestStoriesService> logger) 
        {
            _bestStoriesCache = bestStoriesCache ?? throw new ArgumentNullException(nameof(bestStoriesCache));
            _bestStoriesConfiguration = bestStoriesConfiguration?.Value ?? throw new ArgumentNullException(nameof(bestStoriesConfiguration));
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
                IEnumerable<Story>? storyCache = await _bestStoriesCache.GetStoryCacheAsync(cancellationToken)
                    .ConfigureAwait(false);

                // stories are cached in descending order of their score.
                // just take the first `n` stories from the list.

                return storyCache?.Take(count).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetBestStoriesAsync({count})");

                return await Task.FromException<IEnumerable<Story>>(ex);
            }
        }
    }
}
