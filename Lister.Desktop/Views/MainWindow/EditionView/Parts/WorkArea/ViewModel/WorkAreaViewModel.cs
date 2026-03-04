using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using Lister.Core.Entities.Badge;
using Lister.Desktop.ModelMappings.BadgeVM;

namespace Lister.Desktop.Views.MainWindow.EditionView.Parts.WorkArea.ViewModel;

internal partial class WorkAreaViewModel : ObservableObject
{
    private double _scale = 1.5624;

    private BadgeViewModel? _processable;
    internal BadgeViewModel? Processable 
    {
        get => _processable;

        private set 
        {
            if ( _processable != null )
            {
                _processable.ReleaseFocused ();
                _processable.Hide ();
                _processable.Model.CorrectnessChanged -= HandleBadgeCorrectnessChanged;
            }

            _processable = value;

            if ( value != null && _processable != null ) 
            {
                _processable.Model.CorrectnessChanged += HandleBadgeCorrectnessChanged;
            }

            Processable?.Show ();
            SetCorrectScale ();
            OnPropertyChanged ();
        }
    }

    internal event Action<string, int>? ElementGotFocus;
    internal event Action? ElementLostFocus;
    internal event Action<bool>? CorrectnessChanged;

    public WorkAreaViewModel ()
    {

    }

    internal void SetProcessable ( BadgeViewModel? processable ) 
    {
        Processable?.Hide ();
        Processable = processable;
        SetCorrectScale ();
        Processable?.Show ();
    }

    private void SetCorrectScale ( )
    {
        if ( Processable != null && Processable.Scale != _scale )
        {
            if ( Processable.Scale != 1 )
            {
                Processable.ZoomOut ( Processable.Scale );
            }

            Processable.ZoomOn ( _scale );
        }
    }

    private void HandleBadgeCorrectnessChanged ( ) 
    {
        if ( _processable != null ) 
        {
            CorrectnessChanged?.Invoke ( _processable.Model.IsCorrect );
        }
    }

    internal void SetUp ( BadgeViewModel processable ) 
    {
        Processable = processable;
    }

    internal void CancelChanges ( )
    {
        Processable?.CancelChanges ( );
    }

    internal void ZoomOn ( double step ) 
    {
        Processable?.ZoomOn ( step );
        _scale *= step;
    }

    internal void ZoomOut ( double step )
    {
        Processable?.ZoomOut ( step );
        _scale /= step;
    }

    #region Processing
    internal void MoveCaptured ( Point delta )
    {
        Processable?.MoveCaptured ( delta );
    }

    internal void FocusedToSide ( string direction )
    {
        if ( Processable == null ) 
        {
            return;
        }

        Processable.FocusedToSide ( direction );
    }

    internal void Split ()
    {
        Processable?.Split ();
    }
    #endregion

    #region Focusing
    internal void FocusTextLine ( string? focusedContent, int elementNumber )
    {
        if ( Processable == null || focusedContent == null )
        {
            return;
        }

        Processable.SetFocusedLine ( focusedContent, elementNumber );
        ElementGotFocus?.Invoke ( focusedContent, elementNumber );
    }

    internal void FocusShape ( ShapeType kindName, int shapeId )
    {
        if ( kindName == ShapeType.rectangle )
        {
            Processable?.SetFocusedRectangle ( shapeId );
        }
        else if ( kindName == ShapeType.ellipse )
        {
            Processable?.SetFocusedEllipse ( shapeId );
        }
    }

    internal void FocusImage ( int id )
    {
        Processable?.SetFocusedImage ( id );
    }

    internal void ReleaseCaptured ()
    {
        if ( Processable == null )
        {
            return;
        }

        Processable.ReleaseFocused ();
        ElementLostFocus?.Invoke ();
    }
    #endregion

    #region FontSizeChange
    internal void IncreaseFontSize ()
    {
        Processable?.IncreaseFontSize ();
    }

    internal void DecreaseFontSize ()
    {
        Processable?.ReduceFontSize ();
    }
    #endregion
}
