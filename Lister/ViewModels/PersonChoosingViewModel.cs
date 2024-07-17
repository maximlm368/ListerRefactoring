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
    public class PersonChoosingViewModel : ViewModelBase
    {
        private static double _withScroll = 454;
        private static double _withoutScroll = 469;
        private static double _upperHeight = 15;
        private static double _scrollingScratch = 25;
        private static string _placeHolder = "Весь список";
        private static int _maxVisibleCount = 4;
        private static double _oneHeight = 25;
        private static double _allListHeight = 25;
        private static readonly int _edge = 3;

        private static SolidColorBrush _entireListColor = new SolidColorBrush (new Avalonia.Media.Color (255, 255, 182, 193));
        private static SolidColorBrush _unfocusedColor = new SolidColorBrush (new Avalonia.Media.Color (255, 255, 255, 255));
        private static SolidColorBrush _focusedColor = new SolidColorBrush (new Avalonia.Media.Color (255, 0, 0, 0));

        private TemplateChoosingViewModel _templateChoosingVM;
        private PersonSourceViewModel _personSourceVM;
        private bool _allListMustBe = false;
        private double _widthDelta;
        private Timer _timer;
        private VisiblePerson _focused;
        private int _focusedNumber;
        private int _focusedEdge;
        private int _topLimit;
        private bool _personIsSetInSetter;
        internal bool ScrollingIsOccured { get; set; }
        internal bool SinglePersonIsSelected { get; private set; }
        internal bool EntirePersonListIsSelected { get; private set; }
        internal bool BuildingIsPossible { get; private set; }
        internal List <Person> People { get; set; }

        //private ObservableCollection <VisiblePerson> _preloadingShortVisiblePeople;
        private ObservableCollection <VisiblePerson> vP;
        internal ObservableCollection <VisiblePerson> VisiblePeople
        {
            get { return vP; }
            set
            {
                this.RaiseAndSetIfChanged ( ref vP , value , nameof ( VisiblePeople ) );
                SetPersonList ( );
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

                _personIsSetInSetter = true;

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
            ScrollerCanvasLeft = 454;
            VisiblePeople = new ObservableCollection <VisiblePerson> ();
            People = new List<Person> ();
            PersonsScrollValue = _oneHeight;
            TextboxIsReadOnly = false;
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
                if ( ! _personIsSetInSetter ) 
                {
                    ChosenPerson = null;
                    EntirePersonListIsSelected = true;
                    SinglePersonIsSelected = false;
                }

                choosingResult = _placeHolder;
            }
            else 
            {
                if ( ! _personIsSetInSetter )
                {
                    ChosenPerson = _focused.Person;
                    SinglePersonIsSelected = true;
                    EntirePersonListIsSelected = false;
                }

                if ( ChosenPerson == null ) 
                {
                    ChosenPerson = _focused.Person;
                }

                choosingResult = ChosenPerson. StringPresentation;
            }

            _personIsSetInSetter = false;
            HideDropDownWithoutChange ();
            //TryToEnableBadgeCreation ();
            
            return choosingResult;
        }


        internal string ? HideDropDownWithoutChange ()
        {
            string returnable = null;

            DropDownOpacity = 0;
            VisibleHeight = 0;
            FirstItemHeight = 0;
            FirstIsVisible = false;

            if ( ( VisiblePeople.Count == 0 ) && ( People.Count > 0 ) )
            {
                RecoverVisiblePeople (false);

                ChosenPerson = null;
                _focused = null;
                PlaceHolder = _placeHolder;
                EntireListColor = _focusedColor;
                _focusedNumber = _focusedEdge - _maxVisibleCount;
                returnable = _placeHolder;
            }

            return returnable;
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

        internal void SetChosenPerson ( Person person )
        {
            ChosenPerson = person;

            int seekingScratch = _focusedNumber - _maxVisibleCount;

            if ( seekingScratch < 0 )
            {
                seekingScratch = 0;
            }

            int seekingEnd = _focusedNumber + _maxVisibleCount;

            if ( seekingScratch > VisiblePeople.Count )
            {
                seekingScratch = VisiblePeople.Count;
            }

            for ( int index = seekingScratch;   index <= seekingEnd;   index++ )
            {
                VisiblePerson foundPerson = VisiblePeople [index];
                bool isCoincidence = ChosenPerson.Equals (foundPerson.Person);

                if ( isCoincidence )
                {
                    if ( _focused != null )
                    {
                        _focused.BrushColor = _unfocusedColor;
                    }
                    else
                    {
                        EntireListColor = _entireListColor;
                    }

                    _focused = foundPerson;
                    _focused.BrushColor = _focusedColor;
                    _focusedNumber = index;
                    break;
                }
            }
        }


        internal void SetEntireList ( )
        {
            ChosenPerson = null;

            if ( _focused != null )
            {
                _focused.BrushColor = _unfocusedColor;
                _focused = null;
            }

            PlaceHolder = _placeHolder;
            EntireListColor = _focusedColor;
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

            foreach ( VisiblePerson person   in   VisiblePeople ) 
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
            if ( VisiblePeople != null   &&   VisiblePeople. Count > 0 )
            {
                int count = VisiblePeople. Count;
                PersonListHeight = _oneHeight * count;
                bool listIsWhole = ( count == People. Count );

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


        internal void RecoverVisiblePeople ( bool shouldShow )
        {
            ObservableCollection <VisiblePerson> recovered = new ObservableCollection <VisiblePerson> ();

            foreach ( Person person in People )
            {
                VisiblePerson vP = new VisiblePerson (person);
                recovered.Add (vP);
            }

            VisiblePeople = recovered;

            if ( shouldShow ) 
            {
                ShowDropDown ();
            }
        }


        internal void SetEntireList ( int personCount )
        {
            //VisibleHeight = _oneHeight * ( Math.Min (_maxVisibleCount, personCount) + 1 );
            
            _visibleHeightStorage = _oneHeight * ( Math.Min (_maxVisibleCount, personCount) + 1 );
            FirstIsVisible = true;
            _allListMustBe = true;
            FirstItemHeight = _scrollingScratch;
            PersonsScrollValue = _scrollingScratch;

            _focused = null;
            EntireListColor = _focusedColor;
            _focusedNumber = -1;
            _focusedEdge = _edge;

            if ( _personSourceVM == null )
            {
                _personSourceVM = App.services.GetRequiredService<PersonSourceViewModel> ();
            }

            EntirePersonListIsSelected = true;
            EntireListColor = new SolidColorBrush (new Avalonia.Media.Color (255, 0, 0, 0));
            PlaceHolder = _placeHolder;
        }


        internal void SetCutDownList ( int personCount )
        {
            FirstItemHeight = 0;
            FirstIsVisible = false;
            _allListMustBe = false;
            
            //VisibleHeight = _oneHeight * Math.Min (_maxVisibleCount, personCount);
            
            _visibleHeightStorage = _oneHeight * Math.Min (_maxVisibleCount, personCount);
            EntirePersonListIsSelected = false;
            PersonsScrollValue = 0;

            _focusedNumber = 0;
            _focused = VisiblePeople [_focusedNumber];
            _focused.BrushColor = _focusedColor;
           _focusedEdge = _edge;
        }


        private void SetScrollerIfShould () 
        {
            int personCount = VisiblePeople. Count;

            if ( personCount > _maxVisibleCount )
            {
                PersonListWidth = _withScroll - _widthDelta;
                ScrollerWidth = _upperHeight;

                double scrollerWorkAreaHeight = _visibleHeightStorage - ( ScrollerWidth * 2 );
                double proportion = PersonListHeight / scrollerWorkAreaHeight;
                RunnerHeight = _visibleHeightStorage / proportion;
                RealRunnerHeight = RunnerHeight;

                if ( RunnerHeight < 2 )
                {
                    RunnerHeight = 2;
                }

                RunnerTopCoordinate = _upperHeight;
                TopSpanHeight = 0;
                BottomSpanHeight = scrollerWorkAreaHeight - RunnerHeight;
                IsPersonsScrollable = true;
            }
            else 
            {
                PersonListWidth = _withoutScroll - _widthDelta;
                ScrollerWidth = 0;
                IsPersonsScrollable = false;
            }
        }

        #region Scrolling

        internal void ScrollByWheel ( bool isDirectionUp, int count )
        {
            double step = _oneHeight;
            double proportion = 0;
            double runnerStep = 0;
            bool scrollerIsInvolved;

            if ( IsPersonsScrollable )
            {
                proportion = ( VisibleHeight ) / RealRunnerHeight;
                runnerStep = step / proportion;
                scrollerIsInvolved = true;
            }
            else 
            {
                scrollerIsInvolved = false;
            }

            CompleteScrolling (isDirectionUp, step, runnerStep, count, scrollerIsInvolved);
        }


        internal void ScrollByButton ( bool isDirectionUp, int count )
        {
            if ( IsPersonsScrollable )
            {
                TimerCallback callBack = new TimerCallback (ShiftCaller);
                double spanHeightLimit = 0;

                object [] args = new object [3];
                args [0] = isDirectionUp;
                args [1] = count;
                args [2] = spanHeightLimit;

                _timer = new Timer (callBack, args, 0, 100);
            }
        }


        private void ShiftCaller ( object args )
        {
            object [] directionAndCount = ( object [] ) args;
            bool isDirectionUp = ( bool ) directionAndCount [0];
            int count = ( int ) directionAndCount [1];
            double spanHeightLimit = ( double ) directionAndCount [2];

            double step = _oneHeight;
            double proportion = VisibleHeight / RealRunnerHeight;
            double runnerStep = step / proportion;

            bool isTimeToStop = false;

            if ( isDirectionUp )
            {
                if ( TopSpanHeight <= spanHeightLimit )
                {
                    StopScrolling ();
                }
            }
            else
            {
                if ( BottomSpanHeight <= spanHeightLimit )
                {
                    StopScrolling ();
                }
            }

            CompleteScrolling (isDirectionUp, step, runnerStep, count, true);
        }


        internal void StopScrolling ()
        {
            if ( _timer != null )
            {
                _timer.Dispose ();
            }
        }


        internal void ShiftRunner ( bool isDirectionUp, int count, double limit )
        {
            if ( IsPersonsScrollable )
            {
                while ( true ) 
                {
                    double step = _oneHeight;
                    double proportion = VisibleHeight / RealRunnerHeight;
                    double runnerStep = step / proportion;

                    bool isTimeToStop = false;

                    if ( isDirectionUp )
                    {
                        if ( TopSpanHeight < limit )
                        {
                            break;
                        }
                    }
                    else
                    {
                        if ( BottomSpanHeight < limit )
                        {
                            break;
                        }
                    }

                    CompleteScrolling (isDirectionUp, step, runnerStep, count, true);
                }
            }
        }


        internal void MoveRunner ( double runnerStep, int count )
        {
            double proportion = VisibleHeight / RealRunnerHeight;
            double step = runnerStep * proportion;
            step = _oneHeight * Math.Round( step / _oneHeight );
            bool isDirectionUp = false;

            if ( step > 0 ) 
            {
                isDirectionUp = true;
                runnerStep = step / proportion;
            }
            else if( step < 0 ) 
            {
                isDirectionUp = false;
                step = step * ( -1 );
                //runnerStep = runnerStep * ( -1 );
                runnerStep = step / proportion;
            }

            CompleteMoving (isDirectionUp, step, runnerStep, count, true);
        }


        internal void ScrollByKey ( bool isDirectionUp , int count )
        {
            double step = _oneHeight;
            double proportion = 0;
            double runnerStep = 0;
            bool scrollerIsInvolved;

            if ( IsPersonsScrollable )
            {
                proportion = ( VisibleHeight ) / RealRunnerHeight;
                runnerStep = step / proportion;
                scrollerIsInvolved = true;
            }
            else 
            {
                scrollerIsInvolved = false;
            }

            CompleteScrolling (isDirectionUp, step, runnerStep, count, scrollerIsInvolved);
        }


        private void CompleteScrolling ( bool isDirectionUp , double step , double runnerStep, int itemCount
                                       , bool scrollerIsInvolved )
        {
            double currentPersonsScrollValue = PersonsScrollValue;

            if ( isDirectionUp )
            {
                if ( _focused != null )
                {
                    _focused.BrushColor = _unfocusedColor;
                }

                if ( _allListMustBe )
                {
                    bool focusedIsInRange = _focusedNumber > -1;

                    if ( focusedIsInRange )
                    {
                        _focusedNumber--;

                        if ( _focusedNumber < ( _focusedEdge - _maxVisibleCount + 1 ) )
                        {
                            EntireListColor = _focusedColor;
                            _focused = null;

                            if ( _focusedNumber < ( _focusedEdge - _maxVisibleCount + 0 ) )
                            {
                                currentPersonsScrollValue += step;
                                double scrollingLimit = GetScrollLimit ();

                                if ( currentPersonsScrollValue > scrollingLimit )
                                {
                                    currentPersonsScrollValue = scrollingLimit;
                                }

                                if (scrollerIsInvolved) 
                                {
                                    UpRunner (runnerStep);
                                }

                                _focusedEdge--;
                            }
                        }
                    }
                }
                else 
                {
                    bool focusedIsInRange = _focusedNumber > 0;

                    if ( focusedIsInRange )
                    {
                        _focusedNumber--;

                        if ( _focusedNumber < ( _focusedEdge - _maxVisibleCount + 1 ) )
                        {
                            currentPersonsScrollValue += step;
                            double scrollingLimit = GetScrollLimit ();

                            if ( currentPersonsScrollValue > scrollingLimit )
                            {
                                currentPersonsScrollValue = scrollingLimit;
                            }

                            if (scrollerIsInvolved) 
                            {
                                UpRunner (runnerStep);
                            }

                            _focusedEdge--;
                        }
                    }

                }

                if ( (_focusedNumber > -1)   &&   (_focused != null) )
                {
                    _focused = VisiblePeople [_focusedNumber];
                    _focused.BrushColor = _focusedColor;
                }
            }
            else
            {
                bool focusedIsInRange = (_focusedNumber < (VisiblePeople. Count - 1));

                if ( focusedIsInRange )
                {
                    _focusedNumber++;
                    EntireListColor = _entireListColor;

                    if ( _focused != null )
                    {
                        _focused.BrushColor = _unfocusedColor;
                    }

                    _focused = VisiblePeople [_focusedNumber];
                    _focused.BrushColor = _focusedColor;


                    if ( _focusedNumber > _focusedEdge )
                    {
                        _focusedEdge++;

                        double itemHeight = _oneHeight;
                        currentPersonsScrollValue -= step;
                        double listHeight = itemHeight * itemCount;
                        double maxScroll = VisibleHeight - listHeight;
                        bool scrollExceeds = ( currentPersonsScrollValue < maxScroll );

                        if ( scrollExceeds )
                        {
                            currentPersonsScrollValue = maxScroll;
                        }

                        if ( scrollerIsInvolved ) 
                        {
                            DownRunner (runnerStep);
                        }
                    }
                }
            }

            PersonsScrollValue = currentPersonsScrollValue;
            ScrollingIsOccured = true;
        }


        private void CompleteMoving ( bool isDirectionUp, double step, double runnerStep, int itemCount
                                    , bool scrollerIsInvolved )
        {
            double currentPersonsScrollValue = PersonsScrollValue;

            if ( isDirectionUp )
            {
                int visibleCount = _maxVisibleCount - 1;
                int topBorder = 0;

                if ( _allListMustBe )
                {
                    visibleCount = _maxVisibleCount;
                    topBorder = -1;
                }
                
                double stepLoss = 0;
                int focusedMustStepAlone = visibleCount - ( _focusedEdge - _focusedNumber );
                int stepInItems = ( int ) ( step / _oneHeight );

                bool onlyFocusedWillShift = ( stepInItems <= focusedMustStepAlone );

                if ( onlyFocusedWillShift )
                {
                    focusedMustStepAlone = stepInItems;
                    _focusedNumber -= stepInItems;
                }
                else
                {
                    stepLoss = ( _oneHeight * focusedMustStepAlone );
                    _focusedNumber -= stepInItems;
                    step -= stepLoss;

                    bool isTopViolation = ( _focusedNumber < topBorder );

                    if ( isTopViolation )
                    {
                        int excess = _focusedNumber - topBorder;
                        step -= excess * _oneHeight;
                        _focusedNumber = topBorder;
                    }

                    if ( _focused != null )
                    {
                        _focused.BrushColor = _unfocusedColor;
                    }

                    if ( _allListMustBe   &&   (_focusedNumber < ( _focusedEdge - visibleCount + 1 )) )
                    {
                        EntireListColor = _focusedColor;
                        _focused = null;
                    }

                    if ( _focusedNumber < ( _focusedEdge - visibleCount ) )
                    {
                        currentPersonsScrollValue += step;
                        double scrollingLimit = GetScrollLimit ();

                        if ( currentPersonsScrollValue > scrollingLimit )
                        {
                            currentPersonsScrollValue = scrollingLimit;
                        }

                        if ( scrollerIsInvolved )
                        {
                            UpRunner (runnerStep);
                        }

                        _focusedEdge = _focusedNumber + visibleCount;
                    }
                }

                if ( ( _focusedNumber > -1 )   &&   ( _focused != null ) )
                {
                    _focused = VisiblePeople [_focusedNumber];
                    _focused.BrushColor = _focusedColor;
                }
            }
            else
            {
                if ( _allListMustBe ) 
                {
                    EntireListColor = _entireListColor;
                }

                double stepLoss = 0;
                int focusedMustStepAlone = _focusedEdge - _focusedNumber;
                int stepInItems = ( int ) ( step / _oneHeight );

                bool onlyFocusedWillShift = ( stepInItems <= focusedMustStepAlone );

                if ( onlyFocusedWillShift )
                {
                    focusedMustStepAlone = stepInItems;
                    _focusedNumber += stepInItems;
                }
                else 
                {
                    stepLoss = ( _oneHeight * focusedMustStepAlone );
                    _focusedNumber += stepInItems;
                    step -= stepLoss;

                    bool isBottomViolation = (_focusedNumber > ( VisiblePeople. Count - 1 ));

                    if ( isBottomViolation )
                    {
                        int excess = _focusedNumber - ( VisiblePeople. Count - 1 );
                        step -= excess * _oneHeight;
                        _focusedNumber = ( VisiblePeople. Count - 1 );
                    }

                    if ( _focused != null )
                    {
                        _focused.BrushColor = _unfocusedColor;
                    }

                    _focused = VisiblePeople [_focusedNumber];
                    _focused.BrushColor = _focusedColor;

                    if ( _focusedNumber > _focusedEdge )
                    {
                        currentPersonsScrollValue -= step;

                        if ( scrollerIsInvolved )
                        {
                            DownRunner (runnerStep);
                        }

                        _focusedEdge = _focusedNumber;
                    }
                }
            }

            PersonsScrollValue = currentPersonsScrollValue;
            ScrollingIsOccured = true;
        }


        private double GetScrollLimit () 
        {
            double scrollingLimit = 0;
            
            if ( VisiblePeople.Count == People.Count )
            {
                scrollingLimit = _scrollingScratch;
            }

            return scrollingLimit;
        }


        private void UpRunner ( double runnerStep )
        {
            RunnerTopCoordinate -= runnerStep;

            if ( RunnerTopCoordinate < _upperHeight )
            {
                RunnerTopCoordinate = _upperHeight;
            }

            TopSpanHeight -= runnerStep;

            if ( TopSpanHeight < 0 )
            {
                TopSpanHeight = 0;
            }

            BottomSpanHeight += runnerStep;

            double maxHeight = VisibleHeight - _upperHeight - RunnerHeight - _upperHeight;

            if ( BottomSpanHeight > maxHeight )
            {
                BottomSpanHeight = maxHeight;
            }
        }


        private void DownRunner ( double runnerStep )
        {
            TopSpanHeight += runnerStep;

            double maxHeight = VisibleHeight - _upperHeight - RunnerHeight - _upperHeight;

            if ( TopSpanHeight > maxHeight )
            {
                TopSpanHeight = maxHeight;
            }

            RunnerTopCoordinate += runnerStep;

            double maxRunnerTopCoord = _upperHeight + TopSpanHeight;

            if ( RunnerTopCoordinate > maxRunnerTopCoord )
            {
                RunnerTopCoordinate = maxRunnerTopCoord;
            }

            BottomSpanHeight -= runnerStep;

            if ( BottomSpanHeight < 0 )
            {
                BottomSpanHeight = 0;
            }
        }

        #endregion Scrolling


        private void TryToEnableBadgeCreation ()
        {
            if ( ( VisiblePeople != null )   &&   ( VisiblePeople. Count > 0 ) )
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
    }
}


