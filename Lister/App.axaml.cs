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
using System.Diagnostics;
using WiLBiT;
using Microsoft.Win32;

namespace Lister;


public partial class App : Avalonia.Application
{
    public static string ResourceUriFolderName { get; private set; }
    public static string ResourceUriType { get; private set; }
    public static string WorkDirectoryPath { get; private set; }
    public static string ResourceDirectoryUri { get; private set; }
    public static string OsName { get; private set; }

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
            OsName = "Windows";
        }
        else if ( isLinux ) 
        {
            ResourceUriFolderName = "Resources/";
            ResourceUriType = "file://";
            OsName = "Linux";
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



        //string fileName = "Iamsb.ttf";
        //string fontUriString = App.ResourceDirectoryUri + fileName;
        //Uri fontUri = new Uri (fontUriString);
        //FontFamily fm = new FontFamily (fontUri, "I am simplified");
        //var kk = fm.Key;
        //IGlyphTypeface glyphTypeface;
        //bool fd = FontManager.Current.TryGetGlyphTypeface (typeface, out glyphTypeface);
        //FF = new FontFamily ("Segoe UI");
        //FF = new FontFamily ("Kramola");
        //string key = "cg";
        //bool res = this.Resources.ContainsKey (key);
        //res = Resources.TryGetValue (key, out object val);
        //var pushkin = Resources [key];
        //Resources [key] = fm;
        //Resources [key] = fm;

        //string fileSource = "D:\\MML\\Lister\\Lister.Desktop\\bin\\Debug\\net8.0\\win-x64\\Resources\\Kramola.ttf";
        //string fileDestination = "C:\\Users\\Mymrin_ML\\AppData\\Local\\Microsoft\\Windows\\Fonts\\Kramola_0.ttf";
        //fileDestination = "C:\\Windows\\Fonts\\Kramola.ttf";
        //Resources.TryGetValue ("cg", out object val);
        //FF = val as Avalonia.Media.FontFamily;
        //FW = FontWeight.Bold;
    }



    public static void InstallFont ( string FontName, string FullFontName )
    {
        string localAppDataPath = Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData);
        string windowsFontsPath = Path.Combine (localAppDataPath, "Microsoft", "Windows", "Fonts");
        string FontPath = Path.Combine (windowsFontsPath, $"{FontName}.ttf");

        string source = "D:\\MML\\Lister\\Lister\\Assets\\Kramola.ttf";

        if ( ! File.Exists(FontPath) ) 
        {
            File.Copy (source, FontPath);
        }

        RegistryKey fontKey = Registry.CurrentUser.CreateSubKey (@"Software\Microsoft\Windows NT\CurrentVersion\Fonts");
        fontKey.SetValue ($"{FullFontName} (TrueType)", FontPath);
        fontKey.Close ();
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
        collection.AddSingleton (typeof (IFontFileProvider), BadgeFontFactory);

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


    private static IFontFileProvider BadgeFontFactory ( IServiceProvider serviceProvider )
    {
        object service = serviceProvider.GetService (typeof (BadgeAppearenceProvider));

        //IBadLineColorProvider result = service as IBadLineColorProvider;

        IFontFileProvider result =
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

        bool isAimService = ( serviceType.FullName == "ContentAssembler .IBadgeAppearenceProvider" )
                            ||
                            ( serviceType.FullName == "ContentAssembler.IBadLineColorProvider" )
                            ||
                            ( serviceType.FullName == "ContentAssembler.IFontFileProvider" );

        if ( isAimService )
        {
            result = new BadgeAppearenceProvider (App.ResourceDirectoryUri, (App.WorkDirectoryPath + App.ResourceUriFolderName));
        }

        return result;
    }
}



public class FontOperate
{
    [DllImport ("kernel32.dll", SetLastError = true)]
    static extern int WriteProfileString ( string lpszSection, string lpszKeyName, string lpszString );

    [DllImport ("user32.dll")]
    public static extern int SendMessage ( int hWnd,
    uint Msg,
    int wParam,
    int lParam
    );
    [DllImport ("gdi32")]
    public static extern int AddFontResource ( string lpFileName );


    public static bool InstallFont ( string sFontFileName, string sFontName )
    {
        string target = string.Format (@"{0}\Fonts\{1}", System.Environment.GetEnvironmentVariable ("WINDIR"), sFontFileName);//System FONT directory
        /*string source = string.Format (@"{0}\Font\{1}", System.Windows.Forms.Application.StartupPath, sFontFileName);*/
        //FONT directory to be installed

        
        string source = "D:\\MML\\Lister\\Lister\\Assets\\Kramola.ttf";
        
        try
        {
            if ( ! File.Exists (target)   &&   File.Exists (source) )
            {
                int _nRet;
                File.Copy (source, target);
                _nRet = AddFontResource (target);
                _nRet = WriteProfileString ("fonts", sFontName + "(TrueType)", sFontFileName);
            }
        }
        catch (Exception ex)
        {


            return false;
        }
        return true;
    }
}

