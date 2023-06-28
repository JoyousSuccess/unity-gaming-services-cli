using NUnit.Framework;
using Moq;
using Microsoft.Extensions.Logging;
using Unity.Services.Cli.Lobby.Handlers;
using Unity.Services.Cli.TestUtils;
using Unity.Services.Cli.Lobby.Input;
using Unity.Services.Cli.Common.Exceptions;
using Unity.Services.Cli.Lobby.Service;
using Unity.Services.MpsLobby.LobbyApiV1.Generated.Model;
using Newtonsoft.Json;
using Unity.Services.Cli.Common.Logging;
using Unity.Services.Cli.Common.Utils;

namespace Unity.Services.Cli.Lobby.UnitTest.Handlers;

[TestFixture]
class BulkUpdateLobbyHandlerTests
{
    Mock<ILogger> m_MockLogger = new();
    Mock<ILobbyService> m_MockLobby = new();
    Mock<IUnityEnvironment> m_MockUnityEnvironment = new();

    [SetUp]
    public void SetUp()
    {
        m_MockLobby = new();
        m_MockLobby.Setup(l =>
                l.BulkUpdateLobbyAsync(
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    CancellationToken.None))
            .Returns(Task.FromResult(new MpsLobby.LobbyApiV1.Generated.Model.Lobby()));
    }

    [Test]
    public void BulkUpdateLobbyHandler_HandlesInputAndLogsOnSuccess()
    {
        var bulkUpdateRequest = new BulkUpdateRequest();
        var inputBody = JsonConvert.SerializeObject(bulkUpdateRequest);
        var input = new RequiredBodyInput()
        {
            JsonFileOrBody = inputBody,
        };
        Assert.DoesNotThrowAsync(async () => await BulkUpdateLobbyHandler.BulkUpdateLobbyAsync(input, m_MockUnityEnvironment.Object, m_MockLobby.Object, m_MockLogger.Object, default));
        TestsHelper.VerifyLoggerWasCalled(m_MockLogger, LogLevel.Critical, LoggerExtension.ResultEventId, Times.Once);
    }

    [Test]
    public void BulkUpdateLobbyHandler_MissingBodyThrowsException()
    {
        var input = new RequiredBodyInput();
        Assert.ThrowsAsync<CliException>(async () => await BulkUpdateLobbyHandler.BulkUpdateLobbyAsync(input, m_MockUnityEnvironment.Object, m_MockLobby.Object, m_MockLogger.Object, default));
    }
}
