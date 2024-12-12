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
//using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Runtime.InteropServices;
using Avalonia.Media;
using static System.Net.Mime.MediaTypeNames;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media.Fonts;
using Avalonia.Platform;
using SkiaSharp;
using System.Globalization;
using System.Diagnostics;
using Microsoft.Win32;
using Microsoft.Extensions.Configuration;
using System.Runtime.CompilerServices;

namespace Lister;


public partial class App : Avalonia.Application
{
    private static bool _runningOnWindow = false;
    private static bool _runningOnLinux = false;

    public static MainWindow MainWindow { get; private set; }

    public static string JsonSchemeFolderName { get; private set; }
    public static string ResourceFolderName { get; private set; }
    public static string ResourceUriType { get; private set; }
    public static string WorkDirectoryPath { get; private set; }
    public static string ResourceDirectoryUri { get; private set; }
    public static string OsName { get; private set; }

    private ModernMainView _mainView;
    private ModernMainViewModel _mainViewModel;
    

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
    }


    public App (){ }


    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }


    public override async void OnFrameworkInitializationCompleted()
    {
        if ( ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop )
        {
            SplashWindow splashWindow = new SplashWindow ();
            SplashViewModel splashViewModel = new SplashViewModel ();
            splashWindow.DataContext = splashViewModel;
            desktop.MainWindow = splashWindow;

            await InstallFonts ();

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

            App.MainWindow = mainWindow;
            desktop.MainWindow = mainWindow;
            desktop.MainWindow.Show ();
            splashWindow.Close ();
        }
        else if ( ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform )
        {
            singleViewPlatform.MainView = _mainView;
        }

        base.OnFrameworkInitializationCompleted();
    }


    public static string ExecuteBashCommand ( string command )
    {
        using ( Process process = new Process () )
        {
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{command}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            process.Start ();

            string result = process.StandardOutput.ReadToEnd ();

            process.WaitForExit ();

            return result;
        }
    }


    private Task InstallFonts ()
    {
        Task task = new Task
        (() =>
        {
            _runningOnWindow = RuntimeInformation.IsOSPlatform (OSPlatform.Windows);
            _runningOnLinux = RuntimeInformation.IsOSPlatform (OSPlatform.Linux);

            string ResourceUriFolderName = string.Empty;
            string ResourceUriType = string.Empty;

            string workDirectory = @"./";

            if ( _runningOnWindow )
            {
                ResourceUriFolderName = @"Resources\";
            }
            else if ( _runningOnLinux )
            {
                ResourceUriFolderName = "Resources/";
            }

            DirectoryInfo containingDirectory = new DirectoryInfo (workDirectory + ResourceUriFolderName);
            string resourcePath = containingDirectory.FullName;

            List<string> fontFiles = new ();
            List<string> fontNames = new ();
            FileInfo [] containingFiles = containingDirectory.GetFiles ("*.json");

            GetFontFilesAndNames (containingFiles, out fontFiles, out fontNames);

            fontFiles = fontFiles.Distinct (StringComparer.OrdinalIgnoreCase).ToList ();
            fontNames = fontNames.Distinct (StringComparer.OrdinalIgnoreCase).ToList ();

            SetFonts (fontFiles, fontNames, resourcePath);

            Thread.Sleep (3000);
        });

        task.Start ();

        return task;
    }


    private static void GetFontFilesAndNames ( FileInfo [] jsonFiles, out List<string> fontFiles, out List<string> fontNames )
    {
        fontFiles = new ();
        fontNames = new ();

        foreach ( FileInfo fileInfo in jsonFiles )
        {
            string jsonPath = fileInfo.FullName;
            fontFiles.Add (GetterFromJson.GetSectionStrValue (new List<string> { "FamilyName", "FontFile" }, jsonPath));
            fontNames.Add (GetterFromJson.GetSectionStrValue (new List<string> { "FamilyName", "FontName" }, jsonPath));
            fontFiles.Add (GetterFromJson.GetSectionStrValue (new List<string> { "FirstName", "FontFile" }, jsonPath));
            fontNames.Add (GetterFromJson.GetSectionStrValue (new List<string> { "FirstName", "FontName" }, jsonPath));
            fontFiles.Add (GetterFromJson.GetSectionStrValue (new List<string> { "PatronymicName", "FontFile" }, jsonPath));
            fontNames.Add (GetterFromJson.GetSectionStrValue (new List<string> { "PatronymicName", "FontName" }, jsonPath));
            fontFiles.Add (GetterFromJson.GetSectionStrValue (new List<string> { "Post", "FontFile" }, jsonPath));
            fontNames.Add (GetterFromJson.GetSectionStrValue (new List<string> { "Post", "FontName" }, jsonPath));
            fontFiles.Add (GetterFromJson.GetSectionStrValue (new List<string> { "Department", "FontFile" }, jsonPath));
            fontNames.Add (GetterFromJson.GetSectionStrValue (new List<string> { "Department", "FontName" }, jsonPath));

            IEnumerable<IConfigurationSection> unitings =
                      GetterFromJson.GetIncludedItemsOfSection (new List<string> { "UnitedTextBlocks" }, jsonPath);

            foreach ( IConfigurationSection unit in unitings )
            {
                IConfigurationSection fontFileSection = unit.GetSection ("FontFile");
                IConfigurationSection fontNameSection = unit.GetSection ("FontName");
                fontFiles.Add (fontFileSection.Value);
                fontNames.Add (fontNameSection.Value);
            }
        }
    }


    private static void SetFontsOnWindow ( List<string> fontFiles, List<string> fontNames, string resourcePath )
    {
        string localAppDataPath = Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData);
        string windowsFontsPath = Path.Combine (localAppDataPath, "Microsoft", "Windows", "Fonts");

        for ( int index = 0; index < fontFiles.Count; index++ )
        {
            string fontFile = fontFiles [index];
            string fontName = fontNames [index];

            bool fontIsEmpty = ( fontFile == null ) || ( fontName == null );

            if ( fontIsEmpty )
            {
                continue;
            }

            string fontPath = Path.Combine (windowsFontsPath, $"{fontFile}");
            string source = resourcePath + fontFile;

            if ( !File.Exists (fontPath) )
            {
                try
                {
                    File.Copy (source, fontPath);
                }
                catch ( System.IO.DirectoryNotFoundException ex ) { }
            }

            Microsoft.Win32.RegistryKey fontKey =
                Microsoft.Win32.Registry.CurrentUser.CreateSubKey (@"Software\Microsoft\Windows NT\CurrentVersion\Fonts");

            fontKey.SetValue ($"{fontName} (TrueType)", fontPath);
            fontKey.Close ();

            QuestPDF.Drawing.FontManager.RegisterFontWithCustomName (fontName, File.OpenRead (fontPath));
        }
    }
    private static void SetFontsOnLinux ( List<string> fontFiles, List<string> fontNames, string resourcePath )
    {
        string userDirectory = Environment.GetFolderPath (Environment.SpecialFolder.UserProfile);
        string linuxFontsPath = userDirectory + "/.local/share/fonts/";

        if ( !Directory.Exists (linuxFontsPath) )
        {
            Directory.CreateDirectory (linuxFontsPath);
        }

        for ( int index = 0; index < fontFiles.Count; index++ )
        {
            string fontFile = fontFiles [index];
            string fontName = fontNames [index];

            bool fontIsEmpty = ( fontFile == null ) || ( fontName == null );

            if ( fontIsEmpty )
            {
                continue;
            }

            string fontPath = Path.Combine (linuxFontsPath, $"{fontFile}");
            string source = resourcePath + fontFile;

            if ( !File.Exists (fontPath) )
            {
                try
                {
                    File.Copy (source, fontPath);
                }
                catch ( System.IO.DirectoryNotFoundException ex ) { }
            }

            string fontInstallingCommand = "fc-cache -f -v";
            App.ExecuteBashCommand (fontInstallingCommand);

            QuestPDF.Drawing.FontManager.RegisterFontWithCustomName (fontName, File.OpenRead (fontPath));
        }
    }


    private static void SetFonts ( List<string> fontFiles, List<string> fontNames, string sourceDir )
    {
        try
        {
            string usersFontStorageDir = GetUsersFontStorageDirectory ();

            if ( !Directory.Exists (usersFontStorageDir) )
            {
                Directory.CreateDirectory (usersFontStorageDir);
            }

            for ( int index = 0; index < fontFiles.Count; index++ )
            {
                string fontFile = fontFiles [index];
                string fontName = fontNames [index];

                bool fontIsEmpty = ( string.IsNullOrWhiteSpace (fontFile) || string.IsNullOrWhiteSpace (fontName) );

                if ( fontIsEmpty )
                {
                    continue;
                }

                string userFontStorageFile = PlaceFontFileToUsersStorage (usersFontStorageDir, fontFile, sourceDir);

                if ( string.IsNullOrWhiteSpace (userFontStorageFile) )
                {
                    continue;
                }

                if ( _runningOnWindow )
                {
                    Microsoft.Win32.RegistryKey fontKey =
                    Microsoft.Win32.Registry.CurrentUser.CreateSubKey (@"Software\Microsoft\Windows NT\CurrentVersion\Fonts");

                    fontKey.SetValue ($"{fontName} (TrueType)", userFontStorageFile);
                    fontKey.Close ();
                }
                else if ( _runningOnLinux )
                {
                    string fontInstallingCommand = "fc-cache -f -v";
                    App.ExecuteBashCommand (fontInstallingCommand);
                }

                string filePath = sourceDir + fontFile;
                QuestPDF.Drawing.FontManager.RegisterFontWithCustomName (fontName, File.OpenRead (filePath));
            }
        }
        catch ( Exception ex ) { }
    }


    private static string GetUsersFontStorageDirectory ()
    {
        string fontStorageDir = string.Empty;

        if ( _runningOnWindow )
        {
            string localAppDataPath = Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData);
            fontStorageDir = Path.Combine (localAppDataPath, "Microsoft", "Windows", "Fonts");
        }
        else if ( _runningOnLinux )
        {
            string userDirectory = Environment.GetFolderPath (Environment.SpecialFolder.UserProfile);
            fontStorageDir = userDirectory + "/.local/share/fonts/";
        }

        return fontStorageDir;
    }


    private static string PlaceFontFileToUsersStorage ( string usersFontStorageDir, string fontFile, string sourceDir )
    {
        string userFontStorageFile = Path.Combine (usersFontStorageDir, $"{fontFile}");
        string sourceFile = sourceDir + fontFile;

        if ( !File.Exists (userFontStorageFile) )
        {
            try
            {
                File.Copy (sourceFile, userFontStorageFile);
            }
            catch ( System.IO.DirectoryNotFoundException ex ) { }
            catch ( System.IO.FileNotFoundException ex ) { }
        }

        return userFontStorageFile;
    }

}



