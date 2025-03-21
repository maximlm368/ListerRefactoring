namespace Lister.Core.ExtentionsAndAuxiliary;

public static class StringExtention
{
    public static List<string> SeparateTailOnce ( this string str, char [] separators )
    {
        List<string> result = new List<string> ( 2 );

        if ( str == null )
        {
            return result;
        }

        for ( int index = str.Length - 1; index >= 0; index-- )
        {
            if ( separators.Contains ( str [index] ) )
            {
                int gapLength = 1;
                int endPartLength = str.Length - index - gapLength;
                string secondPart = str.Substring ( index + gapLength, endPartLength );
                string firstPart = str.Substring ( 0, index );
                result.Add ( firstPart );
                result.Add ( secondPart );

                break;
            }
        }

        return result;
    }


    public static List<string> SplitBySeparators ( this string str, char [] separators, char [] onceUnremovableSeparators )
    {
        List<string> result = new List<string> ();

        if ( ( separators == null )  ||  ( separators.Length < 1 )  ||  ( str == null ) )
        {
            return result;
        }

        string [] separatorStrs = new string [separators.Length];

        for ( int index = 0; index < separators.Length; index++ ) 
        {
            separatorStrs [index] = separators [index].ToString ();
        }

        string rest = string.Empty;
        int splitingStart = 0;
        int splitingLength = 1;
        bool isWaitingUnremovable = false;
        bool unremovableIsEncountered = false;

        for ( int index = 0; index < str.Length - 1; index++ )
        {
            if ( separators.Contains ( str [index] ) )
            {
                string splited = str.Substring ( splitingStart, splitingLength );
                rest = str.Substring ( index + 1, str.Length - index - 1 );
                splitingStart = index + 1;
                splitingLength = 1;

                if ( ( splited != string.Empty )  &&  ! separatorStrs.Contains ( splited ) )
                {
                    result.Add ( splited );
                }

                if ( onceUnremovableSeparators.Contains ( str [index] ) )
                {
                    if ( result.Count > 0 )
                    {
                        string last = result.Last ();

                        if ( isWaitingUnremovable  &&  ( last != null ) )
                        {
                            char lastGlyph = last.Last ();

                            if ( separators.Contains ( lastGlyph ) )
                            {
                                last = last.TrimEnd ( lastGlyph );
                            }

                            last = last + str [index];
                            result [result.Count - 1] = last;
                        }
                    }

                    isWaitingUnremovable = false;
                    unremovableIsEncountered = true;
                }
                else if ( !onceUnremovableSeparators.Contains ( str [index] )  &&  ! unremovableIsEncountered )
                {
                    isWaitingUnremovable = true;
                }
            }
            else
            {
                splitingLength++;
                isWaitingUnremovable = false;
                unremovableIsEncountered = false;
            }
        }

        result.Add ( rest );

        return result;
    }
}

