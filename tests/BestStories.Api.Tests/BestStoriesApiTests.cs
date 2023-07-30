using BestStories.Api.Cache;
using BestStories.Api.Core.Interfaces;
using BestStories.Api.Core.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using BestStories.Api.Tests.Helpers;

namespace BestStories.Api.Tests
{
    [TestClass]
    public class BestStoriesApiTests
    {
        //[TestMethod]
        //public async Task BestStoriesApi_Endpoint_Integration_Test()
        //{
        //    /////////////////////////////////////////////////////////////////////
        //    ////
        //    //// WARNING 
        //    ////
        //    //// RUNNING THIS TEST WILL SEND REQUESTS TO Hacker News API
        //    ////
        //    //// FOR MANUAL EXECUTION ONLY FOR INTEGRATION TESTING
        //    //// 
        //    //// NOT TO BE RUN AUTOMATED IN A CONTINUOS INTEGRATION ENVIRONMENT
        //    ////
        //    /////////////////////////////////////////////////////////////////////

        //    await using var application = new WebApplicationFactory<Program>();

        //    using var client = application.CreateClient();

        //    var response = await client.GetStringAsync("getbeststories/5");

        //    var stories = JsonSerializer.Deserialize<IEnumerable<Story>>(response);

        //    Assert.IsNotNull(stories);
        //    Assert.AreEqual(5, stories.Count());
        //}
    }
}