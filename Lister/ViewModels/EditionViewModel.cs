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
using static QuestPDF.Helpers.Colors;
using DynamicData;

namespace Lister.ViewModels
{
    public partial class BadgeEditorViewModel : ViewModelBase
    {
        public static event BackingToMainViewHandler BackingToMainViewEvent;

        //internal static readonly double _scale = 2.44140625;
        internal static double _scale = 1.5624;
        //internal static readonly double _scale = 2.5;

        private Dictionary <BadgeViewModel, double> _scaleStorage;

        private List <BadgeViewModel> _allReadonlyBadges;
        private List <BadgeViewModel> _incorrectBadges;
        private List <BadgeViewModel> _fixedBadges;

        private TextLineViewModel _splittable;
        private TextLineViewModel _focusedLine;

        private bool _incorrectsAreSet = false;
        private BadgeEditorView _view;
        private int _incorrectsAmmount;
        private int _visibleRangeStart = 0;
        private int _visibleRangeEnd;

        internal double WidthDelta { get; set; }
        internal double HeightDelta { get; set; }
        //internal bool ScrollChangedByNavigation { get; private set; }

        private double wAW;
        internal double WorkAreaWidth
        {
            get { return wAW; }
            set
            {
                this.RaiseAndSetIfChanged (ref wAW, value, nameof (WorkAreaWidth));
            }
        }

        private Dictionary <BadgeViewModel, BadgeViewModel> _drafts;

        internal Dictionary <int, BadgeViewModel> AllBadges { get; private set; }

        internal BadgeViewModel FirstIncorrect { get; private set; }
        //private Dictionary <BadgeViewModel, BadgeViewModel> inc;
        internal Dictionary <int, int> IncorrectBadges { get; private set; }

        //private Dictionary <BadgeViewModel, BadgeViewModel> fx;
        internal Dictionary <int, int> FixedBadges { get; private set; }

        //{
        //    get { return inc; }
        //    private set
        //    {
        //        this.RaiseAndSetIfChanged (ref inc, value, nameof (IncorrectBadges));
        //    }
        //}

        private int incBC;
        internal int IncorrectBadgesCount
        {
            get { return incBC; }
            private set
            {
                this.RaiseAndSetIfChanged (ref incBC, value, nameof (IncorrectBadgesCount));
            }
        }

        //{
        //    get { return fx; }
        //    private set
        //    {
        //        this.RaiseAndSetIfChanged (ref fx, value, nameof (FixedBadges));
        //    }
        //}

        private BadgeViewModel bpB;
        internal BadgeViewModel BeingProcessedBadge
        {
            get { return bpB; }
            private set
            {
                this.RaiseAndSetIfChanged (ref bpB, value, nameof (BeingProcessedBadge));
            }
        }

        private string fT;
        internal string FocusedText
        {
            get { return fT; }
            set
            {
                this.RaiseAndSetIfChanged (ref fT, value, nameof (FocusedText));
            }
        }

        private int pC;
        internal int ProcessableCount
        {
            get { return pC; }
            private set
            {
                this.RaiseAndSetIfChanged (ref pC, value, nameof (ProcessableCount));
            }
        }

        private bool mE;
        internal bool MoversAreEnable
        {
            get { return mE; }
            private set
            {
                this.RaiseAndSetIfChanged (ref mE, value, nameof (MoversAreEnable));
            }
        }

        private bool sE;
        internal bool SplitterIsEnable
        {
            get { return sE; }
            private set
            {
                this.RaiseAndSetIfChanged (ref sE, value, nameof (SplitterIsEnable));
            }
        }

        //private string fS;
        //internal string FocusedFontSize
        //{
        //    get { return fS; }
        //    private set
        //    {
        //        this.RaiseAndSetIfChanged (ref fS, value, nameof (FocusedFontSize));
        //    }
        //}

        //private Thickness fBT;
        //internal Thickness FocusedBorderThickness
        //{
        //    get { return fBT; }
        //    private set
        //    {
        //        this.RaiseAndSetIfChanged (ref fBT, value, nameof (FocusedBorderThickness));
        //    }
        //}


