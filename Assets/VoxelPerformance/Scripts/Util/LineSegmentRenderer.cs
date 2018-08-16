using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Vertex
{
    public Vector4 pos;
    public Color color;

    public static implicit operator Vertex(Vector3 v) {
        return new Vertex()
        {
            pos = v,
            color = Color.white
        };
    }
}

public class LineSegmentRenderer : MonoBehaviour
{
    public Shader geomShader;
    public float Size = 0.05f;
    Material material;
    ComputeBuffer outputBuffer;
    List<Vertex> points;

    //
    // TODO: test hilbertizing sets of vec3s
    // that fit into some cube (maybe just 4^3)
    //

    // Use this for initialization
    void Start()
    {
        material = new Material(geomShader);
        points = new List<Vertex>();
    }

    void SetupTest()
    {
        Vector3 a = Vector3.zero;
        Vector3 b = Vector3.zero;
        float ang; float rad = 5f;
        for (int i = 0; i < 10; i++)
        {
            for(int j = 0; j < 50; ++j)
            {
                a.y += .02f;
                ang = Mathf.PI * 2f * j / 50f;
                a.x = Mathf.Cos(ang) * rad;
                a.z = Mathf.Sin(ang) * rad;

                ang = Mathf.PI * 2f * (j + .6f) / 50f;
                b.y = a.y;
                b.x = Mathf.Cos(ang) * rad;
                b.z = Mathf.Sin(ang) * rad;

                AddLineSegmentGS(
                    a, b
                    );

            }
        }

        Apply(points.ToArray());
    }

    void AddLineSegmentGS(Vertex a, Vertex b)
    {
        points.Add(a);
        points.Add(b);
    }

    public void Apply(Vertex[] points)
    {
        if(outputBuffer != null) { outputBuffer.Release(); outputBuffer = null; }

        outputBuffer = new ComputeBuffer(points.Length, System.Runtime.InteropServices.Marshal.SizeOf(new Vertex())); 
        outputBuffer.SetData(points);
    }

    public void OnRenderObject()
    {
        if(outputBuffer == null)
        {
            return;
        }
        material.SetPass(0);
        material.SetBuffer("buf_Points", outputBuffer);
        material.SetFloat("_Size", Size);
        material.SetVector("_camPos", Camera.main.transform.position);
        Graphics.DrawProcedural(MeshTopology.LineStrip, outputBuffer.count);
    }

    public void OnDestroy()
    {
        if(outputBuffer != null)
            outputBuffer.Release();
        outputBuffer = null;
    }
}