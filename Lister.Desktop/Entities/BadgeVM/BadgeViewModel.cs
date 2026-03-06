using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Lister.Core.Entities.Badge;
using Lister.Desktop.ExecutersForCoreAbstractions.BadgeCreator;
using Lister.Desktop.Extentions;
using Lister.Desktop.Views.MainView.ViewModel;
using System.Collections.ObjectModel;
using Imaging = Avalonia.Media.Imaging;

namespace Lister.Desktop.Entities.BadgeVM;

/// <summary>
/// Maps Badge model into visible entity.
/// </summary>
public sealed partial class BadgeViewModel : ObservableObject
{
    private static readonly Dictionary<string, Imaging.Bitmap?> _pathToImage;
    private static readonly BadgeLayoutProvider _layoutProvider;

    private SolidColorBrush _incorrectLineBackground = new ( new Color ( 0, 0, 0, 0 ) );
    private SolidColorBrush _incorrectBorderColor = new ( new Color ( 0, 0, 0, 0 ) );
    private SolidColorBrush _correctBorderColor = new ( new Color ( 0, 0, 0, 0 ) );
    private Avalonia.Thickness _incorrectBorderThickness;
    private Avalonia.Thickness _correctBorderThickness;

    [ObservableProperty]
    private Imaging.Bitmap? _imageBitmap;

    [ObservableProperty]
    private double _badgeWidth;

    [ObservableProperty]
    private double _badgeHeight;

    [ObservableProperty]
    private double _borderWidth;

    [ObservableProperty]
    private double _borderHeight;

    [ObservableProperty]
    private Avalonia.Thickness? _margin;

    [ObservableProperty]
    private ObservableCollection<TextLineViewModel> _textLines;

    [ObservableProperty]
    private ObservableCollection<ImageViewModel> _insideImages;

    [ObservableProperty]
    private ObservableCollection<ShapeViewModel> _insideRectangles;

    [ObservableProperty]
    private ObservableCollection<ShapeViewModel> _insideEllipses;

    [ObservableProperty]
    private Avalonia.Thickness? _borderThickness;

    [ObservableProperty]
    private string? _focusedFontSize;

    [ObservableProperty]
    private bool _isChanged;

    private bool _isFocusedRightNow;
    private string? _focusedText;
    internal string? FocusedText
    {
        get => _focusedText;

        private set
        {
            _focusedText = value;

            if ( !_isFocusedRightNow ) 
            {
                ResetFocusedText ( _focusedText );
            }

            _isFocusedRightNow = false;

            if ( _focusedLine != null )
            {
                _focusedLine.Content = _focusedText;
            }

            MarkTextLineCorrectness ();
            OnPropertyChanged ( );
        }
    }

    private TextLineViewModel? _focusedLine;
    internal TextLineViewModel? FocusedLine
    {
        get
        {
            return _focusedLine;
        }

        set
        {
            _focusedLine?.DisFocus ();
            _focusedLine = value;

            if ( _focusedLine != null )
            {
                if ( _focusedLine.IsBorderViolent || _focusedLine.IsOverLayViolent )
                {
                    _focusedLine.Focus ( _incorrectBorderColor, _incorrectBorderThickness );
                }
                else
                {
                    _focusedLine.Focus ( _correctBorderColor, _correctBorderThickness );
                }
            }

            _isFocusedRightNow = true;
            FocusedText = _focusedLine == null || string.IsNullOrWhiteSpace ( _focusedLine.Content ) ? null : _focusedLine.Content;
            OnPropertyChanged ();
        }
    }

    private ImageViewModel? _focusedImage;
    internal ImageViewModel? FocusedImage
    {
        get
        {
            return _focusedImage;
        }

        private set
        {
            if ( _focusedImage != null && value == null )
            {
                _focusedImage.DisFocus ();
                _focusedImage = null;
            }
            else if ( value != null )
            {
                _focusedImage = value;
                _focusedImage.Focus ( _correctBorderColor, _correctBorderThickness );
            }
        }
    }

    private ShapeViewModel? _focusedRect;
    internal ShapeViewModel? FocusedRect
    {
        get
        {
            return _focusedRect;
        }

        private set
        {
            if ( _focusedRect != null && value == null )
            {
                _focusedRect.DisFocus ();
                _focusedRect = null;
            }
            else if ( value != null )
            {
                _focusedRect = value;
                _focusedRect.Focus ( _correctBorderColor, _correctBorderThickness );
            }
        }
    }

