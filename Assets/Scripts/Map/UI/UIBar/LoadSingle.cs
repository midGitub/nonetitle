using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;

public class LoadSingle : MonoBehaviour
{
	public GameObject StorePerfab;
	public GameObject SceneMangerPerfab;
	public GameObject ComingSoonGameObject;
	public GameObject AudioManagerObject;
	public GameObject SettingPerfab;
	public GameObject VIPInforObject;
	public GameObject UserDataUI;
	public GameObject PiggyBank;
	public GameObject NoCollectionObject;
    public GameObject BackButtonPrefab;

	private static bool _isLoad = false;

	public static GameObject _reporter;

	public static void Load()
	{
		if(!_isLoad)
		{
			GameObject o = AssetManager.Instance.LoadAsset<GameObject>("Common/LoadSingle");
			DontDestroyOnLoad(o);
			LoadSingle loadSingle = o.GetComponent<LoadSingle>();
			loadSingle.Init();

			_isLoad = true;
		}
	}

	public void Init()
	{
		LoadPrefab(StorePerfab);
		LoadPrefab(SceneMangerPerfab);
		LoadPrefab(ComingSoonGameObject);
		LoadPrefab(AudioManagerObject);
		LoadPrefab(VIPInforObject);
		LoadPrefab(UserDataUI);
		LoadPrefab(NoCollectionObject);
		LoadPrefab(PiggyBank);
		LoadPrefab(SettingPerfab);

		#if UNITY_EDITOR || UNITY_ANDROID
		LoadPrefab(BackButtonPrefab);
		#endif

		CheckLoadReporter();
	}

	void CheckLoadReporter()
	{
#if DEBUG
		if(_reporter == null)
		{
			GameObject o = AssetManager.Instance.LoadAsset<GameObject>("Game/Reporter");
			_reporter = GameObject.Instantiate(o);
		}
#endif
	}

	GameObject LoadPrefab(GameObject go)
	{
		GameObject newgo = null;
		if(go != null)
		{
			newgo = Instantiate(go);
			newgo.SetActive(true);
			DontDestroyOnLoad(newgo);
		}

		return newgo;
	}
}
