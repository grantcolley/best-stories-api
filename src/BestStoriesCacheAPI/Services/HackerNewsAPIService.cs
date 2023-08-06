using BestStories.Core.Models;
using BestStories.Core.Static;
using BestStoriesCacheAPI.Interfaces;
using System.Text.Json;

namespace BestStoriesCacheAPI.Services
{
    /// <summary>
    /// The <see cref="HackerNewsAPIService"/> is responsible
    /// for sending requests to the HackerNewsAPI to fetch 
    /// best story IDs and the stories by the their ID. 
    /// </summary>
    internal class HackerNewsAPIService : IHackerNewsAPIService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HackerNewsAPIService> _logger;

        public HackerNewsAPIService(IHttpClientFactory httpClientFactory, ILogger<HackerNewsAPIService> logger) 
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Fetches the best stories from HackerNewsAPI:
        ///     - first fetch the IDs of the best stories from the HackerNewsAPI
        ///     - second, fetch each story from the HackerNewsApi.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>The latest best stories from HackerNewsAPI</returns>
        public async Task<IEnumerable<Story>> GetBestStoryiesAsync(CancellationToken cancellationToken)
        {
            IEnumerable<int>? currentBestIds = await GetBestStoryIdsAsync(cancellationToken)
                .ConfigureAwait(false);

            if (cancellationToken.IsCancellationRequested)
            {
                return await Task.FromCanceled<IEnumerable<Story>>(cancellationToken); ;
            }

            return await GetStoriesByIdAsync(currentBestIds, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Get the IDs of the best stories from HackerNewsAPI.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        private async Task<IEnumerable<int>> GetBestStoryIdsAsync(CancellationToken cancellationToken)
        {
            try
            {
                using HttpClient httpClient = _httpClientFactory.CreateClient(Constants.HACKER_NEWS);

                using HttpResponseMessage response = await httpClient.GetAsync("beststories.json", cancellationToken)
                    .ConfigureAwait(false);

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

        /// <summary>
        /// Get each story in the collection by ID from HackerNewsApi.
        /// </summary>
        /// <param name="bestIds">The IDs of the best stories from fetch.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns latest best stories from HackerNewsAPI</returns>
        private async Task<IEnumerable<Story>> GetStoriesByIdAsync(IEnumerable<int> bestIds, CancellationToken cancellationToken)
        {
            Task<Story>[] bestStories = bestIds.Select(id =>
            {
                return GetStoryAsync(id, cancellationToken);
            }).ToArray();

            try
            {
                return await Task.WhenAll(bestStories);
            }
            catch (Exception ex)
            {
                var aggregateExceptions = bestStories.Where(t => t.Exception != null).Select(t => t.Exception);

                if (aggregateExceptions.Any())
                {
                    foreach (AggregateException? aggregateException in aggregateExceptions)
                    {
                        aggregateException?.Handle((x) =>
                            {
                                _logger.LogError(x, x.Message);
                                return true;
                            });
                    }
                }

                return await Task.FromException<IEnumerable<Story>>(ex);
            }
        }

        /// <summary>
        /// Get the story by ID from HackerNewsAPI.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        private async Task<Story> GetStoryAsync(int id, CancellationToken cancellationToken)
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