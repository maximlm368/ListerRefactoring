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
using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace Lister.Views
{
    public partial class PersonChoosingUserControl : UserControl
    {
        private bool _buttonIsPressed = false;
        private bool _cursorIsOverPersonList = false;
        private bool _personListIsDropped = false;
        private Label _chosen;
        private bool _runnerIsCaptured = false;
        private bool _tapScrollingStarted = false;
        private bool _shiftScrollingStarted = false;
        private double _capturingY;
        private double _shiftScratch = 0;
        private bool _choosingChanged = false;
        private PersonChoosingViewModel _vm;
        private string _textBoxText = string.Empty;

        //public bool SinglePersonIsSelected { get; private set; }
        //public bool EntirePersonListIsSelected { get; private set; }


        public PersonChoosingUserControl ()
        {
            InitializeComponent ();
            DataContext = App.services.GetRequiredService<PersonChoosingViewModel> ();
            _vm = (PersonChoosingViewModel) DataContext;

            personTextBox.AddHandler 
                ( TextBox.PastingFromClipboardEvent, IgnorPastingFromClipboard, RoutingStrategies.Bubble);

            personTextBox.AddHandler (TextBox.PointerReleasedEvent, PreventPasting, RoutingStrategies.Tunnel);


        }


        private void IgnorPastingFromClipboard ( object? sender, RoutedEventArgs args )
        {
            args.Handled = true;
        }


        private void PreventPasting ( object sender, PointerReleasedEventArgs args )
        {
            var point = args.GetCurrentPoint (sender as Control);
            var x = point.Position.X;
            var y = point.Position.Y;

            if ( point.Properties.IsLeftButtonPressed )
            {
                int dffd = 0;
            }
            if ( point.Properties.IsRightButtonPressed )
            {
                args.Handled = true;
            }


            args.Handled = true;
        }


        private void TextBox1_PreviewKeyUp ( object sender, KeyEventArgs e )
        {
            //if ( ( Keyboard.Modifiers & ModifierKeys.Control ) == ModifierKeys.Control && e.Key == Key.V )
            //{
            //    e.Handled = true;
            //}



            if ( e.Key.ToString() == "v"   &&   e.KeyModifiers.ToString() == "Control" )
            {
                e.Handled = true;
            }
        }


        internal void IgnorPasting ( object obj, TextChangedEventArgs args )
        {
            personTextBox.Text = _textBoxText;
            int dfd = 0;
        }


        internal void IgnorPasting ( object obj, RoutedEventArgs args )
        {
            _textBoxText = personTextBox.Text;
            int dfd = 0;
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
            personTextBox.Text = _vm.PlaceHolder;

            if ( _chosen != null )
            {
                _chosen.Background = new SolidColorBrush (new Color (255, 255, 255, 255));
                _chosen = null;
            }

            _choosingChanged = true;
            DropOrPickUp ();
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

            //if ( args.Key.ToString () == "V"   &&   args.KeyModifiers.ToString () == "Control" )
            //{
            //    args.Handled = true;
            //    return;
            //}

            if ( key == "Return" )
            {
                _choosingChanged = true;
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
                ScrollByKey(false);
            }


        }


        internal void DropOrPickUpPersons ( object sender, TappedEventArgs args )
        {
            _buttonIsPressed = true;
            DropOrPickUp ();
            personTextBox.Focus (NavigationMethod.Tab);
        }


        internal void DropOrPickUpPersonsByFocus ( object sender, GotFocusEventArgs args )
        {
            if ( _personListIsDropped )
            {
                _vm.HideDropDownWithChange ();
                _personListIsDropped = false;
            }
        }

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

            //string fd = args.Key.ToString ();
            //string dfd = args.KeyModifiers.ToString ();

            if ( args.Key.ToString () == "V"   &&   args.KeyModifiers.ToString () == "Control" )
            {
                args.Handled = true;
                return;
            }

            _vm.ToZeroPersonSelection ();
            _vm.DisableBuildigPossibility ();
            TextBox textBox = ( TextBox ) sender;
            string str = textBox.Text;

            if ( str != null )
            {
                string fromSender = str.ToLower ();
                ObservableCollection <VisiblePerson> foundVisiblePeople = new ObservableCollection <VisiblePerson> ();

                foreach ( Person person   in   _vm.People )
                {
                    if ( fromSender == string.Empty || ( person.StringPresentation.ToLower () == fromSender ) )
                    //if ( fromSender == string.Empty )
                    {
                        RecoverVisiblePeople ();
                        return;
                    }

                    string entireName = person.StringPresentation;
                    entireName = entireName.ToLower ();

                    if ( entireName.Contains (fromSender)   &&   entireName != fromSender )
                    {
                        VisiblePerson vP = new VisiblePerson (person);
                        foundVisiblePeople.Add (vP);
                    }
                }

                _vm.VisiblePeople = foundVisiblePeople;
            }

            if ( ! _personListIsDropped ) 
            {
                DropOrPickUp ();
            }
        }


        internal void HandlePasting ( object obj, PointerPressedEventArgs args )
        {
            TextBox tb = new TextBox ();

            var binding = new Binding ();
            //{
            //    ElementName = "source",
            //    Path = "Text",
            //};

            //tb.Bind (TextBox.CanCopyProperty, binding);
            
            //Button bt = new Button ();
            

            

        }


        internal void HandlePasting ( object obj, TappedEventArgs args )
        {
            args.Handled = true;




        }



        //private void textBox_PreviewExecuted ( object sender, RoutedEventArgs e )
        //{
        //    if ( e.c == ApplicationCommands.Copy ||
        //        e.Command == ApplicationCommands.Cut ||
        //        e.Command == ApplicationCommands.Paste )
        //    {
        //        e.Handled = true;
        //    }
        //}


        //private void CommandBinding_CanExecutePaste ( object sender, CanExecuteRoutedEventArgs e )
        //{
        //    e.CanExecute = false;
        //    e.Handled = true;
        //}





        //internal void HandleListReduction ( object sender, TextChangedEventArgs args )
        //{
        //    _vm.ToZeroPersonSelection ();
        //    _vm.DisableBuildigPossibility ();
        //    TextBox textBox = ( TextBox ) sender;
        //    string str = textBox.Text;

        //    if ( str == "Весь список" ) 
        //    {
        //        return;
        //    }

        //    if ( str != null )
        //    {
        //        string fromSender = str.ToLower ();
        //        ObservableCollection <VisiblePerson> foundVisiblePeople = new ObservableCollection <VisiblePerson> ();

        //        foreach ( Person person   in   _vm.People )
        //        {
        //            if ( (person.StringPresentation.ToLower () == fromSender) )
        //            {
        //                return;
        //            }

        //            if ( fromSender == string.Empty )
        //            {
        //                RecoverVisiblePeople ();
        //                return;
        //            }

        //            string entireName = person.StringPresentation;
        //            entireName = entireName.ToLower ();

        //            if ( entireName.Contains (fromSender)   &&   entireName != fromSender )
        //            {
        //                VisiblePerson vP = new VisiblePerson (person);
        //                foundVisiblePeople.Add (vP);
        //            }
        //        }

        //        _vm.VisiblePeople = foundVisiblePeople;
        //    }

        //    if ( ! _personListIsDropped )
        //    {
        //        DropOrPickUp ();
        //    }
        //}


        private void DisableBuildingButtons () 
        {
            _vm.ToZeroPersonSelection ();
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
            ObservableCollection <VisiblePerson> foundVisiblePeople = new ObservableCollection <VisiblePerson> ();

            foreach ( Person person   in   _vm.People ) 
            {
                VisiblePerson vP = new VisiblePerson (person);
                foundVisiblePeople.Add (vP);
            }

            _vm.VisiblePeople = foundVisiblePeople;
        }

        #endregion PersonListReduction

        #region Choosing

        internal void HandleChoosingByTapping ( object sender, TappedEventArgs args )
        {
            Label chosenLabel = ( Label ) sender;
            chosenLabel.Background = new SolidColorBrush ( new Color ( 255 , 0 , 200 , 200 ) );

            if ( _chosen != null )
            {
                _chosen.Background = new SolidColorBrush ( new Color ( 255 , 255 , 255 , 255 ) );
            }

            _chosen = chosenLabel;
            string chosenName = (string) chosenLabel.Content;
            Person chosenPerson = _vm.FindPersonByStringPresentation (chosenName);
            _vm.SetChosenPerson (chosenPerson);
            personTextBox.Text = chosenPerson.StringPresentation;
            DropOrPickUp ();
        }

        #endregion Choosing

        private void DropOrPickUp ()
        {
            if ( _personListIsDropped )
            {
                personTextBox.Text = _vm.HideDropDownWithChange ();
                personTextBox.SelectionStart = 0;
                personTextBox.SelectionEnd = personTextBox.Text.Length;
                _personListIsDropped = false;
            }
            else
            {
                _vm.ShowDropDown ();
                _personListIsDropped = true;
            }
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
            int count = personList.ItemCount;
            _vm.ScrollByWheel ( isDirectionUp, count );
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
            
            
            int count = personList.ItemCount;
            _vm.ShiftRunner ( isDirectionUp, count, limit );
        }


        //internal void ResetY ( object sender, PointerEventArgs args )
        //{
        //    Canvas activator = sender as Canvas;
        //    _shiftScratch = args.GetPosition (activator).Y;

        //    if ( _shiftScratch < 0   ||   _shiftScratch > activator.Height ) 
        //    {
        //        _vm.StopScrolling ();
        //    }
        //}


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
                _vm.GetFocusedNumber ();
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
                int count = personList.ItemCount;
                _vm.MoveRunner ( runnerVerticalDelta, count );
            }

            //TextBox tb = new TextBox ();
            //tb
        }


        private void ScrollByKey ( bool isDirectionUp )
        {
            int count = personList.ItemCount;
            _vm.ScrollByKey ( isDirectionUp, count );
        }


        #endregion Scrolling

    }
}


