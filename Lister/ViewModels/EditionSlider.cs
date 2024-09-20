﻿using Avalonia.Controls;
using Avalonia.Input;
using Avalonia;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
using System.Diagnostics;

namespace Lister.ViewModels
{
    public partial class BadgeEditorViewModel : ViewModelBase
    {
        private static readonly double _minRunnerHeight = 10;
        private static readonly double _upDownButtonHeightWigth = 15;
        private static readonly double _scrollingScratch = 25;
        private static int _maxVisibleCount = 10;
        
        //private Stopwatch _scrollTimer;
        //private int _timesCommandCalled;
        
        private double _totalItemAmmount;
        private double _proportion;
        private double _scrollValue;
        private double _runnerStep;
        private double _runnerHasWalked;
        private double _runnerHasWalkedStorage;
        private double _scrollingLength;

        //private double _scrollHeight = 204;
        private double _itemHeight = 28;

        private readonly double _collectionFilterWidth = 250;
        private readonly double _sliderWidth = 50;
        private readonly double _namesFilterWidth = 200;
        private double _entireBlockHeight = 385;
        private double _scrollHeight = 280;
        private readonly double _workAreaWidth = 550;
        private readonly double _normalOpacity = 0.4;
        private readonly double _chosenOpacity = 1;
        private bool _filterIsOpen;

        private double _doubleRest;
        private int _visibleRange;
        private int _scrollStepNumberStorage;
        private int _scrollStepNumber;
        //private Dictionary <int, BadgeViewModel> _currentScrollable;
        private int _currentAmmount;


        private Thickness cFM;
        internal Thickness CollectionFilterMargin
        {
            get { return cFM; }
            private set
            {
                this.RaiseAndSetIfChanged (ref cFM, value, nameof (CollectionFilterMargin));
            }
        }

        private double cMW;
        internal double CollectionFilterWidth
        {
            get { return cMW; }
            private set
            {
                this.RaiseAndSetIfChanged (ref cMW, value, nameof (CollectionFilterWidth));
            }
        }

        private double nFW;
        internal double NamesFilterWidth
        {
            get { return nFW; }
            private set
            {
                this.RaiseAndSetIfChanged (ref nFW, value, nameof (NamesFilterWidth));
            }
        }

        private double cO;
        internal double CorrectnessOpacity
        {
            get { return cO; }
            private set
            {
                this.RaiseAndSetIfChanged (ref cO, value, nameof (CorrectnessOpacity));
            }
        }

        private double iO;
        internal double IncorrectnessOpacity
        {
            get { return iO; }
            private set
            {
                this.RaiseAndSetIfChanged (ref iO, value, nameof (IncorrectnessOpacity));
            }
        }

        private Bitmap cR;
        internal Bitmap CorrectnessIcon
        {
            get { return cR; }
            private set
            {
                this.RaiseAndSetIfChanged (ref cR, value, nameof (CorrectnessIcon));
            }
        }

        private Bitmap iC;
        internal Bitmap IncorrectnessIcon
        {
            get { return iC; }
            private set
            {
                this.RaiseAndSetIfChanged (ref iC, value, nameof (IncorrectnessIcon));
            }
        }

        private ObservableCollection <BadgeCorrectnessViewModel> _visibleIconsStorage;
        private ObservableCollection <BadgeCorrectnessViewModel> vI;
        internal ObservableCollection <BadgeCorrectnessViewModel> VisibleIcons
        {
            get { return vI; }
            private set
            {
                this.RaiseAndSetIfChanged (ref vI, value, nameof (VisibleIcons));
            }
        }

        private BadgeCorrectnessViewModel bpI;
        internal BadgeCorrectnessViewModel ActiveIcon
        {
            get { return bpI; }
            private set
            {
                this.RaiseAndSetIfChanged (ref bpI, value, nameof (ActiveIcon));
            }
        }

        private double eBH;
        internal double EntireBlockHeight
        {
            get { return eBH; }
            private set
            {
                this.RaiseAndSetIfChanged (ref eBH, value, nameof (EntireBlockHeight));
            }
        }

        private double sH;
        internal double ScrollHeight
        {
            get { return sH; }
            private set
            {
                this.RaiseAndSetIfChanged (ref sH, value, nameof (ScrollHeight));
            }
        }

