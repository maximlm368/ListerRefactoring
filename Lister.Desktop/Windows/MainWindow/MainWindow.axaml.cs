using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Lister.Desktop.Components.ButtonsBlock.ViewModel;
using Lister.Desktop.Entities.BadgeVM;
using Lister.Desktop.Views.DialogMessageWindows.Dialog;
using Lister.Desktop.Views.DialogMessageWindows.LargeMessage;
using Lister.Desktop.Views.DialogMessageWindows.Message;
using Lister.Desktop.Views.DialogMessageWindows.PrintDialog;
using Lister.Desktop.Views.DialogMessageWindows.PrintDialog.ViewModel;
using Lister.Desktop.Views.EditionView;
using Lister.Desktop.Views.EditionView.ViewModel;
using Lister.Desktop.Views.MainView;
using Lister.Desktop.Views.MainView.ViewModel;
using Lister.Desktop.Windows.DialogMessageWindows.PrintDialog;

namespace Lister.Desktop.Views.MainWindow;

public sealed partial class MainWindow : Window
{
    private static readonly int _onScreenRestriction = 50;
    private static PixelPoint _pointerPosition;
    private static readonly string _backingQuestion = "Сохранить изменения и вернуться к макету?";

    internal static IStorageProvider? CommonStorageProvider { get; private set; }
    internal static MainWindow? Window { get; private set; }
    internal static double HeightfDifference { get; private set; }
    internal static int TappedGoToEditorButton { get; private set; }

    private List<BadgeViewModel>? _processableBadges;
    private double _currentWidth;
    private double _currentHeight;

    internal MainViewUserControl? MainView {  get; set; }
    internal EditorViewUserControl? EditorView { get; set; }

    internal Window? ModalWindow { get; set; }
    internal Func<PrintDialogViewModel>? PrintDialogViewModelGenerator { get; set; }
    internal double WidthDifference { get; private set; }
    internal double HeightDifference { get; private set; }


    public MainWindow ( )
    {
        InitializeComponent();

        CommonStorageProvider = StorageProvider;
        Window = this;
        _currentWidth = Width;
        _currentHeight = Height;
        Cursor = new Cursor (StandardCursorType.Arrow);
        CanResize = true;
        _pointerPosition = Position;

        SizeChanged += OnSizeChanged;
        PointerReleased += ReleaseCaptured;
        PositionChanged += HandlePositionChange;
        PointerMoved += OnPointerMoved;
        LayoutUpdated += OnLayoutUpdated;
        Loaded += OnLoaded;

        EditorViewModel.BackingComplated += ComplateBacking;
        EditorViewModel.BackingActivated += HandleBackingActivated;

        ButtonsBlockViewModel.ToEditionRequired += ToEdition;

        MainViewModel.HasToShowMessage += ShowMessageWindow;
        MainViewModel.HasToShowTemplateErrors += ShowTemplateErrors;
        MainViewModel.HasToPreparePrinting += ShowPrintDialog;
        MainViewModel.FilePickerRequired += ShowFilePicker;
    }

    private void ShowMessageWindow ( string message )
    {
        if ( Content is not MainViewUserControl mainView || mainView.ViewModel == null )
        {
            return;
        }

        MessageWindow messageWindow = new ( message );
        ModalWindow = messageWindow;
        messageWindow.Closed += ( s, a ) => { mainView.ViewModel?.HandleDialogClosing (); };
        messageWindow.ShowDialog ( this );
    }

    private void ShowTemplateErrors ( List<string> message, string errorSource )
    {
        if ( Content is not MainViewUserControl mainView || mainView.ViewModel == null )
        {
            return;
        }

        LargeMessageDialog messegeDialog = new ( message, errorSource );

        messegeDialog.Closed += ( s, a ) => 
        { 
            mainView.ViewModel?.HandleDialogClosing (); 
        };

        messegeDialog.ShowDialog ( this );
        messegeDialog.Focusable = true;
        messegeDialog.Focus ();
    }

    private void ShowPrintDialog ( int pageCount, PrintAdjustingData printAdjusting )
    {
        PrintDialogViewModel? dialogViewModel = PrintDialogViewModelGenerator?.Invoke ();

        if ( dialogViewModel == null ) 
        {
            return;
        }

        PrintDialogWindow printDialog = new ( pageCount, printAdjusting, dialogViewModel );
        printDialog.ShowDialog ( this );

        printDialog.Closed += ( sender, args ) => 
        {
            if ( Content is not MainViewUserControl mainView || mainView.ViewModel == null )
            {
                return;
            }

            mainView.ViewModel?.HandlePrintPreparatinComplated ();
        };
    }

    private void ToEdition ( )
    {
        if ( Content is not MainViewUserControl mainView || mainView.ViewModel == null )
        {
            return;
        }

        _processableBadges = mainView.ViewModel.Scene.ProcessableBadges;

        if ( _processableBadges.Count > 0 )
        {
            EditorView = new EditorViewUserControl ();
            EditorView.SetProperSize ( Width, Height );
            CancelSizeDifference ();
            TappedGoToEditorButton = 1;
            ( MainView?.DataContext as MainViewModel )?.Wait ();
        }
    }

