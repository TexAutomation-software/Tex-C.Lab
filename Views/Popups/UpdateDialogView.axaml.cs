using Avalonia.Controls;
using PCRA.ViewModels.Popups;
using System;

namespace PCRA.Views.Popups;

public partial class UpdateDialogView : Window
{
    public UpdateDialogView()
    {
        InitializeComponent();
    }

    protected override async void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        if (DataContext is UpdateDialogViewModel vm)
        {
            vm.CloseRequested += CloseDialog;

            await vm.InitializeAsync();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        if (DataContext is UpdateDialogViewModel vm)
        {
            vm.CloseRequested -= CloseDialog;
        }

        base.OnClosed(e);
    }

    private void CloseDialog()
    {
        Close();
    }
}