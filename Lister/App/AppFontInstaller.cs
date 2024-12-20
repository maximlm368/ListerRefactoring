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


    //private static void SetFontsOnWindow ( List<string> fontFiles, List<string> fontNames, string resourcePath )
    //{
    //    string localAppDataPath = Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData);
    //    string windowsFontsPath = Path.Combine (localAppDataPath, "Microsoft", "Windows", "Fonts");

    //    for ( int index = 0; index < fontFiles.Count; index++ )
    //    {
    //        string fontFile = fontFiles [index];
    //        string fontName = fontNames [index];

    //        bool fontIsEmpty = ( fontFile == null ) || ( fontName == null );

    //        if ( fontIsEmpty )
    //        {
    //            continue;
    //        }

    //        string fontPath = Path.Combine (windowsFontsPath, $"{fontFile}");
    //        string source = resourcePath + fontFile;

    //        if ( !File.Exists (fontPath) )
    //        {
    //            try
    //            {
    //                File.Copy (source, fontPath);
    //            }
    //            catch ( System.IO.DirectoryNotFoundException ex ) { }
    //        }

    //        Microsoft.Win32.RegistryKey fontKey =
    //            Microsoft.Win32.Registry.CurrentUser.CreateSubKey (@"Software\Microsoft\Windows NT\CurrentVersion\Fonts");

    //        fontKey.SetValue ($"{fontName} (TrueType)", fontPath);
    //        fontKey.Close ();

    //        QuestPDF.Drawing.FontManager.RegisterFontWithCustomName (fontName, File.OpenRead (fontPath));
    //    }
    //}


    //private static void SetFontsOnLinux ( List<string> fontFiles, List<string> fontNames, string resourcePath )
    //{
    //    string userDirectory = Environment.GetFolderPath (Environment.SpecialFolder.UserProfile);
    //    string linuxFontsPath = userDirectory + "/.local/share/fonts/";

    //    if ( !Directory.Exists (linuxFontsPath) )
    //    {
    //        Directory.CreateDirectory (linuxFontsPath);
    //    }

    //    for ( int index = 0; index < fontFiles.Count; index++ )
    //    {
    //        string fontFile = fontFiles [index];
    //        string fontName = fontNames [index];

    //        bool fontIsEmpty = ( fontFile == null ) || ( fontName == null );

    //        if ( fontIsEmpty )
    //        {
    //            continue;
    //        }

    //        string fontPath = Path.Combine (linuxFontsPath, $"{fontFile}");
    //        string source = resourcePath + fontFile;

    //        if ( !File.Exists (fontPath) )
    //        {
    //            try
    //            {
    //                File.Copy (source, fontPath);
    //            }
    //            catch ( System.IO.DirectoryNotFoundException ex ) { }
    //        }

    //        string fontInstallingCommand = "fc-cache -f -v";
    //        App.ExecuteBashCommand (fontInstallingCommand);

    //        QuestPDF.Drawing.FontManager.RegisterFontWithCustomName (fontName, File.OpenRead (fontPath));
    //    }
    //}


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