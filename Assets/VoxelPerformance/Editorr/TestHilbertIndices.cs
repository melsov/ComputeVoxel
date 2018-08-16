using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Mel.Math;
using HilbertExtensions;
using Mel.Util;
using UnityEngine.Assertions;
#if UNITY_EDITOR
using UnityEditor;

namespace Mel.Editorr
{
    public class TestHilbertIndices : MonoBehaviour
    {

        [SerializeField]
        GameObject boundsMarkerPrefab;

        [SerializeField]
        Material boundsMarkerMat;

        [SerializeField]
        int testCubeSizeLog2 = 1;

        int hCubeSize {
            get { return (int)Mathf.Pow(2, testCubeSizeLog2); }
        }

        public static bool isDifferentHilbertCube(IntVector3 prev, IntVector3 next, int cubeSize)
        {
            var dif = (next - prev).Abs;
            return dif.x >= cubeSize || dif.y >= cubeSize || dif.z >= cubeSize; 
        }

        public static bool isDifferentHilbertCube(int prevVoxel, int nextVoxel, int cubeSize)
        {
            IntVector3 p, n;

            p.x = (prevVoxel / 65536) & 255;
            p.y = (prevVoxel / 256) & 255;
            p.z = prevVoxel & 255;

            n.x = (nextVoxel / 65536) & 255;
            n.y = (nextVoxel / 256) & 255;
            n.z = nextVoxel & 255;

            p /= cubeSize;
            n /= cubeSize;

            return p.x != n.x || p.y != n.y || p.z != n.z;
        }

        [MenuItem("MEL/Test Hilbert Indices %#d")]
        static void RunTestHIs()
        {
            //IntVector3 a = new IntVector3(3, 0, 1);
            //IntVector3 b = a;
            //b.z += 1;
            //Debug.Log(isDifferentHilbertCube((int)a.absValuesToUInt256(), (int)b.absValuesToUInt256()));

            var thi = ComponentHelper.FindOrCreateObjectOfType<TestHilbertIndices>();
            thi.TestHIndices();

        }

        public void TestHIndices()
        {
            int size = 4;
            IntVector3 dims = new IntVector3(size);

            List<IntVector3.IndexedIntVector3> vs = dims.IteratorXYZ.ToList();

            int bits = (int)Mathf.Log(size * size * size, 8);

            List<IntVector3.IndexedIntVector3> hsorted = vs.OrderBy(vox => vox.v.ToUint3().CoordsToFlatHilbertIndex(bits)).ToList();

            IntVector3 offset = new IntVector3(4);


            transform.DestroyAllChildrenImmediate();

            var lr = this.GetOrAddComponent<LineRenderer>(); // GetComponent<LineRenderer>();

            lr.positionCount = hsorted.Count;
            lr.SetPosition(0, hsorted[0] + offset);

            Dictionary<IntVector3, int> hcubes = new Dictionary<IntVector3, int>();

            IntVector3 compare = hsorted[0];
            int colorIndex = 0;

            for(int i = 1; i < hsorted.Count; ++i) 
            {
                var next = hsorted[i];
                var prev = hsorted[i - 1];

                dbDraw(prev + offset, next + offset, ColorUtil.roygbivMod(colorIndex));

                


                

                if(isDifferentHilbertCube((int)compare.AbsValuesToUInt256(), (int)next.v.AbsValuesToUInt256(), hCubeSize))
                {
                    if(!hcubes.ContainsKey(compare)) {
                        hcubes.Add(compare, 1);
                    } else
                    {
                        hcubes[compare]++;
                    }
                    addMarker(compare - (compare % hCubeSize) + offset, ColorUtil.roygbivMod(colorIndex));
                    colorIndex++;
                    //addBoundsMarker(prev - (prev % hCubeSize) + offset, c, hCubeSize);
                    compare = next;
                } 

            }

            Debug.Log("hcube entries: " + hcubes.Keys.Count );
            int total = 0;
            foreach(int c in hcubes.Values)
            {
                total += c;
                if(c > 1)
                {
                    Debug.LogWarning("what happened??? " + c);
                }
            }
            Debug.Log("total diff found: " + total);
        }




        private void OnDrawGizmosSelected()
        {

            //TestHIndices();
        }


        void addMarker(IntVector3 pos, Color c, PrimitiveType pType = PrimitiveType.Sphere)
        {
            GameObject sphere = GameObject.CreatePrimitive(pType);
            sphere.transform.position = pos;
            sphere.transform.localScale *= .2f;
            sphere.transform.SetParent(transform);

            Renderer rere = sphere.GetComponent<Renderer>();
            Shader sh = rere.sharedMaterial.shader;
            Material mat = new Material(sh);
            rere.sharedMaterial = mat;
            mat.color = c;

        }

        private void dbDraw(IntVector3 from, IntVector3 to, Color c)
        {
            Debug.DrawLine(from, to, c, 60f, false);

        }

        void addBoundsMarker(IntVector3 pos, Color c, int cubeSize, PrimitiveType pType = PrimitiveType.Cube)
        {
            GameObject cube = GameObject.CreatePrimitive(pType);
            cube.transform.position = pos + Vector3.one * (cubeSize/2f);
            cube.transform.localScale *= cubeSize * .9f;
            cube.transform.SetParent(transform);


            Renderer rere = cube.GetComponent<Renderer>();
            Shader sh = rere.sharedMaterial.shader;
            Material mat = new Material(sh);
            rere.sharedMaterial = mat;
            c.a = .5f;
            mat.color = c;
        }
    }
}

#endif