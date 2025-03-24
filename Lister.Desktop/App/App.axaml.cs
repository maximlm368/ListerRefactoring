using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Lister.Desktop.CoreAbstractionsImplementations.BadgeCreator;
using Lister.Desktop.Views.MainWindow;
using Lister.Desktop.Views.MainWindow.MainView;
using Lister.Desktop.Views.MainWindow.MainView.ViewModel;
using Lister.Desktop.Views.SplashWindow;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.InteropServices;

namespace Lister.Desktop.App;

public partial class ListerApp : Avalonia.Application
{
    private static bool _runningOnWindow = false;
    private static bool _runningOnLinux = false;
    public static string JsonSchemeFolderName { get; private set; }
    public static string WorkDirectoryPath { get; private set; }
    public static string ConfigPath { get; private set; }
    public static MainWin MainWindow { get; private set; }
    public static string ResourceFolderName { get; private set; }
    public static string ResourceUriType { get; private set; }
    public static string ResourceDirectoryUri { get; private set; }
    public static string OsName { get; private set; }
    public static ServiceProvider Services;

    private MainVieww _mainView;


    static ListerApp () 
    {
        ServiceCollection collection = new ServiceCollection ();
        collection.AddNeededServices ();
        Services = collection.BuildServiceProvider ();

        string workDirectory = @"./";
        DirectoryInfo containingDirectory = new DirectoryInfo (workDirectory);
        WorkDirectoryPath = containingDirectory.FullName;

        bool isWindows = RuntimeInformation.IsOSPlatform (OSPlatform.Windows);
        bool isLinux = RuntimeInformation.IsOSPlatform (OSPlatform.Linux);
        string osDescription = RuntimeInformation.OSDescription;
        JsonSchemeFolderName = "avares://Lister.Desktop/Assets/JsonSchema/Schema.json";

        if ( isWindows )
        {
            ResourceFolderName = @"Resources\";
            ResourceUriType = "file:///";
            OsName = "Windows";
        }
        else if ( isLinux ) 
        {
            ResourceFolderName = "Resources/";
            ResourceUriType = "file://";
            OsName = "Linux";
        }

        ResourceDirectoryUri = ResourceUriType + WorkDirectoryPath + ResourceFolderName;
        ConfigPath = WorkDirectoryPath + ResourceFolderName + "ListerConfig.json";
    }


    public ListerApp (){ }


    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }


    public override async void OnFrameworkInitializationCompleted ()
    {
        if ( ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop )
        {
            SplashWindow splashWindow = new SplashWindow ();
            desktop.MainWindow = splashWindow;
            splashWindow.Show ();

            BadgeLayoutProvider layoutProvider = BadgeLayoutProvider.GetInstance ();

            await layoutProvider.SetUp (( WorkDirectoryPath + ResourceFolderName )
                                       , ( JsonSchemeFolderName ), OsName);

            MainViewModel mainViewModel = Services.GetRequiredService<MainViewModel> ();

            MainVieww mainView = new MainVieww ()
            {
                DataContext = mainViewModel
            };

            MainWin mainWindow = new MainWin ()
            {
                Content = mainView
            };

            ListerApp.MainWindow = mainWindow;
            desktop.MainWindow = mainWindow;
            desktop.MainWindow.Show ();
            splashWindow.Close ();

            mainWindow.Closing += ( s, e ) =>
            {
                if ( MainViewModel.InWaitingState )
                {
                    e.Cancel = true;
                }
                else 
                {
                    e.Cancel = false;
                }
            };
        }
        else if ( ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform )
        {
            singleViewPlatform.MainView = _mainView;
        }

        base.OnFrameworkInitializationCompleted ();
    }
}