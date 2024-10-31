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
        private PersonSourceViewModel _vm;
        private string _theme;


        public PersonSourceUserControl ()
        {
            InitializeComponent ();
            DataContext = App.services.GetRequiredService<PersonSourceViewModel> ();
            _vm = (PersonSourceViewModel) DataContext;
            var window = TopLevel.GetTopLevel (this);
            _vm.PassView (this);
            Loaded += OnLoaded;
            ActualThemeVariantChanged += ThemeChanged;
        }


        internal void SetPath ( string ? path )
        {
            _vm.TrySetPath ( this.GetType(), path);
        }


        internal void OnLoaded ( object sender, RoutedEventArgs args )
        {
            _theme = ActualThemeVariant.Key.ToString ();
            _vm.ChangeAccordingTheme (_theme);
        }


        internal void ThemeChanged ( object sender, EventArgs args )
        {
            if ( ActualThemeVariant == null )
            {
                return;
            }

            OnLoaded (null, null);
        }
    }
}
