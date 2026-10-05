using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCRA.Models;
using PCRA.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCRA.Models;
using PCRA.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;

namespace PCRA.ViewModels.Popups;

public partial class UpdateDialogViewModel : ViewModelBase
{
    private readonly IUpdateService _updateService;

    private CancellationTokenSource? _cancellationTokenSource;
    private UpdateInfo? _availableUpdate;

    public event Action? CloseRequested;

    [ObservableProperty]
    private UpdateState state = UpdateState.UpToDate;

    [ObservableProperty]
    private string message =
        "Checking for updates....";

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private double downloadProgress;

    [ObservableProperty]
    private string? downloadedInstallerPath;

    public bool IsChecking =>
        State == UpdateState.Checking;

    public bool IsUpToDate =>
        State == UpdateState.UpToDate;

    public bool IsUpdateAvailable =>
        State == UpdateState.UpdateAvailable;

    public bool IsDownloading =>
        State == UpdateState.Downloading;

    public bool IsCompleted =>
        State == UpdateState.Completed;

    public bool IsCancelled =>
        State == UpdateState.Cancelled;

    public bool IsError =>
        State == UpdateState.Error;

    public bool IsBusy =>
        State == UpdateState.Checking ||
        State == UpdateState.Downloading;

    public bool CanCancel =>
        State == UpdateState.Checking ||
        State == UpdateState.Downloading;

    public string CurrentVersion =>
        _updateService.GetCurrentVersion().ToString();

    public string AvailableVersion =>
        _availableUpdate?.Version ?? string.Empty;

    public UpdateDialogViewModel(IUpdateService updateService)
    {
        _updateService = updateService;
    }

    partial void OnStateChanged(UpdateState value)
    {
        OnPropertyChanged(nameof(IsChecking));
        OnPropertyChanged(nameof(IsUpToDate));
        OnPropertyChanged(nameof(IsUpdateAvailable));
        OnPropertyChanged(nameof(IsDownloading));
        OnPropertyChanged(nameof(IsCompleted));
        OnPropertyChanged(nameof(IsCancelled));
        OnPropertyChanged(nameof(IsError));
        OnPropertyChanged(nameof(IsBusy));
        OnPropertyChanged(nameof(CanCancel));
        OnPropertyChanged(nameof(AvailableVersion));
    }

    [RelayCommand]
    private async Task CheckForUpdatesAsync()
    {
        if (IsBusy)
        {
            return;
        }

        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        ErrorMessage = null;
        Message = "Checking for updates...";
        State = UpdateState.Checking;

        try
        {
            Console.WriteLine("Update check started");
            _availableUpdate =
                await _updateService.CheckForUpdateAsync(
                    _cancellationTokenSource.Token);
            Console.WriteLine("Update check finished");
            if (_availableUpdate is null)
            {
                Message = "You are already using the latest version.";
                State = UpdateState.UpToDate;
                return;
            }

            Message =
                $"Version {_availableUpdate.Version} is available. " +
                "Would you like to download it?";

            State = UpdateState.UpdateAvailable;
        }
        catch (OperationCanceledException)
        {
            Message = "Update check cancelled.";
            State = UpdateState.Cancelled;
        }
        catch (HttpRequestException)
        {
            ErrorMessage =
                "Unable to reach the update server.";

            Message =
                "Please check your internet connection.";

            State = UpdateState.Error;
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
            Message = "The update check failed.";
            State = UpdateState.Error;
        }
    }

    [RelayCommand]
    private async Task DownloadUpdateAsync()
    {
        if (_availableUpdate is null || IsBusy)
        {
            return;
        }

        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        DownloadProgress = 0;
        ErrorMessage = null;
        Message = "Downloading update...";
        State = UpdateState.Downloading;

        var progress = new Progress<double>(value =>
        {
            DownloadProgress = value;
        });

        try
        {
            DownloadedInstallerPath =
                await _updateService.DownloadInstallerAsync(
                    _availableUpdate,
                    progress,
                    _cancellationTokenSource.Token);

            Message =
                "The update has been downloaded and is ready to install.";

            State = UpdateState.Completed;
        }
        catch (OperationCanceledException)
        {
            Message = "Download cancelled.";
            State = UpdateState.Cancelled;
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
            Message = "The update download failed.";
            State = UpdateState.Error;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        if (CanCancel)
        {
            _cancellationTokenSource?.Cancel();
            return;
        }

        State = UpdateState.Cancelled;
        CloseRequested?.Invoke();
    }

    [RelayCommand]
    private void Close()
    {
        CloseRequested?.Invoke();
    }

    public async Task InitializeAsync()
    {
        await CheckForUpdatesAsync();
    }

    [RelayCommand]
    private void InstallUpdate()
    {
        if (string.IsNullOrWhiteSpace(DownloadedInstallerPath))
        {
            return;
        }

        _updateService.RunInstaller(
            DownloadedInstallerPath);

        Environment.Exit(0);
    }
}
