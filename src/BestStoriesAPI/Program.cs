using BestStoriesApi.Cache;
using BestStoriesAPI.Endpoints;
using BestStoriesAPI.Filters;
using BestStoriesAPI.Interfaces;
using BestStoriesAPI.Models;
using BestStoriesAPI.Services;
using BestStoriesAPI.Static;
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

builder.Services.Configure<BestStoriesConfiguration>(builder.Configuration.GetSection("BestStoriesConfiguration"));

int cacheExpiryInSeconds = builder.Configuration.GetValue<int>("BestStoriesConfiguration:CacheExpiryInSeconds");

// NOTE:
// Distributed Memory Cache can be used for development 
// and testing puroses, or when running a single instance.
// However, in a production environment, the distributed
// cache should be configured to use a more appropriate
// caching service instead e.g. Redis

builder.Services.AddDistributedMemoryCache(options =>
{
    options.ExpirationScanFrequency = TimeSpan.FromSeconds(cacheExpiryInSeconds);
});

builder.Services.AddSingleton<IBestStoriesCache, DistributedCache>();
builder.Services.AddSingleton<IHackerNewsAPIService, HackerNewsAPIService>();
builder.Services.AddScoped<IBestStoriesService, BestStoriesService>();

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