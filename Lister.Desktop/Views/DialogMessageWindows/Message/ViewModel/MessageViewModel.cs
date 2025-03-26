using ReactiveUI;

namespace Lister.Desktop.Views.DialogMessageWindows.Message.ViewModel;

public sealed class MessageViewModel : ReactiveObject
{
    private readonly int _lineHeight = 16;
    private int _topMargin = 54;
    private MessageDialog _view;

    private string _message;
    internal string Message
    {
        get { return _message; }
        set
        {
            this.RaiseAndSetIfChanged( ref _message, value, nameof( Message ) );
        }
    }


    internal void PassView(MessageDialog view)
    {
        _view = view;
    }


    internal void Close()
    {
        _view.Shut();
    }
}