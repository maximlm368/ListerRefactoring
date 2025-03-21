﻿using Avalonia.Media;
using Avalonia.Threading;
using Lister.Core.Models.Badge;
using Lister.Desktop.CoreAbstractionsImplimentations.BadgeCreator;
using Lister.Desktop.Extentions;
using ReactiveUI;
using System.Collections.ObjectModel;
using View.MainWindow.MainView.ViewModel;

namespace Lister.Desktop.CoreModelReflections.BadgeVM;

public class BadgeViewModel : ReactiveObject
{
    private static Dictionary<string, Avalonia.Media.Imaging.Bitmap> _pathToImage;
    private static BadgeLayoutProvider _layoutProvider;

    private readonly string _semiProtectedTypeName = "Lister.ViewModels.BadgeEditorViewModel";
    private readonly double _interLineAddition = 1;
    private SolidColorBrush _incorrectLineBackground;
    private SolidColorBrush _incorrectMemberBorderColor;
    private SolidColorBrush _correctMemberBorderColor;
    private Avalonia.Thickness _incorrectMemberBorderThickness;
    private Avalonia.Thickness _correctMemberBorderThickness;
    private SolidColorBrush _normMemberBorderColor;

    internal int Id { get; private set; }
    internal double Scale { get; private set; }
    internal Badge Model { get; private set; }

