using BestStories.Core.Static;
using BestStoriesAPI.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace BestStoriesAPI.Filters
{
    internal class BestStoriesValidationFilter : IEndpointFilter
    {
        private readonly IDistributedCache _distributedCache;
        private readonly BestStoriesConfiguration _bestStoriesConfiguration;
        private readonly string _errorMessage = "Specify number of best stories to fetch between 1 and ";

        public BestStoriesValidationFilter(
            IOptions<BestStoriesConfiguration> bestStoriesConfiguration,
            IDistributedCache distributedCache) 
        {
            if (bestStoriesConfiguration == null) throw new ArgumentNullException(nameof(bestStoriesConfiguration));

            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));

            _bestStoriesConfiguration = bestStoriesConfiguration.Value;
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
            int maxCacheSize;

            byte[]? cacheSize = await _distributedCache.GetAsync(Constants.DISTRIBUTED_CACHE_MAX_SIZE, context.HttpContext.RequestAborted)
                .ConfigureAwait(false);

            if (cacheSize != null)
            {
                maxCacheSize = BitConverter.ToInt32(cacheSize, 0);
            }
            else
            {
                maxCacheSize = _bestStoriesConfiguration.DefaultCacheMaxSize;
            }

            object? arg = context.Arguments.SingleOrDefault(a => a?.GetType() == typeof(int));
            
            if(arg == null) 
            {
                return Results.BadRequest(_errorMessage + maxCacheSize.ToString());
            }

            int count = (int)arg;

            if (count == 0
                || count > maxCacheSize)
            {
                return Results.BadRequest(_errorMessage + maxCacheSize.ToString());
            }

            return await next(context);
        }
    }
}
