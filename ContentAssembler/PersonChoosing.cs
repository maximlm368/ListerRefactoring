//using System.Collections.ObjectModel;
//using System.ComponentModel;

//namespace ContentAssembler
//{
//    public partial class PersonChoosingModel
//    {
//        private static byte [] _entireListColor = new byte [] { 255, 255, 182, 193 };
//        private static byte [] _unfocusedColor = new byte [] { 255, 255, 255, 255 };
//        private static byte [] _focusedBorderColor = new byte [] { 255, 0, 0, 0 };
//        private static byte [] _focusedBackgroundColor = new byte[] { 255, 0, 200, 200 };
//        private Dictionary <BadgeLayout, KeyValuePair<string, List<string>>> _badgeLayouts;

//        internal bool ScrollingIsOccured { get; set; }
//        internal bool SinglePersonIsSelected { get; private set; }
//        internal bool EntirePersonListIsSelected { get; private set; }
//        internal bool BuildingIsPossible { get; private set; }

//        internal List <Person> PeopleStorage { get; private set; }

//        internal List <string> Templates { get; private set; }

//        public string ChosenTemplate { get; private set; }
        
//        public Person ? ChosenPerson { get; private set; }

//        public List <Person> InvolvedPeople { get; private set; }



//        public bool AllAreReady { get; private set; }

//        public bool SickTemplateIsSet { get; private set; }

//        public bool PersonsFileIsOpen { get; private set; }

//        public string PlaceHolder { get; private set; }



//        public PersonChoosingModel ()
//        {
//            BadgeAppearenceProvider badgeAppearenceProvider = new BadgeAppearenceProvider ();

//            if ( _badgeLayouts == null )
//            {
//                _badgeLayouts = badgeAppearenceProvider.GetBadgeLayouts ();
//            }
//        }


//        internal void SetPersonsFromFile ( string ? path )
//        {
//            bool valueIsSuitable = ! string.IsNullOrWhiteSpace (path);

//            if ( valueIsSuitable )
//            {
//                try
//                {
//                    UniformDocAssembler documentAssembler = new UniformDocAssembler ( null, null );
//                    InvolvedPeople = documentAssembler.GetPersons (path);
//                }
//                catch ( IOException ex )
//                {
//                    PersonsFileIsOpen = true;
//                }
//            }
//        }


//        private void SetPersons ( List <Person> ? persons )
//        {
//            if ( persons == null )
//            {
//                return;
//            }

//            List<VisiblePerson> peopleStorage = new ();
//            List<VisiblePerson> involvedPeople = new ();

//            foreach ( Person person in persons )
//            {
//                if ( person.IsEmpty () )
//                {
//                    continue;
//                }

//                VisiblePerson visiblePerson = new VisiblePerson (person);
//                peopleStorage.Add (visiblePerson);
//                involvedPeople.Add (visiblePerson);
//            }

//            PeopleStorage = peopleStorage;
//            InvolvedPeople = involvedPeople;
//            EntirePersonListIsSelected = true;
//            SinglePersonIsSelected = false;
//            ChosenPerson = null;
//        }


//        internal void SetChosenPerson ( string personName )
//        {
//            Person person = FindPersonByStringPresentation (personName);
//            int seekingScratch = _focusedNumber - _maxVisibleCount;

//            if ( seekingScratch < 0 )
//            {
//                seekingScratch = 0;
//            }

//            int seekingEnd = _focusedNumber + _maxVisibleCount;

//            if ( seekingScratch > InvolvedPeople.Count )
//            {
//                seekingScratch = InvolvedPeople.Count;
//            }

//            for ( int index = seekingScratch;   index <= seekingEnd;   index++ )
//            {
//                VisiblePerson foundPerson = InvolvedPeople [index];
//                bool isCoincidence = person.Equals (foundPerson.Person);

//                if ( isCoincidence )
//                {
//                    if ( _focused != null )
//                    {
//                        _focused.BorderBrushColor = _unfocusedColor;
//                        _focused.BackgroundBrushColor = _unfocusedColor;
//                    }
//                    else
//                    {
//                        EntireListColor = _entireListColor;
//                    }

//                    SetTappedNull ();
//                    _tapped = foundPerson;
//                    _focused = foundPerson;
//                    _focused.BorderBrushColor = _focusedBorderColor;
//                    _focused.BackgroundBrushColor = _focusedBackgroundColor;
//                    _focusedNumber = index;
//                    PlaceHolder = personName;
//                    break;
//                }
//            }
//        }


