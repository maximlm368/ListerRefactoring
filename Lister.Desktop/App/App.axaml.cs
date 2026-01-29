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
#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Рассмотрите возможность добавления модификатора "required" или объявления значения, допускающего значение NULL.
    public static ServiceProvider Services { get; private set; }
#pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Рассмотрите возможность добавления модификатора "required" или объявления значения, допускающего значение NULL.

    public override void Initialize ()
    {
        AvaloniaXamlLoader.Load ( this );
    }

    public override async void OnFrameworkInitializationCompleted ()
    {
        string resourceDirectory = Environment.CurrentDirectory;

#if DEBUG
        resourceDirectory = resourceDirectory[..^17];
#endif
        
        string jsonSchemePath = "avares://Lister.Desktop/Assets/JsonSchema/Schema.json";
        string resourcePath = Path.Combine ( resourceDirectory, "Resources" );
        string configPath = Path.Combine ( resourcePath, "Config.json" );
        string osName = OperatingSystem.IsWindows () ? "Windows" : "Linux";

        ServiceCollection collection = new ();
        collection.AddNeededServices ( configPath, osName );
        Services = collection.BuildServiceProvider ();

        if ( ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ) 
        {
            return;
        }

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
        mainWindow.Closing += ( s, e ) => e.Cancel = MainViewModel.HasWaitingState;

        base.OnFrameworkInitializationCompleted ();
    }
}
