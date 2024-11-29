using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Drawing;
using Avalonia;
using ContentAssembler;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Avalonia.Controls;
using Avalonia.Media;
using System.Windows.Input;
using System.Text;
using System.Net.WebSockets;
using System.ComponentModel;
using Lister.Views;
using Lister.Extentions;
using System.Collections.ObjectModel;
using static QuestPDF.Helpers.Colors;
using Avalonia.Controls.Shapes;
using DynamicData;
using ReactiveUI;
using Avalonia.Input;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Buffers.Binary;
using System.Reflection;
using Microsoft.Win32;
using ExtentionsAndAuxiliary;
using Avalonia.Controls.Primitives;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using DataGateway;

namespace Lister.ViewModels
{
    public partial class PersonChoosingViewModel : ViewModelBase
    {
        private static readonly string _fileIsOpenMessage = "Файл открыт в другом приложении. Закройте его и повторите выбор.";
        private static double _withScroll = 462;
        private static double _withoutScroll = 477;
        private static readonly double _minRunnerHeight = 10;
        private static readonly double _upperHeight = 15;
        private static readonly double _scrollingScratch = 32;
        private static readonly string _placeHolder = "Весь список";
        private static readonly int _maxVisibleCount = 4;
        private static readonly double _oneHeight = 32;
        private static readonly int _edge = 3;

        private static SolidColorBrush _entireListColor = new SolidColorBrush (new Avalonia.Media.Color (255, 255, 182, 193));
        private static SolidColorBrush _unfocusedColor = new SolidColorBrush (MainWindow.white);
        private static SolidColorBrush _focusedBorderColor = new SolidColorBrush (MainWindow.black);
        private static SolidColorBrush _focusedBackgroundColor = 
                                                         new SolidColorBrush (new Avalonia.Media.Color (255, 0, 200, 200));
        private Dictionary <BadgeLayout, KeyValuePair<string, List<string>>> _badgeLayouts;
        private bool _allListMustBe = false;
        private double _widthDelta;
        private Timer _timer;
        private VisiblePerson _focused;
        private VisiblePerson _tapped;
        private int _focusedNumber;
        private int _focusedEdge;
        private int _topLimit;
        private bool _chosenPersonIsSetInSetter;
        internal bool ScrollingIsOccured { get; set; }
        internal bool SinglePersonIsSelected { get; private set; }
        internal bool EntirePersonListIsSelected { get; private set; }
        internal bool BuildingIsPossible { get; private set; }
        internal List <VisiblePerson> PeopleStorage { get; set; }

        private ObservableCollection <TemplateViewModel> _templates;
        internal ObservableCollection <TemplateViewModel> Templates
        {
            get
            {
                return _templates;
            }
            set
            {
                this.RaiseAndSetIfChanged (ref _templates, value, nameof (Templates));
            }
        }

        private bool _isOpen;
        internal bool IsOpen
        {
            set
            {
                this.RaiseAndSetIfChanged (ref _isOpen, value, nameof (_isOpen));
            }
            get
            {
                return _isOpen;
            }
        }

        private TemplateViewModel _chosenTemplate;
        internal TemplateViewModel ChosenTemplate
        {
            set
            {
                bool valueIsSuitable = ( value != null )   &&   ( value.Name != string.Empty );

                if ( valueIsSuitable ) 
                {
                    foreach ( TemplateViewModel template in Templates )
                    {
                        bool isCoincident = ( template.Name == value.Name );

                        if ( isCoincident )
                        {
                            this.RaiseAndSetIfChanged (ref _chosenTemplate, value, nameof (ChosenTemplate));

                            SetReadiness ();
                        }
                    }
                }
            }

            get
            {
                return _chosenTemplate;
            }
        }

        private List <VisiblePerson> _involvedPeople;
        internal List <VisiblePerson> InvolvedPeople
        {
            get { return _involvedPeople; }
            set
            {
                _involvedPeople = value;
                SetPersonList ();
            } 
        }

        private Person _chosenPerson;
        internal Person ? ChosenPerson
        {
            get { return _chosenPerson; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _chosenPerson, value, nameof (ChosenPerson));

                _chosenPersonIsSetInSetter = true;

                if ( ChosenPerson == null )
                {
                    SetEntireListChoosingConsequences ();
                }
                else
                {
                    SetPersonChoosingConsequences ();
                }
            }
        }

