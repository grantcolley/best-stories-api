using BestStories.Api.Core.Interfaces;
using BestStories.Api.Core.Models;
using Microsoft.Extensions.Options;

namespace BestStories.Api.Services
{
    public class BestStoriesBackgroundService : BackgroundService
    {
        private readonly IBestStoriesCache _bestStoriesCache;
        private readonly IBestStoriesApiService _bestStoriesApiService;
        private readonly ILogger<BestStoriesBackgroundService> _logger;
        private readonly BestStoriesConfiguration _bestStoriesConfiguration;

        public BestStoriesBackgroundService(
            IBestStoriesCache bestStoriesCache,
            IBestStoriesApiService bestStoriesApiService, 
            IOptions<BestStoriesConfiguration> bestStoriesConfiguration,
            ILogger<BestStoriesBackgroundService> logger)
        {
            _bestStoriesCache = bestStoriesCache ?? throw new ArgumentNullException(nameof(bestStoriesCache));
            _bestStoriesApiService = bestStoriesApiService ?? throw new ArgumentNullException(nameof(bestStoriesApiService));
            _bestStoriesConfiguration = bestStoriesConfiguration?.Value ?? throw new ArgumentNullException(nameof(bestStoriesConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested) 
            {
                try
                {
                    IEnumerable<int>? currentBestIds = await _bestStoriesApiService.GetBestStoriesAsync(cancellationToken)
                        .ConfigureAwait(false);

                    if(cancellationToken.IsCancellationRequested) 
                    {
                        return;
                    }

                    IEnumerable<Story>? newStoryCache = await GetStories(currentBestIds, cancellationToken)
                        .ConfigureAwait(false);

                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    await _bestStoriesCache.RecycleCacheAsync(
                        newStoryCache
                        .OrderByDescending(s => s.score)
                        .Take(_bestStoriesConfiguration.CacheMaxSize)
                        .ToList())
                        .ConfigureAwait(false);
                }
                catch (Exception ex) 
                {
                    _logger.LogError(ex, ex.Message);
                }

                await Task.Delay(_bestStoriesConfiguration.CacheRecycleDelay, cancellationToken);
            }
        }

        private async Task<IEnumerable<Story>> GetStories(IEnumerable<int> bestIds, CancellationToken cancellationToken)
        {
            Task<Story>[] bestStories = bestIds.Select(id =>
            {
                return _bestStoriesApiService.GetStoryAsync(id, cancellationToken);
            }).ToArray();

            Story[] stories = await Task.WhenAll(bestStories);

            return stories;
        }
    }
}
