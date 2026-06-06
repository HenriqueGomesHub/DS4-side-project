using DS4Bridge.Core.Configuration;
using DS4Bridge.Core.Mapping;
using FluentAssertions;
using Xunit;

namespace DS4Bridge.Core.Tests.Configuration;

public class ConfigStoreTests : IDisposable
{
    private readonly string _tempDir;

    public ConfigStoreTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "ds4bridge-tests-" + Guid.NewGuid());
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, true); } catch { /* best effort */ }
    }

    [Fact]
    public void Load_when_file_missing_creates_default_and_persists()
    {
        var store = new ConfigStore(_tempDir);
        var cfg = store.Load();

        cfg.ActiveProfile.Should().Be("default");
        cfg.Profiles.Should().ContainKey("default");
        cfg.Profiles["default"].LeftStickDeadzone.Should().BeApproximately(0.08, 0.0001);
        File.Exists(Path.Combine(_tempDir, "config.json")).Should().BeTrue();
    }

    [Fact]
    public void Save_then_load_round_trips_profile_edits()
    {
        var store = new ConfigStore(_tempDir);
        var cfg = store.Load() with { StartMinimized = false };
        cfg = cfg with
        {
            Profiles = new Dictionary<string, MappingProfile>(cfg.Profiles)
            {
                ["default"] = cfg.Profiles["default"] with { LeftStickDeadzone = 0.20 }
            }
        };

        store.Save(cfg);

        var store2 = new ConfigStore(_tempDir);
        var reloaded = store2.Load();
        reloaded.StartMinimized.Should().BeFalse();
        reloaded.Profiles["default"].LeftStickDeadzone.Should().Be(0.20);
    }

    [Fact]
    public void Load_with_corrupt_file_returns_defaults_and_backs_up()
    {
        var path = Path.Combine(_tempDir, "config.json");
        File.WriteAllText(path, "{not valid json");

        var store = new ConfigStore(_tempDir);
        var cfg = store.Load();

        cfg.ActiveProfile.Should().Be("default");
        Directory.GetFiles(_tempDir, "config.corrupt.*.json").Should().NotBeEmpty();
    }
}
