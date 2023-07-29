using BestStoriesApi.Interfaces;
using BestStoriesApi.Models;

namespace BestStoriesApi.Services
{
    public class BestStoriesBackgroundService : BackgroundService
    {
        private readonly IBestStoriesCache _bestStoriesCache;
        private readonly IBestStoriesApiService _bestStoriesApiService;
        private readonly ILogger<BestStoriesBackgroundService> _logger;
        private readonly int _cacheMaxSize;
        private readonly int _cacheRecycleDelay;

        public BestStoriesBackgroundService(
            IBestStoriesCache bestStoriesCache,
            IBestStoriesApiService bestStoriesApiService, 
            ILogger<BestStoriesBackgroundService> logger,
            IConfiguration configuration)
        {
            _bestStoriesCache = bestStoriesCache;
            _bestStoriesApiService = bestStoriesApiService;
            _logger = logger;

            _cacheMaxSize = configuration.GetValue<int>("BestStories:CacheMaxSize");
            _cacheRecycleDelay = configuration.GetValue<int>("BestStories:CacheRecycleDelay");
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

                    _bestStoriesCache.RecycleCache(
                        newStoryCache.OrderByDescending(s => s.score).Take(_cacheMaxSize).ToList());
                }
                catch (Exception ex) 
                {
                    _logger.LogError(ex, $"ExecuteAsync()");
                }

                await Task.Delay(_cacheRecycleDelay, cancellationToken);
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
