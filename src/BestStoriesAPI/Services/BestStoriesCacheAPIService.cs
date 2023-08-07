using BestStories.Core.Models;
using BestStories.Core.Static;
using BestStoriesAPI.Interfaces;
using System.Text.Json;

namespace BestStoriesAPI.Services
{
    internal class BestStoriesCacheAPIService : IBestStoriesCacheAPIService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<BestStoriesCacheAPIService> _logger;

        public BestStoriesCacheAPIService(IHttpClientFactory httpClientFactory, ILogger<BestStoriesCacheAPIService> logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<Story>> GetBestStoriesAsync(int count, CancellationToken cancellationToken)
        {
            try
            {
                using HttpClient httpClient = _httpClientFactory.CreateClient(Constants.BEST_STORIES_CACHE_API);

                using HttpResponseMessage response = await httpClient.GetAsync($"recyclecachedstories/{count}", cancellationToken)
                    .ConfigureAwait(false);

                return await JsonSerializer.DeserializeAsync<IEnumerable<Story>>(
                    await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false),
                    JsonSerializerOptions.Default, cancellationToken)
                    .ConfigureAwait(false) ?? throw new NullReferenceException();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetBestStoriesAsync({count})");

                return await Task.FromException<IEnumerable<Story>>(ex);
            }
        }
    }
}
