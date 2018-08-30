using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;

namespace Mel.Trees
{
    public class MortonTester : MonoBehaviour
    {

        [MenuItem("MEL/TestMortons")]
        static void TestMortons()
        {
            var m = new Morton.morton3d(1, 2, 3);
            m = m.incX();
            ulong x, y, z;
            m.decode(out x, out y, out z);

            Debug.Log(string.Format("xyz: {0}, {1}, {2}", x, y, z));
        }
    }
}

#endif
