using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Mel.Util
{
    public class WidgetDisplay : MonoBehaviour
    {
        Texture _tex;
        public Texture tex {
            get { return _tex; }
            set {
                _tex = value;
                if(GetComponent<Image>())
                {
                    Image im = GetComponent<Image>();
                    im.material.SetTexture("_MainTex", _tex);
                }
            }
        }

        public Material mat {
            set {
                if (GetComponent<Image>())
                {
                    GetComponent<Image>().material = value;
                }
            }
        }

        [SerializeField] bool DrawOnRenderObject;

        private void OnRenderObject()
        {
            if(!DrawOnRenderObject) { return; }

            Graphics.DrawTexture(new Rect(Vector2.zero, new Vector2(Screen.width, Screen.height) / 2f), tex);
        }

        private void Start()
        {
            hide();
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.H))
            {
                var im = GetComponent<Image>();
                if(im)
                {
                    im.enabled = !im.enabled;
                }
            }
        }

        void hide()
        {
            var im = GetComponent<Image>();
            if (im) { im.enabled = false; }
        }

    }
}
