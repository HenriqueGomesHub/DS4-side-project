using System.Text.Json;
using DS4Bridge.Core.Mapping;

namespace DS4Bridge.Core.Configuration;

public sealed class ConfigStore
{
    private readonly string _directory;
    private readonly string _filePath;

    public ConfigStore(string directory)
    {
        _directory = directory;
        _filePath = Path.Combine(directory, "config.json");
    }

    public string Directory => _directory;
    public string LogDirectory => Path.Combine(_directory, "logs");

    public static ConfigStore Default()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DS4Bridge");
        System.IO.Directory.CreateDirectory(dir);
        return new ConfigStore(dir);
    }

    public AppConfig Load()
    {
        if (!File.Exists(_filePath))
        {
            var fresh = new AppConfig();
            Save(fresh);
            return fresh;
        }

        try
        {
            var json = File.ReadAllText(_filePath);
            var cfg = JsonSerializer.Deserialize<AppConfig>(json, MappingProfile.JsonOptions);
            if (cfg is null || cfg.Profiles is null || !cfg.Profiles.ContainsKey(cfg.ActiveProfile))
                throw new InvalidDataException("Config missing active profile");
            return cfg;
        }
        catch (Exception)
        {
            BackupCorrupt();
            var fresh = new AppConfig();
            Save(fresh);
            return fresh;
        }
    }

    public void Save(AppConfig config)
    {
        System.IO.Directory.CreateDirectory(_directory);
        var json = JsonSerializer.Serialize(config, MappingProfile.JsonOptions);
        var tmp = _filePath + ".tmp";
        File.WriteAllText(tmp, json);
        File.Move(tmp, _filePath, overwrite: true);
    }

    private void BackupCorrupt()
    {
        if (!File.Exists(_filePath)) return;
        var stamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var dest = Path.Combine(_directory, $"config.corrupt.{stamp}.json");
        try { File.Move(_filePath, dest); } catch { /* best effort */ }
    }
}
