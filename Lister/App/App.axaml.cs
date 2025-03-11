using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Core.DataAccess;
using View.SplashWindow;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.InteropServices;
using View.MainWindow.MainView;
using View.MainWindow.MainView.ViewModel;
using View.MainWindow;
using View.CoreAbstractionsImplimentations.BadgeCreator;

namespace View.App;

public partial class ListerApp : Avalonia.Application
{
    private static bool _runningOnWindow = false;
    private static bool _runningOnLinux = false;
    private MainView _mainView;


    public static string JsonSchemeFolderName { get; private set; }
    public static string WorkDirectoryPath { get; private set; }
    public static string ImagesUri { get; private set; }
    public static string ConfigPath { get; private set; }



    public static MainWin MainWindow { get; private set; }
    public static string ResourceFolderName { get; private set; }
    public static string ResourceUriType { get; private set; }
    public static string ResourceDirectoryUri { get; private set; }
    public static string OsName { get; private set; }
    public static ServiceProvider services;


    static ListerApp () 
    {
        ServiceCollection collection = new ServiceCollection ();
        collection.AddNeededServices ();
        services = collection.BuildServiceProvider ();

        string workDirectory = @"./";
        DirectoryInfo containingDirectory = new DirectoryInfo (workDirectory);
        WorkDirectoryPath = containingDirectory.FullName;

        bool isWindows = RuntimeInformation.IsOSPlatform (OSPlatform.Windows);
        bool isLinux = RuntimeInformation.IsOSPlatform (OSPlatform.Linux);
        string osDescription = RuntimeInformation.OSDescription;

        if ( isWindows )
        {
            JsonSchemeFolderName = @"Resources\JsonSchemes\";
            ResourceFolderName = @"Resources\";
            ResourceUriType = "file:///";
            OsName = "Windows";
        }
        else if ( isLinux ) 
        {
            JsonSchemeFolderName = "Resources/JsonSchemes/";
            ResourceFolderName = "Resources/";
            ResourceUriType = "file://";
            OsName = "Linux";
        }

        ResourceDirectoryUri = ResourceUriType + WorkDirectoryPath + ResourceFolderName;
        ImagesUri = "avares://Lister/Assets/";
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
            SplashWindow.SplashWindow splashWindow = new SplashWindow.SplashWindow ();
            desktop.MainWindow = splashWindow;
            splashWindow.Show ();

            BadgeLayoutProvider layoutProvider = BadgeLayoutProvider.GetInstance ();

            await layoutProvider.SetUp (( WorkDirectoryPath + ResourceFolderName )
                                       , ( WorkDirectoryPath + JsonSchemeFolderName ), OsName);

            MainViewModel mainViewModel = services.GetRequiredService<MainViewModel> ();

            MainView mainView = new MainView ()
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