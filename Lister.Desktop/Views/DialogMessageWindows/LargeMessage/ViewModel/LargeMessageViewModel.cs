using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Lister.Desktop.Views.DialogMessageWindows.LargeMessage.ViewModel;

public sealed partial class LargeMessageViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _message;

    [ObservableProperty]
    private string? _errorsSource;

    public event Action? Closing;

    [RelayCommand]
    internal void Close ()
    {
        Closing?.Invoke ();
    }

    internal void Set ( List<string> message, string errorsSource )
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
