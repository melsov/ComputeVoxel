using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mel.Math;
using Mel.Storage;
using Mel.VoxelGen;
using System.Linq;
using System;
using VoxelPerformance;
using UnityEditor;
using UnityEngine.Assertions;
using Mel.FakeData;
using Mel.JobCallback;
using Mel.Extensions;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Mel.VoxelGen
{

    public class AreaGenerator : MonoBehaviour
    {
        public struct GenArea
        {
            public IntBounds3 bounds;
            private string KeyTag {
                get { return "GenArea"; }
            }
            public bool Dirty {
                get {
                    if (PlayerPrefs.HasKey(bounds.ToKey(KeyTag))) {
                        return PlayerPrefs.GetInt(bounds.ToKey(KeyTag)) == 1;
                    }
                    return true;
                }
                set {
                    PlayerPrefs.SetInt(bounds.ToKey(KeyTag), value ? 1 : 0);
                }
            }

        }

        [SerializeField]
        IntVector3 buildArea;

        [SerializeField]
        IntVector3 initialOrigin;

        [SerializeField, Header("Cube size surrounding player where the world should be built") ]
        IntVector3 buildBoundsSize;

        [SerializeField]
        VGenConfig vGenConfig;

        [SerializeField]
        ChunkForge forge;

        [SerializeField]
        int debugMaxAreas = 5;

        [SerializeField]
        ProcessColumn processColumn;

        [SerializeField]
        Transform player;

        [SerializeField]
        Transform buildAroundTarget;


        HashSet<IntVector3> completeChunks = new HashSet<IntVector3>();
        public delegate void OnChunkDataReady(IntVector3 chunkPos);
        public OnChunkDataReady onChunkDataReady;

        ColumnMap3<NeighborChunkGenData> processSet = new ColumnMap3<NeighborChunkGenData>();
        Dictionary<IntVector2, ColumnAndHeightMap<ChunkGenData>> columnLookup = new Dictionary<IntVector2, ColumnAndHeightMap<ChunkGenData>>();
        HashSet<IntVector3> notYetProcessed = new HashSet<IntVector3>();

        Dictionary<IntVector3, ChunkGenData> lookup = new Dictionary<IntVector3, ChunkGenData>();
        ConcurrentDictionary<IntVector3, Task<ChunkGenData>> chunkGenTasks = new ConcurrentDictionary<IntVector3, Task<ChunkGenData>>();

        ChunkGenForgeQueue _cgfq;
        ChunkGenForgeQueue chunkGenForgeQueue {
            get { if (!_cgfq) { _cgfq = GameObject.FindObjectOfType<ChunkGenForgeQueue>(); } return _cgfq; }
        }

        [SerializeField] ChunkCompute compute;

        TargetCenteredChunkBounds targetIterator;

        IntBounds3 genBounds {
            //TODO: vGenConfig place bounds within world 
            get { return new IntBounds3 { start = initialOrigin, size = buildArea }; }
        }

        GenArea genArea {
            get {
                return new GenArea { bounds = genBounds };
            }
        }

        List<IntVector3> GenAreaPositions {
            get {
                return genArea.bounds.IteratorXZYTopDown.ToList();
            }
        }

        bool AdvanceGenArea()
        {
            initialOrigin = targetIterator.Next();
            //initialOrigin.x += genArea.bounds.size.x;
            return true;
            //IntBounds3 next;
            //if (nextArea.Next(out next))
            //{
            //    initialOrigin = next.start;
            //    return true;
            //}
            //return false;
        }

        private void Awake()
        {
            targetIterator = new TargetCenteredChunkBounds(buildAroundTarget, buildBoundsSize, buildArea, vGenConfig, null); //TODO: add a moveMethod
        }

        private void Start()
        {
            Generate();
        }


        [MenuItem("MEL/Test WGen Iterators #&r")]
        static void TestWGIs()
        {
            var targ = FindObjectOfType<AreaGenerator>();
            targ.Generate();
        }


        void Generate()
        {
            
            _Generate();
        }

        async void _Generate()
        {
            while (true)
            {
                if (AdvanceGenArea())
                {
                    GenAreaMetaData gamd = GenAreaMetaData.Read(genArea.bounds.start);
                    if(!vGenConfig.RegenerateAllChunks && gamd.buildStatus == GenAreaMetaData.BuildStatus.NeighborFormatted)
                    {
                        foreach(var pos in GenAreaPositions) { CompletedChunkAt(pos); }
                        continue;
                    }
                    var completedColumnMap = await GenerateAsync();

                    gamd.buildStatus = GenAreaMetaData.BuildStatus.NeighborFormatted;
                    gamd.Write(genArea.bounds.start);

                }
                else
                {
                    Debug.Log("No more areas to build");
                }
            }
        }

        void purgeLookup()
        {
            if(lookup.Count < TotalCalculatedChunks * 4) { return; }

            int dim = genArea.bounds.size.MaxComponent * 2; 
            List<IntVector3> deletes = new List<IntVector3>();

            foreach(var pos in lookup.Keys)
            {
                var dif = pos - genArea.bounds.start;
                if(dif.SquareMagnitude > dim*dim + 4)
                {
                    deletes.Add(pos);
                }
            }

            foreach(var key in deletes)
            {
                lookup.Remove(key);
            }

        }

        int TotalCalculatedChunks { get { return IterBounds.AreaIncludingNeighborsXYZ(genArea.bounds.size); } }

        // TODO: have the genArea update when this
        // whole process is done
        // what do we want?
        // a collection of available chunk coords

            // TODO: add a mechanism to know whether we need to re compute all of these chunks at all
            // however also have a debug-compute-anyway flag

        async Task<ColumnMap3<NeighborChunkGenData>> GenerateAsync()
        {
            // for the genArea

            // possibly:
            // bootstrap some seed data. 
            // such as a known full light voxel 
            // maybe? this function runs once per job: e.g. per light distribution, structure distribution

            ////
            // bootstrap data comes from somewhere
            // foreach chunk containing some bootstrap data:
            // Add the data for that chunk

            var area = genArea;


            purgeLookup();

            processSet.Clear();
            notYetProcessed.Clear();
            chunkGenTasks.Clear();
            columnLookup.Clear();

            var positions = GenAreaPositions;
            foreach (var centerChunkPos in positions) { notYetProcessed.Add(centerChunkPos); }

            //
            // TODO: use metadata. has been processed. <<--- yeah
            // Don't need to re-compute processed chunks.
            // 

            // Per column:
            // bounds. IteratorXZ etc..
            // then get the XZ neighbors also... the ones not contained in bounds.xz
            // Get a Column and HeightMap from compute chunks<ChunkGenData>
            // with each of these add the CGD to lookup

            //TODO: for structures we need all 27
            //and kind of want Octrees

            var outerShellArea = area.bounds.ExpandedBordersAdditive(IntVector3.one);

            foreach (var centerChunkPos in IterBounds.IteratorXZAtY(outerShellArea, outerShellArea.end.y))
            {
                // NOTE: possibly we already computed this column as a neighbor outside bounds
                // need a mechanism (player prefs?) for checking if we've already written these columns/chunks somewhere

                var col = new Column<ChunkGenData>(centerChunkPos.xz);

                col.SetRangeToDefault(outerShellArea.start.y, outerShellArea.end.y);

                //TRY:
                // ComputeColumnAtAsync only does the perlin gen
                var colheight = await compute.ComputeColumnAtAsync(col, (IntVector3 pos) =>
                {
                    if(lookup.ContainsKey(pos)) {
                        return lookup[pos];
                    }
                    return null;
                });

                columnLookup.AddOrSet(col.position, colheight);
            }



            foreach (var centerChunkPos in positions)
            {
                processSet.SetItem(centerChunkPos, new NeighborChunkGenData(centerChunkPos));
            }

            foreach (var centerChunkPos in outerShellArea.IteratorXYZ) 
            {
                var gendata = columnLookup[centerChunkPos.xz].column[centerChunkPos.y];
                lookup.AddOrSet(centerChunkPos, gendata);
            }

            foreach (var col in processSet.GetColumns())
            {
                foreach(var nei in col.Values)
                {
                    nei.GatherData27((IntVector3 pos) => {
                        try
                        {
                            return lookup[pos];
                        } catch (IndexOutOfRangeException ior)
                        {
                            throw ior;
                        }
                    });
                }
                var colAndHeight = new ColumnAndHeightMap<NeighborChunkGenData>
                {
                    column = col,
                    heightMap = columnLookup[col.position].heightMap
                };
                await _ProcessColumn(colAndHeight);
            }

            return processSet;
        }

        async Task _ProcessColumn(ColumnAndHeightMap<NeighborChunkGenData> colAndHeightMap)
        {
            // compute runs a compute shader (a version of mesh-gen) that also gets 
            await compute.PostProcessColumnAsync(colAndHeightMap, (IntVector3 chunkPos) =>
            {
                CompletedChunkAt(chunkPos);
            });

            //a (massive?) ExistsMap27
            /*
            await processColumn.Process(colAndHeightMap, (IntVector3 chunkPos) =>
            {
                CompletedChunkAt(chunkPos);
            });
            */
        }

        void CompletedChunkAt(IntVector3 chunkPos)
        {
            Debug.Log("got completed ");
            completeChunks.Add(chunkPos);
            onChunkDataReady(chunkPos);
        }

        private HeightMap GetHeightMap(IntVector2 position)
        {
            throw new NotImplementedException();
        }


        //async Task<NeighborChunkGenData> GetNeighborChunkGenAtAsync(NeighborChunkGenData nei)
        //{
        //    return await nei.GatherDataAsync(GetChunkAtAsync);
        //}

 

        //async Task<ChunkGenData> GetChunkAtAsync(IntVector3 pos)
        //{
        //    return await chunkGenTasks.GetOrAdd(pos, _GetChunkAtFromEnumeralbleAsync(pos)); 
        //}

        //async Task<ChunkGenData> _GetChunkAtFromEnumeralbleAsync(IntVector3 pos)
        //{
        //    return (ChunkGenData)(await _GetChunkAtAsyncFake(pos));
        //}

        //Dictionary<IntVector3, int> debugCalculatedChunkCounts = new Dictionary<IntVector3, int>();


        ////TODO: replace with a chunkForge method that computes a whole column at a time
        //// saves arrays as ChunkGenData and also saves the height map 
        //// returns a ColumnAndHeightMap
        //IEnumerator _GetChunkAtAsyncFake(IntVector3 pos)
        //{
        //    if (lookup.ContainsKey(pos))
        //    {
        //        yield return lookup[pos];
        //    }
        //    else
        //    {
        //        var fakeCData = FakeChunkData.StairsGenData(pos, vGenConfig.ChunkSize, 6);

        //        var count = debugCalculatedChunkCounts.GetOrAdd(pos, 0);

        //        //DBUG
        //        debugCalculatedChunkCounts[pos]++;
        //        if(debugCalculatedChunkCounts[pos] > 1)
        //            Debug.LogWarning("calc chunk " + debugCalculatedChunkCounts[pos] + " times");


        //        lookup.Add(pos, fakeCData);
        //        yield return new WaitForSeconds(UnityEngine.Random.Range(.1f, .5f));
        //        yield return fakeCData;
        //    }
        //}

        //void GetChunkAt(IntVector3 pos, Action<ChunkGenData> callback)
        //{
        //    if(lookup.ContainsKey(pos))
        //    {
        //        callback(lookup[pos]);
        //        return;
        //    }

        //    StartCoroutine(_GetChunkAt(pos, callback));
        //}

        //private IEnumerator _GetChunkAt(IntVector3 pos, Action<ChunkGenData> callback)
        //{
        //    var fakeCData = FakeChunkData.StairsGenData(pos, vGenConfig.ChunkSize);
        //    yield return new WaitForSeconds(.1f);
        //    callback(fakeCData);
        //}


    }
}
