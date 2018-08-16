using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mel.Math;

namespace Mel.FakeData
{
    public class FakeChunkData
    {
        public static uint[] Stairs(IntVector3 size)
        {
            List<uint> result = new List<uint>(size.Area);

            foreach(var vi in size.IteratorXYZ)
            {
                if(vi.v.x + vi.v.y < size.x)
                {
                    result.Add(vi.v.ToVoxel(1));
                }
            }

            return result.ToArray();
        }
    }
}
