using BestStories.Api.Core.Interfaces;
using BestStories.Api.Core.Models;

namespace BestStories.Api.Cache
{
    public class VolatileCache : IBestStoriesCache
    {
        private readonly ILogger<VolatileCache> _logger;
        private IEnumerable<Story>? _storyCache = null;

        public VolatileCache(ILogger<VolatileCache> logger)
        {
            _logger = logger;
        }

        public Task RecycleCacheAsync(IEnumerable<Story> stories)
        {
            // https://learn.microsoft.com/en-us/dotnet/api/system.threading.interlocked.compareexchange?view=net-7.0#system-threading-interlocked-compareexchange-1(-0@-0-0)
            //
            // Compares two instances of the specified reference type T for
            // reference equality and, if they are equal, replaces the first one.
            // 
            // If the recycle doesn't work simply try again on the next attempt.

            try
            {
                _ = Interlocked.CompareExchange<IEnumerable<Story>?>(ref _storyCache, stories, _storyCache);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return Task.CompletedTask;
        }

        public async Task<IEnumerable<Story>?> GetStoryCacheAsync()
        {
            // https://learn.microsoft.com/en-us/dotnet/api/system.threading.volatile.read?view=net-7.0#system-threading-volatile-read-1(-0@)
            //
            // Returns the reference to T that was read.
            // 
            // This reference is the latest written by any processor in the computer,
            // regardless of the number of processors or the state of processor cache.

            return await Task.FromResult(Volatile.Read<IEnumerable<Story>?>(ref _storyCache));
        }
    }
}
