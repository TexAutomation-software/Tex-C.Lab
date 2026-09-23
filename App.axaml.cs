using Avalonia;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PCRA.ViewModels;
using PCRA.Views;

namespace PCRA;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }


    public override async void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var splash = new SplashView();
            splash.Show();

            await Task.Delay(2000);

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };

            desktop.MainWindow.Show();
            splash.Close();
        }

        base.OnFrameworkInitializationCompleted();
    }
}