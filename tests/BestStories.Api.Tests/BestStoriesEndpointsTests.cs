using BestStories.Api.Cache;
using BestStories.Api.Core.Interfaces;
using BestStories.Api.Core.Models;
using BestStories.Api.Endpoints;
using BestStories.Api.Services;
using BestStories.Api.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace BestStories.Api.Tests
{
    [TestClass]
    public class BestStoriesEndpointsTests
    {
        private readonly ILogger<BestStoriesService> _logger;
        private readonly IConfiguration _configuration;

        public BestStoriesEndpointsTests()
        {
            ILoggerFactory factory = new NullLoggerFactory();

            _logger = factory.CreateLogger<BestStoriesService>();

            Dictionary<string, string?> configSettings = new()
            {
                {"BestStories:CacheRetryDelay", "100"},
                {"BestStories:CacheMaxRetryAttempts", "5"}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configSettings)
                .Build();
        }

        [TestMethod]
        public async Task GetBestStories_Return_Ok()
        {
            // Arrange
            IBestStoriesCache bestStoriesCache = new BestStoriesLockedCache();

            bestStoriesCache.RecycleCache(DataUtility.GetBestStories());

            IBestStoriesService bestStoriesService = new BestStoriesService(
                bestStoriesCache, _logger, _configuration);

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
