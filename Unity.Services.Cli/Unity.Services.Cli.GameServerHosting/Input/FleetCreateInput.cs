using System.CommandLine;
using System.CommandLine.Parsing;
using Unity.Services.Cli.Common.Input;
using Unity.Services.Gateway.GameServerHostingApiV1.Generated.Model;

namespace Unity.Services.Cli.GameServerHosting.Input;

public class FleetCreateInput : CommonInput
{
    public const string NameKey = "--name";
    public const string OsFamilyKey = "--os-family";
    public const string RegionsKey = "--region-id";
    public const string BuildConfigurationsKey = "--build-configuration-id";

    public static readonly Option<string> FleetNameOption = new(
        NameKey,
        "The name of the fleet."
    )
    {
        IsRequired = true
    };

    public static readonly Option<FleetCreateRequest.OsFamilyEnum> FleetOsFamilyOption = new(
        OsFamilyKey,
        "The OS family that the build is based on. Only LINUX currently supported."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string[]> FleetRegionsOption = new(
        RegionsKey,
        "Template fleet region ID to be added to this fleet. Can be supplied more than once."
    )
    {
        AllowMultipleArgumentsPerToken = true,
        IsRequired = true
    };

    public static readonly Option<long[]> FleetBuildConfigurationsOption = new(
        BuildConfigurationsKey,
        "Build configuration to be added to the fleet. Can be supplied more than once."
    )
    {
        AllowMultipleArgumentsPerToken = true,
        IsRequired = true
    };

    static FleetCreateInput()
    {
        FleetRegionsOption.AddValidator(ValidateRegionIds);
        FleetOsFamilyOption.AddValidator(ValidateOsFamilyEnum);
    }

    [InputBinding(nameof(FleetNameOption))]
    public string? FleetName { get; set; }

    [InputBinding(nameof(FleetOsFamilyOption))]
    public FleetCreateRequest.OsFamilyEnum? OsFamily { get; set; }

    [InputBinding(nameof(FleetRegionsOption))]
    public string[]? Regions { get; set; }

    [InputBinding(nameof(FleetBuildConfigurationsOption))]
    public long[]? BuildConfigurations { get; set; }

    static void ValidateRegionIds(OptionResult result)
    {
        var value = result.GetValueOrDefault<string[]>();
        foreach (var region in value!)
        {
            try
            {
                Guid.Parse(region);
            }
            catch (Exception)
            {
                result.ErrorMessage = $"Region '{region}' not a valid UUID.";
            }
        }
    }

    static void ValidateOsFamilyEnum(OptionResult result)
    {
        try
        {
            result.GetValueOrDefault();
        }
        catch (Exception)
        {
            result.ErrorMessage = $"Invalid option for --os-family. Did you mean one of the following? {string.Join(", ", Enum.GetNames<FleetCreateRequest.OsFamilyEnum>())}";
        }
    }
}
