using ContentAssembler;
using ExtentionsAndAuxiliary;
using Microsoft.Extensions.Configuration;
//using NJsonSchema;

namespace DataGateway
{
    public class JsonCompatibleBadgeAppearence
    {
        public string TemplateName { set; get; }
        public string BackgroundImagePath { set; get; }
        public string CommonDefaultFontFamily { set; get; }
        public int Width { set; get; }
        public int Height { set; get; }
        public JsonMargin InsideSpan { set; get; }
        public JsonRGB IncorrectLineBackground { set; get; }

        public JsonCompatibleBadgeLine FamilyName { set; get; }
        public JsonCompatibleBadgeLine FirstName { set; get; }
        public JsonCompatibleBadgeLine PatronymicName { set; get; }
        public JsonCompatibleBadgeLine Post { set; get; }
        public JsonCompatibleBadgeLine Department { set; get; }


    }



    public class JsonCompatibleBadgeLine
    {
        public int Width { set; get; }
        public int Height { set; get; }
        public int TopOffset { set; get; }
        public int LeftOffset { set; get; }
        public string Alignment { set; get; }
        public int FontSize { set; get; }
        public string FontFile { set; get; }
        public string FontName { set; get; }
        public JsonRGB Foreground { set; get; }
        public string FontWeight { set; get; }
        public bool IsSplitable { set; get; }


        public static JsonCompatibleBadgeLine SetDefault ( int widht, int height, int topOffset, int leftOffset
                                                         , string alignment, int fontSize
                                                         , JsonRGB foreground, string fontWeight, bool isSplitable ) 
        {
            JsonCompatibleBadgeLine result = new JsonCompatibleBadgeLine ();

            result.Width = widht;
            result.Height = height;
            result.TopOffset = topOffset;
            result.LeftOffset = leftOffset;
            result.Alignment = alignment;
            result.FontSize = fontSize;
            result.Foreground = foreground;
            result.FontWeight = fontWeight;
            result.IsSplitable = isSplitable;

            return new JsonCompatibleBadgeLine ();
        }
    }



    public class JsonMargin
    {
        public int Left { set; get; }
        public int Top { set; get; }
        public int Right { set; get; }
        public int Bottom { set; get; }

        public JsonMargin SetDefault ( int left, int top, int right, int bottom ) 
        {
            JsonMargin result = new JsonMargin ();

            result.Left = left;
            result.Top = top;
            result.Right = right;
            result.Bottom = bottom;

            return result;
        }
    }



    public class JsonRGB
    {
        public int Red { set; get; }
        public int Green { set; get; }
        public int Blue { set; get; }

        public JsonRGB ( int red, int green, int blue ) 
        {
            Red = red;
            Green = green;
            Blue = blue;
        }
    }




    public class JsonDefaultScheme 
    {
        public IntDefault Width { set; get; }

    }



    public class IntDefault
    {
        public int defaultValue { set; get; }
    }



    public class StringDefault
    {


    }
}