using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Lister.Desktop.Views.MainWindow.EditionView.Parts.Edition.ViewModel;

namespace Lister.Desktop.Views.MainWindow.EditionView.Parts.Edition;

public partial class EditionBlockUserControl : UserControl
{
    private static bool _someControlPressed;

    private EditionBlockViewModel _viewModel;
    private bool _isReleaseLocked;
    private bool _isTextEditorFocused;
    private bool _isZoomFocused;
    private string _processableText;

    public EditionBlockUserControl()
    {
        InitializeComponent();

        DataContextChanged += ( sender, args ) =>
        {
            _viewModel = DataContext as EditionBlockViewModel;
        };

        EditorTextBox.AddHandler ( TextBox.PointerReleasedEvent, PreventPasting, RoutingStrategies.Tunnel );


        //WeakReferenceMessenger.Default.Register<EditionBlockUserControl, CanceledChangesMessage> ( this, ( recipient, message ) =>
        //{
        //    ScalabilityGrade.Content = null;
        //    EditorTextBox.Text = null;
        //} );
    }

    //internal EditionBlockUserControl ( EditionBlockViewModel viewModel ) : this ()
    //{
    //    DataContext = _viewModel = viewModel;
    //    EditorTextBox.AddHandler ( TextBox.PointerReleasedEvent, PreventPasting, RoutingStrategies.Tunnel );
    //}

    private void PreventPasting ( object? sender, PointerReleasedEventArgs args )
    {
        args.Handled = true;
    }

    internal void ChangeFocusedFontSize ( object sender, KeyEventArgs args )
    {
        string key = args.Key.ToString ();

        if ( key == "Up" )
        {
            _viewModel?.IncreaseFontSize ();
        }
        else if ( key == "Down" )
        {
            _viewModel?.ReduceFontSize ();
        }
    }

    internal void ZoomGotFocus ( object sender, GotFocusEventArgs args )
    {
        _isReleaseLocked = true;
        _isZoomFocused = true;
    }


    internal void ZoomOnOutLostFocus ( object sender, RoutedEventArgs args )
    {
        _isZoomFocused = false;
    }

    internal void SomeButtonPressed ( object sender, PointerPressedEventArgs args )
    {
        _someControlPressed = true;
    }

    internal void HandleGettingFocus ( object sender, GotFocusEventArgs args )
    {
        _isReleaseLocked = true;
        _isTextEditorFocused = true;
    }

    internal void TextEditorLostFocus ( object sender, RoutedEventArgs args )
    {
        _isTextEditorFocused = false;
    }

    //internal void HandleTextEdition ( object sender, TextChangedEventArgs args )
    //{
    //    string? editable = EditorTextBox.Text;

    //    if ( editable == null )
    //    {
    //        return;
    //    }



    //    if ( editable.Length > _processableTextLengthLimit )
    //    {
    //        EditorTextBox.Text = editable [.._processableTextLengthLimit];

    //        return;
    //    }

    //    _viewModel?.ResetFocusedText ( editable );
    //}
}