using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mel.Math;
using System.Collections;
using VoxelPerformance;
using Mel.Util;
using System.Threading.Tasks;
using Mel.FakeData;
using Mel.ChunkManagement;

namespace Mel.VoxelGen
{

    public class ChunkCompute : MonoBehaviour
    {
        [SerializeField] VGenConfig vGenConfig;

        [SerializeField] ComputeShader perlinGen;
        [SerializeField] ComputeShader meshGen;
        [SerializeField] ComputeShader neighborFormat;
        [SerializeField] ComputeShader hilbertShader;
        [SerializeField] Shader geometryShader;

        [SerializeField] Texture2D voxelTexture;

        [SerializeField] GameObject displayPrefab;

        [SerializeField] LineSegTestPoints lineSegTestPoints;

        CVoxelMapData cvoxelMapData;
        CVoxelMapFormat cvoxelMapFormat;
        CVoxelNeighborFormat cvoxelNeighborFormat;
        CVoxelFaceCopy cvoxelFaceCopy;



        private void Awake()
        {
            cvoxelMapData = new CVoxelMapData(perlinGen, vGenConfig);
            cvoxelMapFormat = new CVoxelMapFormat(meshGen, hilbertShader, cvoxelMapData, vGenConfig);
            cvoxelNeighborFormat = new CVoxelNeighborFormat(neighborFormat, vGenConfig);
            cvoxelFaceCopy = new CVoxelFaceCopy(neighborFormat, cvoxelNeighborFormat, vGenConfig);
        }

        #region gen-data

        // NOTE: we must renounce chunk computing in the display thread
        // to ensure that we don't scuttle the buffers with overlapping compute calls

        public async Task<ColumnAndHeightMap<ChunkGenData>> ComputeColumnAtAsync(Column<ChunkGenData> column, Func<IntVector3, ChunkGenData> GetFromMemory)
        {
            //...Set data for the heightmap
            ColumnAndHeightMap<ChunkGenData> cah = new ColumnAndHeightMap<ChunkGenData>();
            cah.column = column;
            cah.heightMap = new HeightMap(vGenConfig.ColumnFootprint);

            var keys = cah.column.Keys.ToArray();
            int dbugComputedCount = 0;
            for(int i = 0; i < keys.Length; ++i)
            {
                var data = GetFromMemory(cah.column.position.ToIntVector3XZWithY(keys[i]));
                if (data == null)
                {
                    dbugComputedCount++;
                    data = (ChunkGenData)(await ComputeGenData(cah.column.position.ToIntVector3XZWithY(keys[i])));
                }
                cah.column[keys[i]] = data;
            }


            //TODO: set height map data
            //int[] heights = CVoxelMapFormat.BufferCountArgs.GetData<int>(cvoxelMapData.MapHeights); //TODO: fill with actual data
            //FAKENESS

            int[] heights = FakeChunkData.FakeHeights(vGenConfig.ChunkSize, keys.OrderByDescending((k) => k).ToArray()[0]);

            cah.heightMap.setData(heights);

            return cah;
        }


        public async Task PostProcessColumnAsync(ColumnAndHeightMap<NeighborChunkGenData> columnAndHeightMap, Action<IntVector3> OnWroteChunkData)
        {
            //
            // Work from top to bottom. (TODO: propagate bootstrap data downwards)
            //
            var keys = columnAndHeightMap.column.Keys.OrderByDescending(a => a);

            foreach (var key in keys)
            {
                await PostProcessSetAsync(columnAndHeightMap.column[key], OnWroteChunkData);
            }
        }

        private async Task PostProcessSetAsync(NeighborChunkGenData neighborChunkGenData, Action<IntVector3> onWroteChunkData)
        {
            //FAKE
            //var fakeCGD = (ChunkGenData)(await ComputeGenDataFAKE(neighborChunkGenData.center));
            //neighborChunkGenData.centerChunkData = fakeCGD;

            //WANT
            await _PostProcessSet(neighborChunkGenData); 

            updateChunkGenData(neighborChunkGenData.centerChunkData);
            onWroteChunkData(neighborChunkGenData.center);
        }

        IEnumerator _PostProcessSet(NeighborChunkGenData neighborChunkGenData)
        {
            //awkward. 
            while(cvoxelNeighborFormat == null)
            {
                Debug.Log("null cvox nei format");
                yield return new WaitForSeconds(.2f);
            }
            cvoxelNeighborFormat.SetBuffersWith(neighborChunkGenData);
            cvoxelNeighborFormat.callNeighborFormatKernels();
            yield return new WaitForEndOfFrame();
            cvoxelFaceCopy.callFaceCopyKernel();
            yield return new WaitForEndOfFrame();
            yield return new object();
        }

        private void updateChunkGenData(ChunkGenData chunkGenData)
        {
            // DEbug
            // cvoxelFaceCopy.DebugSetSolids(chunkGenData);
            // end debug

            chunkGenData.displays = cvoxelFaceCopy.CopySolidArraysUsingCounts(); // WANT

            // write center chunk display arrays
            Debug.Log("writing at pos: " + chunkGenData.chunkPos + " : " + chunkGenData.displays.ToString());
            SerializedChunk.SerializedDisplayBuffers.WriteLODArrays(chunkGenData.displays, chunkGenData.chunkPos);
            Chunk.MetaData.Write(chunkGenData);
        }


        IEnumerator ComputeGenDataFAKE(IntVector3 chunkPos)
        {
            ChunkGenData c = FakeChunkData.StairsGenData(chunkPos, vGenConfig.ChunkSize, 5);
            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(.05f);
            yield return c;
        }

        IEnumerator ComputeGenData(IntVector3 chunkPos)
        {
            cvoxelMapData.callClearMapBuffersKernel();
            yield return new WaitForEndOfFrame();

            cvoxelMapData.callPerlinMapGenKernel(chunkPos);
            yield return new WaitForEndOfFrame();

            /*
            cvoxelMapData.DebugBuffs();

            cvoxelMapFormat.callFaceGenKernels();
            yield return new WaitForEndOfFrame();

            cvoxelMapFormat.callFaceCopyKernel();
            yield return new WaitForEndOfFrame();
            cvoxelMapFormat.DebugBuffs();

            */
            ChunkGenData c = cvoxelMapData.CopyArrays();
            c.chunkPos = chunkPos;
            //c.displays = cvoxelMapFormat.CopySolidArraysUsingCounts();

            yield return c;
        }

        public IEnumerator computeGenDataPleasePurgeMe(IntVector3 chunkPos, Action<ChunkGenData> callback)
        {
            cvoxelMapData.callClearMapBuffersKernel();
            yield return new WaitForEndOfFrame();

            cvoxelMapData.callPerlinMapGenKernel(chunkPos);
            yield return new WaitForEndOfFrame();

            var voxels = CVoxelMapFormat.BufferCountArgs.GetData<VoxelGenDataMirror>(cvoxelMapData.MapVoxels);

            callback(new ChunkGenData
            {
                voxels = voxels,
                chunkPos = chunkPos
            });
        }

        #endregion

        // TODO: compute chunk given existing ChunkGenData 
        // no need to call PerlinMapGenKernel

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
            cvoxelNeighborFormat.Release();
            cvoxelFaceCopy.Release();
        }

    }
}
