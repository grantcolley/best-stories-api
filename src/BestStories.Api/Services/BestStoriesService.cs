using BestStories.Api.Core.Exceptions;
using BestStories.Api.Core.Interfaces;
using BestStories.Api.Core.Models;
using Microsoft.Extensions.Options;

namespace BestStories.Api.Services
{
    public class BestStoriesService : IBestStoriesService
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

        public async Task<IEnumerable<Story>> GetBestStoriesAsync(int count, CancellationToken cancellationToken)
        {
            try
            {
                int retryAttempt = 0;

                IEnumerable<Story>? storyCache = await _bestStoriesCache.GetStoryCacheAsync()
                    .ConfigureAwait(false);

                while(storyCache == null)
                {
                    // If the cache is empty, retry the specified
                    // number of times before giving up.

                    await Task.Delay(_bestStoriesConfiguration.CacheRetryDelay, cancellationToken)
                        .ConfigureAwait(false);

                    if(cancellationToken.IsCancellationRequested)
                    {
                        return Enumerable.Empty<Story>();
                    }

                    storyCache = await _bestStoriesCache.GetStoryCacheAsync()
                        .ConfigureAwait(false);

                    retryAttempt++;

                    if (storyCache == null 
                        && retryAttempt > _bestStoriesConfiguration.CacheMaxRetryAttempts)
                    {
                        throw new BestStoryException($"Exceeded max retry attempts {_bestStoriesConfiguration.CacheMaxRetryAttempts}.");
                    }
                }

                return storyCache.Take(count).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetBestStoriesAsync({count})");

                return await Task.FromException<IEnumerable<Story>>(ex);
            }
        }
    }
}
