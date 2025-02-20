using Avalonia.Controls;
using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lister.Views
{
    internal class CustomTextBox : TextBox
    {
        public CustomTextBox () : base () { }

        protected override void OnKeyDown ( KeyEventArgs args )
        {
            System.Diagnostics.Debug.WriteLine (args.Key);
            base.OnKeyDown (args);
        }


    }
}
