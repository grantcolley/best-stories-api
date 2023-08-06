using BestStoriesAPI.Models;

namespace BestStoriesAPI.Interfaces
{
    public interface IBestStoriesService
    {
        Task<IEnumerable<Story>?> GetBestStoriesAsync(int count, CancellationToken token);
    }
}
