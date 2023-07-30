using BestStories.Api.Cache;
using BestStories.Api.Core.Interfaces;
using BestStories.Api.Core.Models;
using BestStories.Api.Endpoints;
using BestStories.Api.Services;
using BestStories.Api.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BestStories.Api.Tests
{
    [TestClass]
    public class BestStoriesEndpointsTests
    {
        private readonly SemaphoreSlimCache _storiesCache;
        private readonly IOptions<BestStoriesConfiguration> _bestStoriesConfiguration;
        private readonly ILogger<BestStoriesService> _logger;

        public BestStoriesEndpointsTests()
        {
            _bestStoriesConfiguration = Options.Create(
                new BestStoriesConfiguration { CacheMaxSize = 200, CacheRetryDelay = 100, CacheMaxRetryAttempts = 5 });

            ILoggerFactory factory = new NullLoggerFactory();

            _logger = factory.CreateLogger<BestStoriesService>();

            ILogger<SemaphoreSlimCache> logger = factory.CreateLogger<SemaphoreSlimCache>();
            _storiesCache = new SemaphoreSlimCache(logger);
        }

        /// <summary>
        /// Tests the BestStoriesEndpoint for a successful Status200OK result.
        /// </summary>
        [TestMethod]
        public async Task GetBestStories_Return_Status200OK()
        {
            // Arrange
            await _storiesCache.RecycleCacheAsync(DataUtility.GetBestStories());

            IBestStoriesService bestStoriesService = new BestStoriesService(
                _storiesCache, _bestStoriesConfiguration, _logger);

            // Act
            IResult resultObject = await BestStoriesEndpoint.GetBestStories(5, bestStoriesService, CancellationToken.None)
                .ConfigureAwait(false);

            //Assert
            var result = resultObject as Ok<IEnumerable<Story>>;

            IEnumerable<Story> stories = DataUtility.GetBestStories();

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(5, result.Value.Count());
            Assert.IsTrue(AssertHelper.AreStoriesEqual(result.Value, stories.OrderByDescending(s => s.score).Take(5)));
        }

        /// <summary>
        /// Tests the BestStoriesEndpoint for a Status500InternalServerError result.
        /// </summary>
        [TestMethod]
        public async Task GetBestStories_Return_Status500InternalServerError()
        {
            // Arrange
            IBestStoriesService bestStoriesService = new MockBadBestStoriesService();

            // Act
            IResult resultObject = await BestStoriesEndpoint.GetBestStories(5, bestStoriesService, CancellationToken.None).ConfigureAwait(false);

            // Assert
            var result = resultObject as StatusCodeHttpResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(500, result.StatusCode);
        }
    }
}
