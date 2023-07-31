# Best Stories API

**Best Stories API** is a RESTful API to retrieve to retrieve the details of the best *n* stories from the [Hacker News API](https://github.com/HackerNews/API), as determined by their score, where *n* is
specified by the caller to the API. 

### Table of Contents
* [Observations](#observations)
* [Assumptions](#assumptions)
* [How to run the application](#how-to-run-the-application)
* [If I had more time](#if-i-had-more-time)
* [Implementation Details](#implementation-details)
	* [Minimal API](#minimal-api)
	* [Caching the results](#caching-the-results)
	* [Types of Caches](#types-of-caches)
		* [Distributed Cache](#distributed-cache)
		* [Local Cache](#local-cache)
		* [Benchmarking GetStoryCacheAsync](#benchmarking-getstorycacheasync)
	* [Validation](#validation)
	* [Configuration](#configuration)
* [Testing](#testing)
	* [Unit Tests](#unit-tests)
	* [Benchmarks](#benchmarks)
	* [Test Harness](#test-harness)
	
## Observations
I conducted a simple test, first calling the endpoint to fetch the IDs for best stories, followed by calling the endpoint to fetch each story. These steps were repeated with 5 seconds interval over a period of time.

I observed the `beststories` endpoint consistently returns 200 IDs, which appear to have been sorted by score in descending order. However, a story’s score is subject to change by the time the story has been fetched by calling the endpoint for individual stories, passing in the story’s ID.

### Assumptions
- There is no way to subscribe to score changes in stories.
- Consumers of **Best Stories API** will not be authenticated. The API will be open to the public like the **Hacker News API**.
- There is [no rate limit](https://github.com/HackerNews/API#uri-and-versioning) on **Hacker News API** endpoints, so no need to "back off" periodically.

## How to run the application
The easiest was to run the application is clone the repository, open the solution in Visual Studio, compile it, and start running by pressing `F5`.

The default url is `https://localhost:7240`. This can be changed in the [launchSettings.json](https://github.com/grantcolley/best-stories-api/blob/f5f76d2b2d6e7f7d2f7b62bad64fd3fb283f07b7/src/BestStories.Api/Properties/launchSettings.json#L24).

Send a request to the API using [postman](https://github.com/grantcolley/best-stories-api/blob/main/readme-images/postman_screenshot.png) or a browser, such as chrome e.g. `https://localhost:7240/getbeststories/200`

![Alt text](/readme-images/chrome_screenshot.png?raw=true "Sending a request in Chrome")

## If I had more time
- I would create a separate API dedicated to running the Distributed Cache
- I would add authentication
- I would add more unit tests

## Implementation Details
### Minimal API
To retrieve the details of the best *n* stories from the Hacker News API, the consumer will call the `getbeststories` minimal API endpoint, specifying the number of stories required.

e.g. `https://localhost:7240/getbeststories/25`

```C#
app.MapGet("getbeststories/{count:int}", BestStoriesEndpoints.GetBestStories)
    .AddEndpointFilter<BestStoriesValidationFilter>();
```

### Caching the results
To efficiently service large numbers of requests without risking overloading of the **Hacker News API**, the results will be cached. 

Because of the indeterminate way each story’s score can be updated, after obtaining the IDs from the `beststories` endpoint, each story will be fetched in the background by the [BestStoriesBackgroundService](https://github.com/grantcolley/best-stories-api/blob/f5f76d2b2d6e7f7d2f7b62bad64fd3fb283f07b7/src/BestStories.Api/Services/BestStoriesBackgroundService.cs#L32).

When all the stories have been fetched, they will be sorted in descending order of score, before being persisted to cache.

The [BackgroundService](https://github.com/grantcolley/best-stories-api/blob/f5f76d2b2d6e7f7d2f7b62bad64fd3fb283f07b7/src/BestStories.Api/Services/BestStoriesBackgroundService.cs#L60) will be used to recycle the cache at a pre-configured interval.

Recycling the cache will involve building a new cache in the background then "swapping it out" with the existing cache.

The maximum cache size will be configurable using `CacheMaxSize`.

### Types of Caches
The cache implements the [IBestStoriesCache](https://github.com/grantcolley/best-stories-api/blob/main/src/BestStories.Api.Core/Interfaces/IBestStoriesCache.cs) interface. There are currently several to choose from. A new implementation can be added by implementing [IBestStoriesCache](https://github.com/grantcolley/best-stories-api/blob/main/src/BestStories.Api.Core/Interfaces/IBestStoriesCache.cs) and registering it in [Program.cs](https://github.com/grantcolley/best-stories-api/blob/f5f76d2b2d6e7f7d2f7b62bad64fd3fb283f07b7/src/BestStories.Api/Program.cs#L56). 

```C#
    public interface IBestStoriesCache
    {
        Task RecycleCacheAsync(IEnumerable<Story> stories);
        Task<IEnumerable<Story>?> GetStoryCacheAsync();
    }
```

#### Distributed Cache
The current implementation for distributed caching is [DistributedCache](https://github.com/grantcolley/best-stories-api/blob/main/src/BestStories.Api/Cache/DistributedCache.cs). 

>  **Note**
>
> Distributed Memory Cache is used for development and testing purposes.
> 
> In a production environment, the distributed cache should be hosted in a dedicated web API, and configured for the appropriate caching service e.g. Redis.

#### Local Cache
Local caching can be used if the web API is intended to run as a single application. There are several *flavours* to choose from.
- **[LockedCache](https://github.com/grantcolley/best-stories-api/blob/main/src/BestStories.Api/Cache/LockedCache.cs)** uses the `lock()` keyword for recycling and fetching a copy of the reference to the cache.
- **[SemaphoreSlimCache](https://github.com/grantcolley/best-stories-api/blob/main/src/BestStories.Api/Cache/SemaphoreSlimCache.cs)** asynchronously waits to enter the SemaphoreSlim `await _semaphore.WaitAsync()`
- **[VolatileCache](https://github.com/grantcolley/best-stories-api/blob/main/src/BestStories.Api/Cache/VolatileCache.cs)** uses `Interlocked.CompareExchange` for recycling the cache, and `Volatile.Read` for fetching a copy of the reference to it. See inline comments for more details.

#### Benchmarking GetStoryCacheAsync 
```C#
BenchmarkDotNet v0.13.6, Windows 11 (10.0.22621.1992/22H2/2022Update/SunValley2)
Intel Core i7-10750H CPU 2.60GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 7.0.306
  [Host]     : .NET 7.0.9 (7.0.923.32018), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.9 (7.0.923.32018), X64 RyuJIT AVX2

|                                Method |          Mean |        Error |       StdDev | Rank |   Gen0 | Allocated |
|-------------------------------------- |--------------:|-------------:|-------------:|-----:|-------:|----------:|
|        LockedCache_GetStoryCacheAsync |      34.52 ns |     0.717 ns |     1.559 ns |    1 | 0.0114 |      72 B |
|      VolatileCache_GetStoryCacheAsync |      40.77 ns |     1.151 ns |     3.265 ns |    2 | 0.0229 |     144 B |
| SemaphoreSlimCache_GetStoryCacheAsync |      69.46 ns |     1.405 ns |     2.773 ns |    3 | 0.0114 |      72 B |
|   DistributedCache_GetStoryCacheAsync | 136,347.64 ns | 2,717.418 ns | 6,077.896 ns |    4 | 2.6855 |   17648 B |
```

### Validation
An [endpoint filter](https://github.com/grantcolley/best-stories-api/tree/main/src/BestStories.Api/Filters) will validate consumers provide a valid number between 1 and the specified `CacheMaxSize`.

### Configuration
The [appsettings.json](https://github.com/grantcolley/best-stories-api/blob/main/src/appsettings.json) contains the following:

|Key|Description|
|---|-----------|
|HackerNewsApi|HackerNewsApi url|
|CacheMaxSize|Maximum stories to be cached. Used by the [BestStoriesBackgroundService](https://github.com/grantcolley/best-stories-api/blob/af0a170d747ec64b634587e06d8025701653edb1/src/BestStories.Api/Services/BestStoriesBackgroundService.cs#L48).|
|CacheRecycleDelay|The delay between each cache recycle in milliseconds. [BestStoriesBackgroundService](https://github.com/grantcolley/best-stories-api/blob/af0a170d747ec64b634587e06d8025701653edb1/src/BestStories.Api/Services/BestStoriesBackgroundService.cs#L60).|
|CacheRetryDelay|The delay between each attempt to read the cache if it is null. Used by the [BestStoriesService](https://github.com/grantcolley/best-stories-api/blob/af0a170d747ec64b634587e06d8025701653edb1/src/BestStories.Api/Services/BestStoriesService.cs#L38) when handling the request.|
|CacheMaxRetryAttempts|Number of attempts to read the cache if it is null before giving up. Used by the [BestStoriesService](https://github.com/grantcolley/best-stories-api/blob/af0a170d747ec64b634587e06d8025701653edb1/src/BestStories.Api/Services/BestStoriesService.cs#L54) when handling the request.|
|IsDistributedCache|Flag to indicate wether to set up distributed or local caching at startup. Used by [Program.cs](https://github.com/grantcolley/best-stories-api/blob/af0a170d747ec64b634587e06d8025701653edb1/src/BestStories.Api/Program.cs#L38) at startup.|

```C#
  "BestStoriesConfiguration": {
    "HackerNewsApi": "https://hacker-news.firebaseio.com/v0/",
    "CacheMaxSize": 200,
    "CacheRecycleDelay": 5000,
    "CacheRetryDelay": 500,
    "CacheMaxRetryAttempts": 5,
    "IsDistributedCache": true
  }
```

## Testing
### Unit Tests 
[BestStoriesApi.Tests](https://github.com/grantcolley/best-stories-api/tree/main/tests/BestStories.Api.Tests) contains the unit tests.

### Benchmarks
[BestStories.Api.Benchmarks](https://github.com/grantcolley/best-stories-api/blob/main/tests/BestStories.Api.Benchmarks/Program.cs) contains the benchmark test.

### Test Harness
[BestStoriesApi.Test.Harness](https://github.com/grantcolley/best-stories-api/blob/main/tests/BestStories.Api.Test.Harness/Program.cs) console application executes 1000 requests in 10 batches of 100 simultaneous requests. The number of batches, requests per batch and *url* can be changed. Run the *BestStoriesApi.Test.Harness.exe* from the output folder after the "Best Stories Api" is running.

>  **Warning**
>
> BestStoriesApi.Test.Harness does not replace proper load/stress testing.
> 
> For proper [load/stress testing](https://learn.microsoft.com/en-us/aspnet/core/test/load-tests) a recognised third party tool should be used.
