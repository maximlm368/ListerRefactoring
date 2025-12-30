using CommunityToolkit.Mvvm.ComponentModel;
using Lister.Desktop.ModelMappings.BadgeVM;
using System.Collections.ObjectModel;

namespace Lister.Desktop.ModelMappings;

/// <summary>
/// Maps BadgeLine model into visible entity.
/// </summary>
public sealed partial class BadgeLine : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<BadgeViewModel> _badges = [];

    [ObservableProperty]
    private Avalonia.Thickness _margin;

    private double _height;
    internal double Height
    {
        get
        {
            foreach ( BadgeViewModel badge in Badges )
            {
                if ( badge.BorderHeight > _height )
                {
                    _height = badge.BorderHeight;
                }
            }

            return _height;
        }

        private set 
        { 
            _height = value; 
        }
    }

    internal BadgeLine ( Core.Document.BadgeLine model, double scale )
    {
        foreach ( Core.Models.Badge.Badge badge in model.Badges )
        {
            BadgeViewModel addable = new ( badge );
            addable.SetCorrectScale ( scale );
            Badges.Add ( addable );
        }

        Margin = new Avalonia.Thickness ( model.Margin.Left, model.Margin.Top, model.Margin.Right, model.Margin.Bottom );
    }

    internal void ZoomOn ( double scaleCoefficient )
    {
        Height *= scaleCoefficient;

        for ( int index = 0; index < Badges.Count; index++ )
        {
            Badges [index].ZoomOn ( scaleCoefficient );
        }
    }

    internal void ZoomOut ( double scaleCoefficient )
    {
        Height /= scaleCoefficient;

        for ( int index = 0; index < Badges.Count; index++ )
        {
            Badges [index].ZoomOut ( scaleCoefficient );
        }
    }

    internal void Show ()
    {
        for ( int index = 0; index < Badges.Count; index++ )
        {
            Badges [index].Show ();
        }
    }

    internal void Hide ()
    {
        for ( int index = 0; index < Badges.Count; index++ )
        {
            Badges [index].Hide ();
        }
    }

    internal void Clear ()
    {
        Badges.Clear ();
    }
}
