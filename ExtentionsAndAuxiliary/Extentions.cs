using System.IO;
using System.Text;
using ContentAssembler;

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


        public static List<string> SplitBySeparators ( this string str, List<char> separators )
        {
            List<string> result = new List<string> ();

            if ( (separators == null)   ||   (separators.Count < 1)   ||   ( str == null ) ) 
            {
                return result;
            }

            string rest = string.Empty;
            int splitedStartIndex = 0;
            int splitedLength = 2;

            for ( int index = 1;   index < str.Length - 1;   index++ )
            {
                if ( separators.Contains (str [index]))
                {
                    string splited = str.Substring (splitedStartIndex, splitedLength);
                    rest = str.Substring (index + 1, str.Length - index - 1);
                    splitedStartIndex = index + 1;
                    splitedLength = 0;
                    result.Add (splited);
                }

                splitedLength++;
            }

            result.Add (rest);
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

            for ( int index = 0;   index < chars.Count;   index++ )
            {
                result [( short ) chars [index]] = ( short ) chars [index];
            }

            return result;
        }


        public static bool IsAllEmpty ( this string target, string [] parts )
        {
            bool result = true;

            for ( int index = 0;   index < parts.Length;   index++ )
            {
                if ( ! string.IsNullOrWhiteSpace (parts [index]) )
                {
                    result = false;
                    break;
                }
            }

            return result;
        }


        public static double TranslateToDoubleOrZeroIfNot ( this string possibleDouble )
        {
            double result = 0;

            if ( possibleDouble == null )
            {
                return 0;
            }

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


        public static List <int> TranslateIntoIntList ( this string possibleArray )
        {
            List<int> result = new List<int> ( );
            int startPosition = 0;

            if ( possibleArray == null ) 
            {
                return result;
            }

            for ( int index = 0;   index < possibleArray.Length;   index++ ) 
            {
                if ( (possibleArray [index] == ',')   ||    (index == possibleArray.Length - 1) ) 
                {
                    string subStr;

                    if ( index == ( possibleArray.Length - 1 ) )
                    {
                        subStr = possibleArray.Substring (startPosition, ( ( index + 1 ) - startPosition ));
                    }
                    else 
                    {
                        subStr = possibleArray.Substring (startPosition, ( index - startPosition ));
                    }

                    try
                    {
                        int subResult = int.Parse (subStr);
                        result.Add (subResult);
                        startPosition = index + 1;
                    }
                    catch ( FormatException ex ) 
                    {
                        result.Add (0);
                    }
                }
            }

            return result;
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
                    beingProcessed = beingProcessed.Substring (0, beingProcessed.Length - 1);
                }
            }

            return beingProcessed;
        }


        public static string TrimLastNewLineChar ( this string beingProcessed )
        {
            bool isNotEmpty = !string.IsNullOrEmpty (beingProcessed);

            if ( isNotEmpty )
            {
                char lastChar = beingProcessed [beingProcessed.Length - 1];
                bool isGoal = ( lastChar == '\n' );

                if ( isGoal )
                {
                    beingProcessed = beingProcessed.Substring (0, beingProcessed.Length - 1);
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



    public class RusStringComparer <T> : IComparer <T>
    {
        public int Compare ( T first, T second )
        {
            int result = -1;

            if ( typeof (T).FullName == "ContentAssembler.Person" )
            {
                Person firstPerson = first as Person;
                Person secondPerson = second as Person;

                string firstStr = firstPerson.StringPresentation;
                string secondStr = secondPerson.StringPresentation;

                for ( int index = 0;   index < firstStr.Length;   index++ )
                {
                    char firstChar = firstStr [index];

                    if ( index > (secondStr.Length - 1) ) 
                    {
                        return 1;
                    }

                    char secondChar = secondStr [index];
                    int firstInt = ( int ) firstChar;
                    int secondInt = ( int ) secondChar;

                    if ( firstInt < secondInt )
                    {
                        result = -1;
                        break;
                    }
                    else if ( firstInt == secondInt )
                    {
                        result = 0;
                    }
                    else
                    {
                        result = 1;
                        break;
                    }
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
