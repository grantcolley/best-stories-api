# Best Stories API

[![Build status](https://ci.appveyor.com/api/projects/status/biv16q70s4vck6u1?svg=true)](https://ci.appveyor.com/project/grantcolley/best-stories-api)

**Best Stories API** is a RESTful API to retrieve to retrieve the details of the best *n* stories from the [Hacker News API](https://github.com/HackerNews/API), as determined by their score, where *n* is
specified by the caller to the API. 

### Table of Contents
* [Observations](#observations)
* [Assumptions](#assumptions)
* [The Solution](#the-solution)
* [How to run the application](#how-to-run-the-application)
* [OpenAPI definition for Best Stories API](#openapi-definition-for-best-stories-api)
* [Implementation Details](#implementation-details)
	* [Best Stories API](#best-stories-api-1)
	* [Best Stories Cache API](#best-stories-cache-api)
	* [Distributed Cache](#distributed-cache)
	* [Filter Validation](#filter-validation)
* [Testing](#testing)
	* [Unit Tests](#unit-tests)
* [If I had more time](#if-i-had-more-time)

## Observations
I conducted a simple test, first calling the Hacker News API endpoint to fetch the IDs for best stories, followed by calling the endpoint to fetch each story. These steps were repeated at 5 second intervals over a period of time.

I observed the `beststories` endpoint consistently returns 200 IDs, which appear to have been sorted by score in descending order. However, a story’s score is subject to change by the time the story has been fetched by calling the endpoint for individual stories, passing in the story’s ID.

### Assumptions
- There is no way to subscribe to score changes in stories.
- Consumers of **Best Stories API** will not be authenticated. The API will be open to the public like the **Hacker News API**.
- There is [no rate limit](https://github.com/HackerNews/API#uri-and-versioning) on **Hacker News API** endpoints, so no need to "back off" periodically.

## The Solution

![Alt text](/readme-images/solution.png?raw=true "The Solution")

Because of the indeterminate way each story’s score can be updated, after obtaining the IDs from the `beststories` endpoint, each story will be fetched, to obtain it's latest score. This currently results in a total of 201 requests to **Hacker News API**, one request for the collection containing 200 best story IDs, followed by request for each of the 200 stories.

To efficiently service large numbers of requests without risking overloading of the **Hacker News API**, the results will be cached in a distributed cache, with an expiry time.

> **Note**
>
> I have taken the approach of having a second API, whose sole responsibility is to co-ordinate recycling the cached stories. Concurrent requests to recycle the cache will await a semaphore, where only the first one will be allowed to call **Hacker News API** to re-build the cache.
> 
> An alternative approach would be to employ a distributed locking mechanism, should the distributed cache support it. This demonstration uses [Distributed Memory Cache](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed?view=aspnetcore-7.0#distributed-memory-cache), which does not support distributed locking.

After the stories have been cached, subsequent requests will simply retrieve the cached stories and return the top *n* specified by the caller, until the cached stories expire and are flushed from the cache.

The first request after the cached stories have expired will fetch an updated collection of best stories from the Hacker News API and cache them.

Prior to persisting the stories in the cache, they will be sorted in descending order of score. The total number of stories persisted will be determined by the `CacheMaxSize`.

The *life* of the cached stories will be determined by the `CacheExpiryInSeconds`.

```C#
        private async Task<IEnumerable<Story>> PersistStoriesToCacheAsync(IEnumerable<Story> stories)
        {
            IEnumerable<Story> rankedStoriesToCache = 
                stories.OrderByDescending(s => s.score)
                .Take(_bestStoriesCacheConfiguration.CacheMaxSize)
                .ToList();

            DateTimeOffset expires = DateTimeOffset.Now.Add(TimeSpan.FromSeconds(_bestStoriesCacheConfiguration.CacheExpiryInSeconds));

            byte[] storiesToCache = UTF8Encoding.UTF8.GetBytes(JsonSerializer.Serialize(rankedStoriesToCache));

            await _distributedCache.SetAsync(
                Constants.DISTRIBUTED_CACHE_BEST_STORIES,
                storiesToCache,
                new DistributedCacheEntryOptions { AbsoluteExpiration = expires })
                .ConfigureAwait(false);

            return rankedStoriesToCache;
        }
```

## How to run the application
The easiest way to run the application is clone the repository and open the solution [BestStories.sln](https://github.com/grantcolley/best-stories-api/blob/main/BestStories.sln) in Visual Studio.

Both **Best Stories API** and **Best Stories Cache API** are required to run the application. Open the solution properties window, select `Multiple startup projects` and set the action to both projects to `Start`.

![Alt text](/readme-images/solution-startup-properties.png?raw=true "The Solution Properties Window")

Compile the solution, and start running by pressing `F5`.

The default url for **Best Stories API** is `https://localhost:7240`. This can be changed in the [launchSettings.json](https://github.com/grantcolley/best-stories-api/blob/df133a13a7e22719eaf384e8dfde5ac5d561bc39/src/BestStoriesAPI/Properties/launchSettings.json#L24).

Send a request to the **Best Stories API** using [postman](https://github.com/grantcolley/best-stories-api/blob/main/readme-images/postman_screenshot.png) or a browser, such as chrome e.g. `https://localhost:7240/getbeststories/200`

![Alt text](/readme-images/chrome_screenshot.png?raw=true "Sending a request in Chrome")

## OpenAPI definition for Best Stories API
Exposing the generated OpenAPI definition for the `getbeststories` endpoint.

`https://localhost:7240/swagger/index.html`

`https://localhost:7240/swagger/v1/swagger.json`

## Implementation Details
### Best Stories API
To retrieve the details of the best *n* stories from the distributed cache, the consumer will call the `getbeststories/{count:int}` minimal API endpoint, specifying the number of stories required.

The flow for **Best Stories API** is follows:
- First check if the cache has been populated, if yes return the required stories from the cache.
- Second, if the cache is empty (previously cached values have expired), send a request to **Best Stories Cache API** to recycle the cache. 
- Finally, when **Best Stories Cache API** returns the required stories from the freshly recycled cache, return them to the consumer.

```C#
app.MapGet("getbeststories/{count:int}", BestStoriesEndpoint.GetBestStories)
    .AddEndpointFilter<BestStoriesValidationFilter>()
    .WithOpenApi()
    .WithName("GetBestStories")
    .WithDescription("The GetBestStories Endpoint")
    .Produces<IEnumerable<Story>>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status500InternalServerError);

// Configuration
  "BestStoriesConfiguration": {
    "BestStoriesCacheAPI": "https://localhost:7157",
    "CacheMaxSize": 200
```

### Best Stories Cache API
If a request to **Best Story API** determines the cached stories have expired, it will send on a request to recycle the cache to **Best Stories Cache API**, which exposes the `recyclecachedstories` minimal API endpoint.

When **Best Stories Cache API** receives a request to recycle the cache, it will ensure only one request is sent to **Hacker News API**.

The flow for **Best Stories Cache API** is follows:
- First check if the cache has been populated, if yes return the required stories from the cache.
- Second, if the cache is empty (previously cached values have expired), enter the semaphore.
- Third, double check if the cache has been poulated, if yes return the required stories from the cache.
- Fourth, fetch the latest best stories from **Hacker News API**.
- Fifth, persist the stories to the cache, setting the expiry to `CacheExpiryInSeconds`.
- Finally, return the required stories from the freshly recycled cache.

> **Note** any requests awaiting the semaphore will now enter it to find the cache has already been recycled, and will simply return the required stories from the freshly recycled cache.

```C#
app.MapGet("recyclecachedstories/{count:int}", BestStoriesCacheEndpoint.GetBestStories)
    .AddEndpointFilter<BestStoriesCacheValidationFilter>()
    .WithOpenApi()
    .WithName("GetBestCachedStories")
    .WithDescription("The Get Best Cached Stories Endpoint")
    .Produces<IEnumerable<Story>>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status500InternalServerError);

// Configuration
  "BestStoriesConfiguration": {
    "HackerNewsApi": "https://hacker-news.firebaseio.com/v0/",
    "CacheMaxSize": 200,
    "CacheExpiryInSeconds": 10
```

### Distributed Cache
Distributed caching was chosen for performance and scalability, especially if **Best Stories API** is hosted by a cloud service or a server farm.

The current implementation for distributed caching is [DistributedCache](https://github.com/grantcolley/best-stories-api/blob/main/src/BestStoriesCacheAPI/Cache/DistributedCache.cs), managing concurrent requests to recycle the cache using `await SemaphoreSlim.WaitAsync()`, ensuring only the first request fetches the latest best stories from **Hacker News API**, and persists them to the distributed cache. All subsequent requests will `await`, until they finally enter the semephore, by which time the cache will already be populated, so they retrieve their stories directly from the cache and return.

>  **Note**
>
> [Distributed Memory Cache](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed?view=aspnetcore-7.0#distributed-memory-cache) is used for development and testing purposes, or in single server scenarios.
> 
> In a production environment, the distributed cache should be configured for an appropriate caching service e.g. Redis.

### Filter Validation
Both **Best Stories API** and **Best Stories Cache API** use endpoint filters to validate their consumers provide a valid number between 1 and the specified `CacheMaxSize`.

## Testing
### Unit Tests 
- [BestStoriesApi.Tests](https://github.com/grantcolley/best-stories-api/tree/main/tests/BestStoriesAPI.Tests).
- [BestStoriesCacheApi.Tests](https://github.com/grantcolley/best-stories-api/tree/main/tests/BestStoriesCacheAPI.Tests).

## If I had more time
- Implement authentication between **Best Stories API** and **Best Stories Cache API** to restrict access to **Best Stories Cache API**. 
- Load/stress testing.
