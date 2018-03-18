using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using CitrusFramework;

public class UserDataChooseWarningController : MonoBehaviour
{
	public Text _title;
	public Button _yesButton;
	public Button _noButton;

	Callback _yesCallback;
	Callback _noCallback;

	// Use this for initialization
	void Start()
	{
		EventTriggerListener.Get(_yesButton.gameObject).onClick += YesButtonDown;
		EventTriggerListener.Get(_noButton.gameObject).onClick += NoButtonDown;

		_title.text = LocalizationConfig.Instance.GetValue("userData_chooseWarning");
	}

	public void Init(Callback yesCallback, Callback noCallback)
	{
		_yesCallback = yesCallback;
		_noCallback = noCallback;
	}

	void YesButtonDown(GameObject obj)
	{
		if(_yesCallback != null)
			_yesCallback();

		CloseSelf();
	}

	void NoButtonDown(GameObject obj)
	{
		if(_noCallback != null)
			_noCallback();

		CloseSelf();
	}

	void CloseSelf()
	{
		Destroy(gameObject);
	}
}

