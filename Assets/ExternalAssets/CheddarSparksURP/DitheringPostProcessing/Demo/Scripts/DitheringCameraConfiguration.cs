#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CheddarSparks.CustomDitheringPostProcessing.Demo
{
    [ExecuteInEditMode]
    public class DitheringCameraConfiguration : MonoBehaviour
    {
        private void OnEnable()
        {
            var urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urpAsset == null)
            {
                Debug.LogWarning("Current pipeline is not URP.");
                return;
            }

            var so = new SerializedObject(urpAsset);
            var list = so.FindProperty("m_RendererDataList");
            bool rendererFound = false;
            int rendererIndex = -1;
            for (int i = 0; i < list.arraySize; i++)
            {
                var renderer = list.GetArrayElementAtIndex(i).objectReferenceValue;
                if (renderer != null && renderer.name.Equals("Demo Dithering Universal Renderer Data"))
                {
                    rendererFound = true;
                    rendererIndex = i;
                    break;
                }
            }

            if (rendererFound)
            {
                var cameraData = GetComponent<UniversalAdditionalCameraData>();
                if (cameraData != null)
                {
                    cameraData.SetRenderer(rendererIndex);
                    Debug.Log($"Set renderer index {list.GetArrayElementAtIndex(rendererIndex).objectReferenceValue.name} on camera '{cameraData.name}'.");
                }
                else
                {
                    Debug.LogWarning("UniversalAdditionalCameraData not found on camera.");
                }
            }
            else
            {
                Debug.LogWarning("Demo Dithering Universal Renderer Data not found in the current URP asset. " +
                                 "Please ensure it is added to the URP asset's renderer list.");
            }
        }
    }
}
#endif