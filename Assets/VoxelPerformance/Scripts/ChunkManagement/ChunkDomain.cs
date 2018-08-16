using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mel.Math;
using Mel.Storage;
using Mel.VoxelGen;
using System.Linq;
using System;
using VoxelPerformance;
using UnityEditor;

public class ChunkDomain : MonoBehaviour {

    [SerializeField]
    IntVector3 halfSize;

    [SerializeField]
    IntVector3 preserveBorderWidth;

    [SerializeField]
    Transform player;


    AddChunkFinder addChunkFinder;
    ChunkAddQueue chunkAddQueue {
        get { return addChunkFinder.chunkAddQueue; }
    }
    LiveChunks liveChunks;

    [SerializeField]
    ChunkForge forge;

    //[SerializeField]
    //ReverseCastBuffer reverseCastBuffer;

    VGenConfig _vg;
    VGenConfig vGenConfig {
        get { if(!_vg) { _vg = FindObjectOfType<VGenConfig>(); } return _vg; }
    }


    IntBounds3 preserveDomain {
        get {
            return addChunkFinder.domain.ExpandBordersAdditive(preserveBorderWidth);
        }
    }

    int MaxChunks {
        get { return preserveDomain.size.Area; }
    }


    private void Awake()
    {
        liveChunks = new LiveChunks();
        addChunkFinder = new AddChunkFinder(halfSize, player, liveChunks, null);
    }

    private void Update()
    {
        CurateDomain();
        TestManualRemove();
        CheckKeys();
    }

