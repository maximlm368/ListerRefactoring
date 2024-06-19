using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using ContentAssembler;
using DynamicData;
using Lister.ViewModels;
using Lister.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;

namespace Lister.Views
{
    public partial class PersonChoosingUserControl : UserControl
    {
        internal static double AllPersonsSignHeight { get; private set; }
        
        private bool _buttonIsPressed = false;
        private bool _cursorIsOverPersonList = false;
        private double _maxPersonListHeight;
        private double _minPersonListHeight;
        private double _personContainerHeight;
        private bool _personListIsDropped = false;
        private bool _allPersonsLableExists = true;
        private Label _chosen;
        private double _runnerStep = 0;
        private bool _runnerIsCaptured = false;
        private bool _tapScrollingStarted = false;
        private bool _shiftScrollingStarted = false;
        private double _capturingY;
        private double _shiftScratch = 0;
        private PersonSourceUserControl _personSourceUC;
        private SceneUserControl _sceneUC;
        private ZoomNavigationUserControl _zoomNavigationUC;
        private PersonChoosingViewModel _vm;
        public bool SinglePersonIsSelected { get; private set; }
        public bool EntirePersonListIsSelected { get; private set; }


        static PersonChoosingUserControl ()
        {
            AllPersonsSignHeight = 24;
        }


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
            EntirePersonListIsSelected = true;
            SinglePersonIsSelected = false;
            personTextBox.Text = "Весь список";
            personTextBox.FontWeight = FontWeight.Bold;

            if ( _chosen != null )
            {
                _chosen.Background = new SolidColorBrush (new Color (255, 255, 255, 255));
                _chosen = null;
            }

            DropOrPickUp ();
            TryToEnableBadgeCreationButton ();
        }


        #region Drop

