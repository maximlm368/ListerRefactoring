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
        private readonly SolidColorBrush _focusedFontSizeColor;
        private readonly SolidColorBrush _releasedFontSizeColor;
        private readonly SolidColorBrush _focusedFontSizeBorderColor;
        private readonly SolidColorBrush _releasedFontSizeBorderColor;

        private SolidColorBrush _focusedFontsizeColor;
        internal SolidColorBrush FocusedFontSizeColor
        {
            get { return _focusedFontsizeColor; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _focusedFontsizeColor, value, nameof (FocusedFontSizeColor));
            }
        }

        private SolidColorBrush _focusedFontsizeBorderColor;
        internal SolidColorBrush FocusedFontSizeBorderColor
        {
            get { return _focusedFontsizeBorderColor; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _focusedFontsizeBorderColor, value, nameof (FocusedFontSizeBorderColor));
            }
        }


        internal void ResetFocusedText ( string newText )
        {
            BeingProcessedBadge.ResetFocusedText (newText);
            EnableSplitting (newText);
            ResetActiveIcon ();
        }


        #region Moving

        internal void MoveCaptured ( Point delta )
        {
            BeingProcessedBadge.MoveCaptured (delta);
        }


        internal void FocusedToSide ( string direction )
        {
            if ( BeingProcessedBadge == null ) return;

            BeingProcessedBadge.FocusedToSide (direction);
            ResetActiveIcon ();
        }

        #endregion

        internal void FocusTextLine ( string focusedContent, int elementNumber )
        {
            MakeBackUp ();
            BeingProcessedBadge.SetFocusedLine (focusedContent, elementNumber);

            if ( BeingProcessedBadge. FocusedLine != null )
            {
                MoversAreEnable = true;
                ZoommerIsEnable = true;
                EnableSplitting (focusedContent, elementNumber);
            }
        }


        internal void FocusShape ( ShapeKind kindName, int shapeId )
        {
            MakeBackUp ();

            if ( kindName == ShapeKind.rectangle )
            {
                BeingProcessedBadge.SetFocusedRectangle (shapeId);
            }
            else if ( kindName == ShapeKind.ellipse ) 
            {
                BeingProcessedBadge.SetFocusedEllipse (shapeId);
            }
        }


        internal void FocusImage ( int id )
        {
            MakeBackUp ();
            BeingProcessedBadge.SetFocusedImage (id);
        }


        private void MakeBackUp ()
        {
            if ( ! BeingProcessedBadge.IsChanged   &&   ( BackupNumbered [BeingProcessedBadge.Id] == null ) )
            {
                BackupNumbered [BeingProcessedBadge.Id] = BeingProcessedBadge.Clone ();
            }
        }


        internal void ReleaseCaptured ()
        {
            if ( BeingProcessedBadge == null ) 
            {
                return;
            }

            if ( BeingProcessedBadge.FocusedLine != null )
            {
                FocusedFontSizeBorderColor = null;

                DisableTextLineEdition ();

                BeingProcessedBadge.CheckFocusedLineCorrectness ();
                BeingProcessedBadge.FocusedFontSize = string.Empty;

                ResetActiveIcon ();
            }

            BadgeViewModel result = Printable [BeingProcessedBadge.Id];
            result.CopyFrom (BeingProcessedBadge);

            BeingProcessedBadge.ReleaseFocused ();
        }


        internal void EnableSplitting ( string content, int elementNumber )
        {
            TextLineViewModel line = BeingProcessedBadge.GetCoincidence (content, elementNumber);

            if ( line == null )
            {
                return;
            }

            EnableSplitting (content);
        }


        private void EnableSplitting ( string content )
        {
            string [] strings = content.Split (new char [] {' ', '-'}, StringSplitOptions.RemoveEmptyEntries);
            bool lineIsSplitable = ( strings.Length > 1 );

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
            DisableTextLineEdition ();
        }


        private void DisableTextLineEdition ()
        {
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