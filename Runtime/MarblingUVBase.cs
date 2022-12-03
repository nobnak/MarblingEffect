using Gist2.Wrappers;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

namespace MarblingEffectSys {

    public class MarblingUVBase : MonoBehaviour {

        public Events events = new Events();

        protected MarblingShader shader;
        protected Queue<Stroke> strokes;
        protected RenderTextureWrapper offset0;
        protected RenderTextureWrapper offset1;

        #region properties
        public Tuner CurrTuner { get; set; } = new Tuner();
        #endregion

        #region unity
        protected virtual void OnEnable() {
            shader = new MarblingShader();
            strokes = new Queue<Stroke>();

            offset0 = new RenderTextureWrapper(GenerateOffsetTexture);
            offset1 = new RenderTextureWrapper(GenerateOffsetTexture);
        }
        protected virtual void OnDisable() {
            if (shader != null) {
                shader.Dispose();
                shader = null;
            }

            events.OnUpdate?.Invoke(null);
            if (offset0 != null) {
                offset0.Dispose();
                offset0 = null;
            }
            if (offset1 != null) {
                offset1.Dispose();
                offset1 = null;
            }
        }
        protected virtual void Update() {
            strokes.Clear();
        }
        #endregion

        #region methods
        public static void Swap<T>(ref T offset0, ref T offset1) {
            var tmp = offset1;
            offset1 = offset0;
            offset0 = tmp;
        }

        protected void Add(Stroke stroke) {
            strokes.Enqueue(stroke);
        }
        protected RenderTexture GenerateOffsetTexture(int2 s) {
            var tex = new RenderTexture(s.x, s.y, 0, RenderTextureFormat.ARGBFloat);
            tex.hideFlags = HideFlags.DontSave;
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.useMipMap = false;
            tex.Create();
            shader.Reset(tex);
            return tex;
        }
        protected void Render(int2 size) {
            offset0.Size = offset1.Size = size;
            if (offset0.Value != null) {
                var reg = CurrTuner.regression * Time.deltaTime;
                if (reg > 0) {
                    shader.Regression(offset1, offset0, reg);
                    Swap(ref offset0, ref offset1);
                }
                if (strokes.Count > 0) {
                    Graphics.Blit(offset0, offset1);
                    foreach (var stroke in strokes)
                        shader.Add(offset1, offset0, stroke.brush, stroke.rect, stroke.offset);
                    Swap(ref offset0, ref offset1);
                }
            }
            events.OnUpdate?.Invoke(offset0);
        }
        #endregion

        #region declarations
        [System.Serializable]
        public class Events {
            public TextureEvent OnUpdate = new TextureEvent();

            [System.Serializable]
            public class TextureEvent : UnityEvent<Texture> { }
        }
        [System.Serializable]
        public struct Stroke {
            public bool enabled;
            public Texture brush;
            public float4 rect;
            public float2 offset;

            public Stroke(Texture brush, float4 rect, float2 offset) {
                this.brush = brush;
                this.enabled = true;
                this.rect = rect;
                this.offset = offset;
            }

            public override string ToString() {
                return $"<{GetType().Name}: v=({offset.x:e2},{offset.y:e2}) rect={rect}>";
            }
        }
        [System.Serializable]
        public class Tuner {
            public float size = 0.2f;
            public float strength = 1f;
            public float regression = 0f;
        }
        #endregion
    }
}