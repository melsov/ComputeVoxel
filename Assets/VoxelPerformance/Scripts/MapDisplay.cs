using UnityEngine;
using System.Collections;
using Mel.VoxelGen;
using UnityEngine.Assertions;
using System;
using System.Text;


// VoxelPerformance/Scripts/MapDisplay.cs
// Copyright 2016 Charles Griffiths
// (This version uses some modifications by Matt Poindexter)

namespace VoxelPerformance
{
    // This class is used to display one chunk produced by compute shaders in VoxelPerformance/Shaders
    // using a geometry shader found in VoxelPerformance/Shaders/VoxelGeometry.shader
    public class MapDisplay : MonoBehaviour 
    {
        [SerializeField]
        Texture2D sprite;

        [SerializeField]
        Shader geometryShader;

        [SerializeField]
        Transform emptyChunkMarker;

        public class LODBuffers
        {

            public ComputeBuffer this[int i] {
                get {
                    return buffs[i];
                }
                set {
                    buffs[i] = value;
                }
            }

            public ComputeBuffer display {
                get { return buffs[0]; }
                set { buffs[0] = value; }
            }
            public ComputeBuffer lod2 {
                get { return buffs[1]; }
                set { buffs[1] = value; }
            }
            public ComputeBuffer lod4 {
                get { return buffs[2]; }
                set { buffs[2] = value; }
            }

            public int[] GetBufferLengths() { return new int[] {
                CountForLOD(0),
                CountForLOD(1),
                CountForLOD(2) }; }

            public bool isEmpty { get { return display == null || !display.IsValid() || display.count == 0; } }

            ComputeBuffer[] buffs = new ComputeBuffer[3]; // { get { return new ComputeBuffer[] { display, lod2, lod4 }; } }

            public void release()
            {
                BufferUtil.ReleaseBuffers(buffs);
            }

            public int bufferCount { get { return buffs.Length; } }

            public int CountForLOD(int octantDepth)
            {
                return buffs[octantDepth] == null ? 0 : buffs[octantDepth].count;
            }

            public string DebugCounts() {

                AssertAllNullOrNoneNull();

                StringBuilder sb = new StringBuilder();
                int i = 0;
                foreach(var cb in buffs)
                {
                    sb.Append(string.Format("[{0}]: {1}", i++, cb != null ? (cb.IsValid() ? "" + cb.count : "invalid" ): "null"));
                }
                return sb.ToString();
            }

            public void AssertAllNullOrNoneNull()
            {
                int nullCount = 0;
                int emptyCount = 0;
                foreach(var cb in buffs)
                {
                    if (cb == null) nullCount++;
                    //else if (cb.count == 0) emptyCount++;
                }
                Assert.IsTrue(nullCount == 0 || nullCount == 3, "??? null count: " + nullCount);
                Assert.IsTrue(emptyCount == 0 || emptyCount == 3, "??? empty count: " + emptyCount);
            }
        }

        //TODO: purge references to DisplayBuffers
        // replaced by LODBuffers
        public class DisplayBuffers
        {
            public ComputeBuffer display;
            public ComputeBuffer hilbertIndices;
            public ComputeBuffer hilbertLODRanges;

            //TODO: may as well set this on creation (and not have to release/null/reallocate the bufffer in VMFormat)
            private uint[] _lodRanges;

            public uint[] lodRanges {
                get {
                    if(_lodRanges == null)
                    {
                        _lodRanges = new uint[hilbertLODRanges.count];
                        hilbertLODRanges.GetData(_lodRanges);
                    }
                    return _lodRanges;
                }
            }


            public void release()
            {
                ComputeBuffer[] buffs = new ComputeBuffer[]
                {
                    display, hilbertIndices, hilbertLODRanges,
                };
                for(int i = 0; i < buffs.Length; ++i)
                {
                    if ( buffs[i] != null) { buffs[i].Release(); }
                    buffs[i] = null;
                }
            }

        }


