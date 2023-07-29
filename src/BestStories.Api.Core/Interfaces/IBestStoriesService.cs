using BestStories.Api.Core.Models;

namespace BestStories.Api.Core.Interfaces
{
    public interface IBestStoriesService
    {
        Task<IEnumerable<Story>> GetBestStoriesAsync(int count, CancellationToken token);
    }
}
