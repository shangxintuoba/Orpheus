using UnityEngine;
using static CheddarSparks.CustomDitheringPostProcessing.CustomDitheringVolumeComponent;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CheddarSparks.CustomDitheringPostProcessing
{
    public class DitheringManager : MonoBehaviour
    {
        public Material postProcessingMaterial;

        [Header("Dithering")]
        public Texture2D ditherTexture;
        [Range(0f, 1f)]
        public float ditherAmount = 1f;
        [Range(1, 100)]
        public int ditherSize = 3;
        [Tooltip("The color only affects when the property SceneColorAmount is less than 1")]
        public Color ditherColorA = Color.white;
        [Tooltip("The color only affects when the property SceneColorAmount is less than 1")]
        public Color ditherColorB = Color.black;

        [Header("Color Banding")]
        [Range(0f, 1f)]
        public float sceneColorAmount = 0f;
        [Range(0f, 1f)]
        public float colorBandsAmount = 0f;
        [Tooltip("If enabled, the color bands will be applied to the all color channels. If disabled, the color bands will be applied to each RGB channel individually. General and RGB configuration cannot be active at the same time")]
        public float colorBandsCountGeneral = 8f;
        public float colorBandsCountR = 8f;
        public float colorBandsCountG = 8f;
        public float colorBandsCountB = 8f;

        [Header("Outline")]
        [Tooltip("When enabled, this may affect performance. To use Depth mode, ensure that the depth texture is enabled in the render pipeline settings or camera settings.")]
        public OutlineMode outlineMode = OutlineMode.None;
        public float outlineWidth = 0.5f;
        [Range(0.002f, 0.2f)]
        public float outlineThreshold = 0.1f;
        public Color outlineColor = Color.black;

        public void SetPostProcessingMaterial(Material material)
        {
            postProcessingMaterial = material;
        }

        public void SetDitherTexture(Texture2D ditherTexture)
        {
            this.ditherTexture = ditherTexture;
            postProcessingMaterial.SetTexture(ShaderPropertyIDs.DitherTexture, ditherTexture);
        }

        public void SetDitherAmount(float ditherAmount)
        {
            this.ditherAmount = ditherAmount;
            postProcessingMaterial.SetFloat(ShaderPropertyIDs.DitherAmount, ditherAmount);
        }

        public void SetDitherSize(int ditherSize)
        {
            this.ditherSize = ditherSize;
            postProcessingMaterial.SetInt(ShaderPropertyIDs.DitherSize, ditherSize);
        }

        public void SetDitherColorA(Color ditherColorA)
        {
            this.ditherColorA = ditherColorA;
            postProcessingMaterial.SetColor(ShaderPropertyIDs.DitherColorA, ditherColorA);
        }

        public void SetDitherColorB(Color ditherColorB)
        {
            this.ditherColorB = ditherColorB;
            postProcessingMaterial.SetColor(ShaderPropertyIDs.DitherColorB, ditherColorB);
        }

        public void SetSceneColorAmount(float sceneColorAmount)
        {
            this.sceneColorAmount = sceneColorAmount;
            postProcessingMaterial.SetFloat(ShaderPropertyIDs.SceneColorAmount, sceneColorAmount);
        }

        public void SetColorBandsAmount(float colorBandsAmount)
        {
            this.colorBandsAmount = colorBandsAmount;
            postProcessingMaterial.SetFloat(ShaderPropertyIDs.ColorBandsAmount, colorBandsAmount);
        }

        public void SetColorBandsGeneral(float colorBandsGeneral)
        {
            SetColorBandsCountR(colorBandsGeneral);
            SetColorBandsCountG(colorBandsGeneral);
            SetColorBandsCountB(colorBandsGeneral);
        }

        public void SetColorBandsCountR(float colorBandsCountR)
        {
            this.colorBandsCountR = colorBandsCountR;
            postProcessingMaterial.SetFloat(ShaderPropertyIDs.ColorBandsCountR, colorBandsCountR);
        }

        public void SetColorBandsCountG(float colorBandsCountG)
        {
            this.colorBandsCountG = colorBandsCountG;
            postProcessingMaterial.SetFloat(ShaderPropertyIDs.ColorBandsCountG, colorBandsCountG);
        }

        public void SetColorBandsCountB(float colorBandsCountB)
        {
            this.colorBandsCountB = colorBandsCountB;
            postProcessingMaterial.SetFloat(ShaderPropertyIDs.ColorBandsCountB, colorBandsCountB);
        }

        public void SetOutlineMode(OutlineMode outlineMode)
        {
            this.outlineMode = outlineMode;
            postProcessingMaterial.DisableKeyword(ShaderPropertyIDs.Keywords.OutlineModelSobel);
            postProcessingMaterial.DisableKeyword(ShaderPropertyIDs.Keywords.OutlineModelDepth);
            postProcessingMaterial.DisableKeyword(ShaderPropertyIDs.Keywords.OutlineModelNone);
            switch (outlineMode)
            {
                case OutlineMode.Sobel:
                    postProcessingMaterial.EnableKeyword(ShaderPropertyIDs.Keywords.OutlineModelSobel);
                    break;
                case OutlineMode.Depth:
                    postProcessingMaterial.EnableKeyword(ShaderPropertyIDs.Keywords.OutlineModelDepth);
                    break;
                case OutlineMode.None:
                    postProcessingMaterial.EnableKeyword(ShaderPropertyIDs.Keywords.OutlineModelNone);
                    break;
            }
        }

        public void SetOutlineWidth(float outlineWidth)
        {
            this.outlineWidth = outlineWidth;
            postProcessingMaterial.SetFloat(ShaderPropertyIDs.OutlineWidth, outlineWidth);
        }

        public void SetOutlineThreshold(float outlineThreshold)
        {
            this.outlineThreshold = outlineThreshold;
            postProcessingMaterial.SetFloat(ShaderPropertyIDs.OutlineThreshold, outlineThreshold);
        }

        public void SetOutlineColor(Color outlineColor)
        {
            this.outlineColor = outlineColor;
            postProcessingMaterial.SetColor(ShaderPropertyIDs.OutlineColor, outlineColor);
        }

#if UNITY_EDITOR
        private float _lastColorBandsCountGeneral;

        private void OnValidate()
        {
            if (postProcessingMaterial == null) return;
            SetDitherTexture(ditherTexture);
            SetDitherAmount(ditherAmount);
            SetDitherSize(ditherSize);
            SetDitherColorA(ditherColorA);
            SetDitherColorB(ditherColorB);
            SetSceneColorAmount(sceneColorAmount);
            SetColorBandsAmount(colorBandsAmount);

            if(_lastColorBandsCountGeneral != colorBandsCountGeneral)
            {
                _lastColorBandsCountGeneral = colorBandsCountGeneral;
                SetColorBandsGeneral(colorBandsCountGeneral);
            }

            SetColorBandsCountR(colorBandsCountR);
            SetColorBandsCountG(colorBandsCountG);
            SetColorBandsCountB(colorBandsCountB);
            SetOutlineMode(outlineMode);
            SetOutlineWidth(outlineWidth);
            SetOutlineThreshold(outlineThreshold);
            SetOutlineColor(outlineColor);
        }

        public static void CreateDitheringManager(Material postProcessingMaterial)
        {
            GameObject ditheringManagerGO = new GameObject("DitheringManager");
            var ditheringManager = ditheringManagerGO.AddComponent<DitheringManager>();
            ditheringManager.SetPostProcessingMaterial(postProcessingMaterial);

            Undo.RegisterCreatedObjectUndo(ditheringManagerGO, "Create Dithering Manager");
            EditorGUIUtility.PingObject(ditheringManagerGO);
        }
#endif
    }
}
