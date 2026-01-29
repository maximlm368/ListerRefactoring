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
        collection.AddSingleton ( typeof ( WaitingViewModel ), WaitingViewModelFactory );
        collection.AddSingleton<LargeMessageViewModel> ();
    }

    private static DocumentProcessor DocumentProcessorFactory ( IServiceProvider serviceProvider )
    {
        return DocumentProcessor.GetInstance ( TextWidthMeasurer.Instance, PdfCreator.GetInstance ( _osName ),
            PdfPrinter.GetInstance ( _osName ),
            BadgeLayoutProvider.GetInstance (), 
            PeopleSourceFactory.GetInstance () 
        );
    }

    private static Printer PdfPrinterFactory ( IServiceProvider serviceProvider )
    {
        DocumentProcessor model = DocumentProcessor.GetInstance ( TextWidthMeasurer.Instance, PdfCreator.GetInstance ( _osName ),
            PdfPrinter.GetInstance ( _osName ), 
            BadgeLayoutProvider.GetInstance (),
            PeopleSourceFactory.GetInstance ()
        );

        return new Printer ( model );
    }

    private static MainViewModel MainViewModelFactory ( IServiceProvider serviceProvider )
    {
        MainViewModelArgs args = new (
            serviceProvider.GetRequiredService<Printer> (),
            serviceProvider.GetRequiredService<PrintDialogViewModel> (),
            serviceProvider.GetRequiredService<PersonChoosingViewModel> (),
            serviceProvider.GetRequiredService<PersonSourceViewModel> (),
            serviceProvider.GetRequiredService<NavigationZoomViewModel> (),
            serviceProvider.GetRequiredService<SceneViewModel> (),
            serviceProvider.GetRequiredService<WaitingViewModel> ()
        )
        {
            OsName = _osName,
            SuggestedFileNames = MainViewConfigs.PdfFileName,
            SaveTitle = MainViewConfigs.SaveTitle,
            IncorrectXSLX = MainViewConfigs.IncorrectXSLX,
            BuildingLimitExhaustedMessage = MainViewConfigs.LimitIsExhaustedMessage,
            FileIsOpenMessage = MainViewConfigs.FileIsOpenMessage,
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
        SolidColorBrush incorrectTemplateForeground = GetColor ( PersonChoosingConfigs.IncorrectTemplateForeground );
        SolidColorBrush correctTemplateForeground = GetColor ( PersonChoosingConfigs.CorrectTemplateForeground );
        BadgeCreator badgesCreator = BadgeCreator.GetInstance ( BadgeLayoutProvider.GetInstance (), PeopleSourceFactory.GetInstance () );

        return new ( incorrectTemplateForeground, correctTemplateForeground, badgesCreator );
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
                PdfPrinter.GetInstance ( _osName ),
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
            PrintDialogConfigs.EmptyCopiesError,
            PrintDialogConfigs.EmptyPagesError,
            PrintDialogConfigs.EmptyPrintersError,
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
