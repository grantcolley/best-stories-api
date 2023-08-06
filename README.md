# Best Stories API

[![Build status](https://ci.appveyor.com/api/projects/status/biv16q70s4vck6u1?svg=true)](https://ci.appveyor.com/project/grantcolley/best-stories-api)

**Best Stories API** is a RESTful API to retrieve to retrieve the details of the best *n* stories from the [Hacker News API](https://github.com/HackerNews/API), as determined by their score, where *n* is
specified by the caller to the API. 

### Table of Contents
* [Observations](#observations)
* [Assumptions](#assumptions)
* [How to run the application](#how-to-run-the-application)
* [OpenAPI definition for GetBestStories](#openapi-definition-for-getbeststories)
* [If I had more time](#if-i-had-more-time)
* [Implementation Details](#implementation-details)
	* [Minimal API](#minimal-api)
	* [Caching the results](#caching-the-results)
	* [Filter Validation](#filter-validation)
	* [Configuration](#configuration)
* [Testing](#testing)
	* [Unit Tests](#unit-tests)
	
## Observations
I conducted a simple test, first calling the Hacker News API endpoint to fetch the IDs for best stories, followed by calling the endpoint to fetch each story. These steps were repeated at 5 second intervals over a period of time.

I observed the `beststories` endpoint consistently returns 200 IDs, which appear to have been sorted by score in descending order. However, a story’s score is subject to change by the time the story has been fetched by calling the endpoint for individual stories, passing in the story’s ID.

### Assumptions
- There is no way to subscribe to score changes in stories.
- Consumers of **Best Stories API** will not be authenticated. The API will be open to the public like the **Hacker News API**.
- There is [no rate limit](https://github.com/HackerNews/API#uri-and-versioning) on **Hacker News API** endpoints, so no need to "back off" periodically.

## How to run the application
The easiest way to run the application is clone the repository, open the solution [BestStoriesAPI.sln](https://github.com/grantcolley/best-stories-api/blob/main/BestStoriesAPI.sln) in Visual Studio, compile it, and start running by pressing `F5`.

The default url is `https://localhost:7240`. This can be changed in the [launchSettings.json](https://github.com/grantcolley/best-stories-api/blob/f5f76d2b2d6e7f7d2f7b62bad64fd3fb283f07b7/src/BestStories.Api/Properties/launchSettings.json#L24).

Send a request to the API using [postman](https://github.com/grantcolley/best-stories-api/blob/main/readme-images/postman_screenshot.png) or a browser, such as chrome e.g. `https://localhost:7240/getbeststories/200`

![Alt text](/readme-images/chrome_screenshot.png?raw=true "Sending a request in Chrome")

## OpenAPI definition for GetBestStories
Exposing the generated OpenAPI definition for the `GetBestStories` endpoint.

`https://localhost:7240/swagger/index.html`

`https://localhost:7240/swagger/v1/swagger.json`

## If I had more time
I would do load/stress testing.

## Implementation Details
### Minimal API
To retrieve the details of the best *n* stories from the Hacker News API, the consumer will call the `getbeststories` minimal API endpoint, specifying the number of stories required.

```C#
app.MapGet("getbeststories/{count:int}", BestStoriesEndpoint.GetBestStories)
    .AddEndpointFilter<BestStoriesValidationFilter>()
    .WithOpenApi()
    .WithName("GetBestStories")
    .WithDescription("The GetBestStories Endpoint")
    .Produces<IEnumerable<Story>>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status500InternalServerError);
```

### Caching the results
Because of the indeterminate way each story’s score can be updated, after obtaining the IDs from the `beststories` endpoint, each story will be fetched, to obtain it's latest score. This currently results in a total of 201 requests, one request for the collection containing 200 best story IDs, followed by request for each of the 200 stories.

To efficiently service large numbers of requests without risking overloading of the **Hacker News API**, the results will be cached with an expiry time.

Subsequent requests will simply retrieve the cached stories and return the top *n* specified by the caller.

The first request after the cached stories have exired will fetch an updated collection of best stories from the Hacker News API and cache them.

Prior to persisting the stories in the cache, they will be sorted in descending order of score. The total number of stories persisted will be determined by the `CacheMaxSize`.

The expiry of the cached stories will be determined by the `CacheExpiryInSeconds`.

```C#
            IEnumerable<Story> rankedStoriesToCache = 
                stories.OrderByDescending(s => s.score)
                .Take(_bestStoriesConfiguration.CacheMaxSize)
                .ToList();

            DateTimeOffset expires = DateTimeOffset.Now.Add(TimeSpan.FromSeconds(_bestStoriesConfiguration.CacheExpiryInSeconds));

            byte[] storiesToCache = UTF8Encoding.UTF8.GetBytes(JsonSerializer.Serialize(rankedStoriesToCache));

            await _distributedCache.SetAsync(
                Constants.DISTRIBUTED_CACHE,
                storiesToCache,
                new DistributedCacheEntryOptions { AbsoluteExpiration = expires })
                .ConfigureAwait(false);
```

#### Distributed Cache
Distributed cachming was chosen for performance and scalability, especially if **Best Stories API** is hosted by a cloud service or a server farm.

The current implementation for distributed caching is [DistributedCache](https://github.com/grantcolley/best-stories-api/blob/main/src/BestStories.Api/Cache/DistributedCache.cs). 

>  **Note**
>
> [Distributed Memory Cache](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed?view=aspnetcore-7.0#distributed-memory-cache) is used for development and testing purposes, or in single server scenarios.
> 
> In a production environment, the distributed cache should be configured for an appropriate caching service e.g. Redis.

### Filter Validation
Endpoint filter [BestStoriesValidationFilter](https://github.com/grantcolley/best-stories-api/blob/182666e6270723b78200302485257e1db0a20329/src/BestStoriesAPI/Filters/BestStoriesValidationFilter.cs#L38-L42) will validate that consumers provide a valid number between 1 and the specified `CacheMaxSize`.

### Configuration
The [appsettings.json](https://github.com/grantcolley/best-stories-api/blob/182666e6270723b78200302485257e1db0a20329/src/BestStoriesAPI/appsettings.json#L8-L12) contains the following:

|Key|Description|
|---|-----------|
|HackerNewsApi|Hacker News API url.|
|CacheMaxSize|Maximum stories to be cached. Currently set to 200 stories.|
|CacheExpiryInSeconds|The AbsoluteExpiration set for the cached value. Currently set to 5 seconds.|

```C#
  "BestStoriesConfiguration": {
    "HackerNewsApi": "https://hacker-news.firebaseio.com/v0/",
    "CacheMaxSize": 200,
    "CacheExpiryInSeconds": 5
```

## Testing
### Unit Tests 
[BestStoriesApi.Tests](https://github.com/grantcolley/best-stories-api/tree/main/tests/BestStoriesAPI.Tests) project contains the unit tests.