        internal void CloseCustomCombobox ()
        {
            bool reasonExists = _personListIsDropped   &&   ! _cursorIsOverPersonList   &&   ! _buttonIsPressed;

            if ( reasonExists )
            {
                visiblePersons.IsVisible = false;
                _vm.FirstItemHeight = 0;
                _vm.FirstIsVisible = false;
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

            if ( _personListIsDropped )
            {
                _personListIsDropped = false;
                visiblePersons.IsVisible = false;
                _vm.FirstIsVisible = false;
                _vm.FirstItemHeight = 0;
            }
            else
            {
                _personListIsDropped = true;
                visiblePersons.IsVisible = true;

                if ( _allPersonsLableExists )
                {
                    _vm.VisiblePeople = _vm.VisiblePeople;

                    //_vm.FirstIsVisible = true;
                    //_vm.FirstItemHeight = PersonChoosingUserControl.AllPersonsSignHeight;
                }
            }
            personTextBox.Focus (NavigationMethod.Tab);
        }


        internal void DropOrPickUpPersonsByFocus ( object sender, GotFocusEventArgs args )
        {
            if ( _personListIsDropped )
            {
                visiblePersons.IsVisible = false;
                _vm.FirstItemHeight = 0;
                _vm.FirstIsVisible = false;
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

            DisableButtons ();
            TextBox textBox = ( TextBox ) sender;
            string str = textBox.Text;

            if ( str != null )
            {
                string fromSender = textBox.Text.ToLower ();
                ObservableCollection <Person> foundVisiblePeople = new ObservableCollection <Person> ();

                foreach ( Person person   in   _vm.People )
                {
                    if ( person.StringPresentation.ToLower () == fromSender )
                    {
                        RecoverVisiblePeople ();
                        return;
                    }

                    string entireName = person.StringPresentation;
                    entireName = entireName.ToLower ();

                    if ( entireName.Contains (fromSender)   &&   entireName != fromSender )
                    {
                        foundVisiblePeople.Add (person);
                    }
                }

                _vm.VisiblePeople = foundVisiblePeople;
                visiblePersons.IsVisible = true;
                //_allPersonsLableExists = false;
                _personListIsDropped = true;
            }
        }


        private void DisableButtons () 
        {
            ModernMainView parent = this.Parent.Parent as ModernMainView;
            TemplateChoosingUserControl templateChoosingUC = parent.templateChoosing;

            templateChoosingUC. buildBadges.IsEnabled = false;
            templateChoosingUC. clearBadges.IsEnabled = false;
            templateChoosingUC. save.IsEnabled = false;
            templateChoosingUC. print.IsEnabled = false;
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
            _vm.VisiblePeople = new () { _vm.People };
            visiblePersons.IsVisible = true;
            _allPersonsLableExists = true;
        }

        #endregion PersonListReduction

        #region Choosing

        internal void AcceptFocusedPersonOrScroll ( object sender, KeyEventArgs args )
        {
            string key = args.Key.ToString ( );
            bool keyIsEnter = key == "Return";

            if ( keyIsEnter )
            {
                Label focused = ( Label ) sender;
                focused.Background = new SolidColorBrush ( 3397631 );
                string chosenName = ( string ) focused.Content;
                Person chosenPerson = _vm.FindPersonByStringPresentation ( chosenName );

                if ( chosenPerson == null )
                {
                    return;
                }

                if ( _chosen != null )
                {
                    personTextBox.Text = chosenName;
                    SinglePersonIsSelected = true;
                    personTextBox.FontWeight = FontWeight.Normal;
                    EntirePersonListIsSelected = false;
                    //_selectionIsChanged = true;
                    _chosen.Background = new SolidColorBrush ( 16777215 );
                }

                _chosen = focused;
                _vm.ChosenPerson = chosenPerson;
                DropOrPickUp ( );
            }
        }


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
            TryToEnableBadgeCreationButton ();
            DropOrPickUp ();

            if ( chosenName == "Весь список" ) 
            {
                EntirePersonListIsSelected = true;
                SinglePersonIsSelected = false;
                chosenPerson = null;
            }

            if ( chosenPerson != null ) 
            {
                personTextBox.Text = chosenName;
                SinglePersonIsSelected = true;
                personTextBox.FontWeight = FontWeight.Normal;
                EntirePersonListIsSelected = false;
                //_selectionIsChanged = true;
                _vm.ChosenPerson = chosenPerson;
            }
        }

        #endregion Choosing

        private void TryToEnableBadgeCreationButton ()
        {
            ModernMainView parent = this.Parent. Parent  as  ModernMainView;
            bool templateIsSelected = parent.templateChoosing.TemplateIsSelected;

            bool itsTimeToEnable = ( SinglePersonIsSelected   ||   EntirePersonListIsSelected )   &&   templateIsSelected;

            if ( itsTimeToEnable )
            {
                TemplateChoosingUserControl templateChoosingUC = parent.templateChoosing;
                templateChoosingUC. buildBadges.IsEnabled = true;
            }
        }


        private void DropOrPickUp ()
        {
            if ( _personListIsDropped )
            {
                visiblePersons.IsVisible = false;
                _vm.FirstItemHeight = 0;
                _vm.FirstIsVisible = false;
                _personListIsDropped = false;
            }
            else
            {
                visiblePersons.IsVisible = true;

                if( _allPersonsLableExists ) 
                {
                    _vm.FirstItemHeight = PersonChoosingUserControl.AllPersonsSignHeight;
                    _vm.FirstIsVisible = true;
                }
                
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


        internal void ResetY ( object sender, PointerEventArgs args )
        {
            Canvas activator = sender as Canvas;
            _shiftScratch = args.GetPosition (activator).Y;

            if ( _shiftScratch < 0   ||   _shiftScratch > activator.Height ) 
            {
                _vm.StopScrolling ();
            }
        }


        internal void CaptureRunner ( object sender, PointerPressedEventArgs args )
        {
            _runnerIsCaptured = true;
            Point inRunnerRelativePosition = args.GetPosition (( Canvas ) args.Source);
            _capturingY = inRunnerRelativePosition.Y;
            int dfd = 0;
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


        //private double GetInfluentStep ( double step ) 
        //{
        //    double wholeStep = Math.Round (step);

        //    if ( wholeStep < 1 )
        //    {
        //        _runnerStep += step;

        //        if ( _runnerStep >= 1 )
        //        {
        //            step = _runnerStep;
        //            _runnerStep = 0;
        //        }
        //    }

        //    return step;
        //}

        #endregion Scrolling

    }
}


