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
            BeingProcessedBadge.Hide ();

            int absoluteNumber = BeingProcessedNumber + 1;
            bool filterOccured = IsProcessableChangedInSpecificFilter (BeingProcessedNumber);

            if ( filterOccured )
            {
                absoluteNumber--;
            }

            VisibleIcons = new ObservableCollection <BadgeCorrectnessViewModel> ();
            _visibleIconsStorage = VisibleIcons;
            BeingProcessedNumber = 1;

            for ( int index = 0;   index < _visibleRange;   index++ )
            {
                VisibleIcons.Add (new BadgeCorrectnessViewModel (false, _currentVisibleCollection.ElementAt(index).Value));

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

            BeingProcessedBadge.Hide ();
            int possableRemovableNumber = BeingProcessedNumber - 1;
            SetSliderToStationBeforeScrollingIfShould ();

            int absoluteNumber = BeingProcessedNumber + 1;
            bool isProcessableChangedInSpecificFilter = IsProcessableChangedInSpecificFilter (BeingProcessedNumber);

            BeingProcessedNumber--;
            _numberAmongVisibleIcons--;

            if ( BeingProcessedNumber < (_visibleRangeEnd - _visibleRange + 2))
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

            if ( isProcessableChangedInSpecificFilter )
            {
                ShrinkVisibleIconsByNumber (possableRemovableNumber, _numberAmongVisibleIcons);
            }

            BadgeCorrectnessViewModel newActiveIcon = VisibleIcons [_numberAmongVisibleIcons - 1];
            ReplaceActiveIcon (newActiveIcon);
            SetManageControlsAbility ();
        }


        internal void ToNext ()
        {
            if ( BeingProcessedNumber >= ProcessableCount )
            {
                return;
            }

            BeingProcessedBadge.Hide ();
            SetSliderToStationBeforeScrollingIfShould ();
            bool isProcessableChangedInSpecificFilter = IsProcessableChangedInSpecificFilter (BeingProcessedNumber);

            BeingProcessedNumber++;
            _numberAmongVisibleIcons++;

            if ( BeingProcessedNumber > ( _visibleRangeEnd + 1 ) )
            {
                ScrollDownAtOneStep ();
                SaveSliderState ();

                _numberAmongVisibleIcons = VisibleIcons. Count;
                _doubleRest = 0;
                _visibleRangeEnd++;
            }

            BeingProcessedBadge = GetAppropriateDraft (BeingProcessedNumber);
            BeingProcessedBadge.Show ();
            SetToCorrectScale (BeingProcessedBadge);

            if ( isProcessableChangedInSpecificFilter )
            {
                BeingProcessedNumber--;
                _numberAmongVisibleIcons--;
                ShrinkVisibleIconsByNumber (BeingProcessedNumber - 1, _numberAmongVisibleIcons - 1);
            }

            BadgeCorrectnessViewModel newActiveIcon = VisibleIcons [_numberAmongVisibleIcons - 1];
            ReplaceActiveIcon (newActiveIcon);
            SetManageControlsAbility ();
        }


        internal void ToLast ()
        {
            BeingProcessedBadge.Hide ();
            BadgeViewModel newProcesseble = null;

            VisibleIcons = new ObservableCollection <BadgeCorrectnessViewModel> ();
            _visibleIconsStorage = VisibleIcons;

            int visibleCountBeforeEnd = _currentVisibleCollection.Count - _visibleRange;

            for ( int index = visibleCountBeforeEnd;   index < _currentVisibleCollection.Count;   index++ )
            {
                BadgeViewModel badge = _currentVisibleCollection.ElementAt (index).Value;
                VisibleIcons.Add (new BadgeCorrectnessViewModel (false, badge));
            }

            ScrollOffset = RunnerWalkSpace;

            int absoluteNumber = _currentVisibleCollection.Count;
            _numberAmongVisibleIcons = VisibleIcons. Count;

            bool filterOccured = IsProcessableChangedInSpecificFilter (BeingProcessedNumber);

            if ( filterOccured )
            {
                absoluteNumber--;
            }

            ReplaceActiveIcon (VisibleIcons [_numberAmongVisibleIcons - 1]);

            BeingProcessedNumber = absoluteNumber;
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
                    if ( _visibleRangeEnd < ( _currentVisibleCollection.Count - 1 ) )
                    {
                        ShrinkVisibleIconsByNumber (( oldNumber - 1 ), ( oldNumberAmongVisibleIcons - 1 ));
                        newActiveIcon = VisibleIcons [amongVisibleIconsDestinationNum - 1];

                        if ( oldNumber < BeingProcessedNumber )
                        {
                            BeingProcessedNumber--;
                            _numberAmongVisibleIcons--;
                            newActiveIcon = VisibleIcons [amongVisibleIconsDestinationNum - 2];
                        }
                    }
                    else if ( _visibleRangeEnd == ( _currentVisibleCollection.Count - 1 ) )
                    {
                        ShrinkVisibleIconsByNumber (( oldNumber - 1 ), ( oldNumberAmongVisibleIcons - 1 ));

                        if ( _currentVisibleCollection.Count <= _visibleRange )
                        {
                            amongVisibleIconsDestinationNum--;
                            _numberAmongVisibleIcons--;
                            _visibleRangeEnd--;
                        }

                        if ( oldNumber > BeingProcessedNumber )
                        {
                            newActiveIcon = VisibleIcons [amongVisibleIconsDestinationNum];
                            _numberAmongVisibleIcons++;
                        }
                        else if ( oldNumber < BeingProcessedNumber )
                        {
                            newActiveIcon = VisibleIcons [amongVisibleIconsDestinationNum - 1];
                            BeingProcessedNumber--;
                        }
                    }

                    _scrollStepNumber--;
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


        private void SetManageControlsAbility ( )
        {
            EnableNavigation ();
            MoversAreEnable = false;
            SplitterIsEnable = false;
            ZoommerIsEnable = false;
            //_view.editorTextBox.IsEnabled = false;
            //_view.scalabilityGrade.IsEnabled = false;
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


        //private void AccomodateExProcessableToProperList ()
        //{
        //    if ( BeingProcessedBadge. IsCorrect   &&   ( _filterState == FilterChoosing.Incorrects ) )
        //    {
        //        ObservableCollection <BadgeCorrectnessViewModel> newVisibleIcons = new ();

        //        for ( int index = (BeingProcessedNumber - 1);   index < VisibleIcons. Count;   index++ )
        //        {
                    

        //            VisibleIcons.Add (new BadgeCorrectnessViewModel (false, _allReadonlyBadges [_scrollStepNumber + index]));
        //        }


        //    }
        //    else if ( ! BeingProcessedBadge. IsCorrect   &&   ( _filterState == FilterChoosing.Corrects ) )
        //    {


        //        _incorrectsAmmount++;

        //        try
        //        {
        //            IncorrectNumberedSources.Add (BeingProcessedBadge. Id, BeingProcessedBadge );
        //        }
        //        catch ( Exception ex ) { }
        //    }

        //    try
        //    {
        //        AllNumberedDrafts.Add (BeingProcessedNumber, BeingProcessedBadge);
        //    }
        //    catch ( Exception ex ) { }
        //}


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