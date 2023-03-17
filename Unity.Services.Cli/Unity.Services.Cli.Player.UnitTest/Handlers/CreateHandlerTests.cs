using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Spectre.Console;
using Unity.Services.Cli.Player.Handlers;
using Unity.Services.Cli.Player.Input;
using Unity.Services.Cli.Player.Service;
using Unity.Services.Cli.Common.Console;
using Unity.Services.Cli.TestUtils;
using Unity.Services.Gateway.PlayerAuthApiV1.Generated.Model;

namespace Unity.Services.Cli.Player.UnitTest.Handlers;

public class CreateHandlerTests
{
    private readonly Mock<IPlayerService>? m_MockPlayerService = new();
    private readonly Mock<ILogger>? m_MockLogger = new();
    private const string k_PlayerId = "player-id";
    private const string k_ProjectId = "abcd1234-ab12-cd34-ef56-abcdef123456";

    [SetUp]
    public void SetUp()
    {
        m_MockPlayerService.Reset();
        m_MockLogger.Reset();
    }

    [Test]
    public async Task CreateAsync_CallsLoadingIndicator()
    {
        Mock<ILoadingIndicator> mockLoadingIndicator = new Mock<ILoadingIndicator>();

        await CreateHandler.CreateAsync(null!,  null!, null!, mockLoadingIndicator.Object, CancellationToken.None);

        mockLoadingIndicator.Verify(ex => ex
            .StartLoadingAsync(It.IsAny<string>(), It.IsAny<Func<StatusContext?,Task>>()), Times.Once);
    }

    [Test]
    public async Task CreateHandler_Valid()
    {
        PlayerInput input = new()
        {
            CloudProjectId = k_ProjectId,
        };

        m_MockPlayerService?.Setup(x => x.CreateAsync(k_ProjectId,
            CancellationToken.None)).ReturnsAsync(new PlayerAuthAuthenticationResponse(user: new PlayerAuthUser(id: k_PlayerId)));

        await CreateHandler.CreateAsync(
            input,
            m_MockPlayerService!.Object,
            m_MockLogger!.Object,
            CancellationToken.None
        );

        m_MockPlayerService.Verify(s => s.CreateAsync(k_ProjectId,CancellationToken.None), Times.Once);
        TestsHelper.VerifyLoggerWasCalled(m_MockLogger, LogLevel.Information, null, Times.Once, $"Player '{k_PlayerId}' created.");
    }
}
