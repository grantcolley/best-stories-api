using BestStoriesAPI.Filters;
using BestStoriesAPI.Tests.Helpers;
using BestStoriesCacheAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;

namespace BestStoriesAPI.Tests
{
    /// <summary>
    /// Tests the <see cref="BestStoriesValidationFilter"/>.
    /// </summary>
    [TestClass]
    public class BestStoriesCacheValidationFilterTests
    {
        private const string successMessage = "Success!";
        private const string errorMessage = $"Specify number of best stories to fetch between 1 and 200";

        private readonly IOptions<BestStoriesCacheConfiguration> _bestStoriesCacheConfiguration;

        public BestStoriesCacheValidationFilterTests()
        {
            _bestStoriesCacheConfiguration = Options.Create(new BestStoriesCacheConfiguration { CacheMaxSize = 200 });
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

            BestStoriesCacheValidationFilter bestStoriesValidationFilter = new(_bestStoriesCacheConfiguration);

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
        /// Returns a Status400BadRequest when no parameter is provided.
        /// </summary>
        [TestMethod]
        public async Task BestStoriesValidationFilter_No_Parameter_Return_Status400BadRequest()
        {
            // Arrange
            MockEndpointFilterInvocationContext mockEndpointFilterInvocationContext = new();

            BestStoriesCacheValidationFilter bestStoriesValidationFilter = new(_bestStoriesCacheConfiguration);

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
        /// Returns a Status400BadRequest after the validating the parameter is outside the permissible range.
        /// </summary>
        [TestMethod]
        public async Task BestStoriesValidationFilter_Parameter_Equals_0_Return_Status400BadRequest()
        {
            // Arrange
            MockEndpointFilterInvocationContext mockEndpointFilterInvocationContext = new();

            mockEndpointFilterInvocationContext.Arguments.Add(0);

            BestStoriesCacheValidationFilter bestStoriesValidationFilter = new(_bestStoriesCacheConfiguration);

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
        /// Returns a Status400BadRequest after the validating the parameter is outside the permissible range.
        /// </summary>
        [TestMethod]
        public async Task BestStoriesValidationFilter_Parameter_Greater_Than_CacheMaxSize_Return_Status400BadRequest()
        {
            // Arrange
            MockEndpointFilterInvocationContext mockEndpointFilterInvocationContext = new();

            mockEndpointFilterInvocationContext.Arguments.Add(201);

            BestStoriesCacheValidationFilter bestStoriesValidationFilter = new(_bestStoriesCacheConfiguration);

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