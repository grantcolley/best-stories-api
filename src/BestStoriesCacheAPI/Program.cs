using BestStories.Core.Models;
using BestStories.Core.Static;
using BestStoriesAPI.Filters;
using BestStoriesCacheAPI.Cache;
using BestStoriesCacheAPI.Endpoints;
using BestStoriesCacheAPI.Interfaces;
using BestStoriesCacheAPI.Models;
using BestStoriesCacheAPI.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddHttpClient(Constants.HACKER_NEWS, (serviceProvider, httpClient) =>
{
    if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
    if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));

    IOptions<BestStoriesCacheConfiguration>? bestStoriesConfiguration = serviceProvider.GetService<IOptions<BestStoriesCacheConfiguration>>();

    if (bestStoriesConfiguration == null)
    {
        throw new NullReferenceException(nameof(bestStoriesConfiguration));
    }

    httpClient.BaseAddress = new Uri(bestStoriesConfiguration.Value.HackerNewsApi ?? throw new ArgumentNullException(bestStoriesConfiguration.Value.HackerNewsApi));
});

builder.Services.Configure<BestStoriesCacheConfiguration>(builder.Configuration.GetSection("BestStoriesCacheConfiguration"));

int cacheExpiryInSeconds = builder.Configuration.GetValue<int>("BestStoriesConfiguration:CacheExpiryInSeconds");
int maxCacheSize = builder.Configuration.GetValue<int>("BestStoriesConfiguration:CacheExpiryInSeconds");

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
builder.Services.AddScoped<IBestStoriesCacheService, BestStoriesCacheService>();

WebApplication app = builder.Build();

IDistributedCache distributedCache = app.Services.GetRequiredService<IDistributedCache>();

distributedCache.Set(
    Constants.DISTRIBUTED_CACHE_MAX_SIZE,
    BitConverter.GetBytes(maxCacheSize));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.MapHealthChecks("/health");

app.MapGet("/error", () => Results.Problem());

app.MapGet("/recyclecachedstories/{count:int}", BestStoriesCacheEndpoint.GetBestStories)
    .AddEndpointFilter<BestStoriesCacheValidationFilter>()
    .WithOpenApi()
    .WithName("GetBestCachedStories")
    .WithDescription("The Get Best Cached Stories Endpoint")
    .Produces<IEnumerable<Story>>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status500InternalServerError);

app.Run();