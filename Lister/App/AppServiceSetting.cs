using Avalonia.Media;
using Core.DataAccess.JsonHandlers;
using Core.BadgesProvider;
using Core.DataAccess.PeopleSource;
using Lister.ViewModels;
using Lister.Views;
using Microsoft.Extensions.DependencyInjection;
using Core.DocumentBuilder;

namespace Lister;

public static class ServiceCollectionExtensions
{
    public static void AddNeededServices ( this IServiceCollection collection )
    {
        collection.AddSingleton <PeopleXlsxSource> ();
        collection.AddSingleton (typeof (ConverterToPdf), ConverterToPdfFactory);
        collection.AddSingleton <Lister.ViewModels.PdfPrinter> ();

        collection.AddSingleton ( typeof ( DocumentBuilder ), DocumentBuilderFactory );

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


    private static ConverterToPdf ConverterToPdfFactory ( IServiceProvider serviceProvider )
    {
        return new ConverterToPdf (App.OsName);
    }


    private static DocumentBuilder DocumentBuilderFactory ( IServiceProvider serviceProvider )
    {
        return new DocumentBuilder ( TextWidthMeasurer.GetMesurer() );
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

        return new PersonChoosingViewModel (placeHolder, inputLimit, incorrectTemplateColor
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

        return new SceneViewModel (badgeLimit, extentionToolTip, shrinkingToolTip, fileIsOpenMessage
                                                             , new DocumentBuilder (TextWidthMeasurer.GetMesurer ()) );
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

        return new PrintDialogViewModel (warnImagePath, emptyCopies, emptyPages, emptyPrinters, App.OsName);
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
