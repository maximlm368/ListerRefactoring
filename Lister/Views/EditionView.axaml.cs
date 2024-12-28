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
using Avalonia.Controls.Presenters;

namespace Lister.Views
{
    public partial class BadgeEditorView : ShowingDialog
    {
        private static readonly string _question ="Сохранить изменения и вернуться к макету ?";

        private static bool _widthIsChanged;
        private static bool _heightIsChanged;

        private double _capturingY;
        private bool _runnerIsCaptured = false;
        private Image _currentIcon;
        private TextBlock _focusedText;
        private Image _focusedImage;
        private Shape _focusedShape;
        private bool _capturedTextExists;
        private bool _capturedImageExists;
        private bool _capturedShapeExists;
        private bool _pointerIsPressed;
        private Point _pointerPosition;
        private MainView _back;
        private BadgeEditorViewModel _viewModel;
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
            if ( newEditorIsNeeded   ||   _viewModel == null )
            {
                EditorViewModelArgs args = App.services.GetRequiredService <EditorViewModelArgs> ();
                BadgeEditorViewModel viewModel = new BadgeEditorViewModel (incorrectBadgesAmmount, args);
                this.DataContext = viewModel;
                _viewModel = viewModel;
            }
            else
            {
                this.DataContext = _viewModel;
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
            _viewModel.HandleDialogClosing ();
        }


        internal void CompleteBacking ( )
        {
            MainWindow mainWindow = Parent as MainWindow;
            _back.SetProperSize ( _viewModel.ViewWidth, _viewModel.ViewHeight );

            mainWindow.CancelSizeDifference ();
            _back.ResetIncorrects ();
            mainWindow.Content = _back;
        }


        internal void ChangeSize ( double widthDifference, double heightDifference )
        {
            _viewModel.ChangeSize ( widthDifference, heightDifference);

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
                _viewModel.ChangeSize (widthDifference, heightDifference);
            }
        }


        internal void PassIncorrectBadges ( List <BadgeViewModel> incorrects
                                          , List <BadgeViewModel> allPrintable, PageViewModel firstPage ) 
        {
            _viewModel.PassIncorrects (incorrects, allPrintable, firstPage);
        }


