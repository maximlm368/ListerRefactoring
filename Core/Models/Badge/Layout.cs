using Core.DocumentBuilder;
using ExtentionsAndAuxiliary;
using System.Runtime.CompilerServices;
using static System.Formats.Asn1.AsnWriter;

namespace Core.Models.Badge
{
    public class Layout
    {
        internal static ITextWidthMeasurer Measurer { get; set; }

        private List<double> _paddinig;
        private List<TextLine> _borderViolentLines;
        private List<TextLine> _overlayViolentLines;
        private TextLine _processableLine;
        
        private LayoutComponent _processableComponent;
        internal LayoutComponent processableComponent 
        {
            get { return _processableComponent; }
            set 
            {
                if ( value == null )
                {
                    _processableLine = null;
                }
                else 
                {
                    if ( value as TextLine != null )
                    {
                        _processableLine = ( TextLine ) value;
                    }  
                }

                _processableComponent = value;
            }
        }

        public string TemplateName { get; private set; }

        public double Width { get; private set; }
        public double Height { get; private set; }
        public double PaddingLeft { get; private set; }
        public double PaddingTop { get; private set; }
        public double PaddignRight { get; private set; }
        public double PaddingBottom { get; private set; }

        public List <TextLine> TextLines { get; private set; }
        public List <InsideImage> InsideImages { get; private set; }
        public List <InsideShape> InsideShapes { get; private set; }

        public bool HasIncorrectLines { get; private set; }


        public Layout ( double width, double height, string templateName, List<double>? padding
                           , List<TextLine>? textualFields, List<InsideImage>? insideImages
                           , List<InsideShape>? insideShapes )
        {
            Width = width;
            Height = height;
            TemplateName = templateName;
            _paddinig = padding;

            if ( padding != null && padding.Count == 4 )
            {
                PaddingLeft = padding [0];
                PaddingTop = padding [1];
                PaddignRight = padding [2];
                PaddingBottom = padding [3];
            }
            else
            {
                PaddingLeft = 0;
                PaddingTop = 0;
                PaddignRight = 0;
                PaddingBottom = 0;
            }

            TextLines = textualFields ?? new List<TextLine> ();
            InsideImages = insideImages ?? new List<InsideImage> ();
            InsideShapes = insideShapes ?? new List<InsideShape> ();

            _borderViolentLines = new ();
            _overlayViolentLines = new ();

            OrderTextlinesVerticaly ( );
        }


        internal Layout Clone ( bool resultIsPersonDataFree )
        {
            List<TextLine> lines = new List<TextLine> ();

            foreach ( var line   in   TextLines )
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

            Layout clone = new Layout ( Width, Height, TemplateName, _paddinig, lines, InsideImages, InsideShapes );
            return clone;
        }


        private void OrderTextlinesVerticaly ( )
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


