using Lister.Desktop.ModelMappings.BadgeVM;

namespace Lister.Desktop.Extentions;

internal class BadgeComparer : IComparer<BadgeViewModel>
{
    public int Compare ( BadgeViewModel? x, BadgeViewModel? y )
    {
        BadgeViewModel? first = x as BadgeViewModel;
        BadgeViewModel? second = y as BadgeViewModel;

        if ( first == null && second != null )
        {
            return -1;
        }

        if ( first != null && second == null )
        {
            return 1;
        }

        if ( first == null && second == null )
        {
            return 0;
        }
#pragma warning disable CS8602 // Разыменование вероятной пустой ссылки.
        string xStringPresentation = first.Model.Person.FullName;
        string yStringPresentation = second.Model.Person.FullName;
#pragma warning disable CS8602 // Разыменование вероятной пустой ссылки.

        return string.Compare ( xStringPresentation, yStringPresentation );
    }
}
