using UnityEngine;

namespace CheddarSparks.CustomDitheringPostProcessing
{
    public static class ShaderPropertyIDs
    {
        public static readonly int DitherTexture = Shader.PropertyToID("_DitherTex");
        public static readonly int DitherAmount = Shader.PropertyToID("_DitherAmount");
        public static readonly int DitherSize = Shader.PropertyToID("_DitherSize");
        public static readonly int DitherColorA = Shader.PropertyToID("_DitherColorA");
        public static readonly int DitherColorB = Shader.PropertyToID("_DitherColorB");

        public static readonly int SceneColorAmount = Shader.PropertyToID("_SceneColorAmount");
        public static readonly int ColorBandsAmount = Shader.PropertyToID("_ColorBandsAmount");
        public static readonly int ColorBandsCountR = Shader.PropertyToID("_ColorBandsCountR");
        public static readonly int ColorBandsCountG = Shader.PropertyToID("_ColorBandsCountG");
        public static readonly int ColorBandsCountB = Shader.PropertyToID("_ColorBandsCountB");

        public static readonly int OutlineWidth = Shader.PropertyToID("_OutlineWidth");
        public static readonly int OutlineThreshold = Shader.PropertyToID("_OutlineThreshold");
        public static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");

        public static readonly int ReferenceHeight = Shader.PropertyToID("_ReferenceHeight");

        public static class Keywords
        {
            public static readonly string OutlineModelSobel = "_OUTLINE_SOBEL";
            public static readonly string OutlineModelDepth = "_OUTLINE_DEPTH";
            public static readonly string OutlineModelNone = "_OUTLINE_OFF";
        }
    }
}