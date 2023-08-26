using BestStories.Core.Models;
using BestStories.Core.Static;
using BestStoriesAPI.Endpoints;
using BestStoriesAPI.Filters;
using BestStoriesAPI.Interfaces;
using BestStoriesAPI.Models;
using BestStoriesAPI.Services;
using Microsoft.Extensions.Options;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddHttpClient(Constants.BEST_STORIES_CACHE_API, (serviceProvider, httpClient) =>
{
    if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
    if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));

    IOptions<BestStoriesConfiguration>? bestStoriesConfiguration = serviceProvider.GetService<IOptions<BestStoriesConfiguration>>();
    
    if(bestStoriesConfiguration == null)
    {
        throw new NullReferenceException(nameof(bestStoriesConfiguration));
    }

    httpClient.BaseAddress = new Uri(bestStoriesConfiguration.Value.BestStoriesCacheAPI ?? throw new ArgumentNullException(bestStoriesConfiguration.Value.BestStoriesCacheAPI));
}).ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler
    {
        // NOTE:
        // Only do this in development when running BestStoriesAPI
        // and BestStoriesCacheAPI in separate docker containers.

        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };

    return handler;
});

builder.Services.Configure<BestStoriesConfiguration>(builder.Configuration.GetSection("BestStoriesConfiguration"));

// NOTE:
// Distributed Memory Cache can be used for development 
// and testing puroses, or when running a single instance.
// However, in a production environment, the distributed
// cache should be configured to use a more appropriate
// caching service instead e.g. Redis

builder.Services.AddDistributedMemoryCache();

builder.Services.AddScoped<IBestStoriesCacheAPIService, BestStoriesCacheAPIService>();
builder.Services.AddScoped<IBestStoriesService, BestStoriesService>();

WebApplication app = builder.Build();

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

app.MapGet("/getbeststories/{count:int}", BestStoriesEndpoint.GetBestStories)
    .AddEndpointFilter<BestStoriesValidationFilter>()
    .WithOpenApi()
    .WithName("GetBestStories")
    .WithDescription("The GetBestStories Endpoint")
    .Produces<IEnumerable<Story>>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status500InternalServerError);

app.Run();