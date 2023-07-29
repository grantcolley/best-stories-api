using BestStoriesApi.Models;

namespace BestStoriesApi.Interfaces
{
    public interface IBestStoriesService
    {
        Task<IEnumerable<Story>> GetBestStoriesAsync(int count, CancellationToken token);
    }
}
