using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using CitrusFramework;

public class MapMachineController : MonoBehaviour {
	private static readonly string _machineUnlockEffectPath = "Effect/Prefab/FX_MachineUnlock";
    private static readonly string _tinyUnlockEffectPath = "Effect/Prefab/FX_MachineSmallUnlock";
	// 锁
	public MachineLockBehaviour _lockBehaviour;
	public MapMachineDownloader _machineDownloader;

	// 机台名
	private string _machineName;
	// 是否可以解锁
	private bool _unlock = false;
	// 机台动画器
	private MapMachineAnimatorController _animatorController = null;
	// 触发解锁
	private bool _isTriggerUnlock = false;
	// 解锁特效
	private GameObject _unlockEffect = null;
    private static Vector3 _ipadMapMachineScale = new Vector3(1.15f, 1.15f, 1.0f);

	public bool IsUnlock { get { return _unlock; } }
	public string MachineName { get { return _machineName; } }
   
	void Awake(){
        _animatorController = gameObject.GetComponent<MapMachineAnimatorController>();
        InitMachineName();
        InitMachineUnlockEffect();

		// Note: the order of the two functions are critical, don't change the order
		InitUnlockValue();
		InitMachineDownloader();

#if UNITY_EDITOR
        UnlockMachineInEditor();
#endif

        CitrusEventManager.instance.AddListener<UserDataLoadEvent> (HandleUnlockAnimationUserDataLoad);
		// 刚进游戏时，这里DayBonus的Event Raise会发生在这里之前，所以第一次的CloseDayBonus是捕获不到的
		CitrusEventManager.instance.AddListener<DailyBonusFinishEvent> (HandleUnlockAnimationAfterDailyBonus);
        CitrusEventManager.instance.AddListener<AskUnlockMachineEvent>(HandleAskUnlockEvent);
        CitrusEventManager.instance.AddListener<EnterMapRoomEvent>(HandleEnterMapRoomEvent);
	}

	void OnDestroy(){
		CitrusEventManager.instance.RemoveListener<UserDataLoadEvent> (HandleUnlockAnimationUserDataLoad);
		CitrusEventManager.instance.RemoveListener<DailyBonusFinishEvent> (HandleUnlockAnimationAfterDailyBonus);
        CitrusEventManager.instance.RemoveListener<AskUnlockMachineEvent>(HandleAskUnlockEvent);
        CitrusEventManager.instance.RemoveListener<EnterMapRoomEvent>(HandleEnterMapRoomEvent);
    }

