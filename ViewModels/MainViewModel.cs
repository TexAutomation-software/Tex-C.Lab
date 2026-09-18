using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCRA.Services;
using PCRA.ViewModels.Pages;

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
}