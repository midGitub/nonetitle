using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundGroup : MonoBehaviour {
	public Image[] imgs;

	public void SetImg(bool isMirror, List<Sprite> sprList){
		for (int i = 0; i < imgs.Length; ++i) {
			imgs [i].sprite = sprList [i];
		}
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
