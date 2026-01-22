using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using ZLinq;


namespace CrowRx.Utility.Editor
{
    public static class TypeUtility
    {
        public static void GatherTypes(string scriptBaseNamespace, Type baseType, string replace, List<Tuple<string, Type>> gatherTypes)
        {
            if (string.IsNullOrEmpty(scriptBaseNamespace))
            {
                return;
            }

            List<Tuple<string, Type>> buffer = new();

            foreach (Type typeInAssembly in RuntimeTypeCache.Types)
            {
                if (!baseType.IsAssignableFrom(typeInAssembly))
                {
                    continue;
                }

                if (typeInAssembly.IsNotPublic ||
                    typeInAssembly.IsInterface ||
                    typeInAssembly.IsAbstract)
                {
                    continue;
                }

                string controllerName = typeInAssembly.FullName;
                if (string.IsNullOrEmpty(controllerName))
                {
                    controllerName = typeInAssembly.Name;
                }

                if (!string.IsNullOrEmpty(scriptBaseNamespace))
                {
                    controllerName = controllerName.Replace($"{scriptBaseNamespace}.", "");
                }

                if (!string.IsNullOrEmpty(replace))
                {
                    controllerName = controllerName.Replace(replace, "");
                }

                controllerName = controllerName.Replace('.', '/');

                buffer.Add(Tuple.Create(controllerName, typeInAssembly));
            }

            gatherTypes.Clear();
            gatherTypes.AddRange(buffer.OrderBy(tuple => tuple.Item1));
        }

        public static string[] GetPrefabPaths<T>() where T : MonoBehaviour
        {
            MonoScript script = MonoScript.FromMonoBehaviour(new GameObject().AddComponent<T>());
            string scriptPath = AssetDatabase.GetAssetPath(script);
            string scriptGuid = AssetDatabase.AssetPathToGUID(scriptPath);
            UnityEngine.Object.DestroyImmediate(script, true);

            return
                AssetDatabase.FindAssets("t:Prefab")
                    .AsValueEnumerable()
                    .Select(guid =>
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(guid);

                        return
                            File.ReadAllText(
                                    Path.Combine(
                                        Path.GetFullPath(Path.Combine(Application.dataPath, "../")),
                                        assetPath))
                                .Contains(scriptGuid) // GUID가 포함되어 있지 않으면 바로 스킵 (빠른 필터링)
                                ? assetPath
                                : null;
                    })
                    .Where(path => path is not null)
                    .ToArray();
        }

        public static string[] GetScriptableObjectPaths<T>() where T : ScriptableObject
        {
            ScriptableObject so = ScriptableObject.CreateInstance(typeof(T));
            string scriptGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(so)));
            UnityEngine.Object.DestroyImmediate(so, true);

            return
                AssetDatabase.FindAssets("t:ScriptableObject")
                    .AsValueEnumerable()
                    .Select(guid =>
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(guid);

                        return
                            File.ReadAllText(
                                    Path.Combine(
                                        Path.GetFullPath(Path.Combine(Application.dataPath, "../")),
                                        assetPath))
                                .Contains(scriptGuid) // GUID가 포함되어 있지 않으면 바로 스킵 (빠른 필터링)
                                ? assetPath
                                : null;
                    })
                    .Where(path => path is not null)
                    .ToArray();
        }
    }
}