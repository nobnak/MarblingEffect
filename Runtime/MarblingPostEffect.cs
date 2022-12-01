using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MarblingEffectSys {

    [ExecuteAlways]
    public class MarblingPostEffect : MonoBehaviour {

        protected MarblingShader shader;

        #region properties
        public Texture OffsetTex { get; set; }
        #endregion

        #region unity
        private void OnEnable() {
            shader = new MarblingShader();
        }
        private void OnDisable() {
            if (shader != null) {
                shader.Dispose();
                shader = null;
            }
        }
        private void OnRenderImage(RenderTexture source, RenderTexture destination) {
            if (shader != null) shader.Render(destination, source, OffsetTex);
        }
        #endregion
    }
}