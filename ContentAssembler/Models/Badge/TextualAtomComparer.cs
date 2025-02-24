namespace Core.Models.Badge
{
    public class TextualAtomComparer<T> : IComparer<T> where T : TextualAtom
    {
        public int Compare ( T first, T second )
        {
            int result = -1;

            bool comparingShouldBe = first != null && second != null;

            if ( comparingShouldBe )
            {
                result = first.NumberToLocate - second.NumberToLocate;
            }

            return result;
        }
    }
}