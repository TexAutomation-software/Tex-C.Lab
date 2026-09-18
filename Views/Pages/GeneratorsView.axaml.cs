using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using PCRA.ViewModels.Pages;

namespace PCRA.Views.Pages;

public partial class GeneratorsView : UserControl
{
    public GeneratorsView()
    {
        InitializeComponent();
    }

    private async void BrowseCardPars_Click(
        object? sender,
        RoutedEventArgs e)
    {
        var files =
            await TopLevel.GetTopLevel(this)!
            .StorageProvider
            .OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = "Select CardPars File",
                    AllowMultiple = false
                });

        if (files.Count > 0 &&
            DataContext is GeneratorsViewModel vm)
        {
            vm.CardParsFile =
                files[0].Path.LocalPath;
        }
    }

    private async void BrowseChecksum_Click(
        object? sender,
        RoutedEventArgs e)
    {
        var files =
            await TopLevel.GetTopLevel(this)!
            .StorageProvider
            .OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = "Select BIN File",
                    AllowMultiple = false
                });

        if (files.Count > 0 && 
            DataContext is GeneratorsViewModel vm)
        {
            ChecksumFileBox.Text =
                files[0].Path.LocalPath;
        }
    }
}