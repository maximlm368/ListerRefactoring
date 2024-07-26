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

namespace Lister.ViewModels
{
    public class BadgeEditorViewModel : ViewModelBase
    {
        private static string _correctnessIcon = "GreenCheckMarker.jpg";
        private static string _incorrectnessIcon = "RedCross.png";
        internal static readonly double _scale = 2.45;
        private Dictionary <BadgeViewModel, double> _scaleStorage;
        private TextLineViewModel _splittable;
        private TextLineViewModel _focusedLine;
        private FilterChoosing _filterState = FilterChoosing.AllIsChosen;
        private bool _incorrectsAreSet = false;
        private ModernMainView _back;
        private BadgeEditorView _view;
        
        internal double WidthDelta { get; set; }
        internal double HeightDelta { get; set; }

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
                    foreach ( BadgeViewModel badge   in   value )
                    {
                        if ( badge == null )
                        {
                            continue;
                        }

                        AddToStorage ( badge );
                        BadgeViewModel clone = badge.Clone ();
                        _drafts.Add (badge, clone);
                        IncorrectBadges.Add (clone);
                        VisibleBadges.Add (clone);
                    }

                    _incorrectsAreSet = true;
                }

                SetBeingProcessed (VisibleBadges [0]);
                SetIcons ();
                ProcessableCount = VisibleBadges. Count;
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

        private ObservableCollection <BadgeViewModel> fx;
        internal ObservableCollection <BadgeViewModel> FixedBadges
        {
            get { return fx; }
            private set
            {
                this.RaiseAndSetIfChanged (ref fx, value, nameof (FixedBadges));
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

        //public ICommand GoBackCommand { get; }
        //internal Interaction <DialogViewModel, string> ShowDialog { get; }


        public BadgeEditorViewModel ( )
        {
            _scaleStorage = new Dictionary <BadgeViewModel, double> ();
            _drafts = new Dictionary <BadgeViewModel, BadgeViewModel> ();
            FocusedFontSize = string.Empty;
            SplitterIsEnable = false;

            IncorrectBadges = new ObservableCollection <BadgeViewModel> ();
            FixedBadges = new ObservableCollection <BadgeViewModel> ();
            VisibleIcons = new ObservableCollection <BadgeCorrectnessViewModel> ();

            //string workDirectory = @"./";
            //DirectoryInfo containingDirectory = new DirectoryInfo (workDirectory);
            //string directoryPath = containingDirectory.FullName;
            //string correctnessIcon = directoryPath + _correctnessIcon;
            //string incorrectnessIcon = directoryPath + _incorrectnessIcon;

            string directoryPath = System.IO.Directory.GetCurrentDirectory ();
            string correctnessIcon = "file:///" + directoryPath + App._resourceUriFolderName + _correctnessIcon;
            string incorrectnessIcon = "file:///" + directoryPath + App._resourceUriFolderName + _incorrectnessIcon;




            Uri correctUri = new Uri (correctnessIcon);
            CorrectnessIcon = ImageHelper.LoadFromResource (correctUri);
            Uri incorrectUri = new Uri (incorrectnessIcon);
            IncorrectnessIcon = ImageHelper.LoadFromResource (incorrectUri);

            CorrectnessOpacity = 1;
            IncorrectnessOpacity = 1;

            //ShowDialog = new Interaction <DialogViewModel, string> ();
            //GoBackCommand = ReactiveCommand.CreateFromTask (async () =>
            //{
            //    bool changesExist = ChangesExist ();

            //    if ( changesExist ) 
            //    {
            //        DialogViewModel dialogVM = new DialogViewModel ();
            //        var result = await ShowDialog.Handle (dialogVM);
                    
            //        if ( result != null )
            //        {
            //            if ( result == "Yes" )
            //            {
            //                GoBack ();
            //            }
            //        }
            //    }
            //    else 
            //    {
            //        GoBack ();
            //    }
            //});
        }


        internal void CancelChanges ()
        {
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
                badge.ZoomOut (_scale);
                SetOriginalScale (badge, scale);
            }

            MainWindow mainWindow = _view.Parent as MainWindow;
            _back.SetProperSize (_view.Width, _view.Height);

            //_back = new ModernMainView ();
            //_back.ChangeSize (mainWindow.WidthDifference, mainWindow.HeightDifference);

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

            BeingProcessedBadge = VisibleBadges [number - 1];
            ActiveIcon. BorderColor = new SolidColorBrush (MainWindow.white);
            ActiveIcon = VisibleIcons [number - 1];
            ActiveIcon. BorderColor = new SolidColorBrush (MainWindow.black);
            BeingProcessedBadge.Show ();
            BeingProcessedNumber = number;
            SetToCorrectScale (BeingProcessedBadge);
            SetEnableBadgeNavigation ();
            MoversAreEnable = false;
            SplitterIsEnable = false;
            ZoommerIsEnable = false;

            ReleaseCaptured ();
        }


        internal void ToLast ( )
        {
            BeingProcessedBadge.Hide ();
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
            BeingProcessedNumber = VisibleBadges. Count;
            SetToCorrectScale (BeingProcessedBadge);
            SetEnableBadgeNavigation ();
            MoversAreEnable = false;
            SplitterIsEnable = false;
            ZoommerIsEnable = false;

            ReleaseCaptured ();
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


        internal void ToParticularBadge ( string numberAsText )
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
            }
            catch ( Exception ex )
            {
                BeingProcessedNumber = BeingProcessedNumber;
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

        internal void MoveCaptured ( string capturedContent, Point delta )
        {
            BeingProcessedBadge.MoveCaptured (capturedContent, delta);
        }


        internal void ToSide ( string focusedContent, string direction )
        {
            BeingProcessedBadge.ToSide (focusedContent, direction, _scale);
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
                BeingProcessedBadge.CheckCorrectness ();
                BeingProcessedBadge. FocusedLine = null;
                BeingProcessedBadge. FocusedFontSize = string.Empty;
                ResetActiveIcon ();
                ZoommerIsEnable = false;
                MoversAreEnable = false;
                SplitterIsEnable = false;
                FocusedBorderThickness = new Thickness (0, 0, 0, 0);
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
            if ( beingPrecessed.Scale > 1 )
            {
                beingPrecessed.ZoomOut (beingPrecessed.Scale);
            }
            if ( beingPrecessed.Scale < 1 )
            {
                beingPrecessed.ZoomOut (beingPrecessed.Scale);
            }
        }


        private void SetOriginalScale ( BadgeViewModel beingPrecessed, double scale )
        {
            if ( scale > 1 )
            {
                beingPrecessed.ZoomOn (scale);
            }

            if ( scale < 1 )
            {
                beingPrecessed.ZoomOn (scale);
            }
        }


        private void SetToCorrectScale ( BadgeViewModel processable )
        {
            if ( processable.Scale != _scale )
            {
                SetStandardScale (processable);
                processable.ZoomOn (_scale);
            }
        }
        #endregion

        private void SetEnableBadgeNavigation ()
        {
            int badgeCount = VisibleBadges.Count;

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
            SetToCorrectScale ( BeingProcessedBadge );
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