//        internal void SetEntireList ()
//        {
//            if ( _focused != null )
//            {
//                _focused.BorderBrushColor = _unfocusedColor;
//                _focused = null;
//            }

//            PlaceHolder = _placeHolder;
//            EntireListColor = _focusedBorderColor;
//            _focusedNumber = _focusedEdge - _maxVisibleCount;
//        }


//        private void SetPersonChoosingConsequences ()
//        {
//            EntirePersonListIsSelected = false;
//            SinglePersonIsSelected = true;
//            FontWeight = FontWeight.Normal;
//        }


//        private void SetEntireListChoosingConsequences ()
//        {
//            EntirePersonListIsSelected = true;
//            SinglePersonIsSelected = false;
//            FontWeight = FontWeight.Bold;
//        }


//        internal void ShiftScroller ( double shift )
//        {
//            _widthDelta += shift;
//            ScrollerCanvasLeft -= shift;
//            PersonListWidth -= shift;
//            _withoutScroll -= shift;
//            _withScroll -= shift;
//        }


//        internal Person ? FindPersonByStringPresentation ( string presentation )
//        {
//            if ( string.IsNullOrWhiteSpace (presentation) )
//            {
//                return null;
//            }

//            Person result = null;

//            foreach ( Person person  in   InvolvedPeople )
//            {
//                bool isIntresting = person.IsMatchingTo (presentation);

//                if ( isIntresting )
//                {
//                    result = person;
//                    break;
//                }
//            }

//            return result;
//        }


//        private void SetPersonList ()
//        {
//            if ( ( InvolvedPeople != null ) && ( InvolvedPeople.Count > 0 ) )
//            {
//                int count = InvolvedPeople.Count;
//                PersonListHeight = _oneHeight * count;
//                bool listIsWhole = ( count == PeopleStorage.Count );
//                _scrollValue = 0;

//                if ( listIsWhole )
//                {
//                    SetEntireList (count);
//                }
//                else
//                {
//                    SetCutDownList (count);
//                }

//                _topLimit = _focusedNumber;
//                SetScrollerIfShould ();
//            }
//            else
//            {
//                FirstItemHeight = 0;
//                FirstIsVisible = false;
//                _allListMustBe = false;
//                _visibleHeightStorage = 0;
//                PersonListWidth = 0;
//                ScrollerWidth = 0;
//                IsPersonsScrollable = false;
//                PersonsScrollValue = 0;
//            }

//            SetReadiness ();
//        }


//        private void SetEntireList ( int personCount )
//        {
//            _visibleHeightStorage = _oneHeight * ( Math.Min (_maxVisibleCount, personCount) + 1 );
//            FirstIsVisible = true;
//            _allListMustBe = true;
//            FirstItemHeight = _scrollingScratch;
//            PersonsScrollValue = _scrollingScratch;

//            _focused = null;
//            EntireListColor = _focusedBorderColor;
//            _focusedNumber = -1;
//            _focusedEdge = _edge;

//            EntirePersonListIsSelected = true;
//            EntireListColor = new SolidColorBrush (MainWindow.black);

//            TextboxIsReadOnly = false;
//            TextboxIsFocusable = true;

//            PlaceHolder = _placeHolder;

//            FontWeight = FontWeight.Bold;

//            SetVisiblePeople (0);
//        }


//        private void SetCutDownList ( int personCount )
//        {
//            FirstItemHeight = 0;
//            FirstIsVisible = false;
//            _allListMustBe = false;

//            _visibleHeightStorage = _oneHeight * Math.Min (_maxVisibleCount, personCount);
//            EntirePersonListIsSelected = false;
//            PersonsScrollValue = 0;

//            _focusedNumber = 0;
//            _focused = InvolvedPeople [_focusedNumber];
//            _focused.BorderBrushColor = _focusedBorderColor;
//            _focusedEdge = _edge;

//            SetVisiblePeople (0);
//        }


//        private void SetVisiblePeople ( int scratch )
//        {
//            VisiblePeople.Clear ();
//            int limit = Math.Min (InvolvedPeople.Count, _maxVisibleCount);

//            for ( int index = 0; index < limit; index++ )
//            {
//                int personIndex = scratch + index;
//                VisiblePeople.Add (InvolvedPeople [personIndex]);
//            }
//        }


