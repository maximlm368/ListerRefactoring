using Avalonia;
using Avalonia.Controls;

namespace View.MainWindow.MainView.Parts.PersonChoosing;

public class CustomViewbox : Viewbox
{
    public static readonly DirectProperty<CustomViewbox, bool> IsScrollableProperty =
         AvaloniaProperty.RegisterDirect<CustomViewbox, bool>
         (
            nameof( IsScrollable ),
            o => o.IsScrollable,
            (o, v) => o.IsScrollable = v
         );

    private bool _isScrollable = false;

    public bool IsScrollable
    {
        get { return _isScrollable; }
        set { SetAndRaise( IsScrollableProperty, ref _isScrollable, value ); }
    }


    public CustomViewbox() : base() { }

}
