using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Avalonia;

namespace Lister.ViewModels
{
    public partial class BadgeEditorViewModel : ViewModelBase
    {
        private int _numberAmongVisibleIcons = 1;

        private bool _upDownIsVisible;
        internal bool UpDownIsVisible
        {
            get { return _upDownIsVisible; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _upDownIsVisible, value, nameof (UpDownIsVisible));
            }
        }

        private Thickness _sliderMargin;
        internal Thickness SliderMargin
        {
            get { return _sliderMargin; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _sliderMargin, value, nameof (SliderMargin));
            }
        }

        private bool _firstIsEnable;
        internal bool FirstIsEnable
        {
            get { return _firstIsEnable; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _firstIsEnable, value, nameof (FirstIsEnable));
            }
        }

        private bool _previousIsEnable;
        internal bool PreviousIsEnable
        {
            get { return _previousIsEnable; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _previousIsEnable, value, nameof (PreviousIsEnable));
            }
        }

        private bool _nextIsEnable;
        internal bool NextIsEnable
        {
            get { return _nextIsEnable; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _nextIsEnable, value, nameof (NextIsEnable));
            }
        }

        private bool _lastIsEnable;
        internal bool LastIsEnable
        {
            get { return _lastIsEnable; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _lastIsEnable, value, nameof (LastIsEnable));
            }
        }

        private int _beingProcessedNumber;
        internal int BeingProcessedNumber
        {
            get { return _beingProcessedNumber; }
            private set
            {
                if ( value < 1 )
                {
                    BeingProcessedNumberText = string.Empty;
                }
                else 
                {
                    BeingProcessedNumberText = value.ToString ();
                }

                this.RaiseAndSetIfChanged (ref _beingProcessedNumber, value, nameof (BeingProcessedNumber));
            }
        }

        private string _beingProcessedNumberText;
        internal string BeingProcessedNumberText
        {
            get { return _beingProcessedNumberText; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _beingProcessedNumberText, value, nameof (BeingProcessedNumberText));
            }
        }


        internal void ToFirst ()
        {
            ControlIncorrectCountValue ();

            int absoluteNumber = BeingProcessedNumber + 1;
            bool isProcessableChangedInSpecificFilter = IsProcessableChangedInSpecificFilter (BeingProcessedNumber);

            if ( isProcessableChangedInSpecificFilter )
            {
                ReduceCurrentCollection (BeingProcessedNumber - 1);

                if ( CurrentVisibleCollection.Count <= _maxVisibleCount )
                {
                    _visibleRange = CurrentVisibleCollection.Count;
                    _visibleRangeEnd = CurrentVisibleCollection.Count - 1;
                    ScrollWidth = 0;
                }
            }

            VisibleIcons = new ObservableCollection <BadgeCorrectnessViewModel> ();
            _visibleIconsStorage = VisibleIcons;
            BeingProcessedNumber = 1;

            for ( int index = 0;   index < _visibleRange;   index++ )
            {
                BadgeViewModel boundBadge = CurrentVisibleCollection.ElementAt (index).Value;

                VisibleIcons.Add (new BadgeCorrectnessViewModel (boundBadge.IsCorrect, boundBadge, _correctnessWidthLimit
                                                  , new int [2] { _minCorrectnessTextLength, _maxCorrectnessTextLength }));

                if ( index == 0 ) 
                {
                    BeingProcessedBadge = GetAppropriateDraft (BeingProcessedNumber);
                }
            }

            _numberAmongVisibleIcons = 1;

            SetToCorrectScale (BeingProcessedBadge);
            BeingProcessedBadge.Show ();

            ScrollOffset = 0;
            _scrollStepIndex = 0;
            _runnerHasWalked = 0;
            _visibleRangeEnd = _visibleRange - 1;

            CalcRunnerHeightAndStep (CurrentVisibleCollection.Count);

            ReplaceActiveIcon (VisibleIcons [0]);
            SetManageControlsAbility ();
        }


        internal void ToPrevious ()
        {
            if ( (BeingProcessedNumber <= 1)   ||   (IsDropDownOpen) )
            {
                return;
            }

            ControlIncorrectCountValue ();
            int possableRemovableNumber = BeingProcessedNumber;
            int possableRemovableAmongVisibleNumber = _numberAmongVisibleIcons;
            SetSliderToStationBeforeScrollingIfShould ();

            int absoluteNumber = BeingProcessedNumber + 1;
            bool isProcessableChangedInSpecificFilter = IsProcessableChangedInSpecificFilter (BeingProcessedNumber);
            bool sliderIsScrollable = ( CurrentVisibleCollection.Count > _maxVisibleCount );

            BeingProcessedNumber--;
            _numberAmongVisibleIcons--;

            if ( isProcessableChangedInSpecificFilter )
            {
                bool endIsNotAchieved = ( _visibleRangeEnd < ( CurrentVisibleCollection.Count - 1 ) );
                bool shiftToSideLast = (endIsNotAchieved   ||   ! sliderIsScrollable);

                ReduceCurrentCollectionAndIcons 
                (possableRemovableNumber-1, possableRemovableAmongVisibleNumber-1, shiftToSideLast, true, sliderIsScrollable);

                CalcRunnerHeightAndStep (CurrentVisibleCollection.Count);

                if ( ! endIsNotAchieved   &&   sliderIsScrollable )
                {
                    _numberAmongVisibleIcons++;
                    _visibleRangeEnd--;
                    _scrollStepIndex--;

                    _runnerHasWalked = _scrollStepIndex * _runnerStep;
                    ScrollOffset = _runnerHasWalked;
                }
            }

            if ( BeingProcessedNumber < ( _visibleRangeEnd - _visibleRange + 2 ) )
            {
                ScrollUpAtOneStep ();
                SaveSliderState ();

                _numberAmongVisibleIcons = 1;
                _doubleRest = 0;
                _visibleRangeEnd--;
            }

            BeingProcessedBadge = GetAppropriateDraft (BeingProcessedNumber);
            SetToCorrectScale (BeingProcessedBadge);
            BeingProcessedBadge.Show ();

            BadgeCorrectnessViewModel newActiveIcon = VisibleIcons [_numberAmongVisibleIcons - 1];
            ReplaceActiveIcon (newActiveIcon);

            sliderIsScrollable = ( CurrentVisibleCollection.Count > _maxVisibleCount );

            if ( ! sliderIsScrollable )
            {
                ScrollWidth = 0;
            }

            SetManageControlsAbility ();
        }


        internal void ToNext ()
        {
            if ( (BeingProcessedNumber >= ProcessableCount)   ||   (IsDropDownOpen) )
            {
                return;
            }

            ControlIncorrectCountValue ();
            SetSliderToStationBeforeScrollingIfShould ();
            bool isProcessableChangedInSpecificFilter = IsProcessableChangedInSpecificFilter (BeingProcessedNumber);
            bool sliderIsScrollable = ( CurrentVisibleCollection.Count > _maxVisibleCount );

            BeingProcessedNumber++;
            _numberAmongVisibleIcons++;

            if ( isProcessableChangedInSpecificFilter )
            {
                BeingProcessedNumber--;
                _numberAmongVisibleIcons--;
                
                bool endIsNotAchieved = (_visibleRangeEnd < (CurrentVisibleCollection.Count - 1));
                bool shiftToSideLast = (endIsNotAchieved   ||   ! sliderIsScrollable);

                ReduceCurrentCollectionAndIcons
                ((BeingProcessedNumber - 1), (_numberAmongVisibleIcons - 1), shiftToSideLast, true, sliderIsScrollable);

                CalcRunnerHeightAndStep (CurrentVisibleCollection.Count);

                if ( ! endIsNotAchieved   &&   sliderIsScrollable )
                {
                    _numberAmongVisibleIcons++;
                    _visibleRangeEnd--;
                    _scrollStepIndex--;

                    _runnerHasWalked = _scrollStepIndex * _runnerStep;
                    ScrollOffset = _runnerHasWalked;
                }
            }

            if ( BeingProcessedNumber > ( _visibleRangeEnd + 1 ) )
            {
                ScrollDownAtOneStep ();
                SaveSliderState ();

                _numberAmongVisibleIcons = VisibleIcons.Count;
                _doubleRest = 0;
                _visibleRangeEnd++;
            }

            BeingProcessedBadge = GetAppropriateDraft (BeingProcessedNumber);
            BeingProcessedBadge.Show ();
            SetToCorrectScale (BeingProcessedBadge);

            BadgeCorrectnessViewModel newActiveIcon = VisibleIcons [_numberAmongVisibleIcons - 1];
            ReplaceActiveIcon (newActiveIcon);

            sliderIsScrollable = ( CurrentVisibleCollection.Count > _maxVisibleCount );

            if ( ! sliderIsScrollable ) 
            {
                ScrollWidth = 0;
            }

            SetManageControlsAbility ();
        }


        internal void ToLast ()
        {
            ControlIncorrectCountValue ();
            BadgeViewModel newProcesseble = null;
            int oldNumber = BeingProcessedNumber;

            VisibleIcons = new ObservableCollection <BadgeCorrectnessViewModel> ();
            _visibleIconsStorage = VisibleIcons;

            bool isProcessableChangedInSpecificFilter = IsProcessableChangedInSpecificFilter (BeingProcessedNumber);

            if ( isProcessableChangedInSpecificFilter )
            {
                ReduceCurrentCollection (BeingProcessedNumber - 1);

                if ( CurrentVisibleCollection.Count <= _maxVisibleCount )
                {
                    _visibleRange = CurrentVisibleCollection.Count;
                    _visibleRangeEnd = CurrentVisibleCollection.Count - 1;
                    ScrollWidth = 0;
                }
            }

            int visibleCountBeforeEnd = CurrentVisibleCollection.Count - _visibleRange;

            for ( int index = visibleCountBeforeEnd;   index < CurrentVisibleCollection.Count;   index++ )
            {
                BadgeViewModel boundBadge = CurrentVisibleCollection.ElementAt (index).Value;
                VisibleIcons.Add (new BadgeCorrectnessViewModel (boundBadge.IsCorrect, boundBadge, _correctnessWidthLimit
                                                  , new int [2] { _minCorrectnessTextLength, _maxCorrectnessTextLength }));
            }

            _numberAmongVisibleIcons = VisibleIcons. Count;

            ReplaceActiveIcon (VisibleIcons [_numberAmongVisibleIcons - 1]);

            BeingProcessedNumber = CurrentVisibleCollection.Count;
            BeingProcessedNumber = GetAppropriateLastNumber ();
            BeingProcessedBadge = GetAppropriateDraft (BeingProcessedNumber);
            BeingProcessedBadge.Show ();
            SetToCorrectScale (BeingProcessedBadge);

            bool sliderIsScrollable = ( CurrentVisibleCollection.Count > _maxVisibleCount );
            _visibleRangeEnd = CurrentVisibleCollection.Count - 1;

            CalcRunnerHeightAndStep (CurrentVisibleCollection.Count);

            if ( ! sliderIsScrollable )
            {
                ScrollWidth = 0;
                _visibleRange = CurrentVisibleCollection.Count;
                _scrollStepIndex = 0;
                _runnerHasWalked = 0;
            }
            else 
            {
                _scrollStepIndex = CurrentVisibleCollection.Count - _visibleRange;
                _runnerHasWalked = _scrollStepIndex * _runnerStep;
                ScrollOffset = _runnerHasWalked;
            }

            SetManageControlsAbility ();
        }


        internal void ToParticularBadge ( BadgeCorrectnessViewModel destinationIcon )
        {
            BadgeViewModel destinationBadge = destinationIcon.BoundBadge;

            if ( destinationBadge.Equals (BeingProcessedBadge) )
            {
                return;
            }

            string destinationNumberText = string.Empty;

            for ( int index = 0;   index < CurrentVisibleCollection.Count;   index++ )
            {
                if ( destinationBadge.Equals (CurrentVisibleCollection.ElementAt (index).Value) )
                {
                    destinationNumberText = ( index + 1 ).ToString ();
                    break;
                }
            }

            ToParticularBadge (destinationNumberText, destinationIcon);
        }


        private void ToParticularBadge ( string destinationNumberAsText, BadgeCorrectnessViewModel newActiveIcon )
        {
            try
            {
                ControlIncorrectCountValue ();

                int destinationNumber = int.Parse (destinationNumberAsText);
                bool isProcessableChangedInSpecificFilter = IsProcessableChangedInSpecificFilter (BeingProcessedNumber);

                int oldNumber = BeingProcessedNumber;
                int oldNumberAmongVisibleIcons = oldNumber - _scrollStepIndex;
                BeingProcessedNumber = destinationNumber;
                int amongVisibleIconsDestinationNum = destinationNumber - _scrollStepIndex;
                _numberAmongVisibleIcons = BeingProcessedNumber - _scrollStepIndex;
                
                BeingProcessedBadge = GetAppropriateDraft (BeingProcessedNumber);

                bool sliderIsScrollable = ( CurrentVisibleCollection.Count > _visibleRange );

                _visibleRangeEnd = _scrollStepIndex + _visibleRange - 1;

                if ( isProcessableChangedInSpecificFilter )
                {
                    bool endIsNotAchieved = ( _visibleRangeEnd < ( CurrentVisibleCollection.Count - 1 ) );
                    bool exProcessableIsNotInVisibleRange = ( oldNumber > ( _visibleRangeEnd + 1 ) ) 
                                                      ||  ( oldNumber < (_visibleRangeEnd - _visibleRange + 2) );

                    if ( endIsNotAchieved )
                    {
                        ReduceCurrentCollectionAndIcons (( oldNumber - 1 ), ( oldNumberAmongVisibleIcons - 1 )
                        , true, ! exProcessableIsNotInVisibleRange, sliderIsScrollable);

                        newActiveIcon = VisibleIcons [amongVisibleIconsDestinationNum - 1];

                        if ( oldNumber < BeingProcessedNumber )
                        {
                            BeingProcessedNumber--;

                            if ( exProcessableIsNotInVisibleRange )
                            {
                                newActiveIcon = VisibleIcons [amongVisibleIconsDestinationNum - 1];
                                _visibleRangeEnd--;
                                _scrollStepIndex--;
                            }
                            else
                            {
                                _numberAmongVisibleIcons--;
                                newActiveIcon = VisibleIcons [amongVisibleIconsDestinationNum - 2];
                            }
                        }
                        else 
                        {
                            newActiveIcon = VisibleIcons [amongVisibleIconsDestinationNum - 1];
                        }
                    }
                    else
                    {
                        if ( CurrentVisibleCollection.Count <= _maxVisibleCount )
                        {
                            ReduceCurrentCollectionAndIcons 
                            (( oldNumber - 1 ), ( oldNumberAmongVisibleIcons - 1 ), true, true, sliderIsScrollable);

                            if ( oldNumber < BeingProcessedNumber )
                            {
                                amongVisibleIconsDestinationNum--;
                                _numberAmongVisibleIcons--;
                                BeingProcessedNumber--;
                            }

                            newActiveIcon = VisibleIcons [amongVisibleIconsDestinationNum - 1];
                        }
                        else 
                        {
                            ReduceCurrentCollectionAndIcons 
                            (( oldNumber - 1 ), ( oldNumberAmongVisibleIcons - 1 ), false, true, sliderIsScrollable);
                            
                            if ( oldNumber < BeingProcessedNumber )
                            {
                                newActiveIcon = VisibleIcons [amongVisibleIconsDestinationNum - 1];
                                BeingProcessedNumber--;
                            }
                            else if ( oldNumber > BeingProcessedNumber )
                            {
                                newActiveIcon = VisibleIcons [amongVisibleIconsDestinationNum];
                                _numberAmongVisibleIcons++;
                                BeingProcessedNumber++;
                                BeingProcessedNumber--;
                            } 

                            _scrollStepIndex--;
                            _visibleRangeEnd--;
                        }
                    }

                    CalcRunnerHeightAndStep (CurrentVisibleCollection.Count);

                    if ( ! endIsNotAchieved   &&   sliderIsScrollable )
                    {
                        _runnerHasWalked = _scrollStepIndex * _runnerStep;
                        ScrollOffset = _runnerHasWalked;
                    }
                }

                ReplaceActiveIcon (newActiveIcon);
                SaveSliderState ();

                SetToCorrectScale (BeingProcessedBadge);
                BeingProcessedBadge.Show ();

                sliderIsScrollable = ( CurrentVisibleCollection.Count > _maxVisibleCount );

                if ( !sliderIsScrollable )
                {
                    ScrollWidth = 0;
                }

                SetManageControlsAbility ();
            }
            catch ( Exception ex )
            {
                BeingProcessedNumber = BeingProcessedNumber;
            }
        }


        private void ControlIncorrectCountValue () 
        {
            if ( (_filterState == FilterChoosing.Corrects)   &&   ( ! BeingProcessedBadge. IsCorrect) ) 
            {
                IncorrectBadgesCount = 0;
            }
        }


        private void SetManageControlsAbility ( )
        {
            EnableNavigation ();
            MoversAreEnable = false;
            SplitterIsEnable = false;
            ZoommerIsEnable = false;
            ReleaseCaptured ();
            TryEnableSliderUpDown (_visibleRange);
        }


        private void TryEnableSliderUpDown ( int enablingRange )
        {
            if ( enablingRange < 2 )
            {
                UpDownIsVisible = false;
                SliderMargin = new Thickness (7, 30);
            }
            else
            {
                UpDownIsVisible = true;
                SliderMargin = new Thickness (7, 0);
            }
        }


        private void ReplaceActiveIcon ( BadgeCorrectnessViewModel newActiveIcon )
        {
            FadeIcon (ActiveIcon);
            ActiveIcon = newActiveIcon;
            HighLightChosenIcon (ActiveIcon);
        }


        private BadgeViewModel ? GetAppropriateDraft ( int number )
        {
            BadgeViewModel goalBadge = null;
            goalBadge = CurrentVisibleCollection.ElementAt (number - 1).Value;

            return goalBadge;
        }


        private int GetAppropriateLastNumber ( )
        {
            int result = 0;
            result = CurrentVisibleCollection.Count;

            return result;
        }


        private void EnableNavigation ( )
        {
            int badgeCount = CurrentVisibleCollection.Count;

            if ( ( BeingProcessedNumber > 1 )   &&   ( BeingProcessedNumber == badgeCount ) )
            {
                FirstIsEnable = true;
                PreviousIsEnable = true;
                NextIsEnable = false;
                LastIsEnable = false;
            }
            else if ( ( BeingProcessedNumber > 1 )   &&   ( BeingProcessedNumber < badgeCount ) )
            {
                FirstIsEnable = true;
                PreviousIsEnable = true;
                NextIsEnable = true;
                LastIsEnable = true;
            }
            else if ( ( BeingProcessedNumber == 1 )   &&   ( badgeCount == 1 ) )
            {
                FirstIsEnable = false;
                PreviousIsEnable = false;
                NextIsEnable = false;
                LastIsEnable = false;
            }
            else if ( ( BeingProcessedNumber == 1 )   &&   ( badgeCount > 1 ) )
            {
                FirstIsEnable = false;
                PreviousIsEnable = false;
                NextIsEnable = true;
                LastIsEnable = true;
            }

            _view.CorrespondToEmptyCurrentCollection (CurrentVisibleCollection.Count);
        }
    }
}