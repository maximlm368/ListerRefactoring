using Avalonia.Media;
using ContentAssembler;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lister.ViewModels
{
    internal class VisiblePerson : ViewModelBase
    {
        //internal int Id { get; private set; }
        internal Person Person { get; private set; }

        private SolidColorBrush bC;
        internal SolidColorBrush BorderBrushColor
        {
            get { return bC; }
            set
            {
                this.RaiseAndSetIfChanged (ref bC, value, nameof (BorderBrushColor));
            }
        }

        private SolidColorBrush bgC;
        internal SolidColorBrush BackgroundBrushColor
        {
            get { return bgC; }
            set
            {
                this.RaiseAndSetIfChanged (ref bgC, value, nameof (BackgroundBrushColor));
            }
        }


        internal VisiblePerson ( Person person ) 
        {
            //Id = id;
            Person = person;
            BorderBrushColor = new SolidColorBrush (new Color (255, 255, 255, 255));
            BackgroundBrushColor = new SolidColorBrush (new Color (255, 255, 255, 255));
        }
    }
}
