using Lister.Core.Entities.Badge;

namespace Lister.Desktop.ModelMappings.BadgeVM;

/// <summary>
/// Represents base for movable badge component that geometrically linked with some textline.
/// </summary>
public abstract class BoundToTextLineBase : BadgeComponentBase
{
    public int Id { get; protected set; }

    private string? _binding;
    public string? Binding
    {
        get 
        { 
            return _binding; 
        }

        protected set
        {
            _binding = value;
            OnPropertyChanged ();
        }
    }

    private bool _isAboveOfBinding;
    public bool IsAboveOfBinding
    {
        get 
        { 
            return _isAboveOfBinding; 
        }

        protected set
        {
            _isAboveOfBinding = value;
            OnPropertyChanged ();
        }
    }

    protected void HandleModelChanged ( LayoutComponentBase model )
    {
        SetUp ( model.Width, model.Height, model.TopOffset, model.LeftOffset );
    }
}
