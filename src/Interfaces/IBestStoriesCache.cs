using BestStoriesApi.Models;

namespace BestStoriesApi.Interfaces
{
    public interface IBestStoriesCache
    {
        void RecycleCache(IEnumerable<Story> stories);
        IEnumerable<Story>? GetStoryCache();
    }
}
