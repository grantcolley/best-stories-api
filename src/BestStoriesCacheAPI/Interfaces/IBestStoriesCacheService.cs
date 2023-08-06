using BestStories.Core.Models;

namespace BestStoriesCacheAPI.Interfaces
{
    public interface IBestStoriesCacheService
    {
        Task<IEnumerable<Story>?> GetBestStoriesAsync(int count, CancellationToken token);
    }
}
