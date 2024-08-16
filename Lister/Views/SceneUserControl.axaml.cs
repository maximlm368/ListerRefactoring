using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.VisualTree;
using Lister.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Globalization;
using System.Reactive.Subjects;

namespace Lister.Views
{
    public partial class SceneUserControl : UserControl
    {
        private SceneViewModel _vm;
        private double _widthDelta;
        private double _heightDelta;


        public SceneUserControl ()
        {
            InitializeComponent ();
            DataContext = App.services.GetRequiredService<SceneViewModel> ();
            _vm = (SceneViewModel) DataContext;
            var window = TopLevel.GetTopLevel (this);
            _vm.PassView (this);
            this.Margin = new Avalonia.Thickness (5);

            Loaded += OnLoaded;
        }


        private void OnLoaded ( object sender, RoutedEventArgs args )
        {
            _vm.SetEdition ();
        }


        internal void EditIncorrectBadges ( List <BadgeViewModel> incorrectBadges )
        {
            ModernMainView mainView = ModernMainView.Instance;
            mainView.EditIncorrectBadges ( incorrectBadges );
        }
    }
}
