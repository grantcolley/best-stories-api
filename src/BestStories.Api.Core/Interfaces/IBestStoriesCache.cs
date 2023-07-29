using BestStories.Api.Core.Models;

namespace BestStories.Api.Core.Interfaces
{
    public interface IBestStoriesCache
    {
        void RecycleCache(IEnumerable<Story> stories);
        IEnumerable<Story>? GetStoryCache();
    }
}
