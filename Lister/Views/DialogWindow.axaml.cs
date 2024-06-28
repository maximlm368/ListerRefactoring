using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Lister.ViewModels;
using ReactiveUI;

namespace Lister.Views
{
     partial class DialogWindow : ReactiveWindow <DialogViewModel>
    {
        public DialogWindow ()
        {
            InitializeComponent ();

            this.WhenActivated (action => action (ViewModel!.ChooseYes.Subscribe (Close)));
            this.WhenActivated (action => action (ViewModel!.ChooseNo.Subscribe (Close)));
        }
    }
}