    private ShapeViewModel? _focusedEllipse;
    internal ShapeViewModel? FocusedEllipse
    {
        get
        {
            return _focusedEllipse;
        }

        private set
        {
            if ( _focusedEllipse != null && value == null )
            {
                _focusedEllipse.DisFocus ();
                _focusedEllipse = null;
            }
            else if ( value != null )
            {
                _focusedEllipse = value;
                _focusedEllipse.Focus ( _correctBorderColor, _correctBorderThickness );
            }
        }
    }

    private bool _isCorrect;
    internal bool IsCorrect
    {
        get => _isCorrect;

        private set
        {
            _isCorrect = value;
            OnPropertyChanged ();
        }
    }

    internal int Id { get; private set; }
    internal double Scale { get; private set; }
    internal Badge Model { get; private set; }

    internal event Action<bool>? CorrectnessChanged;

    static BadgeViewModel ()
    {
        _pathToImage = [];
        _layoutProvider ??= BadgeLayoutProvider.GetInstance ();
    }

    public BadgeViewModel ( Badge model )
    {
        Id = model.Id;
        Model = model;
        Scale = 1;

        if ( model.Margin != null )
        {
            Margin = new Avalonia.Thickness ( model.Margin.Left, model.Margin.Top, model.Margin.Right, model.Margin.Bottom );
        }

        BadgeWidth = model.Layout.Width;
        BorderWidth = model.Layout.BorderWidth;
        BadgeHeight = model.Layout.Height;
        BorderHeight = model.Layout.BorderHeight;

        SetIncorrectComponentMarking ( model );

        TextLines = [];
        SetTextLines ();

        InsideImages = [];
        InsideRectangles = [];
        InsideEllipses = [];

        IsCorrect = model.IsCorrect;
        IsChanged = model.IsChanged;
        BorderThickness = new Avalonia.Thickness ( 1 );
        FocusedFontSize = string.Empty;

        Dispatcher.UIThread.Invoke ( SetImagesAndShapes );

        Model.RolledBack += HandleModelRolledBack;
        Model.CorrectnessChanged += HandleCorrectnessChanged;

        var fdfdf = ImageBitmap;
    }

    #region Handlers
    private void HandleModelRolledBack ()
    {
        TextLines.Clear ();
        InsideImages.Clear ();
        InsideRectangles.Clear ();
        InsideEllipses.Clear ();

        foreach ( TextLine line in Model.Layout.TextLines )
        {
            TextLines.Add ( new TextLineViewModel ( line ) );
        }

        SetImagesAndShapes ();
        IsChanged = Model.IsChanged;
    }

    private void HandleCorrectnessChanged ()
    {
        IsCorrect = Model.IsCorrect;
        CorrectnessChanged?.Invoke ( IsCorrect );
    }
    #endregion

    #region Settings
    private void SetTextLines ()
    {
        TextLines.Clear ();

        foreach ( TextLine model in Model.Layout.TextLines )
        {
            TextLineViewModel line = new ( model );
            line.ZoomOn ( Scale );

            line.IsBorderViolent = model.IsBoundViolating;
            line.IsOverLayViolent = model.IsOverLaying;

            if ( line.IsBorderViolent || line.IsOverLayViolent )
            {
                line.Background = _incorrectLineBackground;
            }

            TextLines.Add ( line );
        }
    }

    private void SetImagesAndShapes ()
    {
        int imageCount = 0;
        int rectCount = 0;
        int ellipseCount = 0;

        foreach ( ComponentImage image in Model.Layout.Images )
        {
            ImageViewModel newImage = new ( imageCount, image );
            newImage.ZoomOn ( Scale );
            InsideImages.Add ( newImage );
            imageCount++;
        }

        foreach ( ComponentShape shape in Model.Layout.Shapes )
        {
            if ( shape.Type == ShapeType.rectangle )
            {
                ShapeViewModel newRect = new ( rectCount, shape );
                newRect.ZoomOn ( Scale );
                InsideRectangles.Add ( newRect );
                rectCount++;
            }
            else if ( shape.Type == ShapeType.ellipse )
            {
                ShapeViewModel newEllipse = new ( ellipseCount, shape );
                newEllipse.ZoomOn ( Scale );
                InsideEllipses.Add ( newEllipse );
                ellipseCount++;
            }
        }
    }

    internal TextLineViewModel? GetCoincidence ( string focusedContent, int elementNumber )
    {
        TextLineViewModel? coincidence = null;
        int counter = 0;

        foreach ( TextLineViewModel line in TextLines )
        {
            if ( line.Content == focusedContent && elementNumber == counter )
            {
                coincidence = line;

                break;
            }

            counter++;
        }

        return coincidence;
    }

    internal void SetCorrectScale ( double scale )
    {
        if ( scale != 1 )
        {
            ZoomOn ( scale );
        }
    }
    #endregion

