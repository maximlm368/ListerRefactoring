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

        processable += separators [0];
        string [] separatorStrs = new string [separators.Length];

        for ( int index = 0; index < separators.Length; index++ )
        {
            separatorStrs [index] = separators [index].ToString ();
        }

        int splitingStart = 0;
        int splitingLength = 1;
        bool isUnremovableEncountered = false;
        bool isWordFound;

        for ( int index = 0; index < processable.Length; index++ )
        {
            isWordFound = false;

            if ( separators.Contains ( processable [index] ) )
            {
                string splited = processable.Substring ( splitingStart, splitingLength );
                splitingStart = index + 1;
                splitingLength = 1;

                if ( ( splited != string.Empty ) && !separatorStrs.Contains ( splited ) )
                {
                    result.Add ( splited );
                    isWordFound = true;
                    isUnremovableEncountered = false;
                }

                if ( unremovableSeparators.Contains ( processable [index] ) && !isUnremovableEncountered && result.Count > 0 )
                {
                    isUnremovableEncountered = true;

                    if ( !isWordFound )
                    {
                        string last = result.Last ();
                        last += processable [index];
                        result [^1] = last;
                    }
                }

                if ( !unremovableSeparators.Contains ( processable [index] ) && isWordFound )
                {
                    string last = result.Last ();
                    result [^1] = last.TrimEnd ( last.Last () );
                }
            }
            else
            {
                splitingLength++;
                isUnremovableEncountered = false;
            }
        }

        if ( result.Count > 0 && separators.Contains ( result.Last ().Last () ) )
        {
            result [^1] = result.Last ().TrimEnd ( result.Last ().Last () );
        }

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
