using System.IO;
using UnityEngine;
using UnityEditor;

namespace CrowRx.Utility.Editor
{
    public class AssetLinkerEditor : UnityEditor.Editor
    {
        private SerializedProperty _bundleName;
        private SerializedProperty _assetName;

        protected Object _targetAsset;

        protected virtual void OnEnable()
        {
            _bundleName = serializedObject.FindProperty("BundleName");
            _assetName = serializedObject.FindProperty("AssetName");

            var paths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(_bundleName.stringValue, _assetName.stringValue);

            if (paths != null && paths.Length > 0)
                _targetAsset = AssetDatabase.LoadAssetAtPath<GameObject>(paths[0]);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var bundleName = _bundleName.stringValue;
            var assetName = _assetName.stringValue;

            _targetAsset = EditorGUILayout.ObjectField("Asset", _targetAsset, typeof(GameObject), false);
            if (_targetAsset != null)
            {
                var assetPath = AssetDatabase.GetAssetPath(_targetAsset);
                bundleName = AssetDatabase.GetImplicitAssetBundleName(assetPath);

                if (string.IsNullOrEmpty(bundleName))
                {
                    EditorGUILayout.LabelField("It's not belong to any asset bundeles.");

                    bundleName = string.Empty;
                    assetName = string.Empty;
                }
                else
                {
                    assetName = Path.GetFileNameWithoutExtension(assetPath);
                }
            }
            else
            {
                EditorGUILayout.LabelField("No Linked Asset.");

                bundleName = string.Empty;
                assetName = string.Empty;
            }

            if (_bundleName.stringValue != bundleName || _assetName.stringValue != assetName)
            {
                _bundleName.stringValue = bundleName;
                _assetName.stringValue = assetName;
            }

            EditorGUILayout.LabelField("Assetbundle : ", bundleName);
            EditorGUILayout.LabelField("Asset : ", assetName);

            serializedObject.ApplyModifiedProperties();
        }
    }
}