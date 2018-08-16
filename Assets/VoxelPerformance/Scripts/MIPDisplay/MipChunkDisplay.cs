using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using MIConvexHull;
using Mel.Util;
using Mel.Math;

namespace Mel.VoxelGen
{
    public class MipChunkDisplay : MonoBehaviour
    {
        MeshRenderer renderr {
            get {
                return GetComponent<MeshRenderer>();
            }
        }
        Mesh _mesh;
        Mesh mesh {
            get {
                if(!_mesh) { _mesh = GetComponent<MeshFilter>().mesh; }
                return _mesh;
            }
        }

        Material _mat;
        Material mat {
            get {
                if(!_mat)
                {
                    _mat = renderr.material;
                }
                return _mat;
            }
        }

        public VGenConfig vGenConfig;

        public void CreateMesh(int[] voxels)
        {
            throw new Exception("This works but I don't want to use it. The absence of this exception proves that I'm not.");
            try
            {
                if (voxels.Length < 7) //ConvexHull seems to hiccup
                {
                    return;
                }

                List<IntVoxel3> vox3s = new List<IntVoxel3>();

                for (int i = 0; i < voxels.Length; ++i)
                {
                    vox3s.Add(new IntVoxel3(voxels[i], i));
                }

                // TODO: try grouping vertices that are close (Hilbert???)

                // or

                // make voxel groups...
                // vox area = subVoxelArray.Length.
                // cube area = pow(2, -depth) * chunkArea
                // if the areas are tolerably close
                //    done with this group. get a convex hull for it
                // else
                //    subDivide the sub array into 8 sub cubes by position

                //TODO: new mesh by aggregating the results of subCHunkSplit...etc..

                List<Vector3> verts = new List<Vector3>();
                List<Color> colors = new List<Color>();
                List<int> indices = new List<int>();

                OctSplitChunk octSplitChunk = new OctSplitChunk(vox3s);
                octSplitChunk.Split(vGenConfig, 8, .99f);
                //octSplitChunk.ranges.Add(vox3s);

                mesh.Clear();
                int indexOffset = 0;
                foreach (var subVox3s in octSplitChunk.ranges)
                {

                    if(subVox3s == null || subVox3s.Count < 7) { continue; }

                    Debug.Log("Vox3s cou: " + subVox3s.Count);
                    ConvexHull<IntVoxel3, VConvexFace<IntVoxel3>> chull = null;
                    try
                    {
                        chull = ConvexHull.Create<IntVoxel3, VConvexFace<IntVoxel3>>(subVox3s);
                    } catch(Exception e)
                    {
                        Debug.LogWarning("CHULL Create excptn: " + e.ToString());
                        continue;
                    }

                    var points = chull.Points.OrderBy((p) => p.index).ToList();
                    for (int i = 0; i < points.Count; ++i)
                    {
                        var point = points[i];
                        point.index = indexOffset + i;
                        verts.Add(point.Vec3);
                        colors.Add(ColorUtil.roygbivMod(point.index));
                    }

                    foreach (var face in chull.Faces)
                    {
                        for (int i = 0; i < face.Vertices.Length; ++i)
                        {
                            indices.Add(face.index(i));
                        }
                    }

                    indexOffset += points.Count;
                }

                Debug.Log("verts: " + verts.Count);

                mesh.vertices = verts.ToArray();
                mesh.triangles = indices.ToArray();
                mesh.colors = colors.ToArray();
            } catch(Exception e)
            {
                Debug.LogWarning(e.ToString());
                mesh.Clear();
            }
        }


        class OctSplitChunk
        {
            public List<List<IntVoxel3>> ranges = new List<List<IntVoxel3>>();

            List<IntVoxel3> storage;

            public OctSplitChunk(List<IntVoxel3> storage)
            {
                this.storage = storage;
            }

            struct SubChunkRange
            {
                public List<IntVoxel3> voxels;
                public IntBounds3 subChunkSize;
            }

            public void Split(VGenConfig vGenConfig, int minSplit = 2, float areaTolerance = .9f)
            {
                Queue<SubChunkRange> subChunksRanges = new Queue<SubChunkRange>();
                subChunksRanges.Enqueue(new SubChunkRange()
                {
                    voxels = storage,
                    subChunkSize = new IntBounds3() { start = new IntVector3(0, 0, 0), size = vGenConfig.ChunkSize }
                });
                bool splitOnce = false;
                while(subChunksRanges.Count > 0)
                {
                    SubChunkRange subChunkRange = subChunksRanges.Dequeue();
                    if(splitOnce && subChunkRange.subChunkSize.size.SurfaceArea / (Mathf.Pow(vGenConfig.Mip64Divisor, 3) * 2) * areaTolerance < subChunkRange.voxels.Count)
                    {
                        ranges.Add(subChunkRange.voxels);
                    }
                    else
                    {
                        if(subChunkRange.subChunkSize.size.MinComponent <= minSplit)
                        {
                            Debug.Log("hit min: " + subChunkRange.subChunkSize.size.MinComponent);
                            continue;
                        }

                        List<IntVoxel3>[] octants = new List<IntVoxel3>[8];
                        for(int i = 0; i < subChunkRange.voxels.Count; ++i)
                        {
                            int octantIndex = subChunkRange.subChunkSize.OctanctIndexOf(subChunkRange.voxels[i].intVector3); 

                            if(octants[octantIndex] == null) {
                                octants[octantIndex] = new List<IntVoxel3>();
                            }
                            octants[octantIndex].Add(subChunkRange.voxels[i]);
                        }
                        for(int i = 0; i < 8; ++i)
                        {
                            if(octants[i] != null)
                            {
                                subChunksRanges.Enqueue(new SubChunkRange()
                                {
                                    voxels = octants[i],
                                    subChunkSize = new IntBounds3()
                                    {
                                        start = subChunkRange.subChunkSize.start + subChunkRange.subChunkSize.OffsetForOctantIndex(i),
                                        size = subChunkRange.subChunkSize.size / 2
                                    }
                                });
                            }
                        }
                    }
                    splitOnce = true;
                }
            }
        }

        internal void beVisible(bool v)
        {
            renderr.enabled = v;
        }
    }
}
