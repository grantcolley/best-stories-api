using BestStories.Api.Core.Interfaces;
using BestStories.Api.Core.Models;

namespace BestStories.Api.Cache
{
    public class BestStoriesLockedCache : IBestStoriesCache
    {
        private readonly object _lockCache = new();
        private IEnumerable<Story>? _storyCache = null;

        public void RecycleCache(IEnumerable<Story> stories)
        {
            lock (_lockCache)
            {
                _storyCache = stories;
            }
        }

        public IEnumerable<Story>? GetStoryCache() 
        {
            lock (_lockCache)
            {
                return _storyCache;
            }
        }
    }
}
