using ReactiveUI;

namespace Lister.Desktop.Views.DialogMessageWindows.Dialog.ViewModel;

public sealed class DialogViewModel : ReactiveObject
{
    public delegate void YesHandler ();
    public event YesHandler? YesChosen;
    public delegate void NoHandler ();
    public event NoHandler? NoChosen;

    private string _question;
    internal string Question
    {
        get => _question;
        set
        {
            if ( value == null ) return;
            this.RaiseAndSetIfChanged ( ref _question, value, nameof ( Question ) );
        }
    }

    public DialogViewModel ( string question ) 
    {
        Question = question;
    }


    internal void ChooseYes()
    {
        YesChosen?.Invoke();
    }


    internal void ChooseNo()
    {
        NoChosen?.Invoke();
    }
}
