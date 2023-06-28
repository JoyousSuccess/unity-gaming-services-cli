using Unity.Services.Gateway.CloudCodeApiV1.Generated.Model;

namespace Unity.Services.Cli.CloudCode.Parameters;

interface ICloudScriptParametersParser
{
    public List<ScriptParameter> ParseToScriptParameters(string parameterJsonString);
}
