using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CitrusFramework;

public class JackpotNotifyBehaviour : MonoBehaviour {
	public Image _playerPortrait;
	public Image _machineImage;
	public Text _playerName;
	public Text _jackpotBonus;

	// 面板对象
	public GameObject _content;
	// 流光特效
	public GameObject _effect;
	// 面板滞留时间
	public float _stayTime = 10.0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnEnable(){
		ShowNotify ();
	}

	public void ShowNotify(){
		if (_content != null)
			_content.SetActive (true);
		if (_effect != null)
			_effect.SetActive (true);
		UnityTimer.Start (this, _stayTime, () => {
			if (_content != null)
				_content.SetActive(false);
			if (_effect != null)
				_effect.SetActive(false);
		});
	}

	public void SetPlayerPortrait(Sprite spr){
		_playerPortrait.sprite = spr;
	}

	public void SetMachineSprite(Sprite spr){
		_machineImage.sprite = spr;
	}

	public void SetPlayerName(string name){
		_playerName.text = name;
	}

	public void SetJackpotBonus(ulong bonus){
		_jackpotBonus.text = bonus.ToString();
	}
}
