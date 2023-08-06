using BestStories.Core.Models;
using BestStoriesAPI.Tests.Helpers;
using BestStoriesCacheAPI.Endpoints;
using BestStoriesCacheAPI.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace BestStoriesAPI.Tests
{
    /// <summary>
    /// Tests the <see cref="BestStoriesCacheEndpoint"/>.
    /// </summary>
    [TestClass]
    public class BestStoriesCacheEndpointTests
    {
        /// <summary>
        /// Tests the BestStoriesCacheEndpoint for a successful Status200OK result.
        /// </summary>
        [TestMethod]
        public async Task BestStoriesEndpoint_Return_Status200OK()
        {
            // Arrange
            Mock<IBestStoriesCacheService> mockBestStoriesCacheService = new();
            mockBestStoriesCacheService.Setup(
                s => s.GetBestStoriesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IEnumerable<Story>?>(DataUtility.GetBestStories().OrderByDescending(s => s.score).Take(5)));

            // Act
            IResult resultObject = await BestStoriesCacheEndpoint.GetBestStories(5, mockBestStoriesCacheService.Object, CancellationToken.None)
                .ConfigureAwait(false);

            //Assert
            mockBestStoriesCacheService.Verify(s => s.GetBestStoriesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()));

            var result = resultObject as Ok<IEnumerable<Story>>;

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(5, result.Value.Count());
        }

        /// <summary>
        /// Tests the BestStoriesCacheEndpoint for a Status500InternalServerError result.
        /// </summary>
        [TestMethod]
        public async Task BestStoriesEndpoint_Return_Status500InternalServerError()
        {
            // Arrange
            Mock<IBestStoriesCacheService> mockBestStoriesService = new();
            mockBestStoriesService.Setup(
                s => s.GetBestStoriesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new NullReferenceException());

            // Act
            IResult resultObject = await BestStoriesCacheEndpoint.GetBestStories(5, mockBestStoriesService.Object, CancellationToken.None)
                .ConfigureAwait(false);

            // Assert
            mockBestStoriesService.Verify(s => s.GetBestStoriesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()));

            var result = resultObject as StatusCodeHttpResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(500, result.StatusCode);
        }
    }
}
