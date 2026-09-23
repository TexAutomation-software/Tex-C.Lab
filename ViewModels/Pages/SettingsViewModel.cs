using PCRA.Services;

namespace PCRA.ViewModels.Pages;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly AppSettingsService _settings;

    public SettingsViewModel(AppSettingsService settings)
    {
        _settings = settings;
    }

    public string OutputFolder
    {
        get => _settings.OutputFolder;
        set
        {
            if (_settings.OutputFolder != value)
            {
                _settings.OutputFolder = value;

                _settings.Save();

                OnPropertyChanged();
            }
        }
    }
}