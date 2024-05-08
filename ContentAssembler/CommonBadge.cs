﻿using System;
using System.IO;
using System.Drawing;
using SkiaSharp;


namespace ContentAssembler
{
    public class Size
    {
        public double width { get; private set; }
        public double height { get; private set; }

        public Size(double width, double height) 
        {
            this.width = width;
            this.height = height;
        }
    }



    public class Badge 
    {
        public Person person { get;  private set; }
        public string backgroundImagePath { get; private set; }
        public OrganizationalDataOfBadge badgeDescription { get; private set; }


        public Badge( Person person,  string backgroundImagePath,  OrganizationalDataOfBadge badgeDescription)
        {
            this.person = person;
            this.backgroundImagePath = backgroundImagePath;
            this.badgeDescription = badgeDescription;
        }
    }



    public class CommonBadge
    {
        private Person person;


        public CommonBadge(Person person)
        {
            this.person = person;
        }

    }



    public class BadgeDimensionss
    {
        public Size outlineSize { get;  private set; }
        public Size personTextAreaSize { get; private set; }
        public double personTextAreaTopShiftOnBackground { get; private set; }
        public double personTextAreaLeftShiftOnBackground { get; private set; }
        public double textAreaWidth { get; private set; }
        public double secondLevelFontSize { get; private set; }
        public double firstLevelFontSize { get; private set; }
        public double thirdLevelFontSize { get; private set; }
        public double firstLevelTBHeight { get; private set; }
        public double secondLevelTBHeight { get; private set; }
        public double thirdLevelTBHeight { get; private set; }

        public BadgeDimensionss(Size outlineSize, Size personTextAreaSize
                                  , double personTextAreaTopShiftOnBackground
                                  , double personTextAreaLeftShiftOnBackground
                                  , double firstLevelFontSize
                                  , double secondLevelFontSize
                                  , double thirdLevelFontSize
                                  , double firstLevelTBHeight
                                  , double secondLevelTBHeight
                                  , double thirdLevelTBHeight
            )
        {
            this.outlineSize = outlineSize;
            this.personTextAreaSize = personTextAreaSize;
            this.personTextAreaTopShiftOnBackground = personTextAreaTopShiftOnBackground;
            this.personTextAreaLeftShiftOnBackground = personTextAreaLeftShiftOnBackground;
            this.firstLevelFontSize = firstLevelFontSize;
            this.secondLevelFontSize = secondLevelFontSize;
            this.thirdLevelFontSize = thirdLevelFontSize;
            this.firstLevelTBHeight = firstLevelTBHeight;
            this.secondLevelTBHeight = secondLevelTBHeight;
            this.thirdLevelTBHeight = thirdLevelTBHeight;
        }
    }



    //public class BadgeDimensions
    //{
    //    public Size OutlineSize { get; private set; }
    //    public List<TextualAtom> TextualBlockDimensions { get; private set; }
       

    //    public BadgeDimensions ( Size outlineSize, List<TextualAtom> textualBlockDimensions )
    //    {
    //        OutlineSize = outlineSize;
    //        TextualBlockDimensions = textualBlockDimensions;
    //    }
    //}



    public class BadgeDimensions
    {
        public Size outlineSize { get; private set; }
        public Size personTextAreaSize { get; private set; }
        public double personTextAreaTopShiftOnBackground { get; private set; }
        public double personTextAreaLeftShiftOnBackground { get; private set; }
        public double textAreaWidth { get; private set; }
        public double secondLevelFontSize { get; private set; }
        public double firstLevelFontSize { get; private set; }
        public double thirdLevelFontSize { get; private set; }
        public double firstLevelTBHeight { get; private set; }
        public double secondLevelTBHeight { get; private set; }
        public double thirdLevelTBHeight { get; private set; }

        public BadgeDimensions ( Size outlineSize, Size personTextAreaSize
                                  , double personTextAreaTopShiftOnBackground
                                  , double personTextAreaLeftShiftOnBackground
                                  , double firstLevelFontSize
                                  , double secondLevelFontSize
                                  , double thirdLevelFontSize
                                  , double firstLevelTBHeight
                                  , double secondLevelTBHeight
                                  , double thirdLevelTBHeight
            )
        {
            this.outlineSize = outlineSize;
            this.personTextAreaSize = personTextAreaSize;
            this.personTextAreaTopShiftOnBackground = personTextAreaTopShiftOnBackground;
            this.personTextAreaLeftShiftOnBackground = personTextAreaLeftShiftOnBackground;
            this.firstLevelFontSize = firstLevelFontSize;
            this.secondLevelFontSize = secondLevelFontSize;
            this.thirdLevelFontSize = thirdLevelFontSize;
            this.firstLevelTBHeight = firstLevelTBHeight;
            this.secondLevelTBHeight = secondLevelTBHeight;
            this.thirdLevelTBHeight = thirdLevelTBHeight;
        }
    }



    public class InsideImage 
    {
        public string path { get; private set; }
        public Size size { get; private set; }
        public double relativeTopShiftOnBackground { get; private set; }
        public double relativeLeftShiftOnBackground { get; private set; }

        public InsideImage(string path, Size size, double relativeTopShiftOnBackground, double relativeLeftShiftOnBackground)
        {
            this.path = path;
            this.size = size;
            this.relativeTopShiftOnBackground = relativeTopShiftOnBackground;
            this.relativeLeftShiftOnBackground = relativeLeftShiftOnBackground;
        }
    }



    public class OrganizationalDataOfBadge 
    {
        public BadgeDimensions badgeDimensions { get; private set; }
        public List<InsideImage> ? insideImages { get; private set; }


        public OrganizationalDataOfBadge( BadgeDimensions badgeDimensions, List<InsideImage>? insideImages)
        {
            this.badgeDimensions = badgeDimensions;
            this.insideImages = insideImages;
        }
    }



    public class TextualAtom
    {
        public string Name { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }
        public double TopOffset { get; private set; }
        public double LeftOffset { get; private set; }
        public string Alignment { get; private set; }
        public double FontSize { get; private set; }
        public string FontFamily { get; private set; }

        public TextualAtom ( string name, double width, double height, double topOffset, double leftOffset
                           , string alignment, double fontSize, string fontFamily )
        {
            Name = name;
            Width = width;
            Height = height;
            TopOffset = topOffset;
            LeftOffset = leftOffset;
            Alignment = alignment;
            FontSize = fontSize;
            FontFamily = fontFamily;
        }

    }



    








}