    #region Focusing
    internal void SetFocusedLine ( string focusedContent, int elementNumber )
    {
        FocusedLine = GetCoincidence ( focusedContent, elementNumber );

        if ( FocusedLine == null )
        {
            return;
        }

        int visibleFontSize = ( int ) Math.Round ( FocusedLine.FontSize / Scale );
        FocusedFontSize = visibleFontSize.ToString ();
        Model.SetProcessable ( FocusedLine.Model );

        if ( FocusedLine.IsBorderViolent || FocusedLine.IsOverLayViolent )
        {
            MarkIncorrect ( FocusedLine );
        }
    }

    internal void SetFocusedImage ( int imageId )
    {
        foreach ( ImageViewModel image in InsideImages )
        {
            if ( imageId == image.Id )
            {
                FocusedImage = image;
                Model.SetProcessable ( FocusedImage.Model );

                break;
            }
        }
    }

    internal void SetFocusedRectangle ( int rectId )
    {
        foreach ( ShapeViewModel rect in InsideRectangles )
        {
            if ( rectId == rect.Id )
            {
                FocusedRect = rect;
                Model.SetProcessable ( FocusedRect.Model );

                break;
            }
        }
    }

    internal void SetFocusedEllipse ( int ellipseId )
    {
        foreach ( ShapeViewModel ellipse in InsideEllipses )
        {
            if ( ellipseId == ellipse.Id )
            {
                FocusedEllipse = ellipse;
                Model.SetProcessable ( FocusedEllipse.Model );

                break;
            }
        }
    }

    internal void ReleaseFocused ()
    {
        if ( FocusedLine != null )
        {
            MarkTextLineCorrectness ();
            FocusedFontSize = string.Empty;
            FocusedLine = null;
        }
        else if ( FocusedEllipse != null )
        {
            FocusedEllipse = null;
        }
        else if ( FocusedRect != null )
        {
            FocusedRect = null;
        }
        else if ( FocusedImage != null )
        {
            FocusedImage = null;
        }

        Model?.ZeroProcessable ();
    }
    #endregion

    #region ShowAndZoom
    internal void Show ()
    {
        if ( ImageBitmap == null && !string.IsNullOrWhiteSpace ( Model.BackgroundImagePath ) )
        {
            if ( !_pathToImage.TryGetValue ( Model.BackgroundImagePath, out Imaging.Bitmap? _ ) )
            {
                Imaging.Bitmap? value = ImageHelper.LoadFromResource ( Model.BackgroundImagePath );
                _pathToImage.Add ( Model.BackgroundImagePath, value );
            }

            ImageBitmap = _pathToImage [Model.BackgroundImagePath];
        }
    }

    internal void Hide ()
    {
        ImageBitmap = null;
    }

    internal void ZoomOn ( double coefficient )
    {
        BadgeWidth *= coefficient;
        BadgeHeight *= coefficient;

        BorderWidth = BadgeWidth + 2;
        BorderHeight = BadgeHeight + 2;

        Scale *= coefficient;

        foreach ( TextLineViewModel line in TextLines )
        {
            line.ZoomOn ( coefficient );
        }

        foreach ( ImageViewModel image in InsideImages )
        {
            image.ZoomOn ( coefficient );
        }

        foreach ( ShapeViewModel rect in InsideRectangles )
        {
            rect.ZoomOn ( coefficient );
        }

        foreach ( ShapeViewModel ellipse in InsideEllipses )
        {
            ellipse.ZoomOn ( coefficient );
        }
    }

    internal void ZoomOut ( double coefficient )
    {
        BadgeWidth /= coefficient;
        BadgeHeight /= coefficient;

        BorderWidth = BadgeWidth + 2;
        BorderHeight = BadgeHeight + 2;

        Scale /= coefficient;

        foreach ( TextLineViewModel line in TextLines )
        {
            line.ZoomOut ( coefficient );
        }

        foreach ( ImageViewModel image in InsideImages )
        {
            image.ZoomOut ( coefficient );
        }

        foreach ( ShapeViewModel rect in InsideRectangles )
        {
            rect.ZoomOut ( coefficient );
        }

        foreach ( ShapeViewModel ellipse in InsideEllipses )
        {
            ellipse.ZoomOut ( coefficient );
        }
    }
    #endregion

