using BestStories.Api.Core.Models;
using Microsoft.Extensions.Options;

namespace BestStories.Api.Filters
{
    public class BestStoriesValidationFilter : IEndpointFilter
    {
        private readonly BestStoriesConfiguration _bestStoriesConfiguration;
        private readonly string _errorMessage;

        public BestStoriesValidationFilter(IOptions<BestStoriesConfiguration> bestStoriesConfiguration) 
        {
            if (bestStoriesConfiguration == null) throw new ArgumentNullException(nameof(bestStoriesConfiguration));

            _bestStoriesConfiguration = bestStoriesConfiguration.Value;

            _errorMessage = $"Specify number of best stories to fetch between 1 and {_bestStoriesConfiguration.CacheMaxSize}";
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
                || count > _bestStoriesConfiguration.CacheMaxSize)
            {
                return Results.BadRequest(_errorMessage);
            }

            return await next(context);
        }
    }
}
