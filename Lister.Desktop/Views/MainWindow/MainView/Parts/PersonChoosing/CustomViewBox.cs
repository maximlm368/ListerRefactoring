using Avalonia;
using Avalonia.Controls;

namespace View.MainWindow.MainView.Parts.PersonChoosing;

public class CustomViewbox : Viewbox
{
    private bool _isScrollable = false;

    public static readonly DirectProperty<CustomViewbox, bool> IsScrollableProperty =
         AvaloniaProperty.RegisterDirect<CustomViewbox, bool>
         (
            nameof( IsScrollable ),
            o => o.IsScrollable,
            (o, v) => o.IsScrollable = v
         );

    public bool IsScrollable
    {
        get { return _isScrollable; }
        set { SetAndRaise( IsScrollableProperty, ref _isScrollable, value ); }
    }


    public CustomViewbox() : base() { }

}
