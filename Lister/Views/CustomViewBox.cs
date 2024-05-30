using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Lister.Views
{
    public class CustomViewbox : Viewbox
    {
        public static readonly DirectProperty<CustomViewbox, bool> IsScrollableProperty =
             AvaloniaProperty.RegisterDirect<CustomViewbox, bool>
             (
                nameof (IsScrollable),
                o => o.IsScrollable,
                ( o, v ) => o.IsScrollable = v
             );

        private bool _isScrollable = false;

        public bool IsScrollable
        {
            get { return _isScrollable; }
            set { SetAndRaise (IsScrollableProperty, ref _isScrollable, value); }
        }


        public CustomViewbox () : base () { }

    }
}
