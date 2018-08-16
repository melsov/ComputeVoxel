using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mel.VoxelGen
{
    public class RaycastDisplay : MonoBehaviour
    {
        [SerializeField]
        Texture2D sprite;

        [SerializeField]
        Shader geometryShader;

        ComputeBuffer _buffer;
        public ComputeBuffer buffer {
            get {
                return _buffer;
            }
            set {
                if(value != null && material != null && _buffer != value)
                {
                    material.SetBuffer("_displayPoints", value);
                }
                _buffer = value;
            }
        }
                

        private VGenConfig vGenConfig;
        Material material;

        Transform globalLight;

        public float size { get { return 1f; } }
        public Bounds bounds { get; private set; }

        public void initialize(
            ComputeBuffer buffer,
            VGenConfig vGenConfig = null
            )
        {
            this.buffer = buffer;
            globalLight = GameObject.Find("GlobalLight").transform;

            this.vGenConfig = vGenConfig;
            if (!this.vGenConfig) { this.vGenConfig = FindObjectOfType<VGenConfig>(); }

            bounds = new Bounds(
                transform.position + this.vGenConfig.ChunkSize / 2f * size, 
                this.vGenConfig.ChunkSize * size);

            material = new Material(geometryShader);
            setMaterialConstants();

            enabled = true;
        }

        void setMaterialConstants()
        {
            material.SetVector("_chunkPosition", transform.position);
            material.SetTexture("_Sprite", sprite);
            material.SetFloat("_Size", size);
            material.SetMatrix("_worldMatrixTransform", transform.localToWorldMatrix);

            material.SetBuffer("_displayPoints", buffer);
        }


        bool isVisible(float distFromCamSquared)
        {
            if (buffer == null) { return false; }
            return enabled
              && distFromCamSquared < 1.2f * Camera.main.farClipPlane * Camera.main.farClipPlane
              && GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(Camera.main), bounds);
        }

        void OnRenderObject()
        {

            if (Input.GetKey(KeyCode.R)) { return; }

            float distFromCamSquared = bounds.SqrDistance(Camera.main.transform.position);
            if (isVisible(distFromCamSquared))
            {
                ////        Shader.globalMaximumLOD = 100;
                ////        material.shader.maximumLOD = 100;
                material.SetPass(0);
                material.SetVector("_cameraPosition", Camera.main.transform.position);

                //int lodIndex = vGenConfig.LODIndexForCamDistance(distFromCamSquared);
                //lodIndex = Input.GetKey(KeyCode.C) ? 0 : lodIndex;
                //lodIndex = Input.GetKey(KeyCode.F) ? 1 : lodIndex;
                //lodIndex = Input.GetKey(KeyCode.G) ? 2 : lodIndex;
                material.SetFloat("_mipStretch", 1f); // Mathf.Pow(2f, lodIndex));
                material.SetVector("_globalLight", globalLight.position);

                Graphics.DrawProcedural(MeshTopology.Points, buffer.count); //maybe want to use callArgs instead
            }
        }

        private void OnDestroy()
        {
            BufferUtil.ReleaseBuffers(buffer);
        }

    }
}
