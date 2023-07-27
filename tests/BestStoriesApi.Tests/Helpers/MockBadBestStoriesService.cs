using BestStoriesApi.Interfaces;
using BestStoriesApi.Models;

namespace BestStoriesApi.Tests.Helpers
{
    public class MockBadBestStoriesService : IBestStoriesService
    {
        public Task<IEnumerable<Story>> GetBestStoriesAsync(int count, CancellationToken cancellationToken)
        {
            throw new DivideByZeroException();
        }
    }
}
