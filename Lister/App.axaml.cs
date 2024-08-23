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
using Avalonia.Media;
using static System.Net.Mime.MediaTypeNames;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media.Fonts;
using Avalonia.Platform;
using SkiaSharp;
using System.Globalization;

namespace Lister;


public partial class App : Avalonia.Application
{
    public static string ResourceUriFolderName { get; private set; }
    public static string ResourceUriType { get; private set; }
    public static string WorkDirectoryPath { get; private set; }
    public static string ResourceDirectoryUri { get; private set; }
    public static FontFamily FF { get; private set; }

    public static IResourceDictionary AvailableResources { get; private set; }

    public static ServiceProvider services;


    static App () 
    {
        ServiceCollection collection = new ServiceCollection ();
        collection.AddNeededServices ();
        services = collection.BuildServiceProvider ();

        string workDirectory = @"./";
        DirectoryInfo containingDirectory = new DirectoryInfo (workDirectory);
        WorkDirectoryPath = containingDirectory.FullName;

        bool isWindows = RuntimeInformation.IsOSPlatform (OSPlatform.Windows);
        bool isLinux = RuntimeInformation.IsOSPlatform (OSPlatform.Linux);

        if ( isWindows )
        {
            //ResourceUriFolderName = "//Resources//";
            ResourceUriFolderName = @"Resources\";
            ResourceUriType = "file:///";
        }
        else if ( isLinux ) 
        {
            ResourceUriFolderName = "Resources/";
            ResourceUriType = "file://";
        }

        ResourceDirectoryUri = ResourceUriType + WorkDirectoryPath + ResourceUriFolderName;
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



        string fileName = "Iamsb.ttf";
        string fontUriString = App.ResourceDirectoryUri + fileName;
        Uri fontUri = new Uri (fontUriString);

        FontFamily fm = new FontFamily (fontUri, "I am simplified");
        var kk = fm.Key;

        

        IGlyphTypeface glyphTypeface;

        //bool fd = FontManager.Current.TryGetGlyphTypeface (typeface, out glyphTypeface);

        
        FF = fm;

        string ffs = fm.ToString ();



        //FF = new FontFamily ("Segoe UI");
        FF = new FontFamily ("Kramola");

        string key = "cg";
        bool res = this.Resources.ContainsKey (key);
        res = Resources.TryGetValue (key, out object val);
        var pushkin = Resources [key];
        //Resources [key] = fm;



        int df = 0;
    }
}



public static class ServiceCollectionExtensions
{
    public static void AddNeededServices ( this IServiceCollection collection )
    {
        collection.AddSingleton <IServiceProvider, BadgeAppearenceServiceProvider> ();
        collection.AddSingleton <IPeopleDataSource, PeopleSource> ();
        collection.AddSingleton <Lister.ViewModels.ConverterToPdf> ();
        collection.AddSingleton <IUniformDocumentAssembler, UniformDocAssembler> ();

        //collection.AddSingleton <IBadgeAppearenceProvider, BadgeAppearenceProvider> ();
        //collection.AddSingleton <IBadLineColorProvider, BadgeAppearenceProvider> ();

        collection.AddSingleton (typeof (IBadgeAppearenceProvider), BadgeAppearenceFactory);
        collection.AddSingleton (typeof (IBadLineColorProvider), BadLineFactory);

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


    private static IBadgeAppearenceProvider BadgeAppearenceFactory ( IServiceProvider serviceProvider )
    {
        object service = serviceProvider.GetService (typeof (BadgeAppearenceProvider));

        //IBadgeAppearenceProvider result = service as IBadgeAppearenceProvider;

        IBadgeAppearenceProvider result = 
            new BadgeAppearenceProvider (App.ResourceDirectoryUri, ( App.WorkDirectoryPath + App.ResourceUriFolderName ));

        return result;
    }


    private static IBadLineColorProvider BadLineFactory ( IServiceProvider serviceProvider )
    {
        object service = serviceProvider.GetService (typeof (BadgeAppearenceProvider));

        //IBadLineColorProvider result = service as IBadLineColorProvider;

        IBadLineColorProvider result =
            new BadgeAppearenceProvider (App.ResourceDirectoryUri, ( App.WorkDirectoryPath + App.ResourceUriFolderName ));

        return result;
    }
}



public class BadgeAppearenceServiceProvider : IServiceProvider
{
    public object ? GetService ( Type serviceType )
    {
        if ( serviceType == null ) 
        {
            return null;
        }

        object result = null;

        bool isAimService = ( serviceType.FullName == "DataGateway.IBadgeAppearenceProvider" ) 
                            ||
                            ( serviceType.FullName == "DataGateway.IBadLineColorProvider" );

        if ( isAimService )
        {
            result = new BadgeAppearenceProvider (App.ResourceDirectoryUri, (App.WorkDirectoryPath + App.ResourceUriFolderName));
        }

        return result;
    }
}