    private void CheckKeys()
    {
        if(Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.X))
        {
            PrepareToQuit();
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    void PrepareToQuit()
    {
        if(liveChunks.SerializeAll())
        {
            vGenConfig.IsSerializedDataValid = true;
        }
    }

    private void TestManualRemove()
    {
        if(Input.GetKeyDown(KeyCode.K))
        {
            liveChunks.TestRemove(new IntVector3(0));
        }
        else if (Input.GetKeyDown(KeyCode.L)){
            if(forge.UnPack(new IntVector3(0), OnChunkIsDone))
            {
                Debug.Log("will add");
            } else
            {
                Debug.Log("forge busy");
            }
        }
    }

    IntVector3 center {
        get {
            return IntVector3.FromVector3(player.position);
        }
    }

    bool Intersects(Bounds b)
    {
        Vector3 nearest = b.ClosestPoint(player.position);
        IntVector3 dif = IntVector3.FromVector3(player.position - nearest).Abs;
        return dif < halfSize;
    }

    void CurateDomain()
    {
        IntVector3 addChunkPos;
        if(addChunkFinder.Next(out addChunkPos))
        {
            chunkAddQueue.Add(addChunkPos);
        }
        IntVector3 forgeChunkPos;
        if(!forge.Busy && addChunkFinder.NextStartBuild(out forgeChunkPos)) // chunkAddQueue.Next(out forgeChunkPos))
        {
            forge.UnPack(forgeChunkPos, OnChunkIsDone);
        }

        var remove = liveChunks.NextRemovable(vGenConfig.PosToChunkPos(center), MaxChunks);
        if (remove != null)
        {
            liveChunks.Remove(remove);
        }

    }

    

    private void OnChunkIsDone(Chunk c)
    {
        if(c == null)
        {
            return;
        }
        liveChunks.Add(c);
        //reverseCastBuffer.SetCaster(c);

        Debug.Log("chunk done" + c.ToString());
    }

    class LiveChunks
    {
        Dictionary<IntVector3, NeighborChunks> lookup = new Dictionary<IntVector3, NeighborChunks>();

        public void Add(Chunk c)
        {
            if (lookup.Keys.Contains(c.ChunkPos))
            {
                //TODO: destroy/clean-up current chunk at chunkPos
            }

            NeighborChunks nc = new NeighborChunks(c);
            nc.OnAddedNeighbor = HandleNewNeighbors;
            if (!lookup.ContainsKey(c.ChunkPos))
            {
                lookup.Add(c.ChunkPos, nc);
            } else
            {
                lookup[c.ChunkPos] = nc;
            }

            alertNeighbors(lookup[c.ChunkPos]);
        }

        public bool IsLive(IntVector3 chunkPos)
        {
            if (lookup.ContainsKey(chunkPos))
            {
                return lookup[chunkPos] != null;
            }
            return false;
        }

        public void HandleNewNeighbors(NeighborChunks.NeighborPair pair)
        {
            // TODO: decide whether we need this
        }

        private void alertNeighbors(NeighborChunks neighborChunks)
        {
            foreach(var neiPos in new CubeNeighbors6 { center = neighborChunks.center }.GetNeighbors)
            {
                if (lookup.ContainsKey(neiPos))
                {
                    lookup[neiPos].Add(neighborChunks.chunk);
                    neighborChunks.Add(lookup[neiPos].chunk);
                }
            }
        }

        public Chunk NextRemovable(IntVector3 playerChunkPos, int maxAllowed)
        {
            if(lookup.Count < maxAllowed)
            {
                return null;
            }
            IntVector3 key;
            if( FindFurthestKey(playerChunkPos, out key))
            {
                return lookup[key].chunk;

            }

            return null;
        }

        private bool FindFurthestKey(IntVector3 playerChunkPos, out IntVector3 key)
        {
            int distSq = 0;
            key = default(IntVector3);

            foreach(var k in lookup.Keys)
            {
                int dq = (playerChunkPos - k).SquareMagnitude;
                if(dq > distSq)
                {
                    distSq = dq;
                    key = k;
                }
            }

            return distSq > 0;
        }

        public bool Remove(Chunk c)
        {
            try
            {
                c.SerializeAndDestory();
                lookup.Remove(c.ChunkPos);
            } catch (Exception e)
            {
                Debug.LogWarning("Exception while serializing: " + e.ToString());
                return false;
            }

            return true;
        }

        internal void TestRemove(IntVector3 chunkPos)
        {
            if (lookup.ContainsKey(chunkPos))
            {
                Remove(lookup[chunkPos].chunk);
            } 
        }

        internal bool SerializeAll()
        {
            try
            {
                foreach (var c in lookup.Values)
                {
                    c.chunk.Serialize();
                }
            } catch(Exception e)
            {
                Debug.LogWarning("Serialize Chunk Exception " + e.ToString());
                return false;
            }

            return true;
        }
    }

    class ChunkAddQueue
    {
        HashSet<IntVector3> storage = new HashSet<IntVector3>();
        private ProximityOrderCoords pCoords;

        public ChunkAddQueue(int size = 16)
        {
            pCoords = new ProximityOrderCoords(size);
        }

        public void Add(IntVector3 cpos)
        {
            storage.Add(cpos);
        }

        public bool Contains(IntVector3 cpos) { return storage.Contains(cpos); }

        public bool Next(IntVector3 center, out IntVector3 cpos)
        {
            cpos = new IntVector3(0);
            if(storage.Count > 0)
            {
                if(Closest(center, out cpos))
                {
                    return true;
                }
            }
            return false;
        }

        bool Closest(IntVector3 center, out IntVector3 result)
        {
            foreach(var c in pCoords.Iterator)
            {
                var subject = center + c;
                if (storage.Remove(subject))
                {
                    result = subject;
                    return true;
                }
            }
            result = default(IntVector3);
            return false;
        }
    }

    class AddChunkFinder
    {
        IntVector3 _halfSize;
        int fakeIndex;
        private LiveChunks lookup;
        public Transform center;

        IntVector3 currentCenter;
        ProximityIterator3 iterator;

        public ChunkAddQueue chunkAddQueue { get; private set; }

        VGenConfig _vg;
        private VGenConfig vGenConfig {
            get {
                if(!_vg) { _vg = FindObjectOfType<VGenConfig>(); }
                return _vg;
            }
        }

        public AddChunkFinder(IntVector3 halfSize, Transform center, LiveChunks lookup, VGenConfig vGenConfig)
        {
            this.lookup = lookup;
            this.center = center;
            _halfSize = halfSize;
            resetIterator();
            iterator = new ProximityIterator3(domain.size.MaxComponent);
            chunkAddQueue = new ChunkAddQueue(domain.size.MaxComponent);
        }

        private void resetIterator()
        {
            currentCenter = chunkLocation;
            iterator.Reset();
        }

        bool IsLiveOrEnqueued(IntVector3 cpos)
        {
            return chunkAddQueue.Contains(cpos) || lookup.IsLive(cpos);
        } 

        public IntVector3 chunkLocation {
            get {
                return vGenConfig.PosToChunkPos(IntVector3.FromVector3(center.position)).ClampedY(_halfSize.y, vGenConfig.MaxHeightInChunks - _halfSize.y);
            }
        }

        public IntBounds3 domain {
            get {
                return IntBounds3.FromCenterHalfSize(chunkLocation, _halfSize);
            }
        }

        public bool NextStartBuild(out IntVector3 chunkPos)
        {
            while(true)
            {
                if(chunkAddQueue.Next(chunkLocation, out chunkPos))
                {
                    if (domain.Contains(chunkPos))
                    {
                        return true;
                    }
                } else { break; }
            }
            chunkPos = default(IntVector3);
            return false;
        }

        public bool Next(out IntVector3 chunkPos)
        {
            if(!currentCenter.Equal(chunkLocation))
            {
                resetIterator();
            }

            while (true)
            {
                if (iterator.Next(out chunkPos))
                {
                    chunkPos += currentCenter;
                    if(!IsLiveOrEnqueued(chunkPos))
                    {
                        return true;
                    }
                } else
                {
                    return false;
                }
            }

            
        }
    }

}
