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

namespace Lister.ViewModels
{
    public class BadgeEditorViewModel : ViewModelBase
    {
        private const double coefficient = 1.1;
        private double _scale = 2.8;
        private Dictionary<BadgeViewModel, double> _scaleStorage; 

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

                BadgeViewModel beingPrecessed = value [0];
                _scaleStorage.Add (beingPrecessed, beingPrecessed.Scale);
                SetStandartScale (beingPrecessed);
                beingPrecessed.ZoomOn (_scale);
                beingPrecessed.Show ();
                BeingProcessedBadge = beingPrecessed;
                BeingProcessedNumber = 1;
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

        private string fS;
        internal string FocusedFontSize
        {
            get { return fS; }
            set
            {
                this.RaiseAndSetIfChanged (ref fS, value, nameof (FocusedFontSize));
            }
        }


        public BadgeEditorViewModel ( )
        {
            _scaleStorage = new Dictionary <BadgeViewModel, double> ();
            FocusedFontSize = string.Empty;
        }


        internal void ToFirst ( )
        {
            BeingProcessedBadge.Hide ();
            BeingProcessedBadge = IncorrectBadges [0];
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
                //FormattedText formatted = new FormattedText (lineContent, CultureInfo.CurrentCulture
                //                                           , FlowDirection.LeftToRight, Typeface.Default
                //                                           , goalLine.FontSize, null);

                //goalLine.Width = formatted.Width * coefficient;
                goalLine.TopOffset -= delta.Y;
                goalLine.LeftOffset -= delta.X;
            } 
        }


        internal void Left ( string focusedContent )
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
                goalLine.LeftOffset -= _scale;
            }
        }


        internal void Right ( string focusedContent )
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
                goalLine.LeftOffset += _scale;
            }
        }


        internal void Up ( string focusedContent )
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
                goalLine.TopOffset -= _scale;
            }
        }


        internal void Down ( string focusedContent )
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
                goalLine.TopOffset += _scale;
            }
        }


        internal void ReduceFontSize ( string focusedContent )
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
                goalLine.FontSize -= _scale;
                goalLine.Width -= _scale;
                goalLine.Height -= _scale;
                FocusedFontSize = goalLine.FontSize.ToString();
            }
        }


        internal void Focus ( string focusedContent )
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
                FocusedFontSize = goalLine.FontSize.ToString ();
                MoversAreEnable = true;
            }
        }


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


        //internal void ReleaseCaptured ( )
        //{
        //    BeingProcessedBadge.TextLines [0].TopOffset -= delta.Y;
        //    BeingProcessedBadge.TextLines [0].LeftOffset -= delta.X;
        //}
    }
}
