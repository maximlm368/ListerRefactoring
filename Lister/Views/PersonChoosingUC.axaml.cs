using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using ContentAssembler;
using DynamicData;
using Lister.ViewModels;
using Lister.Views;
using System;
using System.Collections.ObjectModel;

namespace Lister.Views
{
    public partial class PersonChoosingUserControl : UserControl
    {
        private bool _templateIsSelected = false;
        private bool _cursorIsOverPersonList = false;
        private bool _singlePersonIsSelected = false;
        private bool _selectionIsChanged = false;
        private double _maxPersonListHeight;
        private double _minPersonListHeight;
        private double _personContainerHeight;
        private bool _openedViaButton = false;
        private bool _personListIsDropped = false;
        private TemplateChoosingUserControl _templateChoosingUC;
        private bool _entirePersonListIsSelected = false;
        private TextBox _chosen;
        private bool _isPersonsScrollable = false;
        private double _runnerStep = 0;
        private bool _runnerIsCaptured = false;
        private Point _runnerCapturingPosition = new Point(0, 0);

        public PersonChoosingUserControl ()
        {
            InitializeComponent ();
            
        }


        internal void AcceptEntirePersonList ( object sender, TappedEventArgs args )
        {
            _entirePersonListIsSelected = true;
            _singlePersonIsSelected = false;
            TryToEnableBadgeCreationButton ();
        }


        #region Drop

        internal void DropOrPickUpPersonsByKey ( object sender, KeyEventArgs args )
        {
            string key = args.Key.ToString ();
            bool keyIsEnter = key == "Return";

            if ( keyIsEnter )
            {
                DropOrPickUp ();

                return;
            }
        }


        internal void DropOrPickUpPersons ( object sender, TappedEventArgs args )
        {
            if ( _personListIsDropped )
            {
                visiblePersons.IsVisible = false;
                _personListIsDropped = false;
            }
            else
            {
                personTextBox.Focus (NavigationMethod.Tab);
                visiblePersons.IsVisible = true;
                _personListIsDropped = true;

                if ( _openedViaButton )
                {
                    _openedViaButton = false;
                }
                else
                {
                    _openedViaButton = true;
                }

            }
        }


        internal void DropOrPickUpPersonsByFocus ( object sender, GotFocusEventArgs args )
        {
            if ( _personListIsDropped )
            {
                visiblePersons.IsVisible = false;
                _personListIsDropped = false;
            }
        }


        private double CalculatePersonListHeight ()
        {
            PersonChoosingViewModel vm = (PersonChoosingViewModel) DataContext;
            int personCount = vm.VisiblePeople. Count;
            double personListHeight = _personContainerHeight * personCount;

            if ( personListHeight > _maxPersonListHeight )
            {
                personListHeight = _maxPersonListHeight;
            }

            if ( personListHeight < _minPersonListHeight )
            {
                personListHeight = _minPersonListHeight;
            }

            return personListHeight;
        }

        #endregion Drop

        #region PersonListReduction

