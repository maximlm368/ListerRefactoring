using Avalonia.Media;
using Lister.Core.BadgesCreator;
using Lister.Core.Document;
using Lister.Desktop.App.Configs;
using Lister.Desktop.ExecutersForCoreAbstractions.BadgeCreator;
using Lister.Desktop.ExecutersForCoreAbstractions.DocumentProcessor;
using Lister.Desktop.ExecutersForCoreAbstractions.PeopleAccess;
using Lister.Desktop.ModelMappings.BadgeVM;
using Lister.Desktop.Views.DialogMessageWindows.LargeMessage.ViewModel;
using Lister.Desktop.Views.DialogMessageWindows.PrintDialog.ViewModel;
using Lister.Desktop.Views.MainWindow.EditionView.ViewModel;
using Lister.Desktop.Views.MainWindow.MainView.Parts.BuildButton.ViewModel;
using Lister.Desktop.Views.MainWindow.MainView.Parts.NavigationZoom.ViewModel;
using Lister.Desktop.Views.MainWindow.MainView.Parts.PersonChoosing;
using Lister.Desktop.Views.MainWindow.MainView.Parts.PersonChoosing.ViewModel;
using Lister.Desktop.Views.MainWindow.MainView.Parts.PersonSource.ViewModel;
using Lister.Desktop.Views.MainWindow.MainView.Parts.Scene;
using Lister.Desktop.Views.MainWindow.MainView.Parts.Scene.ViewModel;
using Lister.Desktop.Views.MainWindow.MainView.ViewModel;
using Lister.Desktop.Views.MainWindow.WaitingView.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace Lister.Desktop.App;

public static class ServiceCollectionExtensions
{
    private static string _configPath = string.Empty;
    private static string _osName = string.Empty;

    public static void AddNeededServices ( this IServiceCollection collection, string configPath, string osName )
    {
        _configPath = configPath;
        _osName = osName;

        collection.AddSingleton ( typeof ( Printer ), PdfPrinterFactory );
        collection.AddSingleton ( typeof ( DocumentProcessor ), DocumentProcessorFactory );
        collection.AddSingleton ( typeof ( MainViewModel ), MainViewModelFactory );
        collection.AddSingleton ( typeof ( PersonChoosingViewModel ), PersonChoosingViewModelFactory );
        collection.AddSingleton ( typeof ( PersonSourceViewModel ), PersonSourceViewModelFactory );
        collection.AddSingleton ( typeof ( SceneViewModel ), SceneViewModelFactory );
        collection.AddSingleton ( typeof ( NavigationZoomViewModel ), NavigationZoomerViewModelFactory );
        collection.AddSingleton ( typeof ( PrintDialogViewModel ), PrintDialogViewModelFactory );
        collection.AddSingleton ( typeof ( EditorViewModelArgs ), EditorViewModelArgsFactory );
        collection.AddSingleton<BadgesBuildingViewModel> ();
        collection.AddSingleton ( typeof ( WaitingViewModel ), WaitingViewModelFactory );
        collection.AddSingleton<LargeMessageViewModel> ();
    }

    private static DocumentProcessor DocumentProcessorFactory ( IServiceProvider serviceProvider )
    {
        return DocumentProcessor.GetInstance ( TextWidthMeasurer.Instance, PdfCreator.GetInstance ( _osName )
            , PdfPrinterImplementation.GetInstance ( _osName )
            , BadgeLayoutProvider.GetInstance (), PeopleSourceFactory.GetInstance () );
    }

    private static Printer PdfPrinterFactory ( IServiceProvider serviceProvider )
    {
        DocumentProcessor model = DocumentProcessor.GetInstance ( TextWidthMeasurer.Instance, PdfCreator.GetInstance ( _osName ),
            PdfPrinterImplementation.GetInstance ( _osName ), BadgeLayoutProvider.GetInstance (), PeopleSourceFactory.GetInstance ()
        );

        return new Printer ( model );
    }

