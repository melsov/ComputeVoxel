using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mel.Math;

namespace Mel.VoxelGen
{
    public class ReverseCaster
    {
        ComputeShader shader;
        int reverseCastKernel;
        ComputeBuffer _display;
        ComputeBuffer ledger;
        Vector3 chunkGlobalPos;
        private IntVector2 buffResolution;
        private VGenConfig vGenConfig;

        public void Init(
            ComputeShader shader, 
            ComputeBuffer _display, 
            ComputeBuffer ledger, 
            Vector3 chunkGlobalPos, 
            VGenConfig vGenConfig)
        {
            this.shader = shader;
            this._display = _display;
            this.ledger = ledger;
            this.chunkGlobalPos = chunkGlobalPos;
            this.buffResolution = vGenConfig.ReverseCastBufferResolutionXY;
            this.vGenConfig = vGenConfig;
            reverseCastKernel = shader.FindKernel("ReverseCast");

            InitShader();
        }

        private void InitShader()
        {
            shader.SetBuffer(reverseCastKernel, "Result", ledger);
            if(_display == null)
            {
                return;
            }
            shader.SetBuffer(reverseCastKernel, "_displayBuffer", _display);
            shader.SetInt("_bufferSize", _display.count);
            shader.SetVector("chunkGlobalPos", chunkGlobalPos);


            shader.SetInts("_bufferResolutionXY", buffResolution.x, buffResolution.y);
        }

        bool goodData {
            get { return _display != null && ledger != null; }
        }

        public void CallIfVisible()
        {
            if(!goodData) { return; }

            Bounds b = new Bounds(chunkGlobalPos + vGenConfig.ChunkSize / 2, vGenConfig.ChunkSize);
            if(GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(Camera.main), b))
            {
                Call();
            }
        }

        void Call()
        {
            shader.SetFloat("nearClipDistance", Camera.main.nearClipPlane);

            var llnearClip = CamGeometry.NearClipLowerLeftCorner(Camera.main);
            var urnearClip = CamGeometry.NearClipUpperRightCorner(Camera.main);
            shader.SetVector("_nearClipMinCorner", llnearClip);
            shader.SetVector("_nearClipMaxCorner", urnearClip);
            shader.SetVector("cam", Camera.main.transform.position);
            shader.Dispatch(reverseCastKernel, Mathf.CeilToInt(_display.count / (float)vGenConfig.ReverseCastThreadCount), 1, 1);
        }


    }


}
