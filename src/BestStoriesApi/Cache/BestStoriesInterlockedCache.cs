using BestStoriesApi.Interfaces;
using BestStoriesApi.Models;

namespace BestStoriesApi.Cache
{
    public class BestStoriesInterlockedCache : IBestStoriesCache
    {
        private readonly ILogger<BestStoriesInterlockedCache> _logger;
        private IEnumerable<Story>? _storyCache = null;

        public BestStoriesInterlockedCache(ILogger<BestStoriesInterlockedCache> logger)
        {
            _logger = logger;
        }

        public void RecycleCache(IEnumerable<Story> stories)
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
        }

        public IEnumerable<Story>? GetStoryCache()
        {
            // https://learn.microsoft.com/en-us/dotnet/api/system.threading.volatile.read?view=net-7.0#system-threading-volatile-read-1(-0@)
            //
            // Returns the reference to T that was read.
            // 
            // This reference is the latest written by any processor in the computer,
            // regardless of the number of processors or the state of processor cache.

            return Volatile.Read<IEnumerable<Story>?>(ref _storyCache);
        }
    }
}
