using BestStoriesApi.Cache;
using BestStoriesApi.Exceptions;
using BestStoriesApi.Interfaces;
using BestStoriesApi.Models;
using BestStoriesApi.Services;
using BestStoriesApi.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace BestStoriesApi.Tests
{
    [TestClass]
    public class BestStoriesServiceTests
    {
        private readonly ILogger<BestStoriesService> _logger;
        private readonly IConfiguration _configuration;

        public BestStoriesServiceTests()
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
        public async Task GetBestStoriesAsync_Top_5_Pass()
        {
            // Arrange
            IBestStoriesCache bestStoriesCache = new BestStoriesLockedCache();

            bestStoriesCache.RecycleCache(DataUtility.GetBestStories());

            IBestStoriesService bestStoriesService = new BestStoriesService(
                bestStoriesCache, _logger, _configuration);

            // Act
            IEnumerable<Story> bestStories = await bestStoriesService.GetBestStoriesAsync(5, CancellationToken.None)
                .ConfigureAwait(false);

            //Assert
            IEnumerable<Story> stories = DataUtility.GetBestStories();

            Assert.AreEqual(5, bestStories.Count());
            Assert.IsTrue(AssertHelper.AreStoriesEqual(bestStories, stories.OrderByDescending(s => s.score).Take(5)));
        }

        [TestMethod]
        [ExpectedException(typeof(BestStoryException), $"Exceeded max retry attempts 5.")]
        public async Task GetBestStoriesAsync_MaxRetryAttempts_Fail_ExpectedException()
        {
            // Arrange
            IBestStoriesCache bestStoriesCache = new BestStoriesLockedCache();

            IBestStoriesService bestStoriesService = new BestStoriesService(
                bestStoriesCache, _logger, _configuration);

            // Act
            _ = await bestStoriesService.GetBestStoriesAsync(5, CancellationToken.None).ConfigureAwait(false);

            //Assert
            Assert.Fail();
        }

        [TestMethod]
        public async Task GetBestStoriesAsync_RetryAttempts_Pass_On_Fourth_Attempt()
        {
            // Arrange
            IBestStoriesCache bestStoriesCache = new MockBestStoriesCacheRetryAttempts();

            bestStoriesCache.RecycleCache(DataUtility.GetBestStories());

            IBestStoriesService bestStoriesService = new BestStoriesService(
                bestStoriesCache, _logger, _configuration);

            // Act
            IEnumerable<Story> bestStories = await bestStoriesService.GetBestStoriesAsync(5, CancellationToken.None).ConfigureAwait(false);

            //Assert
            IEnumerable<Story> stories = DataUtility.GetBestStories();

            Assert.AreEqual(5, bestStories.Count());
            Assert.IsTrue(AssertHelper.AreStoriesEqual(bestStories, stories.OrderByDescending(s => s.score).Take(5)));
        }
    }
}