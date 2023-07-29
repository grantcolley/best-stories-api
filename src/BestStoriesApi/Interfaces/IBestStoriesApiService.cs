using BestStoriesApi.Models;

namespace BestStoriesApi.Interfaces
{
    public interface IBestStoriesApiService
    {
        Task<Story> GetStoryAsync(int id, CancellationToken cancellationToken);
        Task<IEnumerable<int>> GetBestStoriesAsync(CancellationToken cancellationToken);
    }
}
