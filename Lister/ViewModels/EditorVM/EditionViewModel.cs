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

        private static readonly string _extentionToolTip = "Развернуть панель";
        private static readonly string _shrinkingToolTip = "Свернуть панель";

        //internal static readonly double _scale = 2.44140625;
        internal static readonly double _startScale = 1.5624;
        internal static double _scale = 1.5624;

        private double _viewWidth = 800;
        private double _viewHeight = 460;

        private double _workAreaWidth = 550;
        private double _workAreaHeight = 380;

        private PageViewModel _firstPage;
        private Dictionary <BadgeViewModel, double> _scaleStorage;

        private TextLineViewModel _splittable;
        private TextLineViewModel _focusedLine;

        private bool _incorrectsAreSet = false;
        private BadgeEditorView _view;
        private int _incorrectsAmmount;
        private int _visibleRangeStart = 0;
        private int _visibleRangeEnd;

        internal double WidthDelta { get; set; }
        internal double HeightDelta { get; set; }

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

        private Thickness mg;
        public Thickness Margin
        {
            get { return mg; }
            private set
            {
                this.RaiseAndSetIfChanged (ref mg, value, nameof (Margin));
            }
        }

        private Dictionary<int, BadgeViewModel> _currentVisibleCollection;
        private Dictionary<int, BadgeViewModel> CurrentVisibleCollection 
        {
            get { return _currentVisibleCollection; }

            set 
            { 
                if ( ( value == null ) || ( value.Count < 1 ) )
                {
                    UpDownIsVisible = false;
                }
                else 
                {
                    UpDownIsVisible = true;
                }

                _currentVisibleCollection = value;
            }
        }

        internal Dictionary <int, BadgeViewModel> AllNumbered { get; private set; }
        internal Dictionary <int, BadgeViewModel> IncorrectNumbered { get; private set; }
        internal Dictionary <int, BadgeViewModel> CorrectNumbered { get; private set; }
        internal Dictionary <int, BadgeViewModel> BackupNumbered { get; private set; }
        internal List <BadgeViewModel> Printable { get; private set; }

        private int incBC;
        internal int IncorrectBadgesCount
        {
            get { return incBC; }
            private set
            {
                this.RaiseAndSetIfChanged (ref incBC, value, nameof (IncorrectBadgesCount));
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

        private string eT;
        internal string ExtentionTip
        {
            get { return eT; }
            private set
            {
                this.RaiseAndSetIfChanged (ref eT, value, nameof (ExtentionTip));
            }
        }


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

            ViewWidth = _viewWidth;
            ViewHeight = _viewHeight;

            ExtenderContent = "\uF060";
            WorkAreaWidth = _workAreaWidth + _namesFilterWidth;
            WorkAreaHeight = _workAreaHeight;
            SetUpSliderBlock ( incorrectBadgesAmmount );

            ChangeScrollHeight (0);
            SetUpZoommer ();

            SplitterIsEnable = false;
            IncorrectNumbered = new ();
            CorrectNumbered = new ();

            FocusedFontSizeColor = _releasedFontSizeColor;
            FocusedFontSizeBorderColor = _releasedFontSizeBorderColor;
        }


        internal void HandleDialogOpenig ( )
        {
            Margin = new Thickness (0, -ViewHeight);
        }


        internal void HandleDialogClosing ()
        {
            Margin = new Thickness (0, 0);
        }


        internal void ChangeSize ( double widthDifference, double heightDifference )
        {
            ChangeScrollHeight (heightDifference);

            ViewWidth -= widthDifference;
            ViewHeight -= heightDifference;
            
            WorkAreaWidth -= widthDifference;
            WorkAreaHeight -= heightDifference;

            EntireBlockHeight -= heightDifference;
            //ScrollHeight -= heightDifference;
        }


        internal void CancelChanges ()
        {
            if ( BeingProcessedBadge == null ) 
            {
                return;
            }

            MoversAreEnable = false;
            SplitterIsEnable = false;
            ZoommerIsEnable = false;
            //_view.zoomOn.IsEnabled = false;
            //_view.zoomOut.IsEnabled = false;

            BeingProcessedBadge.CopyFrom (BackupNumbered [BeingProcessedBadge. Id]);

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

                AllNumbered.Add (badge.Id, badge);
                IncorrectNumbered.Add (badge.Id, badge);
                BackupNumbered.Add (badge.Id, null);

                counter++;
            }

            CurrentVisibleCollection = AllNumbered;
            SetSliderWideness ();

            Dispatcher.UIThread.InvokeAsync
            (
                () =>
                {
                    SetBeingProcessed (AllNumbered.ElementAt(0).Value);
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
                    badge.ZoomOut (badge.Scale);
                    SetOriginalScale (badge, scale);
                }

                if ( badge.IsChanged ) 
                {
                    badge.IsChanged = false;
                }
            }

            ReleaseCaptured ();
            OnBackingToMainView ();
            _scale = _startScale;
            _view.CompleteBacking ();
            _firstPage.Show ();
        }


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