public static class ServiceCollectionExtensions
{
    public static void AddNeededServices ( this IServiceCollection collection )
    {
        collection.AddSingleton <IServiceProvider, BadgeAppearenceServiceProvider> ();
        collection.AddSingleton <IPeopleSourceFactory, PeopleSourceFactory> ();
        collection.AddSingleton <IRowSource, PeopleXlsxSource> ();
        collection.AddSingleton <Lister.ViewModels.ConverterToPdf> ();
        collection.AddSingleton <Lister.ViewModels.PdfPrinter> ();
        collection.AddSingleton <IUniformDocumentAssembler, UniformDocAssembler> ();

        collection.AddSingleton (typeof (IBadgeAppearenceProvider), BadgeAppearenceFactory);
        collection.AddSingleton (typeof (IBadLineColorProvider), BadLineFactory);

        collection.AddSingleton (typeof (ModernMainViewModel), MainViewModelFactory);
        collection.AddSingleton <BadgeViewModel> ();
        collection.AddSingleton <ImageViewModel> ();
        collection.AddSingleton <PageViewModel> ();
        collection.AddSingleton <PersonChoosingViewModel> ();
        collection.AddSingleton <PersonSourceViewModel> ();
        collection.AddSingleton <SceneViewModel> ();
        collection.AddSingleton <BadgesBuildingViewModel> ();
        collection.AddSingleton <PageNavigationZoomerViewModel> ();
        collection.AddSingleton <PageNavigationZoomer> ();
        collection.AddSingleton <TextLineViewModel> ();
        collection.AddSingleton <WaitingViewModel> ();
        collection.AddSingleton <LargeMessageViewModel> ();
        collection.AddSingleton <PrintDialogViewModel> ();
    }


