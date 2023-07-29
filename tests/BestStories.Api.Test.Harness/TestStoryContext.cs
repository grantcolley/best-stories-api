using BestStories.Api.Core.Models;

namespace BestStories.Api.Test.Harness
{
    public class TestStoryContext
    {
        public int Count { get; set; }
        public TimeSpan Duration { get; set; }
        public bool HasErrored { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public IEnumerable<Story>? Stories { get; set; }
    }
}
