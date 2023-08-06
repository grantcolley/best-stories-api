using BestStories.Core.Models;

namespace BestStoriesAPI.Interfaces
{
    public interface IBestStoriesCacheAPIService
    {
        Task<IEnumerable<Story>> GetBestStoriesAsync(int count, CancellationToken cancellationToken);
    }
}
