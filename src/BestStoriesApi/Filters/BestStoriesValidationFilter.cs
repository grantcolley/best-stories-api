namespace BestStoriesApi.Filters
{
    public class BestStoriesValidationFilter : IEndpointFilter
    {
        private readonly int _cacheMaxSize;
        private readonly string _errorMessage;

        public BestStoriesValidationFilter(IConfiguration configuration) 
        {
            _cacheMaxSize = configuration.GetValue<int>("BestStories:CacheMaxSize");

            _errorMessage = $"Specify number of best stories to fetch between 1 and {_cacheMaxSize}";
        }

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            object? arg = context.Arguments.SingleOrDefault(a => a?.GetType() == typeof(int));
            
            if(arg == null) 
            {
                return Results.BadRequest(_errorMessage);
            }

            int count = (int)arg;

            if (count == 0
                || count > _cacheMaxSize)
            {
                return Results.BadRequest(_errorMessage);
            }

            return await next(context);
        }
    }
}
