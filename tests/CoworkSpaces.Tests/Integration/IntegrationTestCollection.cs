namespace CoworkSpaces.Tests.Integration;

[CollectionDefinition(Name, DisableParallelization = true)]
public class IntegrationTestCollection : ICollectionFixture<SqlServerWebApplicationFactory>
{
    public const string Name = "Integration";
}
