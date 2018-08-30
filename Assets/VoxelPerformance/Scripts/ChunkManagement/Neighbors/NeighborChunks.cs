using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mel.Math;
using UnityEngine;

namespace Mel.VoxelGen
{

    public struct NeighborLookup6<T>
    {
        public IntVector3 center;
        Dictionary<IntVector3, T> storage;

        public int Count { get { return storage.Keys.Count; } }

        public NeighborLookup6(IntVector3 center)
        {
            this.center = center;
            storage = new Dictionary<IntVector3, T>(6);
        }

        public T Right { get { return Get(NeighborDirection.Right); } }
        public T Left { get { return Get(NeighborDirection.Left); } }
        public T Up { get { return Get(NeighborDirection.Up); } }
        public T Down { get { return Get(NeighborDirection.Down); } }
        public T Forward { get { return Get(NeighborDirection.Forward); } }
        public T Back { get { return Get(NeighborDirection.Back); } }
        public T Center { get { return this[center]; } }

        public bool Contains(IntVector3 globalChunkPos) { return storage.ContainsKey(globalChunkPos); }

        public bool Contains(NeighborDirection nd) { return storage.ContainsKey(center + CubeNeighbors6.Relative(nd)); }

        public bool ContainsCenter { get { return Contains(center); } }

        public T Get(NeighborDirection nd) { return this[nd]; }
        public void Set(NeighborDirection nd, T item) { this[nd] = item; }

        public bool Get(IntVector3 candidate, out T item)
        {
            if (candidate.Equal(center) || CubeNeighbors6.IsNeighbor(center, candidate))
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
            if(CubeNeighbors6.IsNeighbor(center,candidate) || candidate.Equal(center))
            {
                if(storage.ContainsKey(candidate)) { storage[candidate] = item; }
                else { storage.Add(candidate, item); }
                return true;
            }
            item = default(T);
            return false;
        }

        public T this[NeighborDirection nd] {
            get {
                return this[center + CubeNeighbors6.Relative(nd)];
            }
            set {
                this[center + CubeNeighbors6.Relative(nd)] = value;
            }
        }

        public T this[int i] {
            get {
                return this[center + CubeNeighbors6.Relative(i)];
            }
            set {
                this[center + CubeNeighbors6.Relative(i)] = value;
            }
        }

        public T this[IntVector3 key] {
            get {
                if (storage.ContainsKey(key)) { return storage[key]; }
                return default(T);
            }
            set {
                if (storage.ContainsKey(key)) {
                    storage[key] = value;
                }
                else if (center.Equal(key) || CubeNeighbors6.IsNeighbor(center, key)) { 
                        storage.Add(key, value);
                }
            }
        }

        public IEnumerable<T> GetNeighbors {
            get {
                for (int i = 0; i < 6; ++i) { yield return this[i]; }
            }
        }

        public IEnumerable<IntVector3> GetKeys {
            get {
                foreach(var key in storage.Keys) { yield return key; }
            }
        }

        public struct NeighborDataInDirection
        {
            public T item;
            public NeighborDirection direction;
        }

        public IEnumerable<NeighborDataInDirection> GetNeighborDataCoords {
            get {
                for(int i = 0; i < 6; ++i)
                {
                    var neigh = this[i];
                    yield return new NeighborDataInDirection
                    {
                        item = neigh,
                        direction = (NeighborDirection)i
                    };
                }
            }
        }
    }

    public class NeighborChunkGenData
    {
        public ChunkGenData centerChunkData {
            get {
                ChunkGenData cgend;
                neighbors.Get(neighbors.center, out cgend);
                return cgend;
            }
            set {
                Add(center, value);
            }
        }

        public Neighbors27<ChunkGenData> neighbors { get; private set; }
        //NeighborLookup6<ChunkGenData> neighbors;
        //public Action<NeighborLookup6<ChunkGenData>> OnCompleteSet;
        HashSet<NeighborDirection> ignorableDirections = new HashSet<NeighborDirection>();

        public NeighborChunkGenData(IntVector3 center) 
        {
            neighbors = new Neighbors27<ChunkGenData>(center); // new NeighborLookup6<ChunkGenData>(center);
        }

        public IntVector3 center {
            get { return neighbors.center; }
        }

        public ChunkGenData Get(NeighborDirection nd)
        {
            return neighbors[nd];
        }

        public ChunkGenData Get(IntVector3 pos)
        {
            return neighbors[pos];
        }

        public async Task<NeighborChunkGenData> GatherDataAsync(Func<IntVector3, Task<ChunkGenData>> GetChunkAtAsync)
        {
            foreach(var chunkPos in CubeNeighbors6.NeighborsOfIncludingCenter(center))
            {
                neighbors.Set(chunkPos, await GetChunkAtAsync(chunkPos));
            }
            return this;
        }

        public void GatherData(Func<IntVector3, ChunkGenData> GetChunk)
        {
            foreach(var chunkPos in CubeNeighbors6.NeighborsOfIncludingCenter(center))
            {
                neighbors.Set(chunkPos, GetChunk(chunkPos));
            }
        }

        public void GatherData27(Func<IntVector3, ChunkGenData> GetChunk)
        {
            foreach(var chunkPos in Cube27.NeighborsOfIncludingCenter(center))
            {
                neighbors.Set(chunkPos, GetChunk(chunkPos));
            }
        }

        //public NeighborChunkGenData(ChunkGenData centerChunkData)
        //{
        //    this.centerChunkData = centerChunkData;
        //    neighbors = new NeighborLookup6<ChunkGenData>(centerChunkData.chunkPos);
        //}

        public int Count { get { return neighbors.Count; } }

        public void AddIgnoreDirection(NeighborDirection nd) { ignorableDirections.Add(nd); }

        //public bool isCompleteSet {
        //    get {
        //        foreach(var nd in CubeNeighbors6.GetDirections)
        //        {
        //            if(!ignorableDirections.Contains(nd) && !neighbors.Contains(nd)) { return false; }
        //        }
        //        if(!neighbors.ContainsCenter) { return false; }
        //        return true;
        //    }
        //}

        public void AddIfEmpty(IntVector3 globalChunkPos, ChunkGenData data)
        {
            ChunkGenData already;
            if(neighbors.Get(globalChunkPos, out already))
            {
                return;
            }
            Add(globalChunkPos, data);
        }

        public void Add(IntVector3 globalChunkPos, ChunkGenData data)
        {
            if(neighbors.Set(globalChunkPos, data))
            {
                //if (OnCompleteSet != null && isCompleteSet)
                //{
                //    Debug.Log("will call On Complete: " + centerChunkData.chunkPos.ToString());
                //    OnCompleteSet(neighbors);
                //}
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
