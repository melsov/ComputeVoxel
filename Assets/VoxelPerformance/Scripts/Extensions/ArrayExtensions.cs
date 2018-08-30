using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mel.Extensions
{
    public static class ArrayExtensions
    {
        public static string ElementsToString<T>(this T[] ray, string delimeter = ",")
        {
            return string.Join(delimeter, ray);
        }
    }
}
