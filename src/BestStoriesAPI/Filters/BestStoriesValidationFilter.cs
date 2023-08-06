using BestStoriesAPI.Models;
using Microsoft.Extensions.Options;

namespace BestStoriesAPI.Filters
{
    internal class BestStoriesValidationFilter : IEndpointFilter
    {
        private readonly BestStoriesConfiguration _bestStoriesConfiguration;
        private readonly string _errorMessage;

        public BestStoriesValidationFilter(IOptions<BestStoriesConfiguration> bestStoriesConfiguration) 
        {
            if (bestStoriesConfiguration == null) throw new ArgumentNullException(nameof(bestStoriesConfiguration));

            _bestStoriesConfiguration = bestStoriesConfiguration.Value;

            _errorMessage = $"Specify number of best stories to fetch between 1 and {_bestStoriesConfiguration.CacheMaxSize}";
        }

        /// <summary>
        /// Filters the request to validate the specified number of best stories requested
        /// is between 1 and the CacheMaxSize set in the BestStoriesConfiguration.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
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
