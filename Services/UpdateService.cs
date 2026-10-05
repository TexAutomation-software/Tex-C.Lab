using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using PCRA.Models;

namespace PCRA.Services;

public sealed class UpdateService : IUpdateService
{
    private const string ManifestUrl = "http://tex-download.texautomation.it/restricted/C-Lab/latest.json";

    // Testing only. Do not commit real credentials to source control.
    private const string Username = "texdev";
    private const string Password = "gFu170mBws0IEQ@";

    private readonly HttpClient _httpClient;
    private readonly HttpClientHandler _httpHandler;

    public UpdateService()
    {
        var username = DecodeCredential(ObfuscatedUsername);
        var password = DecodeCredential(ObfuscatedPassword);
        _httpHandler = new HttpClientHandler
        {
            Credentials = new NetworkCredential(
                username,
                password),

            // Allows the handler to send credentials after
            // receiving an authentication challenge.
            PreAuthenticate = true
        };

        _httpClient = new HttpClient(_httpHandler)
        {
            Timeout = TimeSpan.FromSeconds(10)
        };
    }

    public Version GetCurrentVersion()
    {
        var assemblyVersion =
            Assembly.GetEntryAssembly()?
                .GetName()
                .Version;

        if (assemblyVersion is null)
        {
            return new Version(0, 0, 0);
        }

        // The .csproj contains:
        //
        // <Version>0.0.1</Version>
        // <AssemblyVersion>$(Version)</AssemblyVersion>
        //
        // AssemblyVersion is commonly exposed as 0.0.1.0,
        // so normalize it to the required x.x.x format.
        return new Version(
            assemblyVersion.Major,
            assemblyVersion.Minor,
            assemblyVersion.Build);
    }

    public async Task<UpdateInfo?> CheckForUpdateAsync(
        CancellationToken cancellationToken = default)
    {
        var updateInfo =
            await _httpClient.GetFromJsonAsync<UpdateInfo>(
                ManifestUrl,
                cancellationToken);

        if (updateInfo is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(updateInfo.Version))
        {
            throw new InvalidOperationException(
                "The update manifest does not contain a version.");
        }

        if (string.IsNullOrWhiteSpace(updateInfo.InstallerUrl))
        {
            throw new InvalidOperationException(
                "The update manifest does not contain an installer URL.");
        }

        var latestVersion =
            ParseThreePartVersion(updateInfo.Version);

        var currentVersion = GetCurrentVersion();

        return latestVersion > currentVersion
            ? updateInfo
            : null;
    }

    public async Task<string> DownloadInstallerAsync(
        UpdateInfo updateInfo,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        // Validate the manifest version before downloading.
        // This ensures the version remains exactly x.x.x.
        var version = ParseThreePartVersion(updateInfo.Version);

        using var response = await _httpClient.GetAsync(
            updateInfo.InstallerUrl,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var installerUri = new Uri(updateInfo.InstallerUrl);

        var fileName =
            Path.GetFileName(installerUri.LocalPath);

        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = $"PCRA-{version}.exe";
        }

        var downloadDirectory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "PCRA",
            "Updates");

        Directory.CreateDirectory(downloadDirectory);

        var destinationPath = Path.Combine(
            downloadDirectory,
            fileName);

        await using var inputStream =
            await response.Content.ReadAsStreamAsync(
                cancellationToken);

        await using var outputStream =
            File.Create(destinationPath);

        var totalBytes =
            response.Content.Headers.ContentLength;

        var buffer = new byte[81920];
        long totalBytesRead = 0;
        int bytesRead;

        while ((bytesRead = await inputStream.ReadAsync(
                   buffer,
                   cancellationToken)) > 0)
        {
            await outputStream.WriteAsync(
                buffer.AsMemory(0, bytesRead),
                cancellationToken);

            totalBytesRead += bytesRead;

            if (totalBytes.HasValue &&
                totalBytes.Value > 0)
            {
                var percentage =
                    (double)totalBytesRead / totalBytes.Value;

                progress?.Report(percentage);
            }
        }

        progress?.Report(1.0);

        return destinationPath;
    }

    public void RunInstaller(string installerPath)
    {
        Process.Start(
            new ProcessStartInfo
            {
                FileName = installerPath,
                UseShellExecute = true
            });
    }

    private static Version ParseThreePartVersion(
        string versionText)
    {
        var parts = versionText.Split('.');

        if (parts.Length != 3 ||
            !int.TryParse(parts[0], out var major) ||
            !int.TryParse(parts[1], out var minor) ||
            !int.TryParse(parts[2], out var build) ||
            major < 0 ||
            minor < 0 ||
            build < 0)
        {
            throw new InvalidOperationException(
                $"Invalid version '{versionText}'. " +
                "The version must be in the format x.x.x, " +
                "for example 0.0.1.");
        }

        return new Version(major, minor, build);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _httpHandler.Dispose();
    }

    // Testing only. Do not commit real credentials to source control.
    // These values are obfuscated, not encrypted.
    private const string ObfuscatedUsername = "udycfu";
    private const string ObfuscatedPassword = "hEv08/nAxr1HFPA";

    private static string DecodeCredential(string value)
    {
        var characters = value.ToCharArray();

        for (var index = 0; index < characters.Length; index++)
        {
            characters[index] = index % 2 == 0
                ? (char)(characters[index] - 1)
                : (char)(characters[index] + 1);
        }

        return new string(characters);
    }

}
