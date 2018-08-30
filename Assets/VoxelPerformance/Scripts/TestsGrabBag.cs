using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Mel.Math;
#if UNITY_EDITOR
using UnityEditor;

namespace VPTests
{
    public class TestsGrabBag : MonoBehaviour
    {
        [MenuItem("MEL/Print Rubik's Rel positions")]
        static void TestThings() {
            StringBuilder sb = new StringBuilder();
            foreach(var p in new IntVector3(3).IteratorXYZ)
            {
                sb.Append(string.Format("int3({0}), ", (p - IntVector3.one).ToShortString()));
            }
            Debug.Log(sb.ToString());
        }
    }
}

#else

#endif