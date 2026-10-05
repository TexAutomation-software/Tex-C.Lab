using PCRA.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PCRA.Services;

public interface IUpdateService : IDisposable
{
    Version GetCurrentVersion();
    Task<UpdateInfo?> CheckForUpdateAsync(CancellationToken cancellationToken = default);
    Task<string> DownloadInstallerAsync(UpdateInfo updateInfo,IProgress<double>? progress = null,CancellationToken cancellationToken = default);
    void RunInstaller(string installerPath);
}
