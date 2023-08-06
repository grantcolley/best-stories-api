using BestStories.Core.Models;
using System.Text;
using System.Text.Json;

namespace BestStoriesAPI.Tests.Helpers
{
    public static class DataUtility
    {
        public static IEnumerable<Story> GetBestStories()
        {
            string jsonStories = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Stories.txt"));
            return JsonSerializer.Deserialize<IEnumerable<Story>>(jsonStories) ?? throw new NullReferenceException(nameof(jsonStories));
        }

        public static byte[]? GetBestStoriesAsByteArray()
        {
            string stories = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Stories.txt"));
            return Encoding.UTF8.GetBytes(stories);
        }
    }
}
