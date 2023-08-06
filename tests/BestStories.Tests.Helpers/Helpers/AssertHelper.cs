using BestStories.Core.Models;

namespace BestStoriesAPI.Tests.Helpers
{
    public static class AssertHelper
    {
        public static bool AreStoriesEqual(IEnumerable<Story> stories1, IEnumerable<Story> stories2)
        {
            if(stories1 == null || stories2 == null)
            {
                return false;
            }

            for(int i = 0; i < stories1.Count(); i++)
            {
                if(stories1.ElementAt(i).id != stories2.ElementAt(i).id)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
