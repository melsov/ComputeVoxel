using UnityEngine;
using Mel.Trees;
using Mel.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VoxelPerformance;
using System.Threading;
using Mel.FakeData;
using System.Collections;
using g3;

namespace Mel.VoxelGen
{
    public class OctRaycast : MonoBehaviour
    {
        VGenConfig vGenConfig;
        [SerializeField] RaycastDisplay raycastDisplayPrefab;
        VoxelOctree<uint> tree;

        RaycastDisplay rayDisplay;
        ComputeBuffer buffer {
            get { return rayDisplay.buffer; }
            set { rayDisplay.buffer = value; }
        }

        List<uint> currentVoxels = new List<uint>();


        //TODO: possible to use counting buffers to
        // avoid releasing resetting like this?
        void ResetBufferData(uint[] nextData)
        {
            BufferUtil.ReleaseBuffers(buffer);

            if(nextData.Length == 0) { return; }

            buffer = new ComputeBuffer(nextData.Length, sizeof(uint));
            buffer.SetData(nextData);
        }

        int fake;
        public List<Ray3f> rayStepsDebug;
        private List<VoxelOctree<uint>.DBUGColorBounds> debugTraversal;

        private void Start()
        {
            vGenConfig = FindObjectOfType<VGenConfig>();

            FakeInit();
        }

        private void FakeInit()
        {
            Init(IntVector3.zero, FakeChunkData.Stairs(vGenConfig.ChunkSize));
        }

        private void testCanDoPlainVanillaThreading()
        {
            var thStart = new ThreadStart(doLogic);
            thStart += () =>
            {
                Debug.Log("This happened. fake is: " + fake);

            };
            var thread = new Thread(thStart); 
            thread.Start();
        }

        void doLogic()
        {
            try
            {
                fake++;
            } catch (ThreadAbortException)
            {
                //won't catch
            } finally
            {

            }
        }

        //CONSIDER: we will want to define this class's relationship to Chunk at some point

        public void Init(IntVector3 chunkPos, uint[] voxels)
        {
            int depth = vGenConfig.hilbertBits;

            Vector3 center = vGenConfig.ChunkPosToPos(chunkPos) + vGenConfig.ChunkSize / 2f;
            var bounds = new Bounds(center, vGenConfig.ChunkSize);
            tree = new VoxelOctree<uint>(depth, bounds);

            foreach(uint vox in voxels)
            {
                IntVector3 pos = IntVector3.FromUint256(vox);
                tree.Set(vox, pos.ToVector3());
            }

            // instantiate or just set up raycast display
            rayDisplay = Instantiate(raycastDisplayPrefab);
            rayDisplay.transform.SetParent(transform);
            rayDisplay.transform.localPosition = vGenConfig.ChunkPosToPos(chunkPos);

            //ComputeBuffer b = BufferUtil.CreateWith(voxels);

            rayDisplay.initialize(null);
            //ResetBufferData(voxels); //test


            StartCoroutine(Scan()); //want
        }

        private IEnumerator Scan()
        {
            while(true)
            {
                updateCurrentVoxels();
                ResetBufferData(currentVoxels.ToArray());
                yield return new WaitForEndOfFrame();
            }
        }

        private void updateCurrentVoxels()
        {
            currentVoxels.Clear();
            //var fakes = FakeChunkData.Stairs(vGenConfig.ChunkSize);
            //int offset = (int)(Time.time / 3f) % 5 + 1;

            //for(int i=0; i<fakes.Length; ++i)
            //{
            //    if(i % offset == 0)
            //    {
            //        currentVoxels.Add(fakes[i]);
            //    }
            //}

            foreach (Ray ray in CamGeometry.NearClipRays(Camera.main, 5))
            {
                uint leaf;

                if (tree.GetFirstRayhit(ray, out leaf, out debugTraversal, out rayStepsDebug))
                {
                    currentVoxels.Add(leaf);
                }
            }
        }

        public Bounds bounds { get { return tree.bounds; } }

        bool IsVisible()
        {
            return GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(Camera.main), bounds);
        }

        private void OnDestroy()
        {
            BufferUtil.ReleaseBuffers(buffer);
        }
    }
}
