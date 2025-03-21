using Avalonia.Media;
using Lister.Core.BadgesCreator;
using Lister.Core.DocumentProcessor;
using Lister.Desktop.App.Configs;
using Lister.Desktop.CoreAbstractionsImplimentations.BadgeCreator;
using Lister.Desktop.CoreAbstractionsImplimentations.DataAccess;
using Lister.Desktop.CoreAbstractionsImplimentations.DocumentProcessor;
using Lister.Desktop.CoreModelReflections;
using Lister.Desktop.CoreModelReflections.BadgeVM;
using Lister.Desktop.Views.MainWindow.EditionView.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using View.App;
using View.DialogMessageWindows.LargeMessage.ViewModel;
using View.DialogMessageWindows.PrintDialog.ViewModel;
using View.MainWindow.MainView.Parts.BuildButton.ViewModel;
using View.MainWindow.MainView.Parts.NavigationZoom.ViewModel;
using View.MainWindow.MainView.Parts.PersonChoosing;
using View.MainWindow.MainView.Parts.PersonChoosing.ViewModel;
using View.MainWindow.MainView.Parts.PersonSource.ViewModel;
using View.MainWindow.MainView.Parts.Scene.ViewModel;
using View.MainWindow.MainView.ViewModel;
using View.WaitingView.ViewModel;


namespace Lister.Desktop.App;

public static class ServiceCollectionExtensions
{
    public static void AddNeededServices(this IServiceCollection collection)
    {
        collection.AddSingleton( typeof( PdfPrinter ), PdfPrinterFactory );
        collection.AddSingleton( typeof( DocumentProcessor ), DocumentBuilderFactory );

        collection.AddSingleton( typeof( MainViewModel ), MainViewModelFactory );
        collection.AddSingleton( typeof( PersonChoosingViewModel ), PersonChoosingViewModelFactory );
        collection.AddSingleton( typeof( PersonSourceViewModel ), PersonSourceViewModelFactory );
        collection.AddSingleton( typeof( SceneViewModel ), SceneViewModelFactory );
        collection.AddSingleton( typeof( NavigationZoomViewModel ), NavigationZoomerViewModelFactory );
        collection.AddSingleton( typeof( PrintDialogViewModel ), PrintDialogViewModelFactory );
        collection.AddSingleton( typeof( EditorViewModelArgs ), EditorViewModelArgsFactory );
        collection.AddSingleton<BadgesBuildingViewModel>();
        collection.AddSingleton( typeof( WaitingViewModel ), WaitingViewModelFactory );
        collection.AddSingleton<LargeMessageViewModel>();
    }


    private static DocumentProcessor DocumentBuilderFactory(IServiceProvider serviceProvider)
    {
        return DocumentProcessor.GetInstance( TextWidthMeasurer.GetMesurer(), PdfCreator.GetInstance( ListerApp.OsName )
            , PdfPrinterImplementation.GetInstance( ListerApp.OsName, PdfCreator.GetInstance( ListerApp.OsName ) )
            , BadgeLayoutProvider.GetInstance(), PeopleSourceFactory.GetInstance() );
    }


    private static PdfPrinter PdfPrinterFactory(IServiceProvider serviceProvider)
    {
        DocumentProcessor model =
        DocumentProcessor.GetInstance( TextWidthMeasurer.GetMesurer(), PdfCreator.GetInstance( ListerApp.OsName )
            , PdfPrinterImplementation.GetInstance( ListerApp.OsName, PdfCreator.GetInstance( ListerApp.OsName ) )
            , BadgeLayoutProvider.GetInstance(), PeopleSourceFactory.GetInstance() );

        return new PdfPrinter( model );
    }


    private static MainViewModel MainViewModelFactory(IServiceProvider serviceProvider)
    {
        string suggestedName = MainViewConfigs.PdfFileName;
        string saveTitle = MainViewConfigs.SaveTitle;
        string incorrectXSLX = MainViewConfigs.IncorrectXSLX;
        string limitIsExhaustedMessage = MainViewConfigs.LimitIsExhaustedMessage;
        string fileIsOpenMessage = MainViewConfigs.FileIsOpenMessage;
        string fileIsTooBigMessage = MainViewConfigs.FileIsTooBig;

        return new MainViewModel( ListerApp.OsName, suggestedName, saveTitle, incorrectXSLX, limitIsExhaustedMessage
                                                                      , fileIsOpenMessage, fileIsTooBigMessage );
    }


