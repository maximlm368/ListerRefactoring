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
using Lister.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Diagnostics.Metrics;
//using System.Drawing;
using System.Globalization;
using System.Reactive.Subjects;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace Lister.Views
{
    public partial class SceneUserControl : UserControl
    {
        private bool _pageIsCaptured;
        private Point _pointerPosition;

        public SceneUserControl ()
        {
            InitializeComponent ();
            DataContext = App.services.GetRequiredService<SceneViewModel> ();
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
                _pointerPosition = args.GetPosition (A4);
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
                Point newPosition = args.GetPosition (A4);

                double verticalDelta = _pointerPosition.Y - newPosition.Y;
                _pointerPosition = new Point (newPosition.X, newPosition.Y);

                double horizontalOffset = scroller.Offset.X;
                double verticalOffset = scroller.Offset.Y;

                scroller.Offset = new Vector (horizontalOffset, verticalOffset - verticalDelta/1.2);
            }
        }
    }
}
