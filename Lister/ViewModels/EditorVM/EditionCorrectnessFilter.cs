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
using ExCSS;
using System;
using AvaloniaEdit.Utils;
using System.Collections.Immutable;
using ReactiveUI;
using Avalonia;

namespace Lister.ViewModels
{
    public partial class BadgeEditorViewModel : ViewModelBase
    {
        private readonly double _switcherWidth = 32;
        private readonly double _filterLabelWidth = 70;

        private readonly SolidColorBrush _switcherAllForeground = 
                                                          new SolidColorBrush (new Avalonia.Media.Color (255, 250, 250, 250));
        private readonly SolidColorBrush _switcherCorrectForeground = 
                                                          new SolidColorBrush (new Avalonia.Media.Color (255, 97, 184, 97));
        private readonly SolidColorBrush _switcherIncorrectForeground = 
                                                          new SolidColorBrush (new Avalonia.Media.Color (255, 210, 54, 80));

        private readonly string _allFilter;
        private readonly string _incorrectFilter;
        private readonly string _correctFilter;
        private readonly string _allTip;
        private readonly string _correctTip;
        private readonly string _incorrectTip;

        private readonly double _narrowCorrectnessWidthLimit = 155;
        private readonly int _narrowMinCorrectnessTextLength = 14;
        private readonly int _narrowMaxCorrectnessTextLength = 20;

        private readonly double _wideCorrectnessWidthLimit = 160;
        private readonly int _wideMinCorrectnessTextLength = 15;
        private readonly int _wideMaxCorrectnessTextLength = 21;

        private FilterChoosing _filterState = FilterChoosing.All;
        private double _correctnessWidthLimit;
        private int _minCorrectnessTextLength;
        private int _maxCorrectnessTextLength;

        private ScrollWideness _scrollWideness = ScrollWideness.Wide;

        private bool _isDropOpen;
        internal bool IsDropDownOpen
        {
            get { return _isDropOpen; }
            set
            {
                this.RaiseAndSetIfChanged (ref _isDropOpen, value, nameof (IsDropDownOpen));
            }
        }

        private bool _isComboboxEnabled;
        internal bool IsComboboxEnabled
        {
            get { return _isComboboxEnabled; }
            set
            {
                this.RaiseAndSetIfChanged (ref _isComboboxEnabled, value, nameof (IsComboboxEnabled));
            }
        }

        private double _switcherWidt;
        internal double SwitcherWidth
        {
            get { return _switcherWidt; }
            set
            {
                this.RaiseAndSetIfChanged (ref _switcherWidt, value, nameof (SwitcherWidth));
            }
        }

        private double _filterLableWidth;
        internal double FilterLabelWidth
        {
            get { return _filterLableWidth; }
            set
            {
                this.RaiseAndSetIfChanged (ref _filterLableWidth, value, nameof (FilterLabelWidth));
            }
        }

        private int _comboboxSelectedIndex;
        internal int FilterSelectedIndex
        {
            get { return _comboboxSelectedIndex; }
            set
            {
                this.RaiseAndSetIfChanged (ref _comboboxSelectedIndex, value, nameof (FilterSelectedIndex));
            }
        }

        private string _switcherTip;
        internal string SwitcherTip
        {
            get { return _switcherTip; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _switcherTip, value, nameof (SwitcherTip));
            }
        }

