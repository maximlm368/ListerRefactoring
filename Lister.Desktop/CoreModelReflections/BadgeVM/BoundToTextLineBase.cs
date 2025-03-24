using Lister.Core.Models.Badge;
using ReactiveUI;

namespace Lister.Desktop.CoreModelReflections.BadgeVM;

public abstract class BoundToTextLineBase : BadgeComponentBase
{
    public int Id { get; protected set; }

    private string? _binding;
    public string? Binding
    {
        get { return _binding; }
        protected set
        {
            this.RaiseAndSetIfChanged ( ref _binding, value, nameof ( Binding ) );
        }
    }

    private bool _isAboveOfBinding;
    public bool IsAboveOfBinding
    {
        get { return _isAboveOfBinding; }
        protected set
        {
            this.RaiseAndSetIfChanged ( ref _isAboveOfBinding, value, nameof ( IsAboveOfBinding ) );
        }
    }


    protected void HandleModelChanged ( LayoutComponentBase model )
    {
        SetUp ( model.Width, model.Height, model.TopOffset, model.LeftOffset );
    }
}