//        internal void SetInvolvedPeople ( List<VisiblePerson> involvedPeople )
//        {
//            SetTappedNull ();
//            InvolvedPeople = involvedPeople;
//            _scrollValue = 0;
//            ShowDropDown ();
//        }


//        private void SetScrollerIfShould ()
//        {
//            int personCount = InvolvedPeople.Count;

//            if ( personCount > _maxVisibleCount )
//            {
//                PersonListWidth = _withScroll - _widthDelta;
//                ScrollerWidth = _upperHeight;

//                double scrollerWorkAreaHeight = _visibleHeightStorage - ( ScrollerWidth * 2 );
//                double proportion = PersonListHeight / scrollerWorkAreaHeight;
//                RunnerHeight = _visibleHeightStorage / proportion;
//                RealRunnerHeight = RunnerHeight;

//                if ( RunnerHeight < _minRunnerHeight )
//                {
//                    RunnerHeight = _minRunnerHeight;
//                }

//                RunnerTopCoordinate = _upperHeight;
//                TopSpanHeight = 0;
//                BottomSpanHeight = scrollerWorkAreaHeight - RunnerHeight;
//                _runnerStep = BottomSpanHeight / ( InvolvedPeople.Count - _maxVisibleCount );
//                _scrollingLength = BottomSpanHeight;
//                IsPersonsScrollable = true;
//            }
//            else
//            {
//                PersonListWidth = _withoutScroll - _widthDelta;
//                ScrollerWidth = 0;
//                IsPersonsScrollable = false;
//            }
//        }


//        private void SetReadiness ()
//        {
//            if ( ( InvolvedPeople != null ) && ( InvolvedPeople.Count > 0 ) )
//            {
//                bool areReady = ( SinglePersonIsSelected || EntirePersonListIsSelected ) && ( ChosenTemplate != null );

//                if ( areReady )
//                {
//                    AllAreReady = true;
//                }
//                else
//                {
//                    AllAreReady = false;
//                }
//            }
//        }


//        internal void ToZeroPersonSelection ()
//        {
//            SinglePersonIsSelected = false;
//            EntirePersonListIsSelected = false;

//            AllAreReady = false;
//        }


//        internal void RecoverVisiblePeople ()
//        {
//            SetTappedNull ();
//            _scrollValue = _scrollingScratch;
//            List<VisiblePerson> recovered = new List<VisiblePerson> ();

//            foreach ( VisiblePerson person in PeopleStorage )
//            {
//                person.BorderBrushColor = _unfocusedColor;
//                recovered.Add (person);
//            }

//            InvolvedPeople = recovered;
//            ShowDropDown ();
//        }


//        private void SetTappedNull ()
//        {
//            if ( _tapped != null )
//            {
//                _tapped.BackgroundBrushColor = _unfocusedColor;
//                _tapped = null;
//            }
//        }


//        internal void SetUp ( string theme )
//        {
//            SolidColorBrush correctColor = new SolidColorBrush (MainWindow.black);
//            SolidColorBrush uncorrectColor = new SolidColorBrush (new Avalonia.Media.Color (100, 0, 0, 0));

//            if ( theme == "Dark" )
//            {
//                correctColor = new SolidColorBrush (MainWindow.white);
//                uncorrectColor = new SolidColorBrush (new Avalonia.Media.Color (100, 255, 255, 255));
//            }

//            ObservableCollection<TemplateViewModel> templates = new ();

//            foreach ( KeyValuePair<BadgeLayout, KeyValuePair<string, List<string>>> layout in _badgeLayouts )
//            {
//                KeyValuePair<string, List<string>> sourceAndErrors = layout.Value;

//                SolidColorBrush brush;
//                bool correctLayoutHasEmptyMessage = ( sourceAndErrors.Value.Count < 1 );

//                if ( correctLayoutHasEmptyMessage )
//                {
//                    brush = correctColor;
//                }
//                else
//                {
//                    brush = uncorrectColor;
//                }

//                List<string> errors = layout.Value.Value;
//                string source = layout.Value.Key;

//                templates.Add (new TemplateViewModel (new TemplateName (layout.Key.TemplateName, correctLayoutHasEmptyMessage)
//                                                      , brush, errors, source));
//            }

//            Templates = templates;
//        }
//    }
//}