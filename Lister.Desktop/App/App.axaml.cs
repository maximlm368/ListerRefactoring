using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Lister.Core.Document;
using Lister.Desktop.App.Configs;
using Lister.Desktop.ExecutersForCoreAbstractions.BadgeCreator;
using Lister.Desktop.ExecutersForCoreAbstractions.DocumentProcessor;
using Lister.Desktop.ExecutersForCoreAbstractions.PeopleAccess;
using Lister.Desktop.Infrastructure;
using Lister.Desktop.Services;
using Lister.Desktop.Views.DialogMessageWindows.PrintDialog.ViewModel;
using Lister.Desktop.Views.MainView;
using Lister.Desktop.Views.MainView.Parts.PersonChoosing.ViewModel;
using Lister.Desktop.Views.MainView.Parts.PersonSource.ViewModel;
using Lister.Desktop.Views.MainView.Parts.Scene.ViewModel;
using Lister.Desktop.Views.MainView.ViewModel;
using Lister.Desktop.Views.MainWindow;
using Lister.Desktop.Views.WaitingView.ViewModel;
using Lister.Desktop.Views.SplashWindow;
using Microsoft.Extensions.DependencyInjection;

namespace Lister.Desktop.App;

public partial class ListerApp : Avalonia.Application
{
    private static string _configPath = string.Empty;
    private static string _osName = string.Empty;
    private static DocumentProcessor _documentProcessor = DocumentProcessor.GetInstance ( TextWidthMeasurer.Instance, 
        BadgeLayoutProvider.GetInstance (),
        PeopleSourceFactory.GetInstance ()
    );

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
        MainViewModelArgs args = new ( new PrintingManager ( PdfCreator.GetInstance ( _osName ), Printer.GetInstance ( _osName ) ),
            GetPersonChoosing (),
            GetPersonSource (),
            new SceneViewModel ( PersonSourceConfigs.BadgeCountLimit, _documentProcessor ),
            new WaitingViewModel ( WaitingElementConfigs.GifPath )
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

    private static PrintDialogViewModel GetPrintDialog ()
    {
        return new PrintDialogViewModel ( _osName );
    }

    private static PersonSourceViewModel GetPersonSource ()
    {
        string pickerTitle = PersonSourceConfigs.PickerTitle;
        string filePickerTitle = PersonSourceConfigs.FilePickerTitle;

        List<string> headers = [.. PersonSourceConfigs.XlsxHeaders];
        List<string> patterns = [.. PersonSourceConfigs.SourceExtentions];

        int badgeLimit = PersonSourceConfigs.BadgeCountLimit;
        string sourcePathKeeper = JsonProcessor.GetSectionStrValue ( ["personSource"], _configPath, false );

        return new ( pickerTitle, filePickerTitle, patterns, headers, badgeLimit, sourcePathKeeper, _documentProcessor, _configPath );
    }

    private static PersonChoosingViewModel GetPersonChoosing ()
    {
        SolidColorBrush incorrectTemplateForeground = GetColor ( PersonChoosingConfigs.IncorrectTemplateForeground );
        SolidColorBrush correctTemplateForeground = GetColor ( PersonChoosingConfigs.CorrectTemplateForeground );

        return new ( incorrectTemplateForeground, correctTemplateForeground, _documentProcessor );
    }

    private static SolidColorBrush GetColor ( string hexColor )
    {
        if ( Color.TryParse ( hexColor, out Color color ) )
        {
            return new SolidColorBrush ( color );
        }

        return new SolidColorBrush ( new Color ( 255, 0, 0, 0 ) );
    }
}
