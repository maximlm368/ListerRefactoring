using Avalonia.Media;
using Core.BadgesCreator;
using Core.DocumentProcessor;
using Microsoft.Extensions.DependencyInjection;
using View.App.Configs;
using View.CoreAbstractionsImplimentations.BadgeCreator;
using View.CoreAbstractionsImplimentations.DataAccess;
using View.CoreAbstractionsImplimentations.DocumentProcessor;
using View.CoreModelReflection;
using View.CoreModelReflection.Badge;
using View.DialogMessageWindows.LargeMessage.ViewModel;
using View.DialogMessageWindows.PrintDialog.ViewModel;
using View.EditionView.ViewModel;
using View.MainWindow.MainView.Parts.BuildButton.ViewModel;
using View.MainWindow.MainView.Parts.NavigationZoom.ViewModel;
using View.MainWindow.MainView.Parts.PersonChoosing;
using View.MainWindow.MainView.Parts.PersonChoosing.ViewModel;
using View.MainWindow.MainView.Parts.PersonSource.ViewModel;
using View.MainWindow.MainView.Parts.Scene.ViewModel;
using View.MainWindow.MainView.ViewModel;
using View.WaitingView.ViewModel;


namespace View.App;

public static class ServiceCollectionExtensions
{
    public static void AddNeededServices ( this IServiceCollection collection )
    {
        collection.AddSingleton ( typeof ( CoreModelReflection.PdfPrinter ), PdfPrinterFactory );
        collection.AddSingleton ( typeof ( DocumentProcessor ), DocumentBuilderFactory );

        collection.AddSingleton (typeof (MainViewModel), MainViewModelFactory);
        collection.AddSingleton (typeof (PersonChoosingViewModel), PersonChoosingViewModelFactory);
        collection.AddSingleton (typeof (PersonSourceViewModel), PersonSourceViewModelFactory);
        collection.AddSingleton (typeof (SceneViewModel), SceneViewModelFactory);
        collection.AddSingleton (typeof (NavigationZoomViewModel), NavigationZoomerViewModelFactory);
        collection.AddSingleton (typeof (PrintDialogViewModel), PrintDialogViewModelFactory);
        collection.AddSingleton (typeof (EditorViewModelArgs), EditorViewModelArgsFactory);
        collection.AddSingleton <BadgesBuildingViewModel> ();
        collection.AddSingleton (typeof (WaitingViewModel), WaitingViewModelFactory);
        collection.AddSingleton <LargeMessageViewModel> ();
    }


    private static DocumentProcessor DocumentBuilderFactory ( IServiceProvider serviceProvider )
    {
        return DocumentProcessor.GetInstance ( TextWidthMeasurer.GetMesurer(), PdfCreator.GetInstance ( ListerApp.OsName )
            , CoreAbstractionsImplimentations.DocumentProcessor.PdfPrinterImplementation.GetInstance( ListerApp.OsName, PdfCreator.GetInstance ( ListerApp.OsName ) )
            , BadgeLayoutProvider.GetInstance(), PeopleSourceFactory.GetInstance () );
    }


    private static CoreModelReflection.PdfPrinter PdfPrinterFactory ( IServiceProvider serviceProvider )
    {
        DocumentProcessor model =
        DocumentProcessor.GetInstance ( TextWidthMeasurer.GetMesurer (), PdfCreator.GetInstance ( ListerApp.OsName )
            , CoreAbstractionsImplimentations.DocumentProcessor.PdfPrinterImplementation.GetInstance ( ListerApp.OsName, PdfCreator.GetInstance ( ListerApp.OsName))
            , BadgeLayoutProvider.GetInstance (), PeopleSourceFactory.GetInstance () );
    
        return new CoreModelReflection.PdfPrinter ( model );
    }


