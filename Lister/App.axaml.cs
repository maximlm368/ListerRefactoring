using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ContentAssembler;
using DataGateway;

using Lister.ViewModels;
using Lister.Views;

namespace Lister;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        IBadgeAppearenceDataSource badgeAppearenceDataSource = new ConfigFileBasedDataSource(null);
        IPeopleDataSource peopleDataSource = new PeopleSource();
        IResultOfSessionSaver converter = new ContentAssembler.ConverterToPdf ();
        IUniformDocumentAssembler docAssembler = new UniformDocAssembler(converter, badgeAppearenceDataSource, peopleDataSource);
        MainWindow mainWindow = new MainWindow (docAssembler);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = mainWindow;
            PixelSize screenSize = desktop.MainWindow.Screens.Primary.Bounds.Size;
            int screenWidth = screenSize.Width;
            mainWindow.SetWidth (screenWidth);
        }
        else if ( ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform )
        {
            singleViewPlatform.MainView = new MainView (mainWindow, docAssembler);
        }


        //if ( ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop )
        //{
        //    desktop.MainWindow = new MainWindow
        //    {
        //        DataContext = new MainWindowViewModel ()
        //    };
        //}
        //else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        //{
        //    singleViewPlatform.MainView = new MainView
        //    {
        //        DataContext = new MainViewModel(docAssembler)
        //    };
        //}

        base.OnFrameworkInitializationCompleted();
    }
}
