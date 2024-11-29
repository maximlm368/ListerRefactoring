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
using Avalonia.Animation.Easings;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace Lister.Views
{
    public partial class BadgeEditorView : ShowingDialog
        //ReactiveUserControl <BadgeEditorViewModel>
    {
        private static readonly string _question ="Сохранить изменения и вернуться к макету ?";

        private static bool _widthIsChanged;
        private static bool _heightIsChanged;

        private double _capturingY;
        private bool _runnerIsCaptured = false;
        private Image _currentIcon;
        private TextBlock _focused;
        private bool _capturedExists;
        private bool _focusedExists;
        private bool _pointerIsPressed;
        private Point _pointerPosition;
        private ModernMainView _back;
        private BadgeEditorViewModel _vm;
        private bool _isReleaseLocked;
        private Stopwatch _focusTime;
        private double _dastinationPointer = 0;

        public bool IconIsTapped { get; private set; }


        public BadgeEditorView ()
        {
            InitializeComponent ();
        }


        public BadgeEditorView ( bool newEditorIsNeeded, int incorrectBadgesAmmount ) : this()
        {
            if ( newEditorIsNeeded   ||   _vm == null )
            {
                BadgeEditorViewModel viewModel = new BadgeEditorViewModel (incorrectBadgesAmmount);
                this.DataContext = viewModel;
                _vm = viewModel;
            }
            else
            {
                this.DataContext = _vm;
            }

            firstBadge.FocusAdorner = null;
            previousBadge.FocusAdorner = null;
            nextBadge.FocusAdorner = null;
            lastBadge.FocusAdorner = null;
            zoomOnBadge.FocusAdorner = null;
            zoomOutBadge.FocusAdorner = null;
            editionPanel.FocusAdorner = null;
            extender.FocusAdorner = null;
            up.FocusAdorner = null;
            down.FocusAdorner = null;
            upper.FocusAdorner = null;
            downer.FocusAdorner = null;

            filterChoosing.SelectedValue = "Все";
        }


        public override void HandleDialogClosing ()
        {
            _vm.HandleDialogClosing ();
        }


        internal void CompleteBacking ( )
        {
            MainWindow mainWindow = Parent as MainWindow;
            _back.SetProperSize ( _vm.ViewWidth, _vm.ViewHeight );

            mainWindow.CancelSizeDifference ();
            _back.ResetIncorrects ();
            mainWindow.Content = _back;
        }


        internal void ChangeSize ( double widthDifference, double heightDifference )
        {
            //Width -= widthDifference;
            //Height -= heightDifference;
            //workArea.Width -= widthDifference;
            //workArea.Height -= heightDifference;

            _vm.ChangeSize ( widthDifference, heightDifference);

            WaitingViewModel waitingVM = App.services.GetRequiredService <WaitingViewModel> ();
            waitingVM.ChangeSize ( heightDifference, widthDifference );
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

            if ( _widthIsChanged   ||   _heightIsChanged ) 
            {
                _vm.ChangeSize (widthDifference, heightDifference);
            }
        }


        internal void PassIncorrectBadges ( List <BadgeViewModel> incorrects
                                          , List <BadgeViewModel> allPrintable, PageViewModel firstPage ) 
        {
            _vm.PassIncorrects (incorrects, allPrintable, firstPage);
        }


        internal void PassBackPoint ( ModernMainView back )
        {
            _back = back;
            _vm.PassView (this);
        }


        internal void CorrespondToEmptyCurrentCollection ( int currentCount )
        {
            if ( currentCount < 1 )
            {
                cancel.IsEnabled = false;
            }
        }


        internal void CheckBacking ( )
        {
            _vm.HandleDialogOpenig ( );
            var dialog = new DialogWindow (this);
            DialogWindow.IsOpen = true;

            dialog.Message = _question;

            Task result = dialog.ShowDialog ( App.MainWindow );

            TaskScheduler uiScheduler = TaskScheduler.FromCurrentSynchronizationContext ();

            result.ContinueWith 
            (
                task => 
                {
                    if ( dialog.Result == dialog.yes )
                    {
                        _vm.ComplateGoBack (this);
                        DialogWindow.IsOpen = false;
                    }
                },
                uiScheduler
            );
        }


        internal void HandleTextEdition ( object sender, TextChangedEventArgs args )
        {
            string edited = editorTextBox.Text;

            bool actionWasJustNavigation = ((edited == null)   ||   (_focused == null));

            if ( actionWasJustNavigation )
            {
                return;
            }

            _vm.ResetFocusedText (edited);
        }


        internal void SelectionChanged ( object sender, SelectionChangedEventArgs args )
        {
            if ( _vm == null ) 
            {
                return;
            }

            ComboBox comboBox = sender as ComboBox;
            string selected = comboBox.SelectedValue as string;
            _vm.Filter (selected);
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

        internal void ToParticularBadge ( object sender, TappedEventArgs args )
        {
            Avalonia.Controls.Grid image = sender   as   Avalonia.Controls.Grid;
            BadgeCorrectnessViewModel context = image.DataContext as BadgeCorrectnessViewModel;
            _vm.ToParticularBadge (context);
        }


        internal void ShiftRunner ( object sender, TappedEventArgs args )
        {
            Canvas activator = sender as Canvas;
            _dastinationPointer = args.GetPosition (activator).Y;
            _vm.ShiftRunner (_dastinationPointer);
        }


        internal void CaptureRunner ( object sender, PointerPressedEventArgs args )
        {
            _runnerIsCaptured = true;
            Point inRunnerRelativePosition = args.GetPosition (( Canvas ) args.Source);
            _capturingY = inRunnerRelativePosition.Y;


            TextBox tb = new TextBox ();
        }


        internal void MoveRunner ( object sender, PointerEventArgs args )
        {
            if ( _runnerIsCaptured )
            {
                Point pointerPosition = args.GetPosition (( Canvas ) args.Source);
                double runnerVerticalDelta = _capturingY - pointerPosition.Y;
                _vm.MoveRunner (runnerVerticalDelta);
            }
        }


        internal void ReleaseRunner ( object sender, PointerReleasedEventArgs args )
        {
            if ( _runnerIsCaptured )
            {
                _runnerIsCaptured = false;
            }
        }


        internal void ScrollByWheel ( object sender, PointerWheelEventArgs args )
        {
            bool isDirectionUp = args.Delta.Y > 0;
            _vm.ScrollByWheel (isDirectionUp);
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


        internal void HandleScrollChange ( object sender, ScrollChangedEventArgs args )
        {
            double arg = args.OffsetDelta.Y;

            //double scroll = _vm.SliderOffset.Y;

            //int ddf = 0;

            //if ( _vm.ScrollChangedByNavigation ) 
            //{
            //    return;
            //}

            //_vm.SetOldOffset ();
        }

        #endregion

    }



    public abstract class ShowingDialog : UserControl 
    {
        public abstract void HandleDialogClosing ();
    }
}


//internal void Entered ( object sender, PointerEventArgs args)
//{
//    Back.Background = new SolidColorBrush (Colors.White);
//    Back.Foreground = new SolidColorBrush (Colors.Red);
//}


//internal void Exited ( object sender, PointerEventArgs args )
//{
//    Back.Background = new SolidColorBrush (Colors.White);
//    Back.Foreground = new SolidColorBrush (Colors.Black);
//}



//private async Task DoShowDialogAsync ( InteractionContext <DialogViewModel, string?> interaction )
//{
//    var dialog = new DialogWindow ();
//    dialog.DataContext = interaction.Input;

//    var result = await dialog.ShowDialog<string?> (this.Parent as MainWindow);
//    interaction.SetOutput (result);
//}


//internal void ToParticularBadge ( object sender, TextChangedEventArgs args )
//{
//    TextBox textBox = sender as TextBox;
//    string text = textBox.Text;

//    try
//    {
//        int badgeNumber = ( int ) UInt32.Parse (textBox.Text);

//        if ( ( badgeNumber < 1 )   ||   ( badgeNumber > _vm.VisibleBadges. Count ) )
//        {
//            visibleBadgeNumber.Text = _vm.BeingProcessedNumber.ToString ();
//            return;
//        }

//        if ( ( text.Length > 1 )   &&   ( text [0] == '0' ) )
//        {
//            visibleBadgeNumber.Text = _vm.BeingProcessedNumber.ToString ();
//            return;
//        }

//        _vm.ToParticularBadge (text);
//    }
//    catch ( System.FormatException e )
//    {
//        if ( ! string.IsNullOrWhiteSpace(text) ) 
//        {
//            visibleBadgeNumber.Text = _vm.BeingProcessedNumber.ToString ();
//        }
//    }
//    catch ( System.OverflowException e )
//    {
//        visibleBadgeNumber.Text = _vm.BeingProcessedNumber.ToString ();
//    }
//}
