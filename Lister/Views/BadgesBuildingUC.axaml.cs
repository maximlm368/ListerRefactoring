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

        private ModernMainView _parent;
        private BadgesBuildingViewModel _vm;
        private string _theme;


        public BadgesBuildingUserControl ()
        {
            InitializeComponent ();

            DataContext = App.services.GetRequiredService<BadgesBuildingViewModel> ();
            _vm = ( BadgesBuildingViewModel ) DataContext;
            ActualThemeVariantChanged += ThemeChanged;
        }


        internal void ThemeChanged ( object sender, EventArgs args )
        {
            if ( ActualThemeVariant == null ) 
            {
                return;
            }

            _theme = ActualThemeVariant.Key.ToString ();
            _vm.ChangeAccordingTheme ( _theme );
        }


        internal void CloseCustomCombobox ( object sender, GotFocusEventArgs args )
        {
            _parent = this.Parent.Parent as ModernMainView;
            PersonChoosingUserControl personChoosing = _parent. personChoosing;
            
            if ( personChoosing != null ) 
            {
                personChoosing.CloseCustomCombobox ();
            }
        }


        internal void HandleClosing ( object sender, EventArgs args )
        {

        }
    }
}
