using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mel.Math;

namespace Mel.VoxelGen
{
    public class TargetCenteredChunkBounds
    {
        public Transform target;
        public IntVector3 size;
        public VGenConfig vGenConfig;
        ProximityIterator3 iterator;
        public Action moveMethod;

        public TargetCenteredChunkBounds(Transform target, IntVector3 size, IntVector3 boundsSize, VGenConfig vg, Action moveMethod = null)
        {
            this.target = target;
            this.size = size;
            vGenConfig = vg;
            iterator = new ProximityIterator3(size, boundsSize);
            if(moveMethod == null)
            {
                this.moveMethod = () =>
                {
                    target.position += new IntVector3(size.x * boundsSize.x, 0, 0).ToVector3();
                };
            } else
            {
                this.moveMethod = moveMethod;
            }
        }

        public IntVector3 chunkPos {
            get {
                return vGenConfig.PosToChunkPos(IntVector3.FromVector3(target.position));
            }
        }

        public IntBounds3 bounds {
            get {
                return IntBounds3.FromCenterHalfSize(chunkPos, size / 2);
            }
        }

       

        public IntVector3 Next()
        {
            if(!iterator.HasNext)
            {
                moveMethod();
                iterator.Reset();
            }
            IntVector3 next;
            iterator.Next(out next);
            return chunkPos + next;
        }
    }
}
