using BestStories.Api.Core.Models;
using BestStories.Api.Filters;
using BestStories.Api.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;

namespace BestStories.Api.Tests
{
    [TestClass]
    public class BestStoriesValidationFilterTests
    {
        private const string successMessage = "Success!";
        private const string errorMessage = $"Specify number of best stories to fetch between 1 and 100";

        private readonly IOptions<BestStoriesConfiguration> _bestStoriesConfiguration;

        public BestStoriesValidationFilterTests()
        {
            _bestStoriesConfiguration = Options.Create(new BestStoriesConfiguration { CacheMaxSize = 100 });
        }

        /// <summary>
        /// Tests the returns a Status200OK after the validating the parameter is within the permissible range.
        /// </summary>
        [TestMethod]
        public async Task BestStoriesValidationFilter_Return_Status200OK()
        {
            // Arrange
            MockEndpointFilterInvocationContext mockEndpointFilterInvocationContext = new();

            mockEndpointFilterInvocationContext.Arguments.Add(5);

            BestStoriesValidationFilter bestStoriesValidationFilter = new(_bestStoriesConfiguration);

            // Act
            var resultObject = await bestStoriesValidationFilter.InvokeAsync(mockEndpointFilterInvocationContext, EndpointFilterDelegate)
                .ConfigureAwait(false);

            var result = resultObject as Ok<string>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(successMessage, result.Value);
        }

        /// <summary>
        /// Tests the returns a Status400BadRequest when no parameter is provided.
        /// </summary>
        [TestMethod]
        public async Task BestStoriesValidationFilter_No_Parameter_Return_Status400BadRequest()
        {
            // Arrange
            MockEndpointFilterInvocationContext mockEndpointFilterInvocationContext = new();

            BestStoriesValidationFilter bestStoriesValidationFilter = new(_bestStoriesConfiguration);

            // Act
            var resultObject = await bestStoriesValidationFilter.InvokeAsync(mockEndpointFilterInvocationContext, EndpointFilterDelegate)
                .ConfigureAwait(false);

            var result = resultObject as BadRequest<string>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            Assert.AreEqual(errorMessage, result.Value);
        }

        /// <summary>
        /// Tests the returns a Status400BadRequest after the validating the parameter is outside the permissible range.
        /// </summary>
        [TestMethod]
        public async Task BestStoriesValidationFilter_Parameter_Equals_0_Return_Status400BadRequest()
        {
            // Arrange
            MockEndpointFilterInvocationContext mockEndpointFilterInvocationContext = new();

            mockEndpointFilterInvocationContext.Arguments.Add(0);

            BestStoriesValidationFilter bestStoriesValidationFilter = new(_bestStoriesConfiguration);

            // Act
            var resultObject = await bestStoriesValidationFilter.InvokeAsync(mockEndpointFilterInvocationContext, EndpointFilterDelegate)
                .ConfigureAwait(false);

            var result = resultObject as BadRequest<string>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            Assert.AreEqual(errorMessage, result.Value);
        }

        /// <summary>
        /// Tests the returns a Status400BadRequest after the validating the parameter is outside the permissible range.
        /// </summary>
        [TestMethod]
        public async Task BestStoriesValidationFilter_Parameter_Greater_Than_CacheMaxSize_Return_Status400BadRequest()
        {
            // Arrange
            MockEndpointFilterInvocationContext mockEndpointFilterInvocationContext = new();

            mockEndpointFilterInvocationContext.Arguments.Add(101);

            BestStoriesValidationFilter bestStoriesValidationFilter = new(_bestStoriesConfiguration);

            // Act
            var resultObject = await bestStoriesValidationFilter.InvokeAsync(mockEndpointFilterInvocationContext, EndpointFilterDelegate)
                .ConfigureAwait(false);

            var result = resultObject as BadRequest<string>;

            // Assert
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
