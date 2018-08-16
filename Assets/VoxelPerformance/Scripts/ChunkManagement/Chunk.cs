using Mel.ChunkManagement;
using Mel.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using VoxelPerformance;

namespace Mel.VoxelGen
{
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
            Serialize();
            GameObject.Destroy(mapDisplay);
        }
    }
}