    private static MainViewModel MainViewModelFactory ( IServiceProvider serviceProvider )
    {
        string suggestedName = MainViewConfigs.pdfFileName;
        string saveTitle = MainViewConfigs.saveTitle;
        string incorrectXSLX = MainViewConfigs.incorrectXSLX;
        string limitIsExhaustedMessage = MainViewConfigs.limitIsExhaustedMessage;
        string fileIsOpenMessage = MainViewConfigs.fileIsOpenMessage;
        string fileIsTooBigMessage = MainViewConfigs.fileIsTooBig;

        return new MainViewModel (ListerApp.OsName, suggestedName, saveTitle, incorrectXSLX, limitIsExhaustedMessage
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
        GetterFromJson.GetSectionStrValue (new List<string> { "personSource" }, ListerApp.ConfigPath);

        BadgeCreator badgesCreator = BadgeCreator.GetInstance ( BadgeLayoutProvider.GetInstance ()
                                                              , PeopleSourceFactory.GetInstance () );

        PersonSourceViewModel result = 
        new PersonSourceViewModel (pickerTitle, filePickerTitle, patterns, headers, badgeLimit, sourcePathKeeper, badgesCreator);

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

        BadgeCreator badgesCreator = BadgeCreator.GetInstance ( BadgeLayoutProvider.GetInstance ()
                                                              , PeopleSourceFactory.GetInstance() );

        return new PersonChoosingViewModel (placeHolder, inputLimit, incorrectTemplateColor
                                                  , defaultColors, focusedColors, selectedColors, badgesCreator);
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

        return new SceneViewModel ( badgeLimit, extentionToolTip, shrinkingToolTip, fileIsOpenMessage
             , DocumentProcessor.GetInstance ( TextWidthMeasurer.GetMesurer (), PdfCreator.GetInstance ( ListerApp.OsName )
             , CoreAbstractionsImplimentations.DocumentProcessor.PdfPrinterImplementation.GetInstance ( ListerApp.OsName, PdfCreator.GetInstance ( ListerApp.OsName))
             , BadgeLayoutProvider.GetInstance (), PeopleSourceFactory.GetInstance () ) 
             );
    }


    private static NavigationZoomViewModel NavigationZoomerViewModelFactory ( IServiceProvider serviceProvider )
    {
        string procentSymbol = NavigationZoomerConfigs.procentSymbol;

        short maxDepth = NavigationZoomerConfigs.maxDepth;

        short minDepth = NavigationZoomerConfigs.minDepth;

        return new NavigationZoomViewModel (procentSymbol, maxDepth, minDepth);
    }


    private static PrintDialogViewModel PrintDialogViewModelFactory ( IServiceProvider serviceProvider )
    {
        string warnImagePath = PrintDialogConfigs.warnImagePath;
        string emptyCopies = PrintDialogConfigs.emptyCopies;
        string emptyPages = PrintDialogConfigs.emptyPages;
        string emptyPrinters = PrintDialogConfigs.emptyPrinters;

        return new PrintDialogViewModel (warnImagePath, emptyCopies, emptyPages, emptyPrinters, ListerApp.OsName);
    }


    private static EditorViewModelArgs EditorViewModelArgsFactory ( IServiceProvider serviceProvider )
    {
        string extentionToolTip = EditionViewConfigs.extentionToolTip;
        string shrinkingToolTip = EditionViewConfigs.shrinkingToolTip;
        string allFilter = EditionViewConfigs.allFilter;
        string incorrectFilter = EditionViewConfigs.incorrectFilter;

        string correctFilter = EditionViewConfigs.correctFilter;

        string allTip = EditionViewConfigs.allTip;

        string correctTip = EditionViewConfigs.correctTip;

        string incorrectTip = EditionViewConfigs.incorrectTip;

        SolidColorBrush focusedFontColor = GetColor (EditionViewConfigs.focusedFontColor);
        SolidColorBrush releasedFontColor = GetColor (EditionViewConfigs.releasedFontColor);
        SolidColorBrush focusedFontBorderColor = GetColor (EditionViewConfigs.focusedFontBorderColor);
        SolidColorBrush releasedFontBorderColor = GetColor (EditionViewConfigs.releasedFontBorderColor);

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
        return new WaitingViewModel (WaitingElementConfigs.gifName);
    }
}
