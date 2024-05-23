using Avalonia;
using Avalonia.ReactiveUI;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ContentAssembler;
using DataGateway;

using Lister.ViewModels;
using Lister.Views;
using Splat;

namespace Lister;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        MainWindow mainWindow = new MainWindow ();

        //Locator


        //if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        //{
        //    desktop.MainWindow = mainWindow;
        //    mainWindow.DataContext = new MainWindowViewModel ();
        //    PixelSize screenSize = desktop.MainWindow.Screens.Primary.Bounds.Size;
        //    int screenWidth = screenSize.Width;
        //    mainWindow.SetWidth (screenWidth);
        //}
        //else if ( ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform )
        //{
        //    singleViewPlatform.MainView = mainWindow;
        //}


        if ( ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop )
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel ()
            };
        }
        else if ( ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform )
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel (docAssembler)
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
