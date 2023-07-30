namespace BestStories.Api.Core.Models
{
    public class BestStoriesConfiguration
    {
        public string? HackerNewsApi { get; set; }
        public int CacheMaxSize { get; set; }
        public int CacheRecycleDelay { get; set; }
        public int CacheRetryDelay { get; set; }
        public int CacheMaxRetryAttempts { get; set; }
        public bool IsDistributedCache { get; set; }
        public bool LaunchDistributedCache { get; set; }
    }
}