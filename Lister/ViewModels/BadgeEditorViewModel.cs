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

namespace Lister.ViewModels
{
    public class BadgeEditorViewModel : ViewModelBase
    {
        private double _scale = 2.8;
        private Dictionary<BadgeViewModel, double> _scaleStorage;
        private TextLineViewModel _splittable;
        private TextLineViewModel _focusedLine;

        private List <BadgeViewModel> _incorrectBadges;
        internal List <BadgeViewModel> IncorrectBadges
        {
            get { return _incorrectBadges; }
            set
            {
                bool isNullOrEmpty = ( value == null )   ||   ( value.Count == 0 );

                if ( isNullOrEmpty )
                {
                    return;
                }

                this.RaiseAndSetIfChanged (ref _incorrectBadges, value, nameof (IncorrectBadges));

                if ( value [0] == null )
                {
                    return;
                }

                SetBeingProcessed (value);
                SetIcons ();
            }
        }

        private ObservableCollection <BadgeCorrectnessViewModel> cL;
        internal ObservableCollection <BadgeCorrectnessViewModel> CorrectnessIcons
        {
            get { return cL; }
            set
            {
                this.RaiseAndSetIfChanged (ref cL, value, nameof (CorrectnessIcons));
            }
        }

        private BadgeCorrectnessViewModel bpI;
        internal BadgeCorrectnessViewModel ActiveIcon
        {
            get { return bpI; }
            set
            {
                this.RaiseAndSetIfChanged (ref bpI, value, nameof (ActiveIcon));
            }
        }

        private BadgeViewModel bpB;
        internal BadgeViewModel BeingProcessedBadge
        {
            get { return bpB; }
            set
            {
                this.RaiseAndSetIfChanged (ref bpB, value, nameof (BeingProcessedBadge));
            }
        }

        private int bpN;
        internal int BeingProcessedNumber
        {
            get { return bpN; }
            set
            {
                this.RaiseAndSetIfChanged (ref bpN, value, nameof (BeingProcessedNumber));
            }
        }

        private bool fE;
        internal bool FirstIsEnable
        {
            get { return fE; }
            set
            {
                this.RaiseAndSetIfChanged (ref fE, value, nameof (FirstIsEnable));
            }
        }

        private bool pE;
        internal bool PreviousIsEnable
        {
            get { return pE; }
            set
            {
                this.RaiseAndSetIfChanged (ref pE, value, nameof (PreviousIsEnable));
            }
        }

        private bool nE;
        internal bool NextIsEnable
        {
            get { return nE; }
            set
            {
                this.RaiseAndSetIfChanged (ref nE, value, nameof (NextIsEnable));
            }
        }

        private bool lE;
        internal bool LastIsEnable
        {
            get { return lE; }
            set
            {
                this.RaiseAndSetIfChanged (ref lE, value, nameof (LastIsEnable));
            }
        }

        private bool mE;
        internal bool MoversAreEnable
        {
            get { return mE; }
            set
            {
                this.RaiseAndSetIfChanged (ref mE, value, nameof (MoversAreEnable));
            }
        }

        private bool sE;
        internal bool SplitterIsEnable
        {
            get { return sE; }
            set
            {
                this.RaiseAndSetIfChanged (ref sE, value, nameof (SplitterIsEnable));
            }
        }

        private string fS;
        internal string FocusedFontSize
        {
            get { return fS; }
            set
            {
                this.RaiseAndSetIfChanged (ref fS, value, nameof (FocusedFontSize));
            }
        }

        private Thickness fBT;
        internal Thickness FocusedBorderThickness
        {
            get { return fBT; }
            set
            {
                this.RaiseAndSetIfChanged (ref fBT, value, nameof (FocusedBorderThickness));
            }
        }


        public BadgeEditorViewModel ( )
        {
            _scaleStorage = new Dictionary <BadgeViewModel, double> ();
            FocusedFontSize = string.Empty;
            SplitterIsEnable = false;
            CorrectnessIcons = new ObservableCollection<BadgeCorrectnessViewModel> ();
        }

        #region Navigation

        internal void ToFirst ( )
        {
            BeingProcessedBadge.Hide ();
            BeingProcessedBadge = IncorrectBadges [0];
            ActiveIcon. BorderColor = new SolidColorBrush (new Color (255, 255, 255, 255));
            ActiveIcon = CorrectnessIcons [0];
            ActiveIcon. BorderColor = new SolidColorBrush (new Color (255, 0, 0, 0));
            AddToStorage ();
            BeingProcessedBadge.Show ();
            BeingProcessedNumber = 1;
            SetCorrectScale ( );
            SetEnableBadgeNavigation ();
            MoversAreEnable = false;
        }


        internal void ToPrevious ( )
        {
            BeingProcessedBadge.Hide ();
            BeingProcessedBadge = IncorrectBadges [BeingProcessedNumber - 2];
            ActiveIcon. BorderColor = new SolidColorBrush (new Color (255, 255, 255, 255));
            ActiveIcon = CorrectnessIcons [BeingProcessedNumber - 2];
            ActiveIcon. BorderColor = new SolidColorBrush (new Color (255, 0, 0, 0));
            AddToStorage ();
            BeingProcessedBadge.Show ();
            BeingProcessedNumber--;
            SetCorrectScale( );
            SetEnableBadgeNavigation ();
            MoversAreEnable = false;
        }