	// Use this for initialization
	void Start () {
		// ipad 适配
		if (gameObject.GetComponent<UIAdapterBehaviour>() == null){
			UIAdapterBehaviour adapter = gameObject.AddComponent<UIAdapterBehaviour>();
			adapter.UpdateScale(_ipadMapMachineScale);
			adapter.DisableAutoAdapt();
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void InitMachineName()
	{
		//Assumption: the prefab name follows the format "MapMachine_{MachineName}"
		string[] array = gameObject.name.Split(new char[]{'_', '(', ')'}, StringSplitOptions.RemoveEmptyEntries);
		_machineName = array[1];
	}

	void InitMachineDownloader()
	{
		if(_machineDownloader != null)
			_machineDownloader.Init(_machineName, this);
	}

	void InitMachineUnlockEffect()
	{
	    bool isTinyMachine = MachineUnlockSettingConfig.Instance.IsTinyMachine(_machineName);
	    string unlockEffectPath = isTinyMachine ? _tinyUnlockEffectPath : _machineUnlockEffectPath;
        GameObject obj = AssetManager.Instance.LoadAsset<GameObject> (unlockEffectPath);
		if (obj != null) {
			_unlockEffect = UGUIUtility.CreateObj (obj, gameObject);
			if (_unlockEffect != null) {
				_unlockEffect.SetActive (false);
				_unlockEffect.name = StringUtility.SubstractStr (_unlockEffect.name, "(Clone)");
			}
		}
	}

	void InitUnlockValue()
	{
		_unlock = MachineUnlockHelper.CheckMachineUnlock(_machineName);
	}

	public void SwitchLockUI(){
		if (_lockBehaviour != null) {
			_lockBehaviour.SwitchLockUI ();
		}
	}

	private void HandleUnlockAnimationUserDataLoad(UserDataLoadEvent Event){
		// 刷新用户数据时，需要判断是否会解锁
		_unlock = MachineUnlockHelper.CheckMachineUnlock (_machineName);
		_isTriggerUnlock = _unlock && !UserMachineData.Instance.IsMachineUnlock (_machineName);
		#if false
		if (!BonusHelper.CanGetDayBonus ()) {
			StartPlayAnimation ();
		}
		#else
		if (!DayBonus.CheckRefreshBonusState ()) {
			//Debug.Log ("play machine animation after UserDataLoad machine = "+_machineName + " unlock = "+_unlock+" triggerUnlock = "+_isTriggerUnlock);
			StartPlayAnimation ();
		}
		#endif
	}

	private void HandleUnlockAnimationAfterDailyBonus(DailyBonusFinishEvent Event){
		//Debug.Log ("play machine animation AfterDailyBonus machine = "+_machineName + " unlock = "+_unlock+" triggerUnlock = "+_isTriggerUnlock);
		// dailybonus后判断是否处理解锁动画
		StartPlayAnimation ();
	}

#if UNITY_EDITOR
    void UnlockMachineInEditor()
    {
        if (!StartLoading.StartLoadingSceneHasLoaded)
        {
            //HandleLoadSceneFinishEvent(new LoadSceneFinishedEvent(ScenesController.MainMapSceneName));
        }
    }
#endif

    void HandleEnterMapRoomEvent(EnterMapRoomEvent e)
    {
        if (gameObject.activeInHierarchy && _lockBehaviour != null)
        {
           HandleMachineUnlock();
        }
    }

    void HandleAskUnlockEvent(AskUnlockMachineEvent e)
    {
        if (e.UnlockNameList.Contains(_machineName) && gameObject.activeInHierarchy && _lockBehaviour != null)
        {
            HandleMachineUnlock();
        }
    }

    void HandleMachineUnlock()
    {
        _unlock = MachineUnlockHelper.CheckMachineUnlock(_machineName);
        // 是否本次触发解锁
        _isTriggerUnlock = _unlock && !UserMachineData.Instance.IsMachineUnlock(_machineName);

        // 已解锁机台
        bool alreadyUnlock = !_isTriggerUnlock && _unlock;
        // 本次解锁，但是已领取每日奖励
        bool newUnlock = _isTriggerUnlock && !DayBonus.CheckRefreshBonusState();

        if (alreadyUnlock || newUnlock)
        {
            //Debug.Log ("play machine animation in Awake machine="+_machineName + " alreadyUnlock = "+alreadyUnlock+" newUnlock = "+newUnlock);
            if (_unlockEffect != null)
                _unlockEffect.SetActive(false);

                StartPlayAnimation(1.0f);
        }

        if (_lockBehaviour != null){
	        // 判断是否需要开启锁界面
	        _lockBehaviour.Init(_machineName);
	        _lockBehaviour.ShowLockUI(!alreadyUnlock);
        }
    }

    private void StartPlayAnimation(float delay = 1.0f){
		if ( !gameObject.activeInHierarchy)
			return;

		if (_unlock) {
			// 本次解锁
			if (_isTriggerUnlock) {
                UserMachineData.Instance.SetMachineUnlock (_machineName, _unlock, true);
				UnityTimer.Start (this, delay, () => {
					if (_animatorController != null){
						_animatorController.Unlock ();	
					}
					if (_lockBehaviour != null){
						_lockBehaviour.ShowLockUI(false);
					}
					if (_unlockEffect != null) {
						_unlockEffect.SetActive(true);
					}
				});
				if(_machineDownloader != null)
					_machineDownloader.RefreshDownloadIcon();
			} else {
				if (_animatorController != null){
					_animatorController.StartPlay ();
				}
			}
		}
	}
}
