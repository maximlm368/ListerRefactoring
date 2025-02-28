namespace Core.Models.Badge
{
    public class TextLineComparer<T> : IComparer<T> where T : TextLine
    {
        public int Compare ( T first, T second )
        {
            int result = -1;

            bool comparingShouldBe = first != null
                                     &&
                                     second != null;

            if ( comparingShouldBe )
            {
                result = first.NumberToLocate - second.NumberToLocate;
            }

            return result;
        }
    }
}