using Avalonia;
using Avalonia.Media;
using Core.Models.Badge;
using ReactiveUI;
using View.CoreModelReflection.Badge;

namespace View.EditionView.ViewModel;

public partial class BadgeEditorViewModel : ReactiveObject
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


    #region Processing

    internal void MoveCaptured ( Point delta )
    {
        BeingProcessedBadge.MoveCaptured (delta);
        ResetActiveIcon ();
    }


    internal void FocusedToSide ( string direction )
    {
        if ( BeingProcessedBadge == null ) return;

        BeingProcessedBadge.FocusedToSide (direction);
        ResetActiveIcon ();
    }


    internal void ResetFocusedText ( string newText )
    {
        BeingProcessedBadge.ResetFocusedText ( newText );
        EnableSplitting ( newText );
        ResetActiveIcon ();
    }


    internal void Split ()
    {
        BeingProcessedBadge.Split ( _scale );
        DisableTextLineEdition ();
        ResetActiveIcon ();
    }

    #endregion

    #region Focusing

    internal void FocusTextLine ( string focusedContent, int elementNumber )
    {
        BeingProcessedBadge.SetFocusedLine (focusedContent, elementNumber);

        if ( BeingProcessedBadge. FocusedLine != null )
        {
            MoversAreEnable = true;
            ZoommerIsEnable = true;
            EnableSplitting (focusedContent, elementNumber);
        }
    }


    internal void FocusShape ( ShapeType kindName, int shapeId )
    {
        if ( kindName == ShapeType.rectangle )
        {
            BeingProcessedBadge.SetFocusedRectangle (shapeId);
        }
        else if ( kindName == ShapeType.ellipse ) 
        {
            BeingProcessedBadge.SetFocusedEllipse (shapeId);
        }
    }


    internal void FocusImage ( int id )
    {
        BeingProcessedBadge.SetFocusedImage (id);
    }


    internal void ReleaseCaptured ()
    {
        if ( BeingProcessedBadge == null ) 
        {
            return;
        }

        FocusedFontSizeBorderColor = null;
        DisableTextLineEdition ();
        ResetActiveIcon ();
        BeingProcessedBadge.ReleaseFocused ();
    }
    #endregion

    #region FontSizeChange

    internal void IncreaseFontSize ()
    {
        BeingProcessedBadge.IncreaseFontSize ();
        ResetActiveIcon ();
    }


    internal void ReduceFontSize ()
    {
        BeingProcessedBadge.ReduceFontSize ();
        ResetActiveIcon ();
    }
    #endregion


    #region ActionsConsequences

    private void ResetActiveIcon ()
    {
        if ( BeingProcessedBadge.IsCorrect )
        {
            if ( ! ActiveIcon.Correctness )
            {
                ActiveIcon.SwitchCorrectness ();

                if ( _filterState == FilterChoosing.All )
                {
                    try
                    {
                        CorrectNumbered.Add ( BeingProcessedBadge );
                        IncorrectNumbered.Remove ( BeingProcessedBadge );
                    }
                    catch ( Exception ex ) { }
                }

                IncorrectBadgesCount--;
            }
        }
        else if ( ! BeingProcessedBadge.IsCorrect )
        {
            if ( ActiveIcon.Correctness )
            {
                ActiveIcon.SwitchCorrectness ();

                if ( _filterState == FilterChoosing.All )
                {
                    try
                    {
                        IncorrectNumbered.Add ( BeingProcessedBadge );
                        CorrectNumbered.Remove ( BeingProcessedBadge );
                    }
                    catch ( Exception ex ) { }
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
        TextLineViewModel line = BeingProcessedBadge.GetCoincidence ( content, elementNumber );

        if ( line == null )
        {
            return;
        }

        EnableSplitting ( content );
    }


    private void EnableSplitting ( string content )
    {
        string [] strings = content.Split ( new char [] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries );
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
    #endregion
}