        public LODBuffers buffers { get; private set; }

        private VGenConfig _vg;
        private VGenConfig vGenConfig {
            get {
                if(!_vg) { _vg = FindObjectOfType<VGenConfig>(); }
                return _vg;
            }
        }

        Material material;

        Transform globalLight;

        public float size { get; private set; }
        public Bounds bounds { get; private set; }


        public void initialize(
            LODBuffers buffers,
            VGenConfig vGenConfig = null
            )
        {

            size = 1f;
            this.buffers = buffers;
            globalLight = GameObject.Find("GlobalLight").transform; 

            bounds = new Bounds(transform.position + this.vGenConfig.ChunkSize / 2f * size, this.vGenConfig.ChunkSize * size);
            material = new Material(geometryShader);
            setMaterialConstants();

            enabled = true;

            //SetUpEmptyChunkMarker();
        }

        private void SetUpEmptyChunkMarker()
        {
            if(!buffers.isEmpty) { return; }

            var ecm = Instantiate(emptyChunkMarker);
            ecm.SetParent(transform);
            ecm.localScale = ((Vector3) vGenConfig.ChunkSize) * .95f;
            ecm.localPosition = vGenConfig.ChunkSize.ToVector3() / 2f;

        }

        public void releaseDisplayBuffers() {
            if (buffers != null)
            {
                buffers.release();
            }
            enabled = false;
        }


        void setMaterialConstants()
        {
            material.SetVector("_chunkPosition", transform.position);
            material.SetTexture("_Sprite", sprite);
            material.SetFloat("_Size", size);
            material.SetMatrix("_worldMatrixTransform", transform.localToWorldMatrix);

            material.SetBuffer("_displayPoints", buffers[0]);
            material.SetBuffer("_displayPointsLOD2", buffers[1]);
            material.SetBuffer("_displayPointsLOD4", buffers[2]);
        }

        bool isVisible(float distFromCamSquared)
        {
            if (buffers == null || buffers.isEmpty) { return false; } 
            return enabled
              && distFromCamSquared < 1.2f * Camera.main.farClipPlane * Camera.main.farClipPlane
              && GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(Camera.main), bounds)
              && buffers.display != null;
        }

        void OnRenderObject() {
            if(vGenConfig.UseReverseCasting) { return; }

            if(Input.GetKey(KeyCode.R)) { return; }

            float distFromCamSquared = bounds.SqrDistance(Camera.main.transform.position);
            if (isVisible(distFromCamSquared)) {
                ////        Shader.globalMaximumLOD = 100;
                ////        material.shader.maximumLOD = 100;
                material.SetPass(0);
                material.SetVector("_cameraPosition", Camera.main.transform.position);

                int lodIndex = vGenConfig.LODIndexForCamDistance(distFromCamSquared);
                lodIndex = Input.GetKey(KeyCode.C) ? 0 : lodIndex;
                lodIndex = Input.GetKey(KeyCode.F) ? 1 : lodIndex;
                lodIndex = Input.GetKey(KeyCode.G) ? 2 : lodIndex;
                material.SetFloat("_mipStretch", Mathf.Pow(2f, lodIndex));
                material.SetVector("_globalLight", globalLight.position);

                Graphics.DrawProcedural(MeshTopology.Points, buffers.CountForLOD(lodIndex % buffers.bufferCount)); 
            }
        }

        private void OnDestroy()
        {
            releaseDisplayBuffers();
        }

        public void DebugBuffers()
        {
            //DebugChunkData.CheckUniqueData(buffers.display, "buff.display");
            //DebugChunkData.CheckUniqueData(buffers.lod2, "buff.lod2");
            //DebugChunkData.CheckUniqueData(buffers.lod4, "buff.lod4");

            //DebugChunkData.CheckAllSamePositionOnAxis(buffers.lod4, 0, "buff.lod4 ");

        }

    }
}

