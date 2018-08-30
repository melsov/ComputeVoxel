using Mel.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Mel.VoxelGen
{
    //
    // Inscrutable errors when using a >1 size pool in ChunkDomain
    //
    public class HopefullySaferForgePool : ForgePool
    {
        public override int maxSize {
            get {
                return 1;
            }
        }

        public HopefullySaferForgePool(int unusedMaxSize, ChunkForge proto) : base(1, proto)
        {
        }
    }

    public class ForgePool
    {
        protected List<ChunkForge> pool = new List<ChunkForge>();

        public virtual int maxSize { get; private set; }

        public ForgePool(int maxSize, ChunkForge proto)
        {
            this.maxSize = maxSize;
            pool.Add(proto);
        }

        public bool Busy {
            get {
                return pool.Count == maxSize && allBusy();
            }
        }

        public bool UnPack(IntVector3 chunkPos, Action<Chunk> onChunkDone)
        {
            var forge = GetAvailableOrAdd();
            if(forge == null) { return false; }

            //forge.UnPack(chunkPos, onChunkDone);
            forge.Forge(chunkPos, onChunkDone);

            return true;
        }

        ChunkForge GetAvailableOrAdd()
        {
            foreach(var forge in pool)
            {
                if(!forge.Busy) { return forge; }
            }

            if(pool.Count < maxSize)
            {
                var next = UnityEngine.Object.Instantiate(pool[0]);
                pool.Add(next);
                return next;
            }
            return null;
        }

        private bool allBusy()
        {
            foreach(var forge in pool)
            {
                if(!forge.Busy) { return false; }
            }
            return true;
        }


    }
}
