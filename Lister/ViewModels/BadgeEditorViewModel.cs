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
    public class BadgeEditorViewModel : ViewModelBase
    {
        private static string _correctnessIcon = "GreenCheckMarker.jpg";
        private static string _incorrectnessIcon = "RedCross.png";
        internal static readonly double _scale = 2.44140625;
        //internal static readonly double _scale = 2;
        //internal static readonly double _scale = 2.5;
        private Dictionary <BadgeViewModel, double> _scaleStorage;
        private List <BadgeViewModel> _badgeStorage;
        private TextLineViewModel _splittable;
        private TextLineViewModel _focusedLine;
        private FilterChoosing _filterState = FilterChoosing.AllIsChosen;
        private bool _incorrectsAreSet = false;
        private ModernMainView _back;
        private BadgeEditorView _view;
        private int _visibleRangeStart = 0;
        private int _visibleRangeEnd;
        private int _visibleRange;
        private double _scrollHeight = 204;
        private double _itemHeight = 34;
        private Vector _offset;
        private double _doubleRest;
        private readonly int _loadingBadgeCount = 20;
        private List<int []> _loadedStartEndPairs;
        private int [] _containingPair;
        private bool _containingPairExists;
        
        internal double WidthDelta { get; set; }
        internal double HeightDelta { get; set; }
        //internal bool ScrollChangedByNavigation { get; private set; }

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

                if ( ! _incorrectsAreSet ) 
                {
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

                    _loadedStartEndPairs.Add (new int [2] {0, (_loadingBadgeCount - 1)});
                    _containingPairExists = true;
                    _incorrectsAreSet = true;
                }

                Dispatcher.UIThread.InvokeAsync
                (
                    () =>
                    {
                        SetBeingProcessed (VisibleBadges [0]);
                        SetIcons ();
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

        private Vector sO;
        internal Vector SliderOffset
        {
            get { return sO; }
            private set
            {
                this.RaiseAndSetIfChanged (ref sO, value, nameof (SliderOffset));
            }
        }

        private ObservableCollection <BadgeCorrectnessViewModel> cL;
        internal ObservableCollection <BadgeCorrectnessViewModel> VisibleIcons
        {
            get { return cL; }
            private set
            {
                this.RaiseAndSetIfChanged (ref cL, value, nameof (VisibleIcons));
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

        private BadgeViewModel bpB;
        internal BadgeViewModel BeingProcessedBadge
        {
            get { return bpB; }
            private set
            {
                this.RaiseAndSetIfChanged (ref bpB, value, nameof (BeingProcessedBadge));
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

        private int pC;
        internal int ProcessableCount
        {
            get { return pC; }
            private set
            {
                this.RaiseAndSetIfChanged (ref pC, value, nameof (ProcessableCount));
            }
        }

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

        private bool zE;
        internal bool ZoommerIsEnable
        {
            get { return zE; }
            private set
            {
                this.RaiseAndSetIfChanged (ref zE, value, nameof (ZoommerIsEnable));
            }
        }

        private string fS;
        internal string FocusedFontSize
        {
            get { return fS; }
            private set
            {
                this.RaiseAndSetIfChanged (ref fS, value, nameof (FocusedFontSize));
            }
        }

        private Thickness fBT;
        internal Thickness FocusedBorderThickness
        {
            get { return fBT; }
            private set
            {
                this.RaiseAndSetIfChanged (ref fBT, value, nameof (FocusedBorderThickness));
            }
        }


        public BadgeEditorViewModel ( )
        {
            _scaleStorage = new ();
            _badgeStorage = new ();
            _drafts = new ();
            _loadedStartEndPairs = new ();
            FocusedFontSize = string.Empty;
            SplitterIsEnable = false;
            IncorrectBadges = new ObservableCollection <BadgeViewModel> ();
            FixedBadges = new ObservableCollection <BadgeViewModel> ();
            VisibleIcons = new ObservableCollection <BadgeCorrectnessViewModel> ();

            string correctnessIcon = App.ResourceDirectoryUri + _correctnessIcon;
            string incorrectnessIcon = App.ResourceDirectoryUri + _incorrectnessIcon;

            Uri correctUri = new Uri (correctnessIcon);
            CorrectnessIcon = ImageHelper.LoadFromResource (correctUri);
            Uri incorrectUri = new Uri (incorrectnessIcon);
            IncorrectnessIcon = ImageHelper.LoadFromResource (incorrectUri);

            CorrectnessOpacity = 1;
            IncorrectnessOpacity = 1;

            ChangeScrollHeight (0);

            SliderOffset = new Vector (0, 0);
            _offset = new Vector (0, 0);
        }


        internal void ChangeScrollHeight ( double delta )
        {
            _scrollHeight -= delta;
            _visibleRange = ( int ) ( _scrollHeight / _itemHeight );
            _visibleRangeEnd = _visibleRange - 1;
        }


        internal void ChangeSliderHeight ( double delta )
        {
            double oldHeight = _scrollHeight;
            _scrollHeight -= delta;

            int oldRange = _visibleRange;
            _visibleRange = ( int ) ( _scrollHeight / _itemHeight );

            _visibleRangeEnd += _visibleRange - oldRange;

            SaveProcessableIconVisible ();
        }


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


        internal void PassViews ( BadgeEditorView view, ModernMainView back )
        {
            _back = back;
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

            MainWindow mainWindow = _view.Parent as MainWindow;
            _back.SetProperSize (_view.Width, _view.Height);

            mainWindow.CancelSizeDifference ();
            _back.ResetIncorrects ();
            mainWindow.Content = _back;
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


        #region Navigation

        internal void ToFirst ( )
        {
            BeingProcessedBadge.Hide ();
            FilterProcessableBadge (BeingProcessedNumber);
            BeingProcessedBadge = VisibleBadges [0];
            ActiveIcon. BorderColor = new SolidColorBrush (MainWindow.white);
            ActiveIcon = VisibleIcons [0];
            ActiveIcon. BorderColor = new SolidColorBrush (MainWindow.black);
            BeingProcessedBadge.Show ();
            BeingProcessedNumber = 1;
            SetToCorrectScale (BeingProcessedBadge);
            SetEnableBadgeNavigation ();
            MoversAreEnable = false;
            SplitterIsEnable = false;
            ZoommerIsEnable = false;

            ReleaseCaptured ();

            SliderOffset = new Vector (0,0);

            //Crutch instead SliderOffset
            _view. sliderScroller.ScrollToHome ();
            _visibleRangeEnd = _visibleRange - 1;
        }


        internal void ToPrevious ( )
        {
            if ( BeingProcessedNumber <= 1 ) 
            {
                return;
            }

            BeingProcessedBadge.Hide ();
            FilterProcessableBadge (BeingProcessedNumber);
            BeingProcessedBadge = VisibleBadges [BeingProcessedNumber - 2];
            ActiveIcon. BorderColor = new SolidColorBrush (MainWindow.white);
            ActiveIcon = VisibleIcons [BeingProcessedNumber - 2];
            ActiveIcon. BorderColor = new SolidColorBrush (MainWindow.black);
            BeingProcessedBadge.Show ();
            BeingProcessedNumber--;
            SetToCorrectScale (BeingProcessedBadge);
            SetEnableBadgeNavigation ();
            MoversAreEnable = false;
            SplitterIsEnable = false;
            ZoommerIsEnable = false;

            ReleaseCaptured ();

            //Crutch instead SliderOffset
            //_view. sliderScroller.Offset = new Vector (0, SliderOffset.Y);

            SaveProcessableIconVisible ();

            if ( BeingProcessedNumber < ( _visibleRangeEnd - _visibleRange + 2 ) )
            {
                SliderOffset = new Vector (0, SliderOffset.Y - _itemHeight - _doubleRest);
                _doubleRest = 0;
                _visibleRangeEnd--;
            }

        }


        internal void ToNext ()
        {
            if ( BeingProcessedNumber >= ProcessableCount )
            {
                return;
            }

            BeingProcessedBadge.Hide ();
            int number = BeingProcessedNumber + 1;
            bool filterOccured = FilterProcessableBadge (BeingProcessedNumber);

            if ( filterOccured )
            {
                number--;
            }

            BeingProcessedNumber = number;

            MoversAreEnable = false;
            SplitterIsEnable = false;
            ZoommerIsEnable = false;

            ReleaseCaptured ();

            //Crutch instead SliderOffset
            //_view. sliderScroller.Offset = new Vector (0, SliderOffset.Y);

            //SaveProcessableIconVisible ();

            if ( BeingProcessedNumber > ( _visibleRangeEnd + 1 ) )
            {
                _containingPair = null;

                foreach ( int [] pair   in   _loadedStartEndPairs )
                {
                    if ( ( ( BeingProcessedNumber - 2 ) >= pair [0] )   &&   ( ( BeingProcessedNumber - 2 ) <= pair [1] ) )
                    {
                        _containingPair = pair;
                        break;
                    }
                }

                if ( _containingPair == null ) 
                {
                    _containingPair = new int [2] {(BeingProcessedNumber - 1), ( BeingProcessedNumber - 1 ) };
                    _loadedStartEndPairs.Add ( _containingPair );
                }

                bool goneOutOfPair = ( _containingPair [1] < ( BeingProcessedNumber - 1 ) )
                                     ||
                                     ( _containingPair [0] == _containingPair [1] );

                if ( goneOutOfPair ) 
                {
                    BadgeViewModel badge = _badgeStorage [BeingProcessedNumber - 1];

                    if ( ! _drafts.ContainsKey(badge) ) 
                    {
                        BadgeViewModel clone = badge.Clone ();
                        _drafts.Add (badge, clone);
                        IncorrectBadges.Add (clone);
                        VisibleBadges.Add (clone);

                        BadgeCorrectnessViewModel icon = new BadgeCorrectnessViewModel (false, clone);
                        VisibleIcons.Add (icon);

                        _containingPair [1]++;
                    }
                }

                ScrollViewer scroller = _view. sliderScroller;

                SliderOffset = new Vector (0, scroller.Offset.Y + _itemHeight - _doubleRest);
                _doubleRest = 0;
                _visibleRangeEnd++;
            }

            BeingProcessedBadge = VisibleBadges [number - 1];
            BeingProcessedBadge.Show ();

            SetToCorrectScale (BeingProcessedBadge);
            SetEnableBadgeNavigation ();

            ActiveIcon.BorderColor = new SolidColorBrush (MainWindow.white);
            ActiveIcon = VisibleIcons [number - 1];
            ActiveIcon.BorderColor = new SolidColorBrush (MainWindow.black);
        }


        internal void ToLast ( )
        {
            BeingProcessedBadge.Hide ();



            _containingPair = null;
            int maxLoadedNumber = 0;

            foreach ( int [] pair   in   _loadedStartEndPairs )
            {
                if ( pair [1] > maxLoadedNumber ) 
                {
                    maxLoadedNumber = pair [1];
                }

                if ( ( (_badgeStorage.Count - 1) >= pair [0] )   &&   ( (_badgeStorage.Count - 1) <= pair [1] ) )
                {
                    _containingPair = pair;
                }
            }

            if ( _containingPair == null )
            {
                int diff = Math.Min (_loadingBadgeCount, ( _badgeStorage.Count - maxLoadedNumber - 1 ));
                int newPairStart = _badgeStorage.Count - diff - 1;
                _containingPair = new int [2] { newPairStart, (_badgeStorage.Count - 1) };
                _loadedStartEndPairs.Add (_containingPair);
            }

            bool goneOutOfPair = ( _containingPair [1] < ( BeingProcessedNumber - 1 ) )
                                 ||
                                 ( _containingPair [0] == _containingPair [1] );




            if ( VisibleBadges. Count  <  _badgeStorage.Count )
            {
                int diff = Math.Min (_loadingBadgeCount, (_badgeStorage.Count - maxLoadedNumber - 1));

                for ( int index = (_badgeStorage.Count - diff);   index < _badgeStorage.Count;   index++ ) 
                {
                    BadgeViewModel badge = _badgeStorage [index];

                    if ( !_drafts.ContainsKey (badge) )
                    {
                        BadgeViewModel clone = badge.Clone ();
                        _drafts.Add (badge, clone);
                        IncorrectBadges.Add (clone);
                        VisibleBadges.Add (clone);
                        
                        BadgeCorrectnessViewModel icon = new BadgeCorrectnessViewModel (false, clone);
                        VisibleIcons.Add (icon);
                    }
                }
            }

            int number = VisibleBadges. Count;
            bool filterOccured = FilterProcessableBadge (BeingProcessedNumber);

            if ( filterOccured )
            {
                number--;
            }

            BeingProcessedBadge = VisibleBadges [number - 1];
            ActiveIcon. BorderColor = new SolidColorBrush (MainWindow.white);
            ActiveIcon = VisibleIcons [number - 1];
            ActiveIcon. BorderColor = new SolidColorBrush (MainWindow.black);
            BeingProcessedBadge.Show ();
            BeingProcessedNumber = _badgeStorage.Count;
            SetToCorrectScale (BeingProcessedBadge);
            SetEnableBadgeNavigation ();
            MoversAreEnable = false;
            SplitterIsEnable = false;
            ZoommerIsEnable = false;

            ReleaseCaptured ();

            double verticalOffset = (VisibleBadges. Count - _visibleRange) * _itemHeight;
            SliderOffset = new Vector (0, verticalOffset);

            //Crutch instead SliderOffset
            _view. sliderScroller.ScrollToEnd ();
            _visibleRangeEnd = VisibleBadges. Count - 1;
        }


        internal void ToParticularBadge ( BadgeViewModel goalBadge )
        {
            if ( goalBadge.Equals (BeingProcessedBadge) )
            {
                return;
            }

            string numberAsText = string.Empty;

            for ( int index = 0;   index < VisibleBadges. Count;   index++ )
            {
                if ( goalBadge.Equals (VisibleBadges [index]) ) 
                {
                    numberAsText = (index + 1).ToString ();
                    break;
                }
            }

            ToParticularBadge (numberAsText);
        }


        private void ToParticularBadge ( string numberAsText )
        {
            try
            {
                int number = int.Parse (numberAsText);

                if ( number > VisibleBadges. Count   ||   number < 1 )
                {
                    BeingProcessedNumber = BeingProcessedNumber;
                    return;
                }

                BeingProcessedBadge.Hide ();
                bool filterOccured = FilterProcessableBadge (number);

                if ( filterOccured   &&   (number > BeingProcessedNumber) ) 
                {
                    number--;
                }

                BeingProcessedBadge = VisibleBadges [number - 1];

                if ( ActiveIcon != null ) 
                {
                    ActiveIcon.BorderColor = new SolidColorBrush (MainWindow.white);
                }

                ActiveIcon = VisibleIcons [number - 1];
                ActiveIcon. BorderColor = new SolidColorBrush (MainWindow.black);
                BeingProcessedBadge.Show ();
                BeingProcessedNumber = number;
                SetToCorrectScale (BeingProcessedBadge);
                SetEnableBadgeNavigation ();
                MoversAreEnable = false;
                SplitterIsEnable = false;
                ZoommerIsEnable = false;
                _view. editorTextBox.IsEnabled = false;
                _view. scalabilityGrade.IsEnabled = false;

                ReleaseCaptured ();

                //SaveProcessableIconVisible ();
            }
            catch ( Exception ex )
            {
                BeingProcessedNumber = BeingProcessedNumber;
            }
        }


        private void SaveProcessableIconVisible ( )
        {
            bool numberIsOutOfRange = BeingProcessedNumber > ( _visibleRangeEnd + 1 )
                                      ||
                                      BeingProcessedNumber < ( _visibleRangeEnd + 1 - _visibleRange );

            if ( numberIsOutOfRange )
            {
                ScrollViewer scroller = _view.sliderScroller;

                int offsetedItemsCount = (int) (scroller.Offset.Y / _itemHeight);
                _doubleRest =  (scroller.Offset.Y - (offsetedItemsCount * _itemHeight));

                int diff = _visibleRange - ( BeingProcessedNumber - offsetedItemsCount );
                _visibleRangeEnd = BeingProcessedNumber + diff - 1;
            }
        }


        private void SaveProcessableIconVisibleCh ()
        {
            bool numberIsOutOfRange = BeingProcessedNumber > ( _visibleRangeEnd + 1 )
                                      ||
                                      BeingProcessedNumber < ( _visibleRangeEnd + 1 - _visibleRange );

            if ( numberIsOutOfRange )
            {
                _visibleRangeEnd = BeingProcessedNumber + _visibleRange / 2;
                SliderOffset = new Vector (0, _itemHeight * ( BeingProcessedNumber - _visibleRange / 2 ));
            }
        }

        #endregion

        private void AddToStorage ( BadgeViewModel addable )
        {
            if ( ! _scaleStorage.ContainsKey (addable) )
            {
                _scaleStorage.Add (addable, addable. Scale);
            }
        }


        #region Moving

        internal void MoveCaptured ( Point delta )
        {
            BeingProcessedBadge.MoveCaptured ( delta);
        }


        internal void ToSide ( string direction )
        {
            BeingProcessedBadge.ToSide (direction, _scale);
            ResetActiveIcon ();
        }


        internal void Left ( )
        {
            BeingProcessedBadge.Left (_scale);
            ResetActiveIcon ();
        }


        internal void Right ( )
        {
            BeingProcessedBadge.Right (_scale);
            ResetActiveIcon ();
        }


        internal void Up ( )
        {
            BeingProcessedBadge.Up (_scale);
            ResetActiveIcon ();
        }


        internal void Down ( )
        {
            BeingProcessedBadge.Down (_scale);
            ResetActiveIcon ();
        }
        #endregion

        internal void Focus ( string focusedContent, int elementNumber )
        {
            BeingProcessedBadge.SetFocusedLine ( focusedContent, elementNumber );

            if ( BeingProcessedBadge. FocusedLine != null )
            {
                MoversAreEnable = true;
                ZoommerIsEnable = true;

                EnableSplitting (focusedContent, elementNumber);
            }
        }


        internal void ReleaseCaptured ( )
        {
            if ( BeingProcessedBadge. FocusedLine != null )
            {
                BeingProcessedBadge.CheckFocusedLineCorrectness ();
                BeingProcessedBadge. FocusedLine = null;
                BeingProcessedBadge. FocusedFontSize = string.Empty;
                ResetActiveIcon ();
                ZoommerIsEnable = false;
                MoversAreEnable = false;
                SplitterIsEnable = false;
            }
        }


        private void ResetActiveIcon ()
        {
            if ( BeingProcessedBadge. IsCorrect )
            {
                VisibleIcons [BeingProcessedNumber - 1] = new BadgeCorrectnessViewModel (true, BeingProcessedBadge);

                if ( ! FixedBadges.Contains(BeingProcessedBadge) ) 
                {
                    FixedBadges.Add (BeingProcessedBadge);
                }
                
                if ( IncorrectBadges.Contains (BeingProcessedBadge) )
                {
                    IncorrectBadges.Remove (BeingProcessedBadge);
                }

            }
            else
            {
                if ( VisibleIcons [BeingProcessedNumber - 1].Correctness )
                {
                    VisibleIcons [BeingProcessedNumber - 1] = new BadgeCorrectnessViewModel (false, BeingProcessedBadge);
                }

                if ( ! IncorrectBadges.Contains(BeingProcessedBadge) ) 
                {
                    IncorrectBadges.Add (BeingProcessedBadge);
                }

                if ( FixedBadges.Contains (BeingProcessedBadge) )
                {
                    FixedBadges.Remove (BeingProcessedBadge);
                }
            }

            ActiveIcon = VisibleIcons [BeingProcessedNumber - 1];
            ActiveIcon. BorderColor = new SolidColorBrush (MainWindow.black);
        }


        internal void EnableSplitting ( string content, int elementNumber )
        {
            TextLineViewModel line = BeingProcessedBadge.GetCoincidence (content, elementNumber);

            if ( line == null )
            {
                return;
            }

            List<string> strings = content.SplitBySeparators ( new List<char> () { ' ', '-' } );
            bool lineIsSplitable = ( strings.Count > 1 );
            EnableSplitting (lineIsSplitable, line);
        }


        private void EnableSplitting ( bool lineIsSplitable, TextLineViewModel splittable )
        {
            if ( lineIsSplitable )
            {
                SplitterIsEnable = true;
            }
            else
            {
                SplitterIsEnable = false;
            }
        }


        internal void Split ( )
        {
            BeingProcessedBadge.Split (_scale);
            ResetActiveIcon ();
            SplitterIsEnable = false;
        }

        #region FontSizeChange

        internal void IncreaseFontSize ( )
        {
            BeingProcessedBadge.IncreaseFontSize (_scale);
            ResetActiveIcon ();
        }


        internal void ReduceFontSize ( )
        {
            BeingProcessedBadge.ReduceFontSize (_scale);
            ResetActiveIcon ();
        }
        #endregion

        #region Scale

        private void SetStandardScale ( BadgeViewModel beingPrecessed ) 
        {
            if ( beingPrecessed.Scale != 1 )
            {
                beingPrecessed.ZoomOut (beingPrecessed.Scale, true);
            }
        }


        private void SetOriginalScale ( BadgeViewModel beingPrecessed, double scale )
        {
            if ( scale != 1 )
            {
                beingPrecessed.ZoomOn (scale, false);
            }
        }


        private void SetToCorrectScale ( BadgeViewModel processable )
        {
            if ( processable.Scale != _scale )
            {
                SetStandardScale (processable);
                processable.ZoomOn (_scale, false);
            }
        }
        #endregion

        private void SetEnableBadgeNavigation ()
        {
            int badgeCount = _badgeStorage.Count;

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
            else if ( ( BeingProcessedNumber == 1 )   &&   ( badgeCount > 1 ) )
            {
                FirstIsEnable = false;
                PreviousIsEnable = false;
                NextIsEnable = true;
                LastIsEnable = true;
            }
        }


        private void SetBeingProcessed ( BadgeViewModel beingProcessed )
        {
            beingProcessed.Show ();
            BeingProcessedBadge = beingProcessed;
            SetToCorrectScale (BeingProcessedBadge);
            BeingProcessedNumber = 1;
        }


        private void SetIcons (  )
        {
            if( (VisibleBadges != null)   &&   (VisibleBadges. Count > 0) ) 
            {
                for ( int index = 0;   index < VisibleBadges. Count;   index++ ) 
                {
                    BadgeCorrectnessViewModel icon = new BadgeCorrectnessViewModel (false, VisibleBadges [index]);
                    VisibleIcons.Add (icon);
                    icon.BorderColor = new SolidColorBrush (MainWindow.white);
                }

                ActiveIcon = VisibleIcons [0];
                ActiveIcon. BorderColor = new SolidColorBrush (MainWindow.black);
            }
        }


        internal bool FilterProcessableBadge ( int filterableNumber )
        {
            bool filterOccured = false;

            if ( _filterState == FilterChoosing.AllIsChosen )
            {
                return false;
            }
            else if ( _filterState == FilterChoosing.CorrectsIsChosen )
            {
                if ( ! BeingProcessedBadge. IsCorrect ) 
                {
                    filterOccured = true;
                }
            }
            else if ( _filterState == FilterChoosing.IncorrectsIsChosen )
            {
                if ( BeingProcessedBadge. IsCorrect )
                {
                    filterOccured = true;
                }
            }

            if ( filterOccured ) 
            {
                VisibleBadges.Remove (BeingProcessedBadge);
                VisibleIcons.Remove (VisibleIcons [BeingProcessedNumber - 1]);
                ProcessableCount = VisibleBadges. Count;
                filterOccured = true;
            }
           
            return filterOccured;
        }


        internal void FilterCorrects ( )
        {
            if (_filterState == FilterChoosing.AllIsChosen) 
            {
                _filterState = FilterChoosing.CorrectsIsChosen;
            }
            else if ( _filterState == FilterChoosing.CorrectsIsChosen )
            {
                _filterState = FilterChoosing.AllIsChosen;
            }
            else if ( _filterState == FilterChoosing.IncorrectsIsChosen )
            {
                _filterState = FilterChoosing.CorrectsIsChosen;
            }

            SwitchFilter ();
            SetEnableBadgeNavigation ();
        }


        internal void FilterIncorrects ()
        {
            if ( _filterState == FilterChoosing.AllIsChosen )
            {
                _filterState = FilterChoosing.IncorrectsIsChosen;
            }
            else if ( _filterState == FilterChoosing.CorrectsIsChosen )
            {
                _filterState = FilterChoosing.IncorrectsIsChosen;
            }
            else if ( _filterState == FilterChoosing.IncorrectsIsChosen )
            {
                _filterState = FilterChoosing.AllIsChosen;
            }

            SwitchFilter ();
            SetEnableBadgeNavigation();
        }


        private void SwitchFilter ( )
        {
            VisibleIcons.Clear ();
            VisibleBadges.Clear ();

            if ( _filterState == FilterChoosing.AllIsChosen ) 
            {
                VisibleBadges.AddRange (FixedBadges);
                VisibleBadges.AddRange (IncorrectBadges);
                CorrectnessOpacity = 1;
                IncorrectnessOpacity = 1;
            }
            else if ( _filterState == FilterChoosing.CorrectsIsChosen ) 
            {
                VisibleBadges.AddRange (FixedBadges);
                CorrectnessOpacity = 1;
                IncorrectnessOpacity = 0.3;
            }
            else if ( _filterState == FilterChoosing.IncorrectsIsChosen ) 
            {
                VisibleBadges.AddRange (IncorrectBadges);
                CorrectnessOpacity = 0.3;
                IncorrectnessOpacity = 1;
            }

            ProcessableCount = VisibleBadges. Count;

            if ( ProcessableCount == 0 )
            {
                BeingProcessedNumber = 0;
            }
            else 
            {
                BeingProcessedNumber = 1;
            }

            if ( ( VisibleBadges != null )   &&   ( VisibleBadges. Count > 0 ) )
            {
                for ( int index = 0;   index < VisibleBadges. Count;   index++ )
                {
                    BadgeViewModel badge = VisibleBadges [index];
                    BadgeCorrectnessViewModel icon = new BadgeCorrectnessViewModel (badge.IsCorrect, VisibleBadges [index]);
                    VisibleIcons.Add (icon);
                    icon.BorderColor = new SolidColorBrush (MainWindow.white);
                }

                ActiveIcon = VisibleIcons [0];
                ActiveIcon. BorderColor = new SolidColorBrush (MainWindow.black);
                BeingProcessedBadge = VisibleBadges [0];
                SetToCorrectScale (BeingProcessedBadge);
                BeingProcessedBadge.Show ();
            }
        }


        internal void ResetFocusedText ( string newText )
        {
            BeingProcessedBadge.ResetFocusedText ( newText );
            ResetActiveIcon ( );
        }



        private enum FilterChoosing 
        {
           AllIsChosen = 0,
           CorrectsIsChosen = 1,
           IncorrectsIsChosen = 2
        }

    }

}
