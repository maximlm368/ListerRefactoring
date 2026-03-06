using Lister.Core.BadgesCreator.AbstractComponents;
using Lister.Core.Entities;
using Lister.Core.Entities.Badge;

namespace Lister.Core.BadgesCreator;

/// <summary>
/// Single class creating Badge class instance for person using template.
/// </summary>
internal sealed class BadgeCreator
{
    private static BadgeCreator? _instance;
    private static IBadgeLayoutProvider? _badgeLayoutProvider;

    private Layout? _badgeLayout;
    private string? _backgroundPath;
    private string? _template;

    private BadgeCreator ( IBadgeLayoutProvider badgeLayoutProvider )
    {
        _badgeLayoutProvider ??= badgeLayoutProvider;
    }

    internal static BadgeCreator GetInstance ( IBadgeLayoutProvider badgeLayoutProvider )
    {
        _instance ??= new BadgeCreator ( badgeLayoutProvider );

        return _instance;
    }

    internal Badge? CreateBadgeByTemplate ( string? templateName, Person? person )
    {
        if ( string.IsNullOrWhiteSpace ( templateName ) || person == null )
        {
            return null;
        }

        if ( _template != templateName )
        {
            _badgeLayout = _badgeLayoutProvider?.GetBadgeLayout ( templateName );
            _backgroundPath = _badgeLayoutProvider?.GetBadgeImageUri ( templateName );
            _template = templateName;
        }

        return ( _backgroundPath != null && _badgeLayout != null )
            ? Badge.GetBadge ( person, _backgroundPath, _badgeLayout.Clone ( true ) )
            : null;
    }
}
