namespace Lister.Desktop.Extentions;

internal static class ListExtention
{
    public static List<T>? Clone<T> ( this List<T>? source )
    {
        if ( source == null )
        {
            return null;
        }

        List<T> result = [];

        for ( int index = 0; index < source.Count; index++ )
        {
            result.Add ( source [index] );
        }

        return result;
    }
}
