using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ProduceNowApp.Services;
using ProduceNowApp.ViewModels;
using ProduceNowApp.Views;
using Splat;

namespace ProduceNowApp;

public partial class App : Application
{
    private Bootstrapper _bootstrapper = new (Locator.CurrentMutable, Locator.Current);
    
    public override void Initialize()
    {
        System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
           // ((sender, certificate, chain, sslPolicyErrors) => true);
        AvaloniaXamlLoader.Load(this);
        
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}