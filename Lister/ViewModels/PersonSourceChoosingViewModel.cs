using ContentAssembler;
using Lister.Views;
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
    internal class PersonSourceChoosingViewModel : ViewModelBase
    {
        private IUniformDocumentAssembler uniformAssembler;
        internal List<Person> people { get; private set; }
        
        private string sFP;
        internal string sourceFilePath
        {
            get { return sFP; }
            set
            {
                string path = SetPersonsFilePath ( value );
                this.RaiseAndSetIfChanged ( ref sFP , path , nameof ( sourceFilePath ) );
            }
        }

        private ObservableCollection<Person> visibleP;
        internal ObservableCollection<Person> visiblePeople
        {
            get { return visibleP; }
            set
            {
                this.RaiseAndSetIfChanged (ref visibleP, value, nameof (visiblePeople));
            }
        }


        internal PersonSourceChoosingViewModel ( IUniformDocumentAssembler singleTypeDocumentAssembler ) 
        {
            visiblePeople = new ObservableCollection<Person> ();
            people = new List<Person> (); 
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


        internal string SetPersonsFilePath ( string value )
        {
            bool valueIsSuitable = ( value != null ) && ( value != string.Empty );

            if ( valueIsSuitable )
            {
                visiblePeople.Clear ( );
                people.Clear ( );

                try
                {
                    List<Person> persons = uniformAssembler.GetPersons ( value );

                    foreach ( var person in persons )
                    {
                        visiblePeople.Add ( person );
                        people.Add ( person );
                    }

                    return value;
                }
                catch ( IOException ex )
                {
                    int idOk = Winapi.MessageBox ( 0 , "Выбраный файл открыт в другом приложении. Закройте его." , "" , 0 );
                    return string.Empty;
                }
            }

            return string.Empty;
        }
    }
}
