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

        public static IEnumerable<Story> GetUpdatedBestStories()
        {
            string jsonStories = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "StoriesCache.txt"));
            IEnumerable<Story>? stories = JsonSerializer.Deserialize<IEnumerable<Story>>(jsonStories) ?? throw new NullReferenceException(nameof(jsonStories));
            List<Story> updatedStories = new(stories);

            updatedStories.AddRange(new[]
            {
                new Story{ id = 1, score = 1001, title = "New Story 1" },
                new Story{ id = 2, score = 1002, title = "New Story 2" },
                new Story{ id = 3, score = 1003, title = "New Story 3" }
            });

            return updatedStories;
        }
    }
}
