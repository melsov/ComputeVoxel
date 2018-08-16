using UnityEngine;
using Mel.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mel.Util
{
    public class DebugDrawVoxelOrder : MonoBehaviour
    {

        LineRenderer _lr;
        LineRenderer lr {
            get {
                if (!_lr)
                {
                    _lr = this.GetOrAddComponent<LineRenderer>();
                }
                return _lr;
            }
        }


        public static void Draw(int[] voxels, float xOffset = 0f)
        {
            var ddv =  Resources.Load<DebugDrawVoxelOrder>("DebugDrawVoxelOrder");
            ddv._draw(voxels, xOffset);
        }

        void _draw(int[] voxels, float xOffset = 0f)
        {
            IntVector3 offset = IntVector3.right * xOffset;
            IntVector3 end;

            lr.positionCount = voxels.Length;

            if(voxels.Length == 0) { print("zero voxels"); }
            for (int i = 0; i < voxels.Length; ++i)
            {
                end = IntVector3.FromVoxelInt(voxels[i]);
                lr.SetPosition(i, end + offset);
            }
        }

    }
}
