using BestStories.Api.Core.Interfaces;
using BestStories.Api.Core.Models;
using BestStories.Api.Core.Static;
using System.Text.Json;

namespace BestStories.Api.Services
{
    public class BestStoriesApiService : IBestStoriesApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<BestStoriesApiService> _logger;

        public BestStoriesApiService(IHttpClientFactory httpClientFactory, ILogger<BestStoriesApiService> logger) 
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<int>> GetBestStoriesAsync(CancellationToken cancellationToken)
        {
            try
            {
                using HttpClient httpClient = _httpClientFactory.CreateClient(Constants.HACKER_NEWS);

                using HttpResponseMessage response = await httpClient.GetAsync("beststories.json", cancellationToken);

                return await JsonSerializer.DeserializeAsync<IEnumerable<int>>(
                    await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false), 
                    JsonSerializerOptions.Default, cancellationToken)
                    .ConfigureAwait(false) ?? throw new NullReferenceException();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                return await Task.FromException<IEnumerable<int>>(ex);
            }
        }

        public async Task<Story> GetStoryAsync(int id, CancellationToken cancellationToken)
        {
            try
            {
                using HttpClient httpClient = _httpClientFactory.CreateClient(Constants.HACKER_NEWS);
                
                using HttpResponseMessage response = await httpClient.GetAsync($"item/{id}.json", cancellationToken);

                return await JsonSerializer.DeserializeAsync<Story>(
                    await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false),
                    JsonSerializerOptions.Default, cancellationToken).ConfigureAwait(false) ?? throw new NullReferenceException();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetStoryAsync({id})");

                return await Task.FromException<Story>(ex);
            }
        }
    }
}