using BestStories.Api.Core.Interfaces;
using BestStories.Api.Core.Models;

namespace BestStories.Api.Endpoints
{
    public static class BestStoriesEndpoint
    {
        public static async Task<IResult> GetBestStories(int count, IBestStoriesService bestStoriesService, CancellationToken token)
        {
            try
            {
                IEnumerable<Story> bestStories = await bestStoriesService.GetBestStoriesAsync(count, token)
                    .ConfigureAwait(false);

                return Results.Ok(bestStories);
            }
            catch (Exception)
            {
                // Exceptions thrown from bestStoriesService.GetBestStoriesAsync(count)
                // have already been logged so simply return Status500InternalServerError.

                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}