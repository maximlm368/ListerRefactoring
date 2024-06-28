using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Lister.ViewModels
{
    internal class DialogViewModel : ViewModelBase
    {
        public ReactiveCommand <Unit, string ?> ChooseYes { get; }
        public ReactiveCommand <Unit, string ?> ChooseNo { get; }


        public DialogViewModel ()
        {
            ChooseYes = ReactiveCommand.Create (() =>
            {
                return "Yes";
            });

            ChooseNo = ReactiveCommand.Create (() =>
            {
                return "No";
            });
        }


    }
}
