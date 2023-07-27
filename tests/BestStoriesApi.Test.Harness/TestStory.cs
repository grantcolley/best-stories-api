namespace BestStoriesApi.Test.Harness
{
    public class TestStory
    {
        public int id { get; set; }
        public string title { get; set; } = string.Empty;
        public string url { get; set; } = string.Empty;
        public string by { get; set; } = string.Empty;
        public long time { get; set; }
        public int score { get; set; }
        public int descendants { get; set; }
    }
}
