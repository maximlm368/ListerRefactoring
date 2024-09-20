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
using ExCSS;
using System;
using AvaloniaEdit.Utils;

namespace Lister.ViewModels
{
    public partial class BadgeEditorViewModel : ViewModelBase
    {
        private static string _correctnessIcon = "GreenCheckMarker.jpg";
        private static string _incorrectnessIcon = "RedCross.png";
        private FilterChoosing _filterState = FilterChoosing.All;


        internal bool FilterProcessableBadge ( int filterableNumber )
        {
            bool filterOccured = false;

            if ( _filterState == FilterChoosing.All )
            {
                return false;
            }
            else if ( _filterState == FilterChoosing.Corrects )
            {
                if ( ! BeingProcessedBadge.IsCorrect )
                {
                    filterOccured = true;
                }
            }
            else if ( _filterState == FilterChoosing.Incorrects )
            {
                if ( BeingProcessedBadge.IsCorrect )
                {
                    filterOccured = true;
                }
            }

            //if ( filterOccured )
            //{
            //    AllBadges.Remove (BeingProcessedBadge);
            //    VisibleIcons.Remove (VisibleIcons [BeingProcessedNumber - 1]);
            //    ProcessableCount = AllBadges.Count;
            //    filterOccured = true;
            //}

            return filterOccured;
        }


        internal void Filter ()
        {
            if ( _filterState == FilterChoosing.All )
            {
                _filterState = FilterChoosing.Corrects;
            }
            else if ( _filterState == FilterChoosing.Corrects )
            {
                _filterState = FilterChoosing.Incorrects;
            }
            else if ( _filterState == FilterChoosing.Incorrects )
            {
                _filterState = FilterChoosing.All;
            }

            SwitchFilter ();
            EnableNavigation ();
        }


        //internal void FilterCorrects ()
        //{
        //    if ( _filterState == FilterChoosing.All )
        //    {
        //        _filterState = FilterChoosing.Corrects;
        //    }
        //    else if ( _filterState == FilterChoosing.Corrects )
        //    {
        //        _filterState = FilterChoosing.All;
        //    }
        //    else if ( _filterState == FilterChoosing.Incorrects )
        //    {
        //        _filterState = FilterChoosing.Corrects;
        //    }

        //    SwitchFilter ();
        //    EnableNavigation ();
        //}


        //internal void FilterIncorrects ()
        //{
        //    if ( _filterState == FilterChoosing.All )
        //    {
        //        _filterState = FilterChoosing.Incorrects;
        //    }
        //    else if ( _filterState == FilterChoosing.Corrects )
        //    {
        //        _filterState = FilterChoosing.Incorrects;
        //    }
        //    else if ( _filterState == FilterChoosing.Incorrects )
        //    {
        //        _filterState = FilterChoosing.All;
        //    }

        //    SwitchFilter ();
        //    EnableNavigation ();
        //}


        private void SwitchFilter ()
        {
            //VisibleIcons.Clear ();
            //AllBadges.Clear ();

            if ( _filterState == FilterChoosing.All )
            {



                //AllBadges.AddRange (FixedBadges);
                //AllBadges.AddRange (IncorrectBadges);
                //CorrectnessOpacity = 1;
                //IncorrectnessOpacity = 1;
            }
            else if ( _filterState == FilterChoosing.Corrects )
            {
                _currentAmmount = FixedBadges. Count;


                //AllBadges.AddRange (FixedBadges);
                CorrectnessOpacity = 1;
                IncorrectnessOpacity = 0.3;
            }
            else if ( _filterState == FilterChoosing.Incorrects )
            {
                _currentAmmount = IncorrectBadges. Count;

                VisibleIcons = new ObservableCollection<BadgeCorrectnessViewModel> ();

                

                for ( int index = 0;   index < _visibleRange;   index++ )
                {
                    BadgeViewModel badge = _allReadonlyBadges [IncorrectBadges.ElementAt (index).Key];

                    BadgeCorrectnessViewModel correctnessIcon = new BadgeCorrectnessViewModel (false, badge);

                    VisibleIcons.Add (correctnessIcon);
                }

                BeingProcessedBadge = AllBadges [0];

                ProcessableCount = IncorrectBadges. Count;

                //AllBadges.AddRange (IncorrectBadges);
                //CorrectnessOpacity = 0.3;
                //IncorrectnessOpacity = 1;
            }

            

            if ( ProcessableCount == 0 )
            {
                BeingProcessedNumber = 0;
            }
            else
            {
                BeingProcessedNumber = 1;
            }

            if ( ( AllBadges != null )   &&   ( AllBadges. Count > 0 ) )
            {
                //for ( int index = 0;   index < AllBadges. Count;   index++ )
                //{
                //    //BadgeViewModel badge = AllBadges [_allReadonlyBadges [index - 1]];
                //    //BadgeCorrectnessViewModel icon = 
                //    //    new BadgeCorrectnessViewModel (badge.IsCorrect, AllBadges [_allReadonlyBadges [index - 1]]);
                //    //VisibleIcons.Add (icon);
                //    //FadeIcon (icon);
                //}

                ActiveIcon = VisibleIcons [0];
                HighLightChosenIcon (ActiveIcon);
                //BeingProcessedBadge = AllBadges [_allReadonlyBadges [0]];
                SetToCorrectScale (BeingProcessedBadge);
                BeingProcessedBadge.Show ();
            }
        }
    }
}