        internal static void OnBackingToMainView ()
        {
            if ( BackingToMainViewEvent == null )
            {
                return;
            }

            BackingToMainViewEvent ();
        }


        public BadgeEditorViewModel ( int incorrectBadgesAmmount )
        {
            _scaleStorage = new ();
            _allReadonlyBadges = new ();
            _incorrectBadges = new ();
            _fixedBadges = new ();
            _drafts = new ();

            WorkAreaWidth = _workAreaWidth + _namesFilterWidth;
            SetUpSliderBlock ( incorrectBadgesAmmount );

            //SetUpIcons ();
            ChangeScrollHeight (0);
            SetUpZoommer ();

            SplitterIsEnable = false;
            IncorrectBadges = new ();
            FixedBadges = new ();
        }


        #region Change viewSize

        internal void ChangeSize ( double widthDifference, double heightDifference )
        {
            ChangeScrollHeight (heightDifference);
            WorkAreaWidth -= widthDifference;
            //EntireBlockHeight -= heightDifference;
            //ScrollHeight -= heightDifference;
        }


        //internal void ChangeSliderHeight ( double delta )
        //{
        //    double oldHeight = _scrollHeight;
        //    _scrollHeight -= delta;

        //    int oldRange = _visibleRange;
        //    _visibleRange = ( int ) ( _scrollHeight / _itemHeight );

        //    _visibleRangeEnd += _visibleRange - oldRange;

        //    //SaveProcessableIconVisible ();
        //}

        #endregion


        internal void CancelChanges ()
        {
            if ( BeingProcessedBadge == null ) 
            {
                return;
            }

            BeingProcessedBadge.Hide ();
            MoversAreEnable = false;
            SplitterIsEnable = false;
            ZoommerIsEnable = false;
            _view.zoomOn.IsEnabled = false;
            _view.zoomOut.IsEnabled = false;

            BadgeViewModel draftSource = null;
            int counter = 0;

            foreach ( var keyValue   in   _drafts )
            {
                if ( keyValue.Value.Equals (BeingProcessedBadge) )
                {
                    draftSource = keyValue.Key;
                    break;
                }

                counter++;
            }

            if ( FixedBadges.ContainsValue(counter) )
            {
                FixedBadges.Remove (counter);
            }

            if ( IncorrectBadges.ContainsValue (counter) )
            {
                IncorrectBadges.Remove (counter);
            }

            BeingProcessedBadge = draftSource.Clone ();
            _drafts [draftSource] = BeingProcessedBadge;
            AllBadges [counter] = BeingProcessedBadge;
            IncorrectBadges.Add (counter, counter);
            SetToCorrectScale (BeingProcessedBadge);
            BeingProcessedBadge.Show ();
            BeingProcessedBadge.CheckCorrectnessAfterCancelation ();
            
            ResetActiveIcon ();
        }


        internal void PassIncorrects ( List <BadgeViewModel> incorrects )
        {
            bool isNullOrEmpty = ( incorrects == null )   ||   ( incorrects.Count == 0 );

            if ( isNullOrEmpty )
            {
                return;
            }

            AllBadges = new ();
            IncorrectBadges = new ();
            FixedBadges = new ();
            _scaleStorage = new Dictionary<BadgeViewModel, double> ();
            _incorrectsAmmount = incorrects.Count;
            _currentAmmount = incorrects.Count;

            int counter = 0;

            foreach ( BadgeViewModel badge   in   incorrects )
            {
                if ( badge == null )
                {
                    continue;
                }

                AddToScalesStorage (badge);
                IncorrectBadges.Add (counter, counter);
                _allReadonlyBadges.Add (badge);
                _incorrectBadges.Add (badge);
                _fixedBadges.Add (null);

                if ( counter < 1 )
                {
                    Dispatcher.UIThread.InvokeAsync
                    (
                        () =>
                        {
                            BadgeViewModel clone = badge.Clone ();
                            _drafts.Add (badge, clone);
                            AllBadges.Add (0, clone);
                            FirstIncorrect = clone;
                        }
                    );
                }
                counter++;
            }

            //_loadedStartEndPairs.Add (new int [2] { 0, ( _visibleRange - 1 ) });
            //_containingPairExists = true;
            //_incorrectsAreSet = true;

            Dispatcher.UIThread.InvokeAsync
            (
                () =>
                {
                    SetBeingProcessed (AllBadges [0]);
                    EnableNavigation ();
                    SetVisibleIcons ();
                    ScrollOffset = 0;
                }
            );

            ProcessableCount = _scaleStorage.Count;
            IncorrectBadgesCount = _scaleStorage.Count;
        }


