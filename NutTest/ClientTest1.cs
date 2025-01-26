using Microsoft.Extensions.Configuration;

namespace nutwinsvc.Tests;

[TestClass]
public sealed class ClientTest1
{

    readonly NutClient.NutClient client;

    public ClientTest1()
    {
        var builder = new ConfigurationBuilder()
            .AddUserSecrets<NutWinSvc.NutOptions>();
        var configuration = builder.Build();
        string host = configuration.GetValue<string>("NutWinSvc:Host") ?? string.Empty;
        string upsName = configuration.GetValue<string>("NutWinSvc:UpsName") ?? string.Empty;
        string username = configuration.GetValue<string>("NutWinSvc:Username") ?? string.Empty;
        string password = configuration.GetValue<string>("NutWinSvc:Password") ?? string.Empty;
        client = new(host, upsName, username, password);
    }

    [TestInitialize]
    public void Initialize()
    {

    }

    [TestMethod]
    public async Task Open()
    {
        await client.OpenAsync(CancellationToken.None);
        Assert.IsTrue(client.Connected);
    }

    [TestCleanup]
    public void Cleanup()
    {
        client?.Dispose();
    }
}
