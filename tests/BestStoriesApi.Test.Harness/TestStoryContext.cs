namespace BestStoriesApi.Test.Harness
{
    public class TestStoryContext
    {
        public int Count { get; set; }
        public TimeSpan Duration { get; set; }
        public bool HasErrored { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public IEnumerable<TestStory>? Stories { get; set; }
    }
}