        internal void SetUpTextFields ( Dictionary<string, string> textualComponents )
        {
            SetTextualValues ( textualComponents );

            double summaryVerticalOffset = 0;

            List<TextLine> result = new ();

            for ( int index = 0;   index < TextLines.Count;   index++ )
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

                    if ( ! lineIsOverflow )
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

                    List<string> splited = processableContent.SeparateTail ();

                    if ( ( splited.Count > 0 )   &&   processable.IsSplitable )
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
                        line.IsBorderViolent = true;
                        HasIncorrectLines = true;

                        break;
                    }
                }
            }

            TextLines = result;
            GatherIncorrectLines ();
        }


        private void SetTextualValues ( Dictionary<string, string> personProperties )
        {
            List<TextLine> includibles = new ();
            List<TextLine> includings = new ();

            AllocateValues ( personProperties, includibles );
            SetComplexValuesToIncludingAtoms ( includings, includibles );

            List <TextLine> removables = new ();

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
                foreach ( TextLine atom in TextLines )
                {
                    if ( atom.Name == property.Key )
                    {
                        atom.Content = property.Value;
                        includibles.Add ( atom );
                        break;
                    }
                }
            }
        }


        private void SetComplexValuesToIncludingAtoms ( List<TextLine> includings, List<TextLine> includibles )
        {
            foreach ( TextLine atom in TextLines )
            {
                bool atomIsIncluding = ! atom.ContentIsSet;

                if ( atomIsIncluding )
                {
                    string complexContent = "";

                    foreach ( string includedAtomName in atom.IncludedAtoms )
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

                    atom.Content = complexContent;
                    includings.Add ( atom );
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
                bool isViolent = CheckInsideSpansViolation ( line );

                if ( isViolent )
                {
                    _borderViolentLines.Add ( line );
                    line.IsBorderViolent = true;
                    HasIncorrectLines = true;
                }
            }

            CheckOverlayViolation ();
        }


        private bool CheckInsideSpansViolation ( TextLine line )
        {
            double rest = Width - ( line.LeftOffset + line.UsefullWidth );
            bool notExceedToRight = ( rest >= PaddignRight );
            bool notExceedToLeft = ( line.LeftOffset >= PaddingLeft );
            bool notExceedToTop = ( line.TopOffset >= PaddingTop );
            rest = Height - ( line.TopOffset + line.FontSize );
            bool notExceedToBottom = ( rest >= PaddingBottom );

            bool isViolent = ! (notExceedToRight
                                && notExceedToLeft
                                && notExceedToTop
                                && notExceedToBottom);

            return isViolent;
        }


        private void CheckOverlayViolation ()
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
            List <TextLine> splitted = splitable.SplitYourself ( Width );
            ReplaceTextLine ( splitable, splitted );
        }


        private void ReplaceTextLine ( TextLine replaceble, List <TextLine> replacings )
        {
            if ( replaceble.IsBorderViolent )
            {
                _borderViolentLines.Remove ( replaceble );
            }

            if ( replaceble.IsOverLayViolent )
            {
                _overlayViolentLines.Remove ( replaceble );
            }

            TextLines.Remove ( replaceble );

            foreach ( TextLine line   in   replacings )
            {
                CheckLineCorrectness ( line );
                TextLines.Add ( line );
            }
        }


        private void CheckLineCorrectness ( TextLine checkable )
        {
            bool isBorderViolent = CheckInsideSpansViolation ( checkable );

            if ( ! isBorderViolent )
            {
                if ( checkable.IsBorderViolent )
                {
                    _borderViolentLines.Remove ( checkable );
                    checkable.IsBorderViolent = false;
                }
            }
            else
            {
                if ( ! checkable.IsBorderViolent )
                {
                    _borderViolentLines.Add ( checkable );
                    checkable.IsBorderViolent = true;
                }
            }

            bool isOverlayViolent = CheckSingleOverlayViolation ( 0, checkable );

            if ( ! isOverlayViolent )
            {
                if ( checkable.IsOverLayViolent )
                {
                    _overlayViolentLines.Remove ( checkable );
                    checkable.IsOverLayViolent = false;
                }
            }
            else
            {
                if ( ! checkable.IsOverLayViolent )
                {
                    _overlayViolentLines.Add ( checkable );
                    checkable.IsOverLayViolent = true;
                }
            }

            HasIncorrectLines = ( _overlayViolentLines.Count > 0 ) || ( _borderViolentLines.Count > 0 );
        }


        internal void CheckProcessableLineCorrectness ( )
        {
            TextLine processable = processableComponent as TextLine;

            if ( processable == null )
            {
                return;
            }

            CheckLineCorrectness ( processable );

            bool borderViolentsExist = ( _borderViolentLines.Count > 0 );
            bool overlayingExist = ( _overlayViolentLines.Count > 0 );

            if ( _overlayViolentLines.Count > 0 )
            {
                TextLine line = _overlayViolentLines [0];
            }

            HasIncorrectLines = ! ( borderViolentsExist   ||   overlayingExist );
        }


        internal void ShiftProcessable ( string direction )
        {
            if ( processableComponent == null ) 
            {
                return;
            }

            List<string> directions = [];
            directions.Add ( direction );

            double currentLeftOffset = processableComponent.LeftOffset;
            double currentTopOffset = processableComponent.TopOffset;

            if ( direction == "Left" )
            {
                processableComponent.Shift ( 0, 1 );
            }

            if ( direction == "Right" )
            {
                processableComponent.Shift ( 0, -1 );
            }

            if ( direction == "Up" )
            {
                processableComponent.Shift ( 1, 0 );
            }

            if ( direction == "Down" )
            {
                processableComponent.Shift ( -1, 0 );
            }

            PreventMemberHiding ( processableComponent, currentLeftOffset, currentTopOffset, directions );

            if ( processableComponent as TextLine != null ) 
            {
                CheckLineCorrectness ( processableComponent as TextLine );
            }
        }


        internal void MoveComponent ( LayoutComponent component, double verticalDelta, double horizontalDelta )
        {
            component.Shift ( verticalDelta, horizontalDelta );
        }


        private void PreventMemberHiding ( LayoutComponent preventable, double oldLeftOffset, double oldTopOffset
                                                                                          , List<string> directions )
        {
            bool isHiddenBeyondRight = ( preventable.LeftOffset > ( Width - PaddignRight ) )
                                       && ( directions.Contains ( "Right" ) );

            if ( isHiddenBeyondRight )
            {
                preventable.LeftOffset = oldLeftOffset;
            }

            bool isHiddenBeyondLeft = ( preventable.LeftOffset < ( preventable.Width - PaddingLeft ) * ( -1 ) )
                                     && ( directions.Contains ( "Left" ) );

            if ( isHiddenBeyondLeft )
            {
                preventable.LeftOffset = oldLeftOffset;
            }

            bool isHiddenBeyondBottom = ( preventable.TopOffset > ( Height - PaddingBottom ) )
                                        && ( directions.Contains ( "Down" ) );

            if ( isHiddenBeyondBottom )
            {
                preventable.TopOffset = oldTopOffset;
            }

            bool isHiddenBeyondTop = ( preventable.TopOffset < ( preventable.Height / 4 ) * ( -1 ) )
                                     && ( directions.Contains ( "Up" ) );

            if ( isHiddenBeyondTop )
            {
                preventable.TopOffset = oldTopOffset;
            }
        }


        public void ResetProcessableContent ( string newContent )
        {
            TextLine processable = processableComponent as TextLine;

            if ( processable == null ) 
            {
                return;
            }

            if ( changable.Content != newContent )
            {
                IsChanged = true;
            }

            changable.ResetContent ( newContent );
            CheckProcessableLineCorrectness ();
        }
    }
}