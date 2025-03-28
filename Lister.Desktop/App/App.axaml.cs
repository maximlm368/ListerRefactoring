using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Lister.Desktop.ExecutersForCoreAbstractions.BadgeCreator;
using Lister.Desktop.Views.MainWindow;
using Lister.Desktop.Views.MainWindow.MainView;
using Lister.Desktop.Views.MainWindow.MainView.ViewModel;
using Lister.Desktop.Views.SplashWindow;
using Microsoft.Extensions.DependencyInjection;

namespace Lister.Desktop.App;

public partial class ListerApp : Avalonia.Application
{
    public static ServiceProvider Services { get; private set; }


    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }


    public override async void OnFrameworkInitializationCompleted ()
    {
        string jsonSchemePath = "avares://Lister.Desktop/Assets/JsonSchema/Schema.json";
        string resourcePath = Path.Combine ( Environment.CurrentDirectory, "Resources" );
        string configPath = Path.Combine ( resourcePath, "Config.json" );
        string osName = OperatingSystem.IsWindows () ? "Windows" : "Linux";

        ServiceCollection collection = new ();
        collection.AddNeededServices ( configPath, osName );
        Services = collection.BuildServiceProvider ();

        if ( ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ) return;

        SplashWindow splashWindow = new ();
        desktop.MainWindow = splashWindow;
        splashWindow.Show ();

        BadgeLayoutProvider layoutProvider = BadgeLayoutProvider.GetInstance ();
        await layoutProvider.SetUp ( resourcePath, jsonSchemePath, osName );

        MainViewModel mainViewModel = Services.GetRequiredService<MainViewModel> ();
        MainView mainView = new ( mainViewModel );

        MainWindow mainWindow = new ()
        {
            Content = mainView
        };

        desktop.MainWindow = mainWindow;
        desktop.MainWindow.Show ();
        splashWindow.Close ();
        mainWindow.Closing += ( s, e ) => e.Cancel = MainViewModel.InWaitingState;

        base.OnFrameworkInitializationCompleted ();
    }
}