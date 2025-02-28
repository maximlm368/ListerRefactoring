using Core.Models.Badge;
using System.Collections.ObjectModel;

namespace Core.DocumentBuilder;

public class BadgeLine
{
    internal double _restWidth;
    private double _heightConstraint;

    //private Thickness margin;
    //internal Thickness Margin
    //{
    //    get { return margin; }
    //    set
    //    {
    //        this.RaiseAndSetIfChanged ( ref margin, value, nameof ( Margin ) );
    //    }
    //}

    private double _height;
    public double Height
    {
        get
        {
            return _height;
        }

        private set 
        {
            if ( value > _height )
            {
                _height = value;
            }
        }
    }

    public List <Badge> Badges { get; private set; }
    public int asd;


    internal BadgeLine ( double width, double heightConstraint, bool isFirst )
    {
        Badges = new ();

        _restWidth = width;
        _heightConstraint = heightConstraint;
    }


    private BadgeLine ( BadgeLine line )
    {
        Badges = new ();
        _restWidth = line._restWidth;
        _heightConstraint = line._heightConstraint;

        foreach ( Badge badge   in   line.Badges )
        {
            Badges.Add ( badge );
        }
    }


    internal BadgeLine GetDimensionalOriginal ()
    {
        BadgeLine original = new BadgeLine ( this );
        return original;
    }


    internal BuildingSuccess AddBadge ( Badge badge )
    {
        bool isFailureByWidth = ( _restWidth < badge.Layout.Width );
        bool isFailureByHeight = ( _heightConstraint < badge.Layout.Height );

        if ( isFailureByWidth )
        {
            return BuildingSuccess.FailureByWidth;
        }
        else if ( isFailureByHeight )
        {
            return BuildingSuccess.FailureByHeight;
        }
        else
        {
            //if ( Badges.Count == 0 )
            //{
            //    badge.Margin = new Thickness ( 0, 0, 0, 0 );
            //}
            //else
            //{
            //    badge.Margin = new Thickness ( -1, 0, 0, 0 );
            //}

            Badges.Add ( badge );
            Height = badge.Layout.Height;
            _restWidth -= badge.Layout.Width;

            return BuildingSuccess.Success;
        }
    }


    internal void Clear ()
    {
        Badges.Clear ();
    }
}
