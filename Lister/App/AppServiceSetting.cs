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
using static SkiaSharp.HarfBuzz.SKShaper;

namespace Lister;

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

        collection.AddSingleton (typeof (MainViewModel), MainViewModelFactory);
        collection.AddSingleton (typeof (PersonChoosingViewModel), PersonChoosingViewModelFactory);
        collection.AddSingleton (typeof (PersonSourceViewModel), PersonSourceViewModelFactory);
        collection.AddSingleton (typeof (SceneViewModel), SceneViewModelFactory);
        collection.AddSingleton (typeof (PageNavigationZoomerViewModel), NavigationZoomerViewModelFactory);
        collection.AddSingleton (typeof (PrintDialogViewModel), PrintDialogViewModelFactory);
        collection.AddSingleton (typeof (EditorViewModelArgs), EditorViewModelArgsFactory);
        collection.AddSingleton <BadgeViewModel> ();
        collection.AddSingleton <ImageViewModel> ();
        collection.AddSingleton <PageViewModel> ();
        collection.AddSingleton <BadgesBuildingViewModel> ();
        collection.AddSingleton <TextLineViewModel> ();
        collection.AddSingleton <WaitingViewModel> ();
        collection.AddSingleton <LargeMessageViewModel> ();
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


    private static MainViewModel MainViewModelFactory ( IServiceProvider serviceProvider )
    {
        string suggestedName =
        GetterFromJson.GetSectionStrValue (new List<string> { "MainViewModel", "pdfFileName" }, App.ConfigPath);

        string saveTitle =
        GetterFromJson.GetSectionStrValue (new List<string> { "MainViewModel", "saveTitle" }, App.ConfigPath);

        string incorrectXSLX =
        GetterFromJson.GetSectionStrValue (new List<string> { "MainViewModel", "incorrectXSLX" }, App.ConfigPath);

        string limitIsExhaustedMessage =
        GetterFromJson.GetSectionStrValue (new List<string> { "MainViewModel", "limitIsExhaustedMessage" }, App.ConfigPath);

        string fileIsOpenMessage =
        GetterFromJson.GetSectionStrValue (new List<string> { "MainViewModel", "fileIsOpenMessage" }, App.ConfigPath);

        IEnumerable<IConfigurationSection> sections =
        GetterFromJson.GetChildren (new List<string> { "MainViewModel", "patterns" }, App.ConfigPath);

        List<string> patterns = new ();

        foreach ( IConfigurationSection section in sections )
        {
            patterns.Add (section.Value);
        }

        return new MainViewModel (App.OsName, suggestedName, saveTitle, incorrectXSLX, limitIsExhaustedMessage
                                                                                           , fileIsOpenMessage);
    }


    private static PersonSourceViewModel PersonSourceViewModelFactory ( IServiceProvider serviceProvider )
    {
        List<string> args = new ();

        string pickerTitle =
        GetterFromJson.GetSectionStrValue (new List<string> { "PersonSourceViewModel", "pickerTitle" }, App.ConfigPath);
        args.Add (pickerTitle);

        string sourcePathKeeper =
        GetterFromJson.GetSectionStrValue (new List<string> { "PersonSourceViewModel", "sourcePathKeeper" }, App.ConfigPath);
        args.Add (sourcePathKeeper);

        string fileIsOpenMessage =
        GetterFromJson.GetSectionStrValue (new List<string> { "PersonSourceViewModel", "filePickerTitle" }, App.ConfigPath);
        args.Add (fileIsOpenMessage);

        IEnumerable<IConfigurationSection> sections =
            GetterFromJson.GetChildren (new List<string> { "PersonSourceViewModel", "xlsxHeaderNames" }, App.ConfigPath);

        List<string> headers = new ();

        foreach ( IConfigurationSection section in sections )
        {
            headers.Add (section.Value);
        }

        sections =
            GetterFromJson.GetChildren (new List<string> { "PersonSourceViewModel", "sourceExtentions" }, App.ConfigPath);

        List<string> patterns = new ();

        foreach ( IConfigurationSection section in sections )
        {
            patterns.Add (section.Value);
        }

        PersonSourceViewModel result = new PersonSourceViewModel (args, patterns, headers);
        return result;
    }


    private static PersonChoosingViewModel PersonChoosingViewModelFactory ( IServiceProvider serviceProvider )
    {
        string placeHolder =
        GetterFromJson.GetSectionStrValue (new List<string> { "PersonChoosingViewModel", "placeHolder" }, App.ConfigPath);

        SolidColorBrush entireListColor = GetColor ("PersonChoosingViewModel", "entireListColor");
        SolidColorBrush focusedBackgroundColor = GetColor ("PersonChoosingViewModel", "focusedBackgroundColor");
        SolidColorBrush unfocusedColor = GetColor ("PersonChoosingViewModel", "unfocusedColor");
        SolidColorBrush focusedBorderColor = GetColor ("PersonChoosingViewModel", "focusedBorderColor");

        return new PersonChoosingViewModel (placeHolder, entireListColor, focusedBackgroundColor, unfocusedColor
                                                                                             , focusedBorderColor);
    }


    private static SolidColorBrush GetColor ( string forWhat, string colorSectionName )
    {
        IEnumerable<IConfigurationSection> sections =
        GetterFromJson.GetChildren (new List<string> { forWhat, colorSectionName }, App.ConfigPath);

        List<int> rgb = new () { 100, 100, 100 };
        int counter = 0;

        foreach ( IConfigurationSection section in sections )
        {
            int rgbIndex = 0;

            try
            {
                rgbIndex = int.Parse (section.Value);
            }
            catch ( Exception ex ) { }

            rgb [counter] = rgbIndex;
            counter++;

            if ( counter >= 3 ) break;
        }

        return new SolidColorBrush (new Color (255, ( byte ) rgb [0], ( byte ) rgb [1], ( byte ) rgb [2]));
    }


    private static SceneViewModel SceneViewModelFactory ( IServiceProvider serviceProvider )
    {
        int badgeLimit = 0;

        string badgeLimitStr =
        GetterFromJson.GetSectionStrValue (new List<string> { "SceneViewModel", "badgeCountLimit" }, App.ConfigPath);

        try
        {
            badgeLimit = int.Parse (badgeLimitStr);
        }
        catch ( Exception ex ) { }

        string extentionToolTip =
        GetterFromJson.GetSectionStrValue (new List<string> { "SceneViewModel", "extentionToolTip" }, App.ConfigPath);

        string shrinkingToolTip =
        GetterFromJson.GetSectionStrValue (new List<string> { "SceneViewModel", "shrinkingToolTip" }, App.ConfigPath);

        string fileIsOpenMessage =
        GetterFromJson.GetSectionStrValue (new List<string> { "SceneViewModel", "fileIsOpenMessage" }, App.ConfigPath);

        return new SceneViewModel (badgeLimit, extentionToolTip, shrinkingToolTip, fileIsOpenMessage);
    }


    private static PageNavigationZoomerViewModel NavigationZoomerViewModelFactory ( IServiceProvider serviceProvider )
    {
        string procentSymbol =
        GetterFromJson.GetSectionStrValue (new List<string> { "NavigationZoomerViewModel", "procentSymbol" }, App.ConfigPath);

        string maxDepthStr =
        GetterFromJson.GetSectionStrValue (new List<string> { "NavigationZoomerViewModel", "maxDepth" }, App.ConfigPath);
        short maxDepth = 3;

        try
        {
            maxDepth = short.Parse (maxDepthStr);
        }
        catch ( Exception ex ) { }

        string minDepthStr =
        GetterFromJson.GetSectionStrValue (new List<string> { "NavigationZoomerViewModel", "minDepth" }, App.ConfigPath);
        short minDepth = -3;

        try
        {
            minDepth = short.Parse (minDepthStr);
        }
        catch ( Exception ex ) { }

        if ( maxDepth < 1 )
        {
            maxDepth = 3;
        }

        if ( minDepth > -1 )
        {
            minDepth = -3;
        }

        return new PageNavigationZoomerViewModel (procentSymbol, maxDepth, minDepth);
    }


    private static PrintDialogViewModel PrintDialogViewModelFactory ( IServiceProvider serviceProvider )
    {
        string warnImagePath =
        GetterFromJson.GetSectionStrValue (new List<string> { "PrintDialogViewModel", "warnImagePath" }, App.ConfigPath);

        string emptyCopies =
        GetterFromJson.GetSectionStrValue (new List<string> { "PrintDialogViewModel", "emptyCopies" }, App.ConfigPath);

        string emptyPages =
        GetterFromJson.GetSectionStrValue (new List<string> { "PrintDialogViewModel", "emptyPages" }, App.ConfigPath);

        string emptyPrinters =
        GetterFromJson.GetSectionStrValue (new List<string> { "PrintDialogViewModel", "emptyPrinters" }, App.ConfigPath);

        return new PrintDialogViewModel (warnImagePath, emptyCopies, emptyPages, emptyPrinters);
    }


    private static EditorViewModelArgs EditorViewModelArgsFactory ( IServiceProvider serviceProvider )
    {
        string extentionToolTip =
        GetterFromJson.GetSectionStrValue (new List<string> { "EditorViewModel", "extentionToolTip" }, App.ConfigPath);

        string shrinkingToolTip =
        GetterFromJson.GetSectionStrValue (new List<string> { "EditorViewModel", "shrinkingToolTip" }, App.ConfigPath);

        string correctnessIcon =
        GetterFromJson.GetSectionStrValue (new List<string> { "EditorViewModel", "correctnessIcon" }, App.ConfigPath);

        string incorrectnessIcon =
        GetterFromJson.GetSectionStrValue (new List<string> { "EditorViewModel", "incorrectnessIcon" }, App.ConfigPath);

        string allLabel =
        GetterFromJson.GetSectionStrValue (new List<string> { "EditorViewModel", "allLabel" }, App.ConfigPath);

        string incorrectLabel =
        GetterFromJson.GetSectionStrValue (new List<string> { "EditorViewModel", "incorrectLabel" }, App.ConfigPath);

        string correctLabel =
        GetterFromJson.GetSectionStrValue (new List<string> { "EditorViewModel", "correctLabel" }, App.ConfigPath);

        string allTip =
        GetterFromJson.GetSectionStrValue (new List<string> { "EditorViewModel", "allTip" }, App.ConfigPath);

        string correctTip =
        GetterFromJson.GetSectionStrValue (new List<string> { "EditorViewModel", "correctTip" }, App.ConfigPath);

        string incorrectTip =
        GetterFromJson.GetSectionStrValue (new List<string> { "EditorViewModel", "incorrectTip" }, App.ConfigPath);

        SolidColorBrush focusedFontSizeColor = GetColor ("EditorViewModel", "focusedFontSizeColor");
        SolidColorBrush releasedFontSizeColor = GetColor ("EditorViewModel", "releasedFontSizeColor");
        SolidColorBrush focusedFontSizeBorderColor = GetColor ("EditorViewModel", "focusedFontSizeBorderColor");
        SolidColorBrush releasedFontSizeBorderColor = GetColor ("EditorViewModel", "releasedFontSizeBorderColor");

        EditorViewModelArgs result = new ();

        result.extentionToolTip = extentionToolTip;
        result.shrinkingToolTip = shrinkingToolTip;
        result.correctnessIcon = correctnessIcon;
        result.incorrectnessIcon = incorrectnessIcon;
        result.allLabel = allLabel;
        result.correctLabel = correctLabel;
        result.incorrectLabel = incorrectLabel;
        result.allTip = allTip;
        result.correctTip = correctTip;
        result.incorrectTip = incorrectTip;

        result.focusedFontSizeColor = focusedFontSizeColor;
        result.releasedFontSizeColor = releasedFontSizeColor;
        result.focusedFontSizeBorderColor = focusedFontSizeBorderColor;
        result.releasedFontSizeBorderColor = releasedFontSizeBorderColor;

        return result;
    }
}



public class BadgeAppearenceServiceProvider : IServiceProvider
{
    public object? GetService ( Type serviceType )
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
            result = new BadgeAppearenceProvider (App.ResourceDirectoryUri, ( App.WorkDirectoryPath + App.ResourceFolderName )
                                                                      , ( App.WorkDirectoryPath + App.JsonSchemeFolderName ));
        }

        return result;
    }
}