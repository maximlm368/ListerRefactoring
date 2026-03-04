using Lister.Core.Entities;

namespace Lister.Desktop.ModelMappings.BadgeVM;

/// <summary>
/// Is Person wraper, to get Person.FullName from ToString method.
/// </summary>
internal sealed partial class VisiblePerson
{
    internal Person Model { get; private set; }

    internal VisiblePerson ( Person person )
    {
        Model = person;
    }

    public override string ToString ()
    {
        return Model.FullName;
    }
}
