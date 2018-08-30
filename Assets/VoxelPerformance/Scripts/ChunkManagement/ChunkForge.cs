using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Mel.ChunkManagement;
using Mel.Math;
using VoxelPerformance;
using System.Threading.Tasks;

namespace Mel.VoxelGen
{
    public class ChunkForge : MonoBehaviour
    {
        [SerializeField]
        ChunkCompute _chunkCompute;
        ChunkCompute chunkCompute {
            get {
                if(!_chunkCompute)
                {
                    _chunkCompute = Instantiate(_chunkCompute);
                }
                return _chunkCompute;
            }
        }

        Action<Chunk> OnChunksDone;
        Action<ChunkGenData> OnChunkGenDataDone;

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

        #region chunk-gen-data

        

        public bool ForgeChunkGenData(IntVector3 chunkPos, Action<ChunkGenData> callback)
        {
            if(Busy) { return false; }
            OnChunkGenDataDone = callback;

            if(vGenConfig.IsSerializedDataValid)
            {
                var data = SerializedChunkGenData.Read(chunkPos);
                if(data != null)
                {
                    chunkGenDataDone(data);
                    return true;
                }
            }
            computeGenData(chunkPos);
            return true;
        }

        void computeGenData(IntVector3 chunkPos)
        {
            chunkCompute.computeGenDataPleasePurgeMe(chunkPos, chunkGenDataDone);
        }

        void chunkGenDataDone(ChunkGenData dat)
        {
            Busy = false;
            OnChunkGenDataDone(dat);
        }

        #endregion

        #region chunk-from-gen-data

        //public bool FromGenData(ChunkGenData data, Action<Chunk> callback)
        //{
        //    if(Busy) { return false; }

        //}

        #endregion

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

                    Debug.Log("got back null from read");
                    computeChunk(chunkPos);
                });
            }
            else
            {
                Debug.Log("ser data invalid");
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
                Debug.Log("will read chunk: " + chunkPos);
                SerializedChunk.ReadAsync(chunkPos, (SerializedChunk serChunk) =>
                {
                    if (serChunk != null)
                    {
                        Debug.Log("serchunk not null");
                        Chunk result = Chunk.FromSerializedChunk(serChunk, CreateMapDisplay(chunkPos));
                        chunksDone(result);
                        return;
                    }
                    chunksDone(null);
                });
            } else
            {
                Debug.Log("ser data not valid");
                chunksDone(null);
            }
            
        }

        void computeChunk(IntVector3 chunkPos)
        {
            //
            // Acutally let's avoid computing chunks in chunk forge
            // only read them from disk

            chunksDone(null);

            //Don't want this
            /*
            if (!vGenConfig.DebugDontComputeChunksInDisplayPipeline)
            {
                StartCoroutine(chunkCompute.compute(chunkPos, chunksDone));
            }
            else
            {
                chunksDone(null);
            }
            */
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
