using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Remote.Protocol.Input;
using Avalonia.VisualTree;
using ContentAssembler;
using DynamicData;
using Lister.ViewModels;
using Lister.Views;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Lister.Views
{
    public partial class PersonChoosingUserControl : UserControl
    {
        private static SolidColorBrush _unfocusedColor = new SolidColorBrush (MainWindow.white);
        private readonly int _inputLimit = 100;
        private string _previousText;
        private bool _buttonIsPressed = false;
        private bool _cursorIsOverPersonList = false;
        private bool _personListIsDropped = false;
        private Label _chosen;
        private bool _runnerIsCaptured = false;
        private bool _tapScrollingStarted = false;
        private bool _shiftScrollingStarted = false;
        private double _capturingY;
        private double _shiftScratch = 0;
        private PersonChoosingViewModel _vm;
        private string _textBoxText = string.Empty;

        private SolidColorBrush _personTBBackground = new SolidColorBrush (MainWindow.black);


        public PersonChoosingUserControl ()
        {
            InitializeComponent ();

            DataContext = App.services.GetRequiredService<PersonChoosingViewModel> ();
            _vm = (PersonChoosingViewModel) DataContext;
            _vm.PassView (this);

            //personTextBox.Background = _personTBBackground;

            //personTextBox.AddHandler 
            //    ( TextBox.PastingFromClipboardEvent, IgnorPastingFromClipboard, RoutingStrategies.Bubble);

            personTextBox.AddHandler (TextBox.PointerReleasedEvent, PreventPasting, RoutingStrategies.Tunnel);
        }


        private void PreventPasting ( object sender, PointerReleasedEventArgs args )
        {
            var point = args.GetCurrentPoint (sender as Control);
            var x = point.Position.X;
            var y = point.Position.Y;

            //if ( point.Properties.IsLeftButtonPressed )
            //{
            //}
            //if ( point.Properties.IsRightButtonPressed )
            //{
            //    args.Handled = true;
            //}

            args.Handled = true;
        }


        internal void AdjustComboboxWidth ( double shift )
        {
            personList.Width -= shift;
            personTextBox.Width -= shift;
            comboboxFrame.Width -= shift;
            visiblePersons.Width -= shift;
            listFrame.Width -= shift;
            _vm.ShiftScroller (shift);
        }


        internal void AcceptEntirePersonList ( object sender, TappedEventArgs args )
        {
            _vm.SetEntireList ();

            if ( _chosen != null )
            {
                _chosen.Background = new SolidColorBrush (new Color (255, 255, 255, 255));
                _chosen = null;
            }

            DropOrPickUp ();
        }


        internal void GotFocus ( object sender, GotFocusEventArgs args )
        {
            if ( personTextBox.Text == null )
            {
                return;
            }

            personTextBox.SelectionStart = personTextBox.Text.Length;
            personTextBox.SelectionEnd = personTextBox.Text.Length;
        }


        internal void LostFocuse ( object sender, RoutedEventArgs args )
        {
            personTextBox.Foreground = new SolidColorBrush (MainWindow.black);
        }


        internal void PointerEntered ( object sender, PointerEventArgs args )
        {
            personTextBox.Foreground = new SolidColorBrush (MainWindow.white);
        }


        internal void PointerExited ( object sender, PointerEventArgs args )
        {
            if ( personTextBox.IsFocused ) 
            {
                return;
            }

            personTextBox.Foreground = new SolidColorBrush (MainWindow.black);
        }


        #region Drop

        internal void CloseCustomCombobox ()
        {
            bool reasonExists = _personListIsDropped   &&   ! _cursorIsOverPersonList   &&   ! _buttonIsPressed;

            if ( reasonExists )
            {
                _vm.HideDropDownWithoutChange ();
                _personListIsDropped = false;
            }

            _buttonIsPressed = false;
        }


        internal void DropOrPickUpPersonsOrScroll ( object sender, KeyEventArgs args )
        {
            string key = args.Key.ToString ();

            if ( key == "Return" )
            {
                DropOrPickUp ();
                return;
            }

            if ( key == "Escape" )
            {
                PickUp ();
                return;
            }

            if ( key == "Up" )
            {
                ScrollByKey (true);
                return;
            }

            if ( key == "Down" )
            {
                ScrollByKey (false);
            }
        }


        internal void DropOrPickUpPersons ( object sender, TappedEventArgs args )
        {
            _buttonIsPressed = true;
            DropOrPickUp ();
            personTextBox.Focus (NavigationMethod.Tab);
        }


        //internal void DropOrPickUpPersonsByFocus ( object sender, GotFocusEventArgs args )
        //{
        //    if ( _personListIsDropped )
        //    {
        //        _vm.HideDropDownWithChange ();
        //        _personListIsDropped = false;
        //    }
        //}
        
        #endregion Drop

        #region PersonListReduction

        internal void HandlePersonListReduction ( object sender, KeyEventArgs args )
        {
            string key = args.Key.ToString ();
            bool keyIsUnimpacting = IsKeyUnipacting (key);

            if ( keyIsUnimpacting )
            {
                return;
            }

            _vm.ToZeroPersonSelection ();
            _vm.DisableBuildigPossibility ();
            TextBox textBox = ( TextBox ) sender;
            string input = textBox.Text;

            if ( input != null )
            {
                RestrictInput (input);

                if ( ( input == string.Empty ) )
                {
                    RecoverVisiblePeople ();
                    _personListIsDropped = true;
                    return;
                }

                List <VisiblePerson> foundVisiblePeople = new List <VisiblePerson> ();

                foreach ( VisiblePerson person   in   _vm.PeopleStorage )
                {
                    person.BorderBrushColor = _unfocusedColor;
                    string entireName = person.Person. StringPresentation;

                    if ( entireName.Contains (input, StringComparison.CurrentCultureIgnoreCase) )
                    {
                        foundVisiblePeople.Add (person);
                    }
                }

                _vm.SetInvolvedPeople (foundVisiblePeople);
                _personListIsDropped = true;
            }
        }


        private void RestrictInput ( string input )
        {
            if ( input.Length >= _inputLimit )
            {
                personTextBox.Text = _previousText;
                input = _previousText;
            }
            else 
            {
                _previousText = input;
            }
        }


        private bool IsKeyUnipacting ( string key )
        {
            bool keyIsUnimpacting = key == "Tab";
            keyIsUnimpacting = keyIsUnimpacting || ( key == "Left" );
            keyIsUnimpacting = keyIsUnimpacting || ( key == "Up" );
            keyIsUnimpacting = keyIsUnimpacting || ( key == "Right" );
            keyIsUnimpacting = keyIsUnimpacting || ( key == "Down" );
            keyIsUnimpacting = keyIsUnimpacting || ( key == "Return" );
            keyIsUnimpacting = keyIsUnimpacting || ( key == "Escape" );
            return keyIsUnimpacting;
        }


        private void RecoverVisiblePeople ()
        {
            _vm.RecoverVisiblePeople ();
            _personListIsDropped = true;
        }

        #endregion PersonListReduction

        #region Choosing

        internal void HandleChoosingByTapping ( object sender, TappedEventArgs args )
        {
            Label chosenLabel = ( Label ) sender;
            _chosen = chosenLabel;
            string chosenName = ( string ) chosenLabel.Content;
            _vm.SetChosenPerson (chosenName);
            DropOrPickUp ();
        }

        #endregion Choosing

        private void DropOrPickUp ()
        {
            if ( personTextBox.Text == null ) 
            {
                return;
            }

            if ( _personListIsDropped )
            {
                _vm.HideDropDownWithChange ();
                _personListIsDropped = false;
            }
            else
            {
                _vm.ShowDropDown ();
                _personListIsDropped = true;
            }

            personTextBox.SelectionStart = personTextBox.Text.Length;
            personTextBox.SelectionEnd = personTextBox.Text.Length;
        }


        private void PickUp ()
        {
            if ( _personListIsDropped )
            {
                _vm.HideDropDownWithoutChange ();
                _personListIsDropped = false;
            }
        }


        internal void CursorIsOverPersonList ( object sender, PointerEventArgs args )
        {
            _cursorIsOverPersonList = true;
        }


        internal void CursorIsOutOfPersonList ( object sender, PointerEventArgs args )
        {
            _cursorIsOverPersonList = false;
        }

        #region Scrolling

        internal void ScrollByWheel ( object sender, PointerWheelEventArgs args )
        {
            bool isDirectionUp = args.Delta.Y > 0;
            _vm.ScrollByWheel ( isDirectionUp );
        }


        internal void ScrollByTapping ( object sender, PointerPressedEventArgs args )
        {
            Canvas activator = sender as Canvas;
            bool isDirectionUp = activator.Name == "upper";
            int count = personList.ItemCount;
            _tapScrollingStarted = true;
            _vm.ScrollByButton ( isDirectionUp, count );
        }


        internal void ShiftRunner ( object sender, PointerPressedEventArgs args )
        {
            Canvas activator = sender as Canvas;
            _shiftScratch = args.GetPosition ( activator ).Y;
            double limit = 0;
            bool isDirectionUp = activator.Name == "topSpan";

            if ( isDirectionUp ) 
            {
                limit = args.GetPosition (activator).Y;
            }
            else 
            {
                limit = bottomSpan.Height - args.GetPosition (activator).Y;
            }
            
            _vm.ShiftRunner ( isDirectionUp, limit );
        }


        internal void CaptureRunner ( object sender, PointerPressedEventArgs args )
        {
            _runnerIsCaptured = true;
            Point inRunnerRelativePosition = args.GetPosition (( Canvas ) args.Source);
            _capturingY = inRunnerRelativePosition.Y;
        }


        internal void ReleaseScrollingLeverage ( )
        {
            if ( _runnerIsCaptured ) 
            {
                _runnerIsCaptured = false;
            }

            if( _tapScrollingStarted )
            {
                _tapScrollingStarted = false;
                _vm.StopScrolling ( );
            }
        }


        internal void MoveRunner ( object sender, PointerEventArgs args )
        {
            if ( _runnerIsCaptured ) 
            {
                Point pointerPosition = args.GetPosition (( Canvas ) args.Source);
                double runnerVerticalDelta = _capturingY - pointerPosition.Y;
                _vm.MoveRunner ( runnerVerticalDelta );
            }
        }


        private void ScrollByKey ( bool isDirectionUp )
        {
            _vm.ScrollByKey ( isDirectionUp );
        }


        #endregion Scrolling

    }
}


