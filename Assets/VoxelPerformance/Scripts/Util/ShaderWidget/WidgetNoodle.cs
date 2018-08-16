using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mel.VoxelGen;
using Mel.Math;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;

namespace Mel.Util
{

    public class WidgetNoodle : MonoBehaviour
    {
        [SerializeField] ComputeShader compute;
        [SerializeField] Shader shader;
        RenderTexture rentex;
        [SerializeField] Renderer display;
        [SerializeField] WidgetDisplay widgetDisplay;
        [SerializeField] InputModifier inputModifier;

        ComputeBuffer inputModifBuffer;

        [Serializable]
        public struct InputModifier
        {
            public Vector3 scale;
            public Vector3 offset;

            public static InputModifier DefaultIM()
            {
                return new InputModifier()
                {
                    scale = Vector3.one,
                    offset = Vector3.zero
                };
            }

            public InputModifier[] ToArrayOne() { return new InputModifier[] { this }; }
        }

        int mainKernel;
        int texPropertyID;
        ComputeBuffer buff;

        Material _material;
        Material material {
            get {
                if(!_material) { _material = new Material(shader); }
                return _material;
            }
        }

        [MenuItem("MEL/Update Cross-section %&X")]
        static void DoWhatWidgetWants()
        {
            var widget = FindObjectOfType<WidgetNoodle>();
            widget.DoMyThing();



        }

        private void Start()
        {
            DoMyThing();
        }

        private void DoMyThing()
        {
            if(inputModifier.scale.sqrMagnitude < Mathf.Epsilon) { inputModifier = InputModifier.DefaultIM(); }
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(shader));
            InitComputeShader();
            RunKernel();


            
        }

        private void InitComputeShader()
        {
            mainKernel = compute.FindKernel("SNoiseModule");

            rentex = new RenderTexture(256 * 2, 256, 24);
            rentex.enableRandomWrite = true;
            rentex.Create();

            texPropertyID = Shader.PropertyToID("Result");
            compute.SetTexture(mainKernel, texPropertyID, rentex);
        }

        private void RunKernel()
        {
            var groupSize = BufferUtil.GetThreadGroupSizes(compute, mainKernel);
            var groups = new IntVector3(rentex.width, rentex.height, 1) / groupSize;
            compute.Dispatch(mainKernel, groups.x, groups.y, groups.z);
            material.SetTexture("_MainTex", rentex);
            display.material = material;

            widgetDisplay.mat = material;
            widgetDisplay.tex = rentex;
        }

    }
}

#endif