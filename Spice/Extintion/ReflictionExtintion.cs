using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.Extintion
{
    public static class ReflictionExtintion
    {
        public static string GetPropertyValue<T>(this T item, String propertyName)
        {
            return item.GetType().GetProperty(propertyName).GetValue(item, null).ToString();
        }
    }
}
