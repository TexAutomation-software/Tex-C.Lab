using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using CommunityToolkit.Mvvm.Input;

namespace PCRA.ViewModels.Pages;

public partial class AboutViewModel : ViewModelBase
{
    public string Version =>
        Assembly.GetExecutingAssembly()
            .GetName()
            .Version?
            .ToString(3) ?? "Unknown";
    
    [RelayCommand]
    private void OpenChangelog()
    {
        var pdfPath = Path.Combine(
            AppContext.BaseDirectory,
            "Changelog",
            "build",
            "Changelog.pdf");

        if (!File.Exists(pdfPath))
        {
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = pdfPath,
            UseShellExecute = true
        });
    }
}
