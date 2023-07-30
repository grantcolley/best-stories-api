using BestStories.Api.Cache;
using BestStories.Api.Core.Interfaces;
using BestStories.Api.Core.Static;
using BestStories.Api.Endpoints;
using BestStories.Api.Filters;
using BestStories.Api.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddHttpClient(Constants.HACKER_NEWS, client =>
{
    string? baseAddress = builder.Configuration.GetValue<string>("HackerNewsApi:Url");

    if(baseAddress == null)
    {
        throw new NullReferenceException(nameof(baseAddress));
    }

    client.BaseAddress = new Uri(baseAddress);
});

builder.Services.AddSingleton<IBestStoriesCache, SemaphoreSlimCache>();
builder.Services.AddSingleton<IBestStoriesApiService, BestStoriesApiService>();
builder.Services.AddScoped<IBestStoriesService, BestStoriesService>();
builder.Services.AddHostedService<BestStoriesBackgroundService>();

WebApplication app = builder.Build();

app.MapGet("getbeststories/{count:int}", BestStoriesEndpoint.GetBestStories)
    .AddEndpointFilter<BestStoriesValidationFilter>();

app.Run();

internal partial class Program { }