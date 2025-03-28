using ReactiveUI;

namespace Lister.Desktop.Views.DialogMessageWindows.LargeMessage.ViewModel;

public sealed class LargeMessageViewModel : ReactiveObject
{
    private string _message;
    internal string Message
    {
        get { return _message; }
        set
        {
            this.RaiseAndSetIfChanged( ref _message, value, nameof( Message ) );
        }
    }

    private string _errorsSource;
    internal string ErrorsSource
    {
        get { return _errorsSource; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _errorsSource, value, nameof( ErrorsSource ) );
        }
    }

    public delegate void ClosedHandler ();
    public event ClosedHandler? Closed;


    internal void Close()
    {
        Closed?.Invoke ();
    }


    internal void Set(List<string> message, string errorsSource)
    {
        ErrorsSource = errorsSource;
        Message = string.Empty;

        for ( int index = 0; index < message.Count; index++ )
        {
            Message += message [index] + "  ";

            if ( index != ( message.Count - 1 ) ) Message += "\n";
        }
    }
}