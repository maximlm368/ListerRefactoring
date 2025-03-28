using ReactiveUI;

namespace Lister.Desktop.Views.DialogMessageWindows.Message.ViewModel;

public sealed class MessageViewModel : ReactiveObject
{
    private string _message;
    internal string Message
    {
        get => _message;
        set
        {
            if ( value == null ) return;
            this.RaiseAndSetIfChanged ( ref _message, value, nameof ( Message ) );
        }
    }

    public delegate void ClosedHandler ( );
    public event ClosedHandler? Closed;


    public MessageViewModel ( string message ) 
    {
        Message = message;
    }


    internal void Close ()
    {
        Closed?.Invoke ();
    }
}