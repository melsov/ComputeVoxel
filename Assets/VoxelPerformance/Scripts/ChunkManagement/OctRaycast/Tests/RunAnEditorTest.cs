using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mel.VoxelGen;
using Mel.Math;
#if UNITY_EDITOR
namespace Mel.RunATest
{ 
    public class RunAnEditorTest : MonoBehaviour
    {
        [MenuItem("MEL/Test cross neibs 12")]
        static void TestCrossNeibs12()
        {
            CrossBits12.TestAndMakeTable();
        }

        [MenuItem("MEL/Print Cube27")]
        static void PrintCube27()
        {
            StringBuilder s = new StringBuilder();
            foreach (var pos in new IntVector3(3).IteratorXYZ)
            {
                s.Append(string.Format("int3({0}), {1}", (pos - IntVector3.one).ToShortString(), ""));
            }
            Debug.Log(s.ToString());
        }
    }
}

#endif