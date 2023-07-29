using BestStories.Api.Core.Models;

namespace BestStories.Api.Core.Interfaces
{
    public interface IBestStoriesApiService
    {
        Task<Story> GetStoryAsync(int id, CancellationToken cancellationToken);
        Task<IEnumerable<int>> GetBestStoriesAsync(CancellationToken cancellationToken);
    }
}