        private SolidColorBrush _switcherForeground;
        internal SolidColorBrush SwitcherForeground
        {
            get { return _switcherForeground; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _switcherForeground, value, nameof (SwitcherForeground));
            }
        }


        internal bool IsProcessableChangedInSpecificFilter ( int filterableNumber )
        {
            bool filterOccured = false;

            if ( _filterState == FilterChoosing.All )
            {
                return false;
            }
            else if ( _filterState == FilterChoosing.Corrects )
            {
                if ( ! BeingProcessedBadge.IsCorrect )
                {
                    filterOccured = true;
                }
            }
            else if ( _filterState == FilterChoosing.Incorrects )
            {
                if ( BeingProcessedBadge.IsCorrect )
                {
                    filterOccured = true;
                }
            }

            return filterOccured;
        }


        internal void Filter ()
        {
            _runnerHasWalked = 0;

            if ( _filterState == FilterChoosing.All )
            {
                _filterState = FilterChoosing.Corrects;

                SwitcherForeground = _switcherCorrectForeground;
                SwitcherTip = _correctTip;
                FilterSelectedIndex = 1;

                TryChangeSpecificLists ();

                ScrollWidth = 0;

                CorrectNumbered.Sort (_comparer);
                CurrentVisibleCollection = CorrectNumbered;
                IncorrectBadgesCount = 0;
            }
            else if ( _filterState == FilterChoosing.Corrects )
            {
                _filterState = FilterChoosing.Incorrects;

                SwitcherForeground = _switcherIncorrectForeground;
                SwitcherTip = _incorrectTip;
                FilterSelectedIndex = 2;

                TryChangeSpecificLists ();

                IncorrectNumbered.Sort (_comparer);
                CurrentVisibleCollection = IncorrectNumbered;
                IncorrectBadgesCount = CurrentVisibleCollection.Count;
            }
            else if ( _filterState == FilterChoosing.Incorrects )
            {
                _filterState = FilterChoosing.All;
                CurrentVisibleCollection = AllNumbered;

                SwitcherForeground = _switcherAllForeground;
                SwitcherTip = _allTip;
                FilterSelectedIndex = 0;

                TryChangeSpecificLists ();

                IncorrectBadgesCount = IncorrectNumbered. Count;
            }

            ProcessableCount = CurrentVisibleCollection.Count;
            SetSliderWideness ();
            CalcVisibleRange (CurrentVisibleCollection.Count);
            SetScroller (CurrentVisibleCollection.Count);
            SetAccordingIcons ();
            EnableNavigation ();
        }


        private void SetAccordingIcons ()
        {
            BeingProcessedBadge = null;
            VisibleIcons = new ();
            NextOnSliderIsEnable = true;
            NextIsEnable = true;
            LastIsEnable = true;

            if ( _filterState == FilterChoosing.All )
            {
                SetMixedIcons ();
            }
            else if ( _filterState == FilterChoosing.Corrects )
            {
                SetIconsForCorrectFilter ();
            }
            else if ( _filterState == FilterChoosing.Incorrects )
            {
                SetIconsForIncorrectFilter ();
            }

            _numberAmongVisibleIcons = 1;
            _scrollStepIndex = 0;

            if ( BeingProcessedBadge != null )
            {
                SetToCorrectScale (BeingProcessedBadge);
                BeingProcessedNumber = 1;
                BeingProcessedBadge.Show ();
                ZeroSliderStation (VisibleIcons);
            }
            else 
            {
                BeingProcessedNumber = 0;
            }

            if ( VisibleIcons. Count == 0 )
            {
                UpDownWidth = 0;
                UpDownIsFocusable = false;
                FirstIsEnable = false;
                PreviousIsEnable = false;
                NextIsEnable = false;
                LastIsEnable = false;
            }
            else 
            {
                UpDownWidth = _upDownWidth;
                UpDownIsFocusable = true;
            }
        }


        private void SetMixedIcons ()
        {
            int counter = 0;

            foreach ( BadgeViewModel badge   in   AllNumbered )
            {
                if ( counter == _visibleRange )
                {
                    break;
                }

                VisibleIcons.Add (new BadgeCorrectnessViewModel ( badge, _extendedScrollableIconWidth, _shrinkedIconWidth
                                                                                  , _correctnessWidthLimit, FilterIsExtended));

                counter++;
            }

            BeingProcessedBadge = AllNumbered.ElementAt (0);
            VisibleIcons [0].BoundFontWeight = Avalonia.Media.FontWeight.Bold;
            VisibleIcons [0].CalcStringPresentation (_correctnessWidthLimit);
            ActiveIcon = VisibleIcons [0];
        }


        private void SetIconsForCorrectFilter ()
        {
            int existingCounter = 0;
            int firstExistingCommonNumber = -1;

            foreach ( BadgeViewModel badge   in   CorrectNumbered )
            {
                if ( existingCounter == _visibleRange )
                {
                    break;
                }

                VisibleIcons.Add (new BadgeCorrectnessViewModel (badge, _extendedScrollableIconWidth, _shrinkedIconWidth
                                                                                  , _correctnessWidthLimit, FilterIsExtended));

                if ( existingCounter == 0 )
                {
                    VisibleIcons [existingCounter].BoundFontWeight = Avalonia.Media.FontWeight.Bold;
                    VisibleIcons [existingCounter].CalcStringPresentation (_correctnessWidthLimit);
                    firstExistingCommonNumber = badge.Id;
                }

                existingCounter++;
            }

            if ( firstExistingCommonNumber > -1 )
            {
                ActiveIcon = VisibleIcons [0];
                BeingProcessedBadge = CorrectNumbered.ElementAt (0);
            }
        }


        private void SetIconsForIncorrectFilter ()
        {
            int existingCounter = 0;
            int firstExistingCommonNumber = -1;

            foreach ( BadgeViewModel badge   in   IncorrectNumbered )
            {
                if ( existingCounter == _visibleRange )
                {
                    break;
                }

                VisibleIcons.Add (new BadgeCorrectnessViewModel ( badge, _extendedScrollableIconWidth, _shrinkedIconWidth
                                                                                   , _correctnessWidthLimit, FilterIsExtended));

                if ( existingCounter == 0 )
                {
                    VisibleIcons [existingCounter].BoundFontWeight = Avalonia.Media.FontWeight.Bold;
                    VisibleIcons [existingCounter].CalcStringPresentation (_correctnessWidthLimit);
                    firstExistingCommonNumber = badge.Id;
                }

                existingCounter++;
            }

            if ( firstExistingCommonNumber > -1 )
            {
                ActiveIcon = VisibleIcons [0];
                BeingProcessedBadge = IncorrectNumbered.ElementAt (0);
            }
        }


        private void TryChangeSpecificLists () 
        {
            if ( BeingProcessedBadge == null ) 
            {
                return;
            }

            if ( BeingProcessedBadge. IsCorrect )
            {
                if( ! CorrectNumbered.Contains(BeingProcessedBadge) )
                {
                    CorrectNumbered.Add (BeingProcessedBadge);

                    if ( IncorrectNumbered.Contains (BeingProcessedBadge) )
                    {
                        IncorrectNumbered.Remove (BeingProcessedBadge);
                    }
                }
            }
            else if ( ! BeingProcessedBadge. IsCorrect )
            {
                if ( ! IncorrectNumbered.Contains (BeingProcessedBadge) )
                {
                    IncorrectNumbered.Add (BeingProcessedBadge);

                    if ( CorrectNumbered.Contains (BeingProcessedBadge) )
                    {
                        CorrectNumbered.Remove (BeingProcessedBadge);
                    }      
                }
            }
        }


        private void SetSliderWideness ( )
        {
            if ( CurrentVisibleCollection.Count > _visibleRange )
            {
                _correctnessWidthLimit = _narrowCorrectnessWidthLimit;
                _minCorrectnessTextLength = _narrowMinCorrectnessTextLength;
                _maxCorrectnessTextLength = _narrowMaxCorrectnessTextLength;
            }
            else
            {
                _correctnessWidthLimit = _wideCorrectnessWidthLimit;
                _minCorrectnessTextLength = _wideMinCorrectnessTextLength;
                _maxCorrectnessTextLength = _wideMaxCorrectnessTextLength;
            }
        }


        internal void Filter ( string filterName )
        {
            ReleaseCaptured ();
            _runnerHasWalked = 0;

            bool appLoadingIs = ( AllNumbered == null ) 
                                || ( CorrectNumbered == null ) 
                                || ( IncorrectNumbered == null );

            if (appLoadingIs) 
            {
                return;
            }

            if ( filterName == _allFilter )
            {
                _filterState = FilterChoosing.All;
                CurrentVisibleCollection = AllNumbered;

                SwitcherForeground = _switcherAllForeground;
                SwitcherTip = _allTip;
                TryChangeSpecificLists ();

                IncorrectBadgesCount = IncorrectNumbered.Count;
            }
            else if ( filterName == _correctFilter )
            {
                _filterState = FilterChoosing.Corrects;

                SwitcherForeground = _switcherCorrectForeground;
                SwitcherTip = _correctTip;

                TryChangeSpecificLists ();

                CorrectNumbered.Sort (_comparer);
                CurrentVisibleCollection = CorrectNumbered;
                IncorrectBadgesCount = 0;
            }
            else if ( filterName == _incorrectFilter )
            {
                _filterState = FilterChoosing.Incorrects;

                SwitcherForeground = _switcherIncorrectForeground;
                SwitcherTip = _incorrectTip;

                TryChangeSpecificLists ();

                IncorrectNumbered.Sort (_comparer);
                CurrentVisibleCollection = IncorrectNumbered;
                IncorrectBadgesCount = CurrentVisibleCollection.Count;
            }

            ProcessableCount = CurrentVisibleCollection.Count;
            SetSliderWideness ();
            CalcVisibleRange (CurrentVisibleCollection.Count);
            SetScroller (CurrentVisibleCollection.Count);
            SetAccordingIcons ();
            EnableNavigation ();
            ExtendOrShrinkSliderItems ();
        }


        internal void ExtendOrShrinkCollectionManagement ()
        {
            if ( FilterIsExtended )
            {
                FilterBlockMargin = new Thickness (_collectionFilterMarginLeft, 0);
                FilterIsExtended = false;
                ExtenderContent = "\uF060";
                SwitcherWidth = _switcherWidth;
                FilterLabelWidth = 0;
                IsComboboxEnabled = false;
                ExtentionTip = _extentionToolTip;

                TryEnableSliderUpDown (VisibleIcons.Count);
                ExtendOrShrinkSliderItems ();
            }
            else
            {
                FilterBlockMargin = new Thickness (0, 0);
                FilterIsExtended = true;
                ExtenderContent = "\uF061";
                SwitcherWidth = 0;
                FilterLabelWidth = _filterLabelWidth;
                IsComboboxEnabled = true;
                ExtentionTip = _shrinkingToolTip;

                TryEnableSliderUpDown (0);
                ExtendOrShrinkSliderItems ();
            }
        }


        internal void ExtendOrShrinkSliderItems ()
        {
            double width;

            if ( FilterIsExtended )
            {
                double scrollerItemsCount = _scrollerHeight / _itemHeightWithMargin;

                if ( CurrentVisibleCollection. Count > scrollerItemsCount )
                {
                    width = _extendedScrollableIconWidth;
                }
                else 
                {
                    width = _mostExtendedIconWidth;
                }
            }
            else 
            {
                width = _shrinkedIconWidth;
            }

            foreach ( BadgeCorrectnessViewModel item   in   VisibleIcons )
            {
                item.Width = width;
            }
        }



        private enum ScrollWideness 
        {
            Wide = 0,
            Narrow = 1
        }
    }
}