# Best Stories Api

**Best Stories Api** is a RESTful API to retrieve the first *n* "best stories" from [Hacker News API](https://github.com/HackerNews/API). 

### Table of Contents
- [Observations](#observations)
- [Assumptions](#assumptions)
- [Solution](#solution)
- [Implementation](#implementation)
- [Testing](#testing)

## Observations
I conducted a simple test, first calling the endpoint to fetch the top 200 “best stories”, followed by calling the endpoint to fetch each story. The test was repeated with an interval of 5 seconds over a period of time.

The endpoint for returning the id’s of the “best stories” consistently returns 200 id’s, which appear to have been sorted by score in descending order.

`https://hacker-news.firebaseio.com/v0/beststories.json`

The story’s score is subject to change by the time the story has been fetched by calling the endpoint for individual stories, passing in the story’s id.

`https://hacker-news.firebaseio.com/v0/item/21233041.json`

Often the same story id’s were returned and in the same order i.e. the scores had either not changed at all, or the change was so slight and therefore didn’t not affect the story’s ranking.
New entrants to the top 200 “best stories” were less frequent.

## Assumptions
- There is no way to subscribe to score changes in stories.
- The endpoint for returning the id’s of the “best stories” from **Hacker News API**, will always return 200 id’s
- Like **Hacker News API**, **Best Stories Api** is open to the public and consumers will not be authenticated.
- There is [no rate limit](https://github.com/HackerNews/API#uri-and-versioning) on **Hacker News API** endpoints, so I assume no need to "back off" periodically.

## Solution
To efficiently service large numbers of requests without risking overloading of the **Hacker News API**:
- **Best Stories Api** will call the endpoint for fetching the top 200 “best stories” from **Hacker News API** periodically, caching the results between calls.
- Because of the indeterminate way each story’s score can be updated, after obtaining the id’s for top 200 “best stories” each story will be fetched. Once all stories have been fetched the cache will be recycled.
- The maximum cache size will be configurable.
- Consumers of the **Best Stories Api** endpoint will fetch the first *n* best stories from the cache.
- Validation will ensure consumers provide a valid number between 1 and the specified maximum cache size.
 
## Implementation
- **Best Stories Api** will expose a [Minimal API](https://github.com/grantcolley/best-stories-api/blob/main/src/Endpoints/BestStoriesEndpoints.cs) for obtaining the first n “best stories”, where n is the number of stories the requestor wants to retrieve.
```C#
app.MapGet("getbeststories/{count:int}", BestStoriesEndpoints.GetBestStories)
    .AddEndpointFilter<BestStoriesValidationFilter>();
```
- A [BackgroundService](https://github.com/grantcolley/best-stories-api/blob/main/src/Services/BestStoriesBackgroundService.cs) will be used to recycle the cache at a pre-configured interval.
- An [endpoint filter](https://github.com/grantcolley/best-stories-api/blob/main/src/Filters/BestStoriesValidationFilter.cs) will enforce validation.
- The following [configuarion](https://github.com/grantcolley/best-stories-api/blob/main/src/appsettings.json) settings will apply:

|Key|Description|
|---|-----------|
|CacheMaxSize|Maximum stories to be cached |
|CacheRecycleDelay|The delay between each cache recycle in milliseconds|
|CacheRetryDelay|The delay between each attempt to read the cache if it is null|
|CacheMaxSize|Number of attempts to read the cache if it is null before giving up|

```C#
  "BestStories": {
    "CacheMaxSize": 200,
    "CacheRecycleDelay": 5000,
    "CacheRetryDelay": 500,
    "CacheMaxRetryAttempts": 5
  }
```

## Testing

>  **Note** 
>
> Testing using [Postman](https://learning.postman.com/docs/getting-started/sending-the-first-request/) or the [BestStoriesApi.Test.Harness](https://github.com/grantcolley/best-stories-api/blob/main/tests/BestStoriesApi.Test.Harness/Program.cs) will first require launching the Best Stories Api solution in Visual Studio with the applicationUrl currently configured to `https://localhost:7240`

- Unit tests can be found in the [BestStoriesApi.Tests](https://github.com/grantcolley/best-stories-api/tree/main/tests/BestStoriesApi.Tests) project.
- Test individual requests using [Postman](https://learning.postman.com/docs/getting-started/sending-the-first-request/) e.g. `https://localhost:7240/getbeststories/10`
- Run the [BestStoriesApi.Test.Harness](https://github.com/grantcolley/best-stories-api/blob/main/tests/BestStoriesApi.Test.Harness/Program.cs) console application to execute 1000 requests in 10 batches of 100 simultaneous requests. 
Compile the solution then run the *BestStoriesApi.Test.Harness.exe* from the output folder.

>  **Warning**
>
> For proper [load/stress testing](https://learn.microsoft.com/en-us/aspnet/core/test/load-tests) a recognised third party tool should be used.
