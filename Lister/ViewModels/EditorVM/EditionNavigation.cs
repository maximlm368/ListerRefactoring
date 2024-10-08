using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace Lister.ViewModels
{
    public partial class BadgeEditorViewModel : ViewModelBase
    {
        private int _numberAmongVisibleIcons = 1;

        private bool fE;
        internal bool FirstIsEnable
        {
            get { return fE; }
            private set
            {
                this.RaiseAndSetIfChanged (ref fE, value, nameof (FirstIsEnable));
            }
        }

        private bool pE;
        internal bool PreviousIsEnable
        {
            get { return pE; }
            private set
            {
                this.RaiseAndSetIfChanged (ref pE, value, nameof (PreviousIsEnable));
            }
        }

        private bool nE;
        internal bool NextIsEnable
        {
            get { return nE; }
            private set
            {
                this.RaiseAndSetIfChanged (ref nE, value, nameof (NextIsEnable));
            }
        }

        private bool lE;
        internal bool LastIsEnable
        {
            get { return lE; }
            private set
            {
                this.RaiseAndSetIfChanged (ref lE, value, nameof (LastIsEnable));
            }
        }

        private int bpN;
        internal int BeingProcessedNumber
        {
            get { return bpN; }
            private set
            {
                this.RaiseAndSetIfChanged (ref bpN, value, nameof (BeingProcessedNumber));
            }
        }


        internal void ToFirst ()
        {
            ControlIncorrectCountValue ();
            BeingProcessedBadge.Hide ();

            int absoluteNumber = BeingProcessedNumber + 1;
            bool isProcessableChangedInSpecificFilter = IsProcessableChangedInSpecificFilter (BeingProcessedNumber);

            if ( isProcessableChangedInSpecificFilter )
            {
                ReduceCurrentCollection (BeingProcessedNumber - 1);

                if ( _currentVisibleCollection.Count <= _maxVisibleCount )
                {
                    _visibleRange = _currentVisibleCollection.Count;
                }
            }

            VisibleIcons = new ObservableCollection <BadgeCorrectnessViewModel> ();
            _visibleIconsStorage = VisibleIcons;
            BeingProcessedNumber = 1;

            for ( int index = 0;   index < _visibleRange;   index++ )
            {
                BadgeViewModel boundBadge = _currentVisibleCollection.ElementAt (index).Value;

                VisibleIcons.Add (new BadgeCorrectnessViewModel (boundBadge.IsCorrect, boundBadge));

                if ( index == 0 ) 
                {
                    BeingProcessedBadge = GetAppropriateDraft (BeingProcessedNumber);
                }
            }

            _numberAmongVisibleIcons = 1;

            SetToCorrectScale (BeingProcessedBadge);
            BeingProcessedBadge.Show ();

            ScrollOffset = 0;
            _scrollStepNumber = 0;
            _runnerHasWalked = 0;
            _visibleRangeEnd = _visibleRange - 1;

            ReplaceActiveIcon (VisibleIcons [0]);
            SetManageControlsAbility ();
        }


        internal void ToPrevious ()
        {
            if ( BeingProcessedNumber <= 1 )
            {
                return;
            }

            ControlIncorrectCountValue ();
            BeingProcessedBadge.Hide ();
            int possableRemovableNumber = BeingProcessedNumber;
            int possableRemovableAmongVisibleNumber = _numberAmongVisibleIcons;
            SetSliderToStationBeforeScrollingIfShould ();

            int absoluteNumber = BeingProcessedNumber + 1;
            bool isProcessableChangedInSpecificFilter = IsProcessableChangedInSpecificFilter (BeingProcessedNumber);
            bool sliderIsScrollable = ( _currentVisibleCollection.Count > _maxVisibleCount );

            BeingProcessedNumber--;
            _numberAmongVisibleIcons--;

            if ( isProcessableChangedInSpecificFilter )
            {
                bool endIsNotAchieved = ( _visibleRangeEnd < ( _currentVisibleCollection.Count - 1 ) );
                bool shiftToSideLast = (endIsNotAchieved   ||   ! sliderIsScrollable);

                ShrinkFilteredListsByNumbers 
                (possableRemovableNumber-1, possableRemovableAmongVisibleNumber-1, shiftToSideLast, true, sliderIsScrollable);

                if ( ! endIsNotAchieved   &&   sliderIsScrollable )
                {
                    _numberAmongVisibleIcons++;
                    _visibleRangeEnd--;
                    _scrollStepNumber--;
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

            sliderIsScrollable = ( _currentVisibleCollection.Count > _maxVisibleCount );

            if ( ! sliderIsScrollable )
            {
                ScrollWidth = 0;
            }

            SetManageControlsAbility ();
        }


        internal void ToNext ()
        {
            if ( BeingProcessedNumber >= ProcessableCount )
            {
                return;
            }

            ControlIncorrectCountValue ();
            BeingProcessedBadge.Hide ();
            SetSliderToStationBeforeScrollingIfShould ();
            bool isProcessableChangedInSpecificFilter = IsProcessableChangedInSpecificFilter (BeingProcessedNumber);
            bool sliderIsScrollable = ( _currentVisibleCollection.Count > _maxVisibleCount );

            BeingProcessedNumber++;
            _numberAmongVisibleIcons++;

            if ( isProcessableChangedInSpecificFilter )
            {
                BeingProcessedNumber--;
                _numberAmongVisibleIcons--;
                
                bool endIsNotAchieved = (_visibleRangeEnd < ( _currentVisibleCollection.Count - 1 ));
                bool shiftToSideLast = (endIsNotAchieved   ||   ! sliderIsScrollable);

                ShrinkFilteredListsByNumbers
                ((BeingProcessedNumber - 1), (_numberAmongVisibleIcons - 1), shiftToSideLast, true, sliderIsScrollable);

                if ( ! endIsNotAchieved   &&   sliderIsScrollable )
                {
                    _numberAmongVisibleIcons++;
                    _visibleRangeEnd--;
                    _scrollStepNumber--;
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

            sliderIsScrollable = ( _currentVisibleCollection.Count > _maxVisibleCount );

            if ( ! sliderIsScrollable ) 
            {
                ScrollWidth = 0;
            }

            SetManageControlsAbility ();
        }


        internal void ToLast ()
        {
            ControlIncorrectCountValue ();
            BeingProcessedBadge.Hide ();
            BadgeViewModel newProcesseble = null;
            int oldNumber = BeingProcessedNumber;

            VisibleIcons = new ObservableCollection <BadgeCorrectnessViewModel> ();
            _visibleIconsStorage = VisibleIcons;

            bool isProcessableChangedInSpecificFilter = IsProcessableChangedInSpecificFilter (BeingProcessedNumber);

            if ( isProcessableChangedInSpecificFilter )
            {
                ReduceCurrentCollection (BeingProcessedNumber - 1);

                //if ( _currentVisibleCollection.Count <= _maxVisibleCount )
                //{
                //    _visibleRangeEnd--;
                //    _visibleRange--;
                //}

                if ( _currentVisibleCollection.Count <= _maxVisibleCount )
                {
                    _visibleRangeEnd--;
                    _visibleRange = _currentVisibleCollection.Count;
                }
            }

            int visibleCountBeforeEnd = _currentVisibleCollection.Count - _visibleRange;

            for ( int index = visibleCountBeforeEnd;   index < _currentVisibleCollection.Count;   index++ )
            {
                BadgeViewModel boundBadge = _currentVisibleCollection.ElementAt (index).Value;
                VisibleIcons.Add (new BadgeCorrectnessViewModel (boundBadge.IsCorrect, boundBadge));
            }

            ScrollOffset = RunnerWalkSpace;
            _numberAmongVisibleIcons = VisibleIcons. Count;

            ReplaceActiveIcon (VisibleIcons [_numberAmongVisibleIcons - 1]);

            BeingProcessedNumber = _currentVisibleCollection.Count;
            BeingProcessedNumber = GetAppropriateLastNumber ();
            BeingProcessedBadge = GetAppropriateDraft (BeingProcessedNumber);
            BeingProcessedBadge.Show ();
            SetToCorrectScale (BeingProcessedBadge);

            SetManageControlsAbility ();

            _scrollStepNumber = _currentVisibleCollection.Count - _visibleRange;
            _runnerHasWalked = _scrollStepNumber * _runnerStep;
            _visibleRangeEnd = _currentVisibleCollection.Count - 1;
        }


        internal void ToParticularBadge ( BadgeCorrectnessViewModel destinationIcon )
        {
            BadgeViewModel destinationBadge = destinationIcon.BoundBadge;

            if ( destinationBadge.Equals (BeingProcessedBadge) )
            {
                return;
            }

            string destinationNumberText = string.Empty;

            for ( int index = 0;   index < _currentVisibleCollection.Count;   index++ )
            {
                if ( destinationBadge.Equals (_currentVisibleCollection.ElementAt (index).Value) )
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

                BeingProcessedBadge.Hide ();
                bool isProcessableChangedInSpecificFilter = IsProcessableChangedInSpecificFilter (BeingProcessedNumber);

                int oldNumber = BeingProcessedNumber;
                int oldNumberAmongVisibleIcons = oldNumber - _scrollStepNumber;
                BeingProcessedNumber = destinationNumber;
                int amongVisibleIconsDestinationNum = destinationNumber - _scrollStepNumber;
                _numberAmongVisibleIcons = BeingProcessedNumber - _scrollStepNumber;
                BeingProcessedBadge = GetAppropriateDraft (BeingProcessedNumber);

                _visibleRangeEnd = _scrollStepNumber + _visibleRange - 1;

                if ( isProcessableChangedInSpecificFilter )
                {
                    bool sliderIsScrollable = ( _currentVisibleCollection.Count > _visibleRange );
                    bool endIsNotAchieved = ( _visibleRangeEnd < ( _currentVisibleCollection.Count - 1 ) );
                    bool exProcessableIsNotInVisibleRange = ( oldNumber > ( _visibleRangeEnd + 1 ) ) 
                                                      ||  ( oldNumber < (_visibleRangeEnd - _visibleRange + 2) );

                    if ( endIsNotAchieved )
                    {
                        ShrinkFilteredListsByNumbers (( oldNumber - 1 ), ( oldNumberAmongVisibleIcons - 1 )
                        , true, ! exProcessableIsNotInVisibleRange, sliderIsScrollable);

                        newActiveIcon = VisibleIcons [amongVisibleIconsDestinationNum - 1];

                        if ( oldNumber < BeingProcessedNumber )
                        {
                            BeingProcessedNumber--;

                            if ( exProcessableIsNotInVisibleRange )
                            {
                                newActiveIcon = VisibleIcons [amongVisibleIconsDestinationNum - 1];
                                _visibleRangeEnd--;
                                _scrollStepNumber--;
                            }
                            else
                            {
                                _numberAmongVisibleIcons--;
                                _scrollStepNumber--;
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
                        if ( _currentVisibleCollection.Count <= _maxVisibleCount )
                        {
                            ShrinkFilteredListsByNumbers 
                            (( oldNumber - 1 ), ( oldNumberAmongVisibleIcons - 1 ), true, true, sliderIsScrollable);

                            if ( oldNumber < BeingProcessedNumber )
                            {
                                amongVisibleIconsDestinationNum--;
                                _numberAmongVisibleIcons--;
                                BeingProcessedNumber--;
                            }

                            //_visibleRangeEnd--;
                            //_visibleRange--;

                            newActiveIcon = VisibleIcons [amongVisibleIconsDestinationNum - 1];
                        }
                        else 
                        {
                            ShrinkFilteredListsByNumbers 
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

                            _scrollStepNumber--;
                            _visibleRangeEnd--;
                        }
                    }
                }

                ReplaceActiveIcon (newActiveIcon);
                SaveSliderState ();

                SetToCorrectScale (BeingProcessedBadge);
                BeingProcessedBadge.Show ();

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

            if ( _filterState == FilterChoosing.All )
            {
                goalBadge = AllNumbered [number - 1];
            }
            else if ( _filterState == FilterChoosing.Corrects )
            {
                goalBadge = CorrectNumbered.ElementAt (number - 1).Value;
            }
            else if ( _filterState == FilterChoosing.Incorrects )
            {
                goalBadge = IncorrectNumbered.ElementAt (number - 1).Value;
            }

            return goalBadge;
        }


        private int GetAppropriateLastNumber ( )
        {
            int result = 0;

            if ( _filterState == FilterChoosing.All )
            {
                result = _allReadonlyBadges.Count;
            }
            else if ( _filterState == FilterChoosing.Corrects )
            {
                result = CorrectNumbered. Count;
            }
            else if ( _filterState == FilterChoosing.Incorrects )
            {
                result = IncorrectNumbered. Count;
            }

            return result;
        }


        private void EnableNavigation ( )
        {
            int badgeCount = _currentVisibleCollection.Count;

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
        }
    }
}