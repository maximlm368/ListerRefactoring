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
        string badgeTemplatesFolderPath = @"./";
        IBadgeAppearenceProvider badgeAppearenceDataSource = new BadgeAppearenceProvider (badgeTemplatesFolderPath);
        IPeopleDataSource peopleDataSource = new PeopleSource ();
        IResultOfSessionSaver converter = new Lister.ViewModels.ConverterToPdf ();
        IUniformDocumentAssembler docAssembler = new UniformDocAssembler (converter, badgeAppearenceDataSource, peopleDataSource);
        ContentAssembler.Size pageSize = new ContentAssembler.Size (794, 1123);

        MainWindow mainWindow = new MainWindow ()
        {
            DataContext = new MainWindowViewModel ()
        };
        MainView mainView = new MainView ()
        {
            DataContext = new MainViewModel (docAssembler, pageSize)
        };

        if ( ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop )
        {
            desktop.MainWindow = mainWindow;
        }
        else if ( ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform )
        {
            singleViewPlatform.MainView = mainView;
            
        }

        base.OnFrameworkInitializationCompleted();
    }
}



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