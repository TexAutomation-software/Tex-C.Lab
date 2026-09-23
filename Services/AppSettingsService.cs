using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PCRA.Services;

public class AppSettingsService
{
    private static readonly string SettingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"PCRA");
    private static readonly string SettingsFile = Path.Combine(SettingsDirectory,"settings.json");
    public string OutputFolder { get; set; } = @"C:\PCRA\Output";

    public void Load()
    {
        if (!File.Exists(SettingsFile))
        {
            Save();
            return;
        }

        var json = File.ReadAllText(SettingsFile);

        var settings = JsonSerializer.Deserialize<AppSettingsService>(json);

        if (settings == null)
            return;

        OutputFolder = settings.OutputFolder;
    }

    public void Save()
    {
        Directory.CreateDirectory(SettingsDirectory);

        var json =
            JsonSerializer.Serialize(
                this,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

        File.WriteAllText(SettingsFile, json);
    }
}
