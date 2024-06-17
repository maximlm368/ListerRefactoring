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

namespace Lister.ViewModels
{
    public class PersonChoosingViewModel : ViewModelBase
    {
        private static double _withScroll = 454;
        private static double _withoutScroll = 469;
        private static double _upperHeight = 15;
        private static double _scrollingScratch = 24;
        private double _widthDelta;
        private Timer _timer;
        internal bool ScrollingIsOccured { get; set; }

        private double _oneHeight;
        internal List <Person> People { get; set; }

        private ObservableCollection <Person> vP;
        internal ObservableCollection <Person> VisiblePeople
        {
            get { return vP; }
            set
            {
                this.RaiseAndSetIfChanged ( ref vP , value , nameof ( VisiblePeople ) );
                SetPersonList ();
            }
        }

        private Person cP;
        internal Person ChosenPerson
        {
            get { return cP; }
            set
            {
                this.RaiseAndSetIfChanged ( ref cP , value , nameof ( ChosenPerson ) );
            }
        }

        private double vH;
        internal double VisibleHeight
        {
            get { return vH; }
            set
            {
                this.RaiseAndSetIfChanged (ref vH, value, nameof (VisibleHeight));
            }
        }

        private double plW;
        internal double PersonListWidth
        {
            get { return plW; }
            set
            {
                this.RaiseAndSetIfChanged (ref plW, value, nameof (PersonListWidth));
            }
        }

        private double plH;
        internal double PersonListHeight
        {
            get { return plH; }
            set
            {
                this.RaiseAndSetIfChanged (ref plH, value, nameof (PersonListHeight));
            }
        }

        private double psV;
        internal double PersonsScrollValue
        {
            get { return psV; }
            set
            {
                this.RaiseAndSetIfChanged (ref psV, value, nameof (PersonsScrollValue));
            }
        }

        internal double RealRunnerHeight { get; private set; }
        private double rH;
        internal double RunnerHeight
        {
            get { return rH; }
            set
            {
                this.RaiseAndSetIfChanged (ref rH, value, nameof (RunnerHeight));
            }
        }

        private double rTC;
        internal double RunnerTopCoordinate
        {
            get { return rTC; }
            set
            {
                this.RaiseAndSetIfChanged (ref rTC, value, nameof (RunnerTopCoordinate));
            }
        }

        private double tSH;
        internal double TopSpanHeight
        {
            get { return tSH; }
            set
            {
                this.RaiseAndSetIfChanged (ref tSH, value, nameof (TopSpanHeight));
            }
        }

        private double bSH;
        internal double BottomSpanHeight
        {
            get { return bSH; }
            set
            {
                this.RaiseAndSetIfChanged (ref bSH, value, nameof (BottomSpanHeight));
            }
        }

        private double sW;
        internal double ScrollerWidth
        {
            get { return sW; }
            set
            {
                this.RaiseAndSetIfChanged (ref sW, value, nameof (ScrollerWidth));
            }
        }

        private double sCL;
        internal double ScrollerCanvasLeft
        {
            get { return sCL; }
            set
            {
                this.RaiseAndSetIfChanged (ref sCL, value, nameof (ScrollerCanvasLeft));
            }
        }

        private double fH;
        internal double FirstItemHeight
        {
            get { return fH; }
            set
            {
                this.RaiseAndSetIfChanged (ref fH, value, nameof (FirstItemHeight));
            }
        }

        private bool fV;
        public bool FirstIsVisible
        {
            get { return fV; }
            set
            {
                this.RaiseAndSetIfChanged (ref fV, value, nameof (FirstIsVisible));
            }
        }

        private bool isPS;
        public bool IsPersonsScrollable
        {
            get { return isPS; }
            set
            {
                this.RaiseAndSetIfChanged (ref isPS, value, nameof (IsPersonsScrollable));
            }
        }


        public PersonChoosingViewModel ( IUniformDocumentAssembler singleTypeDocumentAssembler )
        {
            _oneHeight = 24;
            ScrollerCanvasLeft = 454;
            VisiblePeople = new ObservableCollection<Person> ();
            People = new List<Person> ();
            PersonsScrollValue = _oneHeight;
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

            foreach ( Person person   in   VisiblePeople ) 
            {
                bool isIntresting = person.IsMatchingTo (presentation);
                
                if ( isIntresting ) 
                {
                    result = person;
                    break;
                }
            }

            return result;
        }


        private void SetPersonList ( ) 
        {
            int personCount = VisiblePeople. Count;
            PersonListHeight = _oneHeight * personCount;

            if ( personCount > 4 )
            {
                ShowScroller ();
            }
            else
            {
                FirstItemHeight = 0;
                FirstIsVisible = false;
                VisibleHeight = _oneHeight * personCount;
                PersonListWidth = _withoutScroll - _widthDelta;
                ScrollerWidth = 0;
                IsPersonsScrollable = false;
                PersonsScrollValue = 0;
            }
        }


