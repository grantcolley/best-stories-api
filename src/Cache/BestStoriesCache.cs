using BestStoriesApi.Interfaces;
using BestStoriesApi.Models;

namespace BestStoriesApi.Cache
{
    public class BestStoriesCache : IBestStoriesCache
    {
        private readonly object _lockCache = new();
        private IEnumerable<Story>? _storyCache = null;

        public void RecycleCache(IEnumerable<Story> stories)
        {
            // lock the cache when swapping it for the new one

            lock (_lockCache)
            {
                _storyCache = stories;
            }
        }

        public IEnumerable<Story>? GetStoryCache() 
        {
            // lock the cache when returning a copy of the reference

            lock (_lockCache)
            {
                return _storyCache;
            }
        }
    }
}
