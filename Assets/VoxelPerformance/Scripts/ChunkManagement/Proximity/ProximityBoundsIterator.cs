using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mel.Math
{


    public struct ProximityBoundsIterator
    {
        public IntBounds3 cursor;
        public Func<IntVector3> getCenter;

        public ProximityIterator3 iterator;
    }
}
