using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mel.Util
{
    public class CubesRenderer : MonoBehaviour
    {
        ComputeBuffer buffer;
        [SerializeField]
        Shader geomShader;
        Material material;
        [SerializeField]
        Texture2D sprite;


        public void Apply(List<Vertex> points)
        {
            if(buffer != null) { buffer.Release(); buffer = null; }

            if(!material)
            {
                material = new Material(geomShader);
            }

            buffer = new ComputeBuffer(points.Count, System.Runtime.InteropServices.Marshal.SizeOf(new Vertex()));
            buffer.SetData(points.ToArray());
        }

        private void OnRenderObject()
        {
            if(buffer == null) { return; }

            material.SetVector("_chunkPosition", transform.position);

            material.SetTexture("_Sprite", sprite);
            material.SetFloat("_Size", 1);
            material.SetMatrix("_worldMatrixTransform", transform.localToWorldMatrix);
            material.SetBuffer("_displayPoints", buffer);

            material.SetVector("_cameraPosition", Camera.main.transform.position);

            Graphics.DrawProcedural(MeshTopology.Points, buffer.count);

        }

        private void OnDestroy()
        {
            if(buffer != null) { buffer.Release(); }
            buffer = null;
        }

    }
}
