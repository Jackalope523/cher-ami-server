using System;
using Xunit;

namespace Core.Tests
{
    [CollectionDefinition("Core Collection")]
    public class CoreCollection : ICollectionFixture<CoreCollection>
    {
    }
}

