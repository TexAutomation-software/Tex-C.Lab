using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using PCRA.Services;
using PCRA.ViewModels.Pages;
using System.IO;
using System;

namespace PCRA.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly AppSettingsService _settings;

    private readonly GeneratorsViewModel _generatorsPage;
    private readonly SettingsViewModel _settingsPage;
    private readonly AboutViewModel _aboutPage;

    [ObservableProperty]
    private ViewModelBase? currentPage;

    public MainViewModel()
    {
        _settings = new AppSettingsService();
        _settings.Load();

        _generatorsPage = new GeneratorsViewModel(_settings);
        _settingsPage = new SettingsViewModel(_settings);
        _aboutPage = new AboutViewModel();

        CurrentPage = null;
    }

    [RelayCommand]
    private void OpenGenerators()
    {
        CurrentPage = _generatorsPage;
    }

    [RelayCommand]
    private void OpenSettings()
    {
        CurrentPage = _settingsPage;
    }

    [RelayCommand]
    private void OpenAbout()
    {
        CurrentPage = _aboutPage;
    }

    [RelayCommand]
    private void OpenManual()
    {
        var pdfPath = Path.Combine(AppContext.BaseDirectory,"Docs","build","UserManual.pdf");
        Process.Start(new ProcessStartInfo
        {
            FileName = pdfPath,
            UseShellExecute = true
        });
    }
}