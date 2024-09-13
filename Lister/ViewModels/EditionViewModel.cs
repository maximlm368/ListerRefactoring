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

namespace Lister.ViewModels
{
    public partial class BadgeEditorViewModel : ViewModelBase
    {
        public static event BackingToMainViewHandler BackingToMainViewEvent;

        //internal static readonly double _scale = 2.44140625;
        internal static double _scale = 1.5624;
        //internal static readonly double _scale = 2.5;

        private Dictionary <BadgeViewModel, double> _scaleStorage;
        private List <BadgeViewModel> _badgeStorage;
        private TextLineViewModel _splittable;
        private TextLineViewModel _focusedLine;
        
        private bool _incorrectsAreSet = false;
        private BadgeEditorView _view;
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
        private List <BadgeViewModel> _vB;
        internal List <BadgeViewModel> VisibleBadges
        {
            get { return _vB; }
            set
            {
                bool isNullOrEmpty = ( value == null )   ||   ( value.Count == 0 );

                if ( isNullOrEmpty )
                {
                    return;
                }

                _vB = new List <BadgeViewModel> ();
                _scaleStorage = new Dictionary<BadgeViewModel, double> ();

                Dispatcher.UIThread.InvokeAsync
                (
                    () =>
                    {
                        IncorrectBadges = new ObservableCollection <BadgeViewModel> ();
                    }
                );

                int counter = 0;

                foreach ( BadgeViewModel badge   in   value )
                {
                    if ( badge == null )
                    {
                        continue;
                    }

                    AddToStorage (badge);
                    _badgeStorage.Add (badge);

                    if ( counter < _loadingBadgeCount )
                    {
                        Dispatcher.UIThread.InvokeAsync
                        (
                            () =>
                            {
                                BadgeViewModel clone = badge.Clone ();
                                _drafts.Add (badge, clone);
                                IncorrectBadges.Add (clone);
                                VisibleBadges.Add (clone);
                            }
                        );
                    }
                    counter++;
                }

                _loadedStartEndPairs.Add (new int [2] { 0, ( _loadingBadgeCount - 1 ) });
                _containingPairExists = true;
                //_incorrectsAreSet = true;

                Dispatcher.UIThread.InvokeAsync
                (
                    () =>
                    {
                        SetBeingProcessed (VisibleBadges [0]);
                        EnableNavigation ();
                        SetVisibleIcons ();
                        SliderOffset = new Vector (0, 0);
                    }
                );

                ProcessableCount = _scaleStorage.Count;
                IncorrectBadgesCount = _scaleStorage.Count;
            }
        }

        private ObservableCollection <BadgeViewModel> inc;
        internal ObservableCollection <BadgeViewModel> IncorrectBadges
        {
            get { return inc; }
            private set
            {
                this.RaiseAndSetIfChanged (ref inc, value, nameof (IncorrectBadges));
            }
        }

        private int incBC;
        internal int IncorrectBadgesCount
        {
            get { return incBC; }
            private set
            {
                this.RaiseAndSetIfChanged (ref incBC, value, nameof (IncorrectBadgesCount));
            }
        }

        private ObservableCollection <BadgeViewModel> fx;
        internal ObservableCollection <BadgeViewModel> FixedBadges
        {
            get { return fx; }
            private set
            {
                this.RaiseAndSetIfChanged (ref fx, value, nameof (FixedBadges));
            }
        }

        private BadgeViewModel bpB;
        internal BadgeViewModel BeingProcessedBadge
        {
            get { return bpB; }
            private set
            {
                this.RaiseAndSetIfChanged (ref bpB, value, nameof (BeingProcessedBadge));
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


        public BadgeEditorViewModel ( )
        {
            _scaleStorage = new ();
            _badgeStorage = new ();
            _drafts = new ();
            
            WorkAreaWidth = _workAreaWidth + _namesFilterWidth;
            SetUpSliderBlock ();

            //FocusedFontSize = string.Empty;

            SplitterIsEnable = false;
            IncorrectBadges = new ObservableCollection <BadgeViewModel> ();
            FixedBadges = new ObservableCollection <BadgeViewModel> ();

            SetUpIcons ();
            ChangeScrollHeight (0);
            SetUpZoommer ();
        }

        #region Change viewSize

        internal void ChangeSize ( double widthDifference, double heightDifference )
        {
            ChangeScrollHeight (heightDifference);
            WorkAreaWidth -= widthDifference;
            EntireBlockHeight -= heightDifference;
            ScrollHeight -= heightDifference;
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

            if ( FixedBadges.Contains (BeingProcessedBadge) )
            {
                FixedBadges.Remove (BeingProcessedBadge);
            }

            if ( IncorrectBadges.Contains (BeingProcessedBadge) )
            {
                IncorrectBadges.Remove (BeingProcessedBadge);
            }

            BadgeViewModel draftSource = null;

            foreach ( var keyValue   in   _drafts ) 
            {
                if ( keyValue.Value.Equals (BeingProcessedBadge) ) 
                {
                    draftSource = keyValue.Key;
                    break;
                } 
            }

            BeingProcessedBadge = draftSource.Clone ();
            _drafts [draftSource] = BeingProcessedBadge;
            VisibleBadges [BeingProcessedNumber - 1] = BeingProcessedBadge;
            IncorrectBadges.Add (BeingProcessedBadge);
            SetToCorrectScale (BeingProcessedBadge);
            BeingProcessedBadge.Show ();
            BeingProcessedBadge.CheckCorrectnessAfterCancelation ();
            
            ResetActiveIcon ();
        }


        internal void PassView ( BadgeEditorView view )
        {
            _view = view;
        }


        private bool ChangesExist ( )
        {
            bool exist = false;

            foreach ( BadgeViewModel badge   in   FixedBadges ) 
            {
                if ( badge.IsChanged ) 
                {
                    exist = true;
                    return exist;
                }
            }

            foreach ( BadgeViewModel badge   in   IncorrectBadges )
            {
                if ( badge.IsChanged )
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
            SetDraftResultsInSource (FixedBadges);
            SetDraftResultsInSource (IncorrectBadges);

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


        private void SetDraftResultsInSource ( ObservableCollection <BadgeViewModel> rewritables )
        {
            foreach ( BadgeViewModel badge   in   rewritables )
            {
                BadgeViewModel draftSource = null;

                foreach ( var keyValue   in   _drafts )
                {
                    if ( keyValue.Value.Equals (badge) )
                    {
                        draftSource = keyValue.Key;
                        break;
                    }
                }

                SetToCorrectScale (draftSource);

                if ( badge.IsChanged )
                {
                    badge.ResetPrototype (draftSource);
                }
            }
        }


        private void AddToStorage ( BadgeViewModel addable )
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
