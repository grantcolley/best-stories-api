using BestStories.Core.Models;
using BestStories.Core.Static;
using BestStoriesAPI.Interfaces;
using BestStoriesAPI.Models;
using BestStoriesAPI.Services;
using BestStoriesAPI.Tests.Helpers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace BestStoriesAPI.Tests
{
    /// <summary>
    /// Tests the <see cref="BestStoriesService"/>.
    /// </summary>
    [TestClass]
    public class BestStoriesServiceTests
    {
        private readonly ILogger<BestStoriesService> _logger;

        public BestStoriesServiceTests()
        {
            ILoggerFactory factory = new NullLoggerFactory();

            _logger = factory.CreateLogger<BestStoriesService>();
        }

        /// <summary>
        /// Return the top 5 stories from the cache.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task BestStoriesService_Return_Top_5_Stories_From_Distributed_Cache()
        {
            // Arrange
            Mock<IDistributedCache> mockDistributedCache = new();
            mockDistributedCache.Setup(
                s => s.GetAsync(Constants.DISTRIBUTED_CACHE_BEST_STORIES, CancellationToken.None))
                .Returns(Task.FromResult<byte[]?>(DataUtility.GetBestStoriesAsByteArray()));

            Mock<IBestStoriesCacheAPIService> mockBestStoriesCacheAPIService = new();

            BestStoriesService bestStoriesService = new(mockDistributedCache.Object, mockBestStoriesCacheAPIService.Object, _logger);

            // Act
            IEnumerable<Story>? top5BestStories = await bestStoriesService.GetBestStoriesAsync(5, CancellationToken.None)
                .ConfigureAwait(false);

            // Assert
            mockDistributedCache.Verify(s => s.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

            IEnumerable<Story> stories = DataUtility.GetBestStories();

            Assert.IsNotNull(top5BestStories);
            Assert.AreEqual(5, top5BestStories.Count());
            Assert.IsTrue(AssertHelper.AreStoriesEqual(top5BestStories, stories.OrderByDescending(s => s.score).Take(5)));
        }

        /// <summary>
        /// Return the top 5 stories from the BestStoriesCacheAPI.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task BestStoriesService_Return_Top_5_Stories_From_BestStoriesCacheAPI()
        {
            // Arrange
            Mock<IDistributedCache> mockDistributedCache = new();
            mockDistributedCache.Setup(
                s => s.GetAsync(Constants.DISTRIBUTED_CACHE_BEST_STORIES, CancellationToken.None))
                .Returns(Task.FromResult<byte[]?>(null));

            Mock<IBestStoriesCacheAPIService> mockBestStoriesCacheAPIService = new();

            mockBestStoriesCacheAPIService.Setup(
                s => s.GetBestStoriesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(DataUtility.GetBestStories().Take(5)));

            BestStoriesService bestStoriesService = new(mockDistributedCache.Object, mockBestStoriesCacheAPIService.Object, _logger);

            // Act
            IEnumerable<Story>? top5BestStories = await bestStoriesService.GetBestStoriesAsync(5, CancellationToken.None)
                .ConfigureAwait(false);

            // Assert
            mockDistributedCache.Verify(s => s.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

            IEnumerable<Story> stories = DataUtility.GetBestStories();

            Assert.IsNotNull(top5BestStories);
            Assert.AreEqual(5, top5BestStories.Count());
            Assert.IsTrue(AssertHelper.AreStoriesEqual(top5BestStories, stories.OrderByDescending(s => s.score).Take(5)));
        }

        /// <summary>
        /// Handle an exception thrown from inside _bestStoriesCacheAPIService.GetBestStoryiesAsync(count, cancellationToken).
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task BestStoriesService_Handle_ExpectedException()
        {
            // Arrange
            Mock<IDistributedCache> mockDistributedCache = new();

            Mock<IBestStoriesCacheAPIService> mockBestStoriesCacheAPIService = new();

            mockBestStoriesCacheAPIService.Setup(
                s => s.GetBestStoriesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Throws<InvalidOperationException>();

            BestStoriesService bestStoriesService = new(mockDistributedCache.Object, mockBestStoriesCacheAPIService.Object, _logger);

            // Act
            IEnumerable<Story>? top5BestStories = await bestStoriesService.GetBestStoriesAsync(5, CancellationToken.None)
                .ConfigureAwait(false);

            //Assert
            mockBestStoriesCacheAPIService.Verify(s => s.GetBestStoriesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);

            Assert.Fail();
        }
    }
}
