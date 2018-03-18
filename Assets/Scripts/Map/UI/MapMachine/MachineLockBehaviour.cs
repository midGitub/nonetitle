using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CitrusFramework;

public enum MachineLockUIType{
	Thumbnail,// 锁提示，小
	Detail,// 锁提示，大
}

public class MachineLockBehaviour : MonoBehaviour {
	// 锁提示
	public GameObject _thumbnail;
	// 锁提示
	public GameObject _detail;
	// 解锁等级字样
	public Text _thumbUnlockLv;
	// 解锁等级字样
	public Text _detailUnlockLv;

	// 解锁特效
	public GameObject _unlockEffect;

	// 当前锁UI类型
	private MachineLockUIType _lockType = MachineLockUIType.Thumbnail;

	// 锁界面切回的时间
	public float _lockReserveTime = 4.0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SwitchLockUI(){
		if (_lockType == MachineLockUIType.Thumbnail) {
			SwitchLockUI (MachineLockUIType.Detail);
		} 
//		else if (_lockType == MachineLockUIType.Detail) {
//			SwitchLockUI (MachineLockUIType.Thumbnail);
//		}
	}

	public void SwitchLockUI(MachineLockUIType type){
		if (type == MachineLockUIType.Detail) {
			_thumbnail.SetActive (false);
			// _detail.SetActive (true);
			UnityTimer.Start (this, _lockReserveTime, () => {
				SwitchLockUI(MachineLockUIType.Thumbnail);
				_lockType = MachineLockUIType.Thumbnail;
			});
		} else if (type == MachineLockUIType.Thumbnail) {
			_thumbnail.SetActive (true);
			// _detail.SetActive (false);
		}
		_lockType = type;
	}

	public void ShowUnlockEffect(bool show){
		if (_unlockEffect != null) {
			_unlockEffect.SetActive (show);
		}
	}

	public void Init(string machineName){
	    if (_thumbUnlockLv != null)
	    {
	        int unlockVipLv = MachineUnlockSettingConfig.Instance.GetUnlockVipLevel(machineName);
            _thumbUnlockLv.text = MachineUnlockSettingConfig.Instance.IsVipMachine(machineName)
                 ? VIPConfig.Instance.FindVIPDataByLevel(unlockVipLv).VIPLevelName.ToUpper() + " VIP"
                 : " LEVEL " + MachineUnlockSettingConfig.Instance.GetUnlockLevel(machineName);
	    }
	    // if (_detailUnlockLv != null)
	    // 	_detailUnlockLv.text = " LEVEL "+_unlockLevel.ToString ();
	}

	public void ShowLockUI(bool show){
		gameObject.SetActive (show);
		if (show) {
			SwitchLockUI (MachineLockUIType.Thumbnail);
		}
	}
}