        private bool _allAreSelected;
        public bool AllAreReady
        {
            get { return _allAreSelected; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _allAreSelected, value, nameof (AllAreReady));
            }
        }

        private ObservableCollection <VisiblePerson> _visiblePeople;
        internal ObservableCollection <VisiblePerson> VisiblePeople
        {
            get { return _visiblePeople; }
            set
            {
                this.RaiseAndSetIfChanged ( ref _visiblePeople , value , nameof ( VisiblePeople ) );
            }
        }

        private string _placeholder;
        internal string PlaceHolder
        {
            get { return _placeholder; }
            set
            {
                this.RaiseAndSetIfChanged (ref _placeholder, value, nameof (PlaceHolder));
            }
        }

        private FontWeight _fontWeight;
        internal FontWeight FontWeight
        {
            get { return _fontWeight; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _fontWeight, value, nameof (FontWeight));
            }
        }

        private double _visibleHeightStorage;
        private double _visibleHeight;
        internal double VisibleHeight
        {
            get { return _visibleHeight; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _visibleHeight, value, nameof (VisibleHeight));
            }
        }

        private SolidColorBrush _colorForEntireList;
        internal SolidColorBrush EntireListColor
        {
            get { return _colorForEntireList; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _colorForEntireList, value, nameof (EntireListColor));
            }
        }

        private double _personListWidth;
        internal double PersonListWidth
        {
            get { return _personListWidth; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _personListWidth, value, nameof (PersonListWidth));
            }
        }

        private double _personListHeight;
        internal double PersonListHeight
        {
            get { return _personListHeight; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _personListHeight, value, nameof (PersonListHeight));
            }
        }

        private double _personScrollValue;
        internal double PersonsScrollValue
        {
            get { return _personScrollValue; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _personScrollValue, value, nameof (PersonsScrollValue));
            }
        }

        internal double RealRunnerHeight { get; private set; }
        private double _runnerHeight;
        internal double RunnerHeight
        {
            get { return _runnerHeight; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _runnerHeight, value, nameof (RunnerHeight));
            }
        }

        private double _runnerTopCoordinate;
        internal double RunnerTopCoordinate
        {
            get { return _runnerTopCoordinate; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _runnerTopCoordinate, value, nameof (RunnerTopCoordinate));
            }
        }

        private double _topSpanHeight;
        internal double TopSpanHeight
        {
            get { return _topSpanHeight; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _topSpanHeight, value, nameof (TopSpanHeight));
            }
        }

        private double _bottomSpanHeight;
        internal double BottomSpanHeight
        {
            get { return _bottomSpanHeight; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _bottomSpanHeight, value, nameof (BottomSpanHeight));
            }
        }

        private double _scrollerWidth;
        internal double ScrollerWidth
        {
            get { return _scrollerWidth; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _scrollerWidth, value, nameof (ScrollerWidth));
            }
        }

        private double _scrollerCanvasLeft;
        internal double ScrollerCanvasLeft
        {
            get { return _scrollerCanvasLeft; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _scrollerCanvasLeft, value, nameof (ScrollerCanvasLeft));
            }
        }

        private double _firstItemHeight;
        internal double FirstItemHeight
        {
            get { return _firstItemHeight; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _firstItemHeight, value, nameof (FirstItemHeight));
            }
        }

        private bool _firstIsVisible;
        public bool FirstIsVisible
        {
            get { return _firstIsVisible; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _firstIsVisible, value, nameof (FirstIsVisible));

                if ( value ) 
                {
                    FirstItemHeight = 32;
                }
                else 
                {
                    FirstItemHeight = 0;
                }
            }
        }

        private bool _isPersonsScrollable;
        public bool IsPersonsScrollable
        {
            get { return _isPersonsScrollable; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _isPersonsScrollable, value, nameof (IsPersonsScrollable));
            }
        }

        private bool _textboxIsReadOnly;
        public bool TextboxIsReadOnly
        {
            get { return _textboxIsReadOnly; }
            set
            {
                this.RaiseAndSetIfChanged (ref _textboxIsReadOnly, value, nameof (TextboxIsReadOnly));
            }
        }

        private bool _textboxIsFocusable;
        public bool TextboxIsFocusable
        {
            get { return _textboxIsFocusable; }
            set
            {
                this.RaiseAndSetIfChanged (ref _textboxIsFocusable, value, nameof (TextboxIsFocusable));
            }
        }

        private double _dropDownOpacity;
        public double DropDownOpacity
        {
            get { return _dropDownOpacity; }
            set
            {
                this.RaiseAndSetIfChanged (ref _dropDownOpacity, value, nameof (DropDownOpacity));
            }
        }


        public PersonChoosingViewModel ( )
        {
            VisiblePeople = new ObservableCollection <VisiblePerson> ();
            ScrollerCanvasLeft = _withScroll;
            PersonsScrollValue = _oneHeight;
            TextboxIsReadOnly = false;
            TextboxIsFocusable = true;
            FontWeight = FontWeight.Bold;
            EntireListColor = _entireListColor;
            _focusedEdge = _edge;
        }


        internal void SetPersonsFromFile ( string ? path )
        {
            bool valueIsSuitable = ! string.IsNullOrWhiteSpace (path);

            if ( valueIsSuitable )
            {
                try
                {
                    IUniformDocumentAssembler documentAssembler = App.services.GetService<IUniformDocumentAssembler> ();
                    List<Person> persons = documentAssembler.GetPersons (path);
                    SetPersons (persons);
                    SwitchPersonChoosingEnabling (true);
                }
                catch ( IOException ex )
                {
                    var messegeDialog = new MessageDialog (ModernMainView.Instance, _fileIsOpenMessage);
                    WaitingViewModel waitingVM = App.services.GetRequiredService<WaitingViewModel> ();
                    waitingVM.HandleDialogOpenig ();
                    messegeDialog.ShowDialog (MainWindow.Window);
                }
            }
            else
            {
                SetPersons (null);
                SwitchPersonChoosingEnabling (true);
            }
        }


        private void SwitchPersonChoosingEnabling ( bool shouldEnable )
        {
            TextboxIsReadOnly = ! shouldEnable;
            TextboxIsFocusable = shouldEnable;
        }


        #region DropDown

        internal void HideDropDownWithChange ( )
        {
            if ( _focused == null )
            {
                if ( ! _chosenPersonIsSetInSetter ) 
                {
                    ChosenPerson = null;
                }

                PlaceHolder = _placeHolder;
            }
            else 
            {
                if ( ! _chosenPersonIsSetInSetter )
                {
                    ChosenPerson = _focused.Person;
                }
                ChosenPerson = _focused.Person;
                PlaceHolder = ChosenPerson. StringPresentation;
            }

            _chosenPersonIsSetInSetter = false;
            HideDropDownWithoutChange ();
            SetReadiness ();
        }


        internal void HideDropDownWithoutChange ()
        {
            if ( (InvolvedPeople. Count == 0)   &&   (PeopleStorage. Count > 0) ) 
            {
                RecoverVisiblePeople ();
                FontWeight = FontWeight.Bold;
                PlaceHolder = _placeHolder;
            }

            DropDownOpacity = 0;
            VisibleHeight = 0;
            FirstItemHeight = 0;
            FirstIsVisible = false;
        }


        internal void ShowDropDown ()
        {
            DropDownOpacity = 1;
            VisibleHeight = _visibleHeightStorage;
            FirstIsVisible = _allListMustBe;

            if (_allListMustBe) 
            {
                FirstItemHeight = _scrollingScratch;
            }
        }
        #endregion


        internal void SetPersons ( List <Person> ? persons )
        {
            if ( persons == null )
            {
                return;
            }

            List <VisiblePerson> peopleStorage = new ();
            List <VisiblePerson> involvedPeople = new ();

            foreach ( Person person   in   persons )
            {
                if (person.IsEmpty())
                {
                    continue;
                }

                VisiblePerson visiblePerson = new VisiblePerson (person);
                peopleStorage.Add (visiblePerson);
                involvedPeople.Add (visiblePerson);
            }

            PeopleStorage = peopleStorage;
            InvolvedPeople = involvedPeople;
            EntirePersonListIsSelected = true;
            SinglePersonIsSelected = false;
            ChosenPerson = null;
        }


        internal bool IsEmpty ( Person person )
        {
            bool isEmpty = false;

            isEmpty = isEmpty   ||   ( string.IsNullOrWhiteSpace (person.FamilyName) )
                                ||   ( string.IsNullOrWhiteSpace (person.FirstName) )
                                ||   ( string.IsNullOrWhiteSpace (person.PatronymicName) )
                                ||   ( string.IsNullOrWhiteSpace (person.Post) )
                                ||   ( string.IsNullOrWhiteSpace (person.Department) );

            return isEmpty;
        }


        internal void SetChosenPerson ( string personName )
        {
            Person person = FindPersonByStringPresentation (personName);
            int seekingScratch = _focusedNumber - _maxVisibleCount;

            if ( seekingScratch < 0 )
            {
                seekingScratch = 0;
            }

            int seekingEnd = _focusedNumber + _maxVisibleCount;

            if ( seekingScratch > InvolvedPeople.Count )
            {
                seekingScratch = InvolvedPeople.Count;
            }

            for ( int index = seekingScratch;   index <= seekingEnd;   index++ )
            {
                VisiblePerson foundPerson = InvolvedPeople [index];
                bool isCoincidence = person.Equals (foundPerson.Person);

                if ( isCoincidence )
                {
                    if ( _focused != null )
                    {
                        _focused.BorderBrushColor = _unfocusedColor;
                        _focused.BackgroundBrushColor = _unfocusedColor;
                    }
                    else
                    {
                        EntireListColor = _entireListColor;
                    }

                    SetTappedNull ();
                    _tapped = foundPerson;
                    _focused = foundPerson;
                    _focused.BorderBrushColor = _focusedBorderColor;
                    _focused.BackgroundBrushColor = _focusedBackgroundColor;
                    _focusedNumber = index;
                    PlaceHolder = personName;
                    break;
                }
            }
        }


        internal void SetEntireList ( )
        {
            if ( _focused != null )
            {
                _focused.BorderBrushColor = _unfocusedColor;
                _focused = null;
            }

            PlaceHolder = _placeHolder;
            EntireListColor = _focusedBorderColor;
            _focusedNumber = _focusedEdge - _maxVisibleCount;
        }


        private void SetPersonChoosingConsequences ( )
        {
            EntirePersonListIsSelected = false;
            SinglePersonIsSelected = true;
            FontWeight = FontWeight.Normal;
        }


        private void SetEntireListChoosingConsequences ()
        {
            EntirePersonListIsSelected = true;
            SinglePersonIsSelected = false;
            FontWeight = FontWeight.Bold;
        }


        internal void ShiftScroller ( double shift )
        {
            _widthDelta += shift;
            ScrollerCanvasLeft -= shift;
            PersonListWidth -= shift;
            _withoutScroll -= shift;
            _withScroll -= shift;
        }


        internal Person ? FindPersonByStringPresentation ( string presentation ) 
        {
            if ( string.IsNullOrWhiteSpace (presentation) ) 
            {
                return null;
            }

            Person result = null;

            foreach ( VisiblePerson person   in   InvolvedPeople ) 
            {
                bool isIntresting = person.Person.IsMatchingTo (presentation);
                
                if ( isIntresting ) 
                {
                    result = person.Person;
                    break;
                }
            }

            return result;
        }


        private void SetPersonList ( ) 
        {
            if ( (InvolvedPeople != null)   &&   (InvolvedPeople. Count > 0) )
            {
                int count = InvolvedPeople. Count;
                PersonListHeight = _oneHeight * count;
                bool listIsWhole = ( count == PeopleStorage. Count );
                _scrollValue = 0;

                if ( listIsWhole )
                {
                    SetEntireList (count);
                }
                else
                {
                    SetCutDownList (count);
                }

                _topLimit = _focusedNumber;
                SetScrollerIfShould ();
            }
            else 
            {
                FirstItemHeight = 0;
                FirstIsVisible = false;
                _allListMustBe = false;
                _visibleHeightStorage = 0;
                PersonListWidth = 0;
                ScrollerWidth = 0;
                IsPersonsScrollable = false;
                PersonsScrollValue = 0;
            }

            SetReadiness ();
        }


        private void SetEntireList ( int personCount )
        { 
            _visibleHeightStorage = _oneHeight * ( Math.Min (_maxVisibleCount, personCount) + 1 );
            FirstIsVisible = true;
            _allListMustBe = true;
            FirstItemHeight = _scrollingScratch;
            PersonsScrollValue = _scrollingScratch;

            _focused = null;
            EntireListColor = _focusedBorderColor;
            _focusedNumber = -1;
            _focusedEdge = _edge;

            EntirePersonListIsSelected = true;
            EntireListColor = new SolidColorBrush (MainWindow.black);

            TextboxIsReadOnly = false;
            TextboxIsFocusable = true;

            PlaceHolder = _placeHolder;

            FontWeight = FontWeight.Bold;

            SetVisiblePeople (0);
        }


        private void SetCutDownList ( int personCount )
        {
            FirstItemHeight = 0;
            FirstIsVisible = false;
            _allListMustBe = false;
            
            _visibleHeightStorage = _oneHeight * Math.Min (_maxVisibleCount, personCount);
            EntirePersonListIsSelected = false;
            PersonsScrollValue = 0;

            _focusedNumber = 0;
            _focused = InvolvedPeople [_focusedNumber];
            _focused.BorderBrushColor = _focusedBorderColor;
           _focusedEdge = _edge;

            SetVisiblePeople (0);
        }


        private void SetVisiblePeople ( int scratch )
        {
            VisiblePeople.Clear ();
            int limit = Math.Min (InvolvedPeople. Count, _maxVisibleCount);

            for ( int index = 0;   index < limit;   index++ ) 
            {
                int personIndex = scratch + index;
                VisiblePeople.Add (InvolvedPeople [personIndex]);
            }
        }


        internal void SetInvolvedPeople ( List <VisiblePerson> involvedPeople )
        {
            SetTappedNull ();
            InvolvedPeople = involvedPeople;
            _scrollValue = 0;
            ShowDropDown ();
        }


        private void SetScrollerIfShould () 
        {
            int personCount = InvolvedPeople. Count;

            if ( personCount > _maxVisibleCount )
            {
                PersonListWidth = _withScroll - _widthDelta;
                ScrollerWidth = _upperHeight;

                double scrollerWorkAreaHeight = _visibleHeightStorage - ( ScrollerWidth * 2 );
                double proportion = PersonListHeight / scrollerWorkAreaHeight;
                RunnerHeight = _visibleHeightStorage / proportion;
                RealRunnerHeight = RunnerHeight;

                if ( RunnerHeight < _minRunnerHeight )
                {
                    RunnerHeight = _minRunnerHeight;
                }

                RunnerTopCoordinate = _upperHeight;
                TopSpanHeight = 0;
                BottomSpanHeight = scrollerWorkAreaHeight - RunnerHeight;
                _runnerStep = BottomSpanHeight / (InvolvedPeople. Count - _maxVisibleCount);
                _scrollingLength = BottomSpanHeight;
                IsPersonsScrollable = true;
            }
            else 
            {
                PersonListWidth = _withoutScroll - _widthDelta;
                ScrollerWidth = 0;
                IsPersonsScrollable = false;
            }
        }


        private void SetReadiness ()
        {
            if ( ( InvolvedPeople != null )   &&   ( InvolvedPeople. Count > 0 ) )
            {
                bool areReady = ( SinglePersonIsSelected   ||   EntirePersonListIsSelected )   &&   (ChosenTemplate != null);

                if ( areReady )
                {
                    AllAreReady = true;
                }
                else
                {
                    AllAreReady = false;
                }
            }
        }


        internal void ToZeroPersonSelection ()
        {
            SinglePersonIsSelected = false;
            EntirePersonListIsSelected = false;

            AllAreReady = false;
        }


        internal void RecoverVisiblePeople ()
        {
            SetTappedNull ();
            _scrollValue = _scrollingScratch;
            List <VisiblePerson> recovered = new List <VisiblePerson> ();

            foreach ( VisiblePerson person   in   PeopleStorage )
            {
                person.BorderBrushColor = _unfocusedColor;
                recovered.Add (person);
            }

            InvolvedPeople = recovered;
            ShowDropDown ();
        }


        private void SetTappedNull ()
        {
            if ( _tapped != null )
            {
                _tapped.BackgroundBrushColor = _unfocusedColor;
                _tapped = null;
            }
        }


        internal void SetUp ( string theme )
        {
            IBadgeAppearenceProvider badgeAppearenceProvider = App.services.GetRequiredService<IBadgeAppearenceProvider> ();
            _badgeLayouts = badgeAppearenceProvider.GetBadgeLayouts ();

            SolidColorBrush correctColor = new SolidColorBrush (MainWindow.black);
            SolidColorBrush uncorrectColor = new SolidColorBrush (new Avalonia.Media.Color (100, 0, 0, 0));

            if ( theme == "Dark" )
            {
                correctColor = new SolidColorBrush (MainWindow.white);
                uncorrectColor = new SolidColorBrush (new Avalonia.Media.Color (100, 255, 255, 255));
            }

            ObservableCollection <TemplateViewModel> templates = new ();

            foreach ( KeyValuePair <BadgeLayout, KeyValuePair<string, List<string>>> layout   in   _badgeLayouts)
            {
                KeyValuePair<string, List<string>> sourceAndErrors = layout.Value;

                SolidColorBrush brush;
                bool correctLayoutHasEmptyMessage = ( sourceAndErrors.Value. Count < 1 );

                if ( correctLayoutHasEmptyMessage )
                {
                    brush = correctColor;
                }
                else
                {
                    brush = uncorrectColor;
                }

                List<string> errors = layout.Value. Value;
                string source = layout.Value. Key;
                
                templates.Add (new TemplateViewModel (new TemplateName(layout.Key.TemplateName, correctLayoutHasEmptyMessage)
                                                      , brush, errors, source));
            }

            Templates = templates;
        }



        //public void SetChosenTemplate ( Type callerType, TemplateViewModel settableTemplate )
        //{
        //    bool callerIsExeptable = ( callerType.FullName == "Lister.Views.PersonChoosingUserControl" );

        //    if ( callerIsExeptable )
        //    {
        //        foreach ( TemplateViewModel template   in   Templates ) 
        //        {
        //            bool isCoincident = ( template.Name == settableTemplate.Name );

        //            if ( isCoincident ) 
        //            {
                    
        //            }
        //        }



        //        ChosenTemplate = settableTemplate;
        //    }
        //}


        //public List <TemplateName> GetBadgeTemplateNames ()
        //{
        //    List<TemplateName> templateNames = new ();

        //    foreach ( KeyValuePair<string, string> template in _nameAndJson )
        //    {
        //        string jsonPath = template.Value;
        //        string backgroundPath =
        //            GetterFromJson.GetSectionValue (new List<string> { "BackgroundImagePath" }, jsonPath) ?? "";

        //        string imagePath = _resourceFolder + backgroundPath;
        //        bool isFound = true;

        //        try
        //        {
        //            using Stream stream = new FileStream (imagePath, FileMode.Open);
        //        }
        //        catch ( FileNotFoundException ex )
        //        {
        //            isFound = false;
        //        }

        //        templateNames.Add (new TemplateName (template.Key, isFound));
        //    }

        //    return templateNames;
        //}
    }



    public class TemplateName
    {
        public string Name { get; private set; }
        public bool IsFound { get; private set; }


        public TemplateName ( string name, bool isFound )
        {
            Name = name;
            IsFound = isFound;
        }
    }



    public class TemplateViewModel : ViewModelBase
    {
        private TemplateName TemplateName { get; set; }
        public string SourcePath { get; private set; }

        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                this.RaiseAndSetIfChanged (ref _name, value, nameof (Name));
            }
        }

        private SolidColorBrush _color;
        public SolidColorBrush Color
        {
            get
            {
                return _color;
            }
            set
            {
                this.RaiseAndSetIfChanged (ref _color, value, nameof (Color));
            }
        }

        private List<string> _correctnessMessage;
        public List<string> CorrectnessMessage
        {
            get
            {
                return _correctnessMessage;
            }
            set
            {
                _correctnessMessage = value;
            }
        }


        public TemplateViewModel ( TemplateName templateName, SolidColorBrush color
                                  , List<string> correctnessMessage, string sourcePath )
        {
            TemplateName = templateName;
            Name = templateName.Name;
            Color = color;
            CorrectnessMessage = correctnessMessage;
            SourcePath = sourcePath;
        }
    }
}


