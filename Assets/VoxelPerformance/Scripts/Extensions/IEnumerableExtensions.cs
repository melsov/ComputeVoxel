using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mel.Util
{
    public static class IEnumerableExtensions
    {
        public static string ToDelimitedString(this IEnumerable e, string delimiter = ",")
        {
            StringBuilder sb = new StringBuilder();
            foreach(var v in e)
            {
                sb.Append(v.ToString());
                sb.Append(delimiter);
            }
            return sb.ToString();
        }
    }
}
