using BestStories.Api.Core.Interfaces;
using BestStories.Api.Core.Models;

namespace BestStories.Api.Cache
{
    public class ReaderWriterLockSlimCache : IBestStoriesCache
    {
        private readonly ReaderWriterLockSlim _readerWriterLockSlim = new();
        private readonly ILogger<ReaderWriterLockSlimCache> _logger;
        private IEnumerable<Story>? _storyCache = null;

        public ReaderWriterLockSlimCache(ILogger<ReaderWriterLockSlimCache> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task RecycleCacheAsync(IEnumerable<Story> stories)
        {
            try
            {
                _readerWriterLockSlim.EnterWriteLock();

                _storyCache = stories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            finally
            {
                _readerWriterLockSlim.ExitWriteLock();
            }

            return Task.CompletedTask;
        }

        public Task<IEnumerable<Story>?> GetStoryCacheAsync()
        {
            try
            {
                _readerWriterLockSlim.EnterReadLock();

                return Task.FromResult(_storyCache);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                return Task.FromException<IEnumerable<Story>?>(ex);
            }
            finally
            {
                _readerWriterLockSlim.ExitReadLock();
            }
        }
    }
}
