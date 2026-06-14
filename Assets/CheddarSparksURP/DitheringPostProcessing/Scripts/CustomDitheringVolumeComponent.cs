using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace CheddarSparks.CustomDitheringPostProcessing
{
    public class CustomDitheringVolumeComponent : VolumeComponent
    {
        public BoolParameter enabled = new BoolParameter(true, true);

        [Header("Dithering")]
        public Texture2DParameter ditherTexture = new Texture2DParameter(null, true);
        public ClampedFloatParameter ditherAmount = new ClampedFloatParameter(1f, 0f, 1f, true);
        public ClampedIntParameter ditherSize = new ClampedIntParameter(3, 1, 100, true);
        [Tooltip("The color only affects when the property SceneColorAmount is less than 1")]
        public ColorParameter ditherColorA = new ColorParameter(Color.white, true);
        [Tooltip("The color only affects when the property SceneColorAmount is less than 1")]
        public ColorParameter ditherColorB = new ColorParameter(Color.black, true);

        [Header("Color Banding")]
        public ClampedFloatParameter sceneColorAmount = new ClampedFloatParameter(0f, 0f, 1f, false);
        public ClampedFloatParameter colorBandsAmount = new ClampedFloatParameter(0f, 0f, 1f, false);
        [Tooltip("If enabled, the color bands will be applied to the all color channels. If disabled, the color bands will be applied to each RGB channel individually. General and RGB configuration cannot be active at the same time")]
        public FloatParameter colorBandsCountGeneral = new FloatParameter(0f, false);
        public FloatParameter colorBandsCountR = new FloatParameter(8f, false);
        public FloatParameter colorBandsCountG = new FloatParameter(8f, false);
        public FloatParameter colorBandsCountB = new FloatParameter(8f, false);

        [Header("Outline")]
        [Tooltip("When enabled, this may affect performance. To use Depth mode, ensure that the depth texture is enabled in the render pipeline settings or camera settings.")]
        public EnumParameter<OutlineMode> outlineMode = new EnumParameter<OutlineMode>(OutlineMode.None, false);
        public FloatParameter outlineWidth = new FloatParameter(0f, false);
        public ClampedFloatParameter outlineThreshold = new ClampedFloatParameter(0.2f, 0.002f, 1f, false);
        public ColorParameter outlineColor = new ColorParameter(Color.black, false);

        public bool IsActive() => enabled.value;

        public enum OutlineMode
        {
            Sobel,
            Depth,
            None
        }

        private void OnValidate()
        {
            if (colorBandsCountR.overrideState || colorBandsCountG.overrideState || colorBandsCountB.overrideState)
            {
                colorBandsCountGeneral.overrideState = false;
            }

            if (colorBandsCountGeneral.value > 0f && colorBandsCountGeneral.overrideState)
            {
                colorBandsCountR.overrideState = false;
                colorBandsCountG.overrideState = false;
                colorBandsCountB.overrideState = false;
            }
        }
    }

    [Serializable]
    public class EnumParameter<T> : VolumeParameter<T> where T : Enum
    {
        public EnumParameter(T value, bool overrideState = false) : base(value, overrideState) { }
    }
}
