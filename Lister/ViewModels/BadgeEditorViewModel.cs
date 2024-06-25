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

namespace Lister.ViewModels
{
    public class BadgeEditorViewModel : ViewModelBase
    {
        private static string _correctnessIcon = "GreenCheckMarker.jpg";
        private static string _incorrectnessIcon = "RedCross.png";
        private double _scale = 2.8;
        private Dictionary<BadgeViewModel, double> _scaleStorage;
        private TextLineViewModel _splittable;
        private TextLineViewModel _focusedLine;
        private FilterChoosing _filterState = FilterChoosing.AllIsChosen;
        private bool _incorrectsAreSet = false;

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

        private List <BadgeViewModel> _visibleBadges;
        internal List <BadgeViewModel> VisibleBadges
        {
            get { return _visibleBadges; }
            set
            {
                bool isNullOrEmpty = ( value == null )   ||   ( value.Count == 0 );

                if ( isNullOrEmpty )
                {
                    return;
                }

                if ( value [0] == null )
                {
                    return;
                }

                _visibleBadges = value;
                SetBeingProcessed (value);
                SetIcons ();

                if ( ! _incorrectsAreSet ) 
                {
                    foreach ( BadgeViewModel badge   in   VisibleBadges )
                    {
                        IncorrectBadges.Add (badge);
                    }

                    _incorrectsAreSet = true;
                }

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

        //private int cC;
        //internal int CorrectCount
        //{
        //    get { return cC; }
        //    set
        //    {
        //        this.RaiseAndSetIfChanged (ref cC, value, nameof (CorrectCount));
        //    }
        //}

        //private int inC;
        //internal int IncorrectCount
        //{
        //    get { return inC; }
        //    set
        //    {
        //        this.RaiseAndSetIfChanged (ref inC, value, nameof (IncorrectCount));
        //    }
        //}

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
            _scaleStorage = new Dictionary <BadgeViewModel, double> ();
            FocusedFontSize = string.Empty;
            SplitterIsEnable = false;

            IncorrectBadges = new ObservableCollection <BadgeViewModel> ();
            FixedBadges = new ObservableCollection <BadgeViewModel> ();
            VisibleIcons = new ObservableCollection <BadgeCorrectnessViewModel> ();

            string workDirectory = @"./";
            DirectoryInfo containingDirectory = new DirectoryInfo (workDirectory);
            string directoryPath = containingDirectory.FullName;
            string correctnessIcon = directoryPath + _correctnessIcon;
            string incorrectnessIcon = directoryPath + _incorrectnessIcon;

            Uri uri = new Uri (correctnessIcon);
            CorrectnessIcon = ImageHelper.LoadFromResource (uri);
            uri = new Uri (incorrectnessIcon);
            IncorrectnessIcon = ImageHelper.LoadFromResource (uri);

            CorrectnessOpacity = 1;
            IncorrectnessOpacity = 1;
    }

        #region Navigation

        internal void ToFirst ( )
        {
            BeingProcessedBadge.Hide ();
            BeingProcessedBadge = VisibleBadges [0];
            ActiveIcon. BorderColor = new SolidColorBrush (new Color (255, 255, 255, 255));
            ActiveIcon = VisibleIcons [0];
            ActiveIcon. BorderColor = new SolidColorBrush (new Color (255, 0, 0, 0));
            AddToStorage ();
            BeingProcessedBadge.Show ();
            BeingProcessedNumber = 1;
            SetCorrectScale ( );
            SetEnableBadgeNavigation ();
            MoversAreEnable = false;
            SplitterIsEnable = false;
        }


        internal void ToPrevious ( )
        {
            BeingProcessedBadge.Hide ();
            BeingProcessedBadge = VisibleBadges [BeingProcessedNumber - 2];
            ActiveIcon. BorderColor = new SolidColorBrush (new Color (255, 255, 255, 255));
            ActiveIcon = VisibleIcons [BeingProcessedNumber - 2];
            ActiveIcon. BorderColor = new SolidColorBrush (new Color (255, 0, 0, 0));
            AddToStorage ();
            BeingProcessedBadge.Show ();
            BeingProcessedNumber--;
            SetCorrectScale( );
            SetEnableBadgeNavigation ();
            MoversAreEnable = false;
            SplitterIsEnable = false;
        }


        internal void ToNext ()
        {
            BeingProcessedBadge.Hide ();
            BeingProcessedBadge = VisibleBadges [BeingProcessedNumber];
            ActiveIcon. BorderColor = new SolidColorBrush (new Color (255, 255, 255, 255));
            ActiveIcon = VisibleIcons [BeingProcessedNumber];
            ActiveIcon. BorderColor = new SolidColorBrush (new Color (255, 0, 0, 0));
            AddToStorage ();
            BeingProcessedBadge.Show ();
            BeingProcessedNumber++;
            SetCorrectScale ( );
            SetEnableBadgeNavigation ();
            MoversAreEnable = false;
            SplitterIsEnable = false;
        }


        internal void ToLast ( )
        {
            BeingProcessedBadge.Hide ();
            BeingProcessedBadge = VisibleBadges [VisibleBadges. Count - 1];
            ActiveIcon. BorderColor = new SolidColorBrush (new Color (255, 255, 255, 255));
            ActiveIcon = VisibleIcons [VisibleIcons. Count - 1];
            ActiveIcon. BorderColor = new SolidColorBrush (new Color (255, 0, 0, 0));
            AddToStorage ();
            BeingProcessedBadge.Show ();
            BeingProcessedNumber = VisibleBadges. Count;
            SetCorrectScale ( );
            SetEnableBadgeNavigation ();
            MoversAreEnable = false;
            SplitterIsEnable = false;
        }


        internal void ToParticularBadge ( string numberAsText )
        {
            try
            {
                int number = int.Parse (numberAsText);

                if ( number > VisibleBadges.Count   ||   number < 1 )
                {
                    BeingProcessedNumber = BeingProcessedNumber;
                    return;
                }

                BeingProcessedBadge.Hide ();
                BeingProcessedBadge = VisibleBadges [number - 1];

                if ( ActiveIcon != null ) 
                {
                    ActiveIcon.BorderColor = new SolidColorBrush (new Color (255, 255, 255, 255));
                }

                ActiveIcon = VisibleIcons [number - 1];
                ActiveIcon. BorderColor = new SolidColorBrush (new Color (255, 0, 0, 0));
                AddToStorage ();
                BeingProcessedBadge.Show ();
                BeingProcessedNumber = number;
                SetCorrectScale ();
                SetEnableBadgeNavigation ();
                MoversAreEnable = false;
                SplitterIsEnable = false;
            }
            catch ( Exception ex )
            {
                BeingProcessedNumber = BeingProcessedNumber;
                return;
            }
        }

        #endregion

        private void AddToStorage ( )
        {
            if ( ! _scaleStorage.ContainsKey (BeingProcessedBadge) )
            {
                _scaleStorage.Add (BeingProcessedBadge, BeingProcessedBadge. Scale);
            }
        }


        private void SetCorrectScale () 
        {
            if ( BeingProcessedBadge.Scale != _scale )
            {
                SetStandartScale (BeingProcessedBadge);
                BeingProcessedBadge.ZoomOn (_scale);
            }
        }

        #region Moving

        internal void MoveCaptured ( string capturedContent, Point delta )
        {
            ObservableCollection <TextLineViewModel> lines = BeingProcessedBadge. TextLines;
            string lineContent = string.Empty;
            TextLineViewModel goalLine = null;

            foreach ( TextLineViewModel line   in   lines ) 
            {
                lineContent = line.Content;

                if ( lineContent == capturedContent ) 
                {
                    goalLine = line;
                    break;
                }
            }

            if ( goalLine != null ) 
            {
                goalLine.TopOffset -= delta.Y;
                goalLine.LeftOffset -= delta.X;
            } 
        }


        internal void ToSide ( string focusedContent, string direction )
        {
            ObservableCollection<TextLineViewModel> lines = BeingProcessedBadge. TextLines;
            string lineContent = string.Empty;
            TextLineViewModel goalLine = null;

            foreach ( TextLineViewModel line in lines )
            {
                lineContent = line.Content;

                if ( lineContent == focusedContent )
                {
                    goalLine = line;
                    break;
                }
            }

            if ( goalLine != null )
            {
                if ( direction == "Left" ) 
                {
                    goalLine.LeftOffset -= _scale;
                }

                if ( direction == "Right" )
                {
                    goalLine.LeftOffset += _scale;
                }

                if ( direction == "Up" )
                {
                    goalLine.TopOffset -= _scale;
                }

                if ( direction == "Down" )
                {
                    goalLine.TopOffset += _scale;
                }

                BeingProcessedBadge.CheckCorrectness ();
                ResetActiveIcon ();
            }
        }


        internal void Left ( )
        {
            if ( _focusedLine != null )
            {
                _focusedLine.LeftOffset -= _scale;
            }
        }


        internal void Right ( )
        {
            if ( _focusedLine != null )
            {
                _focusedLine.LeftOffset += _scale;
            }
        }


        internal void Up ( )
        {
            if ( _focusedLine != null )
            {
                _focusedLine.TopOffset -= _scale;
            }
        }


        internal void Down ( )
        {
            if ( _focusedLine != null )
            {
                _focusedLine.TopOffset += _scale;
            }
        }
        #endregion

        internal void Focus ( string focusedContent )
        {
            ObservableCollection<TextLineViewModel> lines = BeingProcessedBadge. TextLines;
            string lineContent = string.Empty;
            _focusedLine = GetCoincidence (focusedContent);

            if ( _focusedLine != null )
            {
                FocusedFontSize = _focusedLine.FontSize.ToString ();
                MoversAreEnable = true;
                BeingProcessedBadge. FocusedLine = _focusedLine;
            }
        }


        internal void ReleaseCaptured ( )
        {
            if ( _focusedLine != null )
            {
                BeingProcessedBadge.CheckCorrectness ();
                ResetActiveIcon ();
                _focusedLine = null;
                SplitterIsEnable = false;
            }
        }


        private void ResetActiveIcon ()
        {
            if ( BeingProcessedBadge. IsCorrect )
            {
                VisibleIcons [BeingProcessedNumber - 1] = new BadgeCorrectnessViewModel (true);
                VisibleIcons [BeingProcessedNumber - 1].BorderColor = new SolidColorBrush (new Color (255, 0, 0, 0));

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
                    VisibleIcons [BeingProcessedNumber - 1] = new BadgeCorrectnessViewModel (false);
                    VisibleIcons [BeingProcessedNumber - 1].BorderColor = new SolidColorBrush (new Color (255, 0, 0, 0));
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
        }


        private TextLineViewModel ? GetCoincidence ( string focusedContent )
        {
            ObservableCollection<TextLineViewModel> lines = BeingProcessedBadge. TextLines;
            string lineContent = string.Empty;
            TextLineViewModel goalLine = null;

            foreach ( TextLineViewModel line in lines )
            {
                lineContent = line.Content;

                if ( lineContent == focusedContent )
                {
                    goalLine = line;
                    break;
                }
            }

            return goalLine;
        }


        internal void EnableSplitting ( string content )
        {
            TextLineViewModel line = GetCoincidence (content);

            if ( line == null )
            {
                return;
            }

            List<string> strings = content.SplitBySeparators ();
            bool lineIsSplitable = ( strings.Count > 1 );
            EnableSplitting (lineIsSplitable, line);
        }


        private void EnableSplitting ( bool lineIsSplitable, TextLineViewModel splittable )
        {
            if ( lineIsSplitable )
            {
                _splittable = splittable;
                SplitterIsEnable = true;
            }
            else
            {
                _splittable = null;
                SplitterIsEnable = false;
            }
        }


        internal void Split ( List<string> splittedContents )
        {
            if ( _splittable == null )
            {
                return;
            }

            double layoutWidth = BeingProcessedBadge. BadgeWidth;
            List <TextLineViewModel> splitted = _splittable.SplitYourself (splittedContents, _scale, layoutWidth);
            BeingProcessedBadge.ReplaceTextLine ( _splittable, splitted );
            BeingProcessedBadge.CheckCorrectness ();
            ResetActiveIcon ();
            SplitterIsEnable = false;
        }

        #region FontSizeChange

        internal void IncreaseFontSize ( string focusedContent )
        {
            ObservableCollection<TextLineViewModel> lines = BeingProcessedBadge.TextLines;
            string lineContent = string.Empty;
            TextLineViewModel goalLine = null;

            foreach ( TextLineViewModel line in lines )
            {
                lineContent = line.Content;

                if ( lineContent == focusedContent )
                {
                    goalLine = line;
                    break;
                }
            }

            if ( goalLine != null )
            {
                goalLine.Increase (_scale);
                FocusedFontSize = goalLine.FontSize.ToString ();
            }
        }


        internal void ReduceFontSize ( string focusedContent )
        {
            ObservableCollection<TextLineViewModel> lines = BeingProcessedBadge.TextLines;
            string lineContent = string.Empty;
            TextLineViewModel goalLine = null;

            foreach ( TextLineViewModel line in lines )
            {
                lineContent = line.Content;

                if ( lineContent == focusedContent )
                {
                    goalLine = line;
                    break;
                }
            }

            if ( goalLine != null )
            {
                goalLine.Reduce (_scale);
                FocusedFontSize = goalLine.FontSize.ToString ();
            }
        }
        #endregion

        #region Scale

        internal void SetOriginalScale ( )
        {
            foreach ( KeyValuePair <BadgeViewModel, double> badgeScale   in   _scaleStorage ) 
            {
                BadgeViewModel badge = badgeScale.Key;
                double scale = badgeScale.Value;
                badge.ZoomOut (_scale);
                SetOriginalScale (badge, scale);
            }

        }


        private void SetStandartScale ( BadgeViewModel beingPrecessed ) 
        {
            if ( beingPrecessed.Scale > 1 )
            {
                beingPrecessed.ZoomOut (beingPrecessed.Scale);
            }
            if ( beingPrecessed.Scale < 1 )
            {
                beingPrecessed.ZoomOn (beingPrecessed.Scale);
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
                beingPrecessed.ZoomOut (scale);
            }
        }
        #endregion

        private void SetEnableBadgeNavigation ()
        {
            int badgeCount = VisibleBadges.Count;

            if ( badgeCount > 1 )
            {
                if ( ( BeingProcessedNumber > 1 )   &&   ( BeingProcessedNumber == badgeCount ) )
                {
                    FirstIsEnable = true;
                    PreviousIsEnable = true;
                    NextIsEnable = false;
                    LastIsEnable = false;
                }
                else if ( ( BeingProcessedNumber > 1 )   &&   ( BeingProcessedNumber < badgeCount ) )
                {
                    FirstIsEnable = true;
                    PreviousIsEnable = true;
                    NextIsEnable = true;
                    LastIsEnable = true;
                }
                else if ( ( BeingProcessedNumber == 1 )   &&   ( badgeCount == 1 ) )
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
        }


        private void SetBeingProcessed ( List <BadgeViewModel> visibleBadges )
        {
            BadgeViewModel beingPrecessed = visibleBadges [0];
            _scaleStorage.Add (beingPrecessed, beingPrecessed.Scale);
            SetStandartScale (beingPrecessed);
            beingPrecessed.ZoomOn (_scale);
            beingPrecessed.Show ();
            BeingProcessedBadge = beingPrecessed;
            BeingProcessedNumber = 1;
        }


        private void SetIcons (  )
        {
            if( (VisibleBadges != null)   &&   (VisibleBadges. Count > 0) ) 
            {
                for ( int index = 0;   index < VisibleBadges. Count;   index++ ) 
                {
                    BadgeCorrectnessViewModel icon = new BadgeCorrectnessViewModel (VisibleBadges [index].IsCorrect);
                    VisibleIcons.Add (icon);
                    icon.BorderColor = new SolidColorBrush (new Color (255, 255, 255, 255));

                    //bool metBeingProcessed = IncorrectBadges [index].Equals (BeingProcessedBadge);

                    //if ( metBeingProcessed ) 
                    //{
                    //    ActiveIcon = icon;
                    //    icon.BorderColor = new SolidColorBrush (new Color (155, 0, 0, 255));
                    //}
                }

                VisibleIcons [0].BorderColor = new SolidColorBrush (new Color (255, 0, 0, 0));
            }
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
            BeingProcessedNumber = 1;

            if ( ( VisibleBadges != null )   &&   ( VisibleBadges. Count > 0 ) )
            {
                for ( int index = 0;   index < VisibleBadges. Count;   index++ )
                {
                    BadgeViewModel badge = VisibleBadges [index];
                    BadgeCorrectnessViewModel icon = new BadgeCorrectnessViewModel (badge.IsCorrect);
                    VisibleIcons.Add (icon);
                    icon.BorderColor = new SolidColorBrush (new Color (255, 255, 255, 255));
                }

                VisibleIcons [0].BorderColor = new SolidColorBrush (new Color (255, 0, 0, 0));
                BeingProcessedBadge = VisibleBadges [0];
                BeingProcessedBadge.Show ();
            }
        }


        private void FilterByCorrectnes ( bool correctness )
        {
            VisibleIcons.Clear();

            if ( ( VisibleBadges != null )  &&  ( VisibleBadges.Count > 0 ) )
            {
                for ( int index = 0;   index < VisibleBadges.Count;   index++ )
                {
                    BadgeViewModel badge = VisibleBadges [index];
                    bool correctnessCoincides = badge.IsCorrect.Equals(correctness);

                    if ( correctnessCoincides ) 
                    {
                        IncorrectBadges.Add (badge);
                        BadgeCorrectnessViewModel icon = new BadgeCorrectnessViewModel (badge.IsCorrect);
                        VisibleIcons.Add (icon);
                        icon.BorderColor = new SolidColorBrush (new Color (255, 255, 255, 255));
                    }
                }

                VisibleIcons [0].BorderColor = new SolidColorBrush (new Color (255, 0, 0, 0));
            }
        }



        private enum FilterChoosing 
        {
           AllIsChosen = 0,
           CorrectsIsChosen = 1,
           IncorrectsIsChosen = 2
        }


        private enum Transition 
        {
            FromAllToCorr = 0,
            FromAllToIncorr = 1,
            FromCorrToAll = 2,
            FromCorrToIncorr = 3,
            FromIncorrToAll = 4,
            FromIncorrToCorr = 5
        }
    }

}
