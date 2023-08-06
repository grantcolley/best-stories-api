using BestStoriesCacheAPI.Models;
using Microsoft.Extensions.Options;

namespace BestStoriesAPI.Filters
{
    internal class BestStoriesCacheValidationFilter : IEndpointFilter
    {
        private readonly BestStoriesCacheConfiguration _bestStoriesCacheConfiguration;
        private readonly string _errorMessage;

        public BestStoriesCacheValidationFilter(IOptions<BestStoriesCacheConfiguration> bestStoriesCacheConfiguration) 
        {
            if (bestStoriesCacheConfiguration == null) throw new ArgumentNullException(nameof(bestStoriesCacheConfiguration));

            _bestStoriesCacheConfiguration = bestStoriesCacheConfiguration.Value;

            _errorMessage = $"Specify number of best stories to fetch between 1 and {_bestStoriesCacheConfiguration.CacheMaxSize}";
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
                || count > _bestStoriesCacheConfiguration.CacheMaxSize)
            {
                return Results.BadRequest(_errorMessage);
            }

            return await next(context);
        }
    }
}
