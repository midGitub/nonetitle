using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;

public class LoadingManager : Singleton<LoadingManager> {
	
	public GameObject _loading;

	public void ShowLoading(bool show){
		if (_loading != null) {
			_loading.SetActive (show);
		}
	}

}
