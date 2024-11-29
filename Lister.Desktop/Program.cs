using System;
using System.Data;
using System.Globalization;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.ReactiveUI;
using ContentAssembler;
using SkiaSharp;
using Avalonia.Skia;
using Avalonia.Media.Fonts;
using System.Reactive.Concurrency;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using DataGateway;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using DocumentFormat.OpenXml.Packaging;
using System.Text.Json;
//using Json.Schema;
//using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NJsonSchema.Validation;
using NJsonSchema.Generation;


namespace Lister.Desktop;

class Program
{
    private static bool _runningOnWindow = false;
    private static bool _runningOnLinux = false;


    public static void InstallFonts ( )
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
    }


    private static void GetFontFilesAndNames ( FileInfo [] jsonFiles, out List<string> fontFiles, out List<string> fontNames ) 
    {
        fontFiles = new ();
        fontNames = new ();

        foreach ( FileInfo fileInfo   in   jsonFiles )
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

            foreach ( IConfigurationSection unit   in   unitings )
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

        for ( int index = 0;   index < fontFiles.Count;   index++ )
        {
            string fontFile = fontFiles [index];
            string fontName = fontNames [index];

            bool fontIsEmpty = ( fontFile == null )   ||   (fontName == null);

            if (fontIsEmpty) 
            {
                continue;
            }

            string fontPath = Path.Combine (windowsFontsPath, $"{fontFile}");
            string source = resourcePath + fontFile;

            if ( ! File.Exists (fontPath) )
            {
                try 
                {
                    File.Copy (source, fontPath);
                }
                catch ( System.IO.DirectoryNotFoundException ex ) {}
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

        if ( ! Directory.Exists(linuxFontsPath) ) 
        {
            Directory.CreateDirectory (linuxFontsPath);
        }

        for ( int index = 0;   index < fontFiles.Count;   index++ )
        {
            string fontFile = fontFiles [index];
            string fontName = fontNames [index];

            bool fontIsEmpty = ( fontFile == null )   ||   ( fontName == null );

            if ( fontIsEmpty )
            {
                continue;
            }

            string fontPath = Path.Combine (linuxFontsPath, $"{fontFile}");
            string source = resourcePath + fontFile;

            if ( ! File.Exists (fontPath) )
            {
                try
                {
                    File.Copy (source, fontPath);
                }
                catch ( System.IO.DirectoryNotFoundException ex ) { }
            }

            string fontInstallingCommand = "fc-cache -f -v";
            App.ExecuteBashCommand ( fontInstallingCommand );

            QuestPDF.Drawing.FontManager.RegisterFontWithCustomName (fontName, File.OpenRead (fontPath));
        }
    }


    private static void SetFonts ( List<string> fontFiles, List<string> fontNames, string sourceDir )
    {
        try
        {
            string usersFontStorageDir = GetUsersFontStorageDirectory ();

            if ( ! Directory.Exists (usersFontStorageDir) )
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
        catch ( Exception ex ){}
    }


    private static string GetUsersFontStorageDirectory ( )
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

        if ( ! File.Exists (userFontStorageFile) )
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


    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main ( string [] args )
    {
        InstallFonts ();
        Thread.Sleep (1000);

        //bool isWithoutCollector = GC.TryStartNoGCRegion (500000000);

        try
        {
            BuildAvaloniaApp ()
            .StartWithClassicDesktopLifetime (args);
        }
        catch ( StackOverflowException ex )
        {
        }
        catch ( System.Threading.Tasks.TaskCanceledException ex ) 
        {
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont ()
            //.WithFontBySourceHanSansCN ()
            .LogToTrace()
            .UseReactiveUI();
}


//Process fileExplorer = new Process ();
//fileExplorer.StartInfo.FileName = "Nautilus";
//fileExplorer.StartInfo.Arguments = @"./";
//fileExplorer.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
//fileExplorer.Start ();





//public static class AppBuilderExtension
//{
//    public static AppBuilder WithFontBySourceHanSansCN ( this AppBuilder appBuilder )
//    {
//        FontFamily def = FontFamily.Default;

//        var uri = "avares://Assets/Pushkin.ttf  #  Pushkin";
//        var name = FontFamily.DefaultFontFamilyName;
//        name = "Pushkin";
//        Uri fontUri = new Uri (uri);
//        var ff = new FontFamily (fontUri, name);



//        FontManager.Current.AddFontCollection (new EmbeddedFontCollection (fontUri, fontUri));

//        var fontManager = new FontManager (new FontManagerImpl ());
//        fontManager.AddFontCollection (new EmbeddedFontCollection (fontUri, fontUri));




//        AppBuilder ab = appBuilder.With (new FontManagerOptions ()
//        {
//            DefaultFamilyName = uri,
//            FontFallbacks = new [] { new FontFallback { FontFamily = ff } }
//        });

//        return ab;
//    }
//}



//public class CustomFontManager : IFontManagerImpl
//{
//    private readonly string _defaultFamilyName;
//    private readonly IFontCollection _customFonts;
//    private bool _isInitialized;


//    public CustomFontManager ()
//    {
//        _defaultFamilyName = "Noto Mono";

//        var source = new Uri ("resm:Avalonia.Skia.UnitTests.Assets?assembly=Avalonia.Skia.UnitTests");

//        _customFonts = new EmbeddedFontCollection (source, source);
//    }


//    public string GetDefaultFontFamilyName ()
//    {
//        return _defaultFamilyName;
//    }


//    public string [] GetInstalledFontFamilyNames ( bool checkForUpdates = false )
//    {
//        if ( !_isInitialized )
//        {
//            _customFonts.Initialize (this);

//            _isInitialized = true;
//        }

//        return _customFonts.Select (x => x.Name).ToArray ();
//    }


//    private readonly string [] _bcp47 = { CultureInfo.CurrentCulture.ThreeLetterISOLanguageName, CultureInfo.CurrentCulture.TwoLetterISOLanguageName };


//    public bool TryMatchCharacter ( int codepoint, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch,
//        CultureInfo culture, out Typeface typeface )
//    {
//        if ( !_isInitialized )
//        {
//            _customFonts.Initialize (this);
//        }

//        if ( _customFonts.TryMatchCharacter (codepoint, fontStyle, fontWeight, fontStretch, null, culture, out typeface) )
//        {
//            return true;
//        }

//        var fallback = SKFontManager.Default.MatchCharacter (null, ( SKFontStyleWeight ) fontWeight,
//            ( SKFontStyleWidth ) fontStretch, ( SKFontStyleSlant ) fontStyle, _bcp47, codepoint);

//        typeface = new Typeface (fallback?.FamilyName ?? _defaultFamilyName, fontStyle, fontWeight);

//        return true;
//    }


//    public bool TryCreateGlyphTypeface ( string familyName, FontStyle style, FontWeight weight,
//    FontStretch stretch, [NotNullWhen (true)] out IGlyphTypeface glyphTypeface )
//    {
//        if ( !_isInitialized )
//        {
//            _customFonts.Initialize (this);
//        }

//        if ( _customFonts.TryGetGlyphTypeface (familyName, style, weight, stretch, out glyphTypeface) )
//        {
//            return true;
//        }

//        var skTypeface = SKTypeface.FromFamilyName (familyName,
//                    ( SKFontStyleWeight ) weight, SKFontStyleWidth.Normal, ( SKFontStyleSlant ) style);

//        //glyphTypeface = new GlyphTypefaceImpl (skTypeface, FontSimulations.None);

//        return true;
//    }


//    public bool TryCreateGlyphTypeface ( Stream stream, FontSimulations fontSimulations, [NotNullWhen (true)] out IGlyphTypeface glyphTypeface )
//    {
//        var skTypeface = SKTypeface.FromStream (stream);

//        //glyphTypeface = new GlyphTypefaceImpl (skTypeface, fontSimulations);
//        glyphTypeface = null;

//        return true;
//    }
//}




//public class CustomFontManagerImpl : IFontManagerImpl
//{
//    private readonly Typeface [] _customTypefaces;
//private readonly string _defaultFamilyName;

////Load font resources in the project, you can load multiple font resources
//private readonly Typeface _defaultTypeface =
//    new Typeface ("resm:ProjectManager.Assets.Fonts.msyh#微软雅黑");

//private readonly Typeface SegoeUiTypeFace =
//    new Typeface ("resm:ProjectManager.Assets.Fonts.segoeui#Segoe UI");


//private readonly Typeface SegUiVarTypeface =
//    new Typeface ("resm:ProjectManager.Assets.Fonts.SegUIVar#Segoe UI Variable Text");


//private readonly Typeface msjhTypeFace =
//    new Typeface ("resm:ProjectManager.Assets.Fonts.seguisym#Microsoft JhengHei UI");

//private readonly Typeface FluentAvaloniaTypeFace =
//    new Typeface ("resm:ProjectManager.Assets.Fonts.FluentAvalonia#Symbols");


//public CustomFontManagerImpl ()
//{
//    _customTypefaces = new []
//        { _defaultTypeface, SegoeUiTypeFace, SegUiVarTypeface, msjhTypeFace, FluentAvaloniaTypeFace};
//    _defaultFamilyName = _defaultTypeface.FontFamily.FamilyNames.PrimaryFamilyName;
//}

//public string GetDefaultFontFamilyName ()
//{
//    return _defaultFamilyName;
//}

//public IEnumerable<string> GetInstalledFontFamilyNames ( bool checkForUpdates = false )
//{
//    return _customTypefaces.Select (x => x.FontFamily.Name);
//}

//private readonly string [] _bcp47 =
//    { CultureInfo.CurrentCulture.ThreeLetterISOLanguageName, CultureInfo.CurrentCulture.TwoLetterISOLanguageName };

//public bool TryMatchCharacter ( int codepoint, FontStyle fontStyle, FontWeight fontWeight, FontFamily fontFamily,
//    CultureInfo culture, out Typeface typeface )
//{
//    foreach ( var customTypeface in _customTypefaces )
//    {
//        if ( customTypeface.GlyphTypeface.GetGlyph (( uint ) codepoint) == 0 )
//        {
//            continue;
//        }

//        typeface = new Typeface (customTypeface.FontFamily.Name, fontStyle, fontWeight);

//        return true;
//    }

//    var fallback = SKFontManager.Default.MatchCharacter (fontFamily?.Name, ( SKFontStyleWeight ) fontWeight,
//        SKFontStyleWidth.Normal, ( SKFontStyleSlant ) fontStyle, _bcp47, codepoint);

//    typeface = new Typeface (fallback?.FamilyName ?? _defaultFamilyName, fontStyle, fontWeight);

//    return true;
//}

//public IGlyphTypefaceImpl CreateGlyphTypeface ( Typeface typeface )
//{
//    SKTypeface skTypeface;
//    switch ( typeface.FontFamily.Name )
//    {
//        case "Segoe UI":
//            {
//                var typefaceCollection = Avalonia.Skia.SKTypefaceCollectionCache.GetOrAddTypefaceCollection (SegoeUiTypeFace.FontFamily);
//                skTypeface = typefaceCollection.Get (typeface);
//                break;
//            }
//        case FontFamily.DefaultFontFamilyName:
//        case "微软雅黑":
//            {
//                var typefaceCollection = SKTypefaceCollectionCache.GetOrAddTypefaceCollection (_defaultTypeface.FontFamily);
//                skTypeface = typefaceCollection.Get (typeface);
//                break;
//            }
//        case "Segoe UI Variable Text":
//            {
//                var typefaceCollection = SKTypefaceCollectionCache.GetOrAddTypefaceCollection (SegUiVarTypeface.FontFamily);
//                skTypeface = typefaceCollection.Get (typeface);
//                break;
//            }
//        case "Symbols":
//            {
//                var typefaceCollection = SKTypefaceCollectionCache.GetOrAddTypefaceCollection (FluentAvaloniaTypeFace.FontFamily);
//                skTypeface = typefaceCollection.Get (_defaultTypeface);
//                break;
//            }
//        default:
//            {
//                skTypeface = SKTypeface.FromFamilyName (typeface.FontFamily.Name,
//                    ( SKFontStyleWeight ) typeface.Weight, SKFontStyleWidth.Normal, ( SKFontStyleSlant ) typeface.Style);
//                break;
//            }
//    }
//    return new GlyphTypefaceImpl (skTypeface);
//}
//}



//public class CustFontManagerImpl : IFontManagerImpl
//{
//    private readonly Typeface [] _customTypefaces;
//    private readonly string _defaultFamilyName;

//    //Load font resources in the project, you can load multiple font resources
//    private readonly Typeface _defaultTypeface =
//        new Typeface (new FontFamily("Arial"));

//    private readonly Typeface SegoeUiTypeFace =
//        new Typeface ("resm:Test.Assets.Fonts.segoeui#Segoe UI");


//    private readonly Typeface SegUiVarTypeface =
//        new Typeface ("resm:Test.Assets.Fonts.SegUIVar#Segoe UI Variable Text");


//    private readonly Typeface msjhTypeFace =
//        new Typeface ("resm:Test.Assets.Fonts.seguisym#Microsoft JhengHei UI");

//    private readonly Typeface FluentAvaloniaTypeFace =
//        new Typeface ("resm:Test.Assets.Fonts.FluentAvalonia#Symbols");

//    private readonly string [] _bcp47 =
//    { CultureInfo.CurrentCulture.ThreeLetterISOLanguageName, CultureInfo.CurrentCulture.TwoLetterISOLanguageName };


//    public CustFontManagerImpl ()
//    {
//        _customTypefaces = new []
//            { _defaultTypeface, SegoeUiTypeFace, SegUiVarTypeface, msjhTypeFace, FluentAvaloniaTypeFace };
//        _defaultFamilyName = _defaultTypeface.FontFamily.FamilyNames.PrimaryFamilyName;
//    }


//    public string GetDefaultFontFamilyName ()
//    {
//        //return _defaultFamilyName;

//        return "";
//    }


//    public IEnumerable<string> GetInstalledFontFamilyNames ( bool checkForUpdates = false )
//    {
//        return _customTypefaces.Select (x => x.FontFamily.Name);
//    }


//    public bool TryMatchCharacter ( int codepoint, FontStyle fontStyle, FontWeight fontWeight, FontFamily fontFamily,
//        CultureInfo culture, out Typeface typeface )
//    {
//        foreach ( var customTypeface in _customTypefaces )
//        {
//            if ( customTypeface.GlyphTypeface.GetGlyph (( uint ) codepoint) == 0 )
//            {
//                continue;
//            }

//            typeface = new Typeface (customTypeface.FontFamily.Name, fontStyle, fontWeight);

//            return true;
//        }

//        var fallback = SKFontManager.Default.MatchCharacter (fontFamily?.Name, ( SKFontStyleWeight ) fontWeight,
//            SKFontStyleWidth.Normal, ( SKFontStyleSlant ) fontStyle, _bcp47, codepoint);

//        typeface = new Typeface (fallback?.FamilyName ?? _defaultFamilyName, fontStyle, fontWeight);

//        return true;
//    }


//    public GlyphTypefaceImpl CreateGlyphTypeface ( Typeface typeface )
//    {
//        SKTypeface skTypeface;

//        switch ( typeface.FontFamily.Name )
//        {
//            case "Segoe UI": //font family name
//                skTypeface = SKTypeface.FromFamilyName (SegoeUiTypeFace.FontFamily.Name);
//                break;
//            case "微软雅黑":
//                skTypeface = SKTypeface.FromFamilyName (_defaultTypeface.FontFamily.Name);
//                break;
//            case "Segoe UI Variable Text":
//                skTypeface = SKTypeface.FromFamilyName (SegUiVarTypeface.FontFamily.Name);
//                break;
//            case FontFamily.DefaultFontFamilyName:
//            case "Symbols":
//                skTypeface = SKTypeface.FromFamilyName (FluentAvaloniaTypeFace.FontFamily.Name);
//                break;
//            case "Microsoft JhengHei UI":
//                skTypeface = SKTypeface.FromFamilyName (msjhTypeFace.FontFamily.Name);
//                break;
//            default:
//                skTypeface = SKTypeface.FromFamilyName (typeface.FontFamily.Name,
//                    ( SKFontStyleWeight ) typeface.Weight, SKFontStyleWidth.Normal, ( SKFontStyleSlant ) typeface.Style);
//                break;
//        }

//        if ( skTypeface == null )
//        {
//            skTypeface = SKTypeface.FromFamilyName (_defaultTypeface.FontFamily.Name);
//        }

//        return new GlyphTypefaceImpl (skTypeface);
//    }
//}