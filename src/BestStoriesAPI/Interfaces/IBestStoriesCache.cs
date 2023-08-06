using BestStoriesAPI.Models;

namespace BestStoriesAPI.Interfaces
{
    public interface IBestStoriesCache
    {
        Task<IEnumerable<Story>?> GetStoryCacheAsync(CancellationToken cancellationToken);
    }
}
