using BestStories.Core.Models;

namespace BestStoriesCacheAPI.Interfaces
{
    public interface IHackerNewsAPIService
    {
        Task<IEnumerable<Story>> GetBestStoryiesAsync(CancellationToken cancellationToken);
    }
}
