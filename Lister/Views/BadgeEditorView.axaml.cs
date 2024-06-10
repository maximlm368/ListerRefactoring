using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Lister.ViewModels;
using System.Text;

namespace Lister.Views
{
    public partial class BadgeEditorView : UserControl
    {
        ModernMainView _back;
        private Label _focused;
        private bool _capturedExists;
        private Point _pointerPosition;
        private BadgeEditorViewModel _vm;

        public BadgeEditorView ()
        {
            InitializeComponent ();
            this.DataContext = new BadgeEditorViewModel ();
            _vm = DataContext as BadgeEditorViewModel;
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
            label.Background = new SolidColorBrush (new Color (100, 255, 255, 255));

            if( _focused != null ) 
            {
                _focused.Background = null;
            }

            _focused = label;
        }


        internal void Capture ( object sender, PointerPressedEventArgs args )
        {
            Label captured = sender as Label;
            

            if ( captured != _focused ) 
            {
                return;
            }

            _pointerPosition = args.GetPosition (captured);
            _capturedExists = true;
        }


        internal void Move ( object sender, PointerEventArgs args )
        {
            if ( _capturedExists )
            {
                Point newPosition = args.GetPosition (_focused);
                double verticalDelta = _pointerPosition.Y - newPosition.Y;
                double horizontalDelta = _pointerPosition.X - newPosition.X;
                Point delta = new Point ( horizontalDelta, verticalDelta );
                _vm.MoveCaptured ( delta );
            }
        }


        internal void ReleaseCaptured ( )
        {
            if ( _capturedExists )
            {
                _capturedExists = false;
                _focused.Background = null;
                _focused = null;
            }
        }


        internal void ToFirst ( object sender, TappedEventArgs args ) 
        {
            _vm.ToFirst ();
            SetEnableBadgeNavigation ();
        }


        internal void ToPrevious ( object sender, TappedEventArgs args )
        {
            _vm.ToPrevious ();
            SetEnableBadgeNavigation ();
        }


        internal void ToNext ( object sender, TappedEventArgs args )
        {
            _vm.ToNext ();
            SetEnableBadgeNavigation ();
        }


        internal void ToLast ( object sender, TappedEventArgs args )
        {
            _vm.ToLast ();
            SetEnableBadgeNavigation ();
        }


        internal void ToParticularBadge ( object sender, TextChangedEventArgs args )
        {
            TextBox textBox = sender as TextBox;
            string text = textBox.Text;
            
            try 
            {
                int number = int.Parse ( text );
                _vm.ToParticularBadge (number);
                SetEnableBadgeNavigation ();
            }
            catch ( Exception ex ) 
            {
                return;
            }
        }


        internal void SetEnableBadgeNavigation ()
        {
            int badgeCount = _vm.IncorrectBadges. Count;

            if ( badgeCount > 1 )
            {
                if ( ( _vm.BeingProcessedNumber > 1 )   &&   ( _vm.BeingProcessedNumber == badgeCount ) )
                {
                    firstBadge.IsEnabled = true;
                    previousBadge.IsEnabled = true;
                    nextBadge.IsEnabled = false;
                    lastBadge.IsEnabled = false;
                }
                else if ( ( _vm.BeingProcessedNumber > 1 )   &&   ( _vm.BeingProcessedNumber < badgeCount ) )
                {
                    firstBadge.IsEnabled = true;
                    previousBadge.IsEnabled = true;
                    nextBadge.IsEnabled = true;
                    lastBadge.IsEnabled = true;
                }
                else if ( ( _vm.BeingProcessedNumber == 1 )   &&   ( badgeCount == 1 ) )
                {
                    firstBadge.IsEnabled = false;
                    previousBadge.IsEnabled = false;
                    nextBadge.IsEnabled = false;
                    lastBadge.IsEnabled = false;
                }
                else if ( ( _vm.BeingProcessedNumber == 1 )   &&   ( badgeCount > 1 ) )
                {
                    firstBadge.IsEnabled = false;
                    previousBadge.IsEnabled = false;
                    nextBadge.IsEnabled = true;
                    lastBadge.IsEnabled = true;
                }
            }
        }


        internal void GoBack ( object sender, TappedEventArgs args )
        {
            //BadgeEditorView ancestorView = this.Parent as BadgeEditorView;
            MainWindow owner = this.Parent as MainWindow;
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
