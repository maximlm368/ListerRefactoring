using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using ContentAssembler;
using ExtentionsAndAuxiliary;
using Lister.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Lister.Views
{
    public partial class BadgesBuildingUserControl : UserControl
    {
        private static readonly string _jsonError = 
        "Невозможно загрузить этот шаблон.Обратитесь к разработчику по телефону 324-708";

        private static TemplateViewModel _chosen;

        private MainView _parent;
        private BadgesBuildingViewModel _viewModel;
        private string _theme;


        public BadgesBuildingUserControl ()
        {
            InitializeComponent ();

            DataContext = App.services.GetRequiredService<BadgesBuildingViewModel> ();
            _viewModel = ( BadgesBuildingViewModel ) DataContext;
            ActualThemeVariantChanged += ThemeChanged;

            buildBadges.FocusAdorner = null;
        }


        private void RightPointerPressed ( object sender, PointerPressedEventArgs args )
        {
            var point = args.GetCurrentPoint (sender as Control);

            if ( point.Properties.IsRightButtonPressed )
            {
                MainView.SomeControlPressed = true;
                this.Focus ();
            }
        }


        internal void ThemeChanged ( object sender, EventArgs args )
        {
            if ( ActualThemeVariant == null ) 
            {
                return;
            }

            _theme = ActualThemeVariant.Key.ToString ();
            _viewModel.ChangeAccordingTheme ( _theme );
        }
    }
}
