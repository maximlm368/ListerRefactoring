using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Lister.Core.Models.Badge;
using Lister.Desktop.ModelMappings.BadgeVM;

namespace Lister.Desktop.Views.MainWindow.EditionView.ViewModel;

internal partial class BadgeEditorViewModel : ObservableObject
{
    private readonly SolidColorBrush _focusedFontsizeColor;
    private readonly SolidColorBrush _releasedFontsizeColor;
    private readonly SolidColorBrush _focusedFontsizeBorderColor;
    private readonly SolidColorBrush _releasedFontsizeBorderColor;

    [ObservableProperty]
    private SolidColorBrush _focusedFontSizeColor;

    [ObservableProperty]
    private SolidColorBrush _focusedFontSizeBorderColor;

    #region Processing
    internal void MoveCaptured ( Point delta )
    {
        ProcessableBadge?.MoveCaptured ( delta );
        ResetActiveIcon ();
    }

    internal void FocusedToSide ( string direction )
    {
        if ( ProcessableBadge == null ) return;

        ProcessableBadge.FocusedToSide ( direction );
        ResetActiveIcon ();
    }

    internal void ResetFocusedText ( string newText )
    {
        ProcessableBadge?.ResetFocusedText ( newText );
        EnableSplitting ( newText );
        ResetActiveIcon ();
    }

    internal void Split ()
    {
        ProcessableBadge?.Split ();
        DisableTextLineEdition ();
        ResetActiveIcon ();
    }
    #endregion

    #region Focusing
    internal void FocusTextLine ( string? focusedContent, int elementNumber )
    {
        if ( ProcessableBadge == null || focusedContent == null )
        {
            return;
        }

        ProcessableBadge.SetFocusedLine ( focusedContent, elementNumber );

        if ( ProcessableBadge.FocusedLine != null )
        {
            MoversAreEnable = true;
            ZoommerIsEnable = true;
            EnableSplitting ( focusedContent, elementNumber );
        }
    }

    internal void FocusShape ( ShapeType kindName, int shapeId )
    {
        if ( kindName == ShapeType.rectangle )
        {
            ProcessableBadge?.SetFocusedRectangle ( shapeId );
        }
        else if ( kindName == ShapeType.ellipse )
        {
            ProcessableBadge?.SetFocusedEllipse ( shapeId );
        }
    }

    internal void FocusImage ( int id )
    {
        ProcessableBadge?.SetFocusedImage ( id );
    }

    internal void ReleaseCaptured ()
    {
        if ( ProcessableBadge == null )
        {
            return;
        }

        FocusedFontSizeBorderColor = _releasedFontsizeBorderColor;
        DisableTextLineEdition ();
        ResetActiveIcon ();
        ProcessableBadge.ReleaseFocused ();
    }
    #endregion

    #region FontSizeChange
    internal void IncreaseFontSize ()
    {
        ProcessableBadge?.IncreaseFontSize ();
        ResetActiveIcon ();
    }

    internal void ReduceFontSize ()
    {
        ProcessableBadge?.ReduceFontSize ();
        ResetActiveIcon ();
    }
    #endregion

    #region ActionsConsequences
    private void ResetActiveIcon ()
    {
        if ( ProcessableBadge == null || ActiveIcon == null )
        {
            return;
        }

        if ( ProcessableBadge.IsCorrect )
        {
            if ( !ActiveIcon.Correctness )
            {
                ActiveIcon.SwitchCorrectness ();

                if ( _filterState == FilterChoosing.All )
                {
                    CorrectNumbered.Add ( ProcessableBadge );
                    IncorrectNumbered.Remove ( ProcessableBadge );
                }

                IncorrectBadgesCount--;
            }
        }
        else if ( !ProcessableBadge.IsCorrect )
        {
            if ( ActiveIcon.Correctness )
            {
                ActiveIcon.SwitchCorrectness ();

                if ( _filterState == FilterChoosing.All )
                {
                    IncorrectNumbered.Add ( ProcessableBadge );
                    CorrectNumbered.Remove ( ProcessableBadge );
                }

                IncorrectBadgesCount++;
            }
        }
    }

    private void DisableTextLineEdition ()
    {
        SplitterIsEnable = false;
        MoversAreEnable = false;
        ZoommerIsEnable = false;
    }

    internal void EnableSplitting ( string content, int elementNumber )
    {
        TextLineViewModel? line = ProcessableBadge?.GetCoincidence ( content, elementNumber );

        if ( line == null )
        {
            return;
        }

        EnableSplitting ( content );
    }

    private void EnableSplitting ( string content )
    {
        string [] strings = content.Split ( [' ', '-'], StringSplitOptions.RemoveEmptyEntries );
        bool lineIsSplitable = strings.Length > 1;

        if ( lineIsSplitable )
        {
            SplitterIsEnable = true;
        }
        else
        {
            SplitterIsEnable = false;
        }
    }
    #endregion
}
