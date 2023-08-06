using BestStories.Core.Models;

namespace BestStoriesCacheAPI.Interfaces
{
    public interface IBestStoriesCache
    {
        Task<IEnumerable<Story>?> GetStoryCacheAsync(CancellationToken cancellationToken);
    }
}
