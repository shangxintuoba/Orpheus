#if UNITY_2023_3_OR_NEWER
#define USE_RENDER_GRAPH
#endif

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static CheddarSparks.CustomDitheringPostProcessing.CustomDitheringVolumeComponent;

#if USE_RENDER_GRAPH
using UnityEngine.Rendering.RenderGraphModule;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CheddarSparks.CustomDitheringPostProcessing
{
    public class CustomDitheringRenderFeature : ScriptableRendererFeature
    {
        [SerializeField]
        private Material _postProMaterial;
        [SerializeField]
        private bool _usePostProcessingVolumes = true;

        private CustomDitheringRenderPass _ditheringRenderPass;

        public Material PostproMaterial => _postProMaterial;
        public bool UsePostProcessingVolumes => _usePostProcessingVolumes;

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(_ditheringRenderPass);
        }

#if !USE_RENDER_GRAPH
        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType == CameraType.Game)
            {
                _ditheringRenderPass.ConfigureInput(ScriptableRenderPassInput.Color);
                _ditheringRenderPass.Setup(renderer.cameraColorTargetHandle);
            }
        }
#endif

        public override void Create()
        {
#if UNITY_EDITOR
            if (_postProMaterial == null)
            {
                var guids = AssetDatabase.FindAssets("CustomDitheringPostProcessing t:Material");
                if (guids.Length > 0)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    _postProMaterial = AssetDatabase.LoadAssetAtPath<Material>(path);
                }
                else
                {
                    Debug.LogWarning("CustomDitheringRenderFeature: Could not find 'CustomDitheringPostProcessing' material in project.");
                }
            }
#endif
            _ditheringRenderPass = new CustomDitheringRenderPass(this)
            {
                renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing
            };
        }
    }

    public class CustomDitheringRenderPass : ScriptableRenderPass
    {
        private Material _postproMaterial;
        private CustomDitheringRenderFeature _customDitheringRenderFeature;
        private CustomDitheringVolumeComponent _volumeComponent;
        private OutlineMode _lastOutlineMode;
        private static readonly ProfilingSampler k_ProfilingDither = new("Apply Dithering");
        private static readonly ProfilingSampler k_ProfilingBlitBack = new("Blit Back Dithering");
        private float _referenceHeight = 1080f;

        public CustomDitheringRenderPass(CustomDitheringRenderFeature customDitheringRenderFeature)
        {
            _customDitheringRenderFeature = customDitheringRenderFeature;
            _postproMaterial = customDitheringRenderFeature.PostproMaterial;
        }

#if USE_RENDER_GRAPH
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var stack = VolumeManager.instance.stack;
            _volumeComponent = stack.GetComponent<CustomDitheringVolumeComponent>();

            if (_postproMaterial == null ||
                (_customDitheringRenderFeature.UsePostProcessingVolumes && (_volumeComponent == null || !_volumeComponent.IsActive())))
                return;

            var resourceData = frameData.Get<UniversalResourceData>();
            var cameraData = frameData.Get<UniversalCameraData>();

            var desc = resourceData.activeColorTexture.GetDescriptor(renderGraph);
            desc.name = "_CustomDitherTempColor";
            var tempColor = renderGraph.CreateTexture(desc);

            _referenceHeight = GetReferenceHeight(desc);

            ApplyDithering(renderGraph, resourceData.activeColorTexture, tempColor);
            BlitBackToActive(renderGraph, tempColor, resourceData.activeColorTexture);
        }

        private void ApplyDithering(RenderGraph graph, in TextureHandle source, in TextureHandle destination)
        {
            using var builder = graph.AddUnsafePass<DitheringPassData>("Apply Dithering", out var data, k_ProfilingDither);

            data.sourceTexture = source;
            data.destinationTexture = destination;
            data.material = _postproMaterial;

            SetMaterialConf(data.material);
            if (_customDitheringRenderFeature.UsePostProcessingVolumes)
            {
                SetMaterialProperties(data.material);
            }

            builder.AllowPassCulling(false);
            builder.UseTexture(source, AccessFlags.Read);
            builder.UseTexture(destination, AccessFlags.Write);

            builder.SetRenderFunc(static (DitheringPassData d, UnsafeGraphContext ctx) =>
            {
                var cmd = CommandBufferHelpers.GetNativeCommandBuffer(ctx.cmd);
                using var scope = new ProfilingScope(cmd, new ProfilingSampler("Blit Dithering"));
                Blitter.BlitCameraTexture(cmd, d.sourceTexture, d.destinationTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, d.material, 0);
            });
        }

        private void BlitBackToActive(RenderGraph graph, in TextureHandle source, in TextureHandle destination)
        {
            using var builder = graph.AddUnsafePass<DitheringPassData>("Blit Back Dithering", out var data, k_ProfilingBlitBack);

            data.sourceTexture = source;
            data.destinationTexture = destination;

            builder.AllowPassCulling(false);
            builder.UseTexture(source, AccessFlags.Read);
            builder.UseTexture(destination, AccessFlags.Write);

            builder.SetRenderFunc(static (DitheringPassData d, UnsafeGraphContext ctx) =>
            {
                var cmd = CommandBufferHelpers.GetNativeCommandBuffer(ctx.cmd);
                using var scope = new ProfilingScope(cmd, new ProfilingSampler("Blit Back Dithering"));
                Blitter.BlitCameraTexture(cmd, d.sourceTexture, d.destinationTexture);
            });
        }
        private float GetReferenceHeight(TextureDesc textureDesc)
        {
            return textureDesc.height;
        }


        private sealed class DitheringPassData
        {
            internal TextureHandle sourceTexture;
            internal TextureHandle destinationTexture;
            internal Material material;
        }
