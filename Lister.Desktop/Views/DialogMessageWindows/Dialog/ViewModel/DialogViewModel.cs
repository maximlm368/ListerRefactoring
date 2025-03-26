using ReactiveUI;

namespace Lister.Desktop.Views.DialogMessageWindows.Dialog.ViewModel;

public sealed class DialogViewModel : ReactiveObject
{
    private DialogWindow _view;


    public DialogViewModel(DialogWindow view)
    {
        _view = view;
    }


    internal void ChooseYes()
    {
        _view.ChooseYes();
    }


    internal void ChooseNo()
    {
        _view.ChooseNo();
    }
}
