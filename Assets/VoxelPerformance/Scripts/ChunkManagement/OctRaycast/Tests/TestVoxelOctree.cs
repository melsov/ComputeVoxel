using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mel.Trees;
using g3;
using Mel.Util;
using UnityEngine.UI;
using System.Collections;

namespace Mel.Tests
{
    [CustomEditor(typeof(TestVoxelOctree))]
    public class TestVoxOctreeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }

    public class TestVoxelOctree : MonoBehaviour
    {
        VoxelOctree<Vector3> tree;
        [SerializeField] float size = 20;
        [SerializeField, Range(0, 24)] int depth = 2;
        private bool shouldDrawGizmos;

        [SerializeField] Vector3 rayO = new Vector3(0f, -3f, -15f);
        [SerializeField] Vector3 rayPointingTo = new Vector3(10f, 13f, -11f);
        [SerializeField] Text dText;

        [SerializeField] LineSegmentRenderer lsr;

        List<Vertex> showPoints = new List<Vertex>();

        private void Start()
        {
            transform.DestroyAllChildrenImmediate();
            test();
            showTraversals();
            StartCoroutine(periodicShowTraversals());
        }

        private IEnumerator periodicShowTraversals()
        {
            while (true)
            {
                yield return new WaitForSeconds(.3f);
                showTraversals();
            }
        }

        private void test()
        {
            tree = new VoxelOctree<Vector3>(depth, new Bounds(Vector3.zero, Vector3.one * size));
            addTestData();

            //showTraversals();
        }

        private IEnumerable<Ray3f> GetSomeRays()
        {
            yield return new Ray3f(rayO, rayPointingTo - rayO);
        }

        GameObject _marker;
        GameObject marker {
            get {
                if(!_marker) { _marker = GameObject.CreatePrimitive(PrimitiveType.Sphere); }
                return _marker;
            }
        }

        private void showTraversals()
        {
            showPoints.Clear();
            int counter = 0;
            foreach (var ray in GetSomeRays()) // TestEnterExitVectors.SomeRandomRays(1, 22))
            {
                List<VoxelOctree<Vector3>.DBUGColorBounds> traversal;
                List<Ray3f> raySteps;
                Vector3 leaf;
                string info = string.Empty;
                Vertex a, b;
                if(tree.GetFirstRayhit(ray, out leaf, out traversal, out raySteps))
                {
                    Debug.Log("got a hit");
                    a = new Vertex() { pos = (Vector3)ray.origin - Vector3.up, color = Color.red };
                    b = new Vertex() { pos = (Vector3)(ray.origin + ray.direction * ray.direction.Dot((Vector3f)leaf - ray.origin)) - Vector3.up, color = Color.red };

                    marker.transform.SetParent(transform);
                    marker.transform.localPosition = leaf;

                } else
                {
                    Debug.Log("no hit");
                    a = new Vertex() { pos = (Vector3)ray.origin, color = Color.cyan };
                    b = new Vertex() { pos = (Vector3)ray.positionAt(30f), color = Color.cyan };

                }
                showPoints.Add(a); showPoints.Add(b);
                showPoints.Add(new Vertex() { pos = a.pos, color = Color.clear });


                StringBuilder sb = new StringBuilder();
                int index = 0;
                foreach (var bou in traversal)
                {
                    if (bou.validVertex)
                    {
                        if (index++ == 0)
                        {
                            showPoints.Add(new Vertex() { pos = bou.vertex.pos, color = Color.clear });
                        }
                        showPoints.Add(bou.vertex);
                    }
                    sb.Append(string.Format("{0} | {1} ", bou.text, Environment.NewLine));
                }

                lsr.Apply(showPoints.ToArray());
                dText.text = sb.ToString();



                //Gizmos.DrawSphere(ray.origin, .3f);
                counter++;

            }
            //foreach (var node in tree.GetAllNonNullNodes())
            //{
            //    if (node.isLeaf)
            //    {
            //        Gizmos.color = Color.white;
            //        Gizmos.DrawWireCube(node.data, tree.LeafSize() * .9f);
            //    }
            //}
        }

        void DrawGizmoArrow(Ray3f r, float rad = .5f)
        {
            if(r.direction.LengthSquared < Mathf.Epsilon) { return; }
            Gizmos.DrawSphere(r.origin, rad);
            Gizmos.DrawLine(r.origin, r.origin + r.direction);
        }

        private void addTestData()
        {
            var points = new Vector3[]
            {
                tree.bounds.max - Vector3.one * .3f,
                tree.bounds.min + Vector3.one * .3f,
                tree.bounds.center - Vector3.one * .3f,
                tree.bounds.center + new Vector3(1, -1, 1) * .3f,
                tree.bounds.min + Vector3.Scale(tree.bounds.max , Vector3.right) + Vector3.one * .3f,
            };

            foreach(var point in points)
            {
                tree.Set(point, point);
            }

            transform.DestroyAllChildrenImmediate();
            foreach(var node in tree.GetAllNonNullBoundsNodes())
            {
                if (node.node.isLeaf)
                {
                    var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.SetParent(transform);
                    cube.transform.localPosition = node.bounds.center;
                    cube.transform.localScale = node.bounds.size;
                }
            }

            //foreach (var b in tree.GetAllNonNullBoundsNodes())
            //{
            //    Gizmos.color = b.node.isLeaf ? Color.white : new Color(.2f, .1f, .7f);// prettyBoundsEncoding(b.bounds);
            //    Gizmos.DrawWireCube(b.bounds.center, b.bounds.size * (b.node.isLeaf ? .88f : 1f));
            //}
        }

        Color prettyBoundsEncoding(Bounds b)
        {
            Vector3 relPos = b.min - tree.bounds.min;
            relPos /= size;
            var relSize = b.size / size;
            Vector4 c = Vector4.zero;
            c.x = relPos.x;
            c.y = relPos.y / 2 + relSize.y / 2;
            c.z = relSize.z;
            c.w = 1f;
            return c;
        }

        //private void OnDrawGizmos()
        //{
            
        //}

        //private void OnDrawGizmosSelected()
        //{
        //    //test();
        //}

      
    }
}
