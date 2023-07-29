namespace BestStoriesApi.Exceptions
{
    public class BestStoryException : Exception
    {
        public BestStoryException()
        {
        }

        public BestStoryException(string message)
            : base(message)
        {
        }

        public BestStoryException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
