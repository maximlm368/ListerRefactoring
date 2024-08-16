﻿using Avalonia;
using Avalonia.ReactiveUI;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ContentAssembler;
using DataGateway;
using Microsoft.Extensions.DependencyInjection;
using Lister.ViewModels;
using Lister.Views;
using Splat;
using Avalonia.Styling;
using Avalonia.Controls;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Runtime.InteropServices;

namespace Lister;


public partial class App : Application
{
    public static string ResourceUriFolderName { get; private set; }
    public static string ResourceUriType { get; private set; }
    public static ServiceProvider services;


    static App () 
    {
        ServiceCollection collection = new ServiceCollection ();
        collection.AddNeededServices ();
        services = collection.BuildServiceProvider ();

        bool isWindows = RuntimeInformation.IsOSPlatform (OSPlatform.Windows);
        bool isLinux = RuntimeInformation.IsOSPlatform (OSPlatform.Linux);

        if ( isWindows )
        {
            ResourceUriFolderName = "//Resources//";
            ResourceUriType = "file:///";
        }
        else if ( isLinux ) 
        {
            ResourceUriFolderName = "Resources/";
            ResourceUriType = "file://";
        }
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


