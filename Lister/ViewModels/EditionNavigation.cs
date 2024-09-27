using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace Lister.ViewModels
{
    public partial class BadgeEditorViewModel : ViewModelBase
    {
        private int _numberAmongLoadedIcons = 1;

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
                VisibleIcons.Add (new BadgeCorrectnessViewModel (false, _allReadonlyBadges [index]));

                if ( index == 0 ) 
                {
                    //if ( BeingProcessedBadge != null ) 
                    //{
                    //    AccomodateExProcessableToProperList ();
                    //}

                    BeingProcessedBadge = GetAppropriateDraft (BeingProcessedNumber);
                }
            }

            _numberAmongLoadedIcons = 1;

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
            _numberAmongLoadedIcons--;

            if ( BeingProcessedNumber < (_visibleRangeEnd - _visibleRange + 2))
            {
                ScrollUpAtOneStep ();
                SaveSliderStation ();

                _numberAmongLoadedIcons = 1;
                _doubleRest = 0;
                _visibleRangeEnd--;
            }

            BeingProcessedBadge = GetAppropriateDraft (BeingProcessedNumber);
            SetToCorrectScale (BeingProcessedBadge);
            BeingProcessedBadge.Show ();

            if ( isProcessableChangedInSpecificFilter )
            {
                ShrinkVisibleIconsByNumber (possableRemovableNumber, _numberAmongLoadedIcons);
            }

            BadgeCorrectnessViewModel newActiveIcon = VisibleIcons [_numberAmongLoadedIcons - 1];
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
            int possableRemovableNumber = BeingProcessedNumber - 1;
            SetSliderToStationBeforeScrollingIfShould ();
            bool isProcessableChangedInSpecificFilter = IsProcessableChangedInSpecificFilter (BeingProcessedNumber);

            BeingProcessedNumber++;
            _numberAmongLoadedIcons++;

            if ( BeingProcessedNumber > ( _visibleRangeEnd + 1 ) )
            {
                ScrollDownAtOneStep ();
                SaveSliderStation ();

                _numberAmongLoadedIcons = VisibleIcons. Count;
                _doubleRest = 0;
                _visibleRangeEnd++;
            }

            BeingProcessedBadge = GetAppropriateDraft (BeingProcessedNumber);
            BeingProcessedBadge.Show ();
            SetToCorrectScale (BeingProcessedBadge);

            if ( isProcessableChangedInSpecificFilter )
            {
                BeingProcessedNumber--;
                ShrinkVisibleIconsByNumber (possableRemovableNumber, _numberAmongLoadedIcons - 2);
                _numberAmongLoadedIcons--;
            }

            BadgeCorrectnessViewModel newActiveIcon = VisibleIcons [_numberAmongLoadedIcons - 1];
            ReplaceActiveIcon (newActiveIcon);
            SetManageControlsAbility ();
        }


        internal void ToLast ()
        {
            BeingProcessedBadge.Hide ();
            BadgeViewModel newProcesseble = null;

            VisibleIcons = new ObservableCollection <BadgeCorrectnessViewModel> ();
            _visibleIconsStorage = VisibleIcons;

            int visibleCountBeforeEnd = _allReadonlyBadges.Count - _visibleRange;

            for ( int index = visibleCountBeforeEnd;   index < _allReadonlyBadges.Count;   index++ )
            {
                BadgeViewModel badge = _allReadonlyBadges [index];
                VisibleIcons.Add (new BadgeCorrectnessViewModel (false, badge));
            }

            ScrollOffset = RunnerWalkSpace;

            int absoluteNumber = _allReadonlyBadges.Count;
            _numberAmongLoadedIcons = VisibleIcons. Count;

            bool filterOccured = IsProcessableChangedInSpecificFilter (BeingProcessedNumber);

            if ( filterOccured )
            {
                absoluteNumber--;
            }

            ReplaceActiveIcon (VisibleIcons [_numberAmongLoadedIcons - 1]);

            BeingProcessedNumber = absoluteNumber;
            BeingProcessedNumber = GetAppropriateLastNumber ();
            BeingProcessedBadge = GetAppropriateDraft (BeingProcessedNumber);
            BeingProcessedBadge.Show ();
            SetToCorrectScale (BeingProcessedBadge);

            SetManageControlsAbility ();

            _scrollStepNumber = _allReadonlyBadges.Count - _visibleRange;
            _runnerHasWalked = _scrollStepNumber * _runnerStep;
            _visibleRangeEnd = _allReadonlyBadges.Count - 1;
        }


        internal void ToParticularBadge ( BadgeCorrectnessViewModel icon )
        {
            BadgeViewModel goalBadge = icon.BoundBadge;

            if ( goalBadge.Equals (BeingProcessedBadge) )
            {
                return;
            }

            string numberAsText = string.Empty;

            for ( int index = 0;   index < _allReadonlyBadges.Count;   index++ )
            {
                if ( goalBadge.Equals (_allReadonlyBadges[index]) )
                {
                    numberAsText = ( index + 1 ).ToString ();
                    break;
                }
            }

            ToParticularBadge (numberAsText, icon);
        }


        private void ToParticularBadge ( string numberAsText, BadgeCorrectnessViewModel newActiveIcon )
        {
            try
            {
                int number = int.Parse (numberAsText);
                //int diff = number - BeingProcessedNumber;

                BeingProcessedBadge.Hide ();
                //bool filterOccured = FilterProcessableBadge (number);

                //if ( filterOccured   &&   ( number > BeingProcessedNumber ) )
                //{
                //    number--;
                //}

                BeingProcessedNumber = number;
                BeingProcessedBadge = GetAppropriateDraft (BeingProcessedNumber);
                _numberAmongLoadedIcons = number - _scrollStepNumber;
                _visibleRangeEnd = _scrollStepNumber + _visibleRange - 1;

                ReplaceActiveIcon (newActiveIcon);
                SaveSliderStation ();

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


        private void EnableNavigation ()
        {
            int badgeCount = _allReadonlyBadges.Count;

            if ( ( BeingProcessedNumber > 1 ) && ( BeingProcessedNumber == badgeCount ) )
            {
                FirstIsEnable = true;
                PreviousIsEnable = true;
                NextIsEnable = false;
                LastIsEnable = false;
            }
            else if ( ( BeingProcessedNumber > 1 ) && ( BeingProcessedNumber < badgeCount ) )
            {
                FirstIsEnable = true;
                PreviousIsEnable = true;
                NextIsEnable = true;
                LastIsEnable = true;
            }
            else if ( ( BeingProcessedNumber == 1 ) && ( badgeCount == 1 ) )
            {
                FirstIsEnable = false;
                PreviousIsEnable = false;
                NextIsEnable = false;
                LastIsEnable = false;
            }
            else if ( ( BeingProcessedNumber == 1 ) && ( badgeCount > 1 ) )
            {
                FirstIsEnable = false;
                PreviousIsEnable = false;
                NextIsEnable = true;
                LastIsEnable = true;
            }
        }
    }
}