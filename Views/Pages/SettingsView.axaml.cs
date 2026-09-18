using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using PCRA.ViewModels.Pages;

namespace PCRA.Views.Pages;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private async void Browse_Click(
        object? sender,
        RoutedEventArgs e)
    {
        var folders =
            await TopLevel.GetTopLevel(this)!
            .StorageProvider
            .OpenFolderPickerAsync(
                new FolderPickerOpenOptions
                {
                    Title = "Select Output Folder"
                });

        if (folders.Count > 0)
        {
            if (DataContext is SettingsViewModel vm)
            {
                vm.OutputFolder =
                    folders[0].Path.LocalPath;
            }
        }
    }
}