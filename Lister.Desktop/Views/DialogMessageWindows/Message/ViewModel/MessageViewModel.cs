using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Lister.Desktop.Views.DialogMessageWindows.Message.ViewModel;

public sealed partial class MessageViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _message;

    public delegate void ClosedHandler ( );
    public event ClosedHandler? Closed;

    public MessageViewModel ( string message ) 
    {
        Message = message;
    }

    [RelayCommand]
    internal void Close ()
    {
        Closed?.Invoke ();
    }
}