#else

        private RTHandle _temporatyColorTexture;
        private RTHandle _source;
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (_postproMaterial == null)
                return;

            var stack = VolumeManager.instance.stack;
            _volumeComponent = stack.GetComponent<CustomDitheringVolumeComponent>();

            if (_customDitheringRenderFeature.UsePostProcessingVolumes && (_volumeComponent == null || !_volumeComponent.IsActive()))
                return;

            var cmd = CommandBufferPool.Get("CustomDitheringPass");
            _referenceHeight = GetReferenceHeight(renderingData);

            using (new ProfilingScope(cmd, k_ProfilingDither))
            {
                SetMaterialConf(_postproMaterial);
                if (_customDitheringRenderFeature.UsePostProcessingVolumes)
                {
                    SetMaterialProperties(_postproMaterial);
                }

                Blitter.BlitCameraTexture(cmd, _source, _source, _postproMaterial, 0);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public void Setup(RTHandle source)
        {
            _source = source;
        }

        
        private float GetReferenceHeight(RenderingData renderingData)
        {
            return renderingData.cameraData.cameraTargetDescriptor.height;
        }
#endif
        private void SetMaterialConf(Material mat)
        {
            mat.SetFloat(ShaderPropertyIDs.ReferenceHeight, _referenceHeight);
        }

        private void SetMaterialProperties(Material mat)
        {
            mat.SetTexture(ShaderPropertyIDs.DitherTexture, _volumeComponent.ditherTexture.value);
            mat.SetFloat(ShaderPropertyIDs.DitherAmount, _volumeComponent.ditherAmount.value);
            mat.SetFloat(ShaderPropertyIDs.DitherSize, _volumeComponent.ditherSize.value);
            mat.SetColor(ShaderPropertyIDs.DitherColorA, _volumeComponent.ditherColorA.value);
            mat.SetColor(ShaderPropertyIDs.DitherColorB, _volumeComponent.ditherColorB.value);
            mat.SetFloat(ShaderPropertyIDs.SceneColorAmount, _volumeComponent.sceneColorAmount.value);
            mat.SetFloat(ShaderPropertyIDs.ColorBandsAmount, _volumeComponent.colorBandsAmount.value);

            if (_volumeComponent.colorBandsCountGeneral.value > 0f && _volumeComponent.colorBandsCountGeneral.overrideState)
            {
                mat.SetFloat(ShaderPropertyIDs.ColorBandsCountR, _volumeComponent.colorBandsCountGeneral.value);
                mat.SetFloat(ShaderPropertyIDs.ColorBandsCountG, _volumeComponent.colorBandsCountGeneral.value);
                mat.SetFloat(ShaderPropertyIDs.ColorBandsCountB, _volumeComponent.colorBandsCountGeneral.value);
            }
            else
            {
                mat.SetFloat(ShaderPropertyIDs.ColorBandsCountR, _volumeComponent.colorBandsCountR.value);
                mat.SetFloat(ShaderPropertyIDs.ColorBandsCountG, _volumeComponent.colorBandsCountG.value);
                mat.SetFloat(ShaderPropertyIDs.ColorBandsCountB, _volumeComponent.colorBandsCountB.value);
            }

            if (_volumeComponent.outlineMode.value != _lastOutlineMode)
            {
                _lastOutlineMode = _volumeComponent.outlineMode.value;

                mat.DisableKeyword(ShaderPropertyIDs.Keywords.OutlineModelSobel);
                mat.DisableKeyword(ShaderPropertyIDs.Keywords.OutlineModelDepth);
                mat.DisableKeyword(ShaderPropertyIDs.Keywords.OutlineModelNone);

                switch (_volumeComponent.outlineMode.value)
                {
                    case OutlineMode.Sobel:
                        mat.EnableKeyword(ShaderPropertyIDs.Keywords.OutlineModelSobel);

                        break;
                    case OutlineMode.Depth:
                        mat.EnableKeyword(ShaderPropertyIDs.Keywords.OutlineModelDepth);
                        break;
                    case OutlineMode.None:
                        mat.EnableKeyword(ShaderPropertyIDs.Keywords.OutlineModelNone);
                        break;
                }
            }

            mat.SetFloat(ShaderPropertyIDs.OutlineWidth, _volumeComponent.outlineWidth.value);
            mat.SetFloat(ShaderPropertyIDs.OutlineThreshold, _volumeComponent.outlineThreshold.value);
            mat.SetColor(ShaderPropertyIDs.OutlineColor, _volumeComponent.outlineColor.value);
        }
    }
}
