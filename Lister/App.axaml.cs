using Avalonia;
using Avalonia.ReactiveUI;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ContentAssembler;
using DataGateway;
using Microsoft.Extensions.DependencyInjection;
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
        
        ServiceCollection collection = new ServiceCollection ();
        collection.AddNeededServices ();
        ServiceProvider services = collection.BuildServiceProvider ();
        ModernMainViewModel mainViewModel = services.GetRequiredService<ModernMainViewModel> ();

        ModernMainView mainView = new ModernMainView ()
        {
            DataContext = mainViewModel
        };

        MainWindow mainWindow = new MainWindow ()
        {
            DataContext = new MainWindowViewModel (),
            Content = mainView
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



public static class ServiceCollectionExtensions
{
    public static void AddNeededServices ( this IServiceCollection collection )
    {
        collection.AddSingleton<IBadgeAppearenceProvider, BadgeAppearenceProvider> ();
        collection.AddSingleton<IPeopleDataSource, PeopleSource> ();
        //collection.AddSingleton<IResultOfSessionSaver, Lister.ViewModels.ConverterToPdf> ();
        collection.AddSingleton<Lister.ViewModels.ConverterToPdf> ();
        collection.AddSingleton<IUniformDocumentAssembler, UniformDocAssembler> ();
        collection.AddSingleton<ContentAssembler.Size> ();
        collection.AddSingleton<IBadgeAppearenceProvider, BadgeAppearenceProvider> ();
        collection.AddSingleton<ModernMainViewModel> ();
        collection.AddSingleton<BadgeViewModel> ();
        collection.AddSingleton<ImageViewModel> ();
        collection.AddSingleton<PageViewModel> ();
        collection.AddSingleton<PersonChoosingViewModel> ();
        collection.AddSingleton<PersonSourceViewModel> ();
        collection.AddSingleton<SceneViewModel> ();
        collection.AddSingleton<TemplateChoosingViewModel> ();
        collection.AddSingleton<ZoomNavigationViewModel> ();
        collection.AddSingleton<TextLineViewModel> ();
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

//IBadgeAppearenceProvider badgeAppearenceDataSource = new BadgeAppearenceProvider (badgeTemplatesFolderPath);
//IPeopleDataSource peopleDataSource = new PeopleSource ();
//IResultOfSessionSaver converter = new Lister.ViewModels.ConverterToPdf ();
//IUniformDocumentAssembler docAssembler = new UniformDocAssembler (converter, badgeAppearenceDataSource, peopleDataSource);
//ContentAssembler.Size pageSize = new ContentAssembler.Size (794, 1123);

//PersonChoosingViewModel personChoosingVM = new PersonChoosingViewModel (docAssembler, pageSize);
//PersonSourceViewModel personSourceVM = new PersonSourceViewModel (docAssembler, pageSize, personChoosingVM);
//TemplateChoosingViewModel templateChoosingVM = new TemplateChoosingViewModel (docAssembler, pageSize);
//ZoomNavigationViewModel zoomNavigationVM = new ZoomNavigationViewModel (docAssembler, pageSize);
//SceneViewModel sceneVM = new SceneViewModel (docAssembler, pageSize);

//PersonChoosingUserControl personChoosing = new PersonChoosingUserControl () { DataContext = personChoosingVM };
//PersonSourceUserControl personSource = new PersonSourceUserControl () { DataContext = personSourceVM };
//TemplateChoosingUserControl templateChoosing = new TemplateChoosingUserControl () { DataContext = templateChoosingVM };
//ZoomNavigationUserControl zoomNavigation = new ZoomNavigationUserControl () { DataContext = zoomNavigationVM };
//SceneUserControl scene = new SceneUserControl () { DataContext = sceneVM };