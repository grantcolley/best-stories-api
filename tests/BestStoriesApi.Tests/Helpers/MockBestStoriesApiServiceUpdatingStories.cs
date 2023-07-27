using BestStoriesApi.Interfaces;
using BestStoriesApi.Models;

namespace BestStoriesApi.Tests.Helpers
{
    public class MockBestStoriesApiServiceUpdatingStories : IBestStoriesApiService
    {
        private readonly IEnumerable<Story> _stories;

        private int _count;

        public MockBestStoriesApiServiceUpdatingStories()
        {
            _stories = DataUtility.GetUpdatedBestStories();
        }

        public Task<IEnumerable<int>> GetBestStoriesAsync(CancellationToken cancellationToken)
        {
            IEnumerable<int> bestStoryIds = DataUtility.GetBestStoryIds();

            if(_count == 0) 
            {
                _count++;
                return Task.FromResult<IEnumerable<int>>(bestStoryIds);
            }

            List<int> updatedStoryIds = new(bestStoryIds);
            
            updatedStoryIds.AddRange(new[] { 1, 2, 3 });

            return Task.FromResult<IEnumerable<int>>(updatedStoryIds);
        }

        public Task<Story> GetStoryAsync(int id, CancellationToken cancellationToken)
        {
            return Task.FromResult<Story>(_stories.First(s => s.id == id));
        }
    }
}
