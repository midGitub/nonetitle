using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class WarningLayer : MonoBehaviour
{
	public Text _mainText;
	public Button _okButton;
	public WidgetJumpController _jumpController;

	private bool _isClosing;

	// Use this for initialization
	void Start () {
		EventTriggerListener.Get(_okButton.gameObject).onClick += OKButtonDown;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public static void ShowWarningLayer(string text)
	{
		GameObject prefab = AssetManager.Instance.LoadAsset<GameObject>("Common/WarningLayer");
		GameObject obj = Instantiate(prefab);
		obj.GetComponent<WarningLayer>().ShowText(text);
	}

	public void ShowText(string text)
	{
		_mainText.text = text;
	}

	public void OKButtonDown(GameObject obj)
	{
		if(!_isClosing)
		{
			_jumpController.Open(false, () => {
				Destroy(gameObject);
			});
		}
		_isClosing = true;
	}
}
