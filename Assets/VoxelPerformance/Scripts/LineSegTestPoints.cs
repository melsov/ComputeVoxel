using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mel.Math;
using Mel.Util;
using HilbertExtensions;
using Mel.Editorr;
using VoxelPerformance;

namespace Mel.VoxelGen
{
    public class LineSegTestPoints : MonoBehaviour
    {
        public enum TestShape {
            Cube, Sphere, Ramp, InvertedRamp
        }
        [SerializeField]
        TestShape testShape;

        int _chunkSize {
            get { return vGenConfig.ChunkDimension; }
        }
        [SerializeField]
        bool makeTestShapes;
        [SerializeField]
        LineSegmentRenderer lsr;
        //[SerializeField]
        //CubesRenderer cr;
        [SerializeField]
        private MapDisplay displayPrefab;
        [SerializeField]
        private Shader voxelShader;
        [SerializeField]
        private Texture2D spriteTex;

        VGenConfig _vGenConfig;
        VGenConfig vGenConfig {
            get {
                if(!_vGenConfig)
                {
                    _vGenConfig = FindObjectOfType<VGenConfig>();
                }
                return _vGenConfig;
            }
        }

        private void Start()
        {
            if (makeTestShapes)
            {
                RenderPoints();
            }
        }

        void RenderPoints()
        {
            IntVector3 size = new IntVector3(_chunkSize);
            var points = GetPoints(size); 
            //int bits = (int) Mathf.Log( _chunkSize * _chunkSize * _chunkSize, 8);
            points = points.OrderBy(v => HilbertTables.XYZToHilbertIndex[v.index]).ToList(); // v.v.ToUint3().CoordsToFlatHilbertIndex(bits)).ToList();

            var verts = new List<Vertex>(points.Count);

            for(int i=0; i < points.Count; i++)
            {
                Vertex a = new Vertex() { pos = (Vector3)points[i].v, color = ColorUtil.roygbivMod(i) };
                verts.Add(a);
            }
            TestHilbertBackToXYZ(points);
            lsr.Apply(verts.ToArray());

            //cr.Apply(verts);

            var dbuffers = new MapDisplay.LODBuffers(); // GenerateMapBuffers(points);
            MapDisplay md = Instantiate(displayPrefab);
            md.transform.SetParent(transform);
            md.transform.localPosition = transform.localPosition + Vector3.right * 10f; // just nudge right a bit
            md.initialize(dbuffers);
                //voxelShader,
                //ColorUtil.roygbiv, 
                //spriteTex,
                //dbuffers);
        }

        private void TestHilbertBackToXYZ(List<IntVector3.IndexedIntVector3> points)
        {
            var blsr = Instantiate(lsr);
            var bpoints = points.Select(v => v).ToList();

            for(int i = 0; i < bpoints.Count; ++i)
            {
                var v = bpoints[i];
                v.index = i;
                bpoints[i] = v;
            }
            bpoints = bpoints.OrderBy(v => HilbertTables.HilbertIndexToXYZ[v.index]).ToList();

            var verts = new List<Vertex>(bpoints.Count);


            for (int i = 0; i < bpoints.Count; i++)
            {
                Vertex a = new Vertex() { pos = bpoints[i].v + Vector3.up * 10f, color = ColorUtil.roygbivMod(i) };
                verts.Add(a);
            }

            blsr.Apply(verts.ToArray());
        }

        //TODO: option to accept an array of points from without
        public static MapDisplay.DisplayBuffers GenTestBuffers(int _chunkSize, List<IntVector3.IndexedIntVector3> points = null)
        {
            IntVector3 size = new IntVector3(_chunkSize);

            if (points == null)
            {
                points = GetPoints(size, TestShape.Cube);
            }
         
            //int bits = (int)Mathf.Log(_chunkSize * _chunkSize * _chunkSize, 8);
            points = points.OrderBy(v => HilbertTables.XYZToHilbertIndex[v.index]).ToList(); // v.v.ToUint3().CoordsToFlatHilbertIndex(bits)).ToList();

            var dbuffers = GenerateMapBuffers(points);
            return dbuffers;
        }

        public static bool PackedVoxelsToIndexedIntVector3s(uint[] uis, out List<IntVector3.IndexedIntVector3> points, VGenConfig vGenConfig)
        {
            points = new List<IntVector3.IndexedIntVector3>(uis.Length * 4);
            for(int i=0; i<uis.Length; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    // PGen encodes voxel position by index: z, x, y (y least significant).
                    // Voxel values encoded in 8 bits, packed 4 per uint.
                    int voxel = ((int)uis[i] >> (8 * j)) & 0xFF;
                    if(voxel < 1) { continue; }

                    IntVector3 v = new IntVector3();
                    int vi = i * 4 + j;
                    v.z = vi / (vGenConfig.ChunkSizeY * vGenConfig.ChunkSizeX);
                    v.x = (vi % (vGenConfig.ChunkSizeZ * vGenConfig.ChunkSizeY)) / vGenConfig.ChunkSizeY;
                    v.y = vi % vGenConfig.ChunkSizeY; // ?? (vGenConfig.ChunkSizeZ * vGenConfig.ChunkSizeX);

                    points.Add(new IntVector3.IndexedIntVector3()
                    {
                        index = i,
                        v = v
                    });
                }
            }
            return points.Count > 0;
        }

