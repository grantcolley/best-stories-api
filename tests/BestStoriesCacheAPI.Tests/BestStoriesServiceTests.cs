using BestStories.Core.Models;
using BestStoriesAPI.Tests.Helpers;
using BestStoriesCacheAPI.Interfaces;
using BestStoriesCacheAPI.Models;
using BestStoriesCacheAPI.Services;
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
        private readonly ILogger<BestStoriesCacheService> _logger;

        public BestStoriesServiceTests()
        {
            ILoggerFactory factory = new NullLoggerFactory();

            _logger = factory.CreateLogger<BestStoriesCacheService>();
        }

        /// <summary>
        /// Return the top 5 stories from the cache.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task BestStoriesService_Return_Top_5_Stories()
        {
            // Arrange
            Mock<IBestStoriesCache> mockBestStoriesCache = new();
            mockBestStoriesCache.Setup(
                s => s.GetStoryCacheAsync(CancellationToken.None))
                .Returns(Task.FromResult<IEnumerable<Story>?>(DataUtility.GetBestStories()));

            BestStoriesCacheService bestStoriesCacheService = new(mockBestStoriesCache.Object, _logger);

            // Act
            IEnumerable<Story>? top5BestStories = await bestStoriesCacheService.GetBestStoriesAsync(5, CancellationToken.None)
                .ConfigureAwait(false);

            // Assert
            mockBestStoriesCache.Verify(s => s.GetStoryCacheAsync(It.IsAny<CancellationToken>()), Times.Once);

            IEnumerable<Story> stories = DataUtility.GetBestStories();

            Assert.IsNotNull(top5BestStories);
            Assert.AreEqual(5, top5BestStories.Count());
            Assert.IsTrue(AssertHelper.AreStoriesEqual(top5BestStories, stories.OrderByDescending(s => s.score).Take(5)));
        }

        /// <summary>
        /// Handle an exception thrown from inside _bestStoriesCache.GetStoryCacheAsync(cancellationToken).
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task BestStoriesService_Handle_ExpectedException()
        {
            // Arrange
            Mock<IBestStoriesCache> mockBestStoriesCache = new();
            mockBestStoriesCache.Setup(
                s => s.GetStoryCacheAsync(CancellationToken.None))
                .Throws<InvalidOperationException>();

            BestStoriesCacheService bestStoriesCacheService = new(mockBestStoriesCache.Object, _logger);

            // Act
            IEnumerable<Story>? top5BestStories = await bestStoriesCacheService.GetBestStoriesAsync(5, CancellationToken.None)
                .ConfigureAwait(false);

            //Assert
            Assert.Fail();
        }
    }
}
