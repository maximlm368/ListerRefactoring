﻿using Avalonia.Controls;
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

namespace Lister.ViewModels
{
    public class BadgeEditorViewModel : ViewModelBase
    {
        private static string _correctnessIcon = "GreenCheckMarker.jpg";
        private static string _incorrectnessIcon = "RedCross.png";
        private double _scale = 2.65;
        private Dictionary<BadgeViewModel, double> _scaleStorage;
        private TextLineViewModel _splittable;
        private TextLineViewModel _focusedLine;
        private FilterChoosing _filterState = FilterChoosing.AllIsChosen;
        private bool _incorrectsAreSet = false;
        private ModernMainView _back;
        private BadgeEditorView _view;

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
            _drafts = new Dictionary <BadgeViewModel, BadgeViewModel> ();
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


        internal void CancelChanges ()
        {
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


        internal void GoBack ()
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

            MainWindow owner = _view.Parent as MainWindow;
            _back.ChangeSize (owner.WidthDifference, owner.HeightDifference);
            _back.SetIncorrectBadges (IncorrectBadges);
            owner.ResetDifference ();
            owner.Content = _back;
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
                    draftSource.TextLines = badge.TextLines;
                }
            }
        }


        #region Navigation

        internal void ToFirst ( )
        {
            BeingProcessedBadge.Hide ();
            BeingProcessedBadge = VisibleBadges [0];
            ActiveIcon. BorderColor = new SolidColorBrush (new Color (255, 255, 255, 255));
            ActiveIcon = VisibleIcons [0];
            ActiveIcon. BorderColor = new SolidColorBrush (new Color (255, 0, 0, 0));
            BeingProcessedBadge.Show ();
            BeingProcessedNumber = 1;
            SetToCorrectScale (BeingProcessedBadge);
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
            BeingProcessedBadge.Show ();
            BeingProcessedNumber--;
            SetToCorrectScale (BeingProcessedBadge);
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
            BeingProcessedBadge.Show ();
            BeingProcessedNumber++;
            SetToCorrectScale (BeingProcessedBadge);
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
            BeingProcessedBadge.Show ();
            BeingProcessedNumber = VisibleBadges. Count;
            SetToCorrectScale (BeingProcessedBadge);
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
                BeingProcessedBadge.Show ();
                BeingProcessedNumber = number;
                SetToCorrectScale (BeingProcessedBadge);
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

        internal void Focus ( string focusedContent )
        {
            BeingProcessedBadge.SetFocusedLine ( focusedContent );

            if ( BeingProcessedBadge. FocusedLine != null )
            {
                MoversAreEnable = true;
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
                SplitterIsEnable = false;
                MoversAreEnable = false;
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


        internal void EnableSplitting ( string content )
        {
            TextLineViewModel line = BeingProcessedBadge.GetCoincidence (content);

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
            else if ( ( BeingProcessedNumber == 1 ) && ( badgeCount > 1 ) )
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
                    BadgeCorrectnessViewModel icon = new BadgeCorrectnessViewModel (false);
                    VisibleIcons.Add (icon);
                    icon.BorderColor = new SolidColorBrush (new Color (255, 255, 255, 255));
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
                SetToCorrectScale (BeingProcessedBadge);
                BeingProcessedBadge.Show ();
            }
        }


        //private void FilterByCorrectnes ( bool correctness )
        //{
        //    VisibleIcons.Clear();

        //    if ( ( VisibleBadges != null )  &&  ( VisibleBadges.Count > 0 ) )
        //    {
        //        for ( int index = 0;   index < VisibleBadges.Count;   index++ )
        //        {
        //            BadgeViewModel badge = VisibleBadges [index];
        //            bool correctnessCoincides = badge.IsCorrect.Equals(correctness);

        //            if ( correctnessCoincides ) 
        //            {
        //                IncorrectBadges.Add (badge);
        //                BadgeCorrectnessViewModel icon = new BadgeCorrectnessViewModel (badge.IsCorrect);
        //                VisibleIcons.Add (icon);
        //                icon.BorderColor = new SolidColorBrush (new Color (255, 255, 255, 255));
        //            }
        //        }

        //        VisibleIcons [0].BorderColor = new SolidColorBrush (new Color (255, 0, 0, 0));
        //    }
        //}



        private enum FilterChoosing 
        {
           AllIsChosen = 0,
           CorrectsIsChosen = 1,
           IncorrectsIsChosen = 2
        }

    }

}
