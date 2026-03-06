using Lister.Core.Entities.Badge;

namespace Lister.Core.BadgesCreator.AbstractComponents;

/// <summary>
/// Define abstraction implementation of wich returns layout (in state without personal details) for template.
/// </summary>
public interface IBadgeLayoutProvider
{
    public string GetBadgeImageUri ( string templateName );

    public Layout? GetBadgeLayout ( string templateName );
}