using BestStories.Api.Cache;
using BestStories.Api.Core.Exceptions;
using BestStories.Api.Core.Interfaces;
using BestStories.Api.Core.Models;
using BestStories.Api.Services;
using BestStories.Api.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BestStories.Api.Tests
{
    [TestClass]
    public class BestStoriesServiceTests
    {
        private readonly SemaphoreSlimCache _storiesCache;
        private readonly IOptions<BestStoriesConfiguration> _bestStoriesConfiguration;
        private readonly ILogger<BestStoriesService> _logger;

        public BestStoriesServiceTests()
        {
            _bestStoriesConfiguration = Options.Create(
                new BestStoriesConfiguration { CacheMaxSize = 200, CacheRetryDelay = 100, CacheMaxRetryAttempts = 5 });

            ILoggerFactory factory = new NullLoggerFactory();

            _logger = factory.CreateLogger<BestStoriesService>();

            ILogger<SemaphoreSlimCache> logger = factory.CreateLogger<SemaphoreSlimCache>();
            _storiesCache = new SemaphoreSlimCache(logger);
        }

        /// <summary>
        /// Tests the BestStoriesService returns the top 5 best stories from the cache.
        /// </summary>
        [TestMethod]
        public async Task GetBestStoriesAsync_Return_Top_5_Stories()
        {
            // Arrange
            await _storiesCache.RecycleCacheAsync(DataUtility.GetBestStories());

            IBestStoriesService bestStoriesService = new BestStoriesService(
                _storiesCache, _bestStoriesConfiguration, _logger);

            // Act
            IEnumerable<Story> bestStories = await bestStoriesService.GetBestStoriesAsync(5, CancellationToken.None)
                .ConfigureAwait(false);

            //Assert
            IEnumerable<Story> stories = DataUtility.GetBestStories();

            Assert.AreEqual(5, bestStories.Count());
            Assert.IsTrue(AssertHelper.AreStoriesEqual(bestStories, stories.OrderByDescending(s => s.score).Take(5)));
        }

        /// <summary>
        /// Tests the BestStoriesService exceed's its max retry attempts because the cache is empty.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(BestStoryException), $"Exceeded max retry attempts 5.")]
        public async Task GetBestStoriesAsync_MaxRetryAttempts_ExpectedException()
        {
            // Arrange
            IBestStoriesService bestStoriesService = new BestStoriesService(
                _storiesCache, _bestStoriesConfiguration, _logger);

            // Act
            _ = await bestStoriesService.GetBestStoriesAsync(5, CancellationToken.None).ConfigureAwait(false);

            //Assert
            Assert.Fail();
        }

        /// <summary>
        /// Tests the BestStoriesService successfully returns the top 5 best stories from
        /// the cache after four attempts, before exceeding it's max retry attempts.
        /// </summary>
        [TestMethod]
        public async Task GetBestStoriesAsync_RetryAttempts_Pass_On_Fourth_Attempt()
        {
            // Arrange
            IBestStoriesCache storiesCache = new MockBestStoriesCacheRetryAttempts();

            await storiesCache.RecycleCacheAsync(DataUtility.GetBestStories());

            IBestStoriesService bestStoriesService = new BestStoriesService(
                storiesCache, _bestStoriesConfiguration, _logger);

            // Act
            IEnumerable<Story> bestStories = await bestStoriesService.GetBestStoriesAsync(5, CancellationToken.None).ConfigureAwait(false);

            //Assert
            IEnumerable<Story> stories = DataUtility.GetBestStories();

            Assert.AreEqual(5, bestStories.Count());
            Assert.IsTrue(AssertHelper.AreStoriesEqual(bestStories, stories.OrderByDescending(s => s.score).Take(5)));
        }
    }
}