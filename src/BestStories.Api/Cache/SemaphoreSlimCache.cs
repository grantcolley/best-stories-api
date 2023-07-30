using BestStories.Api.Core.Interfaces;
using BestStories.Api.Core.Models;

namespace BestStories.Api.Cache
{
    public class SemaphoreSlimCache : IBestStoriesCache
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly ILogger<SemaphoreSlimCache> _logger;
        private IEnumerable<Story>? _storyCache = null;

        public SemaphoreSlimCache(ILogger<SemaphoreSlimCache> logger)
        {
            _logger = logger;
        }

        public async Task RecycleCacheAsync(IEnumerable<Story> stories)
        {
            // If the recycle doesn't work simply try again on the next attempt.

            await _semaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                _storyCache = stories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<IEnumerable<Story>?> GetStoryCacheAsync()
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                return _storyCache;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                return await Task.FromException<IEnumerable<Story>?>(ex);
            }
            finally 
            {
                _semaphore.Release();
            }
        }
    }
}
