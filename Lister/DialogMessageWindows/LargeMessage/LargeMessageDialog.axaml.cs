using Avalonia.Controls;
using Avalonia.Input;
using MessageBox.Avalonia.Views;
using View.DialogMessageWindows.LargeMessage.ViewModel;
using View.ViewBase;
using View.MainWindow;
using View.App;

namespace View.DialogMessageWindows.LargeMessage;

public partial class LargeMessageDialog : BaseWindow
{
    private LargeMessageViewModel _viewModel;
    public ShowingDialog Caller { get; private set; }

    private bool _messageIsSelected;
    private bool _pathIsSelected;
    private string _message;
    internal string Message 
    {
        get 
        {
            return _message; 
        }

        set 
        {
            if ( value != null )
            {
                string [] lines = value.Split (new char[]{'.'}, StringSplitOptions.RemoveEmptyEntries);

                if ( lines.Length == 1 ) 
                {
                    lines = [value];
                }

                List<string> pureLines = new List<string> ();

                foreach ( string line   in   lines ) 
                {
                    string pureLine = line.Trim ();
                    pureLines.Add ( pureLine );
                }

                _viewModel.MessageLines = pureLines;
                _message = value;
            }
        }
    }


    public LargeMessageDialog ()
    {
        InitializeComponent ();
    }


    public LargeMessageDialog ( ShowingDialog caller, List<string> errors, string errorsSource ) : this ()
    {
        Caller = caller;
        _viewModel = new LargeMessageViewModel ();
        DataContext = _viewModel;
        CanResize = false;
        _viewModel.PassView (this);

        CanResize = false;
        ok.FocusAdorner = null;
        textArea.FocusAdorner = null;
        
        Activated += delegate { ok.Focus (NavigationMethod.Tab, KeyModifiers.None); };

        MainWin mainWindow = ListerApp.MainWindow;
        mainWindow.ModalWindow = this;

        List<string> messageLines = null;
        _viewModel.Set (errors, errorsSource);
    }


    internal List<string> ConvertToLinesForStandard ( string message )
    {
        List<string> pureLines = new List<string> ();

        if ( message == null )
        {
            return pureLines;
        }

        string [] lines = message.Split (new char[]{'.'}, StringSplitOptions.RemoveEmptyEntries);

        if ( lines.Length == 1 )
        {
            lines = [message];
        }

        foreach ( string line   in   lines )
        {
            string pureLine = line.Trim ();
            pureLines.Add (pureLine);
        }

        return pureLines;
    }


    internal void Shut ()
    {
        Caller.HandleDialogClosing ();
        this.Close ();
    }


    internal void TappedForWholeSelection ( object sender, TappedEventArgs args )
    {
        if ( !_messageIsSelected )
        {
            _messageIsSelected = true;
            SelectableTextBlock textBlock = sender as SelectableTextBlock;
            textBlock.SelectAll ();
        }
        else
        {
            _messageIsSelected = false;
        }
    }


    internal void TappedPathForWholeSelection ( object sender, TappedEventArgs args )
    {
        if ( !_pathIsSelected )
        {
            _pathIsSelected = true;
            TextBox textBox = sender as TextBox;
            textBox.SelectAll ();
        }
        else
        {
            _pathIsSelected = false;
        }
    }


    internal void MessageGotFocus ( object sender, GotFocusEventArgs args )
    {
        SelectableTextBlock textBlock = sender as SelectableTextBlock;
        textBlock.SelectAll ();
    }
}
