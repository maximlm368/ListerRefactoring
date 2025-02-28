using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using Core.Models.Badge;
using Core.DataAccess;
using ExtentionsAndAuxiliary;
using Lister.Extentions;
using ReactiveUI;
using System.Collections.ObjectModel;
using static System.Net.Mime.MediaTypeNames;
using Avalonia.Controls;


namespace Lister.ViewModels;


public class BadgeViewModel : ReactiveObject
{
    private static Dictionary<string, Avalonia.Media.Imaging.Bitmap> _pathToImage;

    private readonly string _semiProtectedTypeName = "Lister.ViewModels.BadgeEditorViewModel";
    private readonly double _interLineAddition = 1;
    private SolidColorBrush _incorrectLineBackground;
    private SolidColorBrush _incorrectMemberBorderColor;
    private SolidColorBrush _correctMemberBorderColor;
    private Avalonia.Thickness _incorrectMemberBorderThickness;
    private Avalonia.Thickness _correctMemberBorderThickness;
    private SolidColorBrush _normMemberBorderColor;

    internal int Id { get; set; }
    internal double Scale { get; private set; }
    internal double PaddingLeft { get; private set; }
    internal double PaddingTop { get; private set; }
    internal double PaddingRight { get; private set; }
    internal double PaddingBottom { get; private set; }
    internal Badge Model { get; private set; }

