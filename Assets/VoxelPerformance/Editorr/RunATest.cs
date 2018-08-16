using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using HilbertExtensions;
using Mel.Math;
using Mel.ChunkManagement;
#if UNITY_EDITOR
using UnityEditor;

public class RunATest : MonoBehaviour
{

    #region Hilbert;
    [MenuItem("MEL/HilbertTest #&e")]
    static void HilbertTest()
    {
        //uint[] hindex = new uint[] {(uint)Mathf.Pow(2, 14), 7 << 12, 7 << 9, 7 << 8, 7 << 7 };
        //foreach(uint h in hindex) { _hilbertTest(h); }

        //Debug.DrawLine(Vector3.zero, Vector3.one * 100f, Color.red);
        drawHilbert();
    }

    //static void _hilbertTest(uint hindex)
    //{
    //    Debug.Log("hindex: " + Convert.ToString(hindex, 2));
    //    uint[] tindex = HilbertCurveTransform.TransposeIndex( hindex, 5, 3);
    //    foreach(uint u in tindex)
    //    {
    //        Debug.Log(Convert.ToString(u, 2));
    //    }

    //}


    private static void drawHilbert()
    {
        int n = 2;
        //int area = n * n * n;
        //uint[] hindex = new uint[area];
        Vector3 prev = Vector3.zero;
        int samples = (int)Mathf.Pow(8, n);
        for(uint i = 0; i < (uint) samples; ++i)
        {
            //uint[] tindex = HilbertCurveTransform.TransposeIndex(i, n, 3);
            uint[] xyz = HilbertCurveTransform.HilbertIndexToCoords(i, n, 3); //  tindex.HilbertAxes(n);
            Vector3 next = xyz.ToVector3();
            if(i > 0)
            {
                Debug.DrawLine(prev, next, Color.Lerp(Color.red, Color.blue,  i/(float)samples));
            }
            prev = next;
        }

        Debug.Log("axes to hilbert to flat");
        //for(uint x = 0; x < n; ++x)
        //{
        //    for(uint y = 0; y < n; ++y)
        //    {
        //        for(uint z = 0; z < n; ++z)
        //        {
        //            //uint[] xyz = new uint[] { x, y, z };
        //            //uint[] hi = xyz.HilbertIndexTransposed(n);
        //            //Debug.Log(hi.FlatHilbertIndex(n));
        //        }
        //    }
        //}
    }
    #endregion

    #region OctantIndex

    [MenuItem("MEL/TestOctantIndex")]
    static void TestOctantIndex()
    {
        int size = 32;
        for(int x=0; x<size; ++x)
        {
            for(int z=0; z<size;++z)
            {
                IntVector3 v = new IntVector3(x, 0, z);
                Debug.Log(string.Format("x {0} , z {1} oI: {2}", x, z, v.OctantIndex(size)));
            }
        }
    }
    #endregion


    #region serdes

    [MenuItem("MEL/Serdes SerDisplayBuffers")]
    static void SerDesDisplayBuffers()
    {
        SerializedChunk.TestSerDisplayBuffers();
    }

    #endregion
}



#else 

#endif
