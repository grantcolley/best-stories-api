using BestStoriesAPI.Models;

namespace BestStoriesAPI.Interfaces
{
    public interface IHackerNewsAPIService
    {
        Task<IEnumerable<Story>> GetBestStoryiesAsync(CancellationToken cancellationToken);
    }
}
