using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mel.Math;

namespace Mel.VoxelGen
{

    public struct NeighborLookup6<T>
    {
        public IntVector3 center;
        Dictionary<IntVector3, T> storage;

        public NeighborLookup6(IntVector3 center)
        {
            this.center = center;
            storage = new Dictionary<IntVector3, T>(6);
        }

        public T Right { get { return Get(NeighborDirections.Right); } }
        public T Left { get { return Get(NeighborDirections.Left); } }
        public T Up { get { return Get(NeighborDirections.Up); } }
        public T Down { get { return Get(NeighborDirections.Down); } }
        public T Forward { get { return Get(NeighborDirections.Forward); } }
        public T Back { get { return Get(NeighborDirections.Back); } }

        public IntVector3 relative(int i)
        {
            switch (i)
            {
                case 0: default: return IntVector3.right;
                case 1: return IntVector3.left;
                case 2: return IntVector3.up;
                case 3: return IntVector3.down;
                case 4: return IntVector3.forward;
                case 5: return IntVector3.back;
            }
        }

        public T Get(NeighborDirections nd) { return this[(int)nd]; }
        public void Set(NeighborDirections nd, T item) { this[(int)nd] = item; }

        public bool Get(IntVector3 candidate, out T item)
        {
            if (CubeNeighbors6.IsNeighbor(center, candidate))
            {
                if (storage.ContainsKey(candidate))
                {
                    item = storage[candidate];
                    return true;
                }
            }
            item = default(T);
            return false;
        }

        public bool Set(IntVector3 candidate, T item)
        {
            if(CubeNeighbors6.IsNeighbor(center,candidate))
            {
                if(storage.ContainsKey(candidate)) { storage[candidate] = item; }
                else { storage.Add(candidate, item); }
            }
            item = default(T);
            return false;
        }

        public T this[int i] {
            get {
                var key = center + relative(i);
                if(storage.ContainsKey(key)) { return storage[key]; }
                return default(T);
            }
            set {
                var key = center + relative(i);
                if(storage.ContainsKey(key)) { storage[key] = value; }
                else { storage.Add(key, value); }
            }
        }

        public IEnumerable<T> GetNeighbors {
            get {
                for (int i = 0; i < 6; ++i) { yield return this[i]; }
            }
        }
    }

    public class NeighborChunks
    {
        public Chunk chunk { get; private set; }
        NeighborLookup6<Chunk> neighbors;

        public IntVector3 center { get { return neighbors.center; } }

        public struct NeighborPair
        {
            public Chunk center, neighbor;
        }

        public Action<NeighborPair> OnAddedNeighbor;

        public NeighborChunks(Chunk chunk)
        {
            this.chunk = chunk;
            neighbors = new NeighborLookup6<Chunk>(chunk.ChunkPos);
        }

        public void Add(Chunk neighbor)
        {
            if(!neighbors.Set(neighbor.ChunkPos, neighbor)) { return; }

            if (OnAddedNeighbor == null) { return; }

            OnAddedNeighbor(new NeighborPair
            {
                center = chunk,
                neighbor = neighbor
            });

        }
    }
}
