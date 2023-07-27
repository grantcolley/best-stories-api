using BestStoriesApi.Interfaces;
using BestStoriesApi.Models;
using BestStoriesApi.Static;
using System.Text.Json;

namespace BestStoriesApi.Services
{
    public class BestStoriesApiService : IBestStoriesApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;

        public BestStoriesApiService(IHttpClientFactory httpClientFactory, ILogger<BestStoriesApiService> logger) 
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<int>> GetBestStoriesAsync(CancellationToken cancellationToken)
        {
            try
            {
                using HttpClient httpClient = _httpClientFactory.CreateClient(HttpClientNames.HACKER_NEWS);

                using HttpResponseMessage response = await httpClient.GetAsync("beststories.json", cancellationToken);

                return await JsonSerializer.DeserializeAsync<IEnumerable<int>>(
                    await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false), 
                    JsonSerializerOptions.Default, cancellationToken)
                    .ConfigureAwait(false) ?? throw new NullReferenceException();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetBestStoriesAsync()");

                return await Task.FromException<IEnumerable<int>>(ex);
            }
        }

        public async Task<Story> GetStoryAsync(int id, CancellationToken cancellationToken)
        {
            try
            {
                using HttpClient httpClient = _httpClientFactory.CreateClient(HttpClientNames.HACKER_NEWS);
                
                using HttpResponseMessage response = await httpClient.GetAsync($"item/{id}.json", cancellationToken);

                return await JsonSerializer.DeserializeAsync<Story>(
                    await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false),
                    JsonSerializerOptions.Default, cancellationToken).ConfigureAwait(false) ?? throw new NullReferenceException();
            }
            catch (Exception ex)
            {
                string message = $"GetStoryAsync({id})";
                _logger.LogError(ex, message);

                return await Task.FromException<Story>(ex);
            }
        }
    }
}