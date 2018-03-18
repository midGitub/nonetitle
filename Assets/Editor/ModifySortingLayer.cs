using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public static class ModifySortingLayer
{
	static string[] _resPaths = new string[] {
//		"Assets/Resources/Map/PiggyBank/PiggyBank.prefab",
//		"Assets/Resources/Map/BackFlowReward.prefab",
//		"Assets/Resources/Map/Bankrupt/BankruptCredits.prefab",
//		"Assets/Resources/Map/DailyBonus/DayBouns.prefab",
//		"Assets/Resources/Map/DoubleLevelUp/DoubleLevelUpNotify.prefab",
//		"Assets/Resources/Map/Friend/FriendUI.prefab",
//		"Assets/Resources/Map/Gift/NoGiftUI.prefab",
//		"Assets/Resources/Map/Loading/Loading.prefab",
//		"Assets/Resources/Map/MachineComment/MachineCommentUI.prefab",
//		"Assets/Resources/Map/Mail/MailUI.prefab",
//		"Assets/Resources/Map/PayRotaryTable/PayRotaryTable.prefab",
//		"Assets/Resources/Map/QuitGameUI.prefab",
//		"Assets/Resources/Map/ScoreApp/ScoreAppUi.prefab",
//		"Assets/Resources/Map/SettingPrefab/feedback.prefab",
//		"Assets/Resources/Map/SettingPrefab/Setting.prefab",
//		"Assets/Resources/Map/VIP/VIPInfor.prefab",
//		"Assets/Resources/Store/Store.prefab",
//		"Assets/Resources/Store/StoreItem.prefab",
//		"Assets/Resources/Game/SpecialOffer.prefab",
//		"Assets/Resources/Game/UI/DiceUi/DiceUi.prefab",
	};

	[MenuItem("Tools/Modify SortingLayer")]
	static void Run()
	{
		PerformModify("Default", "UI");
		Debug.Log("Modify done");
	}

	static void PerformModify(string fromLayer, string toLayer)
	{
		foreach(string path in _resPaths)
		{
			Object obj = AssetDatabase.LoadAssetAtPath<Object>(path);

			if(obj == null)
				Debug.LogError("Fail to load: " + path);

			if (obj is GameObject)
			{
				Debug.Log("handle: " + path);

				bool isHandled = false;
				GameObject go = obj as GameObject;
				GameObject instance = PrefabUtility.InstantiatePrefab(obj) as GameObject;

				// Handle canvas
				Canvas[] canvases = instance.GetComponentsInChildren<Canvas>(true);

				bool[] statusArray = new bool[canvases.Length];

				for(int i = 0; i < canvases.Length; i++)
				{
					statusArray[i] = canvases[i].gameObject.activeSelf;
					canvases[i].gameObject.SetActive(true);
				}

				for(int i = 0; i < canvases.Length; i++)
				{
					Canvas c = canvases[i];
					c.worldCamera = Camera.main;

					if(c.renderMode == RenderMode.ScreenSpaceCamera)
					{
						if(c.sortingLayerName == fromLayer)
						{
							c.sortingLayerName = toLayer;
							isHandled = true;
							Debug.Log("Canvas change ok: " + c.gameObject.name);

							if(c.sortingLayerName == fromLayer)
								Debug.LogError("Canvas change fail, path:" + path + ", name:" + c.gameObject.name);
						}
					}

					c.worldCamera = null;
				}

				for(int i = 0; i < canvases.Length; i++)
				{
					canvases[i].gameObject.SetActive(statusArray[i]);
				}

				// Handle particle
				ParticleSystemRenderer[] renderers = instance.GetComponentsInChildren<ParticleSystemRenderer>(true);
				foreach(ParticleSystemRenderer r in renderers)
				{
					if(r.sortingLayerName == fromLayer)
					{
						r.sortingLayerName = toLayer;
						isHandled = true;
						Debug.Log("particle change ok: " + r.gameObject.name);
					}
					else if(r.sortingLayerName != toLayer)
					{
						LogUtility.Log("warning: particle layer is: " + r.sortingLayerName);
					}
				}

				if(isHandled)
					PrefabUtility.ReplacePrefab(instance, go, ReplacePrefabOptions.ConnectToPrefab);
				else
					Object.DestroyImmediate(instance);
			}
		}

		AssetDatabase.SaveAssets();
	}
}
