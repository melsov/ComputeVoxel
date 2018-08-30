using Mel.ChunkManagement;
using Mel.Math;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unity.Collections;
using UnityEngine;
using VoxelPerformance;

namespace Mel.VoxelGen
{
    public class ChunkGenData
    {
        public VoxelGenDataMirror[] voxels {
            get { return lods[0]; }
            set { lods[0] = value; }
        }

        public uint[] ExistsMap;

        public IntVector3 chunkPos;
        public VoxelGenDataMirror[][] lods;
        public MapDisplay.LODArrays displays; 

        public static int LODLevels => 3;

        public ChunkGenData()
        {
            lods = new VoxelGenDataMirror[LODLevels][];
            displays = MapDisplay.LODArrays.Create();
        }

        public VoxelGenDataMirror[] this[int i] {
            get {
                return lods[i];
            }
            set {
                lods[i] = value;
            }
        }
    }

    public static class ChunkIndex
    {
        public static int GetPerlinGenPackedIndex(IntVector3 chunkRelative, IntVector3 chunkSize, int voxelsPerMapData = 4)
        {
            return chunkRelative.ToFlatZXYIndex(chunkSize) / voxelsPerMapData;
        }

        public static int GetExistsMapIndex(IntVector3 pos, IntVector3 chunkSize)
        {
            return pos.ToFlatZXYIndex(chunkSize) / VGenConfig.SizeOfHLSLInt;
        }

        public static uint GetExists(NativeArray<uint> existsMap, IntVector3 pos, IntVector3 chunkSize)
        {
            var val = existsMap[GetExistsMapIndex(pos, chunkSize)];
            return (val >> (pos.ToFlatZXYIndex(chunkSize) % VGenConfig.SizeOfHLSLInt)) & 1;
        }
    }

    public class Chunk
    {
        //TODO: Serialize hidden voxels also

        public MapDisplay mapDisplay { get; private set; }
        public IntVector3 ChunkPos { get; private set; }

        public ComputeBuffer display {
            get {
                return mapDisplay.buffers.display;
            }
        }

        public Bounds bounds { get { return mapDisplay.bounds; } }

        public struct MetaData
        {
            public bool Dirty;
            public int[] LODBufferLengths;

            public bool isInvalid {
                get {
                    return LODBufferLengths == null || LODBufferLengths.Length == 0;
                }
            }

            public static bool SavedChunkExists(IntVector3 chunkPos)
            {
                return File.Exists(SerializedChunk.GetMetaDataFullPath(chunkPos));
            }

            public bool HasBeenNeighborProcessed;

            public static void Write(ChunkGenData chunkGenData)
            {
                MetaData metaData = new MetaData
                {
                    LODBufferLengths = chunkGenData.displays.getLengths(),
                    HasBeenNeighborProcessed = true
                };
                XMLOp.Serialize(metaData, SerializedChunk.GetMetaDataFullPath(chunkGenData.chunkPos));
            }
        }

        public MetaData metaData;

        public void SetDirty(bool dirty = true)
        {
            metaData.Dirty = dirty;
        }

        public void Init(IntVector3 ChunkPos, MapDisplay display, MetaData metaData)
        {
            mapDisplay = display;
            this.ChunkPos = ChunkPos;
            this.metaData = metaData;
            this.metaData.LODBufferLengths = mapDisplay.buffers.GetBufferLengths();
        }

        public static Chunk FromSerializedChunk(SerializedChunk serChunk, MapDisplay display)
        {
            return serChunk.ToChunk(display);
        }

        public void Serialize()
        {
            if(!metaData.Dirty) { return; }

            metaData.Dirty = false;
            var serChunk = new SerializedChunk(ChunkPos, mapDisplay.buffers, new uint[0], metaData);
            serChunk.Write();
        }

        public void SerializeAndDestory()
        {
            // Turn off Serialize 
            // Serialize();
            GameObject.Destroy(mapDisplay);
        }
    }
}
