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
        private PersonSourceUserControl _personSourceUC;
        private TemplateChoosingUserControl _templateChoosingUC;
        private ZoomNavigationUserControl _zoomNavigationUC;
        private SceneUserControl _sceneUC;
        private bool _entirePersonListIsSelected = false;
        private TextBox _chosen;

        public PersonChoosingUserControl ()
        {
            InitializeComponent ();
            
        }


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


        private void TryToEnableBadgeCreationButton ()
        {
            bool itsTimeToEnable = ( _singlePersonIsSelected   ||   _entirePersonListIsSelected )   &&   _templateIsSelected;

            if ( itsTimeToEnable )
            {
                _templateChoosingUC. buildBadges.IsEnabled = true;
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


        internal void AcceptEntirePersonList ( object sender, TappedEventArgs args )
        {
            _entirePersonListIsSelected = true;
            _singlePersonIsSelected = false;
            TryToEnableBadgeCreationButton ();
        }

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