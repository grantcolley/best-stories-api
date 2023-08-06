using BestStories.Core.Models;
using BestStoriesCacheAPI.Interfaces;

namespace BestStoriesCacheAPI.Endpoints
{
    internal static class BestStoriesCacheEndpoint
    {
        internal static async Task<IResult> GetBestStories(int count, IBestStoriesCacheService bestStoriesCacheService, CancellationToken cancellationToken)
        {
            try
            {
                IEnumerable<Story>? bestStories = await bestStoriesCacheService.GetBestStoriesAsync(count, cancellationToken)
                    .ConfigureAwait(false);

                return Results.Ok(bestStories);
            }
            catch (Exception)
            {
                // Exceptions thrown from bestStoriesService.GetBestStoriesAsync(count, token)
                // have already been logged so simply return Status500InternalServerError.

                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}