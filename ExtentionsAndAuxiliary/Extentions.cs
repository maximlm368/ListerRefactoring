using System.IO;
using System.Text;

namespace ExtentionsAndAuxiliary
{
    public static class StringExtention
    {
        public static string ExtractFileNameFromPath ( this string path )
        {
            string result;
            var builder = new StringBuilder ();

            for ( var charCounter = path.Length - 1;   charCounter >= 0;   charCounter-- )
            {
                if ( (path [charCounter] != '/')   &&   (path [charCounter] != '\\') )
                {
                    builder.Append (path [charCounter]);
                }
                else
                {
                    break;
                }
            }

            result = builder.ToString ().ReverseAndReturn ();
            return result;
        }


        public static string ReverseAndReturn ( this string str )
        {
            string result;
            var builder = new StringBuilder ();

            for ( var charCounter = str.Length - 1;   charCounter >= 0;   charCounter-- )
            {
                builder.Append (str [charCounter]);
            }

            result = builder.ToString ();
            return result;
        }


        public static List<string> SeparateTail ( this string str )
        {
            List<string> result = new List<string> ();

            if ( str == null ) 
            {
                return result;
            }

            for ( var index = str.Length - 1;   index >= 0;   index-- )
            {
                if ( (str [index] == ' ')   ||   (str [index] == '-') )
                {
                    int gapLength = 1;
                    int endPartLength = str.Length - index - gapLength;
                    string secondPart = str.Substring (index + 1, endPartLength);
                    string firstPart = str.Substring (0, index);
                    result.Add (firstPart);
                    result.Add (secondPart);

                    break;
                }
            }

            return result;
        }


        public static List<string> SeparateIntoMainAndTailViaLastSeparators ( this string str, List<char> separators )
        {
            List<string> result = new List<string> ();

            if ( ( str == null ) || ( separators == null ) )
            {
                return result;
            }

            List<short> mask = GetMaskOf (separators);

            for ( int index = str.Length - 1;   index >= 0;   index-- )
            {
                short shortPresentation = (short) str[index];

                if ( shortPresentation == mask [shortPresentation] )
                {
                    int gapLength = 1;
                    int endPartLength = str.Length - index - gapLength;
                    string secondPart = str.Substring (index + 1, endPartLength);
                    string firstPart = str.Substring (0, index);
                    result.Add (firstPart);
                    result.Add (secondPart);

                    break;
                }
            }

            return result;
        }


        private static List<short> GetMaskOf ( List<char> chars ) 
        {
            List<short> result = new List<short>();
            short counter = 0;

            while ( counter < short.MaxValue ) 
            {
                result.Add (0);
                counter++;
            }

            for ( int index = 0; index < chars.Count; index++ )
            {
                result [( short ) chars [index]] = ( short ) chars [index];
            }

            return result;
        }


        public static bool IsAllEmpty ( this string target, string [] parts )
        {
            bool result = true;

            for ( int index = 0; index < parts.Length; index++ )
            {
                if ( !string.IsNullOrWhiteSpace (parts [index]) )
                {
                    result = false;
                    break;
                }
            }

            return result;
        }


        public static double TranslateIntoDouble ( this string possibleDouble )
        {
            double result = 0;

            try
            {
                result = Double.Parse (possibleDouble);
            }
            catch ( FormatException ex )
            {
                return 0;
            }

            return result;
        }


        public static string ExtractPathWithoutFileName ( this string wholePath )
        {
            var builder = new StringBuilder ();
            string goalPath = string.Empty;

            for ( var index = wholePath.Length - 1;   index >= 0;   index-- )
            {
                bool fileNameIsAchieved = (wholePath [index] == '/')   ||   (wholePath [index] == '\\');

                if ( fileNameIsAchieved )
                {
                    goalPath = wholePath.Substring (0, index);
                    break;
                }
            }

            return goalPath;
        }


        public static string TrimLastSpaceOrQuoting ( this string beingProcessed )
        {
            bool isNotEmpty = ! string.IsNullOrEmpty (beingProcessed);

            if ( isNotEmpty ) 
            {
                char lastChar = beingProcessed [beingProcessed.Length - 1];
                bool isGoal = ( lastChar == ' ' )   ||   ( lastChar == '"' );

                if ( isGoal )
                {
                    beingProcessed = beingProcessed.Substring (0, beingProcessed.Length - 2);
                }
            }

            return beingProcessed;
        }

    }



    public static class ListExtensions
    {
        public static List<T []> SeparateIntoPairs<T> ( this List<T> items ) where T : class
        {
            List<T []> result = new List<T []> ();
            int counterInPair = 0;
            T [] pair = [null, null];
            bool pairIsNotEmptyAlready = false;

            for ( int itemCounter = 0; itemCounter < items.Count; itemCounter++ )
            {
                pair [counterInPair] = items [itemCounter];
                pairIsNotEmptyAlready = true;

                if ( counterInPair == 1 )
                {
                    result.Add (pair);
                    pair = [null, null];
                    counterInPair = 0;
                    pairIsNotEmptyAlready = false;
                }
                else
                {
                    counterInPair++;
                }

                bool isLastPair = ( itemCounter == items.Count - 1 );

                if ( isLastPair && pairIsNotEmptyAlready )
                {
                    result.Add (pair);
                }
            }

            return result;
        }
    }



    public static class ArrayExtensions
    {
        public static T [] SubArray<T> ( this T [] array, int offset, int length )
        {
            T [] result = new T [length];
            Array.Copy (array, offset, result, 0, length);
            return result;
        }


        public static T [] ? ReplaceNullItem <T> ( this T [] array, T placeHolder )
        {
            if ( (array == null)   ||   (placeHolder == null) ) 
            {
                return null;
            }

            T [] result = new T [array.Length];

            for ( int index = 0;   index < array.Length;   index++ ) 
            {
                if ( array [index] == null ) 
                {
                    result [index] = placeHolder;
                }
            }
            
            return result;
        }
    }


    public enum FileDialogMode
    {
        pick = 0,
        save = 1
    }
}
