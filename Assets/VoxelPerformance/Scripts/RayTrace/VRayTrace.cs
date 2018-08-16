using UnityEngine;
using Mel.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mel.VoxelGen
{
    public class VRayTrace : MonoBehaviour
    {
        struct NearClipData
        {
            public float nearClipDistance;
            public Vector2 nearClipWidthHeight; 
        }

        private void Start()
        {
            //var distance = Camera.main.nearClipPlane;
            //var frustumHeight = 2.0f * distance * Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
            //var frustumWidth = frustumHeight * Camera.main.aspect;

            //NearClipData ncd = new NearClipData()
            //{
            //    nearClipDistance = distance,
            //    nearClipWidthHeight = new Vector2(frustumWidth, frustumHeight)
            //};

            //var roM = Matrix4x4.Rotate(Camera.main.transform.rotation);

        }
    }
}