        internal void HandlePersonListReduction ( object sender, KeyEventArgs args )
        {
            PersonChoosingViewModel vm = ( PersonChoosingViewModel ) DataContext;
            string key = args.Key.ToString ();
            bool keyIsUnimpacting = IsKeyUnipacting (key);

            if ( keyIsUnimpacting )
            {
                return;
            }

            _templateChoosingUC. buildBadges.IsEnabled = false;
            _templateChoosingUC. clearBadges.IsEnabled = false;
            _templateChoosingUC. save.IsEnabled = false;
            _templateChoosingUC. print.IsEnabled = false;

            TextBox textBox = ( TextBox ) sender;
            string str = textBox.Text;

            if ( str != null )
            {
                string fromSenderLower = textBox.Text.ToLower ();

                List <Person> people = vm.People;
                ObservableCollection <Person> foundVisiblePeople = new ObservableCollection <Person> ();

                foreach ( Person person   in   people )
                {
                    if ( person.StringPresentation.ToLower () == fromSenderLower )
                    {
                        RecoverVisiblePeople ();
                        return;
                    }

                    string entireName = person.StringPresentation;
                    string entireNameInLowCase = entireName.ToLower ();

                    if ( entireNameInLowCase.Contains (fromSenderLower)   &&   entireNameInLowCase != fromSenderLower )
                    {
                        foundVisiblePeople.Add (person);
                    }
                }

                vm.VisiblePeople = foundVisiblePeople;
                visiblePersons.IsVisible = true;
                _personListIsDropped = true;
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
            return keyIsUnimpacting;
        }


        private void RecoverVisiblePeople ()
        {
            PersonChoosingViewModel vm = ( PersonChoosingViewModel ) DataContext;
            List <Person> people = vm.People;
            vm.VisiblePeople = new ();
            vm.VisiblePeople.AddRange (people);
        }

        #endregion PersonListReduction

        #region Choosing

        internal void AcceptFocusedPerson ( object sender, KeyEventArgs args )
        {
            string key = args.Key.ToString ();
            bool keyIsEnter = key == "Return";

            if ( keyIsEnter )
            {
                PersonChoosingViewModel vm = ( PersonChoosingViewModel ) DataContext;
                TextBox focused = ( TextBox ) sender;
                focused.Background = new SolidColorBrush (3397631);
                string chosenName = focused.Text;
                Person chosenPerson = vm.FindPersonByStringPresentation (chosenName);

                if ( chosenPerson == null )
                {
                    return;
                }

                if ( _chosen != null )
                {
                    personTextBox.Text = chosenName;
                    _singlePersonIsSelected = true;
                    _entirePersonListIsSelected = false;
                    _selectionIsChanged = true;
                    _chosen.Background = new SolidColorBrush (16777215);
                }

                _chosen = focused;
                vm.ChosenPerson = chosenPerson;
                DropOrPickUp ();
            }
        }


        internal void HandleChoosingByTapping ( object sender, TappedEventArgs args )
        {
            PersonChoosingViewModel vm = ( PersonChoosingViewModel ) DataContext;

            TextBox chosenControl = ( TextBox ) sender;
            chosenControl.Background = new SolidColorBrush (3397631);

            if ( _chosen != null )
            {
                _chosen.Background = new SolidColorBrush (16777215);
            }

            string chosenName = (string) chosenControl.Text;
            Person chosenPerson = vm.FindPersonByStringPresentation (chosenName);
            
            TryToEnableBadgeCreationButton ();
            DropOrPickUp ();
            
            if ( chosenPerson != null ) 
            {
                personTextBox.Text = chosenName;
                _singlePersonIsSelected = true;
                _entirePersonListIsSelected = false;
                _selectionIsChanged = true;
                vm.ChosenPerson = chosenPerson;
            }
        }

        #endregion Choosing

        private void TryToEnableBadgeCreationButton ()
        {
            bool itsTimeToEnable = ( _singlePersonIsSelected   ||   _entirePersonListIsSelected )   &&   _templateIsSelected;

            if ( itsTimeToEnable )
            {
                _templateChoosingUC. buildBadges.IsEnabled = true;
            }
        }


        private void DropOrPickUp ()
        {
            if ( _personListIsDropped )
            {
                visiblePersons.IsVisible = false;
                _personListIsDropped = false;
            }
            else
            {
                visiblePersons.IsVisible = true;
                _personListIsDropped = true;
                _openedViaButton = false;
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
            if ( _isPersonsScrollable ) 
            {
                PersonChoosingViewModel vm = ( PersonChoosingViewModel ) DataContext;
                double step = 20;
                double proportion = visiblePersons.Height / runner.Height;
                double runnerStep = step / proportion;
                runnerStep = GetInfluentStep (runnerStep);
                bool isDirectionUp = true;

                CompleteScrolling (isDirectionUp, step, runnerStep, vm);
            }
        }


        internal void ScrollByTapping ( object sender, TappedEventArgs args )
        {
            if ( _isPersonsScrollable )
            {
                PersonChoosingViewModel vm = ( PersonChoosingViewModel ) DataContext;
                int personCount = vm.VisiblePeople. Count;
                double step = personList.Height / personCount;
                double proportion = visiblePersons.Height / runner.Height;
                double runnerStep = step / proportion;
                runnerStep = GetInfluentStep (runnerStep);
                
                Canvas activator = sender as Canvas;
                bool isDirectionUp = activator.Name == "upper";

                CompleteScrolling (isDirectionUp, step, runnerStep, vm);
            }
        }


        internal void ShiftRunner ( object sender, TappedEventArgs args )
        {
            if ( _isPersonsScrollable )
            {
                PersonChoosingViewModel vm = ( PersonChoosingViewModel ) DataContext;
                double runnerStep = runner.Height;
                double step = visiblePersons.Height;

                Canvas activator = sender as Canvas;
                bool isDirectionUp = activator.Name == "topSpan";

                CompleteScrolling (isDirectionUp, step, runnerStep, vm);
            }
        }


        internal void CaptureRunner ( object sender, PointerPressedEventArgs args )
        {
            _runnerIsCaptured = true;
            _runnerCapturingPosition = args.GetPosition (( Canvas ) args.Source);
        }


        internal void ReleaseRunner ( )
        {
            _runnerIsCaptured = false;
        }


        internal void MoveRunner ( object sender, PointerEventArgs args )
        {
            if ( _runnerIsCaptured ) 
            {
                PersonChoosingViewModel vm = ( PersonChoosingViewModel ) DataContext;
                Point pointerPosition = args.GetPosition (( Canvas ) args.Source);
                double runnerVerticalDelta = _runnerCapturingPosition.Y - pointerPosition.Y;
                double proportion = visiblePersons.Height / runner.Height;
                double personsVerticalDelta = runnerVerticalDelta * proportion;

                bool isDirectionUp = (runnerVerticalDelta > 0);
                CompleteScrolling ( isDirectionUp, personsVerticalDelta, runnerVerticalDelta, vm );
            }
        }


        private void CompleteScrolling ( bool isDirectionUp, double step, double runnerStep, PersonChoosingViewModel vm ) 
        {
            double currentPersonsScrollValue = vm.PersonsScrollValue;

            if ( isDirectionUp )
            {
                currentPersonsScrollValue -= step;
                double maxScroll = visiblePersons.Height - personList.Height;
                bool scrollExceeds = ( currentPersonsScrollValue < maxScroll );

                if ( scrollExceeds )
                {
                    currentPersonsScrollValue = maxScroll;
                }

                UpRunner (runnerStep, vm);
            }
            else
            {
                currentPersonsScrollValue += step;

                if ( currentPersonsScrollValue > 0 )
                {
                    currentPersonsScrollValue = 0;
                }

                DownRunner (runnerStep, vm);
            }

            vm.PersonsScrollValue = currentPersonsScrollValue;
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


//internal void PassNeighbours ( PersonSourceUserControl personSource, SceneUserControl scene
//                                     , ZoomNavigationUserControl zoomNavigation, TemplateChoosingUserControl templateChoosing )
//{
//    _sceneUC = scene;
//    _personSourceUC = personSource;
//    _zoomNavigationUC = zoomNavigation;
//    _templateChoosingUC = templateChoosing;
//}

//internal void HandlePersonChoosingViaTapping ( object sender, TappedEventArgs args )
//{
//    //personChoosingIsTapped = true;


//    if ( _personListIsDropped   &&   _selectionIsChanged )
//    {
//        visiblePersons.IsVisible = false;
//        _personListIsDropped = false;
//        _selectionIsChanged = false;
//    }
//}
//private PersonSourceUserControl _personSourceUC;
//private ZoomNavigationUserControl _zoomNavigationUC;
//private SceneUserControl _sceneUC;

