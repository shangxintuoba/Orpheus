using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CheddarSparks.CustomDitheringPostProcessing.Editor
{
    public class CustomDitheringPackageImportWatcher : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(
            string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (var asset in importedAssets)
            {
                if (asset.Contains("DitheringPostProcessing") && !EditorPrefs.HasKey("CustomDithering_Initialized"))
                {
                    EditorPrefs.SetBool("CustomDithering_Initialized", true);
                    EditorApplication.delayCall += () =>
                    {
                        bool userAccepted = EditorUtility.DisplayDialog(
                            "Setup Custom Dithering Render Pipeline",
                            "Would you like to import the demo render pipeline required for the demo scene to work?\n\n" +
                            "Click 'OK' to import it automatically. This will add the demo renderer to your Render Pipeline Asset\n" +
                            "Click 'Cancel' if you prefer to configure it manually. This window will not show again. You can refer to the documentation for setup instructions.",
                            "OK", "Cancel");

                        if (userAccepted)
                        {
                            AddRendererToPipeline();
                        }
                    };
                    break;
                }
            }
        }

        private static void AddRendererToPipeline()
        {
            Debug.Log("Adding Custom Dithering Renderer to Render Pipeline Asset...");

            var urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urpAsset == null)
            {
                Debug.LogWarning("Current Render Pipeline is not URP.");
                return;
            }

            var customRendererData = AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(
                "Assets/CheddarSparksURP/DitheringPostProcessing/RenderPipeline/Demo Dithering Universal Renderer Data.asset");

            if (customRendererData == null)
            {
                Debug.LogWarning("Custom Renderer asset not found.");
                return;
            }

            var serializedURP = new SerializedObject(urpAsset);
            var rendererList = serializedURP.FindProperty("m_RendererDataList");

            int index = rendererList.arraySize;
            rendererList.InsertArrayElementAtIndex(index);
            rendererList.GetArrayElementAtIndex(index).objectReferenceValue = customRendererData;

            
            serializedURP.ApplyModifiedProperties();
            Debug.Log("Custom renderer added to URP asset.");
        }
    }
}
