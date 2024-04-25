using ContentAssembler;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static QuestPDF.Helpers.Colors;

namespace Lister.ViewModels
{
    internal class PersonSourceFileChoosingUCViewModel : ViewModelBase
    {
        private IUniformDocumentAssembler uniformAssembler;
        internal List<VMPerson> people { get; private set; }
        internal string sourceFilePath
        {
            //get { return personsSourceFilePath; }
            set
            {
                SetPersonsFilePath (value);
                OnPropertyChanged ("sourceFilePath");
            }
        }

        private ObservableCollection<VMPerson> visibleP;
        internal ObservableCollection<VMPerson> visiblePeople
        {
            get { return visibleP; }
            set
            {
                this.RaiseAndSetIfChanged (ref visibleP, value, nameof (visiblePeople));
            }
        }


        internal PersonSourceFileChoosingUCViewModel ( IUniformDocumentAssembler singleTypeDocumentAssembler ) 
        {
            visiblePeople = new ObservableCollection<VMPerson> ();
            people = new List<VMPerson> (); 
            this.uniformAssembler = singleTypeDocumentAssembler;
            
        }


        internal event PropertyChangedEventHandler PropertyChanged;


        protected virtual void OnPropertyChanged ( string propertyName )
        {
            if ( PropertyChanged != null )
            {
                PropertyChanged?.Invoke (this, new System.ComponentModel.PropertyChangedEventArgs (propertyName));
            }
            //else
            //{
            //    throw new Exception ("empty delegate in mainViewModel  " + propertyName);
            //}
        }


        private void SetPersonsFilePath ( string value )
        {
            bool valueIsSuitable = value != string.Empty && value != null;

            if ( valueIsSuitable )
            {
                value = value.Substring (8, value.Length - 8);

                visiblePeople.Clear ();
                people.Clear ();
                var persons = this.uniformAssembler.GetPersons (value);

                foreach ( var person in persons )
                {
                    VMPerson vmPerson = new VMPerson (person);
                    visiblePeople.Add (vmPerson);
                    people.Add (vmPerson);
                }
            }
        }


    }
}
