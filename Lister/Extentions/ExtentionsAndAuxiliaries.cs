using Avalonia.Media.Imaging;
using View.CoreModelReflection.Badge;

namespace Lister.Extentions
{
    internal class BadgeComparer : IComparer <BadgeViewModel>
    {
        public int Compare ( BadgeViewModel ? x, BadgeViewModel ? y )
        {
            if ((x==null)  ||  (y==null)) 
            {
                return 0;
            }

            string xStringPresentation = x.Model.Person.FullName;
            string yStringPresentation = y.Model.Person.FullName;

            int res = string.Compare (xStringPresentation, yStringPresentation);

            return res;
        }
    }



    internal static class ListExtention
    {
        public static List <T> Clone <T> ( this List <T> source )
        {
            List <T> result = new List <T> ();

            if ( source == null )
            {
                return result;
            }

            for ( int index = 0;   index < source.Count;   index++ )
            {
                result.Add ( source[index] );
            }

            return result;
        }
    }



    internal static class ImageHelper
    {
        public static Bitmap ? LoadFromResource (string source)
        {
            Bitmap bitmap = null;

            if ( File.Exists (source) ) 
            {
                bitmap = new Bitmap (source);
            }

            return bitmap;
        }
    }



    internal static class StringExtention 
    {
        public static string TrimLastNewLineChar ( this string beingProcessed )
        {
            bool isNotEmpty = !string.IsNullOrEmpty ( beingProcessed );

            if ( isNotEmpty )
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
}