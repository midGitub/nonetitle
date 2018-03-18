using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiftInitBehaviour : MonoBehaviour {
	private string _url = "default";
	private bool _initServerVerify = false;

	// Use this for initialization
	void Start () {
		DontDestroyOnLoad(gameObject);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SentGiftURL(string url){
		LogUtility.Log("GiftInitBehaviour url = " + url, Color.yellow);
		
		_url = url;
		GiftHelper.Instance.SentURL(url);
		GiftCodeServerVerify();
	}

	// 只做一次
    // 礼包码界面
    private void GiftCodeServerVerify(){
		// if (!_initServerVerify){
			LogUtility.Log("GiftCodeServerVerify", Color.yellow);
			GiftHelper.Instance.ServerVerify();
			// _initServerVerify = true;
		// }
    }
}
