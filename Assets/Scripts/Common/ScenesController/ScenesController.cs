using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class ScenesController : Singleton<ScenesController>
{
	public static readonly string MainMapSceneName = "MainMap";
	public static readonly string GameSceneName = "Game";
	public static readonly string StartLoadingSceneName = "StartLoading";

	public GameObject _loadingGameObject;
	public GameObject _RotateGameObject;

    private WaitForSeconds _discreteRotateSpan = new WaitForSeconds(0.1f);

    //Xhj loading 图标是否active
    public bool IsAsyncLoading { get { return _loadingGameObject.activeInHierarchy; } }

	//	public void LoadScene(string sceneName)
	//	{
	//		SceneManager.LoadScene(sceneName);
	//	}

	void Start()
	{
		Debug.Assert(_loadingGameObject != null, "loading game obj is null");
		Debug.Assert(_RotateGameObject != null, "rotate game obj is null");
	}

	public string GetCurrSceneName()
	{
		//Debug.Log(SceneManager.GetActiveScene().name);
		return SceneManager.GetActiveScene().name;
	}

	public IEnumerator StartLoadingEnterMainMapScene()
	{
		CitrusEventManager.instance.Raise(new EnterMainMapSceneEvent());
		//CitrusEventManager.instance.Raise(new SaveUserDataToServerEvent());
		yield return StartCoroutine(LoadAsyncNoUI(MainMapSceneName));
	}

	public void EnterMainMapScene(Action callback)
	{
		LoadSceneAsync(MainMapSceneName, () => {
			if(callback != null)
				callback();
			SceneLoadEndCallback();
		});
		CitrusEventManager.instance.Raise(new EnterMainMapSceneEvent());
		CitrusEventManager.instance.Raise(new SaveUserDataToServerEvent());
	}

	public void EnterGameScene(Action callback)
	{
		LoadSceneAsync(GameSceneName, () => {
			if(callback != null)
				callback();
			SceneLoadEndCallback();
		});
		CitrusEventManager.instance.Raise(new EnterGameSceneEvent());
	}

	#region Private

    private void LoadSceneAsync(string sceneName, Action callBack)
	{
		if(_loadingGameObject != null)
			_loadingGameObject.SetActive(true);

        StartCoroutine(LoadAsync(sceneName, callBack));
	}

	private IEnumerator StartRotate()
	{
		while(true)
		{
			if(_RotateGameObject != null)
				_RotateGameObject.transform.Rotate(-36 * Vector3.forward);
            yield return _discreteRotateSpan;
		}
	}

    private IEnumerator LoadAsync(string sceneName, Action callBack)
	{
		var sRotatI = StartCoroutine(StartRotate());
		yield return new WaitForSeconds(0.5f);
        UIManager.Instance.CleanPopupsOnSceneLoad();
        AsyncOperation loadInfor = SceneManager.LoadSceneAsync(sceneName);
		loadInfor.allowSceneActivation = true;
//		bool InterstitialPlayed = false;
		while(!loadInfor.isDone)
		{
			LogUtility.Log("loading scene progress :" + loadInfor.progress, Color.red);
			yield return new WaitForSecondsRealtime(0.1f);
		}

		CitrusEventManager.instance.Raise(new LoadSceneFinishedEvent(sceneName));
        if (callBack != null)
            callBack();

		StopCoroutine(sRotatI);
		if(sceneName == GameSceneName)
		{
			// 可以播放广告广告之后关闭
			if(ShowADSController.Instance.TryGetPlaqueADS())
			{
				//这里用=而不用!=是因为这些广告事件只有此对象会关注，防止多次关注。
				ADSManager.Instance.InterstitialFinished = _OnInterstitialFinished;
				_interstitialHasOpened = false;
				ADSManager.Instance.InterstitialOpened = _OnInterstitialOpened;
				StartCoroutine(_checkInterstitialHasOpened());
				CitrusEventManager.instance.Raise(new ShowInterstitialEvent());
			}
			// 不可以播放广告直接关闭
			else
			{
				_CloseLoading();
			}
		}
		else
		{
			_CloseLoading();
		}
	}

	void SceneLoadEndCallback()
	{
		UserBasicData.Instance.Save();
		UserMachineData.Instance.Save();
		UserDeviceLocalData.Instance.Save();
	}

	#endregion

	#region Interstitial

	private bool _interstitialHasOpened;

	/// <summary>
	/// Checks the interstitial has opened. 为了预防iOS平台下插屏广告没有Open回调的情况，万一没有回调，也可以在3秒钟后隐藏_loadingGameObject。
	/// </summary>
	/// <returns>The interstitial has opened.</returns>
	private IEnumerator _checkInterstitialHasOpened()
	{
		//By nichos:
		//I think the interstitial callbacks are not reliable at all, in terms of two cases:
		//(1) (confirmed) sometimes there is no Open and Close callback
		//(2) (I guess) sometimes when the player clicks the close button, the Close callback 
		//    isn't called until about 30 seconds later.
		//So don't depend on the callbacks, just remove loading circle after the Ads starts to play

		yield return new WaitForSeconds(2);

        //if (!_interstitialHasOpened)
		if(true)
        {
            //LogUtility.Log("!!!Interstitial Open CallBack missed!!!", Color.red);
            ADSManager.Instance.StartPrepareWatch = false;
            _CloseLoading();
        }
	}

	private void _OnInterstitialOpened()
	{
		_interstitialHasOpened = true;	
	}

	private void _OnInterstitialFinished()
	{
		_CloseLoading();	
		ADSManager.Instance.InterstitialFinished -= _OnInterstitialFinished;
		ADSManager.Instance.InterstitialOpened -= _OnInterstitialOpened;
	}

	private void _CloseLoading()
	{
		if(_loadingGameObject != null)
			_loadingGameObject.SetActive(false);
	}

    private static int _tempEventCount = 1;

	public IEnumerator LoadAsyncNoUI(string sceneName)
	{
		Debug.Log("LoadAsyncNoUI: " + sceneName);
		AsyncOperation loadInfor = SceneManager.LoadSceneAsync(sceneName);
		loadInfor.allowSceneActivation = true;
		while(!loadInfor.isDone)
		{
			Debug.Log("loading scene progress :" + loadInfor.progress);
			yield return new WaitForSecondsRealtime(0.1f);
		}

        LogUtility.Log("SceneController raise(" + sceneName + ") event" + (_tempEventCount++), Color.magenta); 
		CitrusEventManager.instance.Raise(new LoadSceneFinishedEvent(sceneName));
		yield break;
	}

	#endregion
}
