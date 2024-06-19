using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Lister.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Lister.Views
{
    public partial class PersonSourceUserControl : UserControl
    {
        private PersonSourceViewModel _vm;
       

        public PersonSourceUserControl ()
        {
            InitializeComponent ();
            DataContext = App.services.GetRequiredService<PersonSourceViewModel> ();
            _vm = (PersonSourceViewModel) DataContext;
            var window = TopLevel.GetTopLevel (this);
            _vm.PassView (this);
        }
    }
}
