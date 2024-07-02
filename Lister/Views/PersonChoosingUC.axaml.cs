using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.VisualTree;
using ContentAssembler;
using DynamicData;
using Lister.ViewModels;
using Lister.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

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
        private PersonChoosingViewModel _vm;
        //public bool SinglePersonIsSelected { get; private set; }
        //public bool EntirePersonListIsSelected { get; private set; }


        public PersonChoosingUserControl ()
        {
            InitializeComponent ();
            DataContext = App.services.GetRequiredService<PersonChoosingViewModel> ();
            _vm = (PersonChoosingViewModel) DataContext;
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
            _vm.ChosenPerson = null;
            personTextBox.Text = _vm.PlaceHolder;

            if ( _chosen != null )
            {
                _chosen.Background = new SolidColorBrush (new Color (255, 255, 255, 255));
                _chosen = null;
            }

            DropOrPickUp ();
        }


        #region Drop

        internal void CloseCustomCombobox ()
        {
            bool reasonExists = _personListIsDropped   &&   ! _cursorIsOverPersonList   &&   ! _buttonIsPressed;

            if ( reasonExists )
            {
                _vm.HideDropDown ();
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
                _vm.HideDropDown ();
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

            DisableBuildingButtons ();
            _vm.DisableBuildigPossibility ();
            TextBox textBox = ( TextBox ) sender;
            string str = textBox.Text;

            if ( str != null )
            {
                string fromSender = str.ToLower ();
                ObservableCollection <VisiblePerson> foundVisiblePeople = new ObservableCollection <VisiblePerson> ();

                foreach ( Person person   in   _vm.People )
                {
                    if ( fromSender == string.Empty   ||   (person.StringPresentation.ToLower () == fromSender) )
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


        private void DisableBuildingButtons () 
        {
            //ModernMainView parent = this.Parent.Parent as ModernMainView;
            //TemplateChoosingUserControl templateChoosingUC = parent.templateChoosing;
            //templateChoosingUC.buildBadges.IsEnabled = false;

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
            _vm.ChosenPerson = chosenPerson;
            personTextBox.Text = chosenPerson.StringPresentation;
            DropOrPickUp ();
        }

        #endregion Choosing

        private void DropOrPickUp ()
        {
            if ( _personListIsDropped )
            {
                personTextBox.Text = _vm.HideDropDown ();
                _personListIsDropped = false;
            }
            else
            {
                _vm.ShowDropDown ();
                _personListIsDropped = true;
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


            if ( _shiftScratch < 0   ||   _shiftScratch > activator.Height )
            {
                _vm.StopScrolling ();
                return;
            }

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
            _shiftScrollingStarted = true;
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


        internal void ReleasePressed ( )
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

            if ( _shiftScrollingStarted )
            {
                _shiftScrollingStarted = false;
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
        }


        private void ScrollByKey ( bool isDirectionUp )
        {
            int count = personList.ItemCount;
            _vm.ScrollByKey ( isDirectionUp, count );
        }


        #endregion Scrolling

    }
}


