using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.VisualTree;
using Lister.ViewModels;
using System.Reactive.Linq;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;
using ExtentionsAndAuxiliary;
using System.Globalization;
using System.IO;
using Avalonia.Controls.Shapes;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System.Runtime.InteropServices;

namespace Lister.Views
{
    public partial class BadgeEditorView : ReactiveUserControl <BadgeEditorViewModel>
    {
        private static bool _widthIsChanged;
        private static bool _heightIsChanged;

        private ContentControl _focused;
        private bool _capturedExists;
        private bool _pointerIsPressed;
        private Point _pointerPosition;
        private BadgeEditorViewModel _vm;


        public BadgeEditorView ()
        {
            InitializeComponent ();
            this.DataContext = new BadgeEditorViewModel ();
            _vm = DataContext as BadgeEditorViewModel;

            left.Focus ();

            this.WhenActivated (action => action (ViewModel!.ShowDialog.RegisterHandler (DoShowDialogAsync)));
        }


        private async Task DoShowDialogAsync ( InteractionContext <DialogViewModel, string?> interaction )
        {
            var dialog = new DialogWindow ();
            dialog.DataContext = interaction.Input;

            var result = await dialog.ShowDialog<string?> (this.Parent as MainWindow);
            interaction.SetOutput (result);
        }


        internal void ChangeSize ( double widthDifference, double heightDifference )
        {
            Width -= widthDifference;
            Height -= heightDifference;
            workArea.Width -= widthDifference;
            workArea.Height -= heightDifference;
            slider.Height -= heightDifference;
            _vm.WidthDelta = widthDifference;
            _vm.HeightDelta = heightDifference;
        }


        internal void SetProperSize ( double properWidth, double properHeight )
        {
            if ( properWidth != Width )
            {
                _widthIsChanged = true;
            }
            else 
            {
                _widthIsChanged = false;
            }

            if ( properHeight != Height )
            {
                _heightIsChanged = true;
            }
            else 
            {
                _heightIsChanged = false;
            }

            double widthDifference = Width - properWidth;
            double heightDifference = Height - properHeight;
            Width = properWidth;
            Height = properHeight;

            if ( _widthIsChanged ) 
            {
                workArea.Width -= widthDifference;
            }

            if ( _heightIsChanged ) 
            {
                workArea.Height -= heightDifference;
                slider.Height -= heightDifference;
            }
        }


        internal void PassIncorrectBadges ( List <BadgeViewModel> incorrects ) 
        {
            BadgeEditorViewModel viewModel = (BadgeEditorViewModel) DataContext;
            viewModel.VisibleBadges = incorrects;
        }


        internal void PassBackPoint ( ModernMainView back )
        {
            _vm.PassViews (this, back);
        }


        internal void ToParticularBadge ( object sender, KeyEventArgs args )
        {
            string key = args.Key.ToString();

            if ( key == "Up" )
            {
                _vm.ToPrevious ();
            }
            else if ( key == "Down" ) 
            {
                _vm.ToNext ();
            }
        }


        #region CapturingAndMovingByMouse

        internal void Focus ( object sender, TappedEventArgs args ) 
        {
            Label label = sender as Label;
            Border container;

            if ( _focused != null )
            {
                container = _focused.Parent as Border;
                container.BorderThickness = new Thickness (0, 0, 0, 0);
                _focused.Background = null;
            }

            _focused = label;
            zoomOn.IsEnabled = true;
            zoomOut.IsEnabled = true;
            string content = ( string ) _focused.Content;
            _vm.EnableSplitting(content);
            container = label.Parent as Border;
            container.BorderThickness = new Thickness(1,1,1,1);
            _vm.Focus (content);
            left.Focus ();
            Cursor = new Cursor(StandardCursorType.SizeAll);
        }


        internal void Move ( object sender, PointerEventArgs args )
        {
            Label label = sender as Label;

            if ( _capturedExists )
            {
                label.Content = label.Content;
                Point newPosition = args.GetPosition (_focused);
                double verticalDelta = _pointerPosition.Y - newPosition.Y;
                double horizontalDelta = _pointerPosition.X - newPosition.X;
                Point delta = new Point (horizontalDelta, verticalDelta);
                string capturedContent = label.Content.ToString () ?? string.Empty;
                _vm.MoveCaptured (capturedContent, delta);
            }
        }


        internal void Capture ( object sender, PointerPressedEventArgs args )
        {
            Label label = sender as Label;

            if ( label != _focused )
            {
                return;
            }

            _pointerPosition = args.GetPosition (label);
            _capturedExists = true;
        }


        internal void ReleaseCaptured ()
        {
            if ( _capturedExists )
            {
                _capturedExists = false;
                Border container = _focused.Parent as Border;
                container.BorderThickness = new Thickness (0, 0, 0, 0);
                _focused = null;
                zoomOn.IsEnabled = false;
                zoomOut.IsEnabled = false;
                spliter.IsEnabled = false;
                Cursor = new Cursor (StandardCursorType.Arrow);
                _vm.ReleaseCaptured ();
            }
        }
        #endregion

        #region Cursor

        internal void SetCrossCursor ( object sender, PointerEventArgs args )
        {
            //Border border = sender as Border;
            Label label = sender as Label;

            if ( label != _focused )
            {
                return;
            }

            Cursor = new Cursor (StandardCursorType.SizeAll);
        }


        internal void SetArrowCursor ( object sender, PointerEventArgs args )
        {
            //Border border = sender as Border;
            Label label = sender as Label;

            if ( label != _focused )
            {
                return;
            }

            Cursor = new Cursor (StandardCursorType.Arrow);
        }

        #endregion

        #region MovingByButtons

        internal void ToSide ( object sender, KeyEventArgs args )
        {
            if ( _focused == null )
            {
                return;
            }

            string key = args.Key.ToString ();

            string content = ( string ) _focused.Content;
            _vm.ToSide (content, key);
        }
        #endregion

        #region Navigation

        internal void ToParticularBadge ( object sender, TextChangedEventArgs args )
        {
            TextBox textBox = sender as TextBox;
            string text = textBox.Text;
            _vm.ToParticularBadge (text);
        }

        #endregion

    }



    //public class EnumerableLabel : Label 
    //{
    //    public static readonly DirectProperty<EnumerableLabel, int> IdProperty =
    //        AvaloniaProperty.RegisterDirect<EnumerableLabel, int>
    //        (
    //           nameof (Id),
    //           o => o.Id,
    //           ( o, v ) => o.Id = v
    //        );

    //    private int _id = 0;

    //    public int Id
    //    {
    //        get { return _id; }
    //        set { SetAndRaise (IdProperty, ref _id, value); }
    //    }


    //    public EnumerableLabel () : base () { }


    //}
}
