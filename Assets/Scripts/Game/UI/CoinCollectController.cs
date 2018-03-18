using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CitrusFramework;

public class CoinCollectController : MonoBehaviour {
	// 特效
	public FriendCoinController _coinEffect;
	// 特效协程
	private Coroutine _coroutine = null;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnDisable(){
		Show (false);
	}

	public void Show(bool show){
		if (_coinEffect != null) {
			_coinEffect.gameObject.SetActive (show);
			if (show) {
				_coinEffect.Stop ();
				_coinEffect.Play ();

				if (_coroutine != null) {
					StopCoroutine (_coroutine);
					_coroutine = null;
				}
				_coroutine = UnityTimer.Start (this, 3.0f, () => {
					_coinEffect.gameObject.SetActive (false);
					_coroutine = null;
				});

				AudioManager.Instance.PlaySound (AudioType.MailBoxCollectCredits);
			}
		}
	}
}
