using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Mel.VoxelGen;
using Mel.Math;
using System;

// VoxelPerformance/Scripts/MapGen.cs
// Copyright 2016 Charles Griffiths

namespace VoxelPerformance
{
    public class MapGen : MonoBehaviour
    {
        public bool createSponge = false;
        public bool showVoxels = true;
        public bool createMesh = false;
        public bool createUnityTerrain = false;

        public ComputeShader perlinGen;
        public ComputeShader meshGen;
        public ComputeShader hilbertIndicesShader;
        public Shader geometryShader;

        VoxelMapData mapChunkCreation;
        VoxelMapFormat mapChunkMeshing;

        public GameObject displayPrefab;
        [SerializeField]
        GameObject mipChunkDisplayPrefab;
        public GameObject meshPrefab;

        public Color[] color;
        public float size;
        public Texture2D sprite;

        static public Camera mainCamera;
        static public Plane[] cameraPlanes;

        [SerializeField]
        int firstIndex;

        int offsetIndex;
        bool createChunks;

        [Tooltip("Maximum dimension (x and z)")]
        public IntVector3 chunkMax;

        [SerializeField]
        VGenConfig vGenConfig;

        void Start() {
            mainCamera = Camera.main;

            mapChunkCreation = new VoxelMapData(perlinGen, vGenConfig);
            mapChunkMeshing = new VoxelMapFormat(meshGen, hilbertIndicesShader, mapChunkCreation, vGenConfig);

            offsetIndex = firstIndex;

            StartCoroutine(generateChunks());
        }

        private IEnumerator generateChunks()
        {
            while(offsetIndex < chunkMax.Area)
            {
                mapChunkCreation.callPerlinMapGenKernel(mapOffset(offsetIndex));
                //mapChunkMeshing.callFaceGenKernelMip64();
                yield return new WaitForEndOfFrame();
                //int[] mipVoxels = mapChunkMeshing.getMipVoxels();

                //TODO: purge mip chunk?
                //MipChunkDisplay mipChunk = null; // Instantiate(mipChunkDisplayPrefab.GetComponent<MipChunkDisplay>());
                //mipChunk.vGenConfig = vGenConfig;
                //mipChunk.CreateMesh(mipVoxels); 

                //int mip64Offset = 0;  // mapChunkMeshing.getMip64Offset();

                mapChunkMeshing.callFaceGenKernel();
                yield return new WaitForEndOfFrame();
                mapChunkMeshing.hilbertSortVoxels();

                mapChunkMeshing.callConstructHilbertIndicesKernel();
                yield return new WaitForEndOfFrame();
                mapChunkMeshing.callFaceCopyKernel();
                generateNextMapDisplay();
                offsetIndex++;
            }
            yield return new WaitForEndOfFrame();
            releaseTemporaryBuffers();
        }

        void generateNextMapDisplay() 
        {
            GameObject go = (GameObject)Instantiate(displayPrefab);
            go.transform.SetParent(transform);
            go.transform.localPosition = transform.localPosition + Vector3.Scale(mapOffset(offsetIndex), vGenConfig.ChunkSize) * size;
            MapDisplay md = go.GetComponent<MapDisplay>();

            md.initialize(
                //geometryShader, color, sprite,
                null, //// <--fake mapChunkMeshing.takeDisplayBuffers(),
                vGenConfig);
        }

        Vector3 mapOffset(int index) {
            return new Vector3((index / chunkMax.y) % chunkMax.x, index % chunkMax.y, index / (chunkMax.x * chunkMax.y));
        }

        void releaseTemporaryBuffers() {
            mapChunkCreation.releaseTemporaryBuffers();
            mapChunkMeshing.releaseTemporaryBuffers();
        }

        public void releaseDisplayBuffers() {
            foreach (MapDisplay md in gameObject.GetComponentsInChildren<MapDisplay>(true))
                md.releaseDisplayBuffers();
        }

        void LateUpdate() {
            cameraPlanes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
        }

        void OnDestroy() {
            releaseTemporaryBuffers();
            releaseDisplayBuffers();
        }
    }
}

