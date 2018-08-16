using System;
using System.Collections.Generic;
using System.Linq;

namespace Mel.Math
{
    public static class SumComputer
    {
        public static float Sum(int n, Func<int, float> formula)
        {
            return Sum(Enumerable.Range(0, n), formula);
        }

        public static float Sum(IEnumerable<int> iterator, Func<int, float> formula)
        {
            float result = 0;
            foreach(int i in iterator) { result += formula(i); }
            return result;
        } 
    }
}
