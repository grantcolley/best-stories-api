using BestStories.Core.Static;
using BestStoriesAPI.Filters;
using BestStoriesAPI.Models;
using BestStoriesAPI.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Moq;

namespace BestStoriesAPI.Tests
{
    /// <summary>
    /// Tests the <see cref="BestStoriesValidationFilter"/>.
    /// </summary>
    [TestClass]
    public class BestStoriesValidationFilterTests
    {
        private const string successMessage = "Success!";
        private const string errorMessage = $"Specify number of best stories to fetch between 1 and 200";

        private readonly IOptions<BestStoriesConfiguration> _bestStoriesConfiguration;
        private readonly Mock<IDistributedCache> _mockDistributedCache;

        public BestStoriesValidationFilterTests()
        {
            _bestStoriesConfiguration = Options.Create(new BestStoriesConfiguration { DefaultCacheMaxSize = 200 });

            _mockDistributedCache = new();
            _mockDistributedCache.Setup(
                s => s.GetAsync(Constants.DISTRIBUTED_CACHE_MAX_SIZE, CancellationToken.None))
                .Returns(Task.FromResult<byte[]?>(BitConverter.GetBytes(200)));
        }

        /// <summary>
        /// Returns a Status200OK after the validating the parameter is within the permissible range.
        /// </summary>
        [TestMethod]
        public async Task BestStoriesValidationFilter_Return_Status200OK()
        {
            // Arrange
            MockEndpointFilterInvocationContext mockEndpointFilterInvocationContext = new();

            mockEndpointFilterInvocationContext.Arguments.Add(5);

            BestStoriesValidationFilter bestStoriesValidationFilter = new(_bestStoriesConfiguration, _mockDistributedCache.Object);

            // Act
            var resultObject = await bestStoriesValidationFilter.InvokeAsync(mockEndpointFilterInvocationContext, EndpointFilterDelegate)
                .ConfigureAwait(false);

            var result = resultObject as Ok<string>;

            // Assert
            _mockDistributedCache.Verify(s => s.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(successMessage, result.Value);
        }

        /// <summary>
        /// Returns a Status200OK after the validating the parameter is within the permissible 
        /// range as defined in the BestStoriesConfiguration.DefaultCacheMaxSize.
        /// </summary>
        [TestMethod]
        public async Task BestStoriesValidationFilter_Use_DefaultMaxCacheSize_Return_Status200OK()
        {
            // Arrange
            Mock<IDistributedCache> mockLocalDistributedCache = new();
            mockLocalDistributedCache.Setup(
                s => s.GetAsync(Constants.DISTRIBUTED_CACHE_MAX_SIZE, CancellationToken.None))
                .Returns(Task.FromResult<byte[]?>(null));

            MockEndpointFilterInvocationContext mockEndpointFilterInvocationContext = new();

            mockEndpointFilterInvocationContext.Arguments.Add(5);

            BestStoriesValidationFilter bestStoriesValidationFilter = new(_bestStoriesConfiguration, mockLocalDistributedCache.Object);

            // Act
            var resultObject = await bestStoriesValidationFilter.InvokeAsync(mockEndpointFilterInvocationContext, EndpointFilterDelegate)
                .ConfigureAwait(false);

            var result = resultObject as Ok<string>;

            // Assert
            mockLocalDistributedCache.Verify(s => s.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(successMessage, result.Value);
        }

        /// <summary>
        /// Returns a Status400BadRequest when no parameter is provided.
        /// </summary>
        [TestMethod]
        public async Task BestStoriesValidationFilter_No_Parameter_Return_Status400BadRequest()
        {
            // Arrange
            MockEndpointFilterInvocationContext mockEndpointFilterInvocationContext = new();

            BestStoriesValidationFilter bestStoriesValidationFilter = new(_bestStoriesConfiguration, _mockDistributedCache.Object);

            // Act
            var resultObject = await bestStoriesValidationFilter.InvokeAsync(mockEndpointFilterInvocationContext, EndpointFilterDelegate)
                .ConfigureAwait(false);

            var result = resultObject as BadRequest<string>;

            // Assert
            _mockDistributedCache.Verify(s => s.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            Assert.AreEqual(errorMessage, result.Value);
        }

        /// <summary>
        /// Returns a Status400BadRequest after the validating the parameter is outside the permissible range.
        /// </summary>
        [TestMethod]
        public async Task BestStoriesValidationFilter_Parameter_Equals_0_Return_Status400BadRequest()
        {
            // Arrange
            MockEndpointFilterInvocationContext mockEndpointFilterInvocationContext = new();

            mockEndpointFilterInvocationContext.Arguments.Add(0);

            BestStoriesValidationFilter bestStoriesValidationFilter = new(_bestStoriesConfiguration, _mockDistributedCache.Object);

            // Act
            var resultObject = await bestStoriesValidationFilter.InvokeAsync(mockEndpointFilterInvocationContext, EndpointFilterDelegate)
                .ConfigureAwait(false);

            var result = resultObject as BadRequest<string>;

            // Assert
            _mockDistributedCache.Verify(s => s.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            Assert.AreEqual(errorMessage, result.Value);
        }

        /// <summary>
        /// Returns a Status400BadRequest after the validating the parameter is outside the permissible range.
        /// </summary>
        [TestMethod]
        public async Task BestStoriesValidationFilter_Parameter_Greater_Than_CacheMaxSize_Return_Status400BadRequest()
        {
            // Arrange
            MockEndpointFilterInvocationContext mockEndpointFilterInvocationContext = new();

            mockEndpointFilterInvocationContext.Arguments.Add(201);

            BestStoriesValidationFilter bestStoriesValidationFilter = new(_bestStoriesConfiguration, _mockDistributedCache.Object);

            // Act
            var resultObject = await bestStoriesValidationFilter.InvokeAsync(mockEndpointFilterInvocationContext, EndpointFilterDelegate)
                .ConfigureAwait(false);

            var result = resultObject as BadRequest<string>;

            // Assert
            _mockDistributedCache.Verify(s => s.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            Assert.AreEqual(errorMessage, result.Value);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async ValueTask<object?> EndpointFilterDelegate(EndpointFilterInvocationContext context)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            return Results.Ok(successMessage);
        }
    }
}