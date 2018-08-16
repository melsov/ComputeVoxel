using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mel.Math;
using System.Collections;
using VoxelPerformance;
using Mel.Util;

namespace Mel.VoxelGen
{

    public class ChunkCompute : MonoBehaviour
    {
        [SerializeField] VGenConfig vGenConfig;

        [SerializeField] ComputeShader perlinGen;
        [SerializeField] ComputeShader meshGen;
        [SerializeField] ComputeShader hilbertShader;
        [SerializeField] Shader geometryShader;

        [SerializeField] Texture2D voxelTexture;

        [SerializeField] GameObject displayPrefab;

        [SerializeField] LineSegTestPoints lineSegTestPoints;

        CVoxelMapData cvoxelMapData;
        CVoxelMapFormat cvoxelMapFormat;

        private void Awake()
        {
            cvoxelMapData = new CVoxelMapData(perlinGen, vGenConfig);
            cvoxelMapFormat = new CVoxelMapFormat(meshGen, hilbertShader, cvoxelMapData, vGenConfig);
        }

        public IEnumerator compute(IntVector3 chunkPos, Action<Chunk> callback)
        {
            cvoxelMapData.callClearMapBuffersKernel();
            yield return new WaitForEndOfFrame();

            cvoxelMapData.callPerlinMapGenKernel(chunkPos);
            yield return new WaitForEndOfFrame();
            cvoxelMapData.DebugBuffs();

            cvoxelMapFormat.callFaceGenKernels();
            yield return new WaitForEndOfFrame();

            cvoxelMapFormat.callFaceCopyKernel();
            yield return new WaitForEndOfFrame();
            cvoxelMapFormat.DebugBuffs();
            
            GenerateChunk(chunkPos, callback);
            //yield return new WaitForEndOfFrame();
            
        }

        void GenerateChunk(IntVector3 chunkPos, Action<Chunk> callback)
        {
            //TODO: Map Display: there's only one of them!
            // a class TreeRayCaster curates a buffer by reading several OctTrees (one per chunk)

            MapDisplay md = GenerateMapDisplay(chunkPos, cvoxelMapFormat.takeDisplayBuffers()); 

            Chunk chunk = new Chunk();
            chunk.Init(
                chunkPos, 
                md, 
                new Chunk.MetaData()
                {
                    Dirty = true
                });

            callback(chunk);
        }

        public class CPUSideVoxelProcessing
        {
            

            //private static bool getVoxel(int x, int y, int z, VGenConfig vGenConfig, List<IntVoxel3> storage, out IntVoxel3 voxi)
            //{
            //    int index = IntVector3.FlatXYZIndex(x, y, z, vGenConfig.ChunkSize);
            //    if(index >= storage.Count)
            //    {
            //        voxi = new IntVoxel3(0,0);
            //        return false;
            //    }
            //    voxi = storage[index];
            //    return true;
            //}


                

            public static uint[] ExposedVoxels(uint[] foundVoxels, VGenConfig vGenConfig)
            {
                var iVoxels = PackedVoxelsToIndexedIntVoxel(foundVoxels, vGenConfig);
                var exposedVoxels = new List<uint>();

                int[] sideCheckIncrs = new int[]
                {
                    1, -1, // y
                    vGenConfig.ChunkSizeY, -vGenConfig.ChunkSizeY, // x
                    vGenConfig.ChunkSizeY * vGenConfig.ChunkSizeX, -vGenConfig.ChunkSizeY * vGenConfig.ChunkSizeX,
                };

                for(int i=0;i<iVoxels.Count; ++i)
                {
                    var voxi = iVoxels[i];
                    if(voxi.voxelType < 1) {
                        continue;
                    }

                    if (voxi.intVector3.IsOnAFace(vGenConfig.ChunkSize))
                    {
                        exposedVoxels.Add((uint)voxi.voxel);
                        continue;
                    }

                    int DBUGWithinStorageCount = 0;
                    foreach(int incr in sideCheckIncrs)
                    {
                        int sideIndex = i + incr;
                        if(sideIndex < 0 || sideIndex >= iVoxels.Count) {
                            continue;
                        }
                        var sideVoxi = iVoxels[sideIndex];
                        DBUGWithinStorageCount++;
                        if(sideVoxi.voxelType == 0)
                        {
                            Debug.Log("found one");
                            exposedVoxels.Add((uint)voxi.voxel);
                            break;
                        }
                    }

                    Debug.Log("within storage: " + DBUGWithinStorageCount);
                }

                return exposedVoxels.ToArray();
            }

            public static List<IntVoxel3> PackedVoxelsToIndexedIntVoxel(uint[] uis, VGenConfig vGenConfig)
            {
                var points = new List<IntVoxel3>(uis.Length * 4);
                for (int i = 0; i < uis.Length; ++i)
                {
                    for (int j = 0; j < 4; ++j)
                    {
                        IntVoxel3 ivox = IntVoxel3.FromPackedVoxel(i, j, uis[i], vGenConfig);
                        points.Add(ivox);
                    }
                }
                return points;
            }
        }

        MapDisplay GenerateMapDisplay(IntVector3 chunkPos, MapDisplay.LODBuffers dbuffers) // MapDisplay.DisplayBuffers dbuffers)
        {
            GameObject go = (GameObject)Instantiate(displayPrefab);
            go.transform.SetParent(transform);
            go.transform.localPosition = transform.localPosition + Vector3.Scale(chunkPos, vGenConfig.ChunkSize);
            MapDisplay md = go.GetComponent<MapDisplay>();

            md.initialize(
                //geometryShader, 
                //ColorUtil.roygbiv, 
                //voxelTexture,
                dbuffers,
                vGenConfig);
            return md;
        }

        private void OnDestroy()
        {
            cvoxelMapData.releaseTemporaryBuffers();
            cvoxelMapFormat.releaseTemporaryBuffers();
        }

    }
}
