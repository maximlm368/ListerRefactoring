using Lister.Core.Models.Badge;

namespace Lister.Core.DocumentProcessor;

/// <summary>
/// Represents badges placed on one line.
/// Place sent badge in itself.
/// </summary>
public class BadgeLine
{
    internal double _restWidth;
    private double _heightConstraint;

    public Thickness Margin { get; private set; }

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


    internal BadgeLine ( double width, double heightConstraint, bool isFirst )
    {
        Badges = new ();

        if ( isFirst )
        {
            Margin = new Thickness ( 0, 0, 0, 0 );
        }
        else
        {
            Margin = new Thickness ( 0, -1, 0, 0 );
        }

        _restWidth = width;
        _heightConstraint = heightConstraint;
    }


    internal AdditionSuccess AddBadge ( Badge badge )
    {
        bool isFailureByWidth = ( _restWidth < badge.Layout.BorderWidth );
        bool isFailureByHeight = ( _heightConstraint < badge.Layout.BorderHeight );

        if ( isFailureByWidth )
        {
            return AdditionSuccess.FailureByWidth;
        }
        else if ( isFailureByHeight )
        {
            return AdditionSuccess.FailureByHeight;
        }
        else
        {
            if ( Badges.Count == 0 )
            {
                badge.Margin = new Thickness ( 0, 0, 0, 0 );
            }
            else
            {
                badge.Margin = new Thickness ( -1, 0, 0, 0 );
            }

            Badges.Add ( badge );
            Height = badge.Layout.BorderHeight;
            _restWidth -= badge.Layout.BorderWidth;

            return AdditionSuccess.Success;
        }
    }


    internal void Clear ()
    {
        Badges.Clear ();
    }
}
