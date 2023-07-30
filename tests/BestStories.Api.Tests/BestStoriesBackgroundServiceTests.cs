using BestStories.Api.Cache;
using BestStories.Api.Core.Models;
using BestStories.Api.Services;
using BestStories.Api.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

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
        public async Task ExecuteAsync_Cache_Recycle_Pass()
        {
            // Arrange
            BestStoriesBackgroundService bestStoriesBackgroundService
                = new(_storiesCache, new MockBestStoriesApiService(), _bestStoriesConfiguration, _logger);

            // Act
            await bestStoriesBackgroundService.StartAsync(CancellationToken.None);

            await Task.Delay(1000);

            await bestStoriesBackgroundService.StopAsync(CancellationToken.None);

            //Assert
            IEnumerable<Story>? cache = await _storiesCache.GetStoryCacheAsync();

            IEnumerable<Story> stories = DataUtility.GetBestStories();

            Assert.IsNotNull(cache);
            Assert.AreEqual(200, cache.Count());
            Assert.IsTrue(AssertHelper.AreStoriesEqual(cache, stories.OrderByDescending(s => s.score)));
        }
    }
}
