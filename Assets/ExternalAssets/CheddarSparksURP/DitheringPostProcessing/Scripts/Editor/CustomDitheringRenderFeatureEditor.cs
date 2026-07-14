
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CheddarSparks.CustomDitheringPostProcessing.Editor
{
    [CustomEditor(typeof(CustomDitheringRenderFeature))]
    public class CustomDitheringRenderFeatureEditor : UnityEditor.Editor
    {
        #region Strings
        private static string postProMaterialLabel = "Post-Processing Material";
        private static string postProMaterialTooltip = "Material that applies dithering post-processing using the CustomDitheringPostProcessing shader.";
        private static string usePostProcessingVolumesLabel = "Use Post-Processing Volumes";
        private static string usePostProcessingVolumesTooltip =
        "If enabled, uses CustomDitheringVolumeComponents in the scene to control dithering settings.\n\n" +
        "If disabled, settings from the Post-Processing Material will be used instead.\n\n" +
        "[Optional] You may also add a Dithering Manager to the scene to configure these properties via script. " +
        "Note: Dithering Manager is ignored when this option is enabled.";
        private static string createDitheringManagerButtonLabel = "Create Dithering Manager";
        #endregion

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty postProMaterialProperty = serializedObject.FindProperty("_postProMaterial");

            if (postProMaterialProperty != null)
            {
                EditorGUILayout.PropertyField(
                    postProMaterialProperty,
                    new GUIContent(postProMaterialLabel, postProMaterialTooltip));
            }

            SerializedProperty usePostProcessingVolumesProperty = serializedObject.FindProperty("_usePostProcessingVolumes");

            if (usePostProcessingVolumesProperty != null)
            {
                EditorGUILayout.PropertyField(
                    usePostProcessingVolumesProperty,
                    new GUIContent(usePostProcessingVolumesLabel, usePostProcessingVolumesTooltip));
            }

            if (!usePostProcessingVolumesProperty.boolValue && GUILayout.Button(createDitheringManagerButtonLabel))
            {
                DitheringManager.CreateDitheringManager((Material)postProMaterialProperty.objectReferenceValue);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif