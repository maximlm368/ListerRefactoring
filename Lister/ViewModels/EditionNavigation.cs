using Avalonia.Controls;
using Avalonia.Input;
using Avalonia;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContentAssembler;
using static System.Net.Mime.MediaTypeNames;
using System.Collections.ObjectModel;
using Avalonia.Media;
using System.Globalization;
using System.Reflection.Metadata;
using ExtentionsAndAuxiliary;
using Microsoft.VisualBasic;
using Avalonia.Media.Imaging;
using Lister.Extentions;
using System.Linq.Expressions;
using Lister.Views;
using System.Windows.Input;
using Avalonia.Platform.Storage;
using System.Reactive.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Avalonia.Threading;
using System.Reflection;
using AvaloniaEdit.Utils;
using ExCSS;
using System.Collections.Immutable;

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
            //FilterProcessableBadge (BeingProcessedNumber);
            
            VisibleIcons = new ObservableCollection <BadgeCorrectnessViewModel> ();
            _visibleIconsStorage = VisibleIcons;
            BeingProcessedNumber = 1;

            for ( int index = 0;   index < _visibleRange;   index++ )
            {
                VisibleIcons.Add (new BadgeCorrectnessViewModel (false, _allReadonlyBadges [index]));

                if ( index == 0 ) 
                {
                    if ( BeingProcessedBadge != null ) 
                    {
                        AccomodateExProcessableToProperList ();
                    }

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
            SetSliderToStationBeforeScrollingIfShould ();
            //FilterProcessableBadge (BeingProcessedNumber);

            BeingProcessedNumber--;
            _numberAmongLoadedIcons--;

            //BadgeViewModel newProcesseble = null;

            if ( BeingProcessedNumber < (_visibleRangeEnd - _visibleRange + 2))
            {
                //BadgeViewModel badge = _readonlyBadges [BeingProcessedNumber - 1];
                //BadgeViewModel clone = badge.Clone ();
                //newProcesseble = clone;

                //if ( !_drafts.ContainsKey (badge) )
                //{
                //    _drafts.Add (badge, clone);
                //}

                //TryAddToIncorrectsAndEntireLists (badge, clone);

                ScrollUpAtOneStep ();
                SaveSliderStation ();

                _numberAmongLoadedIcons = 1;
                _doubleRest = 0;
                _visibleRangeEnd--;
            }
            //else
            //{
            //    newProcesseble = _readonlyBadges [BeingProcessedNumber - 1].Clone ();
            //}

            AccomodateExProcessableToProperList ();
            BeingProcessedBadge = GetAppropriateDraft (BeingProcessedNumber);
            SetToCorrectScale (BeingProcessedBadge);
            BeingProcessedBadge.Show ();

            ReplaceActiveIcon (VisibleIcons [_numberAmongLoadedIcons - 1]);
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

            int absoluteNumber = BeingProcessedNumber + 1;
            //bool filterOccured = FilterProcessableBadge (BeingProcessedNumber);

            //if ( filterOccured )
            //{
            //    absoluteNumber--;
            //}

            BeingProcessedNumber = absoluteNumber;
            _numberAmongLoadedIcons++;

            if ( BeingProcessedNumber > ( _visibleRangeEnd + 1 ) )
            {
                ScrollDownAtOneStep ();
                SaveSliderStation ();

                _numberAmongLoadedIcons = VisibleIcons. Count;
                _doubleRest = 0;
                _visibleRangeEnd++;
            }

            //AccomodateExProcessableToProperList ();
            BeingProcessedBadge = GetAppropriateDraft (BeingProcessedNumber);
            BeingProcessedBadge.Show ();
            SetToCorrectScale (BeingProcessedBadge);
            
            ReplaceActiveIcon (VisibleIcons [_numberAmongLoadedIcons - 1]);
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

                //if ( index == ( _readonlyBadges.Count - 1 ) )
                //{
                //    BadgeViewModel clone = badge.Clone ();
                //    newProcesseble = clone;

                //    if ( !_drafts.ContainsKey (badge) )
                //    {
                //        _drafts.Add (badge, clone);
                //    }

                //    //TryAddToIncorrectsAndEntireLists (badge, clone);
                //}
            }

            ScrollOffset = RunnerWalkSpace;

            int absoluteNumber = _allReadonlyBadges.Count;
            _numberAmongLoadedIcons = VisibleIcons. Count;

            //bool filterOccured = FilterProcessableBadge (BeingProcessedNumber);

            //if ( filterOccured )
            //{
            //    absoluteNumber--;
            //}

            ReplaceActiveIcon (VisibleIcons [_numberAmongLoadedIcons - 1]);

            AccomodateExProcessableToProperList ();
            BeingProcessedNumber = _allReadonlyBadges.Count;
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

                AccomodateExProcessableToProperList ();
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


        private BadgeViewModel ? GetAppropriateDrafts ( int number )
        {
            BadgeViewModel goalBadge = null;
            BadgeViewModel source = null;

            if ( _filterState == FilterChoosing.All )
            {
                source = _allReadonlyBadges [number - 1];

                try
                {
                    goalBadge = AllBadges [number - 1];
                }
                catch ( Exception ex ) 
                {
                    goalBadge = source.Clone ();
                }
            }
            else if ( _filterState == FilterChoosing.Corrects )
            {
                goalBadge = AllBadges [FixedBadges.ElementAt (number - 1).Key];
            }
            else if ( _filterState == FilterChoosing.Incorrects )
            {
                try
                {
                    goalBadge = AllBadges [IncorrectBadges.ElementAt (number - 1).Key];
                }
                catch ( Exception ex ) {}

                if ( goalBadge == null ) 
                {
                        goalBadge = _allReadonlyBadges [IncorrectBadges.ElementAt (number - 1).Key].Clone ();
                        source = _allReadonlyBadges [IncorrectBadges.ElementAt (number - 1).Key];
                }
            }

            try
            {
                if ( goalBadge != null ) 
                {
                    AllBadges.Add (goalBadge.Id, goalBadge);
                    _drafts.Add (source, goalBadge);
                }
            }
            catch ( Exception ex ) { }

            return goalBadge;
        }


        private BadgeViewModel ? GetAppropriateDraft ( int number )
        {
            BadgeViewModel goalBadge = null;
            BadgeViewModel source = null;

            if ( _filterState == FilterChoosing.All )
            {
                source = _allReadonlyBadges [number - 1];

                try
                {
                    goalBadge = AllBadges [number - 1];
                }
                catch ( Exception ex )
                {
                    goalBadge = source.Clone ();
                }
            }
            else if ( _filterState == FilterChoosing.Corrects )
            {
                int counter = 0;

                foreach ( KeyValuePair <int, int> goalBadgeNumber   in   FixedBadges )
                {
                    if ( counter == ( number - 1 ) )
                    {
                        try
                        {
                            goalBadge = AllBadges [goalBadgeNumber.Value];
                            break;
                        }
                        catch ( Exception ex ) 
                        {
                            goalBadge = _allReadonlyBadges [goalBadgeNumber.Value].Clone();
                            source = _allReadonlyBadges [goalBadgeNumber.Value];
                            break;
                        }
                    }

                    counter++;
                }
            }
            else if ( _filterState == FilterChoosing.Incorrects )
            {
                int counter = 0;

                foreach ( KeyValuePair <int, int> goalBadgeNumber   in   IncorrectBadges )
                {
                    if ( counter == ( number - 1 ) )
                    {
                        try
                        {
                            goalBadge = AllBadges [goalBadgeNumber.Value];
                            break;
                        }
                        catch ( Exception ex )
                        {
                            goalBadge = _allReadonlyBadges [goalBadgeNumber.Value].Clone ();
                            source = _allReadonlyBadges [goalBadgeNumber.Value];
                            break;
                        }
                    }

                    counter++;
                }
            }

            try
            {
                if ( goalBadge != null )
                {
                    AllBadges.Add (goalBadge.Id, goalBadge);
                    _drafts.Add (source, goalBadge);
                }
            }
            catch ( Exception ex ) { }

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
                result = FixedBadges. Count;
            }
            else if ( _filterState == FilterChoosing.Incorrects )
            {
                result = IncorrectBadges. Count;
            }

            return result;
        }


        private void AccomodateExProcessableToProperList ()
        {
            if ( BeingProcessedBadge. IsCorrect   &&   ( _filterState == FilterChoosing.Incorrects ) )
            {
                ObservableCollection <BadgeCorrectnessViewModel> newVisibleIcons = new ();

                for ( int index = (BeingProcessedNumber - 1);   index < VisibleIcons. Count;   index++ )
                {
                    

                    VisibleIcons.Add (new BadgeCorrectnessViewModel (false, _allReadonlyBadges [_scrollStepNumber + index]));
                }


            }
            else if ( ! BeingProcessedBadge. IsCorrect   &&   ( _filterState == FilterChoosing.Corrects ) )
            {


                _incorrectsAmmount++;

                try
                {
                    IncorrectBadges.Add (BeingProcessedBadge. Id, BeingProcessedBadge. Id );
                }
                catch ( Exception ex ) { }
            }

            try
            {
                AllBadges.Add (BeingProcessedNumber, BeingProcessedBadge);
            }
            catch ( Exception ex ) { }
        }


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


        //private void TryAddToIncorrectsAndEntireLists ( BadgeViewModel badge, BadgeViewModel clone )
        //{
        //    if ( ! IncorrectBadges.ContainsKey (badge) )
        //    {
        //        IncorrectBadges.Add (badge, clone);
        //    }

        //    if ( ! AllBadges.ContainsKey (badge) )
        //    {
        //        AllBadges.Add (badge, clone);
        //    }
        //}


        //private void SaveProcessableIconVisible ()
        //{
        //    bool numberIsOutOfRange = BeingProcessedNumber > ( _visibleRangeEnd + 1 )
        //                              ||
        //                              BeingProcessedNumber < ( _visibleRangeEnd + 1 - _visibleRange );

        //    if ( numberIsOutOfRange )
        //    {
        //        //ScrollViewer scroller = _view.sliderScroller;
        //        //int offsetedItemsCount = ( int ) ( scroller.Offset.Y / _itemHeight );
        //        //_doubleRest = ( scroller.Offset.Y - ( offsetedItemsCount * _itemHeight ) );

        //        //int diff = _visibleRange - ( BeingProcessedNumber - offsetedItemsCount );
        //        //_visibleRangeEnd = BeingProcessedNumber + diff - 1;
        //    }
        //}


        //private void SaveProcessableIconVisibleCh ()
        //{
        //    bool numberIsOutOfRange = BeingProcessedNumber > ( _visibleRangeEnd + 1 )
        //                              ||
        //                              BeingProcessedNumber < ( _visibleRangeEnd + 1 - _visibleRange );

        //    if ( numberIsOutOfRange )
        //    {
        //        _visibleRangeEnd = BeingProcessedNumber + _visibleRange / 2;
        //        ScrollOffset = _itemHeight * ( BeingProcessedNumber - _visibleRange / 2 );
        //    }
        //}
    }
}