    private static MainViewModel MainViewModelFactory ( IServiceProvider serviceProvider )
    {
        MainViewModelArgs args = new ()
        {
            OsName = _osName,
            SuggestedFileNames = MainViewConfigs.PdfFileName,
            SaveTitle = MainViewConfigs.SaveTitle,
            IncorrectXSLX = MainViewConfigs.IncorrectXSLX,
            BuildingLimitExhaustedMessage = MainViewConfigs.LimitIsExhaustedMessage,
            FileIsOpenMessage = MainViewConfigs.FileIsOpenMessage,
            FileIsTooBigMessage = MainViewConfigs.FileIsTooBig,
            Printer = serviceProvider.GetRequiredService<Printer> (),
            PrintDialogViewModel = serviceProvider.GetRequiredService<PrintDialogViewModel> (),
            PersonChoosingViewModel = serviceProvider.GetRequiredService<PersonChoosingViewModel> (),
            PersonSourceViewModel = serviceProvider.GetRequiredService<PersonSourceViewModel> (),
            BadgesBuildingViewModel = serviceProvider.GetRequiredService<BadgesBuildingViewModel> (),
            NavigationZoomViewModel = serviceProvider.GetRequiredService<NavigationZoomViewModel> (),
            SceneViewModel = serviceProvider.GetRequiredService<SceneViewModel> (),
            WaitingViewModel = serviceProvider.GetRequiredService<WaitingViewModel> ()
        };

        return new ( args );
    }

    private static PersonSourceViewModel PersonSourceViewModelFactory ( IServiceProvider serviceProvider )
    {
        string pickerTitle = PersonSourceConfigs.PickerTitle;
        string filePickerTitle = PersonSourceConfigs.FilePickerTitle;

        List<string> headers = [.. PersonSourceConfigs.XlsxHeaderNames];
        List<string> patterns = [.. PersonSourceConfigs.SourceExtentions];

        int badgeLimit = PersonSourceConfigs.BadgeCountLimit;
        string sourcePathKeeper = JsonProcessor.GetSectionStrValue ( ["personSource"], _configPath, false );
        BadgeCreator badgesCreator = BadgeCreator.GetInstance ( BadgeLayoutProvider.GetInstance (), PeopleSourceFactory.GetInstance () );

        return new ( pickerTitle, filePickerTitle, patterns, headers, badgeLimit, sourcePathKeeper, badgesCreator, _configPath );
    }

    private static PersonChoosingViewModel PersonChoosingViewModelFactory ( IServiceProvider serviceProvider )
    {
        string placeHolder = PersonChoosingConfigs.PlaceHolder;
        int inputLimit = PersonChoosingConfigs.InputLimit;

        SolidColorBrush incorrectTemplateColor = GetColor ( PersonChoosingConfigs.IncorrectTemplateColor );

        SolidColorBrush defaultBackgroundColor = GetColor ( PersonChoosingConfigs.DefaultBackgroundColor );
        SolidColorBrush defaultBorderColor = GetColor ( PersonChoosingConfigs.DefaultBorderColor );
        SolidColorBrush defaultForegroundColor = GetColor ( PersonChoosingConfigs.DefaultForegroundColor );

        List<SolidColorBrush> defaultColors = [defaultBackgroundColor, defaultBorderColor, defaultForegroundColor];

        SolidColorBrush focusedBackgroundColor = GetColor ( PersonChoosingConfigs.FocusedBackgroundColor );
        SolidColorBrush focusedBorderColor = GetColor ( PersonChoosingConfigs.FocusedBorderColor );

        List<SolidColorBrush> focusedColors = [focusedBackgroundColor, focusedBorderColor];

        SolidColorBrush hoveredBackgroundColor = GetColor ( PersonChoosingConfigs.HoveredBackgroundColor );
        SolidColorBrush hoveredBorderColor = GetColor ( PersonChoosingConfigs.HoveredBorderColor );

        SolidColorBrush selectedBackgroundColor = GetColor ( PersonChoosingConfigs.SelectedBackgroundColor );
        SolidColorBrush selectedBorderColor = GetColor ( PersonChoosingConfigs.SelectedBorderColor );
        SolidColorBrush selectedForegroundColor = GetColor ( PersonChoosingConfigs.SelectedForegroundColor );

        List<SolidColorBrush> selectedColors = [selectedBackgroundColor, selectedBorderColor, selectedForegroundColor];

        PersonChoosingUserControl.SetComboboxHoveredItemColors ( hoveredBackgroundColor, hoveredBorderColor );
        VisiblePerson.SetColors ( defaultColors, focusedColors, selectedColors );

        BadgeCreator badgesCreator = BadgeCreator.GetInstance ( BadgeLayoutProvider.GetInstance (), PeopleSourceFactory.GetInstance () );

        return new ( placeHolder, inputLimit, incorrectTemplateColor, defaultColors, focusedColors, selectedColors, badgesCreator );
    }