        internal void ToNext ()
        {
            BeingProcessedBadge.Hide ();
            BeingProcessedBadge = IncorrectBadges [BeingProcessedNumber];
            ActiveIcon. BorderColor = new SolidColorBrush (new Color (255, 255, 255, 255));
            ActiveIcon = CorrectnessIcons [BeingProcessedNumber];
            ActiveIcon. BorderColor = new SolidColorBrush (new Color (255, 0, 0, 0));
            AddToStorage ();
            BeingProcessedBadge.Show ();
            BeingProcessedNumber++;
            SetCorrectScale ( );
            SetEnableBadgeNavigation ();
            MoversAreEnable = false;
        }


        internal void ToLast ( )
        {
            BeingProcessedBadge.Hide ();
            BeingProcessedBadge = IncorrectBadges [IncorrectBadges. Count - 1];
            ActiveIcon. BorderColor = new SolidColorBrush (new Color (255, 255, 255, 255));
            ActiveIcon = CorrectnessIcons [CorrectnessIcons. Count - 1];
            ActiveIcon. BorderColor = new SolidColorBrush (new Color (255, 0, 0, 0));
            AddToStorage ();
            BeingProcessedBadge.Show ();
            BeingProcessedNumber = IncorrectBadges. Count;
            SetCorrectScale ( );
            SetEnableBadgeNavigation ();
            MoversAreEnable = false;
        }


        internal void ToParticularBadge ( string numberAsText )
        {
            try
            {
                int number = int.Parse (numberAsText);

                if ( number > IncorrectBadges.Count   ||   number < 1 )
                {
                    BeingProcessedNumber = BeingProcessedNumber;
                    return;
                }

                BeingProcessedBadge.Hide ();
                BeingProcessedBadge = IncorrectBadges [number - 1];

                if ( ActiveIcon != null ) 
                {
                    ActiveIcon.BorderColor = new SolidColorBrush (new Color (255, 255, 255, 255));
                }

                ActiveIcon = CorrectnessIcons [number - 1];
                ActiveIcon. BorderColor = new SolidColorBrush (new Color (255, 0, 0, 0));
                AddToStorage ();
                BeingProcessedBadge.Show ();
                BeingProcessedNumber = number;
                SetCorrectScale ();
                SetEnableBadgeNavigation ();
                MoversAreEnable = false;
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
                _focusedLine = null;
            }

            BeingProcessedBadge.CheckCorrectness ();
            ResetActiveIcon ();
        }


        private void ResetActiveIcon ()
        {
            if ( BeingProcessedBadge.IsCorrect )
            {
                CorrectnessIcons [BeingProcessedNumber - 1] = new BadgeCorrectnessViewModel (true);
                CorrectnessIcons [BeingProcessedNumber - 1].BorderColor = new SolidColorBrush (new Color (255, 0, 0, 0));
            }
            else
            {
                if ( CorrectnessIcons [BeingProcessedNumber - 1].Correctness )
                {
                    CorrectnessIcons [BeingProcessedNumber - 1] = new BadgeCorrectnessViewModel (false);
                    CorrectnessIcons [BeingProcessedNumber - 1].BorderColor = new SolidColorBrush (new Color (255, 0, 0, 0));
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
            //bool lineIsSplitable = ( strings.Count > 1 ) && ( !line.IsCorrect () );

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
                goalLine.FontSize += _scale;
                goalLine.Width += _scale;
                goalLine.Height += _scale;
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
                goalLine.FontSize -= _scale;
                goalLine.Width -= _scale;
                goalLine.Height -= _scale;
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
            int badgeCount = IncorrectBadges.Count;

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


        private void SetBeingProcessed ( List<BadgeViewModel> incorrectBadges )
        {
            BadgeViewModel beingPrecessed = incorrectBadges [0];
            _scaleStorage.Add (beingPrecessed, beingPrecessed.Scale);
            SetStandartScale (beingPrecessed);
            beingPrecessed.ZoomOn (_scale);
            beingPrecessed.Show ();
            BeingProcessedBadge = beingPrecessed;
            BeingProcessedNumber = 1;
        }


        private void SetIcons (  )
        {
            if( (IncorrectBadges != null)   &&   (IncorrectBadges. Count > 0) ) 
            {
                for ( int index = 0;   index < IncorrectBadges. Count;   index++ ) 
                {
                    BadgeCorrectnessViewModel icon = new BadgeCorrectnessViewModel (IncorrectBadges [index].IsCorrect);
                    CorrectnessIcons.Add (icon);
                    icon.BorderColor = new SolidColorBrush (new Color (255, 255, 255, 255));

                    //bool metBeingProcessed = IncorrectBadges [index].Equals (BeingProcessedBadge);

                    //if ( metBeingProcessed ) 
                    //{
                    //    ActiveIcon = icon;
                    //    icon.BorderColor = new SolidColorBrush (new Color (155, 0, 0, 255));
                    //}
                }

                CorrectnessIcons [0].BorderColor = new SolidColorBrush (new Color (255, 0, 0, 0));
            }
        }

    }
}
