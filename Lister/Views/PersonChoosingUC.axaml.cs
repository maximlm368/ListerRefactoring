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
        private double _capturingY;
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
            Label chosenControl = ( Label ) sender;
            chosenControl.Background = new SolidColorBrush ( new Color ( 255 , 0 , 200 , 200 ) );

            if ( _chosen != null )
            {
                _chosen.Background = new SolidColorBrush ( new Color ( 255 , 255 , 255 , 255 ) );
            }

            _chosen = chosenControl;
            string chosenName = (string) chosenControl.Content;
            Person chosenPerson = _vm.FindPersonByStringPresentation (chosenName);
            TryToEnableBadgeCreationButton ();
            DropOrPickUp ();

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
            if ( visiblePersons.IsScrollable ) 
            {
                int count = personList.ItemCount;
                double listHeight = personList.Height;
                double itemHeight = listHeight / count;
                double proportion = visiblePersons.Height / runner.Height;
                double runnerStep = itemHeight / proportion;
                runnerStep = GetInfluentStep (runnerStep);
                bool isDirectionUp = true;

                CompleteScrolling (isDirectionUp, itemHeight, runnerStep);
            }
        }


        internal void ScrollByTapping ( object sender, TappedEventArgs args )
        {
            if ( visiblePersons.IsScrollable )
            {
                //int personCount = _vm.VisiblePeople. Count;
                //double step = personList.Height / personCount;
                double step = 24;
                double proportion = visiblePersons.Height / runner.Height;
                double runnerStep = step / proportion;
                runnerStep = GetInfluentStep (runnerStep);
                
                Canvas activator = sender as Canvas;
                bool isDirectionUp = activator.Name == "upper";

                CompleteScrolling (isDirectionUp, step, runnerStep);
            }
        }


        internal void ShiftRunner ( object sender, TappedEventArgs args )
        {
            if ( visiblePersons.IsScrollable )
            {
                double runnerStep = runner.Height;
                double step = visiblePersons.Height;

                Canvas activator = sender as Canvas;
                bool isDirectionUp = activator.Name == "topSpan";

                CompleteScrolling (isDirectionUp, step, runnerStep);
            }
        }


        internal void CaptureRunner ( object sender, PointerPressedEventArgs args )
        {
            _runnerIsCaptured = true;
            Point inRunnerRelativePosition = args.GetPosition (( Canvas ) args.Source);
            _capturingY = inRunnerRelativePosition.Y;
            int fdfd = 0;
        }


        internal void ReleaseRunner ( )
        {
            _runnerIsCaptured = false;
        }


        internal void MoveRunner ( object sender, PointerEventArgs args )
        {
            if ( _runnerIsCaptured ) 
            {
                Canvas can = ( Canvas ) args.Source;
                string name = can.Name;

                Point pointerPosition = args.GetPosition (( Canvas ) args.Source);
                double runnerVerticalDelta = _capturingY - pointerPosition.Y;

                double proportion = visiblePersons.Height / runner.Height;
                double personsVerticalDelta = runnerVerticalDelta * proportion;

                bool isDirectionUp = (runnerVerticalDelta > 0);
                CompleteScrolling ( isDirectionUp, personsVerticalDelta, runnerVerticalDelta );
            }
        }


        private void ScrollByKey ( bool isDirectionUp )
        {
            if ( visiblePersons.IsScrollable ) 
            {
                int count = personList.ItemCount;
                //double listHeight = personList.Height;
                //double itemHeight = listHeight / count;
                double itemHeight = 24;
                double proportion = visiblePersons.Height / runner.Height;
                double wholeSpan = _vm.TopSpanHeight + _vm.BottomSpanHeight - _vm.RunnerHeight;
                double runnerStep = wholeSpan / count;
                CompleteScrolling (isDirectionUp, itemHeight, runnerStep);
            }
        }


        private void CompleteScrolling ( bool isDirectionUp, double step, double runnerStep ) 
        {
            if ( scroller.Width == 0 ) return;

            double currentPersonsScrollValue = _vm.PersonsScrollValue;

            if ( isDirectionUp )
            {
                currentPersonsScrollValue += step;

                if ( currentPersonsScrollValue > 24 )
                {
                    currentPersonsScrollValue = 24;
                }

                UpRunner (runnerStep, _vm);
            }
            else
            {
                int count = personList.ItemCount;
                double itemHeight = 24;
                currentPersonsScrollValue -= step;
                double listHeight = itemHeight * count;
                double maxScroll = visiblePersons.Height - listHeight;
                bool scrollExceeds = ( currentPersonsScrollValue < maxScroll );

                if ( scrollExceeds )
                {
                    currentPersonsScrollValue = maxScroll;
                }

                DownRunner (runnerStep, _vm);
            }

            _vm.PersonsScrollValue = currentPersonsScrollValue;
            _vm.ScrollingIsOccured = true;
        }


        private void UpRunner ( double runnerStep, PersonChoosingViewModel vm ) 
        {
            vm.RunnerTopCoordinate -= runnerStep;

            if ( vm.RunnerTopCoordinate < upper.Height )
            {
                vm.RunnerTopCoordinate = upper.Height;
            }

            vm.TopSpanHeight -= runnerStep;

            if ( vm.TopSpanHeight < 0 )
            {
                vm.TopSpanHeight = 0;
            }

            vm.BottomSpanHeight += runnerStep;

            double maxHeight = scroller.Height - upper.Height - runner.Height - downer.Height;

            if ( vm.BottomSpanHeight > maxHeight )
            {
                vm.BottomSpanHeight = maxHeight;
            }
        }


        private void DownRunner ( double runnerStep, PersonChoosingViewModel vm ) 
        {
            vm.TopSpanHeight += runnerStep;

            double maxHeight = scroller.Height - upper.Height - runner.Height - downer.Height;

            if ( vm.TopSpanHeight > maxHeight )
            {
                vm.TopSpanHeight = maxHeight;
            }

            vm.RunnerTopCoordinate += runnerStep;

            double maxRunnerTopCoord = upper.Height + topSpan.Height;

            if ( vm.RunnerTopCoordinate > maxRunnerTopCoord )
            {
                vm.RunnerTopCoordinate = maxRunnerTopCoord;
            }

            vm.BottomSpanHeight -= runnerStep;

            if ( vm.BottomSpanHeight < 0 )
            {
                vm.BottomSpanHeight = 0;
            }
        }


        private double GetInfluentStep ( double step ) 
        {
            double wholeStep = Math.Round (step);

            if ( wholeStep < 1 )
            {
                _runnerStep += step;

                if ( _runnerStep >= 1 )
                {
                    step = _runnerStep;
                    _runnerStep = 0;
                }
            }

            return step;
        }

        #endregion Scrolling

    }
}


