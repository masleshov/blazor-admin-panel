using Radzen;
using System;
using System.Linq;
using System.Threading.Tasks;

public static class SortDescription
{

    public static string ConvertToStringDescriptor(this SortDescriptor sortDescriptor)
    {
        if (sortDescriptor == null || sortDescriptor.SortOrder == null) return null;

        if (sortDescriptor.SortOrder.Value == SortOrder.Descending)
        {
            return string.Concat('-', sortDescriptor.Property);
        }
        else
        {
            return sortDescriptor.Property;
        }
    }  


}