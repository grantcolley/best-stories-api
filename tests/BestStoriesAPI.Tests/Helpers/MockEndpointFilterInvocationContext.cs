using Microsoft.AspNetCore.Http;

namespace BestStoriesAPI.Tests.Helpers
{
    public class MockEndpointFilterInvocationContext : EndpointFilterInvocationContext
    {
        public MockEndpointFilterInvocationContext() 
        {
            Arguments = new List<object?>();
        }

        public override HttpContext HttpContext => new MockHttpContext();

        public override IList<object?> Arguments { get; }

        public override T GetArgument<T>(int index)
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8603 // Possible null reference return.
            return (T)Arguments[index];
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }
    }
}
