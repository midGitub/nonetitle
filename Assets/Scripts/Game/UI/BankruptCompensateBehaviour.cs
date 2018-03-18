using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using CitrusFramework;

public class BankruptCompensateBehaviour : MonoBehaviour {
	// 界面
	public GameObject _content;
	// 获得credits
	public Text _coinText;
	// 收集credits按钮
	public Button _collectButton;
	// 本次奖励的credits
	private int _collectCredits;
	// 按钮回调
	public UnityAction _collectionAction = null;
	// 收集特效
	public GameObject _coinEffect;

	// Use this for initialization
	void Start () {
		Canvas canvas = gameObject.GetComponent<Canvas> ();
		if (canvas != null) {
			canvas.worldCamera = Camera.main;
		}

		_collectButton.onClick.AddListener (OnCollectCredits);
	}

	private void OnCollectCredits(){
		if (_collectionAction != null) {
			_collectionAction ();
		}
	}

	public void InitUI(int credits){
		if (_coinText != null) {
			_coinText.text = StringUtility.ConvertDigitalIntToString (credits);
		}

		_collectCredits = credits;
	}

	public void ShowUI(bool show){
		_content.SetActive (show);
	}

	public void CollectCoins(){
		AudioManager.Instance.PlaySound(AudioType.HourlyBonusCreditsRollUp);
		ShowUI(false);
		// 播放特效，然后结束
		_coinEffect.SetActive(true);
		UnityTimer.Start(this, 4.0f, ()=>{
			_coinEffect.SetActive(false);
		});
	}
}
