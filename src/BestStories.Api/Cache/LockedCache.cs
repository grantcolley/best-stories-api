using BestStories.Api.Core.Interfaces;
using BestStories.Api.Core.Models;

namespace BestStories.Api.Cache
{
    public class LockedCache : IBestStoriesCache
    {
        private readonly object _lockCache = new();
        private IEnumerable<Story>? _storyCache = null;

        public Task RecycleCacheAsync(IEnumerable<Story> stories)
        {
            lock (_lockCache)
            {
                _storyCache = stories;
            }

            return Task.CompletedTask;
        }

        public Task<IEnumerable<Story>?> GetStoryCacheAsync()
        {
            lock (_lockCache)
            {
                return Task.FromResult(_storyCache);
            }
        }
    }
}
