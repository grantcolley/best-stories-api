using BestStories.Api.Core.Interfaces;
using BestStories.Api.Core.Models;
using BestStories.Api.Core.Static;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System.Text.Json;

namespace BestStories.Api.Cache
{
    public class DistributedCache : IBestStoriesCache
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<DistributedCache> _logger;

        public DistributedCache(IDistributedCache distributedCache, ILogger<DistributedCache> logger) 
        {
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task RecycleCacheAsync(IEnumerable<Story> stories)
        {
            try
            {
                var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(stories));

                await _distributedCache.SetAsync(
                    Constants.DISTRIBUTED_CACHE, bytes, new DistributedCacheEntryOptions())
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        public async Task<IEnumerable<Story>?> GetStoryCacheAsync()
        {
            try
            {
                byte[]? bytes = await _distributedCache.GetAsync(Constants.DISTRIBUTED_CACHE)
                    .ConfigureAwait(false);

                if (bytes == null)
                {
                    return null;
                }

                return JsonSerializer.Deserialize<IEnumerable<Story>>(bytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                return await Task.FromException<IEnumerable<Story>?>(ex);
            }
        }
    }
}
