using BestStoriesApi.Filters;
using BestStoriesApi.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;

namespace BestStoriesApi.Tests
{
    [TestClass]
    public class BestStoriesValidationFilterTests
    {
        private const string successMessage = "Success!";
        private const string errorMessage = $"Specify number of best stories to fetch between 1 and 100";

        private readonly IConfiguration _configuration;

        public BestStoriesValidationFilterTests()
        {
            Dictionary<string, string?> configSettings = new()
            {
                {"BestStories:CacheMaxSize", "100"}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configSettings)
                .Build();
        }

        [TestMethod]
        public async Task BestStoriesValidationFilter_Return_Ok()
        {
            // Arrange
            MockEndpointFilterInvocationContext mockEndpointFilterInvocationContext = new();

            mockEndpointFilterInvocationContext.Arguments.Add(5);

            BestStoriesValidationFilter bestStoriesValidationFilter = new(_configuration);

            // Act
            var resultObject = await bestStoriesValidationFilter.InvokeAsync(mockEndpointFilterInvocationContext, EndpointFilterDelegate)
                .ConfigureAwait(false);

            var result = resultObject as Ok<string>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(successMessage, result.Value);
        }

        [TestMethod]
        public async Task BestStoriesValidationFilter_No_Parameter_Return_BadRequest()
        {
            // Arrange
            MockEndpointFilterInvocationContext mockEndpointFilterInvocationContext = new();

            BestStoriesValidationFilter bestStoriesValidationFilter = new(_configuration);

            // Act
            var resultObject = await bestStoriesValidationFilter.InvokeAsync(mockEndpointFilterInvocationContext, EndpointFilterDelegate)
                .ConfigureAwait(false);

            var result = resultObject as BadRequest<string>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            Assert.AreEqual(errorMessage, result.Value);
        }

        [TestMethod]
        public async Task BestStoriesValidationFilter_Parameter_Equals_0_Return_BadRequest()
        {
            // Arrange
            MockEndpointFilterInvocationContext mockEndpointFilterInvocationContext = new();

            mockEndpointFilterInvocationContext.Arguments.Add(0);

            BestStoriesValidationFilter bestStoriesValidationFilter = new(_configuration);

            // Act
            var resultObject = await bestStoriesValidationFilter.InvokeAsync(mockEndpointFilterInvocationContext, EndpointFilterDelegate)
                .ConfigureAwait(false);

            var result = resultObject as BadRequest<string>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            Assert.AreEqual(errorMessage, result.Value);
        }

        [TestMethod]
        public async Task BestStoriesValidationFilter_Parameter_Greater_Than_CacheMaxSize_Return_BadRequest()
        {
            // Arrange
            MockEndpointFilterInvocationContext mockEndpointFilterInvocationContext = new();

            mockEndpointFilterInvocationContext.Arguments.Add(101);

            BestStoriesValidationFilter bestStoriesValidationFilter = new(_configuration);

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
