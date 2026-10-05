using Avalonia.Controls;
using PCRA.Services;
using PCRA.ViewModels;
using PCRA.ViewModels.Popups;
using PCRA.Views.Popups;
using System;

namespace PCRA.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(
        object? sender,
        EventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.UpdateRequested -= OpenUpdateDialog;
            viewModel.UpdateRequested += OpenUpdateDialog;
        }
    }

    private async void OpenUpdateDialog()
    {
        var updateViewModel =
            new UpdateDialogViewModel(
                new UpdateService());

        var dialog = new UpdateDialogView
        {
            DataContext = updateViewModel
        };

        await dialog.ShowDialog(this);
    }

    protected override void OnClosed(EventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.UpdateRequested -= OpenUpdateDialog;
        }

        base.OnClosed(e);
    }
}
