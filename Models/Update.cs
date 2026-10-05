namespace PCRA.Models;

public enum UpdateState
{
    Checking,
    UpToDate,
    UpdateAvailable,
    Downloading,
    Completed,
    Error,
    Cancelled
}

public sealed class UpdateInfo
{
    public string Version { get; set; } = string.Empty;
    public string InstallerUrl { get; set; } = string.Empty;
}