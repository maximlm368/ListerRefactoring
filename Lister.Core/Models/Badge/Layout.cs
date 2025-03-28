using Lister.Core.DocumentProcessor.Abstractions;
using Lister.Core.Extentions;

namespace Lister.Core.Models.Badge;

/// <summary>
/// Definds components of badge and its description.
/// Controls change of this content.
/// Can have processable component to change it.
/// </summary>
public sealed class Layout
{
    internal static ITextWidthMeasurer Measurer { get; set; }

    private readonly double _interLineAddition = 1;

    private List<double> _paddinigs;
    private List<TextLine> _paddingViolentLines;
    private List<TextLine> _overlayViolentLines;
    private TextLine _processableLine;

    private LayoutComponentBase _processableComponent;
    internal LayoutComponentBase ProcessableComponent
    {
        get { return _processableComponent; }
        set
        {
            _processableLine = value as TextLine;
            _processableComponent = value;
        }
    }

    internal delegate void RolledBackHandler ();
    internal event RolledBackHandler? RolledBack;

    public string TemplateName { get; private set; }
    public double Width { get; private set; }
    public double Height { get; private set; }
    public double BorderWidth { get; private set; }
    public double BorderHeight { get; private set; }
    public double PaddingLeft { get; private set; }
    public double PaddingTop { get; private set; }
    public double PaddignRight { get; private set; }
    public double PaddingBottom { get; private set; }
    public List<TextLine> TextLines { get; private set; }
    public List<ComponentImage> Images { get; private set; }
    public List<ComponentShape> Shapes { get; private set; }
    public bool HasIncorrectLines { get; private set; }


    public Layout ( double width, double height, string templateName, List<double>? paddings
                       , List<TextLine>? textualFields, List<ComponentImage>? insideImages
                       , List<ComponentShape>? insideShapes )
    {
        Width = width;
        Height = height;
        BorderWidth = Width + 2;
        BorderHeight = Height + 2;
        TemplateName = templateName;
        SetPaddings ( paddings );
        TextLines = textualFields ?? new List<TextLine> ();
        Images = insideImages ?? new List<ComponentImage> ();
        Shapes = insideShapes ?? new List<ComponentShape> ();
        _paddingViolentLines = new ();
        _overlayViolentLines = new ();

        foreach ( TextLine line in TextLines )
        {
            if ( line.IsPaddingViolent )
            {
                _paddingViolentLines.Add ( line );
            }
            else if ( line.IsOverLayViolent )
            {
                _overlayViolentLines.Add ( line );
            }
        }

        OrderTextlinesVerticaly ();
    }


    internal void SetUpComponents ( Dictionary<string, string> textualComponents )
    {
        SetTextualValues ( textualComponents );
        SetUpTextFields ();
        SetUpImages ();
        SetUpShapes ();
        GatherIncorrectLines ();
    }


    private void SetUpImages ()
    {
        for ( int index = 0; index < Images.Count; index++ )
        {
            ShiftDownBelowMembersIfShould ( Images [index] );
        }
    }


    private void SetUpShapes ()
    {
        for ( int index = 0; index < Shapes.Count; index++ )
        {
            ShiftDownBelowMembersIfShould ( Shapes [index] );
        }
    }


