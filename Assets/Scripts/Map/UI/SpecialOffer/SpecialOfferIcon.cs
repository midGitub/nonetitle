using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpecialOfferIcon : MonoBehaviour 
{
	public Button IconButton;
	// Use this for initialization
	void Start () {
		IconButton.enabled = true;
		IconButton.interactable = true;
		IconButton.onClick.AddListener (IconButtonClick);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
		

	void IconButtonClick()
	{
        AudioManager.Instance.PlaySound(AudioType.Click);
        SpecialOfferHelper.Instance.ShowSpecialWindow (true);
	}
}
