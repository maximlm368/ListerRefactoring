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
using ExtentionsAndAuxiliary;

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
        collection.AddSingleton (typeof (IMemberColorProvider), BadLineFactory);

        collection.AddSingleton (typeof (MainViewModel), MainViewModelFactory);
        collection.AddSingleton (typeof (PersonChoosingViewModel), PersonChoosingViewModelFactory);
        collection.AddSingleton (typeof (PersonSourceViewModel), PersonSourceViewModelFactory);
        collection.AddSingleton (typeof (SceneViewModel), SceneViewModelFactory);
        collection.AddSingleton (typeof (PageNavigationZoomerViewModel), NavigationZoomerViewModelFactory);
        collection.AddSingleton (typeof (PrintDialogViewModel), PrintDialogViewModelFactory);
        collection.AddSingleton (typeof (EditorViewModelArgs), EditorViewModelArgsFactory);
        collection.AddSingleton <BadgesBuildingViewModel> ();
        collection.AddSingleton (typeof (WaitingViewModel), WaitingViewModelFactory);
        collection.AddSingleton <LargeMessageViewModel> ();
    }


    private static IBadgeAppearenceProvider BadgeAppearenceFactory ( IServiceProvider serviceProvider )
    {
        object service = serviceProvider.GetService (typeof (BadgeAppearenceProvider));

        IBadgeAppearenceProvider result = new BadgeAppearenceProvider ();

        return result;
    }


    private static IMemberColorProvider BadLineFactory ( IServiceProvider serviceProvider )
    {
        object service = serviceProvider.GetService (typeof (BadgeAppearenceProvider));

        IMemberColorProvider result =
             new BadgeAppearenceProvider ();

        return result;
    }


    private static MainViewModel MainViewModelFactory ( IServiceProvider serviceProvider )
    {
        string suggestedName = MainViewConfigs.pdfFileName;
        string saveTitle = MainViewConfigs.saveTitle;
        string incorrectXSLX = MainViewConfigs.incorrectXSLX;
        string limitIsExhaustedMessage = MainViewConfigs.limitIsExhaustedMessage;
        string fileIsOpenMessage = MainViewConfigs.fileIsOpenMessage;
        string fileIsTooBigMessage = MainViewConfigs.fileIsTooBig;

        return new MainViewModel (App.OsName, suggestedName, saveTitle, incorrectXSLX, limitIsExhaustedMessage
                                                                      , fileIsOpenMessage, fileIsTooBigMessage);
    }


    private static PersonSourceViewModel PersonSourceViewModelFactory ( IServiceProvider serviceProvider )
    {
        string pickerTitle = PersonSourceConfigs.pickerTitle;
        string filePickerTitle = PersonSourceConfigs.filePickerTitle;

        List<string> headers = PersonSourceConfigs.xlsxHeaderNames.ToList();
        List<string> patterns = PersonSourceConfigs.sourceExtentions.ToList();

        int badgeLimit = PersonSourceConfigs.badgeCountLimit;

        string sourcePathKeeper =
        GetterFromJson.GetSectionStrValue (new List<string> { "personSource" }, App.ConfigPath);

        PersonSourceViewModel result = 
                    new PersonSourceViewModel (pickerTitle, filePickerTitle, patterns, headers, badgeLimit, sourcePathKeeper);
        return result;
    }


    private static PersonChoosingViewModel PersonChoosingViewModelFactory ( IServiceProvider serviceProvider )
    {
        string placeHolder = PersonChoosingConfigs.placeHolder;

        int inputLimit = PersonChoosingConfigs.inputLimit;

        SolidColorBrush entireListColor = GetColor (PersonChoosingConfigs.entireListColor);

        SolidColorBrush incorrectTemplateColor = GetColor (PersonChoosingConfigs.incorrectTemplateColor);

        SolidColorBrush defaultBackgroundColor = GetColor (PersonChoosingConfigs.defaultBackgroundColor);
        SolidColorBrush defaultBorderColor = GetColor (PersonChoosingConfigs.defaultBorderColor);
        SolidColorBrush defaultForegroundColor = GetColor (PersonChoosingConfigs.defaultForegroundColor);

        List <SolidColorBrush> defaultColors = new List <SolidColorBrush> () { defaultBackgroundColor, defaultBorderColor 
                                                                              , defaultForegroundColor};

        SolidColorBrush focusedBackgroundColor = GetColor (PersonChoosingConfigs.focusedBackgroundColor);
        SolidColorBrush focusedBorderColor = GetColor (PersonChoosingConfigs.focusedBorderColor);

        List <SolidColorBrush> focusedColors = new List <SolidColorBrush> () { focusedBackgroundColor, focusedBorderColor };

        SolidColorBrush hoveredBackgroundColor = GetColor (PersonChoosingConfigs.hoveredBackgroundColor);
        SolidColorBrush hoveredBorderColor = GetColor (PersonChoosingConfigs.hoveredBorderColor);

        List <SolidColorBrush> hoveredColors = new List <SolidColorBrush> () { hoveredBackgroundColor, hoveredBorderColor };

        SolidColorBrush selectedBackgroundColor = GetColor (PersonChoosingConfigs.selectedBackgroundColor);
        SolidColorBrush selectedBorderColor = GetColor (PersonChoosingConfigs.selectedBorderColor);
        SolidColorBrush selectedForegroundColor = GetColor (PersonChoosingConfigs.selectedForegroundColor);

        List <SolidColorBrush> selectedColors = new List <SolidColorBrush> () { selectedBackgroundColor, selectedBorderColor
                                                                            , selectedForegroundColor };

        PersonChoosingUserControl.SetComboboxHoveredItemColors (hoveredBackgroundColor, hoveredBorderColor);
        VisiblePerson.SetColors (defaultColors, focusedColors, selectedColors);

        return new PersonChoosingViewModel (placeHolder, entireListColor, inputLimit, incorrectTemplateColor
                                                  , defaultColors, focusedColors, selectedColors);
    }


    private static SolidColorBrush GetColor ( string hexColor )
    {
        Color color;

        if ( Color.TryParse(hexColor, out color) ) 
        {
            return new SolidColorBrush (color);
        }


        return new SolidColorBrush (new Color (255, 0, 0, 0));
    }


    private static SceneViewModel SceneViewModelFactory ( IServiceProvider serviceProvider )
    {
        int badgeLimit = SceneConfigs.badgeCountLimit;
        string extentionToolTip = SceneConfigs.extentionToolTip;
        string shrinkingToolTip = SceneConfigs.shrinkingToolTip;
        string fileIsOpenMessage = SceneConfigs.fileIsOpenMessage;

        return new SceneViewModel (badgeLimit, extentionToolTip, shrinkingToolTip, fileIsOpenMessage);
    }


    private static PageNavigationZoomerViewModel NavigationZoomerViewModelFactory ( IServiceProvider serviceProvider )
    {
        string procentSymbol = NavigationZoomerConfigs.procentSymbol;

        short maxDepth = NavigationZoomerConfigs.maxDepth;

        short minDepth = NavigationZoomerConfigs.minDepth;

        return new PageNavigationZoomerViewModel (procentSymbol, maxDepth, minDepth);
    }


    private static PrintDialogViewModel PrintDialogViewModelFactory ( IServiceProvider serviceProvider )
    {
        string warnImagePath = PrintDialogConfigs.warnImagePath;
        string emptyCopies = PrintDialogConfigs.emptyCopies;
        string emptyPages = PrintDialogConfigs.emptyPages;
        string emptyPrinters = PrintDialogConfigs.emptyPrinters;

        return new PrintDialogViewModel (warnImagePath, emptyCopies, emptyPages, emptyPrinters);
    }


    private static EditorViewModelArgs EditorViewModelArgsFactory ( IServiceProvider serviceProvider )
    {
        string extentionToolTip = EditorConfigs.extentionToolTip;
        string shrinkingToolTip = EditorConfigs.shrinkingToolTip;
        string allFilter = EditorConfigs.allFilter;
        string incorrectFilter = EditorConfigs.incorrectFilter;

        string correctFilter = EditorConfigs.correctFilter;

        string allTip = EditorConfigs.allTip;

        string correctTip = EditorConfigs.correctTip;

        string incorrectTip = EditorConfigs.incorrectTip;

        SolidColorBrush focusedFontColor = GetColor (EditorConfigs.focusedFontColor);
        SolidColorBrush releasedFontColor = GetColor (EditorConfigs.releasedFontColor);
        SolidColorBrush focusedFontBorderColor = GetColor (EditorConfigs.focusedFontBorderColor);
        SolidColorBrush releasedFontBorderColor = GetColor (EditorConfigs.releasedFontBorderColor);

        EditorViewModelArgs result = new ();

        result.extentionToolTip = extentionToolTip;
        result.shrinkingToolTip = shrinkingToolTip;
        result.allFilter = allFilter;
        result.correctFilter = correctFilter;
        result.incorrectFilter = incorrectFilter;
        result.allTip = allTip;
        result.correctTip = correctTip;
        result.incorrectTip = incorrectTip;

        result.focusedFontSizeColor = focusedFontColor;
        result.releasedFontSizeColor = releasedFontColor;
        result.focusedFontSizeBorderColor = focusedFontBorderColor;
        result.releasedFontSizeBorderColor = releasedFontBorderColor;

        return result;
    }


    private static WaitingViewModel WaitingViewModelFactory ( IServiceProvider serviceProvider )
    {
        return new WaitingViewModel (WaitingConfigs.gifName);
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
            result = new BadgeAppearenceProvider ();
        }

        return result;
    }
}