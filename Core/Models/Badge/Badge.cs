using Core.DocumentBuilder;
using ExtentionsAndAuxiliary;
using System;
using System.Reflection.Metadata;

namespace Core.Models.Badge
{
    public class Badge
    {
        private static int _lastId;
        private static Dictionary<int, Badge> _backup = new ();

        public int Id { get; private set; }
        public Person Person { get; private set; }
        public string BackgroundImagePath { get; private set; }
        public Layout Layout { get; private set; }
        public bool IsCorrect { get; private set; }
        public bool IsChanged { get; private set; }


        private Badge ( ){}


        private Badge ( Person person, string backgroundImagePath, Layout layout )
        {
            Person = person;
            BackgroundImagePath = backgroundImagePath;
            Layout = layout;

            Dictionary<string, string> personProperties = Person.GetProperties ();

            Layout.SetUpTextFields ( personProperties );
            IsCorrect = ! Layout.HasIncorrectLines;
        }


        public static Badge ? GetBadge ( Person person, string backgroundImagePath, Layout layout )
        {
            bool isArgumentNull = ( person == null ) 
                                  ||
                                  backgroundImagePath == null
                                  ||
                                  layout == null;

            if ( isArgumentNull )
            {
                return null;
            }

            Badge result = new Badge ( person, backgroundImagePath, layout );
            result.Id = _lastId;
            _lastId++;

            return result;
        }


        public static void ClearCommonData ()
        {
            _backup = new ();
            _lastId = 0;
        }


        public void Split ( TextLine splitable )
        {
            Layout.Split ( splitable );
            IsChanged = true;
        }


        public void PrepareBackup ( LayoutComponent processableComponent )
        {
            if ( _backup.ContainsKey ( Id ) )
            {
                return;
            }

            Badge backup = new Badge ();
            backup.Person = Person;
            backup.BackgroundImagePath = BackgroundImagePath;
            backup.Layout = Layout.Clone(false);
            backup.IsChanged = false;
            backup.IsCorrect = IsCorrect;
            backup.Id = Id;
            backup.Layout.processableComponent = processableComponent;
            _backup.Add ( Id, backup );
        }


        public void CancelChanges ()
        {
            Badge backup = _backup [Id];
            Layout = backup.Layout.Clone(false);
            IsCorrect = backup.IsCorrect;
            IsChanged = false;
        }


        public void ShiftComponent ( string direction )
        {
            Layout.ShiftProcessable ( direction );
            IsChanged = true;
            IsCorrect = ! Layout.HasIncorrectLines;
        }


        public void MoveComponent ( LayoutComponent component, double verticalDelta, double horizontalDelta )
        {
            Layout.MoveComponent ( component, verticalDelta, horizontalDelta );
        }


        public void ResetProcessableContent ( string newContent )
        {
            

            //if ( changable.Content != newContent )
            //{
            //    IsChanged = true;
            //}

            //changable.ResetContent ( newContent );

            ResetProcessableContent ( newContent );
            //Layout.CheckProcessableLineCorrectness ();
        }



    }

}