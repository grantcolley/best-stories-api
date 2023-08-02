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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

builder.Services.AddSingleton<IBestStoriesApiService, BestStoriesApiService>();
builder.Services.AddScoped<IBestStoriesService, BestStoriesService>();

bool isDistributedCache = builder.Configuration.GetValue<bool>("BestStoriesConfiguration:IsDistributedCache");

if (isDistributedCache)
{
    //////////////////////////////////////////////////////////////////////////////////
    // NOTE:
    // 
    // Distributed Memory Cache is used for development and testing puroses.
    // The BestStoriesBackgroundService will be used to recycle the cache.
    // 
    // In a production environment, the distributed cache should be hosted in a
    // dedicated web api, configured for the appropriate caching service e.g. Redis
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddHostedService<BestStoriesBackgroundService>();
    //////////////////////////////////////////////////////////////////////////////////

    builder.Services.AddSingleton<IBestStoriesCache, DistributedCache>();
}
else
{
    builder.Services.AddSingleton<IBestStoriesCache, ReaderWriterLockSlimCache>();
    builder.Services.AddHostedService<BestStoriesBackgroundService>();
}

WebApplication app = builder.Build();

app.MapGet("getbeststories/{count:int}", BestStoriesEndpoint.GetBestStories)
    .AddEndpointFilter<BestStoriesValidationFilter>()
    .WithOpenApi()
    .WithName("GetBestStories")
    .WithDescription("The GetBestStories Endpoint")
    .Produces<IEnumerable<Story>>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status500InternalServerError);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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