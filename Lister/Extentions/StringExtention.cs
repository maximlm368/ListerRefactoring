namespace View.Extentions;

internal static class StringExtention
{
    public static string TrimLastNewLineChar ( this string beingProcessed )
    {
        if ( ! string.IsNullOrEmpty ( beingProcessed ) )
        {
            char lastChar = beingProcessed [beingProcessed.Length - 1];
            bool isGoal = ( lastChar == '\n' );

            if ( isGoal )
            {
                beingProcessed = beingProcessed.Substring ( 0, beingProcessed.Length - 1 );
            }
        }

        return beingProcessed;
    }
}