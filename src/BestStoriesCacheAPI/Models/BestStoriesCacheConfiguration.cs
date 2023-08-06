namespace BestStoriesCacheAPI.Models
{
    public class BestStoriesCacheConfiguration
    {
        public string? HackerNewsApi { get; set; }
        public int CacheMaxSize { get; set; }
        public int CacheExpiryInSeconds { get; set; }
    }
}