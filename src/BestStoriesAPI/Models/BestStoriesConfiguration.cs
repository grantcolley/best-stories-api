namespace BestStoriesAPI.Models
{
    public class BestStoriesConfiguration
    {
        public string? HackerNewsApi { get; set; }
        public int CacheMaxSize { get; set; }
        public int CacheExpiryInSeconds { get; set; }
    }
}