using BestStoriesApi.Cache;
using BestStoriesApi.Endpoints;
using BestStoriesApi.Filters;
using BestStoriesApi.Interfaces;
using BestStoriesApi.Services;
using BestStoriesApi.Static;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddHttpClient(HttpClientNames.HACKER_NEWS, client =>
{
    string? baseAddress = builder.Configuration.GetValue<string>("HackerNewsApi:Url");

    if(baseAddress == null)
    {
        throw new NullReferenceException(nameof(baseAddress));
    }

    client.BaseAddress = new Uri(baseAddress);
});

builder.Services.AddSingleton<IBestStoriesCache, BestStoriesCache>();
builder.Services.AddSingleton<IBestStoriesApiService, BestStoriesApiService>();
builder.Services.AddScoped<IBestStoriesService, BestStoriesService>();
builder.Services.AddHostedService<BestStoriesBackgroundService>();

WebApplication app = builder.Build();

app.MapGet("getbeststories/{count:int}", BestStoriesEndpoints.GetBestStories)
    .AddEndpointFilter<BestStoriesValidationFilter>();

app.Run();

internal partial class Program { }