    private Avalonia.Media.Imaging.Bitmap _imageBitmap;
    internal Avalonia.Media.Imaging.Bitmap ImageBitmap
    {
        get { return _imageBitmap; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _imageBitmap, value, nameof( ImageBitmap ) );
        }

    }

    private double _badgeWidth;
    internal double BadgeWidth
    {
        get { return _badgeWidth; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _badgeWidth, value, nameof( BadgeWidth ) );
        }
    }

    private double _badgeHeight;
    internal double BadgeHeight
    {
        get { return _badgeHeight; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _badgeHeight, value, nameof( BadgeHeight ) );
        }
    }

    private double _borderWidth;
    internal double BorderWidth
    {
        get { return _borderWidth; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _borderWidth, value, nameof( BorderWidth ) );
        }
    }

    private double _borderHeight;
    internal double BorderHeight
    {
        get { return _borderHeight; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _borderHeight, value, nameof( BorderHeight ) );
        }
    }

    private Avalonia.Thickness _margin;
    internal Avalonia.Thickness Margin
    {
        get { return _margin; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _margin, value, nameof( Margin ) );
        }
    }

    private TextLineViewModel _focusedLine;
    internal TextLineViewModel FocusedLine
    {
        get { return _focusedLine; }
        private set
        {
            if (value == null && _focusedLine != null)
            {
                _focusedLine.BecomeUnFocused();
                _focusedLine = value;
            }
            else if (value != null)
            {
                _focusedLine = value;

                if (_focusedLine.IsBorderViolent || _focusedLine.IsOverLayViolent)
                {
                    _focusedLine.BecomeFocused( _incorrectMemberBorderColor, _incorrectMemberBorderThickness );
                }
                else
                {
                    _focusedLine.BecomeFocused( _correctMemberBorderColor, _correctMemberBorderThickness );
                }
            }

            if (_focusedLine == null || string.IsNullOrWhiteSpace( _focusedLine.Content ))
            {
                FocusedText = string.Empty;
            }
            else
            {
                FocusedText = _focusedLine.Content;
            }
        }
    }

    private string _focusedText;
    internal string FocusedText
    {
        get { return _focusedText; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _focusedText, value, nameof( FocusedText ) );
        }
    }

    private ObservableCollection<TextLineViewModel> _textLines;
    internal ObservableCollection<TextLineViewModel> TextLines
    {
        get { return _textLines; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _textLines, value, nameof( TextLines ) );
        }
    }

    private ImageViewModel _focusedImage;
    internal ImageViewModel FocusedImage
    {
        get { return _focusedImage; }
        private set
        {
            if (_focusedImage != null && value == null)
            {
                _focusedImage.BecomeUnFocused();
                _focusedImage = null;
            }
            else if (value != null)
            {
                _focusedImage = value;
                _focusedImage.BecomeFocused( _correctMemberBorderColor, _correctMemberBorderThickness );
            }
        }
    }
    private ObservableCollection<ImageViewModel> _insideImages;
    internal ObservableCollection<ImageViewModel> InsideImages
    {
        get { return _insideImages; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _insideImages, value, nameof( InsideImages ) );
        }
    }

    private ShapeViewModel _focusedRect;
    internal ShapeViewModel FocusedRect
    {
        get { return _focusedRect; }
        private set
        {
            if (_focusedRect != null && value == null)
            {
                _focusedRect.BecomeUnFocused();
                _focusedRect = null;
            }
            else if (value != null)
            {
                _focusedRect = value;
                _focusedRect.BecomeFocused( _correctMemberBorderColor, _correctMemberBorderThickness );
            }
        }
    }
    private ObservableCollection<ShapeViewModel> _insideRectangles;
    internal ObservableCollection<ShapeViewModel> InsideRectangles
    {
        get { return _insideRectangles; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _insideRectangles, value, nameof( InsideRectangles ) );
        }
    }

    private ShapeViewModel _focusedEllipse;
    internal ShapeViewModel FocusedEllipse
    {
        get { return _focusedEllipse; }
        private set
        {
            if (_focusedEllipse != null && value == null)
            {
                _focusedEllipse.BecomeUnFocused();
                _focusedEllipse = null;
            }
            else if (value != null)
            {
                _focusedEllipse = value;
                _focusedEllipse.BecomeFocused( _correctMemberBorderColor, _correctMemberBorderThickness );
            }
        }
    }
    private ObservableCollection<ShapeViewModel> _insideEllipses;
    internal ObservableCollection<ShapeViewModel> InsideEllipses
    {
        get { return _insideEllipses; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _insideEllipses, value, nameof( InsideEllipses ) );
        }
    }

    private double _borderThickness;
    private Avalonia.Thickness _thicknessOfBorder;
    internal Avalonia.Thickness BorderThickness
    {
        get { return _thicknessOfBorder; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _thicknessOfBorder, value, nameof( BorderThickness ) );
        }
    }

    private string _focusedFontSize;
    internal string FocusedFontSize
    {
        get { return _focusedFontSize; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _focusedFontSize, value, nameof( FocusedFontSize ) );
        }
    }

    internal bool IsCorrect { get; private set; }

    private bool _isChanged;
    internal bool IsChanged
    {
        get { return _isChanged; }
        set
        {
            this.RaiseAndSetIfChanged( ref _isChanged, value, nameof( IsChanged ) );
        }
    }


    static BadgeViewModel()
    {
        _pathToImage = new Dictionary<string, Avalonia.Media.Imaging.Bitmap>();
    }


    public BadgeViewModel(Badge model)
    {
        if (_layoutProvider == null)
        {
            _layoutProvider = BadgeLayoutProvider.GetInstance();
        }

        SetUp( model );
        Model.RolledBack += HandleModelRolledBack;
        Model.CorrectnessChanged += HandleCorrectnessChanged;
    }

    #region Handlers

    private void HandleModelRolledBack()
    {
        TextLines = new();
        InsideImages = new();
        InsideRectangles = new();
        InsideEllipses = new();

        foreach (TextLine line in Model.Layout.TextLines)
        {
            TextLines.Add( new TextLineViewModel( line ) );
        }

        SetImagesAndShapes();
        IsChanged = Model.IsChanged;
    }


    private void HandleCorrectnessChanged()
    {
        IsCorrect = Model.IsCorrect;
    }
    #endregion

    #region Settings

    private void SetTextLines()
    {
        TextLines = new();

        foreach (TextLine model in Model.Layout.TextLines)
        {
            TextLineViewModel line = new TextLineViewModel( model );
            line.ZoomOn( Scale );

            line.IsBorderViolent = model.IsPaddingViolent;
            line.IsOverLayViolent = model.IsOverLayViolent;

            if (line.IsBorderViolent || line.IsOverLayViolent)
            {
                line.Background = _incorrectLineBackground;
            }

            TextLines.Add( line );
        }
    }


    private void SetUp(Badge model)
    {
        Id = model.Id;
        Model = model;
        Layout layout = model.Layout;
        Scale = 1;

        Margin = new Avalonia.Thickness( model.Margin.Left, model.Margin.Top, model.Margin.Right, model.Margin.Bottom );

        BadgeWidth = layout.Width;
        BorderWidth = layout.BorderWidth;
        BadgeHeight = layout.Height;
        BorderHeight = layout.BorderHeight;

        SetIncorrectComponentMarking( model );
        SetTextLines();

        InsideImages = new ObservableCollection<ImageViewModel>();
        InsideRectangles = new ObservableCollection<ShapeViewModel>();
        InsideEllipses = new ObservableCollection<ShapeViewModel>();

        IsCorrect = model.IsCorrect;
        IsChanged = model.IsChanged;
        _borderThickness = 1;
        BorderThickness = new Avalonia.Thickness( _borderThickness );
        FocusedFontSize = string.Empty;

        Dispatcher.UIThread.Invoke
        ( () =>
        {
            SetImagesAndShapes();
        } );
    }


    private void SetImagesAndShapes()
    {
        int imageCount = 0;
        int rectCount = 0;
        int ellipseCount = 0;

        foreach (ComponentImage image in Model.Layout.Images)
        {
            ImageViewModel newImage = new ImageViewModel( imageCount, image );
            newImage.ZoomOn( Scale );
            InsideImages.Add( newImage );
            imageCount++;
        }

        foreach (ComponentShape shape in Model.Layout.Shapes)
        {
            if (shape.Type == ShapeType.rectangle)
            {
                ShapeViewModel newRect = new ShapeViewModel( rectCount, shape );
                newRect.ZoomOn( Scale );
                InsideRectangles.Add( newRect );
                rectCount++;
            }
            else if (shape.Type == ShapeType.ellipse)
            {
                ShapeViewModel newEllipse = new ShapeViewModel( ellipseCount, shape );
                newEllipse.ZoomOn( Scale );
                InsideEllipses.Add( newEllipse );
                ellipseCount++;
            }
        }
    }


    internal TextLineViewModel? GetCoincidence(string focusedContent, int elementNumber)
    {
        string lineContent = string.Empty;
        TextLineViewModel goalLine = null;
        int counter = 0;

        foreach (TextLineViewModel line in TextLines)
        {
            lineContent = line.Content;

            if (lineContent == focusedContent && elementNumber == counter)
            {
                goalLine = line;
                break;
            }

            counter++;
        }

        return goalLine;
    }


    internal void SetCorrectScale(double scale)
    {
        if (scale != 1)
        {
            ZoomOn( scale );
        }
    }
    #endregion

    #region Focusing

    internal void SetFocusedLine(string focusedContent, int elementNumber)
    {
        FocusedLine = GetCoincidence( focusedContent, elementNumber );

        if (FocusedLine == null)
        {
            return;
        }

        int visibleFontSize = (int)Math.Round( FocusedLine.FontSize / Scale );
        FocusedFontSize = visibleFontSize.ToString();
        Model.SetProcessable( FocusedLine.Model );

        if (FocusedLine.IsBorderViolent || FocusedLine.IsOverLayViolent)
        {
            MarkAsIncorrect( FocusedLine );
        }
    }


    internal void SetFocusedImage(int id)
    {
        foreach (ImageViewModel image in InsideImages)
        {
            if (id == image.Id)
            {
                FocusedImage = image;
                break;
            }
        }

        Model.SetProcessable( FocusedImage.Model );
    }


    internal void SetFocusedRectangle(int rectId)
    {
        foreach (ShapeViewModel rect in InsideRectangles)
        {
            if (rectId == rect.Id)
            {
                FocusedRect = rect;
                break;
            }
        }

        Model.SetProcessable( FocusedRect.Model );
    }


    internal void SetFocusedEllipse(int ellipseId)
    {
        foreach (ShapeViewModel ellipse in InsideEllipses)
        {
            if (ellipseId == ellipse.Id)
            {
                FocusedEllipse = ellipse;
                break;
            }
        }

        Model.SetProcessable( FocusedEllipse.Model );
    }


    internal void ReleaseFocused()
    {
        if (FocusedLine != null)
        {
            MarkTextLine();
            FocusedFontSize = string.Empty;
            FocusedLine = null;
        }
        else if (FocusedEllipse != null)
        {
            FocusedEllipse = null;
        }
        else if (FocusedRect != null)
        {
            FocusedRect = null;
        }
        else if (FocusedImage != null)
        {
            FocusedImage = null;
        }

        Model?.ZeroProcessable();
    }
    #endregion

    #region ShowZoom

    internal void Show()
    {
        string path = Model.BackgroundImagePath;

        if (!_pathToImage.ContainsKey( path ))
        {
            _pathToImage.Add( path, ImageHelper.LoadFromResource( path ) );
        }

        ImageBitmap = _pathToImage[path];
    }


    internal void Hide()
    {
        ImageBitmap = null;
    }


    internal void ZoomOn(double coefficient)
    {
        BadgeWidth *= coefficient;
        BadgeHeight *= coefficient;

        BorderWidth = BadgeWidth + 2;
        BorderHeight = BadgeHeight + 2;

        Scale *= coefficient;

        foreach (TextLineViewModel line in TextLines)
        {
            line.ZoomOn( coefficient );
        }

        foreach (ImageViewModel image in InsideImages)
        {
            image.ZoomOn( coefficient );
        }

        foreach (ShapeViewModel rect in InsideRectangles)
        {
            rect.ZoomOn( coefficient );
        }

        foreach (ShapeViewModel ellipse in InsideEllipses)
        {
            ellipse.ZoomOn( coefficient );
        }
    }


    internal void ZoomOut(double coefficient)
    {
        BadgeWidth /= coefficient;
        BadgeHeight /= coefficient;

        BorderWidth = BadgeWidth + 2;
        BorderHeight = BadgeHeight + 2;

        Scale /= coefficient;

        foreach (TextLineViewModel line in TextLines)
        {
            line.ZoomOut( coefficient );
        }

        foreach (ImageViewModel image in InsideImages)
        {
            image.ZoomOut( coefficient );
        }

        foreach (ShapeViewModel rect in InsideRectangles)
        {
            rect.ZoomOut( coefficient );
        }

        foreach (ShapeViewModel ellipse in InsideEllipses)
        {
            ellipse.ZoomOut( coefficient );
        }
    }
    #endregion

    #region MarkLineCorrectness

    private void SetIncorrectComponentMarking(Badge model)
    {
        string incorrectLineBackgroundHexStr = _layoutProvider.GetIncorrectLineBackgroundStr( model.Layout.TemplateName );
        string incorrectMemberBorderHexStr = _layoutProvider.GetIncorrectMemberBorderStr( model.Layout.TemplateName );
        string correctMemberBorderHexStr = _layoutProvider.GetCorrectMemberBorderStr( model.Layout.TemplateName );
        List<byte> incorrectMemberBorderThickness = _layoutProvider.GetIncorrectMemberBorderThickness( model.Layout.TemplateName );
        List<byte> correctMemberBorderThickness = _layoutProvider.GetCorrectMemberBorderThickness( model.Layout.TemplateName );

        _incorrectLineBackground = GetColor( incorrectLineBackgroundHexStr );
        _incorrectMemberBorderColor = GetColor( incorrectMemberBorderHexStr );
        _correctMemberBorderColor = GetColor( correctMemberBorderHexStr );
        _incorrectMemberBorderThickness = GetThickness( incorrectMemberBorderThickness );
        _correctMemberBorderThickness = GetThickness( correctMemberBorderThickness );
    }


    private void MarkTextLine()
    {
        if (FocusedLine == null) return;

        if (FocusedLine.Model.IsPaddingViolent || FocusedLine.Model.IsOverLayViolent)
        {
            MarkAsIncorrect( FocusedLine );
        }
        else
        {
            MarkAsCorrect( FocusedLine );
        }
    }


    private void MarkAsIncorrect(TextLineViewModel setable)
    {
        setable.Background = _incorrectLineBackground;
        setable.Mark( _incorrectMemberBorderColor, _incorrectMemberBorderThickness );
    }


    private void MarkAsCorrect(TextLineViewModel setable)
    {
        setable.Background = null;
        setable.Mark( _correctMemberBorderColor, _correctMemberBorderThickness );
    }


    private SolidColorBrush? GetColor(List<byte> rgb)
    {
        if (rgb == null)
        {
            return null;
        }
        else
        {
            if (rgb.Count == 3)
            {
                byte r = rgb[0];
                byte g = rgb[1];
                byte b = rgb[2];

                SolidColorBrush brush;
                Color color = new Color( 255, r, g, b );

                if (MainViewModel.MainViewIsWaiting)
                {
                    var result = Dispatcher.UIThread.Invoke
                    ( () =>
                    {
                        return new SolidColorBrush( color );
                    } );

                    brush = result;
                }
                else
                {
                    brush = new SolidColorBrush( color );
                }

                return brush;
            }
            else
            {
                return null;
            }
        }
    }


    private SolidColorBrush? GetColor(string hexColor)
    {
        if (hexColor == null)
        {
            return null;
        }
        else
        {
            if (hexColor.Length == 7)
            {
                SolidColorBrush brush;
                Color color;

                if (!Color.TryParse( hexColor, out color ))
                {
                    return null;
                }

                if (MainViewModel.MainViewIsWaiting)
                {
                    var result = Dispatcher.UIThread.Invoke
                    ( () =>
                    {
                        return new SolidColorBrush( color );
                    } );

                    brush = result;
                }
                else
                {
                    brush = new SolidColorBrush( color );
                }

                return brush;
            }
            else
            {
                return null;
            }
        }
    }


    private Avalonia.Thickness GetThickness(List<byte> ltrb)
    {
        if (ltrb == null)
        {
            return new Avalonia.Thickness( 0, 0, 0, 0 );
        }
        else
        {
            if (ltrb.Count == 4)
            {
                double left = ltrb[0];
                double top = ltrb[1];
                double right = ltrb[2];
                double bottom = ltrb[3];

                Avalonia.Thickness thickness;

                if (MainViewModel.MainViewIsWaiting)
                {
                    var result = Dispatcher.UIThread.Invoke
                    ( () =>
                    {
                        return new Avalonia.Thickness( left, top, right, bottom );
                    } );

                    thickness = result;
                }
                else
                {
                    thickness = new Avalonia.Thickness( left, top, right, bottom );
                }

                return thickness;
            }
            else
            {
                return new Avalonia.Thickness( 0, 0, 0, 0 );
            }
        }
    }

    #endregion

    #region Processing

    internal void Split(double scale)
    {
        if (FocusedLine == null)
        {
            return;
        }

        Model.Split( FocusedLine.Model );
        IsChanged = Model.IsChanged;
        FocusedFontSize = string.Empty;
        FocusedLine = null;
        SetTextLines();
    }


    internal void MoveCaptured(Avalonia.Point delta)
    {
        Model?.MoveProcessable( delta.Y / Scale, delta.X / Scale );
        IsChanged = Model.IsChanged;
        MarkTextLine();
    }


    internal void FocusedToSide(string direction)
    {
        if (FocusedLine == null && FocusedImage == null && FocusedRect == null && FocusedEllipse == null) return;

        Model?.ShiftProcessable( direction );
        IsChanged = Model.IsChanged;
        MarkTextLine();
    }


    internal void ResetFocusedText(string newText)
    {
        if (FocusedLine == null) return;
        if (newText == FocusedLine.StartContent) return;

        Model.ResetProcessableContent( newText );
        IsChanged = Model.IsChanged;
    }


    internal void IncreaseFontSize()
    {
        ChangeFontSize( false );
    }


    internal void ReduceFontSize()
    {
        ChangeFontSize( true );
    }


    private void ChangeFontSize(bool toReduce)
    {
        if (FocusedLine == null)
        {
            return;
        }

        if (toReduce)
        {
            Model.ReduceFontSize();
        }
        else
        {
            Model.IncreaseFontSize();
        }

        IsChanged = true;
        int visibleFontSize = (int)Math.Round( FocusedLine.FontSize / Scale );
        FocusedFontSize = visibleFontSize.ToString();
        MarkTextLine();
    }


    internal void CancelChanges()
    {
        Model.CancelChanges();
        SetTextLines();
    }

    #endregion
}