    private async Task<IStorageFile?>? ShowFilePicker ( FilePickerSaveOptions options ) 
    {
        if ( CommonStorageProvider == null )
        {
            return null;
        }

        IStorageFile? chosenFile = await CommonStorageProvider.SaveFilePickerAsync ( options );

        return chosenFile;
    }

    private void OnLoaded ( object? sender, RoutedEventArgs args )
    {
        MainView = Content as MainViewUserControl;
    }

    private void OnLayoutUpdated ( object? sender, EventArgs args )
    {
        if ( !IsLoaded )
        {
            return;
        }

        if ( TappedGoToEditorButton == 1 )
        {
            SwitchToEditor ();
        }
    }

    private void SwitchToEditor ()
    {
        if ( EditorView == null )
        {
            return;
        }

        TappedGoToEditorButton = 0;

        Task task = new
        (
            () =>
            {
                if ( _processableBadges != null && _processableBadges.Count > 0 )
                {
                    EditorView.PrepareBy ( _processableBadges );
                }

                Dispatcher.UIThread.Invoke
                (
                    () =>
                    {
                        MainViewModel? mainViewModel = MainView?.DataContext as MainViewModel;
                        mainViewModel?.EndWaiting ();
                        Content = EditorView;
                    }
                );
            }
        );

        task.Start ();
    }

    private void ComplateBacking ()
    {
        MainView?.SetProperSize ( Width, Height );
        CancelSizeDifference ();
        MainView?.Show ();
        Content = MainView;
    }

    private async void HandleBackingActivated ( Action dialogClosedHandler, Action goBack )
    {
        DialogWindow dialog = new ( _backingQuestion );
        dialog.Closed += ( s, a ) => dialogClosedHandler ();

        bool result = await dialog.ShowDialog<bool> ( this );

        if ( result )
        {
            goBack ();
        }
    }

    private void OnSizeChanged ( object? sender, SizeChangedEventArgs args )
    {
        if ( Content is MainViewUserControl mainView )
        {
            double newWidth = args.NewSize.Width;
            double newHeight = args.NewSize.Height;
            double newWidthDifference = _currentWidth - newWidth;
            double newHeightDifference = _currentHeight - newHeight;
            WidthDifference += newWidthDifference;
            HeightDifference = newHeightDifference;
            _currentWidth = newWidth;
            _currentHeight = newHeight;
            mainView.ChangeSize ( newWidthDifference, newHeightDifference );

            return;
        }

        if ( Content is EditorViewUserControl editionView )
        {
            double newWidth = args.NewSize.Width;
            double newHeight = args.NewSize.Height;
            double widthDifference = _currentWidth - newWidth;
            double heightDifference = _currentHeight - newHeight;
            WidthDifference += widthDifference;
            HeightDifference = heightDifference;
            _currentWidth = newWidth;
            _currentHeight = newHeight;
            editionView.ChangeSize ( widthDifference, heightDifference );
        }
    }

    internal void CancelSizeDifference ( )
    {
        WidthDifference = 0;
    }

    internal void ReleaseCaptured ( object? sender, PointerReleasedEventArgs args )
    {
        if ( Content is MainViewUserControl mainView )
        {
            mainView.ReleaseCaptured ();
        }
        else
        {
            if ( Content is EditorViewUserControl editorView )
            {
                editorView.ReleaseCaptured ();
            }
        }
    }

    internal void OnPointerMoved ( object? sender, PointerEventArgs args )
    {
        if ( Content is MainViewUserControl mainView )
        {
            mainView.MovePage ( args );
        }
        else
        {
            if ( Content is EditorViewUserControl editorView )
            {
                editorView.MoveBadge ( args );
            }
        }
    }

    internal void HandlePositionChange ( object? sender, PixelPointEventArgs args )
    {
        RestrictPosition (sender, args);
        HoldDialogIfExistsOnLinux (sender, args);
        _pointerPosition = Position;
    }

    private void RestrictPosition ( object? sender, PixelPointEventArgs args )
    {
        PixelPoint currentPosition = this.Position;
        Screens screens = Screens;
        Screen screen = screens.All [0];
        int screenHeight = screen.WorkingArea.Height;

        if ( currentPosition.Y > ( screenHeight - _onScreenRestriction ) )
        {
            this.Position = new PixelPoint (currentPosition.X, ( screenHeight - _onScreenRestriction ));
        }
    }

    private void HoldDialogIfExistsOnLinux ( object? sender, PixelPointEventArgs args )
    {
        if ( ModalWindow == null ) 
        {
            return;
        }

        PixelPoint delta = Position - _pointerPosition;
        ModalWindow.Position += delta;
    }
}
