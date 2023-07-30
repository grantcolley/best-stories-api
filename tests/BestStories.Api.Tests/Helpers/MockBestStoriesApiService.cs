﻿using BestStories.Api.Core.Interfaces;
using BestStories.Api.Core.Models;

namespace BestStories.Api.Tests.Helpers
{
    public class MockBestStoriesApiService : IBestStoriesApiService
    {
        private readonly IEnumerable<int> _bestStoryIds;
        private readonly IEnumerable<Story> _stories;

        public MockBestStoriesApiService() 
        {
            _bestStoryIds = DataUtility.GetBestStoryIds();
            _stories = DataUtility.GetBestStories();
        }

        public Task<IEnumerable<int>> GetBestStoriesAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<int>>(_bestStoryIds);
        }

        public Task<Story> GetStoryAsync(int id, CancellationToken cancellationToken)
        {
            return Task.FromResult<Story>(_stories.First(s => s.id == id));
        }
    }
}
