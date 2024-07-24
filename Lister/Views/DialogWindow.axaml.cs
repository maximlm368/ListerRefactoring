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

            this.Icon = null;

            this.CanResize = false;

            this.ExtendClientAreaToDecorationsHint = false;

            this.WhenActivated (action => action (ViewModel!.ChooseYes.Subscribe (Close)));
            this.WhenActivated (action => action (ViewModel!.ChooseNo.Subscribe (Close)));

            yes.CornerRadius = new Avalonia.CornerRadius ();
            no.CornerRadius = new Avalonia.CornerRadius ();
        }
    }
}
