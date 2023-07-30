using BestStories.Api.Core.Exceptions;
using BestStories.Api.Core.Interfaces;
using BestStories.Api.Core.Models;

namespace BestStories.Api.Services
{
    public class BestStoriesService : IBestStoriesService
    {
        private readonly IBestStoriesCache _bestStoriesCache;
        private readonly ILogger<BestStoriesService> _logger;
        private readonly int _cacheRetryDelay;
        private readonly int _cacheMaxRetryAttempts;

        public BestStoriesService(
            IBestStoriesCache bestStoriesCache, 
            ILogger<BestStoriesService> logger,
            IConfiguration configuration) 
        {
            _bestStoriesCache = bestStoriesCache;
            _logger = logger;

            _cacheRetryDelay = configuration.GetValue<int>("BestStories:CacheRetryDelay");
            _cacheMaxRetryAttempts = configuration.GetValue<int>("BestStories:CacheMaxRetryAttempts");
        }

        public async Task<IEnumerable<Story>> GetBestStoriesAsync(int count, CancellationToken cancellationToken)
        {
            try
            {
                int retryAttempt = 0;

                IEnumerable<Story>? storyCache = await _bestStoriesCache.GetStoryCacheAsync();

                while(storyCache == null)
                {
                    // If the cache is empty, retry the specified
                    // number of times before giving up.

                    await Task.Delay(_cacheRetryDelay, cancellationToken);

                    if(cancellationToken.IsCancellationRequested)
                    {
                        return Enumerable.Empty<Story>();
                    }

                    storyCache = await _bestStoriesCache.GetStoryCacheAsync();

                    retryAttempt++;

                    if (storyCache == null 
                        && retryAttempt > _cacheMaxRetryAttempts)
                    {
                        throw new BestStoryException($"Exceeded max retry attempts {_cacheMaxRetryAttempts}.");
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