        private void ShowScroller () 
        {
            VisibleHeight = _oneHeight * 5;
            PersonListWidth = _withScroll - _widthDelta;
            ScrollerWidth = 15;

            double scrollerWorkAreaHeight = VisibleHeight - (ScrollerWidth * 2);
            double proportion = PersonListHeight / scrollerWorkAreaHeight;
            RunnerHeight = VisibleHeight / proportion;
            RealRunnerHeight = RunnerHeight;

            if (RunnerHeight < 2) 
            {
                RunnerHeight = 2;
            }

            RunnerTopCoordinate = 15;
            TopSpanHeight = 0;
            BottomSpanHeight = scrollerWorkAreaHeight - RunnerHeight;
            IsPersonsScrollable = true;

            bool listIsWhole = (VisiblePeople. Count == People. Count);

            if ( listIsWhole ) 
            {
                FirstIsVisible = true;
                FirstItemHeight = PersonChoosingUserControl.AllPersonsSignHeight;

                if (! ScrollingIsOccured) 
                {
                    PersonsScrollValue = PersonChoosingUserControl.AllPersonsSignHeight;
                }
            }
        }

        #region Scrolling

        internal void ScrollByWheel ( bool isDirectionUp, int count )
        {
            if ( IsPersonsScrollable )
            {
                double step = 24;
                double proportion = VisibleHeight / RealRunnerHeight;
                double runnerStep = step / proportion;

                CompleteScrolling ( isDirectionUp , step , runnerStep , count );
            }
        }


        internal void ScrollByButton ( bool isDirectionUp, int count )
        {
            if ( IsPersonsScrollable )
            {
                TimerCallback callBack = new TimerCallback ( ShiftCaller );

                object [ ] args = new object [ 2 ];
                args [ 0 ] = isDirectionUp;
                args [ 1 ] = count;

                _timer = new Timer ( callBack , args, 0 , 100 );
            }
        }


        private void ShiftCaller ( object args )
        {
            object [ ] directionAndCount = ( object [ ] ) args;
            bool isDirectionUp = ( bool ) directionAndCount [ 0 ];
            int count = ( int ) directionAndCount [ 1 ];

            double step = 24;
            double proportion = VisibleHeight / RealRunnerHeight;
            double runnerStep = step / proportion;

            CompleteScrolling ( isDirectionUp , step , runnerStep , count );
        }


        internal void StopScrolling ( )
        {
            if ( IsPersonsScrollable )
            {
                _timer.Dispose ( );
            }
        }


        internal void ShiftRunner ( bool isDirectionUp , int count )
        {
            if ( IsPersonsScrollable )
            {
                TimerCallback callBack = new TimerCallback ( ShiftCaller );

                object [ ] args = new object [ 2 ];
                args [ 0 ] = isDirectionUp;
                args [ 1 ] = count;

                _timer = new Timer ( callBack , args , 0 , 20 );
            }
        }


        internal void MoveRunner ( double runnerVerticalDelta, int count )
        {
            double proportion = VisibleHeight / RealRunnerHeight;
            double personsVerticalDelta = runnerVerticalDelta * proportion;
            bool isDirectionUp = ( runnerVerticalDelta > 0 );
            CompleteScrolling ( isDirectionUp , personsVerticalDelta , runnerVerticalDelta, count );
        }


        internal void ScrollByKey ( bool isDirectionUp , int count )
        {
            if ( IsPersonsScrollable )
            {
                double itemHeight = 24;
                double proportion = VisibleHeight / RealRunnerHeight;
                double wholeSpan = TopSpanHeight + BottomSpanHeight - RunnerHeight;
                double runnerStep = wholeSpan / count;
                CompleteScrolling ( isDirectionUp , itemHeight , runnerStep, count );
            }
        }


        private void CompleteScrolling ( bool isDirectionUp , double step , double runnerStep, int count )
        {
            if ( ScrollerWidth == 0 )
                return;

            double currentPersonsScrollValue = PersonsScrollValue;

            if ( isDirectionUp )
            {
                currentPersonsScrollValue += step;
                double scrollingScratch = 0;

                if ( VisiblePeople.Count == People.Count )
                {
                    scrollingScratch = _scrollingScratch;
                }
                else 
                {
                    scrollingScratch = 0;
                }

                if ( currentPersonsScrollValue > scrollingScratch )
                {
                    currentPersonsScrollValue = scrollingScratch;
                }

                UpRunner ( runnerStep );
            }
            else
            {
                double itemHeight = _scrollingScratch;
                currentPersonsScrollValue -= step;
                double listHeight = itemHeight * count;
                double maxScroll = VisibleHeight - listHeight;
                bool scrollExceeds = ( currentPersonsScrollValue < maxScroll );

                if ( scrollExceeds )
                {
                    currentPersonsScrollValue = maxScroll;
                }

                DownRunner ( runnerStep );
            }

            PersonsScrollValue = currentPersonsScrollValue;
            ScrollingIsOccured = true;
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
    }
}




//private double tS;
//internal double PeopleTopShift
//{
//    get { return tS; }
//    set
//    {
//        this.RaiseAndSetIfChanged (ref tS, value, nameof (PeopleTopShift));
//    }
//}


//  private double plH = 89;