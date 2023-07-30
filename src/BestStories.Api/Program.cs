using BestStories.Api.Cache;
using BestStories.Api.Core.Interfaces;
using BestStories.Api.Core.Models;
using BestStories.Api.Core.Static;
using BestStories.Api.Endpoints;
using BestStories.Api.Filters;
using BestStories.Api.Services;
using Microsoft.Extensions.Options;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddHttpClient(Constants.HACKER_NEWS, (serviceProvider, httpClient) =>
{
    if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
    if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));

    IOptions<BestStoriesConfiguration>? bestStoriesConfiguration = serviceProvider.GetService<IOptions<BestStoriesConfiguration>>();
    
    if(bestStoriesConfiguration == null)
    {
        throw new NullReferenceException(nameof(bestStoriesConfiguration));
    }

    httpClient.BaseAddress = new Uri(bestStoriesConfiguration.Value.HackerNewsApi ?? throw new ArgumentNullException(bestStoriesConfiguration.Value.HackerNewsApi));
});

builder.Services.Configure<BestStoriesConfiguration>(
            builder.Configuration.GetSection("BestStoriesConfiguration"));

builder.Services.AddSingleton<IBestStoriesCache, SemaphoreSlimCache>();
builder.Services.AddSingleton<IBestStoriesApiService, BestStoriesApiService>();
builder.Services.AddScoped<IBestStoriesService, BestStoriesService>();
builder.Services.AddHostedService<BestStoriesBackgroundService>();

WebApplication app = builder.Build();

app.MapGet("getbeststories/{count:int}", BestStoriesEndpoint.GetBestStories)
    .AddEndpointFilter<BestStoriesValidationFilter>();

app.Run();

/// <summary>
/// 
/// https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-7.0#basic-tests-with-the-default-webapplicationfactory
/// 
/// Expose the implicitly defined Program class to the test project BestStories.Api.Tests.
/// 
/// Also requires the following entry in BestStories.Api.csproj
/// 
///     <ItemGroup>
///         <InternalsVisibleTo Include = "BestStories.Api.Tests" />
///     </ItemGroup>
/// 
/// </summary>
internal partial class Program { }