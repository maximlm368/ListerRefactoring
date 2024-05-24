using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using ContentAssembler;
using DynamicData;
using Lister.ViewModels;
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
        private bool _entirePersonListIsSelected = false;
        private PersonSourceUserControl _personSourceUC;
        private TemplateChoosingUserControl _templateChoosingUC;
        private ZoomNavigationUserControl _zoomNavigationUC;
        private SceneUserControl _sceneUC;


        public PersonChoosingUserControl ()
        {
            InitializeComponent ();
        }


        internal void PassNeighbours ( PersonSourceUserControl personSource, SceneUserControl scene
                                     , ZoomNavigationUserControl zoomNavigation, TemplateChoosingUserControl templateChoosing )
        {
            _sceneUC = scene;
            _personSourceUC = personSource;
            _zoomNavigationUC = zoomNavigation;
            _templateChoosingUC = templateChoosing;
        }


        internal void DropDownOrPickUpPersonListViaKey ( object sender, KeyEventArgs args )
        {
            string key = args.Key.ToString ();
            bool keyIsEnter = key == "Return";

            if ( keyIsEnter )
            {
                if ( _personListIsDropped )
                {
                    personList.Height = 0;
                    _personListIsDropped = false;
                }
                else
                {
                    personList.Height = CalculatePersonListHeight ();
                    _personListIsDropped = true;
                    _openedViaButton = false;
                }

                return;
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
                string partOfName = textBox.Text.ToLower ();

                List<Person> people = vm.People;
                ObservableCollection<Person> foundVisiblePeople = new ObservableCollection<Person> ();

                foreach ( Person person in people )
                {
                    if ( person.StringPresentation.ToLower () == partOfName )
                    {
                        RecoverVisiblePeople ();
                        return;
                    }

                    string entireName = person.StringPresentation;
                    string entireNameInLowCase = entireName.ToLower ();

                    if ( entireNameInLowCase.Contains (partOfName) && entireNameInLowCase != partOfName )
                    {
                        foundVisiblePeople.Add (person);
                    }
                }

                vm.VisiblePeople = foundVisiblePeople;
                personList.Height = CalculatePersonListHeight ();
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
            List<Person> people = vm.People;
            vm.VisiblePeople = new ();
            vm.VisiblePeople.AddRange (people);
        }


        internal void HandlePersonChoosingViaTapping ( object sender, TappedEventArgs args )
        {
            //personChoosingIsTapped = true;


            if ( _personListIsDropped   &&   _selectionIsChanged )
            {
                personList.Height = 0;
                _personListIsDropped = false;
                _selectionIsChanged = false;
            }
        }


        internal void HandleSelectionChanged ( object sender, SelectionChangedEventArgs args )
        {
            PersonChoosingViewModel vm = ( PersonChoosingViewModel ) DataContext;
            vm.ChosenPerson = ( Person ) personList.SelectedItem;
            Person person = ( Person ) personList.SelectedItem;

            if ( person != null )
            {
                personTextBox.Text = person.StringPresentation;
                _singlePersonIsSelected = true;
                _entirePersonListIsSelected = false;
                TryToEnableBadgeCreationButton ();
                _selectionIsChanged = true;
            }
        }


        private void TryToEnableBadgeCreationButton ()
        {
            bool itsTimeToEnable = ( _singlePersonIsSelected   ||   _entirePersonListIsSelected )   &&   _templateIsSelected;

            if ( itsTimeToEnable )
            {
                _templateChoosingUC.buildBadges.IsEnabled = true;
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


        internal void DropDownOrPickUpPersonList ( object sender, TappedEventArgs args )
        {
            if ( _personListIsDropped )
            {
                personList.Height = 0;
                _personListIsDropped = false;
            }
            else
            {
                personTextBox.Focus (NavigationMethod.Tab);
                personList.Height = CalculatePersonListHeight ();
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


        internal void AcceptEntirePersonList ( object sender, TappedEventArgs args )
        {
            _entirePersonListIsSelected = true;
            _singlePersonIsSelected = false;
            TryToEnableBadgeCreationButton ();
        }


        internal void DropDownOrPickUpPersonListViaFocus ( object sender, GotFocusEventArgs args )
        {
            if ( _personListIsDropped )
            {
                personList.Height = 0;
                _personListIsDropped = false;
            }
        }

    }
}
