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
    public static ServiceProvider services;


    static App () 
    {
        ServiceCollection collection = new ServiceCollection ();
        collection.AddNeededServices ();
        services = collection.BuildServiceProvider ();
    }


    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }


    public override void OnFrameworkInitializationCompleted()
    {
        
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
        //BadgeAppearenceProviderFactory factory = new BadgeAppearenceProviderFactory ();

        //Type serviceType = typeof (DataGateway.BadgeAppearenceProvider);

        //collection.AddSingleton <IBadgeAppearenceProvider> 
        //                           (( factory ) => (BadgeAppearenceProvider) factory.GetService(serviceType));

        collection.AddSingleton <IBadgeAppearenceProvider, BadgeAppearenceProvider>();
        collection.AddSingleton <IPeopleDataSource, PeopleSource> ();

        //collection.AddSingleton<IResultOfSessionSaver, Lister.ViewModels.ConverterToPdf> ();

        collection.AddSingleton <Lister.ViewModels.ConverterToPdf> ();
        collection.AddSingleton <IUniformDocumentAssembler, UniformDocAssembler> ();
        collection.AddSingleton <IBadgeAppearenceProvider, BadgeAppearenceProvider> ();
        collection.AddSingleton <IBadLineColorProvider, BadgeAppearenceProvider> ();
        collection.AddSingleton <ModernMainViewModel> ();
        collection.AddSingleton <BadgeViewModel> ();
        collection.AddSingleton <ImageViewModel> ();
        collection.AddSingleton <PageViewModel> ();
        collection.AddSingleton <PersonChoosingViewModel> ();
        collection.AddSingleton <PersonSourceViewModel> ();
        collection.AddSingleton <SceneViewModel> ();
        collection.AddSingleton <TemplateChoosingViewModel> ();
        collection.AddSingleton <ZoomNavigationViewModel> ();
        collection.AddSingleton <TextLineViewModel> ();
        collection.AddSingleton <MessageViewModel> ();
    }
}


