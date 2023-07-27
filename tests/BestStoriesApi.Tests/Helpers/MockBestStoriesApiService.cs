﻿using BestStoriesApi.Interfaces;
using BestStoriesApi.Models;

namespace BestStoriesApi.Tests.Helpers
{
    public class MockBestStoriesApiService : IBestStoriesApiService
    {
        private readonly IEnumerable<Story> _stories;

        public MockBestStoriesApiService() 
        {
            _stories = DataUtility.GetBestStories();
        }

        public Task<IEnumerable<int>> GetBestStoriesAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<int>>(DataUtility.GetBestStoryIds());
        }

        public Task<Story> GetStoryAsync(int id, CancellationToken cancellationToken)
        {
            return Task.FromResult<Story>(_stories.First(s => s.id == id));
        }
    }
}
