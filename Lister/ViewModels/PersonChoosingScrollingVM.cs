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
        #region Scrolling

        private double _scrollValue;


        internal void ScrollByWheel ( bool isDirectionUp )
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

            CompleteScrolling (isDirectionUp, step, runnerStep, scrollerIsInvolved);
        }


        internal void ScrollByButton ( bool isDirectionUp, int count )
        {
            if ( IsPersonsScrollable )
            {
                TimerCallback callBack = new TimerCallback (ShiftCaller);
                double spanHeightLimit = 0;

                object [] args = new object [2];
                args [0] = isDirectionUp;
                args [1] = spanHeightLimit;

                _timer = new Timer (callBack, args, 0, 100);
            }
        }


        private void ShiftCaller ( object args )
        {
            object [] directionAndCount = ( object [] ) args;
            bool isDirectionUp = ( bool ) directionAndCount [0];
            double spanHeightLimit = ( double ) directionAndCount [1];

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

            CompleteScrolling (isDirectionUp, step, runnerStep, true);
        }


        internal void StopScrolling ()
        {
            if ( _timer != null )
            {
                _timer.Dispose ();
            }
        }


        internal void ShiftRunner ( bool isDirectionUp, double limit )
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

                    CompleteScrolling (isDirectionUp, step, runnerStep, true);
                }
            }
        }


        internal void MoveRunner ( double runnerStep )
        {
            double proportion = VisibleHeight / RealRunnerHeight;
            double step = runnerStep * proportion;
            step = _oneHeight * Math.Round (step / _oneHeight);
            bool isDirectionUp = false;

            if ( step > 0 )
            {
                isDirectionUp = true;
                runnerStep = step / proportion;
            }
            else if ( step < 0 )
            {
                isDirectionUp = false;
                step = step * ( -1 );
                //runnerStep = runnerStep * ( -1 );
                runnerStep = step / proportion;
            }

            CompleteMoving (isDirectionUp, step, runnerStep, true);
        }


        internal void ScrollByKey ( bool isDirectionUp )
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

            CompleteScrolling (isDirectionUp, step, runnerStep, scrollerIsInvolved);
        }


        private void CompleteScrolling ( bool isDirectionUp, double step, double runnerStep, bool scrollerIsInvolved )
        {
            double currentScrollValue = _scrollValue;

            if ( isDirectionUp )
            {
                if ( _focused != null )
                {
                    _focused.BorderBrushColor = _unfocusedColor;
                }

                if ( _allListMustBe )
                {
                    bool focusedIsInRange = _focusedNumber > -1;

                    if ( focusedIsInRange )
                    {
                        _focusedNumber--;

                        if ( _focusedNumber < ( _focusedEdge - _maxVisibleCount + 1 ) )
                        {
                            EntireListColor = _focusedBorderColor;
                            _focused = null;

                            if ( _focusedNumber < ( _focusedEdge - _maxVisibleCount + 0 ) )
                            {
                                currentScrollValue += step;
                                double scrollingLimit = GetScrollLimit ();

                                if ( currentScrollValue > scrollingLimit )
                                {
                                    currentScrollValue = scrollingLimit;
                                }

                                if ( scrollerIsInvolved )
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
                            currentScrollValue += step;
                            double scrollingLimit = GetScrollLimit ();

                            if ( currentScrollValue > scrollingLimit )
                            {
                                currentScrollValue = scrollingLimit;
                            }

                            if ( scrollerIsInvolved )
                            {
                                UpRunner (runnerStep);
                            }

                            _focusedEdge--;
                        }
                    }

                }

                if ( ( _focusedNumber > -1 ) && ( _focused != null ) )
                {
                    _focused = InvolvedPeople [_focusedNumber];
                    _focused.BorderBrushColor = _focusedBorderColor;
                }
            }
            else
            {
                bool focusedIsInRange = ( _focusedNumber < ( InvolvedPeople. Count - 1 ) );

                if ( focusedIsInRange )
                {
                    _focusedNumber++;
                    EntireListColor = _entireListColor;

                    if ( _focused != null )
                    {
                        _focused.BorderBrushColor = _unfocusedColor;
                    }

                    _focused = InvolvedPeople [_focusedNumber];
                    _focused.BorderBrushColor = _focusedBorderColor;


                    if ( _focusedNumber > _focusedEdge )
                    {
                        _focusedEdge++;

                        double itemHeight = _oneHeight;
                        currentScrollValue -= step;

                        //double listHeight = itemHeight * (InvolvedPeople. Count);
                        //double maxScroll = VisibleHeight - listHeight;
                        //bool scrollExceeds = ( currentScrollValue < maxScroll );

                        //if ( scrollExceeds )
                        //{
                        //    currentScrollValue = maxScroll;
                        //}

                        if ( scrollerIsInvolved )
                        {
                            DownRunner (runnerStep);
                        }
                    }
                }
            }

            _scrollValue = currentScrollValue;
            SetVisiblePeople ();
            ScrollingIsOccured = true;
        }


        private void CompleteMoving ( bool isDirectionUp, double step, double runnerStep, bool scrollerIsInvolved )
        {
            double currentScrollValue = _scrollValue;

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
                        _focused.BorderBrushColor = _unfocusedColor;
                    }

                    if ( _allListMustBe && ( _focusedNumber < ( _focusedEdge - visibleCount + 1 ) ) )
                    {
                        EntireListColor = _focusedBorderColor;
                        _focused = null;
                    }

                    if ( _focusedNumber < ( _focusedEdge - visibleCount ) )
                    {
                        currentScrollValue += step;
                        double scrollingLimit = GetScrollLimit ();

                        if ( currentScrollValue > scrollingLimit )
                        {
                            currentScrollValue = scrollingLimit;
                        }

                        if ( scrollerIsInvolved )
                        {
                            UpRunner (runnerStep);
                        }

                        _focusedEdge = _focusedNumber + visibleCount;
                    }
                }

                if ( ( _focusedNumber > -1 ) && ( _focused != null ) )
                {
                    _focused = InvolvedPeople [_focusedNumber];
                    _focused.BorderBrushColor = _focusedBorderColor;
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

                    bool isBottomViolation = ( _focusedNumber > ( InvolvedPeople. Count - 1 ) );

                    if ( isBottomViolation )
                    {
                        int excess = _focusedNumber - ( InvolvedPeople. Count - 1 );
                        step -= excess * _oneHeight;
                        _focusedNumber = ( InvolvedPeople. Count - 1 );
                    }

                    if ( _focused != null )
                    {
                        _focused.BorderBrushColor = _unfocusedColor;
                    }

                    _focused = InvolvedPeople [_focusedNumber];
                    _focused.BorderBrushColor = _focusedBorderColor;

                    if ( _focusedNumber > _focusedEdge )
                    {
                        currentScrollValue -= step;

                        if ( scrollerIsInvolved )
                        {
                            DownRunner (runnerStep);
                        }

                        _focusedEdge = _focusedNumber;
                    }
                }
            }

            _scrollValue = currentScrollValue;
            SetVisiblePeople ();
            ScrollingIsOccured = true;
        }


        private void SetVisiblePeople ()
        {
            int allListLineAddition = 0;

            if ( _allListMustBe )
            {
                allListLineAddition++;
            }

            int shiftInLines = (int) (_scrollValue / _oneHeight) * (-1);

            if ( shiftInLines < 0 )
            {
                shiftInLines = 0;
                _scrollValue = 0;
            }

            SetVisiblePeople ( shiftInLines );
        }


        private double GetScrollLimit ()
        {
            double scrollingLimit = 0;

            if ( InvolvedPeople. Count == PeopleStorage. Count )
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




    }
}
