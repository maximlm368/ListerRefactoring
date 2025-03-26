namespace Lister.Desktop.ModelMappings;

public sealed class TemplateName
{
    public string Name { get; private set; }
    public bool IsFound { get; private set; }


    public TemplateName(string name, bool isFound)
    {
        Name = name;
        IsFound = isFound;
    }
}