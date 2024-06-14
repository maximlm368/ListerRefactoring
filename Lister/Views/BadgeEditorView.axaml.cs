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

namespace Lister.Views
{
    public partial class BadgeEditorView : UserControl
    {
        private static double _widthDelta;
        private static double _heightDelta;
        ModernMainView _back;
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
            workArea.Width -= _widthDelta;
            workArea.Height -= _heightDelta;
        }


        internal void ChangeSize ( double widthDifference, double heightDifference )
        {
            workArea.Width -= widthDifference;
            workArea.Height -= heightDifference;
            _widthDelta += widthDifference;
            _heightDelta += heightDifference;
        }


        internal void PassIncorrectBadges ( List<BadgeViewModel> incorrects ) 
        {
            BadgeEditorViewModel viewModel = (BadgeEditorViewModel) DataContext;
            viewModel.IncorrectBadges = incorrects;
        }


        internal void PassBackPoint ( ModernMainView back )
        {
            _back = back;
        }


        internal void Focus ( object sender, TappedEventArgs args ) 
        {
            Label label = sender as Label;
            //label.Background = new SolidColorBrush (new Color (100, 255, 255, 255));
            
            if ( _focused != null ) 
            {
                _focused.Background = null;
            }

            _focused = label;
            zoomOn.IsEnabled = true;
            zoomOut.IsEnabled = true;
            string content = (string) _focused.Content;
            Border container = label.Parent as Border;
            container.BorderThickness = new Thickness(1,1,1,1);
            _vm.Focus (content);
            Cursor = new Cursor(StandardCursorType.Hand);
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


        internal void Move ( object sender, PointerEventArgs args )
        {
            Label label = sender as Label;
            
            if ( _capturedExists )
            {
                label.Content = label.Content;
                Point newPosition = args.GetPosition (_focused);
                double verticalDelta = _pointerPosition.Y - newPosition.Y;
                double horizontalDelta = _pointerPosition.X - newPosition.X;
                Point delta = new Point ( horizontalDelta, verticalDelta );
                string capturedContent = label.Content.ToString () ?? string.Empty;
                _vm.MoveCaptured ( capturedContent, delta );
            }
        }


        internal void StopConstantly ( object sender, PointerReleasedEventArgs args )
        {
            _pointerIsPressed = false;
        }


        internal void Left ( object sender, TappedEventArgs args )
        {
            if ( _focused == null ) 
            {
                return;
            }

            string content = (string) _focused.Content;
            _vm.Left ( content );
        }


        internal void Lefts ( object sender, KeyEventArgs args )
        {
            if ( _focused == null )
            {
                return;
            }

            string key = args.Key.ToString ();
            bool keyIsLeft = key == "Left";

            if (keyIsLeft) 
            {
                string content = ( string ) _focused.Content;
                _vm.Left (content);
            }

            
        }


        internal void LeftConstantly ( object sender, PointerPressedEventArgs args )
        {
            if ( _focused == null )
            {
                return;
            }

            _pointerIsPressed = true;
            string content = ( string ) _focused.Content;

            while (_pointerIsPressed) 
            {
                _vm.Left (content);
                Thread.Sleep (100);
            }
        }


        internal void Right ( object sender, TappedEventArgs args )
        {
            if ( _focused == null )
            {
                return;
            }

            string content = ( string ) _focused.Content;
            _vm.Right (content);
        }


        internal void Up ( object sender, TappedEventArgs args )
        {
            if ( _focused == null )
            {
                return;
            }

            string content = ( string ) _focused.Content;
            _vm.Up (content);
        }


        internal void Down ( object sender, TappedEventArgs args )
        {
            if ( _focused == null )
            {
                return;
            }

            string content = ( string ) _focused.Content;
            _vm.Down (content);
        }


        internal void ReleaseCaptured ( )
        {
            if ( _capturedExists )
            {
                _capturedExists = false;
                Border container = _focused.Parent as Border;
                container.BorderThickness = new Thickness (0, 0, 0, 0);
                //_focused.Background = null;
                _focused = null;
                zoomOn.IsEnabled = true;
                zoomOut.IsEnabled = true;
                Cursor = new Cursor (StandardCursorType.Arrow);
            }
        }


        internal void ToFirst ( object sender, TappedEventArgs args ) 
        {
            _vm.ToFirst ();
        }


        internal void ToPrevious ( object sender, TappedEventArgs args )
        {
            _vm.ToPrevious ();
        }


        internal void ToNext ( object sender, TappedEventArgs args )
        {
            _vm.ToNext ();
        }


        internal void ToLast ( object sender, TappedEventArgs args )
        {
            _vm.ToLast ();
        }


        internal void ToParticularBadge ( object sender, TextChangedEventArgs args )
        {
            TextBox textBox = sender as TextBox;
            string text = textBox.Text;
            _vm.ToParticularBadge (text);
        }


        internal void ReduceFontSize ( object sender, TappedEventArgs args )
        {
            if ( _focused == null ) 
            {
                return;
            }

            string content = (string) _focused.Content;
            _vm.ReduceFontSize (content);
        }


        internal void IncreaseFontSize ( object sender, TappedEventArgs args )
        {
            if ( _focused == null )
            {
                return;
            }

            string content = ( string ) _focused.Content;
            _vm.IncreaseFontSize (content);
        }


        internal void GoBack ( object sender, TappedEventArgs args )
        {
            _vm.SetOriginalScale ();
            MainWindow owner = this.Parent as MainWindow;
            _back.ChangeSize ( owner.WidthDifference, owner.HeightDifference );
            owner.ResetDifference ();
            owner.Content = _back;
        }
    }



    public class EnumerableLabel : Label 
    {
        public static readonly DirectProperty<EnumerableLabel, int> IdProperty =
            AvaloniaProperty.RegisterDirect<EnumerableLabel, int>
            (
               nameof (Id),
               o => o.Id,
               ( o, v ) => o.Id = v
            );

        private int _id = 0;

        public int Id
        {
            get { return _id; }
            set { SetAndRaise (IdProperty, ref _id, value); }
        }


        public EnumerableLabel () : base () { }


    }
}
