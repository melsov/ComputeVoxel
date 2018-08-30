using Mel.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Mel.VoxelGen
{
    public class HeightMapBuffers : MonoBehaviour
    {
        Dictionary<IntVector2, ComputeBuffer> storage = new Dictionary<IntVector2, ComputeBuffer>();

        VGenConfig _vg;
        VGenConfig vGenConfig {
            get {
                if(!_vg) { _vg = GameObject.FindObjectOfType<VGenConfig>(); }
                return _vg;
            }
        }

        ComputeBuffer GetOrCreate(IntVector3 pos)
        {
            if (storage.ContainsKey(pos.xz))
            {
                return storage[pos.xz];
            }
            var buff = new ComputeBuffer(vGenConfig.ColumnFootprint.Area, sizeof(uint));
            storage.Add(pos.xz, buff);
            return buff;
        }

        public ComputeBuffer Get(IntVector3 pos)
        {
            return GetOrCreate(pos);
        }

        private void OnDestroy()
        {
            BufferUtil.ReleaseBuffers(storage.Values.ToArray());
        }
    }
}
