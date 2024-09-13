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
        private static string _correctnessIcon = "GreenCheckMarker.jpg";
        private static string _incorrectnessIcon = "RedCross.png";
        private FilterChoosing _filterState = FilterChoosing.All;

        private void SetUpIcons ()
        {
            VisibleIcons = new ObservableCollection <BadgeCorrectnessViewModel> ();

            string correctnessIcon = App.ResourceDirectoryUri + _correctnessIcon;
            string incorrectnessIcon = App.ResourceDirectoryUri + _incorrectnessIcon;

            Uri correctUri = new Uri (correctnessIcon);
            CorrectnessIcon = ImageHelper.LoadFromResource (correctUri);
            Uri incorrectUri = new Uri (incorrectnessIcon);
            IncorrectnessIcon = ImageHelper.LoadFromResource (incorrectUri);

            CorrectnessOpacity = 1;
            IncorrectnessOpacity = 1;
        }


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

            if ( filterOccured )
            {
                VisibleBadges.Remove (BeingProcessedBadge);
                VisibleIcons.Remove (VisibleIcons [BeingProcessedNumber - 1]);
                ProcessableCount = VisibleBadges.Count;
                filterOccured = true;
            }

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


        internal void FilterCorrects ()
        {
            if ( _filterState == FilterChoosing.All )
            {
                _filterState = FilterChoosing.Corrects;
            }
            else if ( _filterState == FilterChoosing.Corrects )
            {
                _filterState = FilterChoosing.All;
            }
            else if ( _filterState == FilterChoosing.Incorrects )
            {
                _filterState = FilterChoosing.Corrects;
            }

            SwitchFilter ();
            EnableNavigation ();
        }


        internal void FilterIncorrects ()
        {
            if ( _filterState == FilterChoosing.All )
            {
                _filterState = FilterChoosing.Incorrects;
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


        private void SwitchFilter ()
        {
            VisibleIcons.Clear ();
            VisibleBadges.Clear ();

            if ( _filterState == FilterChoosing.All )
            {
                VisibleBadges.AddRange (FixedBadges);
                VisibleBadges.AddRange (IncorrectBadges);
                CorrectnessOpacity = 1;
                IncorrectnessOpacity = 1;
            }
            else if ( _filterState == FilterChoosing.Corrects )
            {
                VisibleBadges.AddRange (FixedBadges);
                CorrectnessOpacity = 1;
                IncorrectnessOpacity = 0.3;
            }
            else if ( _filterState == FilterChoosing.Incorrects )
            {
                VisibleBadges.AddRange (IncorrectBadges);
                CorrectnessOpacity = 0.3;
                IncorrectnessOpacity = 1;
            }

            ProcessableCount = VisibleBadges.Count;

            if ( ProcessableCount == 0 )
            {
                BeingProcessedNumber = 0;
            }
            else
            {
                BeingProcessedNumber = 1;
            }

            if ( ( VisibleBadges != null ) && ( VisibleBadges.Count > 0 ) )
            {
                for ( int index = 0; index < VisibleBadges.Count; index++ )
                {
                    BadgeViewModel badge = VisibleBadges [index];
                    BadgeCorrectnessViewModel icon = new BadgeCorrectnessViewModel (badge.IsCorrect, VisibleBadges [index]);
                    VisibleIcons.Add (icon);
                    FadeIcon (icon);
                }

                ActiveIcon = VisibleIcons [0];
                HighLightChosenIcon (ActiveIcon);
                BeingProcessedBadge = VisibleBadges [0];
                SetToCorrectScale (BeingProcessedBadge);
                BeingProcessedBadge.Show ();
            }
        }
    }
}