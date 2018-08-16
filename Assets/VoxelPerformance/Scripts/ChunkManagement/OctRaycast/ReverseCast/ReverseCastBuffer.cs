using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mel.Math;

namespace Mel.VoxelGen
{
    public class ReverseCastBuffer : MonoBehaviour
    {
        VGenConfig _vg;
        VGenConfig vGenConfig {
            get {
                if(!_vg) { _vg = FindObjectOfType<VGenConfig>(); }
                return _vg;
            }
        }

        ComputeBuffer buffer;
        [SerializeField] ComputeShader reverseShader;

        [SerializeField] Shader geomShader;
        Material material;
        [SerializeField] Transform globalLight;
        int clearKernel;

        Dictionary<IntVector3, ReverseCaster> lookup = new Dictionary<IntVector3, ReverseCaster>();

        private void Awake()
        {
            buffer = new ComputeBuffer(
                vGenConfig.ReverseCastBufferSize, 
                System.Runtime.InteropServices.Marshal.SizeOf(new VGenConfig.RevCastDataMirror()));

            material = new Material(geomShader);
            clearKernel = reverseShader.FindKernel("ClearCastBuffer");
            reverseShader.SetBuffer(clearKernel, "Result", buffer);
            material.SetBuffer("_displayPoints", buffer);
            CallClearKernel();
        }

        void CallClearKernel()
        {
            reverseShader.Dispatch(clearKernel, vGenConfig.ReverseCastClearKernelThreadCount.x, vGenConfig.ReverseCastClearKernelThreadCount.y, 1);
        }

        public void SetCaster(Chunk chunk)
        {
            var caster = new ReverseCaster();
            caster.Init(
                reverseShader,
                chunk.display,
                buffer,
                vGenConfig.ChunkPosToPos(chunk.ChunkPos),
                vGenConfig);
            if (lookup.ContainsKey(chunk.ChunkPos))
            {
                lookup[chunk.ChunkPos] = caster;
            }
            else
            {
                lookup.Add(chunk.ChunkPos, caster);
            }
        }

        private void OnRenderObject()
        {
            callCasters();

            material.SetPass(0);
            material.SetVector("_cameraPosition", Camera.main.transform.position);
            material.SetFloat("_mipStretch", 1f); 
            material.SetVector("_globalLight", globalLight.position);

            Graphics.DrawProcedural(MeshTopology.Points, buffer.count);

        }

        private void callCasters()
        {
            foreach(var key in lookup.Keys)
            {
                lookup[key].CallIfVisible();
            }
        }

        private void OnDestroy()
        {
            BufferUtil.ReleaseBuffers(buffer);
        }
    }
}
