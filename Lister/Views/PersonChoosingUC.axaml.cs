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
        
        private bool _cursorIsOverPersonList = false;
        private double _maxPersonListHeight;
        private double _minPersonListHeight;
        private double _personContainerHeight;
        private bool _personListIsDropped = false;
        private Label _chosen;
        private double _runnerStep = 0;
        private bool _runnerIsCaptured = false;
        private Point _runnerCapturingPosition = new Point(0, 0);
        private PersonSourceUserControl _personSourceUC;
        private SceneUserControl _sceneUC;
        private ZoomNavigationUserControl _zoomNavigationUC;
        private PersonChoosingViewModel _vm;
        public bool SinglePersonIsSelected { get; private set; }
        public bool EntirePersonListIsSelected { get; private set; }


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


        internal void ShiftScroller ( double shift )
        {
            _vm.ShiftScroller ( shift );
        }


        internal void AcceptEntirePersonList ( object sender, TappedEventArgs args )
        {
            EntirePersonListIsSelected = true;
            SinglePersonIsSelected = false;
            TryToEnableBadgeCreationButton ();
        }


        #region Drop

        internal void CloseCustomCombobox ()
        {
            bool reasonExists = _personListIsDropped   &&   !_cursorIsOverPersonList;

            if ( reasonExists )
            {
                visiblePersons.IsVisible = false;
                _personListIsDropped = false;
            }
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
            int fdfd = 0;

            if ( _personListIsDropped )
            {
                _personListIsDropped = false;
                visiblePersons.IsVisible = false;
            }
            else
            {
                _personListIsDropped = true;
                visiblePersons.IsVisible = true;
            }
            personTextBox.Focus (NavigationMethod.Tab);
            int dfdf = 0;
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
            int personCount = _vm.VisiblePeople. Count;
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
                string fromSenderLower = textBox.Text.ToLower ();
                ObservableCollection <Person> foundVisiblePeople = new ObservableCollection <Person> ();

                foreach ( Person person   in   _vm.People )
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

                _vm.VisiblePeople = foundVisiblePeople;
                visiblePersons.IsVisible = true;
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
        }

        #endregion PersonListReduction

        #region Choosing

        internal void AcceptFocusedPersonOrScroll ( object sender, KeyEventArgs args )
        {
            string key = args.Key.ToString ();
            bool keyIsEnter = key == "Return";

            if ( keyIsEnter )
            {
                Label focused = ( Label ) sender;
                focused.Background = new SolidColorBrush (3397631);
                string chosenName = (string) focused.Content;
                Person chosenPerson = _vm.FindPersonByStringPresentation (chosenName);

                if ( chosenPerson == null )
                {
                    return;
                }

                if ( _chosen != null )
                {
                    personTextBox.Text = chosenName;
                    SinglePersonIsSelected = true;
                    EntirePersonListIsSelected = false;
                    //_selectionIsChanged = true;
                    _chosen.Background = new SolidColorBrush (16777215);
                }

                _chosen = focused;
                _vm.ChosenPerson = chosenPerson;
                DropOrPickUp ();
            }
        }


        internal void HandleChoosingByTapping ( object sender, TappedEventArgs args )
        {
            Label chosenControl = ( Label ) sender;
            chosenControl.Background = new SolidColorBrush (3397631);

            if ( _chosen != null )
            {
                _chosen.Background = new SolidColorBrush (16777215);
            }

            string chosenName = (string) chosenControl.Content;
            Person chosenPerson = _vm.FindPersonByStringPresentation (chosenName);
            
            TryToEnableBadgeCreationButton ();
            DropOrPickUp ();
            
            if ( chosenPerson != null ) 
            {
                personTextBox.Text = chosenName;
                SinglePersonIsSelected = true;
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
                _personListIsDropped = false;
            }
            else
            {
                visiblePersons.IsVisible = true;
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

                CompleteScrolling (isDirectionUp, itemHeight, runnerStep, _vm);
            }
        }


        internal void ScrollByTapping ( object sender, TappedEventArgs args )
        {
            if ( visiblePersons.IsScrollable )
            {
                int personCount = _vm.VisiblePeople. Count;
                double step = personList.Height / personCount;
                double proportion = visiblePersons.Height / runner.Height;
                double runnerStep = step / proportion;
                runnerStep = GetInfluentStep (runnerStep);
                
                Canvas activator = sender as Canvas;
                bool isDirectionUp = activator.Name == "upper";

                CompleteScrolling (isDirectionUp, step, runnerStep, _vm);
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

                CompleteScrolling (isDirectionUp, step, runnerStep, _vm);
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
                Point pointerPosition = args.GetPosition (( Canvas ) args.Source);
                double runnerVerticalDelta = _runnerCapturingPosition.Y - pointerPosition.Y;
                double proportion = visiblePersons.Height / runner.Height;
                double personsVerticalDelta = runnerVerticalDelta * proportion;

                bool isDirectionUp = (runnerVerticalDelta > 0);
                CompleteScrolling ( isDirectionUp, personsVerticalDelta, runnerVerticalDelta, _vm );
            }
        }


        private void ScrollByKey ( bool isDirectionUp )
        {
            if ( visiblePersons.IsScrollable ) 
            {
                int count = personList.ItemCount;
                double listHeight = personList.Height;
                double itemHeight = listHeight / count;
                double proportion = visiblePersons.Height / runner.Height;
                double runnerStep = itemHeight / proportion;

                CompleteScrolling (isDirectionUp, itemHeight, runnerStep, _vm);
            }
        }


        private void CompleteScrolling ( bool isDirectionUp, double step, double runnerStep, PersonChoosingViewModel vm ) 
        {
            if ( scroller.Width == 0 ) return;

            double currentPersonsScrollValue = vm.PersonsScrollValue;

            if (! isDirectionUp )
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
//private bool _openedViaButton = false;
//private bool _selectionIsChanged = false;
//private bool _templateIsSelected = false;