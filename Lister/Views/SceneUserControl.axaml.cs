using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.LogicalTree;
using Lister.ViewModels;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using System.Reactive.Subjects;

namespace Lister.Views
{
    public partial class SceneUserControl : UserControl
    {
        private SceneViewModel _vm;


        public SceneUserControl ()
        {
            InitializeComponent ();
            DataContext = App.services.GetRequiredService<SceneViewModel> ();
            this.Margin = new Avalonia.Thickness (5);


            //var source = new Subject<string> ();
            //var label = new Label ();

            //// Bind TextBlock.Text to source
            //var subscription = label.Bind (Canvas.TopProperty, source);

            //// Set textBlock.Text to "hello"
            //source.OnNext ("hello");
            //// Set textBlock.Text to "world!"
            //source.OnNext ("world!");

            //// Terminate the binding
            //subscription.Dispose ();




            IEnumerable<Control> children = lines.GetTemplateChildren ();

            foreach ( Control child in children )
            {
                ItemsControl items = child as ItemsControl;
                IEnumerable<Control> badges = items.GetTemplateChildren ();


                foreach ( Control badge   in   badges )
                {
                    Border border = child as Border;
                    Canvas canvas = border.Child as Canvas;
                    
                    

                }

            }

        }

    }
}



//internal void PassNeighbours ( PersonSourceUserControl personSource, PersonChoosingUserControl personChoosing
//                             , ZoomNavigationUserControl zoomNavigation, TemplateChoosingUserControl templateChoosing )
//{
//    _personChoosing = personChoosing;
//    _personSource = personSource;
//    _zoomNavigation = zoomNavigation;
//    _templateChoosing = templateChoosing;


//}