        private double rBWS;
        internal double RunnerBruttoWalkSpace
        {
            get { return rBWS; }
            private set
            {
                this.RaiseAndSetIfChanged (ref rBWS, value, nameof (RunnerBruttoWalkSpace));
            }
        }

        private double _scrollOffsetStorage;
        private double sO;
        internal double ScrollOffset
        {
            get { return sO; }
            private set
            {
                this.RaiseAndSetIfChanged (ref sO, value, nameof (ScrollOffset));
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


        private double sW;
        internal double ScrollWidth
        {
            get { return sW; }
            private set
            {
                this.RaiseAndSetIfChanged (ref sW, value, nameof (ScrollWidth));
            }
        }

        private double rWS;
        internal double RunnerWalkSpace
        {
            get { return rWS; }
            private set
            {
                this.RaiseAndSetIfChanged (ref rWS, value, nameof (RunnerWalkSpace));
            }
        }


        private void SetUpSliderBlock ( int incorrectBadgesAmmount )
        {
            CollectionFilterWidth = _sliderWidth;
            CollectionFilterMargin = new Thickness (_namesFilterWidth, 0);

            _scrollHeight -= MainWindow.HeightfDifference;
            _entireBlockHeight -= MainWindow.HeightfDifference;

            ScrollHeight = _scrollHeight;
            EntireBlockHeight = _entireBlockHeight;

            int newVisibleRange = ( int ) ( _scrollHeight / _itemHeight );
            int diff = newVisibleRange - _visibleRange;
            _visibleRange = newVisibleRange;
            _visibleRangeEnd = diff - 1;

            NamesFilterWidth = 0;

            SetScroller (incorrectBadgesAmmount);
        }


        private void SetScroller ( int incorrectBadgesAmmount )
        {
            _totalItemAmmount = incorrectBadgesAmmount;

            if ( _totalItemAmmount < _scrollHeight/_itemHeight ) 
            {
                ScrollWidth = 0;
                return;
            }

            RunnerBruttoWalkSpace = _scrollHeight - ( _upDownButtonHeightWigth * 2 );
            _proportion = ( _itemHeight * _totalItemAmmount ) / RunnerBruttoWalkSpace;
            RunnerHeight = RunnerBruttoWalkSpace / _proportion;

            RealRunnerHeight = RunnerHeight;

            if ( RunnerHeight < _minRunnerHeight )
            {
                RunnerHeight = _minRunnerHeight;
            }

            RunnerWalkSpace = RunnerBruttoWalkSpace - RunnerHeight;
            _runnerStep = RunnerWalkSpace/_totalItemAmmount;
            ScrollOffset = 0;
            ScrollWidth = _upDownButtonHeightWigth;

            SetUpIcons ();
        }


        private void SetUpIcons ()
        {
            string correctnessIcon = App.ResourceDirectoryUri + _correctnessIcon;
            string incorrectnessIcon = App.ResourceDirectoryUri + _incorrectnessIcon;

            Uri correctUri = new Uri (correctnessIcon);
            CorrectnessIcon = ImageHelper.LoadFromResource (correctUri);
            Uri incorrectUri = new Uri (incorrectnessIcon);
            IncorrectnessIcon = ImageHelper.LoadFromResource (incorrectUri);

            CorrectnessOpacity = 1;
            IncorrectnessOpacity = 1;
        }


        private void SetRunner ( )
        {
            ScrollOffset = 0;
        }


        internal void ChangeScrollHeight ( double delta )
        {
            try
            {
                _scrollHeight -= delta;
                _entireBlockHeight -= delta;
                EntireBlockHeight -= delta;
                ScrollHeight -= delta;
                int newVisibleRange = ( int ) ( _scrollHeight / _itemHeight );
                int diff = newVisibleRange - _visibleRange;
                _visibleRange = newVisibleRange;
                _visibleRangeEnd += diff;
                RunnerBruttoWalkSpace -= delta;

                VisibleIcons = new ObservableCollection <BadgeCorrectnessViewModel> ();

                

                for ( int index = 0;   index < _visibleRange;   index++ )
                {
                    VisibleIcons.Add (new BadgeCorrectnessViewModel (false, _allReadonlyBadges [_scrollStepNumber + index]));
                }

                ActiveIcon = VisibleIcons [BeingProcessedNumber - _scrollStepNumber - 1];
                HighLightChosenIcon (ActiveIcon);
            }
            catch ( Exception ex ) {}
        }


        internal void ScrollUp ( )
        {
            ScrollUpAtOneStep ();
            ShowActiveIconIfInRange ();
        }


        internal void ScrollUpAtOneStep ()
        {
            if ( _scrollStepNumber == 0 )
            {
                return;
            }

            _scrollStepNumber--;

            ObservableCollection<BadgeCorrectnessViewModel> iconsCopy = VisibleIcons;
            int index = 0;

            VisibleIcons = new ObservableCollection <BadgeCorrectnessViewModel> ();

            int badgeNumberInCurrentList = ( _scrollStepNumber + index );

            BadgeCorrectnessViewModel firstIcon = GetCorrespondingIcon (badgeNumberInCurrentList);

            VisibleIcons.Add (firstIcon);

            for ( ; index < ( _visibleRange - 1 );   index++ )
            {
                VisibleIcons.Add (iconsCopy [index]);
            }

            _runnerHasWalked -= _runnerStep;
            ScrollOffset = Math.Round (_runnerHasWalked);
        }


        internal void ScrollDown ( )
        {
            ScrollDownAtOneStep ();
            ShowActiveIconIfInRange ();
        }


        private void ScrollDownAtOneStep ()
        {
            if ( _scrollStepNumber == _currentAmmount - _visibleRange )
            {
                return;
            }

            _scrollStepNumber++;

            ObservableCollection <BadgeCorrectnessViewModel> iconsCopy = VisibleIcons;
            VisibleIcons = new ObservableCollection <BadgeCorrectnessViewModel> ();
            
            int index = 1;

            for ( ;   index < _visibleRange;   index++ )
            {
                VisibleIcons.Add (iconsCopy [index]);
            }

            int badgeNumberInCurrentList = ( _scrollStepNumber + index - 1 );

            BadgeCorrectnessViewModel lastIcon = GetCorrespondingIcon (badgeNumberInCurrentList);

            VisibleIcons.Add (lastIcon);

            _runnerHasWalked += _runnerStep;
            ScrollOffset = _runnerHasWalked;
        }


        private BadgeCorrectnessViewModel GetCorrespondingIcon ( int badgeNumber )
        {
            BadgeCorrectnessViewModel result = null;
            bool isCorrect = false;
            BadgeViewModel goalBadge = null;

            if ( _filterState == FilterChoosing.Incorrects )
            {
                try 
                {
                    // to try find badge in corresponding list if it has opposite state

                    goalBadge = AllBadges [IncorrectBadges.ElementAt(badgeNumber).Key];
                    isCorrect = goalBadge.IsCorrect;
                }
                catch ( Exception ex ) 
                {
                    goalBadge = _allReadonlyBadges [IncorrectBadges.ElementAt (badgeNumber).Key];
                    isCorrect = false;
                }
            }
            else if ( _filterState == FilterChoosing.Corrects )
            {
                try
                {
                    goalBadge = AllBadges [FixedBadges.ElementAt (badgeNumber).Key];
                    isCorrect = goalBadge.IsCorrect;
                }
                catch ( Exception ex ) { }
            }
            else 
            {
                try
                {
                    goalBadge = AllBadges [badgeNumber];
                    isCorrect = goalBadge.IsCorrect;
                }
                catch ( Exception ex ) 
                {
                    goalBadge = _allReadonlyBadges [badgeNumber];
                    isCorrect = false;
                }
            }

            result = new BadgeCorrectnessViewModel (isCorrect, goalBadge);

            return result;
        }


        internal void ShiftRunner ( double dastinationPointer )
        {
            bool dastinationIsOnRunner = ( dastinationPointer >= _runnerHasWalked )
                                      && ( dastinationPointer <= (_runnerHasWalked + RunnerHeight) );

            if (dastinationIsOnRunner) 
            {
                return;
            }

            _scrollStepNumber = (int)(dastinationPointer/_runnerStep);
            double wayMustWalk = _scrollStepNumber * _runnerStep;

            if (_scrollStepNumber >= (_currentAmmount - _visibleRange)) 
            {
                wayMustWalk = ( RunnerBruttoWalkSpace - RunnerHeight );
                _scrollStepNumber = ( _currentAmmount - _visibleRange );
            }

            VisibleIcons = new ObservableCollection <BadgeCorrectnessViewModel> ();

            for ( int index = 0;   index < _visibleRange;   index++ )
            {
                VisibleIcons.Add (GetCorrespondingIcon (_scrollStepNumber + index));
            }
            
            _runnerHasWalked = wayMustWalk;
            ScrollOffset = wayMustWalk;
            ShowActiveIconIfInRange ();
        }


        internal void ScrollByWheel ( bool isDirectionUp )
        {
            if ( isDirectionUp )
            {
                ScrollUp ();
            }
            else 
            {
                ScrollDown ();
            }
        }


        private void ShowActiveIconIfInRange ( )
        {
            bool activeIconIsVisible = ( BeingProcessedNumber > _scrollStepNumber )
                           && ( BeingProcessedNumber < ( _scrollStepNumber + _visibleRange + 1 ) );

            if ( activeIconIsVisible )
            {
                _numberAmongLoadedIcons = (BeingProcessedNumber - _scrollStepNumber);
                _visibleRangeEnd = (_scrollStepNumber + _visibleRange - 1);
                SaveSliderStation ();
                ActiveIcon = VisibleIcons [BeingProcessedNumber - _scrollStepNumber - 1];
                HighLightChosenIcon (ActiveIcon);
            }
        }


        private void SetSliderToStationBeforeScrollingIfShould ( )
        {
            if ( ! _visibleIconsStorage.Equals (VisibleIcons) )
            {
                VisibleIcons = _visibleIconsStorage;
                _scrollStepNumber = _scrollStepNumberStorage;
                _runnerHasWalked = _runnerHasWalkedStorage;
                ScrollOffset = _scrollOffsetStorage;
            }
        }


        private void SaveSliderStation ()
        {
            _scrollStepNumberStorage = _scrollStepNumber;
            _runnerHasWalkedStorage = _runnerHasWalked;
            _scrollOffsetStorage = ScrollOffset;
            _visibleIconsStorage = VisibleIcons;
        }


        private void HighLightChosenIcon ( BadgeCorrectnessViewModel icon )
        {
            //icon.BorderColor = new SolidColorBrush (MainWindow.black);
            icon.BoundFontWeight = FontWeight.Bold;
            icon.IconOpacity = _chosenOpacity;
        }


        private void FadeIcon ( BadgeCorrectnessViewModel icon )
        {
            //icon.BorderColor = new SolidColorBrush (MainWindow.white);
            icon.BoundFontWeight = FontWeight.Normal;
            icon.IconOpacity = _normalOpacity;
        }


        internal void ExtendOrShrinkCollectionManagement ()
        {
            if ( _filterIsOpen )
            {
                CollectionFilterMargin = new Thickness (_namesFilterWidth, 0);
                WorkAreaWidth += _namesFilterWidth;
                _filterIsOpen = false;
            }
            else
            {
                CollectionFilterMargin = new Thickness (0, 0);
                WorkAreaWidth -= _namesFilterWidth;
                _filterIsOpen = true;
            }
        }


        private void ResetActiveIcon ()
        {
            //BadgeViewModel draftSource = null;

            //foreach ( var keyValue   in   _drafts )
            //{
            //    if ( keyValue.Value.Equals (BeingProcessedBadge) )
            //    {
            //        draftSource = keyValue.Key;
            //        break;
            //    }
            //}

            if ( BeingProcessedBadge. IsCorrect )
            {
                if ( ! ActiveIcon. Correctness ) 
                {
                    ActiveIcon = new BadgeCorrectnessViewModel (true, BeingProcessedBadge);
                }

                //VisibleIcons [BeingProcessedNumber - 1] = new BadgeCorrectnessViewModel (true, BeingProcessedBadge);

                //if ( ! FixedBadges.ContainsValue (BeingProcessedBadge) )
                //{
                //    FixedBadges.Add ((FixedBadges. Count), BeingProcessedBadge);

                //    List<int> ints = new List<int> ();
                //    ints.Sort ();
                //}

                //if ( IncorrectBadges.ContainsValue (BeingProcessedBadge) )
                //{
                //    IncorrectBadges.Remove (draftSource);
                //}

                bool firstIncorrectIsRewrote = FirstIncorrect.Equals (BeingProcessedBadge);

                bool shouldSetFirstIncorrect = ( BeingProcessedBadge.Id != ( _allReadonlyBadges.Count - 1 ) )
                                               && ( ( _filterState == FilterChoosing.All )
                                                     || ( _filterState == FilterChoosing.Incorrects )
                                                  );

                if ( firstIncorrectIsRewrote   &&   shouldSetFirstIncorrect )
                {
                    _incorrectsAmmount--;

                    for ( int index = BeingProcessedBadge.Id + 1;   index < AllBadges.Count - 1;   index++ )
                    {
                        if ( AllBadges.ContainsKey (index) )
                        {
                            if ( ! AllBadges [index].IsCorrect )
                            {
                                FirstIncorrect = AllBadges [index];
                                break;
                            }
                            else { continue; }
                        }
                        else
                        {
                            FirstIncorrect = _allReadonlyBadges [index].Clone ();
                            break;
                        }
                    }
                }

                try
                {
                    _fixedBadges [BeingProcessedBadge. Id] = _allReadonlyBadges [BeingProcessedBadge. Id];
                    _incorrectBadges [BeingProcessedBadge. Id] = null;
                    FixedBadges.Add (BeingProcessedBadge. Id, BeingProcessedBadge. Id);
                    IncorrectBadges.Remove (BeingProcessedBadge. Id);
                }
                catch ( Exception ex ) { }
            }
            else
            {
                if ( ActiveIcon. Correctness )
                {
                    ActiveIcon = new BadgeCorrectnessViewModel (false, BeingProcessedBadge);
                }

                //    if ( VisibleIcons [BeingProcessedNumber - 1].Correctness )
                //    {
                //        VisibleIcons [BeingProcessedNumber - 1] = new BadgeCorrectnessViewModel (false, BeingProcessedBadge);
                //    }

                //    if ( ! IncorrectBadges.ContainsValue (BeingProcessedBadge) )
                //    {
                //        IncorrectBadges.Add (draftSource, BeingProcessedBadge);
                //    }

                //    if ( FixedBadges.ContainsValue (BeingProcessedBadge) )
                //    {
                //        FixedBadges.Remove (draftSource);
                //    }

                _incorrectsAmmount++;

                try
                {
                    _incorrectBadges [BeingProcessedBadge. Id] = _allReadonlyBadges [BeingProcessedBadge. Id];
                    _fixedBadges [BeingProcessedBadge. Id] = null;
                    IncorrectBadges.Add (BeingProcessedBadge. Id, BeingProcessedBadge. Id);
                    FixedBadges.Remove(BeingProcessedBadge. Id );
                }
                catch ( Exception ex ) { }
            }

            try
            {
                AllBadges.Add (BeingProcessedBadge. Id, BeingProcessedBadge);
            }
            catch ( Exception ex ) { }

            //ActiveIcon = VisibleIcons [BeingProcessedNumber - 1];
            //HighLightChosenIcon (ActiveIcon);
        }


        private void SetVisibleIcons ()
        {
            VisibleIcons = new ObservableCollection <BadgeCorrectnessViewModel> ();
            _visibleIconsStorage = new ObservableCollection <BadgeCorrectnessViewModel> ();

            if ( ( _allReadonlyBadges != null )   &&   ( _allReadonlyBadges. Count > 0 )   &&   ( _visibleRange > 0 ) )
            {
                for ( int index = 0;   index < _visibleRange;   index++ )
                {
                    BadgeCorrectnessViewModel icon = new BadgeCorrectnessViewModel (false, _allReadonlyBadges [index]);
                    VisibleIcons.Add (icon);
                    _visibleIconsStorage.Add (icon);
                    FadeIcon (icon);
                }

                ActiveIcon = VisibleIcons [0];
                HighLightChosenIcon (ActiveIcon);
            }
        }
    }
}