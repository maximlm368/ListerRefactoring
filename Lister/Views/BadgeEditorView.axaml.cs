using Avalonia;
using Avalonia.Diagnostics;
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
using System.Diagnostics;

namespace Lister.Views
{
    public partial class BadgeEditorView : UserControl
        //ReactiveUserControl <BadgeEditorViewModel>
    {
        private static readonly string _question ="Сохранить изменения и вернуться к макету ?";

        private static bool _widthIsChanged;
        private static bool _heightIsChanged;

        private Image _currentIcon;
        private TextBlock _focused;
        private bool _capturedExists;
        private bool _focusedExists;
        private bool _pointerIsPressed;
        private Point _pointerPosition;
        private BadgeEditorViewModel _vm;
        private bool _isReleaseLocked;
        private Stopwatch _focusTime;


        public BadgeEditorView ()
        {
            InitializeComponent ();
            this.DataContext = new BadgeEditorViewModel ();
            _vm = DataContext as BadgeEditorViewModel;
            left.Focus ();


            //this.WhenActivated (action => action (ViewModel!.ShowDialog.RegisterHandler (DoShowDialogAsync)));
        }


        //private async Task DoShowDialogAsync ( InteractionContext <DialogViewModel, string?> interaction )
        //{
        //    var dialog = new DialogWindow ();
        //    dialog.DataContext = interaction.Input;

        //    var result = await dialog.ShowDialog<string?> (this.Parent as MainWindow);
        //    interaction.SetOutput (result);
        //}


        internal void ChangeSize ( double widthDifference, double heightDifference )
        {
            Width -= widthDifference;
            Height -= heightDifference;
            workArea.Width -= widthDifference;
            workArea.Height -= heightDifference;
            slider.Height -= heightDifference;
            _vm.ChangeSliderHeight (heightDifference);
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


        internal void CheckBacking ( )
        {
            var dialog = new DialogWindow ();
            dialog.Message = _question;
            Task result = dialog.ShowDialog (MainWindow._mainWindow);
            TaskScheduler uiScheduler = TaskScheduler.FromCurrentSynchronizationContext ();

            result.ContinueWith 
            (
                task => 
                {
                    if ( dialog.Result == dialog.yes )
                    {
                        _vm.ComplateGoBack (this);
                    }
                },
                uiScheduler
            );
        }


        internal void HandleTextEdition ( object sender, TextChangedEventArgs args )
        {
            string edited = editorTextBox.Text;

            if ( edited == string.Empty ) 
            {
                return;
            }

            _vm.ResetFocusedText (edited);
        }


        #region CapturingAndMovingByMouse

        internal void HandleGettingFocus ( object sender, GotFocusEventArgs args )
        {
            _isReleaseLocked = true;
        }


        internal void Focus ( object sender, TappedEventArgs args ) 
        {
            scalabilityGrade.IsEnabled = true;
            editorTextBox.IsEnabled = true;
            _focusedExists = true;

            TextBlock textBlock = sender as TextBlock;
            Border container;

            if ( _focused != null )
            {
                container = _focused.Parent as Border;
                container.BorderBrush = null;
                _focused.Background = null;
            }

            _focused = textBlock;
            zoomOn.IsEnabled = true;
            zoomOut.IsEnabled = true;
            string content = ( string ) _focused.Text;

            int lineNumber = 0;
            int counter = 0;
            bool shouldBreak = false;

            var children = textLines.GetLogicalChildren ();

            foreach ( var child   in   children )
            {
                var ch = child.GetLogicalChildren ();

                foreach ( var border   in   ch )
                {
                    var textBlocks = border.GetLogicalChildren ();

                    foreach ( var textBl   in   textBlocks )
                    {
                        if ( _focused.Equals(textBl) ) 
                        {
                            lineNumber = counter;
                            shouldBreak = true;
                            break;
                        }
                    }

                    if ( shouldBreak ) 
                    {
                        break;
                    }
                }

                if ( shouldBreak )
                {
                    break;
                }

                counter++;
            }

            container = textBlock.Parent as Border;
            container.BorderBrush = new SolidColorBrush (new Color(255, 0, 0, 255));

            _vm.Focus (content, lineNumber);
            left.Focus ();
            Cursor = new Cursor (StandardCursorType.SizeAll);
            
            _isReleaseLocked = true;
            _focusTime = Stopwatch.StartNew ();
        }


        internal void Move ( object sender, PointerEventArgs args )
        {
            TextBlock textBlock = sender as TextBlock;

            if ( _capturedExists )
            {
                textBlock.Text = textBlock.Text;
                Point newPosition = args.GetPosition (_focused);
                double verticalDelta = _pointerPosition.Y - newPosition.Y;
                double horizontalDelta = _pointerPosition.X - newPosition.X;
                Point delta = new Point (horizontalDelta, verticalDelta);
                _vm.MoveCaptured (delta);
            }
        }


        internal void Capture ( object sender, PointerPressedEventArgs args )
        {
            TextBlock textBlock = sender as TextBlock;

            if ( textBlock != _focused )
            {
                return;
            }

            _pointerPosition = args.GetPosition (textBlock);
            _capturedExists = true;
        }


        internal void ReleaseCaptured ( )
        {
            if ( _capturedExists )
            {
                Release ();
            }
            else if ( (_focused != null)   &&   (_focusTime != null) )
            {
                if ( ! _isReleaseLocked )
                {
                    Release ();
                }
                else 
                {
                    _focusTime.Stop ();
                    TimeSpan timeSpan = _focusTime.Elapsed;

                    if ( timeSpan.Ticks > 100_000_000 )
                    {
                        Release ();
                    }

                    _isReleaseLocked = false;
                }
            }
        }


        private void Release ( )
        {
            _capturedExists = false;
            scalabilityGrade.IsEnabled = false;
            editorTextBox.IsEnabled = false;
            _focusedExists = false;

            if ( _focused != null )
            {
                Border container = _focused.Parent as Border;
                container.BorderBrush = null;
                _focused = null;
            }

            zoomOn.IsEnabled = false;
            zoomOut.IsEnabled = false;
            spliter.IsEnabled = false;
            Cursor = new Cursor (StandardCursorType.Arrow);
            _vm.ReleaseCaptured ();
        }
        #endregion

        #region Cursor

        internal void SetCrossCursor ( object sender, PointerEventArgs args )
        {
            TextBlock textBlock = sender as TextBlock;

            if ( textBlock != _focused )
            {
                return;
            }

            Cursor = new Cursor (StandardCursorType.SizeAll);
        }


        internal void SetArrowCursor ( object sender, PointerEventArgs args )
        {
            TextBlock textBlock = sender as TextBlock;

            if ( textBlock != _focused )
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
            _vm.ToSide (key);
        }
        #endregion

        #region Navigation

        internal void ToParticularBadge ( object sender, TextChangedEventArgs args )
        {
            TextBox textBox = sender as TextBox;
            string text = textBox.Text;

            try
            {
                int badgeNumber = ( int ) UInt32.Parse (textBox.Text);

                if ( ( badgeNumber < 1 )   ||   ( badgeNumber > _vm.VisibleBadges. Count ) )
                {
                    visibleBadgeNumber.Text = _vm.BeingProcessedNumber.ToString ();
                    return;
                }

                if ( ( text.Length > 1 )   &&   ( text [0] == '0' ) )
                {
                    visibleBadgeNumber.Text = _vm.BeingProcessedNumber.ToString ();
                    return;
                }

                _vm.ToParticularBadge (text);
            }
            catch ( System.FormatException e )
            {
                if ( ! string.IsNullOrWhiteSpace(text) ) 
                {
                    visibleBadgeNumber.Text = _vm.BeingProcessedNumber.ToString ();
                }
            }
            catch ( System.OverflowException e )
            {
                visibleBadgeNumber.Text = _vm.BeingProcessedNumber.ToString ();
            }
        }


        internal void ToParticularBadge ( object sender, TappedEventArgs args )
        {
            Avalonia.Controls.Image image = sender   as   Avalonia.Controls.Image;
            BadgeCorrectnessViewModel context = image.DataContext as BadgeCorrectnessViewModel;
            BadgeViewModel boundBadge = context.BoundBadge;
            _vm.ToParticularBadge (boundBadge);
        }


        internal void ToParticularBadge ( object sender, KeyEventArgs args )
        {
            string key = args.Key.ToString ();

            if ( key == "Up" )
            {
                _vm.ToPrevious ();
            }
            else if ( key == "Down" )
            {
                _vm.ToNext ();
            }
        }

        #endregion

    }
}
