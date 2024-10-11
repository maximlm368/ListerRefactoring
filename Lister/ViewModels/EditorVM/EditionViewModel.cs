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
        internal static readonly double _startScale = 1.5624;
        internal static double _scale = 1.5624;
        //internal static readonly double _scale = 2.5;

        private double _viewWidth = 800;
        private double _viewHeight = 460;

        private double _workAreaWidth = 550;
        private double _workAreaHeight = 380;



        private PageViewModel _firstPage;
        private Dictionary <BadgeViewModel, double> _scaleStorage;
        private List <BadgeViewModel> _allReadonlyBadges;

        //private List <BadgeViewModel> _incorrectBadges;
        //private List <BadgeViewModel> _correctBadges;

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

        private double wAH;
        internal double WorkAreaHeight
        {
            get { return wAH; }
            set
            {
                this.RaiseAndSetIfChanged (ref wAH, value, nameof (WorkAreaHeight));
            }
        }

        private double vW;
        internal double ViewWidth
        {
            get { return vW; }
            set
            {
                this.RaiseAndSetIfChanged (ref vW, value, nameof (ViewWidth));
            }
        }

        private double vH;
        internal double ViewHeight
        {
            get { return vH; }
            set
            {
                this.RaiseAndSetIfChanged (ref vH, value, nameof (ViewHeight));
            }
        }

        private Dictionary<int, BadgeViewModel> _currentVisibleCollection;
        internal Dictionary <int, BadgeViewModel> AllNumbered { get; private set; }
        internal Dictionary<int, BadgeViewModel> IncorrectNumbered { get; private set; }
        internal Dictionary<int, BadgeViewModel> CorrectNumbered { get; private set; }
        internal Dictionary<int, BadgeViewModel> BackupNumbered { get; private set; }
        internal List <BadgeViewModel> Printable { get; private set; }

        internal BadgeViewModel FirstIncorrect { get; private set; }

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

        private string eC;
        internal string ExtenderContent
        {
            get { return eC; }
            private set
            {
                this.RaiseAndSetIfChanged (ref eC, value, nameof (ExtenderContent));
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

            ViewWidth = _viewWidth;
            ViewHeight = _viewHeight;

            ExtenderContent = "\uF053";
            WorkAreaWidth = _workAreaWidth + _namesFilterWidth;
            WorkAreaHeight = _workAreaHeight;
            SetUpSliderBlock ( incorrectBadgesAmmount );

            ChangeScrollHeight (0);
            SetUpZoommer ();

            SplitterIsEnable = false;
            IncorrectNumbered = new ();
            CorrectNumbered = new ();
        }


        #region Change viewSize

        internal void ChangeSize ( double widthDifference, double heightDifference )
        {
            ChangeScrollHeight (heightDifference);
            //WorkAreaWidth -= widthDifference;
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

            AllNumbered [BeingProcessedBadge.Id].CopyFrom (BackupNumbered [BeingProcessedBadge.Id]);
            BeingProcessedBadge = AllNumbered [BeingProcessedBadge. Id];

            if ( CorrectNumbered.ContainsKey (BeingProcessedBadge.Id) )
            {
                CorrectNumbered [BeingProcessedBadge.Id] = BeingProcessedBadge;
            }
            else if ( IncorrectNumbered.ContainsKey (BeingProcessedBadge.Id) )
            {
                IncorrectNumbered [BeingProcessedBadge.Id] = BeingProcessedBadge;
            }

            SetToCorrectScale (BeingProcessedBadge);
            BeingProcessedBadge.Show ();
            ResetActiveIcon ();
        }


        internal void PassIncorrects ( List <BadgeViewModel> incorrects
                                     , List <BadgeViewModel> allPrintable, PageViewModel firstPage )
        {
            bool isNullOrEmpty = ( incorrects == null )   ||   ( incorrects.Count == 0 );

            if ( isNullOrEmpty )
            {
                return;
            }

            _firstPage = firstPage;
            AllNumbered = new ();
            IncorrectNumbered = new ();
            CorrectNumbered = new ();
            BackupNumbered = new ();
            Printable = allPrintable;
            _scaleStorage = new ();

            _incorrectsAmmount = incorrects.Count;

            int counter = 0;

            foreach ( BadgeViewModel badge   in   incorrects )
            {
                _scaleStorage.Add (badge, badge.Scale);
                _allReadonlyBadges.Add (badge);

                AllNumbered.Add (badge.Id, badge);
                IncorrectNumbered.Add (badge.Id, badge);
                BackupNumbered.Add (badge.Id, null);

                counter++;
            }

            _currentVisibleCollection = AllNumbered;

            Dispatcher.UIThread.InvokeAsync
            (
                () =>
                {
                    SetBeingProcessed (AllNumbered [0]);
                    EnableNavigation ();
                    SetVisibleIcons ();
                    ScrollOffset = 0;
                    ProcessableCount = _scaleStorage.Count;
                    IncorrectBadgesCount = _scaleStorage.Count;
                }
            );
        }


        internal void PassView ( BadgeEditorView view )
        {
            _view = view;
        }


        private bool ChangesExist ( )
        {
            bool exist = false;

            //foreach ( KeyValuePair <int, int> fixedBadgeNumber   in   FixedBadges ) 
            //{
            //    if ( AllBadges [fixedBadgeNumber.Value].IsChanged ) 
            //    {
            //        exist = true;
            //        return exist;
            //    }
            //}

            foreach ( KeyValuePair <int, BadgeViewModel> badgeNumber   in   AllNumbered )
            {
                if ( badgeNumber.Value. IsChanged )
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
            foreach ( KeyValuePair <BadgeViewModel, double> badgeToScale   in   _scaleStorage )
            {
                BadgeViewModel badge = badgeToScale.Key;
                double scale = badgeToScale.Value;

                if ( scale != badge.Scale ) 
                {
                    badge.ZoomOut (badge.Scale, true);
                    SetOriginalScale (badge, scale);
                }
            }

            OnBackingToMainView ();
            _scale = _startScale;
            _view.ComplateBacking ();
            _firstPage.Show ();
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


        //private void SetDraftResultsInSource ( )
        //{
        //    foreach ( KeyValuePair <int, BadgeViewModel> badge   in   AllNumbered )
        //    {
        //        BadgeViewModel draftSource = null;

        //        //foreach ( KeyValuePair <BadgeViewModel, BadgeViewModel> sourceToDraft   in   _sourceToDrafts )
        //        //{
        //        //    if ( sourceToDraft.Value.Equals (badge.Value) )
        //        //    {
        //        //        draftSource = sourceToDraft.Key;
        //        //        break;
        //        //    }
        //        //}

        //        //SetToCorrectScale (draftSource);

        //        double sourceScale = draftSource.Scale;
        //        SetStandardScale (draftSource);
        //        SetStandardScale (badge.Value);

        //        if ( badge.Value.IsChanged )
        //        {
        //            badge.Value.ResetPrototype (draftSource);
        //        }

        //        SetOriginalScale (draftSource, sourceScale);
        //    }
        //}


        private void AddToScalesStorage ( BadgeViewModel addable )
        {
            if ( ! _scaleStorage.ContainsKey (addable) )
            {
                _scaleStorage.Add (addable, addable.Scale);
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


//if ( counter < 1 )
//{
//    Dispatcher.UIThread.InvokeAsync
//    (
//        () =>
//        {
//            BadgeViewModel clone = badge.Clone ();
//            _sourceToDrafts.Add (badge, clone);
//            AllNumberedDrafts.Add (0, clone);
//            // FirstIncorrect = clone;
//        }
//    );
//}