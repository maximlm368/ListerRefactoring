using ReactiveUI;

namespace View.MainWindow.MainView.Parts.PersonChoosing.ViewModel;

public partial class PersonChoosingViewModel : ReactiveObject
{
    #region Scrolling

    private double _scrollValue;
    private double _runnerStep;
    private double _scrollingLength;

    private bool _entireIsFocused;
    internal bool EntireIsFocused 
    {
        get { return _entireIsFocused; }
        private set 
        {
            _entireIsFocused = value;

            if ( value )
            {
                EntireBackgroundColor = _focusedBackgroundColor;
                EntireBorderColor = _focusedBorderColor;
            }
            else 
            {
                if ( EntireIsSelected )
                {
                    EntireBackgroundColor = _selectedBackgroundColor;
                    EntireBorderColor = _selectedBorderColor;
                    EntireForegroundColor = _selectedForegroundColor;
                }
                else 
                {
                    EntireBackgroundColor = _defaultBackgroundColor;
                    EntireBorderColor = _defaultBorderColor;
                    EntireForegroundColor = _defaultForegroundColor;
                }
            }

        }
    }


    internal void ScrollByWheel ( bool isDirectionUp )
    {
        double step = _oneHeight;
        double runnerStep = 0;
        bool scrollerIsInvolved;

        if ( IsPersonsScrollable )
        {
            runnerStep = _runnerStep;
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
        double runnerStep = _runnerStep;

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
                double runnerStep = _runnerStep;

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
        double listHeight = _oneHeight * InvolvedPeople. Count;
        listHeight = listHeight - VisibleHeight;
        double proportion = listHeight / _scrollingLength;
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
            runnerStep = step / proportion;
        }

        CompleteMoving (isDirectionUp, step, runnerStep, true);
    }


    internal void ScrollByKey ( bool isDirectionUp )
    {
        double step = _oneHeight;
        double runnerStep = 0;
        bool scrollerIsInvolved;

        if ( IsPersonsScrollable )
        {
            runnerStep = _runnerStep;
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
                _focused.IsFocused = false;
            }

            if ( _allListMustBe )
            {
                bool focusedIsInRange = _focusedNumber > -1;

                if ( focusedIsInRange )
                {
                    _focusedNumber--;

                    if ( _focusedNumber < ( _focusedEdge - _maxVisibleCount + 1 ) )
                    {
                        EntireIsFocused = true;
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
                _focused.IsFocused = true;
            }
        }
        else
        {
            if ( _choiceIsAbsent )
            {
                EntireIsFocused = true;
                _choiceIsAbsent = false;

                return;
            }

            bool focusedIsInRange = ( _focusedNumber < ( InvolvedPeople. Count - 1 ) );

            if ( focusedIsInRange )
            {
                _focusedNumber++;
                EntireIsFocused = false;

                if ( _focused != null )
                {
                    _focused.IsFocused = false;
                }

                _focused = InvolvedPeople [_focusedNumber];
                _focused.IsFocused = true;

                if ( _focusedNumber > _focusedEdge )
                {
                    _focusedEdge++;

                    double itemHeight = _oneHeight;
                    currentScrollValue -= step;

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
                    _focused.IsFocused = false;
                }

                if ( _allListMustBe && ( _focusedNumber < ( _focusedEdge - visibleCount + 1 ) ) )
                {
                    EntireIsFocused = true;
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
                _focused.IsFocused = true;
            }
        }
        else
        {
            if ( _allListMustBe )
            {
                EntireIsFocused = false;
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
                    _focused.IsFocused = false;
                }

                _focused = InvolvedPeople [_focusedNumber];
                _focused.IsFocused = true;

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

        SetVisiblePeopleStartingFrom ( shiftInLines );
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
        RunnerYCoordinate -= runnerStep;

        if ( RunnerYCoordinate < _upperHeight )
        {
            RunnerYCoordinate = _upperHeight;
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

        RunnerYCoordinate += runnerStep;
        double maxRunnerTopCoord = _upperHeight + TopSpanHeight;

        if ( RunnerYCoordinate > maxRunnerTopCoord )
        {
            RunnerYCoordinate = maxRunnerTopCoord;
        }

        BottomSpanHeight -= runnerStep;

        if ( BottomSpanHeight < 0 )
        {
            BottomSpanHeight = 0;
        }
    }

    #endregion Scrolling
}
