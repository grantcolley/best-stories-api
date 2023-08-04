using BestStories.Api.Cache;
using BestStories.Api.Core.Interfaces;
using BestStories.Api.Core.Models;
using BestStories.Api.Services;
using BestStories.Api.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace BestStories.Api.Tests
{
    [TestClass]
    public class BestStoriesBackgroundServiceTests
    {
        private readonly SemaphoreSlimCache _storiesCache;
        private readonly IOptions<BestStoriesConfiguration> _bestStoriesConfiguration;
        private readonly ILogger<BestStoriesBackgroundService> _logger;

        public BestStoriesBackgroundServiceTests()
        {
            ILoggerFactory factory = new NullLoggerFactory();
            _logger = factory.CreateLogger<BestStoriesBackgroundService>();

            ILogger<SemaphoreSlimCache> semaphoreSlimCacheLogger = factory.CreateLogger<SemaphoreSlimCache>();
            _storiesCache = new SemaphoreSlimCache(semaphoreSlimCacheLogger);

            _bestStoriesConfiguration = Options.Create(
                new BestStoriesConfiguration { CacheMaxSize = 200, CacheRecycleDelay = 5000 });
        }

        /// <summary>
        /// Tests the execution of the BestStoriesBackgroundService to ensure it populates the cache.
        /// </summary>
        [TestMethod]        
        public async Task ExecuteAsync_Cache_Recycle_Successful()
        {
            // Arrange
            Mock<IBestStoriesApiService> mockBestStoriesApiService = new();

            mockBestStoriesApiService.Setup(
                s => s.GetBestStoriesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(DataUtility.GetBestStoryIds()));

            mockBestStoriesApiService.Setup(
                s => s.GetStoryAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new Story()));

            BestStoriesBackgroundService bestStoriesBackgroundService
                = new(_storiesCache, mockBestStoriesApiService.Object, _bestStoriesConfiguration, _logger);

            // Act
            await bestStoriesBackgroundService.StartAsync(CancellationToken.None);

            await bestStoriesBackgroundService.StopAsync(CancellationToken.None);

            //Assert
            mockBestStoriesApiService.Verify(s => s.GetBestStoriesAsync(It.IsAny<CancellationToken>()));
            mockBestStoriesApiService.Verify(s => s.GetStoryAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()));

            IEnumerable<Story>? cache = await _storiesCache.GetStoryCacheAsync();

            Assert.IsNotNull(cache);
            Assert.AreEqual(200, cache.Count());
            Assert.AreEqual(201, mockBestStoriesApiService.Invocations.Count);
        }
    }
}
