using Avalonia;
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
using ReactiveUI;

namespace Lister.ViewModels
{
    public partial class BadgeEditorViewModel : ViewModelBase
    {
        private readonly SolidColorBrush _focusedFontSizeColor = new SolidColorBrush(new Color(255, 255, 255, 255));
        private readonly SolidColorBrush _releasedFontSizeColor = new SolidColorBrush (new Color (255, 175, 175, 175));

        private readonly SolidColorBrush _focusedFontSizeBorderColor = new SolidColorBrush (new Color (255, 50, 50, 50));
        private readonly SolidColorBrush _releasedFontSizeBorderColor = new SolidColorBrush (new Color (255, 150, 150, 150));

        private SolidColorBrush ffsc;
        internal SolidColorBrush FocusedFontSizeColor
        {
            get { return ffsc; }
            private set
            {
                this.RaiseAndSetIfChanged (ref ffsc, value, nameof (FocusedFontSizeColor));
            }
        }

        private SolidColorBrush ffsbc;
        internal SolidColorBrush FocusedFontSizeBorderColor
        {
            get { return ffsbc; }
            private set
            {
                this.RaiseAndSetIfChanged (ref ffsbc, value, nameof (FocusedFontSizeBorderColor));
            }
        }


        internal void ResetFocusedText ( string newText )
        {
            BeingProcessedBadge.ResetFocusedText (newText);
            ResetActiveIcon ();
        }


        #region Moving

        internal void MoveCaptured ( Point delta )
        {
            BeingProcessedBadge.MoveCaptured (delta);
        }


        internal void ToSide ( string direction )
        {
            BeingProcessedBadge.ToSide (direction, _scale);
            ResetActiveIcon ();
        }

        #endregion

        internal void Focus ( string focusedContent, int elementNumber )
        {
            FocusedFontSizeColor = _focusedFontSizeColor;
            FocusedFontSizeBorderColor = _focusedFontSizeBorderColor;

            if ( ! BeingProcessedBadge. IsChanged    &&   ( BackupNumbered [BeingProcessedBadge. Id] == null ) ) 
            {
                BackupNumbered [BeingProcessedBadge. Id] = BeingProcessedBadge.Clone ();
            }

            BeingProcessedBadge.SetFocusedLine (focusedContent, elementNumber);

            if ( BeingProcessedBadge. FocusedLine != null )
            {
                MoversAreEnable = true;
                ZoommerIsEnable = true;
                EnableSplitting (focusedContent, elementNumber);
            }
        }


        internal void ReleaseCaptured ()
        {
            FocusedFontSizeColor = _releasedFontSizeColor;
            FocusedFontSizeBorderColor = _releasedFontSizeBorderColor;
            ZoommerIsEnable = false;
            MoversAreEnable = false;
            SplitterIsEnable = false;

            if ( BeingProcessedBadge == null ) 
            {
                return;
            }

            if ( BeingProcessedBadge. FocusedLine != null )
            {
                BeingProcessedBadge.CheckFocusedLineCorrectness ();
                
                BeingProcessedBadge.FocusedFontSize = string.Empty;

                BadgeViewModel printable = Printable [BeingProcessedBadge. Id];
                printable.CopyFrom ( BeingProcessedBadge );

                BeingProcessedBadge.FocusedLine = null;
                ResetActiveIcon ();
            }
        }


        internal void EnableSplitting ( string content, int elementNumber )
        {
            TextLineViewModel line = BeingProcessedBadge.GetCoincidence (content, elementNumber);

            if ( line == null )
            {
                return;
            }

            List<string> strings = content.SplitBySeparators (new List<char> () { ' ', '-' });
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


        internal void Split ()
        {
            BeingProcessedBadge.Split (_scale);
            ResetActiveIcon ();
            SplitterIsEnable = false;
            MoversAreEnable = false;
            ZoommerIsEnable = false;
        }

        #region FontSizeChange

        internal void IncreaseFontSize ()
        {
            BeingProcessedBadge.IncreaseFontSize (_scale);
            ResetActiveIcon ();
        }


        internal void ReduceFontSize ()
        {
            BeingProcessedBadge.ReduceFontSize (_scale);
            ResetActiveIcon ();
        }
        #endregion
    }
}