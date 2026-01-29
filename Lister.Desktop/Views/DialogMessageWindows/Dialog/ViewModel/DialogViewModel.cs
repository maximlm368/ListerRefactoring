using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Lister.Desktop.Views.DialogMessageWindows.Dialog.ViewModel;

public sealed partial class DialogViewModel : ObservableObject
{
    public delegate void YesHandler ();
    public event YesHandler? YesChosen;
    public delegate void NoHandler ();
    public event NoHandler? NoChosen;

    [ObservableProperty]
    private string? _question;

    public DialogViewModel ( string? question ) 
    {
        Question = question;
    }

    [RelayCommand]
    internal void Yes ()
    {
        YesChosen?.Invoke();
    }

    [RelayCommand]
    internal void No ()
    {
        NoChosen?.Invoke();
    }
}
