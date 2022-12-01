using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace MarblingEffectSys {

    public class MarblingShader : System.IDisposable {

        public Material Mat { get; protected set; }

        public MarblingShader() {
            Mat = new Material(Resources.Load<Shader>(PATH));
        }

        public void Dispose() {
            if (Mat != null) {
                (Application.isPlaying ? (System.Action<Material>)Object.Destroy : Object.DestroyImmediate)(Mat);
                Mat = null;
            }
        }

        public void Render(RenderTexture dst, Texture src, Texture offset) {
            Mat.SetTexture(P_OffsetTex, offset);
            Graphics.Blit(src, dst, Mat, (int)PASS.RENDER);
        }
        public void Add(RenderTexture dst, Texture src, Texture brush, float4 rect_uv, float2 offset_uv) {
            var prev = RenderTexture.active;
            RenderTexture.active = dst;
            Mat.SetVector(P_Param0, new float4(offset_uv, 0f, 0f));
            Mat.SetVector(P_Param1, rect_uv);
            Graphics.Blit(src, dst, Mat, (int)PASS.ADD);
            RenderTexture.active = prev;
        }

        #region declarations
        public enum PASS { RENDER = 0, ADD }
        public const string PATH = "Marbling";
        public static readonly int P_OffsetTex = Shader.PropertyToID("_OffsetTex");
        public static readonly int P_BrushTex = Shader.PropertyToID("_BrushTex");
        public static readonly int P_Param0 = Shader.PropertyToID("_Param0");
        public static readonly int P_Param1 = Shader.PropertyToID("_Param1");
        public static readonly Bounds BOUNDS = new Bounds(Vector3.zero, 1000f * Vector3.one);
        #endregion
    }
}