    #region MarkLineCorrectness
    private void SetIncorrectComponentMarking ( Badge model )
    {
        _incorrectLineBackground = GetColor ( _layoutProvider.GetIncorrectLineBackgroundHex ( model.Layout.TemplateName ) );
        _incorrectBorderColor = GetColor ( _layoutProvider.GetIncorrectMemberBorderColor ( model.Layout.TemplateName ) );
        _correctBorderColor = GetColor ( _layoutProvider.GetCorrectMemberBorderColor ( model.Layout.TemplateName ) );
        _incorrectBorderThickness = GetThickness ( _layoutProvider.GetIncorrectMemberBorderThickness ( model.Layout.TemplateName ) );
        _correctBorderThickness = GetThickness ( _layoutProvider.GetCorrectMemberBorderThickness ( model.Layout.TemplateName ) );
    }

    private void MarkTextLineCorrectness ()
    {
        if ( FocusedLine == null ) return;

        if ( FocusedLine.Model.IsBoundViolating || FocusedLine.Model.IsOverLaying )
        {
            MarkIncorrect ( FocusedLine );
        }
        else
        {
            MarkCorrect ( FocusedLine );
        }
    }

    private void MarkIncorrect ( TextLineViewModel processable )
    {
        processable.Background = _incorrectLineBackground;
        processable.Mark ( _incorrectBorderColor, _incorrectBorderThickness );
    }

    private void MarkCorrect ( TextLineViewModel setable )
    {
        setable.Background = null;
        setable.Mark ( _correctBorderColor, _correctBorderThickness );
    }

    private static SolidColorBrush GetColor ( string? hexColor )
    {
        if ( hexColor == null )
        {
            return new ( new Color ( 0, 0, 0, 0 ) );
        }

        if ( hexColor.Length == 7 )
        {
            SolidColorBrush brush;

            if ( !Color.TryParse ( hexColor, out Color color ) )
            {
                return new ( new Color ( 0, 0, 0, 0 ) );
            }

            if ( MainViewModel.HasWaitingState )
            {
                SolidColorBrush result = Dispatcher.UIThread.Invoke (
                    () =>
                    {
                        return new SolidColorBrush ( color );
                    }
                );

                brush = result;
            }
            else
            {
                brush = new ( color );
            }

            return brush;
        }

        return new ( new Color ( 0, 0, 0, 0 ) );
    }

    private static Avalonia.Thickness GetThickness ( List<byte> thickness )
    {
        if ( thickness == null )
        {
            return new ( 0, 0, 0, 0 );
        }

        if ( thickness.Count == 4 )
        {
            if ( MainViewModel.HasWaitingState )
            {
                Avalonia.Thickness result = Dispatcher.UIThread.Invoke (
                    () =>
                    {
                        return new Avalonia.Thickness ( thickness [0], thickness [1], thickness [2], thickness [3] );
                    }
                );

                return result;
            }
            else
            {
                return new ( thickness [0], thickness [1], thickness [2], thickness [3] );
            }
        }

        return new ( 0, 0, 0, 0 );
    }
    #endregion

    #region Processing
    internal void Split ()
    {
        if ( FocusedLine == null )
        {
            return;
        }

        Model.Split ( FocusedLine.Model );
        IsChanged = Model.IsChanged;
        FocusedFontSize = string.Empty;
        FocusedLine = null;
        SetTextLines ();
    }

    internal void MoveCaptured ( Avalonia.Point delta )
    {
        Model.MoveProcessable ( delta.Y / Scale, delta.X / Scale );
        IsChanged = Model.IsChanged;
        MarkTextLineCorrectness ();
    }

    internal void FocusedToSide ( string direction )
    {
        if ( FocusedLine == null && FocusedImage == null && FocusedRect == null && FocusedEllipse == null )
        {
            return;
        }

        Model.ShiftProcessable ( direction );
        IsChanged = Model.IsChanged;
        MarkTextLineCorrectness ();
    }

    internal void ResetFocusedText ( string? newText )
    {
        if ( FocusedLine == null )
        {
            return;
        }

        Model.ResetProcessableContent ( newText );
        IsChanged = Model.IsChanged;
    }

    internal void IncreaseFontSize ()
    {
        ChangeFontSize ( false );
    }

    internal void ReduceFontSize ()
    {
        ChangeFontSize ( true );
    }

    private void ChangeFontSize ( bool toReduce )
    {
        if ( FocusedLine == null )
        {
            return;
        }

        if ( toReduce )
        {
            Model.ReduceFontSize ();
        }
        else
        {
            Model.IncreaseFontSize ();
        }

        IsChanged = true;
        int fontSize = ( int ) Math.Round ( FocusedLine.FontSize / Scale );
        FocusedFontSize = fontSize.ToString ();
        MarkTextLineCorrectness ();
    }

    internal void CancelChanges ()
    {
        Model.CancelChanges ();
        SetTextLines ();
        _focusedText = null;
        IsChanged = false;
    }
    #endregion
}
