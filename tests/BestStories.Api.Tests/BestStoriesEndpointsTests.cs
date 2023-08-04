using BestStories.Api.Core.Interfaces;
using BestStories.Api.Core.Models;
using BestStories.Api.Endpoints;
using BestStories.Api.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace BestStories.Api.Tests
{
    [TestClass]
    public class BestStoriesEndpointsTests
    {
        /// <summary>
        /// Tests the BestStoriesEndpoint for a successful Status200OK result.
        /// </summary>
        [TestMethod]
        public async Task GetBestStories_Return_Status200OK()
        {
            // Arrange
            Mock<IBestStoriesService> mockBestStoriesService = new();
            mockBestStoriesService.Setup(
                s => s.GetBestStoriesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(DataUtility.GetBestStories().OrderByDescending(s => s.score).Take(5)));

            // Act
            IResult resultObject = await BestStoriesEndpoint.GetBestStories(5, mockBestStoriesService.Object, CancellationToken.None)
                .ConfigureAwait(false);

            //Assert
            mockBestStoriesService.Verify(s => s.GetBestStoriesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()));

            var result = resultObject as Ok<IEnumerable<Story>>;

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(5, result.Value.Count());
        }

        /// <summary>
        /// Tests the BestStoriesEndpoint for a Status500InternalServerError result.
        /// </summary>
        [TestMethod]
        public async Task GetBestStories_Return_Status500InternalServerError()
        {
            // Arrange
            Mock<IBestStoriesService> mockBestStoriesService = new();
            mockBestStoriesService.Setup(
                s => s.GetBestStoriesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new NullReferenceException());

            // Act
            IResult resultObject = await BestStoriesEndpoint.GetBestStories(5, mockBestStoriesService.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            mockBestStoriesService.Verify(s => s.GetBestStoriesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()));

            var result = resultObject as StatusCodeHttpResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(500, result.StatusCode);
        }
    }
}
