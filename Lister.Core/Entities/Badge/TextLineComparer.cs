namespace Lister.Core.Entities.Badge;

/// <summary>
/// Comparers TextLines.
/// </summary>
/// <typeparam name="T"></typeparam>
public class TextLineComparer <T> : IComparer<T> where T : TextLine
{
    public int Compare ( T? first, T? second )
    {
        if ( first == null && second != null ) 
        {
            return -1;
        }

        if ( first != null && second == null )
        {
            return 1;
        }

        if ( first == null && second == null )
        {
            return 0;
        }

        int result = -1;

        if ( first != null && second != null 
        )
        {
            result = first.NumberToLocate - second.NumberToLocate;
        }

        return result;
    }
}