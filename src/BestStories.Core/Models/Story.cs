namespace BestStories.Core.Models
{
    public class Story
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
