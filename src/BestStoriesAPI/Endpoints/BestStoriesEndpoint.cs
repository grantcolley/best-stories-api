using BestStoriesAPI.Interfaces;
using BestStoriesAPI.Models;

namespace BestStoriesAPI.Endpoints
{
    internal static class BestStoriesEndpoint
    {
        internal static async Task<IResult> GetBestStories(int count, IBestStoriesService bestStoriesService, CancellationToken cancellationToken)
        {
            try
            {
                IEnumerable<Story>? bestStories = await bestStoriesService.GetBestStoriesAsync(count, cancellationToken)
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