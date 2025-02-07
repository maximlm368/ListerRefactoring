using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using ContentAssembler;

namespace ExtentionsAndAuxiliary
{
    public static class StringExtention
    {
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


        public static List<string> SplitBySeparators ( this string str, char [] separators, char [] onceUnremovableSeparators )
        {
            List<string> result = new List<string> ();

            if ( ( separators == null )  ||  ( separators.Length < 1 )  ||  ( str == null ) )
            {
                return result;
            }

            string [] separatorStrs = new string [separators.Length];

            for ( int index = 0;   index < separators.Length;   index++ ) 
            {
                separatorStrs [index] = separators [index].ToString();
            }

            string rest = string.Empty;
            int splitingStart = 0;
            int splitingLength = 1;
            bool shouldSplit = false;
            bool isWaitingUnremovable = false;
            bool unremovableIsEncountered = false;

            for ( int index = 0;   index < str.Length - 1;   index++ )
            {
                if ( separators.Contains (str [index]) )
                {
                    string splited = str.Substring (splitingStart, splitingLength);
                    rest = str.Substring (index + 1, str.Length - index - 1);
                    splitingStart = index + 1;
                    splitingLength = 1;

                    if ( ( splited != string.Empty )   &&   ! separatorStrs.Contains (splited) )
                    {
                        result.Add (splited);
                    }

                    if ( onceUnremovableSeparators.Contains (str [index]) )
                    {
                        if ( result.Count > 0 ) 
                        {
                            string last = result.Last ();

                            if ( isWaitingUnremovable   &&   ( last != null ) )
                            {
                                char lastGlyph = last.Last ();

                                if ( separators.Contains(lastGlyph) ) 
                                {
                                    last = last.TrimEnd (lastGlyph);
                                }

                                last = last + str [index];
                                result [result.Count - 1] = last;
                            }
                        }

                        isWaitingUnremovable = false;
                        unremovableIsEncountered = true;
                    }
                    else if ( ! onceUnremovableSeparators.Contains (str [index])   &&   ! unremovableIsEncountered )
                    {
                        isWaitingUnremovable = true;
                    }
                }
                else
                {
                    splitingLength++;
                    isWaitingUnremovable = false;
                    unremovableIsEncountered = false;
                }
            }

            result.Add (rest);
            return result;
        }
    }



    public static class DigitalStringParser 
    {
        public static readonly char [] digits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };


        public static int ParseToInt ( string parsable )
        {
            if (( parsable == null )   ||   parsable.ContainsNotDigit()) 
            {
                return 0;
            }

            return int.Parse (parsable);
        }


        public static short ParseToShort ( string parsable )
        {
            if ( ( parsable == null )   ||   parsable.ContainsNotDigit () )
            {
                return 0;
            }

            return short.Parse (parsable);
        }


        public static byte ParseToByte ( string parsable )
        {
            if ( (parsable == null)   ||   parsable.ContainsNotDigit () )
            {
                return 0;
            }

            return byte.Parse (parsable);
        }


        private static bool ContainsNotDigit ( this string parsable )
        {
            for ( int index = 0;   index < parsable.Length;   index++ )
            {
                if ( ! digits.Contains (parsable [index]) )
                {
                    return true;
                }
            }

            return false;
        }
    }



    public class RusStringComparer <T> : IComparer<T>
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

                for ( int index = 0; index < firstStr.Length; index++ )
                {
                    char firstChar = firstStr [index];

                    if ( index > ( secondStr.Length - 1 ) )
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

}
