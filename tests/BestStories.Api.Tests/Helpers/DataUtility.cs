using BestStories.Api.Core.Models;
using System.Text.Json;

namespace BestStories.Api.Tests.Helpers
{
    public static class DataUtility
    {
        public static IEnumerable<int> GetBestStoryIds()
        {
            string jsonStoryIds = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "BestStoryIds.txt"));
            return JsonSerializer.Deserialize<IEnumerable<int>>(jsonStoryIds) ?? throw new NullReferenceException(nameof(jsonStoryIds));
        }

        public static IEnumerable<Story> GetBestStories()
        {
            string jsonStories = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "StoriesCache.txt"));
            return JsonSerializer.Deserialize<IEnumerable<Story>>(jsonStories) ?? throw new NullReferenceException(nameof(jsonStories));
        }
    }
}
