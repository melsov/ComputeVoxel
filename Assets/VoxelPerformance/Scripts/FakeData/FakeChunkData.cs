using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mel.Math;
using Mel.Storage;
using Mel.VoxelGen;

namespace Mel.FakeData
{
    public class FakeChunkData
    {

        public static ChunkGenData StairsGenData(IntVector3 chunkPos, IntVector3 size, int verticalOffset = 0)
        {
            ChunkGenData c = new ChunkGenData();
            var data = PackedStairsGenData(size, verticalOffset); // StairsVGenData(size, verticalOffset);
            var displays = StairsVGeom(size, verticalOffset);
            c.chunkPos = chunkPos;

            for(int i=0; i<ChunkGenData.LODLevels; ++i)
            {
                c[i] = data; // disrespecting the meaning of LOD
                c.displays[i] = displays;
            }

            return c;
        }

        public static int[] FakeHeights(IntVector3 size, int startY)
        {
            var stairs = Stairs(size);
            var result = new FlatArray2D<int>(size.xz);
            foreach(var vox in stairs)
            {
                var pos = IntVector3.FromUint256(vox);
                result[pos.xz] = result[pos.xz] < pos.y + startY ? pos.y + startY : result[pos.xz];
            }
            return result.storage;
        }

        static VoxelGenDataMirror[] StairsVGenData(IntVector3 size, int verticalOffset = 0)
        {
            var result = new List<VoxelGenDataMirror>(size.Area);

            foreach (var vi in size.IteratorXYZ)
            {
                result.Add(new VoxelGenDataMirror
                {
                    voxel = vi.v.ToVoxel(vi.v.x + vi.v.y < size.x + verticalOffset ? 2 : 0)
                });
            }

            return result.ToArray();
        }

        static VoxelGeomDataMirror[] StairsVGeom(IntVector3 size, int verticalOffset = 0)
        {
            var result = new List<VoxelGeomDataMirror>(size.Area);

            foreach (var vi in size.IteratorXYZ)
            {
                if (vi.v.x + vi.v.y < size.x + verticalOffset)
                {
                    if (CubeNeighbors6.IsOnFace(vi.v, size) || vi.v.x + vi.v.y == size.x + verticalOffset - 1)
                    {
                        result.Add(new VoxelGeomDataMirror
                        {
                            voxel = vi.v.ToVoxel(2)
                        });

                    }
                }
            }

            return result.ToArray();
        }

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

        public static VoxelGenDataMirror[] PackedStairsGenData(IntVector3 size, int verticalOffset = 0)
        {
            var packed = PackedStairs(size, verticalOffset);
            VoxelGenDataMirror[] data = new VoxelGenDataMirror[packed.Length];
            for(int i=0; i<data.Length; ++i)
            {
                data[i] = new VoxelGenDataMirror
                {
                    voxel = packed[i]
                };
            }
            return data;
        }

        public static uint[] PackedStairs(IntVector3 size, int verticalOffset = 0)
        {
            return PackedGenVoxels(size, (IntVector3 pos) =>
            {
                return pos.x + pos.y < size.x + verticalOffset;
            });
        }

        public static uint[] PackedGenVoxels(IntVector3 size, Func<IntVector3, bool> shouldBeSolid)
        {
            var dims = size;
            dims.y /= VGenConfig.VoxelsPerMapData;
            uint[] result = new uint[dims.Area];

            for(int x = 0; x < dims.x; ++x)
            {
                for (int z = 0; z < dims.z; ++z)
                {
                    for (int y = 0; y < dims.y; ++y)
                    {
                        var pos = new IntVector3(x, y, z);
                        int index = pos.ToFlatXZYIndex(dims);
                        pos.y *= VGenConfig.VoxelsPerMapData;
                        int voxels = 0;
                        for(int i=0; i < VGenConfig.VoxelsPerMapData; ++i)
                        {
                            if (shouldBeSolid(pos))
                            {
                                voxels |= (2 << (i * VGenConfig.VoxelMapDataBitSize));
                            }
                            pos.y++;
                        }
                        result[index] = (uint)voxels;
                    }
                }
            }
            return result;
        }
    }
}
