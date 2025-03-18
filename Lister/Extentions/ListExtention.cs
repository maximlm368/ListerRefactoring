namespace View.Extentions;

internal static class ListExtention
{
    public static List<T> Clone<T> ( this List<T> source )
    {
        List<T> result = new ();

        if ( source == null )
        {
            return result;
        }

        for ( int index = 0;   index < source.Count;   index++ )
        {
            result.Add ( source [index] );
        }

        return result;
    }
}