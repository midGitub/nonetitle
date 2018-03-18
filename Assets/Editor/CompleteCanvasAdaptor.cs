using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public static class CompleteCanvasAdaptor
{
	static string[] _searchPaths = new string[] {
//		"Assets/MachineAsset/Game",
//		"Assets/PresetAssets/Assets",
//		"Resources/Common",
//		"Resources/Game",
//		"Resources/Map",
//		"Resources/Store",
//		"Resources/Tournament",
	};

	[MenuItem("Tools/Complete CanvasAdaptor")]
	static void Run()
	{
		List<string> searchAssetPaths = GetSearchAssetPaths(_searchPaths);
		PerformComplete<CanvasScaler, CanvasScalerAdaptor>(searchAssetPaths);
		Debug.Log("Search Done");
	}

	static List<string> GetSearchAssetPaths(string[] searchPaths)
	{
		List<string> result = new List<string>();
		string[] allPaths = AssetDatabase.GetAllAssetPaths();

		foreach(string p in allPaths)
		{
			bool isContain = ListUtility.IsAnyElementSatisfied(searchPaths, (string searchPath) => {
				return p.Contains(searchPath);
			});

			if(isContain)
				result.Add(p);
		}

		return result;
	}

	static void PerformComplete<T, U>(List<string> assetPaths)
		where T : Component 
		where U : Component
	{
		foreach(string p in assetPaths)
		{
			Object obj = AssetDatabase.LoadAssetAtPath<Object>(p);

			if (obj is GameObject)
			{
				GameObject go = obj as GameObject;
				GameObject instance = PrefabUtility.InstantiatePrefab(obj) as GameObject;

				bool isMissed = false;
				T[] components = instance.GetComponentsInChildren<T>(true);

				foreach(T c in components)
				{
					//Debug.Log("path:" + p + ",name:" + obj.name);
					if(c.GetComponent<U>() == null)
					{
						Debug.LogError("missing: " + p + ", obj:" + obj.name);
						isMissed = true;
						c.gameObject.AddComponent<U>();
					}
				}

				if(isMissed)
					PrefabUtility.ReplacePrefab(instance, go, ReplacePrefabOptions.ConnectToPrefab);
				else
					Object.DestroyImmediate(instance);
			}
		}

		AssetDatabase.SaveAssets();
	}
}
