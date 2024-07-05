using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Lister.ViewModels;
using ReactiveUI;

namespace Lister.Views
{
    internal partial class DialogWindow : ReactiveWindow <DialogViewModel>
    {
        internal DialogWindow ()
        {
            InitializeComponent ();

            this.WhenActivated (action => action (ViewModel!.ChooseYes.Subscribe (Close)));
            this.WhenActivated (action => action (ViewModel!.ChooseNo.Subscribe (Close)));
        }
    }
}
