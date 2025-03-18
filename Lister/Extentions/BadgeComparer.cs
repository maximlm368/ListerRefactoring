using View.CoreModelReflection.Badge;

namespace View.Extentions;

internal class BadgeComparer : IComparer <BadgeViewModel>
{
    public int Compare ( BadgeViewModel ? x, BadgeViewModel ? y )
    {
        if ((x==null)  ||  (y==null)) 
        {
            return 0;
        }

        string xStringPresentation = x.Model.Person.FullName;
        string yStringPresentation = y.Model.Person.FullName;

        return string.Compare (xStringPresentation, yStringPresentation);
    }
}