    private void ShiftDownBelowMembersIfShould ( BindableToAnother bindable )
    {
        if ( !string.IsNullOrWhiteSpace ( bindable.Binding ) )
        {
            double startMemberTopOffset = bindable.TopOffset;
            int scratch = 0;
            int boundPartsCount = 0;
            bool boundIsFound = false;
            double delta = 0;
            int index = 0;

            for ( ; index < TextLines.Count; index++ )
            {
                TextLine line = TextLines [index];

                if ( bindable.Binding == line.Name )
                {
                    if ( !boundIsFound )
                    {
                        delta += line.TopOffset;
                    }

                    boundIsFound = true;
                    boundPartsCount++;

                    if ( bindable.IsAboveOfBinding )
                    {
                        delta -= bindable.TopOffset;
                        delta -= bindable.HeightWithBorder;
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
                bindable.TopOffset += delta;
                ShiftBelowComponents ( bindable, index, startMemberTopOffset );
            }
        }
    }


    private void ShiftBelowComponents ( BindableToAnother bindable, int startIndex, double memberRelativeTopOffset )
    {
        for ( int index = startIndex; index < TextLines.Count; index++ )
        {
            TextLine line = TextLines [index];
            line.TopOffset += ( bindable.Height + memberRelativeTopOffset );
        }

        ShiftBelowBounds ( Images, bindable, startIndex, memberRelativeTopOffset );
        ShiftBelowBounds ( Shapes, bindable, startIndex, memberRelativeTopOffset );
    }


    private void ShiftBelowBounds<T> ( List<T> bounds, BindableToAnother bindable, int startIndex
                                      , double componentRelativeTopOffset ) where T : BindableToAnother
    {
        for ( int index = 0; index < bounds.Count; index++ )
        {
            BindableToAnother bound = bounds [index];

            if ( ( !string.IsNullOrWhiteSpace ( bound.Binding ) ) && ( bound.TopOffset > bindable.TopOffset ) )
            {
                bound.TopOffset += ( bindable.Height + componentRelativeTopOffset );
            }
        }
    }


    private void SetPaddings ( List<double> paddings )
    {
        if ( (paddings != null) && (paddings.Count == 4) )
        {
            PaddingLeft = paddings [0];
            PaddingTop = paddings [1];
            PaddignRight = paddings [2];
            PaddingBottom = paddings [3];
            _paddinigs = paddings;
        }
        else
        {
            PaddingLeft = 0;
            PaddingTop = 0;
            PaddignRight = 0;
            PaddingBottom = 0;
            _paddinigs = new List<double> () { 0, 0, 0, 0 };
        }
    }


    internal Layout Clone ( bool resultIsPersonDataFree )
    {
        List<TextLine> lines = CloneTextLines ( resultIsPersonDataFree );
        List<ComponentImage> images = CloneImages ();
        List<ComponentShape> shapes = CloneShapes ();
        Layout clone = new Layout ( Width, Height, TemplateName, _paddinigs, lines, images, shapes );

        return clone;
    }


    private List<TextLine> CloneTextLines ( bool resultIsPersonDataFree )
    {
        List<TextLine> lines = new ();
        
        foreach ( TextLine line in TextLines )
        {
            TextLine lineClone;

            if ( resultIsPersonDataFree )
            {
                lineClone = line.CloneAsDescription ();
            }
            else
            {
                lineClone = line.Clone ();
            }

            lines.Add ( lineClone );
        }

        return lines;
    }


    private List<ComponentImage> CloneImages ()
    {
        List<ComponentImage> images = new ();

        foreach ( ComponentImage image in Images )
        {
            images.Add ( image.Clone () );
        }

        return images;
    }


    private List<ComponentShape> CloneShapes ()
    {
        List<ComponentShape> shapes = new ();

        foreach ( ComponentShape shape in Shapes )
        {
            shapes.Add ( shape.Clone () );
        }

        return shapes;
    }


    internal void RollBackTo ( Layout destination )
    {
        TextLines = new ();
        _paddingViolentLines = new ();
        _overlayViolentLines = new ();

        for ( int index = 0; index < destination.TextLines.Count; index++ )
        {
            TextLine line = destination.TextLines [index].Clone ();
            TextLines.Add ( line );

            if ( line.IsPaddingViolent )
            {
                _paddingViolentLines.Add ( line );
            }

            if ( line.IsOverLayViolent )
            {
                _overlayViolentLines.Add ( line );
            }
        }

        Images = new ();
        Shapes = new ();

        for ( int index = 0; index < destination.Images.Count; index++ )
        {
            ComponentImage image = destination.Images [index].Clone ();
            Images.Add ( image );
        }

        for ( int index = 0; index < destination.Shapes.Count; index++ )
        {
            ComponentShape shape = destination.Shapes [index].Clone ();
            Shapes.Add ( shape );
        }

        HasIncorrectLines = ( _overlayViolentLines.Count > 0 ) || ( _paddingViolentLines.Count > 0 );
        RolledBack?.Invoke ();
    }


    private void OrderTextlinesVerticaly ()
    {
        for ( int index = 0; index < TextLines.Count; index++ )
        {
            for ( int num = index; num < TextLines.Count - 1; num++ )
            {
                TextLine current = TextLines [index];
                TextLine next = TextLines [index + 1];

                if ( current.TopOffset > next.TopOffset )
                {
                    TextLine reserve = TextLines [index];
                    TextLines [index] = next;
                    TextLines [index + 1] = reserve;
                }
            }
        }
    }


    private void SetUpTextFields ()
    {
        double summaryVerticalOffset = 0;

        List<TextLine> result = new ();

        for ( int index = 0; index < TextLines.Count; index++ )
        {
            bool isSplitingOccured = false;
            bool shouldShiftDownNextLine = false;
            TextLine processable = TextLines [index];

            string fontWeightName = processable.FontWeight;
            double fontSize = processable.FontSize;
            string fontName = processable.FontName;

            processable.TopOffset += summaryVerticalOffset;
            double topOffset = processable.TopOffset;
            string processableContent = processable.Content.Trim ();
            string tail = string.Empty;

            while ( true )
            {
                double usefulTextBlockWidth =
                                         Measurer.Measure ( processableContent, fontWeightName, fontSize, fontName );

                bool lineIsOverflow = ( usefulTextBlockWidth >= processable.Width );

                if ( !lineIsOverflow )
                {
                    if ( shouldShiftDownNextLine )
                    {
                        summaryVerticalOffset += ( processable.FontSize + 1 );
                        topOffset += ( processable.FontSize + 1 );
                    }

                    if ( isSplitingOccured )
                    {
                        shouldShiftDownNextLine = true;
                    }

                    TextLine line = new TextLine ( processable, processableContent, false );
                    line.TopOffset = topOffset;
                    result.Add ( line );

                    if ( tail != string.Empty )
                    {
                        processableContent = tail.Trim ();
                        tail = string.Empty;
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }

                List<string> splited = processableContent.SeparateTailOnce ( new char [] { ' ', '-' } );

                if ( ( splited.Count > 0 ) && processable.IsSplitable )
                {
                    processableContent = splited [0].Trim ();
                    tail = splited [1] + " " + tail;
                    isSplitingOccured = true;
                }
                else
                {
                    TextLine line = new TextLine ( processable, processableContent, false );
                    line.TopOffset = topOffset;
                    result.Add ( line );
                    line.IsPaddingViolent = true;
                    HasIncorrectLines = true;

                    break;
                }
            }
        }

        TextLines = result;
    }


    private void SetTextualValues ( Dictionary<string, string> personProperties )
    {
        List<TextLine> includibles = new ();
        List<TextLine> includings = new ();

        AllocateValues ( personProperties, includibles );
        SetComplexValuesToIncludingAtoms ( includings, includibles );

        List<TextLine> removables = new ();

        foreach ( TextLine includedAtom in includibles )
        {
            if ( !includedAtom.isNeeded )
            {
                removables.Add ( includedAtom );
            }
        }

        foreach ( TextLine removable in removables )
        {
            TextLines.Remove ( removable );
        }

        TextLines.Sort ( new TextLineComparer<TextLine> () );

        TrimTrashEdges ();
    }


    private void AllocateValues ( Dictionary<string, string> personProperties, List<TextLine> includibles )
    {
        foreach ( KeyValuePair<string, string> property in personProperties )
        {
            foreach ( TextLine line in TextLines )
            {
                if ( line.Name == property.Key )
                {
                    line.Content = property.Value;
                    includibles.Add ( line );
                    break;
                }
            }
        }
    }


    private void SetComplexValuesToIncludingAtoms ( List<TextLine> includings, List<TextLine> includibles )
    {
        foreach ( TextLine line in TextLines )
        {
            bool atomIsIncluding = !line.ContentIsSet;

            if ( atomIsIncluding )
            {
                string complexContent = "";

                foreach ( string includedAtomName in line.IncludedLines )
                {
                    foreach ( TextLine includedAtom in includibles )
                    {
                        bool coincide = includedAtom.Name == includedAtomName;

                        if ( coincide )
                        {
                            complexContent += includedAtom.Content + " ";
                            includedAtom.isNeeded = false;
                            break;
                        }
                    }
                }

                line.Content = complexContent;
                includings.Add ( line );
            }
        }
    }


    private void TrimTrashEdges ()
    {
        List<char> unNeeded = new List<char> () { ' ', '"' };

        foreach ( TextLine atom in TextLines )
        {
            atom.TrimUnneededEdgeChar ( unNeeded );
        }
    }


    private void GatherIncorrectLines ()
    {
        foreach ( TextLine line in TextLines )
        {
            bool isViolent = CheckPaddingViolation ( line );

            if ( isViolent )
            {
                _paddingViolentLines.Add ( line );
                line.IsPaddingViolent = true;
                HasIncorrectLines = true;
            }
        }

        DefineOverlayViolations ();
    }


    private bool CheckPaddingViolation ( TextLine line )
    {
        double rest = Width - ( line.LeftOffset + line.UsefullWidth );
        bool notExceedToRight = ( rest >= PaddignRight );
        bool notExceedToLeft = ( line.LeftOffset >= PaddingLeft );
        bool notExceedToTop = ( line.TopOffset >= PaddingTop );
        rest = Height - ( line.TopOffset + line.FontSize );
        bool notExceedToBottom = ( rest >= PaddingBottom );

        bool isViolent = !( notExceedToRight
                            && notExceedToLeft
                            && notExceedToTop
                            && notExceedToBottom );

        return isViolent;
    }


    private void DefineOverlayViolations ()
    {
        for ( int index = 0; index < TextLines.Count; index++ )
        {
            TextLine overlaying = TextLines [index];
            CheckSingleOverlayViolation ( index, overlaying );
        }
    }


    private bool CheckSingleOverlayViolation ( int scratchInLines, TextLine overlaying )
    {
        bool isViolent = false;

        for ( int ind = scratchInLines; ind < TextLines.Count; ind++ )
        {
            TextLine underlaying = TextLines [ind];

            if ( underlaying.Equals ( overlaying ) ) continue;

            double verticalDifference = Math.Abs ( overlaying.TopOffset - underlaying.TopOffset );
            double topOffsetOfAbove = Math.Min ( overlaying.TopOffset, underlaying.TopOffset );
            TextLine above = overlaying;

            if ( topOffsetOfAbove == underlaying.TopOffset )
            {
                above = underlaying;
            }

            isViolent = ( verticalDifference < above.FontSize );

            double horizontalDifference = Math.Abs ( overlaying.LeftOffset - underlaying.LeftOffset );
            double leftOffsetOfLeftist = Math.Min ( overlaying.LeftOffset, underlaying.LeftOffset );
            TextLine leftist = overlaying;

            if ( leftOffsetOfLeftist == underlaying.LeftOffset )
            {
                leftist = underlaying;
            }

            isViolent = isViolent && ( horizontalDifference < leftist.UsefullWidth );

            if ( isViolent )
            {
                if ( !overlaying.IsOverLayViolent )
                {
                    _overlayViolentLines.Add ( overlaying );
                    overlaying.IsOverLayViolent = true;
                }

                break;
            }
        }

        return isViolent;
    }


    public void Split ( TextLine splitable )
    {
        List<TextLine> splitted = splitable.SplitYourself ( Width );
        ReplaceTextLine ( splitable, splitted );
    }


    private void ReplaceTextLine ( TextLine replaceble, List<TextLine> replacings )
    {
        if ( replaceble.IsPaddingViolent )
        {
            _paddingViolentLines.Remove ( replaceble );
        }

        if ( replaceble.IsOverLayViolent )
        {
            _overlayViolentLines.Remove ( replaceble );
        }

        TextLines.Remove ( replaceble );

        foreach ( TextLine line in replacings )
        {
            DefineCorrectnessOf ( line );
            TextLines.Add ( line );
        }
    }


    private void DefineCorrectnessOf ( TextLine? checkable )
    {
        if ( checkable == null )
        {
            return;
        }

        bool isPaddingViolent = CheckPaddingViolation ( checkable );

        if ( !isPaddingViolent && checkable.IsPaddingViolent )
        {
            checkable.IsPaddingViolent = false;
            _paddingViolentLines.Remove ( checkable );
        }
        else if ( isPaddingViolent && !checkable.IsPaddingViolent )
        {
            checkable.IsPaddingViolent = true;
            _paddingViolentLines.Add ( checkable );
        }

        bool isOverlayViolent = CheckSingleOverlayViolation ( 0, checkable );

        if ( !isOverlayViolent && checkable.IsOverLayViolent )
        {
            _overlayViolentLines.Remove ( checkable );
            checkable.IsOverLayViolent = false;
        }
        else if ( isOverlayViolent && !checkable.IsOverLayViolent )
        {
            _overlayViolentLines.Add ( checkable );
            checkable.IsOverLayViolent = true;
        }

        HasIncorrectLines = ( _overlayViolentLines.Count > 0 ) || ( _paddingViolentLines.Count > 0 );
    }


    internal void CheckProcessableLineCorrectness ()
    {
        TextLine processable = ProcessableComponent as TextLine;

        if ( processable == null )
        {
            return;
        }

        DefineCorrectnessOf ( processable );

        bool borderViolentsExist = ( _paddingViolentLines.Count > 0 );
        bool overlayingExist = ( _overlayViolentLines.Count > 0 );

        if ( _overlayViolentLines.Count > 0 )
        {
            TextLine line = _overlayViolentLines [0];
        }

        HasIncorrectLines = !( borderViolentsExist || overlayingExist );
    }


    internal void ShiftProcessable ( string direction )
    {
        ProcessableComponent?.Shift ( direction, GetRestrictions () );
        DefineCorrectnessOf ( _processableLine );
    }


    internal void MoveProcessable ( double verticalDelta, double horizontalDelta )
    {
        ProcessableComponent?.Move ( verticalDelta, horizontalDelta, GetRestrictions () );
        DefineCorrectnessOf ( _processableLine );
    }


    internal void ResetProcessableLineContent ( string newContent )
    {
        _processableLine?.ResetContent ( newContent );
        DefineCorrectnessOf ( _processableLine );
    }


    internal void IncreaseFontSize ()
    {
        _processableLine?.IncreaseFontSize ();
        DefineCorrectnessOf ( _processableLine );
    }


    internal void ReduceFontSize ()
    {
        _processableLine?.ReduceFontSize ();
        DefineCorrectnessOf ( _processableLine );
    }


    internal void SetProcessable ( LayoutComponentBase processableComponent )
    {
        ProcessableComponent = processableComponent;
        _processableLine = processableComponent as TextLine;
    }


    internal void ZeroProcessable ()
    {
        _processableLine = null;
        ProcessableComponent = null;
    }


    private Thickness GetRestrictions ()
    {
        return new Thickness ( ( ProcessableComponent.Width - PaddingLeft ) * ( -1 )
                             , ( ProcessableComponent.Height / 4 ) * ( -1 )
                             , ( Width - PaddignRight )
                             , ( Height - PaddingBottom ) );
    }
}