    private static PersonSourceViewModel PersonSourceViewModelFactory(IServiceProvider serviceProvider)
    {
        string pickerTitle = PersonSourceConfigs.PickerTitle;
        string filePickerTitle = PersonSourceConfigs.FilePickerTitle;

        List<string> headers = PersonSourceConfigs.XlsxHeaderNames.ToList();
        List<string> patterns = PersonSourceConfigs.SourceExtentions.ToList();

        int badgeLimit = PersonSourceConfigs.BadgeCountLimit;

        string sourcePathKeeper =
        GetterFromJson.GetSectionStrValue( new List<string> { "personSource" }, ListerApp.ConfigPath, false );

        BadgeCreator badgesCreator = BadgeCreator.GetInstance( BadgeLayoutProvider.GetInstance()
                                                              , PeopleSourceFactory.GetInstance() );

        PersonSourceViewModel result =
        new PersonSourceViewModel( pickerTitle, filePickerTitle, patterns, headers, badgeLimit, sourcePathKeeper, badgesCreator );

        return result;
    }


    private static PersonChoosingViewModel PersonChoosingViewModelFactory(IServiceProvider serviceProvider)
    {
        string placeHolder = PersonChoosingConfigs.PlaceHolder;
        int inputLimit = PersonChoosingConfigs.InputLimit;

        SolidColorBrush incorrectTemplateColor = GetColor( PersonChoosingConfigs.IncorrectTemplateColor );

        SolidColorBrush defaultBackgroundColor = GetColor( PersonChoosingConfigs.DefaultBackgroundColor );
        SolidColorBrush defaultBorderColor = GetColor( PersonChoosingConfigs.DefaultBorderColor );
        SolidColorBrush defaultForegroundColor = GetColor( PersonChoosingConfigs.DefaultForegroundColor );

        List<SolidColorBrush> defaultColors = new List<SolidColorBrush>() { defaultBackgroundColor, defaultBorderColor
                                                                                                , defaultForegroundColor};

        SolidColorBrush focusedBackgroundColor = GetColor( PersonChoosingConfigs.FocusedBackgroundColor );
        SolidColorBrush focusedBorderColor = GetColor( PersonChoosingConfigs.FocusedBorderColor );

        List<SolidColorBrush> focusedColors = new List<SolidColorBrush>() { focusedBackgroundColor, focusedBorderColor };

        SolidColorBrush hoveredBackgroundColor = GetColor( PersonChoosingConfigs.HoveredBackgroundColor );
        SolidColorBrush hoveredBorderColor = GetColor( PersonChoosingConfigs.HoveredBorderColor );

        List<SolidColorBrush> hoveredColors = new List<SolidColorBrush>() { hoveredBackgroundColor, hoveredBorderColor };

        SolidColorBrush selectedBackgroundColor = GetColor( PersonChoosingConfigs.SelectedBackgroundColor );
        SolidColorBrush selectedBorderColor = GetColor( PersonChoosingConfigs.SelectedBorderColor );
        SolidColorBrush selectedForegroundColor = GetColor( PersonChoosingConfigs.SelectedForegroundColor );

        List<SolidColorBrush> selectedColors = new List<SolidColorBrush>() { selectedBackgroundColor, selectedBorderColor
                                                                                                  , selectedForegroundColor };

        PersonChoosingUserControl.SetComboboxHoveredItemColors( hoveredBackgroundColor, hoveredBorderColor );
        VisiblePerson.SetColors( defaultColors, focusedColors, selectedColors );

        BadgeCreator badgesCreator = BadgeCreator.GetInstance( BadgeLayoutProvider.GetInstance()
                                                              , PeopleSourceFactory.GetInstance() );

        return new PersonChoosingViewModel( placeHolder, inputLimit, incorrectTemplateColor
                                                  , defaultColors, focusedColors, selectedColors, badgesCreator );
    }


