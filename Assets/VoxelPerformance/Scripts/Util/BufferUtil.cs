﻿using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mel.Math;

namespace Mel.VoxelGen
{
    public static class BufferUtil
    {
        public static void ReleaseBuffers(params ComputeBuffer[] buffers)
        {
            for(int i=0; i<buffers.Length; ++i)
            {
                var buff = buffers[i];
                if(buff != null) { buff.Release(); }
                buffers[i] = null;
            }
        }

        public static IntVector3 GetThreadGroupSizes(ComputeShader shader, int kernel)
        {
            UIntVector3 result = IntVector3.zero;
            shader.GetKernelThreadGroupSizes(kernel, out result.x, out result.y, out result.z);
            return result;
        }

        public static ComputeBuffer CreateWith<T>(T[] voxels)
        {
            var result = new ComputeBuffer(voxels.Length, System.Runtime.InteropServices.Marshal.SizeOf(voxels[0]));
            result.SetData(voxels);
            return result;
        }
    }
}