        public static bool TestDisplayBufferFromPerlinGen(uint[] uis, out MapDisplay.DisplayBuffers displayBuffers, VGenConfig vGenConfig)
        {
            List<IntVector3.IndexedIntVector3> points;
            if(!PackedVoxelsToIndexedIntVector3s(uis, out points, vGenConfig))
            {
                displayBuffers = new MapDisplay.DisplayBuffers();
                return false;
            }

            displayBuffers = GenTestBuffers(vGenConfig.ChunkDimension, points);
            return true;
        }

        public bool TestDisplayBufferFromSolidVoxels(uint[] uis, out MapDisplay.DisplayBuffers displayBuffers)
        {
            if(uis.Length == 0)
            {
                displayBuffers = new MapDisplay.DisplayBuffers();
                return false;
            }
            var points = new List<IntVector3.IndexedIntVector3>(uis.Length);
            for (int i = 0; i < uis.Length; ++i) 
            {
                points.Add(new IntVector3.IndexedIntVector3()
                {
                    index = i,
                    v = IntVector3.FromVoxelInt((int)uis[i])
                });
            }

            displayBuffers = GenTestBuffers(_chunkSize, points);
            return true;

        }

        public List<IntVector3.IndexedIntVector3> GetPoints(IntVector3 size)
        {
            return GetPoints(size, testShape);
        }

        public static List<IntVector3.IndexedIntVector3> GetPoints(IntVector3 size, TestShape testShape)
        {
            var result = new List<IntVector3.IndexedIntVector3>();
            var center = size / 2;
            foreach(var iv in size.IteratorXYZ)
            {
                var dif = iv.v - center;
                switch (testShape)
                {
                    case TestShape.Cube:
                    default:
                        result.Add(iv);
                        break;
                    case TestShape.Sphere:
                        if (dif.SquareMagnitude < center.SquareMagnitude / 4)
                        {
                            result.Add(iv);
                        }
                        break;
                    case TestShape.Ramp:
                        if(iv.v.x + iv.v.y < size.x)
                        {
                            result.Add(iv);
                        }
                        break;
                    case TestShape.InvertedRamp:
                        if(iv.v.x + iv.v.y > size.x)
                        {
                            result.Add(iv);
                        }
                        break;

                }
                
            }
            return result;
        }

        private static MapDisplay.DisplayBuffers GenerateMapBuffers(List<IntVector3.IndexedIntVector3> points)
        {
            List<uint> voxels = new List<uint>(points.Count);
            List<uint> hindices = new List<uint>(points.Count / 2);
            IntVector3 prevFour = points[0];
            IntVector3 prevTwo = points[0];
            hindices.Add(0);
            int hFourCount = 1;
            for(int i=0; i < points.Count; ++i)
            {
                var iv = points[i]; 
                voxels.Add(iv.v.ToVoxel());
                if(TestHilbertIndices.isDifferentHilbertCube(iv.v, prevFour, 4))
                {
                    prevFour = iv.v;
                    hindices.Add((uint)i);
                    hFourCount++;
                }
            }
            for (int i = 0; i < points.Count; ++i)
            {
                var iv = points[i];
                if (!TestHilbertIndices.isDifferentHilbertCube(iv.v, prevFour, 4))
                {
                    if (TestHilbertIndices.isDifferentHilbertCube(iv.v, prevTwo, 2))
                    {
                        prevTwo = iv.v;
                        hindices.Add((uint)i);
                    }
                } else
                {
                    prevFour = iv.v;
                }
            }

            uint[] hilbertRanges = new uint[] { (uint)points.Count, (uint)hindices.Count, (uint)hFourCount }; // (uint)hindices.Count };

            MapDisplay.DisplayBuffers dbuffers = new MapDisplay.DisplayBuffers();
            dbuffers.display = new ComputeBuffer(voxels.Count, sizeof(uint));
            dbuffers.display.SetData(voxels.ToArray());

            dbuffers.hilbertIndices = new ComputeBuffer(hindices.Count, sizeof(uint));
            dbuffers.hilbertIndices.SetData(hindices.ToArray());

            dbuffers.hilbertLODRanges = new ComputeBuffer(hilbertRanges.Length, sizeof(uint));
            dbuffers.hilbertLODRanges.SetData(hilbertRanges);

            return dbuffers;
            
        }

     
    }
}