        internal void PassBackPoint ( MainView back )
        {
            _back = back;
            _viewModel.PassView (this);
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
            _viewModel.HandleDialogOpenig ( );
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
                        _viewModel.ComplateGoBack (this);
                        DialogWindow.IsOpen = false;
                    }
                },
                uiScheduler
            );
        }


        internal void HandleTextEdition ( object sender, TextChangedEventArgs args )
        {
            string edited = editorTextBox.Text;

            bool actionWasJustNavigation = ((edited == null)   ||   (_focusedText == null));

            if ( actionWasJustNavigation )
            {
                return;
            }

            _viewModel.ResetFocusedText (edited);
        }


        internal void SelectionChanged ( object sender, SelectionChangedEventArgs args )
        {
            if ( _viewModel == null ) 
            {
                return;
            }

            ComboBox comboBox = sender as ComboBox;
            string selected = comboBox.SelectedValue as string;
            _viewModel.Filter (selected);
        }


        #region CapturingAndMoving

        internal void HandleGettingFocus ( object sender, GotFocusEventArgs args )
        {
            _isReleaseLocked = true;
        }


        internal void Focus ( object sender, TappedEventArgs args ) 
        {
            TextBlock textBlock = sender as TextBlock;

            if ( textBlock != null ) 
            {
                scalabilityGrade.IsEnabled = true;
                editorTextBox.IsEnabled = true;

                FocusTextLine (textBlock);
                return;
            }

            Image image = sender as Image;

            if ( image != null )
            {
                FocusImage (image);
                return;
            }

            Shape shape = sender as Shape;

            if ( shape != null ) 
            {
                FocusShape (shape);
                return;
            }
        }


        internal void FocusTextLine ( TextBlock line )
        {
            Border container;

            if ( _focusedText != null )
            {
                container = _focusedText.Parent as Border;
                container.BorderBrush = null;
            }

            _focusedText = line;
            string content = ( string ) _focusedText.Text;

            int lineNumber = 0;
            int counter = 0;
            bool shouldBreak = false;

            var children = textLines.GetLogicalChildren ();

            foreach ( var child in children )
            {
                var ch = child.GetLogicalChildren ();

                foreach ( var border   in   ch )
                {
                    var textBlocks = border.GetLogicalChildren ();

                    foreach ( var textBl   in   textBlocks )
                    {
                        if ( _focusedText.Equals (textBl) )
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

            container = line.Parent as Border;
            container.BorderBrush = new SolidColorBrush (new Color (255, 0, 0, 255));

            _viewModel.FocusTextLine (content, lineNumber);
            Cursor = new Cursor (StandardCursorType.SizeAll);
            _isReleaseLocked = true;
            _focusTime = Stopwatch.StartNew ();
        }


        internal void FocusShape ( Shape shape )
        {
            ShapeViewModel shapeViewModel = shape.DataContext as ShapeViewModel;

            if ( shapeViewModel != null )
            {
                _viewModel.FocusShape (shapeViewModel.Kind, shapeViewModel.Id);
            }

            _focusedShape = shape;
            Cursor = new Cursor (StandardCursorType.SizeAll);
            _isReleaseLocked = true;
            _focusTime = Stopwatch.StartNew ();
        }


        internal void FocusImage ( Image image )
        {
            ImageViewModel imageViewModel = image.DataContext as ImageViewModel;

            if ( imageViewModel != null )
            {
                _viewModel.FocusImage (imageViewModel.Id);
            }

            _focusedImage = image;
            Cursor = new Cursor (StandardCursorType.SizeAll);
            _isReleaseLocked = true;
            _focusTime = Stopwatch.StartNew ();
        }


        internal void Move ( object sender, PointerEventArgs args )
        {
            bool shouldMove = ( ( _capturedTextExists )  ||  ( _capturedImageExists )  ||  ( _capturedShapeExists ) );

            if ( shouldMove ) 
            {
                Point newPosition = _pointerPosition;

                if ( _capturedTextExists )
                {
                    newPosition = args.GetPosition (_focusedText);
                }
                else if ( _capturedImageExists )
                {
                    newPosition = args.GetPosition (_focusedImage);
                }
                else if ( _capturedShapeExists )
                {
                    newPosition = args.GetPosition (_focusedShape);
                }

                double verticalDelta = _pointerPosition.Y - newPosition.Y;
                double horizontalDelta = _pointerPosition.X - newPosition.X;
                Point delta = new Point (horizontalDelta, verticalDelta);
                _viewModel.MoveCaptured (delta);
            }
        }


        internal void ToSide ( object sender, KeyEventArgs args )
        {
            string key = args.Key.ToString ();
            _viewModel.FocusedToSide (key);
        }


        internal void Capture ( object sender, PointerPressedEventArgs args )
        {
            TextBlock textBlock = sender as TextBlock;

            if ( (_focusedText != null)   &&   (textBlock == _focusedText) )
            {
                _pointerPosition = args.GetPosition (textBlock);
                _capturedTextExists = true;
            }

            Image image = sender as Image;

            if ( ( _focusedImage != null ) && ( image == _focusedImage ) )
            {
                _pointerPosition = args.GetPosition (image);
                _capturedImageExists = true;
            }

            Shape shape = sender as Shape;

            if ( ( _focusedShape != null ) && ( shape == _focusedShape ) )
            {
                _pointerPosition = args.GetPosition (shape);
                _capturedShapeExists = true;
            }
        }


        internal void ReleaseCaptured ( )
        {
            bool focusedExists = ( ( _focusedText != null ) || ( _focusedImage != null ) || ( _focusedShape != null ) );

            if ( _capturedTextExists  ||  _capturedImageExists  ||  _capturedShapeExists )
            {
                Release ();
            }
            else if ( focusedExists   &&   (_focusTime != null) )
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
            if ( _focusedText != null )
            {
                Border container = _focusedText.Parent as Border;
                container.BorderBrush = null;
                _focusedText = null;
                _capturedTextExists = false;
            }
            else if ( _focusedImage != null )
            {
                _focusedImage = null;
                _capturedImageExists = false;
            }
            else if ( _focusedShape != null ) 
            {
                _focusedShape = null;
                _capturedShapeExists = false;
            }

            Cursor = new Cursor (StandardCursorType.Arrow);
            _viewModel.ReleaseCaptured ();
        }
        #endregion

        #region Cursor

        internal void SetCrossCursor ( object sender, PointerEventArgs args )
        {
            TextBlock textBlock = sender as TextBlock;

            if ( (_focusedText != null)   &&   (_focusedText == textBlock) )
            {
                Cursor = new Cursor (StandardCursorType.SizeAll);
            }

            Shape shape = sender as Shape;

            if ( ( _focusedShape != null )   &&   ( shape == _focusedShape) )
            {
                Cursor = new Cursor (StandardCursorType.SizeAll);
            }

            Image image = sender as Image;

            if ( ( _focusedImage != null )   &&   ( image == _focusedImage) )
            {
                Cursor = new Cursor (StandardCursorType.SizeAll);
            }
        }


        internal void SetArrowCursor ( object sender, PointerEventArgs args )
        {
            Cursor = new Cursor (StandardCursorType.Arrow);
        }

        #endregion

        #region Navigation

        internal void ToParticularBadge ( object sender, TappedEventArgs args )
        {
            Avalonia.Controls.Grid image = sender   as   Avalonia.Controls.Grid;
            BadgeCorrectnessViewModel context = image.DataContext as BadgeCorrectnessViewModel;
            _viewModel.ToParticularBadge (context);
        }


        internal void ShiftRunner ( object sender, TappedEventArgs args )
        {
            Canvas activator = sender as Canvas;
            _dastinationPointer = args.GetPosition (activator).Y;
            _viewModel.ShiftRunner (_dastinationPointer);
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
                _viewModel.MoveRunner (runnerVerticalDelta);
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
            _viewModel.ScrollByWheel (isDirectionUp);
        }


        internal void ToParticularBadge ( object sender, KeyEventArgs args )
        {
            string key = args.Key.ToString ();

            if ( key == "Up" )
            {
                _viewModel.ToPrevious ();
            }
            else if ( key == "Down" )
            {
                _viewModel.ToNext ();
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