    private Avalonia.Media.Imaging.Bitmap _imageBitmap;
    internal Avalonia.Media.Imaging.Bitmap ImageBitmap
    {
        get { return _imageBitmap; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _imageBitmap, value, nameof (ImageBitmap));
        }

    }

    private double _widht;
    private double _height;

    private double _badgeWidth;
    internal double BadgeWidth
    {
        get { return _badgeWidth; }
        set
        {
            this.RaiseAndSetIfChanged (ref _badgeWidth, value, nameof (BadgeWidth));
        }
    }

    private double _badgeHeight;
    internal double BadgeHeight
    {
        get { return _badgeHeight; }
        set
        {
            this.RaiseAndSetIfChanged (ref _badgeHeight, value, nameof (BadgeHeight));
        }
    }

    private double _borderWidth;
    internal double BorderWidth
    {
        get { return _borderWidth; }
        set
        {
            this.RaiseAndSetIfChanged (ref _borderWidth, value, nameof (BorderWidth));
        }
    }

    private double _borderHeight;
    internal double BorderHeight
    {
        get { return _borderHeight; }
        set
        {
            this.RaiseAndSetIfChanged (ref _borderHeight, value, nameof (BorderHeight));
        }
    }

    private Avalonia.Thickness _margin;
    internal Avalonia.Thickness Margin
    {
        get { return _margin; }
        set
        {
            this.RaiseAndSetIfChanged (ref _margin, value, nameof (Margin));
        }
    }

    private TextLineViewModel _focusedLine;
    internal TextLineViewModel FocusedLine 
    {
        get { return _focusedLine; }
        private set 
        {
            if (( value == null ) && ( _focusedLine != null ))
            {
                _focusedLine.BecomeUnFocused ();
                _focusedLine = value;
            }
            else if ( value != null ) 
            {
                _focusedLine = value;

                if ( _focusedLine.isBorderViolent   ||   _focusedLine.isOverLayViolent )
                {
                    _focusedLine.BecomeFocused (_incorrectMemberBorderColor, _incorrectMemberBorderThickness);
                }
                else 
                {
                    _focusedLine.BecomeFocused (_correctMemberBorderColor, _correctMemberBorderThickness);
                }
            }

            if ((_focusedLine == null)   ||   string.IsNullOrWhiteSpace(_focusedLine.Content))
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
        set
        {
            this.RaiseAndSetIfChanged (ref _focusedText, value, nameof (FocusedText));
        }
    }

    private List <TextLineViewModel> BorderViolentLines { get; set; }
    private List <TextLineViewModel> OverlayViolentLines { get; set; }
    private ObservableCollection <TextLineViewModel> _textLines;
    internal ObservableCollection <TextLineViewModel> TextLines
    {
        get { return _textLines; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _textLines, value, nameof (TextLines));
        }
    }

    private ImageViewModel _focusedImage;
    internal ImageViewModel FocusedImage 
    {
        get { return _focusedImage; }
        private set 
        {
            if ( ( _focusedImage != null ) && ( value == null ) )
            {
                _focusedImage.BecomeUnFocused ();
                _focusedImage = null;
            }
            else if ( value != null )
            {
                _focusedImage = value;
                _focusedImage.BecomeFocused (_correctMemberBorderColor, _correctMemberBorderThickness);
            }
        }
    }
    private ObservableCollection <ImageViewModel> _insideImages;
    internal ObservableCollection <ImageViewModel> InsideImages
    {
        get { return _insideImages; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _insideImages, value, nameof (InsideImages));
        }
    }

    private ShapeViewModel _focusedRect;
    internal ShapeViewModel FocusedRect 
    {
        get { return _focusedRect; }
        private set
        {
            if ( ( _focusedRect != null ) && ( value == null ) )
            {
                _focusedRect.BecomeUnFocused ();
                _focusedRect = null;
            }
            else if ( value != null )
            {
                _focusedRect = value;
                _focusedRect.BecomeFocused (_correctMemberBorderColor, _correctMemberBorderThickness);
            }
        }
    }
    private ObservableCollection <ShapeViewModel> _insideRectangles;
    internal ObservableCollection <ShapeViewModel> InsideRectangles
    {
        get { return _insideRectangles; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _insideRectangles, value, nameof (InsideRectangles));
        }
    }

    private ShapeViewModel _focusedEllipse;
    internal ShapeViewModel FocusedEllipse 
    {
        get { return _focusedEllipse; }
        private set
        {
            if ( (_focusedEllipse != null)   &&   (value == null) ) 
            {
                _focusedEllipse.BecomeUnFocused ();
                _focusedEllipse = null;
            }
            else if ( value != null )
            {
                _focusedEllipse = value;
                _focusedEllipse.BecomeFocused (_correctMemberBorderColor, _correctMemberBorderThickness);
            }
        }
    }
    private ObservableCollection <ShapeViewModel> _insideEllipses;
    internal ObservableCollection <ShapeViewModel> InsideEllipses
    {
        get { return _insideEllipses; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _insideEllipses, value, nameof (InsideEllipses));
        }
    }

    private double _borderThickness;
    private Avalonia.Thickness _thicknessOfBorder;
    internal Avalonia.Thickness BorderThickness
    {
        get { return _thicknessOfBorder; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _thicknessOfBorder, value, nameof (BorderThickness));
        }
    }

    private string _focusedFontSize;
    internal string FocusedFontSize
    {
        get { return _focusedFontSize; }
        set
        {
            this.RaiseAndSetIfChanged (ref _focusedFontSize, value, nameof (FocusedFontSize));
        }
    }

    internal bool IsCorrect { get; private set; }

    private bool _isChanged;
    internal bool IsChanged
    {
        get { return _isChanged; }
        set
        {
            this.RaiseAndSetIfChanged (ref _isChanged, value, nameof (IsChanged));
        }
    }


    static BadgeViewModel ( )
    {
        _pathToImage = new Dictionary<string , Avalonia.Media.Imaging.Bitmap> ( );
    }


    public BadgeViewModel ( Badge model )
    {
        SetUp ( model );
    }


    private BadgeViewModel ( BadgeViewModel badge )
    {
        Id = badge.Id;

        Model = badge.Model;
        Layout layout = Model.Layout;

        PaddingLeft = badge.PaddingLeft;
        PaddingTop = badge.PaddingTop;
        PaddingRight = badge.PaddingRight;
        PaddingBottom = badge.PaddingBottom;

        BadgeWidth = badge.BadgeWidth;
        BorderWidth = BadgeWidth + 2;
        BadgeHeight = badge.BadgeHeight;
        BorderHeight = BadgeHeight + 2;
        TextLines = new ObservableCollection <TextLineViewModel> ();
        BorderViolentLines = new List <TextLineViewModel> ();
        OverlayViolentLines = new List <TextLineViewModel> ();
        InsideImages = new ObservableCollection <ImageViewModel> ();
        InsideRectangles = new ObservableCollection <ShapeViewModel> ();
        InsideEllipses = new ObservableCollection <ShapeViewModel> ();
        IsCorrect = badge.IsCorrect;
        IsChanged = badge.IsChanged;
        Scale = badge.Scale;
        _borderThickness = 1;
        BorderThickness = new Avalonia.Thickness (_borderThickness);
        FocusedFontSize = string.Empty;

        _incorrectLineBackground = badge._incorrectLineBackground;
        _incorrectMemberBorderColor = badge._incorrectMemberBorderColor;
        _correctMemberBorderColor = badge._correctMemberBorderColor;
        _incorrectMemberBorderThickness = badge._incorrectMemberBorderThickness;
        _correctMemberBorderThickness = badge._correctMemberBorderThickness;

        foreach ( TextLineViewModel line   in   badge.TextLines )
        {
            TextLineViewModel clone = line.Clone ();
            TextLines.Add (clone);

            if ( line.isBorderViolent )
            {
                clone.isBorderViolent = true;
                BorderViolentLines.Add (clone);
            }

            if ( line.isOverLayViolent )
            {
                clone.isOverLayViolent = true;
                OverlayViolentLines.Add (clone);
            }
        }

        foreach ( ImageViewModel prototype   in   badge.InsideImages )
        {
            ImageViewModel image = prototype.Clone ();
            InsideImages.Add (image);
        }

        foreach ( ShapeViewModel prototype   in   badge.InsideRectangles )
        {
            ShapeViewModel rect = prototype.Clone ();
            InsideRectangles.Add (rect);
        }

        foreach ( ShapeViewModel prototype   in   badge.InsideEllipses )
        {
            ShapeViewModel ellipse = prototype.Clone ();
            InsideEllipses.Add (ellipse);
        }

        Scale = badge.Scale;
    }


    private void SetTextLines ()
    {
        TextLines = new ();

        foreach ( TextLine text   in   Model.Layout.TextLines )
        {
            TextLineViewModel line = new TextLineViewModel ( text );
            line.ZoomOn ( Scale );

            if ( text.IsBorderViolent   ||   text.IsOverLayViolent )
            {
                line.Background = _incorrectLineBackground;
            }

            TextLines.Add ( line );
        }

        IsChanged = Model.IsChanged;
    }


    internal void CancelChanges ()
    {
        Model.CancelChanges ();
        double scale = Scale;
        SetUp ( Model );

        if ( scale != Scale ) 
        {
            SetCorrectScale ( scale );
        }
    }


    private void SetUp ( Badge model )
    {
        Id = model.Id;
        Model = model;
        Layout layout = model.Layout;
        Scale = 1;

        PaddingLeft = layout.PaddingLeft;
        PaddingTop = layout.PaddingTop;
        PaddingRight = layout.PaddignRight;
        PaddingBottom = layout.PaddingBottom;

        BadgeWidth = layout.Width;
        BorderWidth = BadgeWidth + 2;
        _widht = BadgeWidth;
        BadgeHeight = layout.Height;
        BorderHeight = BadgeHeight + 2;
        _height = BadgeHeight;

        SetIncorrectComponentMarking ( model );
        SetTextLines ();

        BorderViolentLines = new List<TextLineViewModel> ();
        OverlayViolentLines = new List<TextLineViewModel> ();

        InsideImages = new ObservableCollection<ImageViewModel> ();
        InsideRectangles = new ObservableCollection<ShapeViewModel> ();
        InsideEllipses = new ObservableCollection<ShapeViewModel> ();

        IsCorrect = model.IsCorrect;
        _borderThickness = 1;
        BorderThickness = new Avalonia.Thickness ( _borderThickness );
        FocusedFontSize = string.Empty;

        Dispatcher.UIThread.Invoke
        ( () =>
        {
            List<InsideImage> images = layout.InsideImages;
            SetUpImages ( images );

            List<InsideShape> shapes = layout.InsideShapes;
            SetUpShapes ( shapes );
        } );
    }


    private void SetIncorrectComponentMarking (Badge model)
    {
        string incorrectLineBackgroundHexStr =
                 BadgeAppearence.GetIncorrectLineBackgroundStr ( model.Layout.TemplateName );
        string incorrectMemberBorderHexStr =
                         BadgeAppearence.GetIncorrectMemberBorderStr ( model.Layout.TemplateName );
        string correctMemberBorderHexStr =
                         BadgeAppearence.GetCorrectMemberBorderStr ( model.Layout.TemplateName );
        List<byte> incorrectMemberBorderThickness =
                         BadgeAppearence.GetIncorrectMemberBorderThickness ( model.Layout.TemplateName );
        List<byte> correctMemberBorderThickness =
                         BadgeAppearence.GetCorrectMemberBorderThickness ( model.Layout.TemplateName );


        _incorrectLineBackground = GetColor ( incorrectLineBackgroundHexStr );
        _incorrectMemberBorderColor = GetColor ( incorrectMemberBorderHexStr );
        _correctMemberBorderColor = GetColor ( correctMemberBorderHexStr );
        _incorrectMemberBorderThickness = GetThickness ( incorrectMemberBorderThickness );
        _correctMemberBorderThickness = GetThickness ( correctMemberBorderThickness );
    }


    internal TextLineViewModel ? GetCoincidence ( string focusedContent, int elementNumber )
    {
        string lineContent = string.Empty;
        TextLineViewModel goalLine = null;
        int counter = 0;

        foreach ( TextLineViewModel line   in   TextLines )
        {
            lineContent = line.Content;

            if ( (lineContent == focusedContent)   &&   (elementNumber == counter) )
            {
                goalLine = line;
                break;
            }

            counter++;
        }

        return goalLine;
    }


    internal void SetFocusedLine ( string focusedContent, int elementNumber )
    {
        FocusedLine = GetCoincidence (focusedContent, elementNumber);

        if ( FocusedLine == null )
        {
            return;
        }

        int visibleFontSize = ( int ) Math.Round ( FocusedLine.FontSize / Scale );
        FocusedFontSize = visibleFontSize.ToString ();
        Model.PrepareBackup (FocusedLine.Model);
    }


    internal void SetFocusedImage ( int id )
    {
        foreach ( ImageViewModel image   in   InsideImages )
        {
            if ( id == image.Id )
            {
                FocusedImage = image;
                break;
            }
        }

        Model.PrepareBackup (FocusedImage.Model);
    }


    internal void SetFocusedRectangle ( int rectId )
    {
        foreach ( ShapeViewModel rect   in   InsideRectangles )
        {
            if ( rectId == rect.Id )
            {
                FocusedRect = rect;
                break;
            }
        }

        Model.PrepareBackup (FocusedRect.Model);
    }


    internal void SetFocusedEllipse ( int ellipseId )
    {
        foreach ( ShapeViewModel ellipse   in   InsideEllipses )
        {
            if ( ellipseId == ellipse.Id )
            {
                FocusedEllipse = ellipse;
                break;
            }
        }

        Model.PrepareBackup (FocusedEllipse.Model);
    }


    internal void Show ()
    {
        string path = Model.BackgroundImagePath;

        if ( !_pathToImage.ContainsKey (path) )
        {
            _pathToImage.Add (path, ImageHelper.LoadFromResource (path));
        }

        this.ImageBitmap = _pathToImage[path];
    }


    internal void Hide ()
    {
        this.ImageBitmap = null;
    }


    internal void ZoomOn ( double coefficient )
    {
        BadgeWidth *= coefficient;
        BadgeHeight *= coefficient;

        BorderWidth = BadgeWidth + 2;
        BorderHeight = BadgeHeight + 2;

        Scale *= coefficient;

        PaddingLeft *= coefficient;
        PaddingTop *= coefficient;
        PaddingRight *= coefficient;
        PaddingBottom *= coefficient;

        foreach ( TextLineViewModel line   in   TextLines ) 
        {
            line.ZoomOn (coefficient);
        }

        foreach ( ImageViewModel image   in   InsideImages )
        {
            image.ZoomOn (coefficient);
        }

        foreach ( ShapeViewModel rect   in   InsideRectangles )
        {
            rect.ZoomOn (coefficient);
        }

        foreach ( ShapeViewModel ellipse   in   InsideEllipses )
        {
            ellipse.ZoomOn (coefficient);
        }
    }


    internal void ZoomOut ( double coefficient )
    {
        BadgeWidth /= coefficient;
        BadgeHeight /= coefficient;

        BorderWidth = BadgeWidth + 2;
        BorderHeight = BadgeHeight + 2;

        Scale /= coefficient;

        PaddingLeft /= coefficient;
        PaddingTop /= coefficient;
        PaddingRight /= coefficient;
        PaddingBottom /= coefficient;

        foreach ( TextLineViewModel line   in   TextLines )
        {
            line.ZoomOut (coefficient);
        }

        foreach ( ImageViewModel image   in   InsideImages )
        {
            image.ZoomOut (coefficient);
        }

        foreach ( ShapeViewModel rect   in   InsideRectangles )
        {
            rect.ZoomOut (coefficient);
        }

        foreach ( ShapeViewModel ellipse   in   InsideEllipses )
        {
            ellipse.ZoomOut (coefficient);
        }
    }


    internal void SetCorrectScale ( double scale )
    {
        if ( scale != 1 )
        {
            ZoomOn (scale);
        }
    }


    internal void Split ( double scale )
    {
        if ( FocusedLine == null )
        {
            return;
        }

        Model.Split ( FocusedLine.Model );
        SetTextLines ();
    }


    private void SetUpImages ( List <InsideImage> insideImages ) 
    {
        for ( int index = 0;   index < insideImages.Count;   index++ )
        {
            InsideImage image = insideImages [index];
            ImageViewModel imageViewModel = new ImageViewModel (index, image);

            InsideImages.Add (imageViewModel);

            ShiftDownBelowMembersIfShould ( imageViewModel );
        }
    }


    private void SetUpShapes ( List <InsideShape> insideShapes )
    {
        int rectId = 0;
        int ellipseId = 0;

        for ( int index = 0;   index < insideShapes.Count;   index++ )
        {
            InsideShape shape = insideShapes [index];

            if ( shape.Type == ShapeType.rectangle )
            {
                ShapeViewModel rectangle = new ShapeViewModel (rectId, shape);
                rectId++;
                InsideRectangles.Add (rectangle);
                ShiftDownBelowMembersIfShould( rectangle );
            }
            else if ( shape.Type == ShapeType.ellipse ) 
            {
                ShapeViewModel ellipse = new ShapeViewModel (ellipseId, shape);
                ellipseId++;
                InsideEllipses.Add (ellipse);
                ShiftDownBelowMembersIfShould( ellipse );
            }
        }
    }


    private void ShiftDownBelowMembersIfShould ( BoundToTextLine member )
    {
        if ( ! string.IsNullOrWhiteSpace(member.Binding) ) 
        {
            double startMemberTopOffset = member.TopOffset;
            int scratch = 0;
            int boundPartsCount = 0;
            bool boundIsFound = false;
            double delta = 0;
            int index = 0;

            for ( ;   index < TextLines.Count;   index++ )
            {
                TextLineViewModel line = TextLines [index];

                if ( member.Binding == line.Model.Name )
                {
                    if ( ! boundIsFound ) 
                    {
                        delta += line.TopOffset;
                    }

                    boundIsFound = true;
                    boundPartsCount++;

                    if ( member.IsAboveOfBinding )
                    {
                        delta -= member.TopOffset;
                        delta -= member.HeightWithBorder;
                    }
                    else 
                    {
                        delta += line.FontSize;
                        delta += _interLineAddition;
                    }
                }
                else if ( boundIsFound ) 
                {
                    break;
                }
            }

            if ( boundIsFound ) 
            {
                scratch = index;
                delta -= _interLineAddition;
                member.TopOffset += delta;
                ShiftBelowMembers ( member, index, startMemberTopOffset );
            }
        }
    }


    private void ShiftBelowMembers ( BoundToTextLine member, int startIndex, double memberRelativeTopOffset ) 
    {
        for ( int index = startIndex;   index < TextLines.Count;   index++ )
        {
            TextLineViewModel line = TextLines [index];
            line.TopOffset += ( member.Height + memberRelativeTopOffset );
        }

        ShiftBelowBounds (InsideImages, member, startIndex, memberRelativeTopOffset);
        ShiftBelowBounds (InsideRectangles, member, startIndex, memberRelativeTopOffset);
        ShiftBelowBounds (InsideEllipses, member, startIndex, memberRelativeTopOffset);
    }


    private void ShiftBelowBounds <T> ( ObservableCollection <T> bounds, BoundToTextLine member, int startIndex
                                      , double memberRelativeTopOffset ) where T : BoundToTextLine
    {
        for ( int index = 0;   index < bounds.Count;   index++ )
        {
            BoundToTextLine bound = bounds [index];

            if (( ! string.IsNullOrWhiteSpace (bound.Binding))   &&   (bound.TopOffset > member.TopOffset))
            {
                bound.TopOffset += ( member.Height + memberRelativeTopOffset );
            }
        }
    }


    #region CheckLineCorrectness

    internal void CheckFocusedLineCorrectness ()
    {
        if ( FocusedLine == null )
        {
            return;
        }

        CheckLineCorrectness ( FocusedLine );

        bool borderViolentsExist = ( BorderViolentLines.Count > 0 );
        bool overlayingExist = ( OverlayViolentLines.Count > 0 );

        if ( OverlayViolentLines.Count > 0 )
        {
            TextLineViewModel line = OverlayViolentLines [0];
        }

        IsCorrect = !( borderViolentsExist || overlayingExist );
    }




    private void MarkAsIncorrect ( TextLineViewModel setable )
    {
        setable.Background = _incorrectLineBackground;
        setable.Mark (_incorrectMemberBorderColor, _incorrectMemberBorderThickness);
    }


    private void MarkAsCorrect ( TextLineViewModel setable )
    {
        setable.Background = null;
        setable.Mark (_correctMemberBorderColor, _correctMemberBorderThickness);
    }


    private SolidColorBrush ? GetColor ( List<byte> rgb )
    {
        if ( rgb == null )
        {
            return null;
        }
        else
        {
            if ( rgb.Count == 3 )
            {
                byte r = rgb [0];
                byte g = rgb [1];
                byte b = rgb [2];

                SolidColorBrush brush;
                Color color = new Color (255, r, g, b);

                if ( MainViewModel.MainViewIsWaiting )
                {
                    var result = Dispatcher.UIThread.Invoke
                    (() =>
                    {
                        return new SolidColorBrush (color);
                    });

                    brush = result;
                }
                else
                {
                    brush = new SolidColorBrush (color);
                }

                return brush;
            }
            else
            {
                return null;
            }
        }
    }


    private SolidColorBrush ? GetColor ( string hexColor )
    {
        if ( hexColor == null )
        {
            return null;
        }
        else
        {
            if ( hexColor.Length == 7 )
            {
                SolidColorBrush brush;
                Color color;

                if ( ! Color.TryParse (hexColor, out color) )
                {
                    return null;
                }

                if ( MainViewModel.MainViewIsWaiting )
                {
                    var result = Dispatcher.UIThread.Invoke
                    (() =>
                    {
                        return new SolidColorBrush (color);
                    });

                    brush = result;
                }
                else
                {
                    brush = new SolidColorBrush (color);
                }

                return brush;
            }
            else
            {
                return null;
            }
        }
    }


    private Avalonia.Thickness GetThickness ( List<byte> ltrb )
    {
        if ( ltrb == null )
        {
            return new Avalonia.Thickness (0, 0, 0, 0);
        }
        else
        {
            if ( ltrb.Count == 4 )
            {
                double left = ltrb [0];
                double top = ltrb [1];
                double right = ltrb [2];
                double bottom = ltrb [3];

                Avalonia.Thickness thickness;

                if ( MainViewModel.MainViewIsWaiting )
                {
                    var result = Dispatcher.UIThread.Invoke
                    (() =>
                    {
                        return new Avalonia.Thickness (left, top, right, bottom);
                    });

                    thickness = result;
                }
                else
                {
                    thickness = new Avalonia.Thickness (left, top, right, bottom);
                }

                return thickness;
            }
            else
            {
                return new Avalonia.Thickness (0, 0, 0, 0);
            }
        }
    }

    #endregion

    #region FontSizeChange

    internal void IncreaseFontSize ( double changeSize )
    {
        ChangeFontSize (changeSize, false);
    }


    internal void ReduceFontSize ( double changeSize )
    {
        ChangeFontSize (changeSize, true);
    }


    private void ChangeFontSize ( double changeSize, bool toReduce )
    {
        if ( FocusedLine == null )
        {
            return;
        }

        if ( toReduce )
        {
            FocusedLine.ReduceFontSize (changeSize);
        }
        else
        {
            FocusedLine.IncreaseFontSize (changeSize);
        }

        IsChanged = true;
        int visibleFontSize = (int) Math.Round (FocusedLine. FontSize / Scale);
        FocusedFontSize = visibleFontSize.ToString ();
        CheckFocusedLineCorrectness ();
    }
    #endregion

    #region Moving

    internal void MoveCaptured ( Avalonia.Point delta )
    {
        if ( FocusedLine != null )
        {
            Model.MoveComponent ( FocusedLine.Model, delta.Y/Scale, delta.X/Scale );
            FocusedLine.TopOffset = FocusedLine.Model.TopOffset * Scale;
            FocusedLine.LeftOffset = FocusedLine.Model.LeftOffset * Scale;
        }
        else if ( FocusedImage != null )
        {
            Model.MoveComponent ( FocusedImage.Model, delta.Y / Scale, delta.X / Scale );
            FocusedImage.Model.TopOffset = FocusedImage.Model.TopOffset * Scale;
            FocusedImage.Model.LeftOffset = FocusedImage.Model.LeftOffset * Scale;
        }
        else if ( FocusedRect != null )
        {
            Model.MoveComponent ( FocusedRect.Model, delta.Y / Scale, delta.X / Scale );
            FocusedRect.Model.TopOffset = FocusedRect.Model.TopOffset * Scale;
            FocusedRect.Model.LeftOffset = FocusedRect.Model.LeftOffset * Scale;
        }
        else if ( FocusedEllipse != null )
        {
            Model.MoveComponent ( FocusedEllipse.Model, delta.Y / Scale, delta.X / Scale );
            FocusedEllipse.Model.TopOffset = FocusedEllipse.Model.TopOffset * Scale;
            FocusedEllipse.Model.LeftOffset = FocusedEllipse.Model.LeftOffset * Scale;
        }
    }


    private List<string> GetDirection ( Avalonia.Point delta ) 
    {
        List<string> directions = [];

        if (delta.X > 0) 
        {
            directions.Add ("Left");
        }

        if ( delta.X < 0 )
        {
            directions.Add ("Right");
        }

        if ( delta.Y > 0 )
        {
            directions.Add ("Up");
        }

        if ( delta.Y < 0 )
        {
            directions.Add ("Down");
        }

        return directions;
    }


    internal void FocusedToSide ( string direction )
    {
        if ( FocusedLine != null )
        {
            Model.ShiftComponent ( direction );
            FocusedLine.TopOffset = FocusedLine.Model.TopOffset * Scale;
            FocusedLine.LeftOffset = FocusedLine.Model.LeftOffset * Scale;

            if ( FocusedLine.Model.IsBorderViolent   ||   FocusedLine.Model.IsOverLayViolent )
            {
                MarkAsIncorrect ( FocusedLine );
            }
            else 
            {
                MarkAsCorrect ( FocusedLine );
            }
        }
        else if ( FocusedImage != null )
        {
            Model.ShiftComponent ( direction );
        }
        else if ( FocusedEllipse != null )
        {
            Model.ShiftComponent ( direction );
        }
        else if ( FocusedRect != null )
        {
            Model.ShiftComponent ( direction );
        }

        IsChanged = Model.IsChanged;
        IsCorrect = Model.IsCorrect;
    }

    #endregion


    internal void ResetFocusedText ( string newText )
    {
        if ( FocusedLine == null ) 
        {
            return;
        }

        Model.ResetProcessableContent ( FocusedLine.Model, newText );
        FocusedLine.Content = newText;
        IsCorrect = Model.IsCorrect;
    }


    internal void ReleaseFocused ( )
    {
        if ( FocusedLine != null )
        {
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
    }
}