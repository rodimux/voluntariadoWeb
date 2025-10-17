using Xunit;

namespace Volun.Tests.Integration;

[CollectionDefinition(nameof(IntegrationTestsCollection), DisableParallelization = true)]
public sealed class IntegrationTestsCollection : ICollectionFixture<SecuredWebApplicationFactory>
{
}

