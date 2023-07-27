using BestStoriesApi.Cache;
using BestStoriesApi.Interfaces;
using BestStoriesApi.Models;

namespace BestStoriesApi.Tests.Helpers
{
    public class MockBestStoriesCacheRetryAttempts : BestStoriesCache, IBestStoriesCache
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
