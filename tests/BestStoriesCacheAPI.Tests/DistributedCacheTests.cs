using BestStories.Core.Models;
using BestStories.Core.Static;
using BestStoriesAPI.Tests.Helpers;
using BestStoriesCacheAPI.Cache;
using BestStoriesCacheAPI.Interfaces;
using BestStoriesCacheAPI.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace BestStoriesAPI.Tests
{
    /// <summary>
    /// Tests the <see cref="DistributedCache"/>.
    /// </summary>
    [TestClass]
    public class DistributedCacheTests
    {
        private readonly IOptions<BestStoriesCacheConfiguration> _bestStoriesCacheConfiguration;
        private readonly ILogger<DistributedCache> _logger;

        public DistributedCacheTests() 
        {
            _bestStoriesCacheConfiguration = Options.Create(
                new BestStoriesCacheConfiguration { CacheMaxSize = 200, CacheExpiryInSeconds = 5 });

            ILoggerFactory factory = new NullLoggerFactory();

            _logger = factory.CreateLogger<DistributedCache>();
        }

        /// <summary>
        /// Returns 200 stories from the cache.
        /// </summary>
        [TestMethod]
        public async Task Return_200_Stories_From_DistributedCache()
        {
            // Arrange
            Mock<IDistributedCache> mockDistributedCache = new();

            mockDistributedCache.Setup(
                s => s.GetAsync(Constants.DISTRIBUTED_CACHE_BEST_STORIES, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(DataUtility.GetBestStoriesAsByteArray()));

            Mock<IHackerNewsAPIService> mockHackerNewsAPIService = new();

            DistributedCache distributedCache = new(mockDistributedCache.Object, mockHackerNewsAPIService.Object, _bestStoriesCacheConfiguration, _logger);

            // Act
            IEnumerable<Story>? stories = await distributedCache.GetStoryCacheAsync(CancellationToken.None);

            // Assert
            mockDistributedCache.Verify(s => s.GetAsync(Constants.DISTRIBUTED_CACHE_BEST_STORIES, It.IsAny<CancellationToken>()), Times.Once);

            Assert.IsNotNull(stories);
            Assert.AreEqual(200, stories.Count());
        }

        /// <summary>
        /// Returns 200 stories from the cache during the double check after entering the semaphore.
        /// </summary>
        [TestMethod]
        public async Task Return_200_Stories_From_Double_Checking_The_DistributedCache_After_Entering_The_Semaphore()
        {
            // Arrange
            Mock<IDistributedCache> mockDistributedCache = new();

            mockDistributedCache.SetupSequence(
                s => s.GetAsync(Constants.DISTRIBUTED_CACHE_BEST_STORIES, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<byte[]?>(null))
                .Returns(Task.FromResult(DataUtility.GetBestStoriesAsByteArray()));

            Mock<IHackerNewsAPIService> mockHackerNewsAPIService = new();

            DistributedCache distributedCache = new(mockDistributedCache.Object, mockHackerNewsAPIService.Object, _bestStoriesCacheConfiguration, _logger);

            // Act
            IEnumerable<Story>? stories = await distributedCache.GetStoryCacheAsync(CancellationToken.None);

            // Assert
            mockDistributedCache.Verify(s => s.GetAsync(Constants.DISTRIBUTED_CACHE_BEST_STORIES, It.IsAny<CancellationToken>()), Times.Exactly(2));

            Assert.IsNotNull(stories);
            Assert.AreEqual(200, stories.Count());
        }

        /// <summary>
        /// Returns 200 stories from the cache after re-biuiling and recycling it.
        /// </summary>
        [TestMethod]
        public async Task Return_200_Stories_After_Rebuilding_And_Recycling_The_Cache()
        {
            // Arrange
            Mock<IDistributedCache> mockDistributedCache = new();

            mockDistributedCache.Setup(
                s => s.GetAsync(Constants.DISTRIBUTED_CACHE_BEST_STORIES, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<byte[]?>(null));

            Mock<IHackerNewsAPIService> mockHackerNewsAPIService = new();

            mockHackerNewsAPIService.Setup(
                s => s.GetBestStoryiesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(DataUtility.GetBestStories()));

            DistributedCache distributedCache = new(mockDistributedCache.Object, mockHackerNewsAPIService.Object, _bestStoriesCacheConfiguration, _logger);

            // Act
            IEnumerable<Story>? stories = await distributedCache.GetStoryCacheAsync(CancellationToken.None);

            // Assert
            mockDistributedCache.Verify(s => s.GetAsync(Constants.DISTRIBUTED_CACHE_BEST_STORIES, It.IsAny<CancellationToken>()), Times.Exactly(2));
            mockHackerNewsAPIService.Verify(s => s.GetBestStoryiesAsync(It.IsAny<CancellationToken>()), Times.Exactly(1));

            Assert.IsNotNull(stories);
            Assert.AreEqual(200, stories.Count());
        }
    }
}
