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
        internal SolidColorBrush BrushColor
        {
            get { return bC; }
            set
            {
                this.RaiseAndSetIfChanged (ref bC, value, nameof (BrushColor));
            }
        }


        internal VisiblePerson ( Person person ) 
        {
            //Id = id;
            Person = person;
            BrushColor = new SolidColorBrush (new Color (255, 255, 255, 255));
        }
    }
}
