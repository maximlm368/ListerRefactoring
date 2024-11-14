using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Lister.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Lister.Views
{
    public partial class PersonSourceUserControl : UserControl
    {
        private readonly PersonSourceViewModel _viewModel;


        public PersonSourceUserControl ()
        {
            InitializeComponent ();
            _viewModel = App.services.GetRequiredService<PersonSourceViewModel> ();
            DataContext = _viewModel;

            _viewModel.PassView (this);
        }


        internal void SetPath ( string ? path )
        {
            _viewModel.TrySetPath ( this.GetType(), path);
        }
    }
}
