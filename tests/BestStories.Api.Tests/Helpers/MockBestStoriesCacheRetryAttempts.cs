using BestStories.Api.Cache;
using BestStories.Api.Core.Interfaces;
using BestStories.Api.Core.Models;

namespace BestStories.Api.Tests.Helpers
{
    public class MockBestStoriesCacheRetryAttempts : LockedCache, IBestStoriesCache
    {
        private int _count;

        public new Task<IEnumerable<Story>?> GetStoryCacheAsync()
        {
            if(_count < 3)
            {
                _count++;

                return Task.FromResult<IEnumerable<Story>?>(null);
            }

            return base.GetStoryCacheAsync();
        }
    }
}