    private static SolidColorBrush GetColor ( string hexColor )
    {
        if ( Color.TryParse ( hexColor, out Color color ) )
        {
            return new SolidColorBrush ( color );
        }

        return new SolidColorBrush ( new Color ( 255, 0, 0, 0 ) );
    }

    private static SceneViewModel SceneViewModelFactory ( IServiceProvider serviceProvider )
    {
        return new SceneViewModel (
            SceneConfigs.BadgeCountLimit,
            SceneConfigs.ExtentionToolTip,
            SceneConfigs.ShrinkingToolTip, 
            DocumentProcessor.GetInstance (
                TextWidthMeasurer.Instance,
                PdfCreator.GetInstance ( _osName ),
                PdfPrinterImplementation.GetInstance ( _osName ),
                BadgeLayoutProvider.GetInstance (),
                PeopleSourceFactory.GetInstance ()
            )
        );
    }

    private static NavigationZoomViewModel NavigationZoomerViewModelFactory ( IServiceProvider serviceProvider )
    {
        return new NavigationZoomViewModel ( 
            NavigationZoomerConfigs.ProcentSymbol,
            NavigationZoomerConfigs.MaxDepth,
            NavigationZoomerConfigs.MinDepth
        );
    }

    private static PrintDialogViewModel PrintDialogViewModelFactory ( IServiceProvider serviceProvider )
    {
        return new PrintDialogViewModel (
            PrintDialogConfigs.EmptyCopies,
            PrintDialogConfigs.EmptyPages,
            PrintDialogConfigs.EmptyPrinters,
            _osName
        );
    }

    private static EditorViewModelArgs EditorViewModelArgsFactory ( IServiceProvider serviceProvider )
    {
        string extentionToolTip = EditionViewConfigs.ExtentionToolTip;
        string shrinkingToolTip = EditionViewConfigs.ShrinkingToolTip;
        string allFilter = EditionViewConfigs.AllFilter;
        string incorrectFilter = EditionViewConfigs.IncorrectFilter;
        string correctFilter = EditionViewConfigs.CorrectFilter;
        string allTip = EditionViewConfigs.AllTip;
        string correctTip = EditionViewConfigs.CorrectTip;
        string incorrectTip = EditionViewConfigs.IncorrectTip;

        EditorViewModelArgs result = new ()
        {
            ExtentionToolTip = extentionToolTip,
            ShrinkingToolTip = shrinkingToolTip,
            AllFilter = allFilter,
            CorrectFilter = correctFilter,
            IncorrectFilter = incorrectFilter,
            AllTip = allTip,
            CorrectTip = correctTip,
            IncorrectTip = incorrectTip,

            FocusedFontSizeColor = GetColor ( EditionViewConfigs.FocusedFontColor ),
            ReleasedFontSizeColor = GetColor ( EditionViewConfigs.ReleasedFontColor ),
            FocusedFontSizeBorderColor = GetColor ( EditionViewConfigs.FocusedFontBorderColor ),
            ReleasedFontSizeBorderColor = GetColor ( EditionViewConfigs.ReleasedFontBorderColor )
        };

        return result;
    }

    private static WaitingViewModel WaitingViewModelFactory ( IServiceProvider serviceProvider )
    {
        return new WaitingViewModel ( WaitingElementConfigs.GifPath );
    }
}
