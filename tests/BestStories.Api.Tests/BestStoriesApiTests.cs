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
        ///// <summary>
        ///// Test the Program.cs will successfully launch the BestStoriesApi.
        ///// 
        ///// *** WARNING ****************************************************
        ///// 
        ///// FOR MANUAL EXECUTION ONLY FOR INTEGRATION TESTING
        ///// 
        ///// RUNNING THIS TEST WILL SEND REQUESTS TO Hacker News API
        ///// 
        ///// NOT TO BE RUN AUTOMATED IN AUTOMATED INTEGRATION TESTING
        ///// 
        ///// ****************************************************************
        ///// 
        ///// </summary>
        //[TestMethod]
        //public async Task BestStoriesApi_Endpoint_Integration_Test()
        //{
        //    await using var application = new WebApplicationFactory<Program>();

        //    using var client = application.CreateClient();

        //    var response = await client.GetStringAsync("getbeststories/5");

        //    var stories = JsonSerializer.Deserialize<IEnumerable<Story>>(response);

        //    Assert.IsNotNull(stories);
        //    Assert.AreEqual(5, stories.Count());
        //}
    }
}