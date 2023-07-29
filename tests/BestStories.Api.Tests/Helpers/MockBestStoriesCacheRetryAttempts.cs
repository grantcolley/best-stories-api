using BestStories.Api.Cache;
using BestStories.Api.Core.Interfaces;
using BestStories.Api.Core.Models;

namespace BestStories.Api.Tests.Helpers
{
    public class MockBestStoriesCacheRetryAttempts : BestStoriesLockedCache, IBestStoriesCache
    {
        private int _count;

        public new IEnumerable<Story>? GetStoryCache()
        {
            if(_count < 3)
            {
                _count++;

                return null;
            }

            return base.GetStoryCache();
        }
    }
}
