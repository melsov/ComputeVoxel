using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Mel.ChunkManagement;
using Mel.Math;
using VoxelPerformance;

namespace Mel.VoxelGen
{
    public class ChunkForge : MonoBehaviour
    {
        [SerializeField]
        ChunkCompute chunkCompute;

        Action<Chunk> OnChunksDone;

        [SerializeField]
        MapDisplay mapDisplayPrefab;
        [SerializeField]
        Transform displayParent;

        VGenConfig _vGenConfig;
        VGenConfig vGenConfig { get {
                if(!_vGenConfig) { _vGenConfig = FindObjectOfType<VGenConfig>(); }
                return _vGenConfig;
            } }

        public bool Busy { get; private set; }

        public bool UnPack(IntVector3 chunkPos, Action<Chunk> callback)
        {
            if(Busy) { return false; }
            _UnPack(chunkPos, callback);
            return true;
        }

        public bool Forge(IntVector3 chunkPos, Action<Chunk> callback)
        {
            if(Busy) { return false; }
            _forge(chunkPos, callback);
            return true;
        }

        private void _forge(IntVector3 chunkPos, Action<Chunk> callback)
        {
            Busy = true;
            OnChunksDone = callback;

            if (vGenConfig.IsSerializedDataValid)
            {
                SerializedChunk.ReadAsync(chunkPos, (SerializedChunk serChunk) =>
                {
                    if (serChunk != null)
                    {
                        Chunk result = Chunk.FromSerializedChunk(serChunk, CreateMapDisplay(chunkPos));
                        chunksDone(result);
                        return;
                    }

                    computeChunk(chunkPos);
                });
            }
            else
            {
                computeChunk(chunkPos);
            }

        }

        //Async version
        private void _UnPack(IntVector3 chunkPos, Action<Chunk> callback)
        {
            Busy = true;
            OnChunksDone = callback;
            if (vGenConfig.IsSerializedDataValid)
            {
                SerializedChunk.ReadAsync(chunkPos, (SerializedChunk serChunk) =>
                {
                    if (serChunk != null)
                    {
                        Chunk result = Chunk.FromSerializedChunk(serChunk, CreateMapDisplay(chunkPos));
                        chunksDone(result);
                        return;
                    }
                    chunksDone(null);
                });
            } else
            {
                chunksDone(null);
            }
            
        }

        void computeChunk(IntVector3 chunkPos)
        {
            if (!vGenConfig.DebugDontComputeChunks)
            {
                StartCoroutine(chunkCompute.compute(chunkPos, chunksDone));
            }
            else
            {
                chunksDone(null);
            }
        }

        //private void _UnPack(IntVector3 chunkPos, Action<Chunk> callback)
        //{
        //    Busy = true;
        //    OnChunksDone = callback;
        //    SerializedChunk serChunk = SerializedChunk.Read(chunkPos);

        //    if(serChunk != null && vGenConfig.IsSerializedDataValid)
        //    {
        //        Chunk result = Chunk.FromSerializedChunk(serChunk, CreateMapDisplay(chunkPos));
        //        chunksDone(result);
        //    } else if(!vGenConfig.DebugDontComputeChunks)
        //    {
        //        StartCoroutine(chunkCompute.compute(chunkPos, chunksDone));
        //    }
        //}

        private MapDisplay CreateMapDisplay(IntVector3 chunkPos)
        {
            var md = Instantiate(mapDisplayPrefab);
            md.transform.SetParent(displayParent);
            md.transform.localPosition = vGenConfig.ChunkPosToPos(chunkPos);
            //md.initialize(vGenConfig);
            return md;
        }

        void chunksDone(Chunk chunk)
        {
            OnChunksDone(chunk);
            Busy = false;
        }
    }
}
