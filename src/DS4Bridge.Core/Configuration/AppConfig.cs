using DS4Bridge.Core.Mapping;

namespace DS4Bridge.Core.Configuration;

public sealed record AppConfig
{
    public string ActiveProfile { get; init; } = "default";
    public IReadOnlyDictionary<string, MappingProfile> Profiles { get; init; }
        = new Dictionary<string, MappingProfile> { ["default"] = MappingProfile.CreateDefault() };
    public bool StartMinimized { get; init; } = true;
    public bool ExclusiveMode { get; init; } // v2 flag, surface but ignore for now
}