    private static SolidColorBrush GetColor(string hexColor)
    {
        Color color;

        if (Color.TryParse( hexColor, out color ))
        {
            return new SolidColorBrush( color );
        }


        return new SolidColorBrush( new Color( 255, 0, 0, 0 ) );
    }


    private static SceneViewModel SceneViewModelFactory(IServiceProvider serviceProvider)
    {
        int badgeLimit = SceneConfigs.BadgeCountLimit;
        string extentionToolTip = SceneConfigs.ExtentionToolTip;
        string shrinkingToolTip = SceneConfigs.ShrinkingToolTip;
        string fileIsOpenMessage = SceneConfigs.FileIsOpenMessage;

        return new SceneViewModel
            (
                badgeLimit, extentionToolTip, shrinkingToolTip, fileIsOpenMessage
                , DocumentProcessor.GetInstance
                (
                    TextWidthMeasurer.GetMesurer(), PdfCreator.GetInstance( ListerApp.OsName )
                    , PdfPrinterImplementation.GetInstance( ListerApp.OsName, PdfCreator.GetInstance( ListerApp.OsName ) )
                    , BadgeLayoutProvider.GetInstance(), PeopleSourceFactory.GetInstance()
                )
             );
    }


    private static NavigationZoomViewModel NavigationZoomerViewModelFactory(IServiceProvider serviceProvider)
    {
        string procentSymbol = NavigationZoomerConfigs.ProcentSymbol;

        short maxDepth = NavigationZoomerConfigs.MaxDepth;

        short minDepth = NavigationZoomerConfigs.MinDepth;

        return new NavigationZoomViewModel( procentSymbol, maxDepth, minDepth );
    }


    private static PrintDialogViewModel PrintDialogViewModelFactory(IServiceProvider serviceProvider)
    {
        string warnImagePath = PrintDialogConfigs.WarnImagePath;
        string emptyCopies = PrintDialogConfigs.EmptyCopies;
        string emptyPages = PrintDialogConfigs.EmptyPages;
        string emptyPrinters = PrintDialogConfigs.EmptyPrinters;

        return new PrintDialogViewModel( warnImagePath, emptyCopies, emptyPages, emptyPrinters, ListerApp.OsName );
    }


    private static EditorViewModelArgs EditorViewModelArgsFactory(IServiceProvider serviceProvider)
    {
        string extentionToolTip = EditionViewConfigs.ExtentionToolTip;
        string shrinkingToolTip = EditionViewConfigs.ShrinkingToolTip;
        string allFilter = EditionViewConfigs.AllFilter;
        string incorrectFilter = EditionViewConfigs.IncorrectFilter;

        string correctFilter = EditionViewConfigs.CorrectFilter;

        string allTip = EditionViewConfigs.AllTip;

        string correctTip = EditionViewConfigs.CorrectTip;

        string incorrectTip = EditionViewConfigs.IncorrectTip;

        SolidColorBrush focusedFontColor = GetColor( EditionViewConfigs.FocusedFontColor );
        SolidColorBrush releasedFontColor = GetColor( EditionViewConfigs.ReleasedFontColor );
        SolidColorBrush focusedFontBorderColor = GetColor( EditionViewConfigs.FocusedFontBorderColor );
        SolidColorBrush releasedFontBorderColor = GetColor( EditionViewConfigs.ReleasedFontBorderColor );

        EditorViewModelArgs result = new();

        result.ExtentionToolTip = extentionToolTip;
        result.ShrinkingToolTip = shrinkingToolTip;
        result.AllFilter = allFilter;
        result.CorrectFilter = correctFilter;
        result.IncorrectFilter = incorrectFilter;
        result.AllTip = allTip;
        result.CorrectTip = correctTip;
        result.IncorrectTip = incorrectTip;

        result.FocusedFontSizeColor = focusedFontColor;
        result.ReleasedFontSizeColor = releasedFontColor;
        result.FocusedFontSizeBorderColor = focusedFontBorderColor;
        result.ReleasedFontSizeBorderColor = releasedFontBorderColor;

        return result;
    }


    private static WaitingViewModel WaitingViewModelFactory(IServiceProvider serviceProvider)
    {
        return new WaitingViewModel( WaitingElementConfigs.GifName );
    }
}
