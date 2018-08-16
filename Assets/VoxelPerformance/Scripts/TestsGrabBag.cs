using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

namespace VPTests
{
    public class TestsGrabBag : MonoBehaviour
    {
        [MenuItem("MEL/Test Things")]
        static void TestThings() {
            Debug.Log("nothing here");
        }
    }
}

#else

#endif