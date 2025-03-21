using ReactiveUI;
using System.Collections.ObjectModel;

namespace Lister.Desktop.CoreModelReflections.BadgeVM;

internal class BadgeLine : ReactiveObject
{
    private double _scale;

    private ObservableCollection<BadgeViewModel> badges;
    internal ObservableCollection<BadgeViewModel> Badges
    {
        get { return badges; }
        set
        {
            this.RaiseAndSetIfChanged( ref badges, value, nameof( Badges ) );
        }
    }

    private Avalonia.Thickness margin;
    internal Avalonia.Thickness Margin
    {
        get { return margin; }
        set
        {
            this.RaiseAndSetIfChanged( ref margin, value, nameof( Margin ) );
        }
    }

    private double maxH;
    internal double Height
    {
        get
        {
            foreach (BadgeViewModel badge in Badges)
            {
                if (badge.BorderHeight > maxH)
                {
                    maxH = badge.BorderHeight;
                }
            }

            return maxH;
        }

        private set { maxH = value; }
    }


    internal BadgeLine(Core.DocumentProcessor.BadgeLine model, double scale)
    {
        Badges = new();

        foreach (Core.Models.Badge.Badge badge in model.Badges)
        {
            BadgeViewModel addable = new BadgeViewModel( badge );
            addable.SetCorrectScale( scale );

            Badges.Add( addable );
        }

        Margin =
            new Avalonia.Thickness( model.Margin.Left, model.Margin.Top, model.Margin.Right, model.Margin.Bottom );
        _scale = scale;
    }


    internal void ZoomOn(double scaleCoefficient)
    {
        _scale *= scaleCoefficient;
        Height *= scaleCoefficient;

        for (int index = 0; index < Badges.Count; index++)
        {
            Badges[index].ZoomOn( scaleCoefficient );
        }
    }


    internal void ZoomOut(double scaleCoefficient)
    {
        _scale /= scaleCoefficient;
        Height /= scaleCoefficient;

        for (int index = 0; index < Badges.Count; index++)
        {
            Badges[index].ZoomOut( scaleCoefficient );
        }
    }


    internal void Show()
    {
        for (int index = 0; index < Badges.Count; index++)
        {
            Badges[index].Show();
        }
    }


    internal void Hide()
    {
        for (int index = 0; index < Badges.Count; index++)
        {
            Badges[index].Hide();
        }
    }


    internal void Clear()
    {
        Badges.Clear();
    }
}
