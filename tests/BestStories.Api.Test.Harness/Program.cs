using BestStories.Api.Core.Models;
using BestStories.Api.Test.Harness;
using System.Diagnostics;
using System.Text.Json;
using static System.Net.WebRequestMethods;

string endpointUrl = "https://localhost:7240/";
int totalTestRequests = 100;
int totalIterations = 10;

Console.WriteLine("################################");
Console.WriteLine("BestStories.Api.Test.Harness");
Console.WriteLine("################################");
Console.WriteLine("");
Console.WriteLine("");
Console.WriteLine($"This test harness will send {totalTestRequests * totalIterations} requests to the BesStoriesApi getbeststories endpoint in {totalIterations} batches of {totalTestRequests} simultaneous requests.");
Console.WriteLine($"The ");
Console.WriteLine("");
Console.WriteLine("");
Console.WriteLine("### START TEST");

Console.WriteLine("");
Console.WriteLine($"Generate {totalTestRequests} random `counts` each between 1 and 200");

Random random = new();
List<int> randomCounts = new();

for (int i = 0; i < totalTestRequests; i++)
{
    randomCounts.Add(random.Next(1, 200));
}

Console.WriteLine("");
Console.WriteLine("Sending requests");

HttpClient httpClient = new()
{
    BaseAddress = new Uri(endpointUrl)
};

int iteration = 1;

Stopwatch testStopwatch = Stopwatch.StartNew();

while (iteration < totalIterations + 1)
{
    Console.WriteLine("");
    Console.WriteLine($"Batch {iteration}");
    Console.WriteLine("");

    Stopwatch iterationStopwatch = Stopwatch.StartNew();

    Task<TestStoryContext>[] bestStories = randomCounts.Select(count =>
    {
        return GetBestStoriesAsync(httpClient, count);
    }).ToArray();

    TestStoryContext[] testStoryContexts = await Task.WhenAll(bestStories);

    iterationStopwatch.Stop();

    foreach (TestStoryContext testStoryContext in testStoryContexts)
    {
        if (testStoryContext.HasErrored)
        {
            Console.WriteLine(string.Join(" ", "Request first", testStoryContext.Count, testStoryContext.Count == 1 ? "story" : "stories", testStoryContext.ErrorMessage));
        }
        else if (testStoryContext.Count == testStoryContext.Stories?.Count())
        {
            Console.WriteLine(string.Join(" ", "Request first", testStoryContext.Count, testStoryContext.Count == 1 ? "story" : "stories", testStoryContext.Duration));
        }
    }

    IEnumerable<TestStoryContext> successfulTestStoryContexts = testStoryContexts.Where(ctx => !ctx.HasErrored);
    IEnumerable<TestStoryContext> failedTestStoryContexts = testStoryContexts.Where(ctx => ctx.HasErrored);

    double averageTicks = successfulTestStoryContexts.Select(ctx => ctx.Duration).Average(timeSpan => timeSpan.Ticks);
    TimeSpan average = new(Convert.ToInt64(averageTicks));

    Console.WriteLine("");
    Console.WriteLine($"Batch {iteration}");
    Console.WriteLine("");
    Console.WriteLine($"{successfulTestStoryContexts.Count()} successful requests averaging {average}");
    Console.WriteLine($"{failedTestStoryContexts.Count()} failed requests");
    Console.WriteLine($"Duration for {totalTestRequests} requests in batch no. {iteration} {iterationStopwatch.Elapsed}");

    iteration++;
}

testStopwatch.Stop();

Console.WriteLine("");
Console.WriteLine($"Total duration for {totalTestRequests * totalIterations} requests in {totalIterations} batches of {totalTestRequests} simultaneous requests");
Console.WriteLine(testStopwatch.Elapsed);

Console.WriteLine("");
Console.WriteLine("");
Console.WriteLine("### END TEST");
Console.ReadLine();

static async Task<TestStoryContext> GetBestStoriesAsync(HttpClient httpClient, int count)
{
    TestStoryContext testStoryContext = new() { Count = count };

    Stopwatch stopwatch = Stopwatch.StartNew();

    try
    {
        using HttpResponseMessage response = await httpClient.GetAsync($"getbeststories/{count}", CancellationToken.None);

        testStoryContext.Stories = await JsonSerializer.DeserializeAsync<IEnumerable<Story>>(
            await response.Content.ReadAsStreamAsync(CancellationToken.None).ConfigureAwait(false),
            JsonSerializerOptions.Default, CancellationToken.None).ConfigureAwait(false) ?? throw new NullReferenceException();
    }
    catch (Exception ex)
    {
        testStoryContext.HasErrored = true;
        testStoryContext.ErrorMessage = ex.Message;
    }

    stopwatch.Stop();

    testStoryContext.Duration = stopwatch.Elapsed;

    return testStoryContext;
}
