using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CitrusFramework;
using UnityEngine.EventSystems;

public enum BetTipType{
	Lowest,
	Highest,
	Unlock,
	Max,
}

public class BetTipBehaviour : MonoBehaviour {
	// 最低bet提示
	public GameObject _lowestObj;
	// 最高bet提示
	public GameObject _highestObj;
	// 解锁的bet提示
	public GameObject _unlockObj;
	// 解锁bet文字
	public Text _unlockBetText;
	// tips留存时间
	public float _stayTime = 5.0f;

	private bool _isShowTip = false;

	private Coroutine _coroutine = null;

	public bool IsShowTip { get { return _isShowTip; } }

	// Use this for initialization
	void Start () {
		_isShowTip = false;
		_coroutine = null;

		CitrusEventManager.instance.AddListener<LevelupTipEvent> (ShowLevelupTip);
	}

	void OnDestroy(){
		CitrusEventManager.instance.RemoveListener<LevelupTipEvent> (ShowLevelupTip);
	}
	
	// Update is called once per frame
	void Update () {
		if (_isShowTip) {
			// 点击屏幕就强制关闭
			bool touch = false;
			#if UNITY_EDITOR
			touch = Input.GetMouseButtonDown (0);
			#else
			touch = Input.touchCount > 0 && Input.GetTouch (0).phase == TouchPhase.Began;
			#endif
			if (touch){
				CloseBetTip ();
				LogUtility.Log ("close bet tip ", Color.yellow);
			}
		}
	}

	public void ShowBetTip(BetTipType type){
		CloseBetTip ();
		if (type == BetTipType.Lowest) {
			_lowestObj.SetActive (true);
		} else if (type == BetTipType.Highest) {
			_highestObj.SetActive (true);
		} else if (type == BetTipType.Unlock) {
			_unlockObj.SetActive (true);
			SetMaxBetText (_unlockBetText);
		}

		UnityTimer.Start (this, 0.2f, () => {
			_isShowTip = true;
		});

		_coroutine = UnityTimer.Start (this, _stayTime, () => {
			CloseBetTip();
		});
	}

	public void CloseBetTip(){
		_lowestObj.SetActive (false);
		_highestObj.SetActive (false);
		_unlockObj.SetActive (false);
		_isShowTip = false;
		if (_coroutine != null) {
			StopCoroutine (_coroutine);
			_coroutine = null;
		}
	}

	private void SetMaxBetText(Text text){
		if (text == null)
			return;

	    string machineName = GameScene.Instance != null && GameScene.Instance.PuzzleMachine != null
	        ? GameScene.Instance.PuzzleMachine.MachineName
	        : "";
		ulong betMax = BetUnlockSettingConfig.Instance.GetMaxBet(machineName);
		_unlockBetText.text = "MAXBET : " + betMax.ToString();
	}

	private void ShowLevelupTip(LevelupTipEvent e){
		if (!BetUnlockSettingConfig.Instance.IsMaxBet (e.OldLevel)
			&& !BetUnlockSettingConfig.Instance.IsSameBet(e.OldLevel, e.NewLevel)) {
			ShowBetTip (BetTipType.Unlock);
		}
	}
}
