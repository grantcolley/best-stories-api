using BestStories.Api.Core.Models;

namespace BestStories.Api.Core.Interfaces
{
    public interface IBestStoriesCache
    {
        Task RecycleCacheAsync(IEnumerable<Story> stories);
        Task<IEnumerable<Story>?> GetStoryCacheAsync();
    }
}