    private static IBadgeAppearenceProvider BadgeAppearenceFactory ( IServiceProvider serviceProvider )
    {
        object service = serviceProvider.GetService (typeof (BadgeAppearenceProvider));

        IBadgeAppearenceProvider result = 
            new BadgeAppearenceProvider (App.ResourceDirectoryUri, ( App.WorkDirectoryPath + App.ResourceFolderName )
                                                               , ( App.WorkDirectoryPath + App.JsonSchemeFolderName ));

        return result;
    }


    private static IBadLineColorProvider BadLineFactory ( IServiceProvider serviceProvider )
    {
        object service = serviceProvider.GetService (typeof (BadgeAppearenceProvider));

        IBadLineColorProvider result =
             new BadgeAppearenceProvider (App.ResourceDirectoryUri, ( App.WorkDirectoryPath + App.ResourceFolderName )
                                                                , ( App.WorkDirectoryPath + App.JsonSchemeFolderName ));

        return result;
    }


    private static ModernMainViewModel MainViewModelFactory ( IServiceProvider serviceProvider )
    {
        ModernMainViewModel result = new ModernMainViewModel ( App.OsName );
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
            result = new BadgeAppearenceProvider (App.ResourceDirectoryUri, (App.WorkDirectoryPath + App.ResourceFolderName)
                                                                      , ( App.WorkDirectoryPath + App.JsonSchemeFolderName ));
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

