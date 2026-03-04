using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Lister.Core.BadgesCreator;
using Lister.Core.Document;
using Lister.Desktop.App.Configs;
using Lister.Desktop.ExecutersForCoreAbstractions.BadgeCreator;
using Lister.Desktop.ExecutersForCoreAbstractions.DocumentProcessor;
using Lister.Desktop.ExecutersForCoreAbstractions.PeopleAccess;
using Lister.Desktop.Views.DialogMessageWindows.PrintDialog.ViewModel;
using Lister.Desktop.Views.MainWindow;
using Lister.Desktop.Views.MainWindow.MainView;
using Lister.Desktop.Views.MainWindow.MainView.Parts.PersonChoosing.ViewModel;
using Lister.Desktop.Views.MainWindow.MainView.Parts.PersonSource.ViewModel;
using Lister.Desktop.Views.MainWindow.MainView.Parts.Scene;
using Lister.Desktop.Views.MainWindow.MainView.Parts.Scene.ViewModel;
using Lister.Desktop.Views.MainWindow.MainView.ViewModel;
using Lister.Desktop.Views.MainWindow.WaitingView.ViewModel;
using Lister.Desktop.Views.SplashWindow;
using Microsoft.Extensions.DependencyInjection;

namespace Lister.Desktop.App;

public partial class ListerApp : Avalonia.Application
{
    private static string _configPath = string.Empty;
    private static string _osName = string.Empty;

#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Рассмотрите возможность добавления модификатора "required" или объявления значения, допускающего значение NULL.
    public static ServiceProvider Services { get; private set; }
#pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Рассмотрите возможность добавления модификатора "required" или объявления значения, допускающего значение NULL.

    public override void Initialize ()
    {
        AvaloniaXamlLoader.Load ( this );
    }

    public override async void OnFrameworkInitializationCompleted ()
    {
        string resourceDirectory = Environment.CurrentDirectory;

#if DEBUG
        resourceDirectory = resourceDirectory [..^17];
#endif

        string jsonSchemePath = "avares://Lister.Desktop/Assets/JsonSchema/Schema.json";
        string resourcePath = Path.Combine ( resourceDirectory, "Resources" );
        string configPath = Path.Combine ( resourcePath, "Config.json" );

        ServiceCollection collection = new ();
        _configPath = configPath;
        _osName = OperatingSystem.IsWindows () ? "Windows" : "Linux";

        if ( ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop )
        {
            return;
        }

        SplashWindow splashWindow = new ();
        desktop.MainWindow = splashWindow;
        splashWindow.Show ();

        BadgeLayoutProvider layoutProvider = BadgeLayoutProvider.GetInstance ();
        await layoutProvider.SetUp ( resourcePath, jsonSchemePath, _osName );

        MainViewUserControl mainView = new ( GetMainViewModel () );

        MainWindow mainWindow = new ()
        {
            Content = mainView,
            PrintDialogViewModelGenerator = GetPrintDialog
        };

        desktop.MainWindow = mainWindow;
        desktop.MainWindow.Show ();
        splashWindow.Close ();

        mainWindow.Closing += ( s, e ) => e.Cancel = MainViewModel.HasWaitingState;

        base.OnFrameworkInitializationCompleted ();
    }

    private static MainViewModel GetMainViewModel ()
    {
        MainViewModelArgs args = new (
            GetPdfPrinter (),
            GetPrintDialog (),
            GetPersonChoosing (),
            GetPersonSource (),
            GetScene (),
            GetWaiting ()
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

    private static DocumentOutsider GetPdfPrinter ()
    {
        DocumentProcessor model = DocumentProcessor.GetInstance ( TextWidthMeasurer.Instance, PdfCreator.GetInstance ( _osName ),
            Printer.GetInstance ( _osName ),
            BadgeLayoutProvider.GetInstance (),
            PeopleSourceFactory.GetInstance ()
        );

        return new DocumentOutsider ( model );
    }

    private static PrintDialogViewModel GetPrintDialog ()
    {
        return new PrintDialogViewModel ( _osName );
    }

    //private static DocumentProcessor GetDocumentProcessor ()
    //{
    //    return DocumentProcessor.GetInstance ( TextWidthMeasurer.Instance, PdfCreator.GetInstance ( _osName ),
    //        PdfPrinter.GetInstance ( _osName ),
    //        BadgeLayoutProvider.GetInstance (),
    //        PeopleSourceFactory.GetInstance ()
    //    );
    //}

    private static PersonSourceViewModel GetPersonSource ()
    {
        string pickerTitle = PersonSourceConfigs.PickerTitle;
        string filePickerTitle = PersonSourceConfigs.FilePickerTitle;

        List<string> headers = [.. PersonSourceConfigs.XlsxHeaders];
        List<string> patterns = [.. PersonSourceConfigs.SourceExtentions];

        int badgeLimit = PersonSourceConfigs.BadgeCountLimit;
        string sourcePathKeeper = JsonProcessor.GetSectionStrValue ( ["personSource"], _configPath, false );
        BadgeCreator badgesCreator = BadgeCreator.GetInstance ( BadgeLayoutProvider.GetInstance (), PeopleSourceFactory.GetInstance () );

        return new ( pickerTitle, filePickerTitle, patterns, headers, badgeLimit, sourcePathKeeper, badgesCreator, _configPath );
    }

    private static PersonChoosingViewModel GetPersonChoosing ()
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

    private static SceneViewModel GetScene ()
    {
        return new SceneViewModel (
            SceneConfigs.BadgeCountLimit,
            DocumentProcessor.GetInstance (
                TextWidthMeasurer.Instance,
                PdfCreator.GetInstance ( _osName ),
                Printer.GetInstance ( _osName ),
                BadgeLayoutProvider.GetInstance (),
                PeopleSourceFactory.GetInstance ()
            )
        );
    }

    private static WaitingViewModel GetWaiting ()
    {
        return new WaitingViewModel ( WaitingElementConfigs.GifPath );
    }
}
