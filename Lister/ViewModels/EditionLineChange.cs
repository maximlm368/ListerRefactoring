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

namespace Lister.ViewModels
{
    public partial class BadgeEditorViewModel : ViewModelBase
    {
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


        internal void Left ()
        {
            BeingProcessedBadge.Left (_scale);
            ResetActiveIcon ();
        }


        internal void Right ()
        {
            BeingProcessedBadge.Right (_scale);
            ResetActiveIcon ();
        }


        internal void Up ()
        {
            BeingProcessedBadge.Up (_scale);
            ResetActiveIcon ();
        }


        internal void Down ()
        {
            BeingProcessedBadge.Down (_scale);
            ResetActiveIcon ();
        }
        #endregion

        internal void Focus ( string focusedContent, int elementNumber )
        {
            BeingProcessedBadge.SetFocusedLine (focusedContent, elementNumber);

            if ( BeingProcessedBadge.FocusedLine != null )
            {
                MoversAreEnable = true;
                ZoommerIsEnable = true;

                EnableSplitting (focusedContent, elementNumber);
            }
        }


        internal void ReleaseCaptured ()
        {
            if ( BeingProcessedBadge.FocusedLine != null )
            {
                BeingProcessedBadge.CheckFocusedLineCorrectness ();
                BeingProcessedBadge.FocusedLine = null;
                BeingProcessedBadge.FocusedFontSize = string.Empty;
                ResetActiveIcon ();
                ZoommerIsEnable = false;
                MoversAreEnable = false;
                SplitterIsEnable = false;
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