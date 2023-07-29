using BestStories.Api.Core.Interfaces;
using BestStories.Api.Core.Models;

namespace BestStories.Api.Tests.Helpers
{
    public class MockBadBestStoriesService : IBestStoriesService
    {
        public Task<IEnumerable<Story>> GetBestStoriesAsync(int count, CancellationToken cancellationToken)
        {
            throw new DivideByZeroException();
        }
    }
}
