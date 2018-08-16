using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mel.Math;
using Mel.Util;
using Mel.VoxelGen;
#if UNITY_EDITOR
using UnityEditor;

namespace MIConvexHull
{
    public class TestMIConvexHull : MonoBehaviour
    {
        [MenuItem("MEL/Test MI Convex Hull #%w")]
        static void TestMICHull()
        {
            var vs = someVertex3s();
            Debug.Log("input v count: " + vs.Count);

            var chull = ConvexHull.Create<Vertex3, MConvexFace<Vertex3>>(vs);

            //createMesh(chull);

            Debug.Log("chull v count: " + chull.Points.ToList().Count);
            int faceCount = 0;
            int faceIndex = 0;
            foreach(var face in chull.Faces)
            {
                faceCount += face.Vertices.Length;
                drawFace(face, faceIndex++);
            }

            Debug.Log("face count " + faceCount);

            Vector3 offset = Vector3.right * 5f;
            var points = chull.Points.ToList();
            for (int i = 1; i < points.Count; ++i)
            {
                Debug.DrawLine(points[i - 1].Vec3 + offset, points[i].Vec3 + offset, Color.blue);
            }
        }

        private void Start()
        {
            var vs = someVertex3s();
            var chull = ConvexHull.Create<Vertex3, MConvexFace<Vertex3>>(vs);
            createMesh(chull);
        }

        private void createMesh(ConvexHull<Vertex3, MConvexFace<Vertex3>> chull)
        {
            List<Vector3> verts = new List<Vector3>();
            List<Color> colors = new List<Color>();

            var points = chull.Points.OrderBy((p) => p.index).ToList();
            for (int i=0; i<points.Count; ++i)
            {
                var point = points[i];
                verts.Add(point.Vec3);
                colors.Add(ColorUtil.roygbivMod(point.index));
            }

            List<int> indices = new List<int>();
            foreach(var face in chull.Faces)
            {
                for(int i = 0; i < face.Vertices.Length; ++i)
                {
                    indices.Add(face.index(i));
                }
            }


            Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
            mesh.Clear();

            mesh.vertices = verts.ToArray();
            mesh.triangles = indices.ToArray();
            mesh.colors = colors.ToArray();
        }

        static Vector3 getCenter(MConvexFace<Vertex3> face)
        {
            Vector3 v = Vector3.zero;
            for (int i = 0; i < face.Vertices.Length; ++i)
            {
                v += face.Vertices[i].Vec3;
            }
            return v / face.Vertices.Length;
        }

        private static void drawFace(MConvexFace<Vertex3> face, int faceIndex)
        {
            Color c = ColorUtil.roygbivMod(faceIndex);
            Color altC = c;
            altC.a = .75f;
            var center = getCenter(face);
            for (int i = 1; i <= face.Vertices.Length; ++i)
            {
                Vector3 difA = face.Vertices[i - 1].Vec3 - center;
                Vector3 difB = face.Vertices[i % face.Vertices.Length].Vec3 - center;
                Debug.DrawLine(center + difA * .98f, center + difB * .98f, c);
                //Debug.DrawLine(face.Vertices[i - 1].Vec3, face.Vertices[i % face.Vertices.Length].Vec3, c);
            }
        }

       


        static List<Vertex3> someVertex3s()
        {
            var result = new List<Vertex3>();
            int count = 50;
            //float rad = 5f;
            for(int i=0; i<count; ++i)
            {
                result.Add(new Vertex3(VecExt.RandomVector3().normalized * 5f, i));
                //var rot = Quaternion.AngleAxis(i / (float)count * 360, Vector3.up);
                //float ang = i / (float)count * Mathf.PI * 2f;
                //Vector3 v = new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang));
                //for(int j = 0; j < 5; ++j)
                //{
                //    float yang = j / 5f * Mathf.PI / 2;
                //    v.y = Mathf.Cos(yang);
                //    result.Add(new Vertex3(v.normalized * rad));
                //}

            }
            return result;
        }

        
    }
}
#endif