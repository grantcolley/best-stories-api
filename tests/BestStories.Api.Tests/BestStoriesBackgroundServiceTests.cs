using BestStories.Api.Cache;
using BestStories.Api.Core.Interfaces;
using BestStories.Api.Core.Models;
using BestStories.Api.Services;
using BestStories.Api.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace BestStories.Api.Tests
{
    [TestClass]
    public class BestStoriesBackgroundServiceTests
    {
        private readonly ILogger<BestStoriesBackgroundService> _logger;

        public BestStoriesBackgroundServiceTests()
        {
            ILoggerFactory factory = new NullLoggerFactory();

            _logger = factory.CreateLogger<BestStoriesBackgroundService>();
        }

        [TestMethod]
        public async Task ExecuteAsync_Initial_Cache_Recycle_Pass()
        {
            // Arrange
            IBestStoriesCache bestStoriesCache = new BestStoriesLockedCache();

            Dictionary<string, string?> configSettings = new()
            {
                {"BestStories:CacheMaxSize", "200"},
                {"BestStories:CacheRecycleDelay", "5000"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configSettings)
                .Build();

            BestStoriesBackgroundService bestStoriesBackgroundService
                = new(bestStoriesCache, new MockBestStoriesApiService(), _logger, configuration);

            // Act
            await bestStoriesBackgroundService.StartAsync(CancellationToken.None);

            await Task.Delay(500);

            await bestStoriesBackgroundService.StopAsync(CancellationToken.None);

            //Assert
            IEnumerable<Story>? cache = bestStoriesCache.GetStoryCache();

            IEnumerable<Story> stories = DataUtility.GetBestStories();

            Assert.IsNotNull(cache);
            Assert.AreEqual(200, cache.Count());
            Assert.IsTrue(AssertHelper.AreStoriesEqual(cache, stories.OrderByDescending(s => s.score)));
        }
    }
}
