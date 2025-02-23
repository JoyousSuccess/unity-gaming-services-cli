using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using Unity.Services.Cli.Lobby.Handlers;
using Unity.Services.Cli.Lobby.Handlers.ImportExport;

namespace Unity.Services.Cli.Lobby.UnitTest.Handlers;

[TestFixture]
public class LobbyConfigTests
{
    const string k_TimestampFormat = "yyyy-MM-ddThh:mm:ttZ";
    const string k_DefaultStringSetting = "string-setting";
    const int k_DefaultIntSetting = 1;

    [Test]
    public void TryParse_SucceedsWithValidConfig()
    {
        var configResponse = NewDefaultConfig();
        var json = JsonConvert.SerializeObject(
            configResponse,
            new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

        var success = LobbyConfig.TryParse(json, out var lobbyConfig);
        Assert.True(success);
        Assert.NotNull(lobbyConfig);
        Assert.AreEqual(lobbyConfig.Id, configResponse.Configs.First().Id);
        Assert.AreEqual(2, lobbyConfig.Config.Count);
        Assert.True(lobbyConfig.Config.ContainsKey(nameof(MockLobbyConfig.StringSetting)));
        Assert.AreEqual(k_DefaultStringSetting,
            lobbyConfig.Config.GetValue(nameof(MockLobbyConfig.StringSetting)).Value<string>());
        Assert.True(lobbyConfig.Config.ContainsKey(nameof(MockLobbyConfig.IntSetting)));
        Assert.AreEqual(k_DefaultIntSetting,
            lobbyConfig.Config.GetValue(nameof(MockLobbyConfig.IntSetting)).Value<int>());
    }

    [Test]
    public void TryParse_FailsWithNoLobbyConfigValue()
    {
        var configResponse = NewDefaultConfig(includeMockConfig: false);
        var json = JsonConvert.SerializeObject(
            configResponse,
            new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

        var success = LobbyConfig.TryParse(json, out var lobbyConfig);
        Assert.False(success);
        Assert.Null(lobbyConfig);
    }

    [Test]
    public void TryParse_FailsWithNoConfigs()
    {
        var configResponse = new List<RemoteConfigResponse.Config>();
        var json = JsonConvert.SerializeObject(
            configResponse,
            new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

        var success = LobbyConfig.TryParse(json, out var lobbyConfig);
        Assert.False(success);
        Assert.Null(lobbyConfig);
    }

    static RemoteConfigResponse NewDefaultConfig(bool includeMockConfig = true)
    {
        var mockConfigJson = includeMockConfig ? JsonConvert.SerializeObject(
             new MockLobbyConfig()
            {
                StringSetting = k_DefaultStringSetting,
                IntSetting = k_DefaultIntSetting,
            }) : "{}";

        var now = DateTime.Now.ToString(k_TimestampFormat);
        var config = new RemoteConfigResponse.Config
        {
            ProjectId = Guid.NewGuid().ToString(),
            EnvironmentId = Guid.NewGuid().ToString(),
            Id = Guid.NewGuid().ToString(),
            Type = LobbyConstants.ConfigType,
            CreatedAt = now,
            UpdatedAt = now,
            Value = new List<RemoteConfigResponse.ConfigValue>
            {
                new RemoteConfigResponse.ConfigValue{
                    Key = LobbyConstants.ConfigKey,
                    Type = RemoteConfig.Types.ValueType.Json.ToString().ToLower(),
                    SchemaId = LobbyConstants.ConfigType,
                    Value = JObject.Parse(mockConfigJson)
                }
            }
        };

        return new RemoteConfigResponse
        {
            Configs = new List<RemoteConfigResponse.Config>
            {
                config
            }
        };
    }

    class MockLobbyConfig
    {
        public string StringSetting { get; set; }
        public int IntSetting { get; set; }
    }
}
