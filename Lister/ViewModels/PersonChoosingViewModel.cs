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
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
using static Lister.ViewModels.MainViewModel;
using System.Reflection;
using Microsoft.Win32;
using ExtentionsAndAuxiliary;
using Avalonia.Controls.Primitives;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace Lister.ViewModels
{
    public partial class PersonChoosingViewModel : ViewModelBase
    {
        private static double _withScroll = 454;
        private static double _withoutScroll = 469;
        private static readonly double _minRunnerHeight = 10;
        private static readonly double _upperHeight = 15;
        private static readonly double _scrollingScratch = 25;
        private static readonly string _placeHolder = "Весь список";
        private static readonly int _maxVisibleCount = 4;
        private static readonly double _oneHeight = 25;
        private static readonly int _edge = 3;

        private static SolidColorBrush _entireListColor = new SolidColorBrush (new Avalonia.Media.Color (255, 255, 182, 193));
        private static SolidColorBrush _unfocusedColor = new SolidColorBrush (MainWindow.white);
        private static SolidColorBrush _focusedBorderColor = new SolidColorBrush (MainWindow.black);
        private static SolidColorBrush _focusedBackgroundColor = 
                                                         new SolidColorBrush (new Avalonia.Media.Color (255, 0, 200, 200));

        private TemplateChoosingViewModel _templateChoosingVM;
        private PersonSourceViewModel _personSourceVM;
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

        private List <VisiblePerson> iP;
        internal List <VisiblePerson> InvolvedPeople 
        {
            get { return iP; }
            set
            {
                iP = value;
                SetPersonList ();
            } 
        }

        private ObservableCollection <VisiblePerson> vP;
        internal ObservableCollection <VisiblePerson> VisiblePeople
        {
            get { return vP; }
            set
            {
                this.RaiseAndSetIfChanged ( ref vP , value , nameof ( VisiblePeople ) );
            }
        }

        private string pH;
        internal string PlaceHolder
        {
            get { return pH; }
            private set
            {
                this.RaiseAndSetIfChanged (ref pH, value, nameof (PlaceHolder));
            }
        }

        private FontWeight fW;
        internal FontWeight FontWeight
        {
            get { return fW; }
            private set
            {
                this.RaiseAndSetIfChanged (ref fW, value, nameof (FontWeight));
            }
        }

        private Person cP;
        internal Person ? ChosenPerson
        {
            get { return cP; }
            private set
            {
                this.RaiseAndSetIfChanged (ref cP, value, nameof (ChosenPerson));

                _chosenPersonIsSetInSetter = true;

                if ( ChosenPerson == null )
                {
                    SetEntireListChoosingConsequences ();
                }
                else 
                {
                    SetPersonChoosingConsequences ();
                }

                TryToEnableBadgeCreation ();
            }
        }

        private double _visibleHeightStorage;
        private double vH;
        internal double VisibleHeight
        {
            get { return vH; }
            private set
            {
                this.RaiseAndSetIfChanged (ref vH, value, nameof (VisibleHeight));
            }
        }

        private SolidColorBrush eC;
        internal SolidColorBrush EntireListColor
        {
            get { return eC; }
            private set
            {
                this.RaiseAndSetIfChanged (ref eC, value, nameof (EntireListColor));
            }
        }

        private double plW;
        internal double PersonListWidth
        {
            get { return plW; }
            private set
            {
                this.RaiseAndSetIfChanged (ref plW, value, nameof (PersonListWidth));
            }
        }

        private double plH;
        internal double PersonListHeight
        {
            get { return plH; }
            private set
            {
                this.RaiseAndSetIfChanged (ref plH, value, nameof (PersonListHeight));
            }
        }

        private double psV;
        internal double PersonsScrollValue
        {
            get { return psV; }
            private set
            {
                this.RaiseAndSetIfChanged (ref psV, value, nameof (PersonsScrollValue));
            }
        }

        internal double RealRunnerHeight { get; private set; }
        private double rH;
        internal double RunnerHeight
        {
            get { return rH; }
            private set
            {
                this.RaiseAndSetIfChanged (ref rH, value, nameof (RunnerHeight));
            }
        }

        private double rTC;
        internal double RunnerTopCoordinate
        {
            get { return rTC; }
            private set
            {
                this.RaiseAndSetIfChanged (ref rTC, value, nameof (RunnerTopCoordinate));
            }
        }

        private double tSH;
        internal double TopSpanHeight
        {
            get { return tSH; }
            private set
            {
                this.RaiseAndSetIfChanged (ref tSH, value, nameof (TopSpanHeight));
            }
        }

        private double bSH;
        internal double BottomSpanHeight
        {
            get { return bSH; }
            private set
            {
                this.RaiseAndSetIfChanged (ref bSH, value, nameof (BottomSpanHeight));
            }
        }

        private double sW;
        internal double ScrollerWidth
        {
            get { return sW; }
            private set
            {
                this.RaiseAndSetIfChanged (ref sW, value, nameof (ScrollerWidth));
            }
        }

        private double sCL;
        internal double ScrollerCanvasLeft
        {
            get { return sCL; }
            private set
            {
                this.RaiseAndSetIfChanged (ref sCL, value, nameof (ScrollerCanvasLeft));
            }
        }

        private double fH;
        internal double FirstItemHeight
        {
            get { return fH; }
            private set
            {
                this.RaiseAndSetIfChanged (ref fH, value, nameof (FirstItemHeight));
            }
        }

        private bool fV;
        public bool FirstIsVisible
        {
            get { return fV; }
            private set
            {
                this.RaiseAndSetIfChanged (ref fV, value, nameof (FirstIsVisible));

                if ( value ) 
                {
                    FirstItemHeight = 28;
                }
                else 
                {
                    FirstItemHeight = 0;
                }
            }
        }

        private bool isPS;
        public bool IsPersonsScrollable
        {
            get { return isPS; }
            private set
            {
                this.RaiseAndSetIfChanged (ref isPS, value, nameof (IsPersonsScrollable));
            }
        }

        private bool tRO;
        public bool TextboxIsReadOnly
        {
            get { return tRO; }
            set
            {
                this.RaiseAndSetIfChanged (ref tRO, value, nameof (TextboxIsReadOnly));
            }
        }

        private bool tIF;
        public bool TextboxIsFocusable
        {
            get { return tIF; }
            set
            {
                this.RaiseAndSetIfChanged (ref tIF, value, nameof (TextboxIsFocusable));
            }
        }

        private double dO;
        public double DropDownOpacity
        {
            get { return dO; }
            set
            {
                this.RaiseAndSetIfChanged (ref dO, value, nameof (DropDownOpacity));
            }
        }


        public PersonChoosingViewModel ( IUniformDocumentAssembler singleTypeDocumentAssembler )
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

        #region DropDown

        internal string HideDropDownWithChange ( )
        {
            string choosingResult;

            if ( _focused == null )
            {
                if ( ! _chosenPersonIsSetInSetter ) 
                {
                    ChosenPerson = null;
                    SetEntireListChoosingConsequences ();
                }

                choosingResult = _placeHolder;
            }
            else 
            {
                if ( ! _chosenPersonIsSetInSetter )
                {
                    ChosenPerson = _focused.Person;
                    SetPersonChoosingConsequences ();
                }

                choosingResult = ChosenPerson. StringPresentation;
            }

            _chosenPersonIsSetInSetter = false;
            string placeholder = HideDropDownWithoutChange ();

            if ( placeholder != null ) 
            {
                choosingResult = placeholder;
            }
            
            TryToEnableBadgeCreation ();
            
            return choosingResult;
        }


        internal string ? HideDropDownWithoutChange ()
        {
            string placeholder = null;

            if ( (InvolvedPeople. Count == 0)   &&   (PeopleStorage. Count > 0) ) 
            {
                RecoverVisiblePeople ();
                PlaceHolder = _placeHolder;
                FontWeight = FontWeight.Bold;
                placeholder = _placeHolder;
                PlaceHolder = _placeHolder;
            }

            DropDownOpacity = 0;
            VisibleHeight = 0;
            FirstItemHeight = 0;
            FirstIsVisible = false;

            return placeholder;
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
                TextboxIsFocusable = false;
                TextboxIsReadOnly = true;

                return;
            }

            List <VisiblePerson> peopleStorage = new ();
            List <VisiblePerson> involvedPeople = new ();

            foreach ( Person person in persons )
            {
                if (IsEmpty(person))
                {
                    continue;
                }

                VisiblePerson visiblePerson = new VisiblePerson (person);
                peopleStorage.Add (visiblePerson);
                involvedPeople.Add (visiblePerson);
            }

            PeopleStorage = peopleStorage;
            InvolvedPeople = involvedPeople;
        }


        internal bool IsEmpty ( Person person )
        {
            bool isEmpty = false;

            isEmpty = isEmpty   ||   ( string.IsNullOrWhiteSpace (person.FamilyName) )
                                ||   ( string.IsNullOrWhiteSpace (person.FamilyName) )
                                ||   ( string.IsNullOrWhiteSpace (person.FamilyName) )
                                ||   ( string.IsNullOrWhiteSpace (person.FamilyName) )
                                ||   ( string.IsNullOrWhiteSpace (person.FamilyName) );

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


        //internal void SetIntrusionInVisiblePeople ( )
        //{
        //    _personSourceVM. peopleSettingOccured = false;
        //}


        private void SetPersonList ( ) 
        {
            if ( (InvolvedPeople != null)   &&   (InvolvedPeople. Count > 0) )
            {
                int count = InvolvedPeople. Count;
                PersonListHeight = _oneHeight * count;
                bool listIsWhole = ( count == PeopleStorage. Count );

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

            TryToEnableBadgeCreation ();
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
            PlaceHolder = _placeHolder;

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


        private void TryToEnableBadgeCreation ()
        {
            if ( ( InvolvedPeople != null )   &&   ( InvolvedPeople. Count > 0 ) )
            {
                if ( _templateChoosingVM == null )
                {
                    _templateChoosingVM = App.services.GetRequiredService<TemplateChoosingViewModel> ();
                }

                BuildingIsPossible = ( SinglePersonIsSelected   ||   EntirePersonListIsSelected );

                if ( BuildingIsPossible   &&   ( _templateChoosingVM.ChosenTemplate != null ) )
                {
                    _templateChoosingVM.BuildingIsPossible = true;
                }
            }
        }


        internal void DisableBuildigPossibility ()
        {
            if ( _templateChoosingVM == null )
            {
                _templateChoosingVM = App.services.GetRequiredService<TemplateChoosingViewModel> ();
            }

            _templateChoosingVM.BuildingIsPossible = false;
        }


        internal void ToZeroPersonSelection ()
        {
            SinglePersonIsSelected = false;
            EntirePersonListIsSelected = false;

            if ( _personSourceVM == null )
            {
                _personSourceVM = App.services.GetRequiredService<PersonSourceViewModel> ();
            }

            _personSourceVM. peopleSettingOccured = false;
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
    }
}


