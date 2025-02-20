using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.VisualTree;
using ContentAssembler;
using Lister.Extentions;
using Lister.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Drawing;

//using System.Drawing;
using System.Globalization;
using System.Reactive.Subjects;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace Lister.Views
{
    public partial class SceneUserControl : UserControl
    {
        private bool _pageIsCaptured;
        private Avalonia.Point _pointerPosition;
        private SceneViewModel _viewModel;

        public SceneUserControl ()
        {
            InitializeComponent ();
            DataContext = App.services.GetRequiredService<SceneViewModel> ();
            _viewModel = (SceneViewModel) DataContext;
            extender.FocusAdorner = null;
            edit.FocusAdorner = null;
            clearBadges.FocusAdorner = null;
            save.FocusAdorner = null;
            print.FocusAdorner = null;

            _pageIsCaptured = false;
        }


        internal void SomeButtonPressed ( object sender, PointerPressedEventArgs args )
        {
            MainView.SomeControlPressed = true;
        }


        internal void PagePressed ( object sender, PointerPressedEventArgs args )
        {
            PointerPoint point = args.GetCurrentPoint (sender as Control);

            if ( point.Properties.IsLeftButtonPressed )
            {
                Cursor = new Cursor (StandardCursorType.Hand);
                _pageIsCaptured = true;
                _pointerPosition = args.GetPosition (scroller);
            }
        }


        internal void ReleasePage ( )
        {
            _pageIsCaptured = false;
            Cursor = new Cursor (StandardCursorType.Arrow);
        }


        internal void MovePage ( PointerEventArgs args )
        {
            if ( _pageIsCaptured )
            {
                Avalonia.Point newPosition = args.GetPosition (scroller);
                double horizontalDelta = _pointerPosition.X - newPosition.X;
                double verticalDelta = _pointerPosition.Y - newPosition.Y;
                _pointerPosition = newPosition;

                double horizontalOffset = scroller.Offset.X;
                double verticalOffset = scroller.Offset.Y;

                scroller.Offset = new Vector (horizontalOffset + horizontalDelta, verticalOffset + verticalDelta);
            }
        }
    }
}
