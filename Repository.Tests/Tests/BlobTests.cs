using Xunit.Abstractions;

namespace Repository.Tests
{
    [Collection("Database Collection")]
    public class BlobTests : IDisposable
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public BlobTests(ITestOutputHelper testOutputHelper) 
        {
            _testOutputHelper = testOutputHelper;

        }
        public void Dispose()
        {
            
        }
    }
}