        internal void PassView ( BadgeEditorView view )
        {
            _view = view;
        }


        private bool ChangesExist ( )
        {
            bool exist = false;

            foreach ( KeyValuePair <int, int> badgeNumber   in   FixedBadges ) 
            {
                if ( AllBadges [badgeNumber.Value].IsChanged ) 
                {
                    exist = true;
                    return exist;
                }
            }

            foreach ( KeyValuePair <int, int> badgeNumber   in   IncorrectBadges )
            {
                if ( AllBadges [badgeNumber.Value].IsChanged )
                {
                    exist = true;
                    return exist;
                }
            }

            return exist;
        }


        internal void GoBackCommand ()
        {
            bool changesExist = ChangesExist ();

            if ( changesExist )
            {
                _view.CheckBacking ();
            }
            else
            {
                GoBack ();
            }
        }


        internal void ComplateGoBack ( BadgeEditorView caller )
        {
            if ( _view.Equals(caller) )
            {
                GoBack();
            }
            
        }


        private void GoBack ()
        {
            SetDraftResultsInSource ( );

            foreach ( KeyValuePair <BadgeViewModel, double> badgeScale   in   _scaleStorage )
            {
                BadgeViewModel badge = badgeScale.Key;
                double scale = badgeScale.Value;
                badge.ZoomOut (_scale, true);
                SetOriginalScale (badge, scale);
            }

            OnBackingToMainView ();
            _view.ComplateBacking ();
        }


        //private void SetDraftResultsInSource ( Dictionary <int, BadgeViewModel> rewritables )
        //{
        //    foreach ( KeyValuePair <intd, BadgeViewModel> badge   in   rewritables )
        //    {
        //        BadgeViewModel draftSource = null;

        //        foreach ( var keyValue   in   _drafts )
        //        {
        //            if ( keyValue.Value.Equals (badge.Value) )
        //            {
        //                draftSource = keyValue.Key;
        //                break;
        //            }
        //        }

        //        SetToCorrectScale (draftSource);

        //        if ( badge.Value.IsChanged )
        //        {
        //            badge.Value.ResetPrototype (draftSource);
        //        }
        //    }
        //}


        private void SetDraftResultsInSource ( )
        {
            foreach ( KeyValuePair <int, BadgeViewModel> badge   in   AllBadges )
            {
                BadgeViewModel draftSource = null;

                foreach ( var keyValue   in   _drafts )
                {
                    if ( keyValue.Value.Equals (badge.Value) )
                    {
                        draftSource = keyValue.Key;
                        break;
                    }
                }

                SetToCorrectScale (draftSource);

                if ( badge.Value.IsChanged )
                {
                    badge.Value.ResetPrototype (draftSource);
                }
            }
        }


        private void AddToScalesStorage ( BadgeViewModel addable )
        {
            if ( ! _scaleStorage.ContainsKey (addable) )
            {
                _scaleStorage.Add (addable, addable. Scale);
            }
        }


        private void SetBeingProcessed ( BadgeViewModel beingProcessed )
        {
            beingProcessed.Show ();
            BeingProcessedBadge = beingProcessed;
            SetToCorrectScale (BeingProcessedBadge);
            BeingProcessedNumber = 1;
        }



        private enum FilterChoosing 
        {
           All = 0,
           Corrects = 1,
           Incorrects = 2
        }
    }

}
