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
        public PersonSourceUserControl ()
        {
            InitializeComponent ();
            PersonSourceViewModel viewModel = App.services.GetRequiredService<PersonSourceViewModel> ();
            DataContext = viewModel;

            viewModel.OnLoaded ();
        }
    }
}
