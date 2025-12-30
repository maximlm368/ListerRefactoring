namespace Lister.Core.Extentions;

public static class StringExtentions
{
    /// <summary>
    /// Once separates tail of string defining it by any of separators. 
    /// </summary>
    /// <param name="processable"></param>
    /// <param name="separators"></param>
    /// <returns>List, consisting of body before separator and tail after separator</returns>
    public static List<string> SeparateTailOnce ( this string processable, char [] separators )
    {
        List<string> result = new ( 2 );

        if ( processable == null )
        {
            return result;
        }

        for ( int index = processable.Length - 1; index >= 0; index-- )
        {
            if ( separators.Contains ( processable [index] ) )
            {
                int gapLength = 1;
                int endPartLength = processable.Length - index - gapLength;
                string secondPart = processable.Substring ( index + gapLength, endPartLength );
                string firstPart = processable [..index];
                result.Add ( firstPart );
                result.Add ( secondPart );

                break;
            }
        }

        return result;
    }

    /// <summary>
    /// Splits string by any of separators removing separator that not included in unremovableSeparators.<br/>
    /// Unremovable separator gets added to first from two splitted strings.
    /// </summary>
    /// <param name="processable"></param>
    /// <param name="separators"></param>
    /// <param name="unremovableSeparators"></param>
    /// <returns></returns>
    public static List<string> SplitBySeparators ( this string processable, char [] separators, char [] unremovableSeparators )
    {
        List<string> result = [];

        if ( ( separators == null ) || ( separators.Length < 1 ) || ( processable == null ) )
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

        for ( int index = 0; index < processable.Length - 1; index++ )
        {
            if ( separators.Contains ( processable [index] ) )
            {
                string splited = processable.Substring ( splitingStart, splitingLength );
                rest = processable.Substring ( index + 1, processable.Length - index - 1 );
                splitingStart = index + 1;
                splitingLength = 1;

                if ( ( splited != string.Empty ) && !separatorStrs.Contains ( splited ) )
                {
                    result.Add ( splited );
                }

                if ( unremovableSeparators.Contains ( processable [index] ) )
                {
                    if ( result.Count > 0 )
                    {
                        string last = result.Last ();

                        if ( isWaitingUnremovable && ( last != null ) )
                        {
                            char lastGlyph = last.Last ();

                            if ( separators.Contains ( lastGlyph ) )
                            {
                                last = last.TrimEnd ( lastGlyph );
                            }

                            last += processable [index];
                            result [^1] = last;
                        }
                    }

                    isWaitingUnremovable = false;
                    unremovableIsEncountered = true;
                }
                else if ( !unremovableSeparators.Contains ( processable [index] ) && !unremovableIsEncountered )
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

    /// <summary>
    /// Removes all unacceptable glyphs from source string
    /// </summary>
    /// <param name="processable"></param>
    /// <param name="acceptableGlyphs"></param>
    /// <returns>pure (whithout unacceptable glyphs) string</returns>
    public static string? RemoveUnacceptableGlyphs ( this string? processable, List<char> acceptableGlyphs )
    {
        if ( string.IsNullOrWhiteSpace ( processable ) )
        {
            return processable;
        }

        for ( int index = 0; index < processable.Length; index++ )
        {
            if ( !acceptableGlyphs.Contains ( processable [index] ) )
            {
                processable = processable.Remove ( index, 1 );
                break;
            }
        }

        return processable;
    }
}
