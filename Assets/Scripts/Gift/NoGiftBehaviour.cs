using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class NoGiftBehaviour : MonoBehaviour {
	private WindowInfo _windowInfoReceipt = null;
	public Canvas _canvas;
	public Button _exitButton;
	public Button _confirmButton;
	private NoGiftType _type = NoGiftType.NotExist;

	public GameObject _notExistText;
	public GameObject _alreadyGotText;
	public GameObject _runOutText;
	public GameObject _channelText;

	// Use this for initialization
	void Start () {
		_exitButton.onClick.AddListener(HandleExit);
		_confirmButton.onClick.AddListener(HandleConfirm);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void Open()
	{
		if (_canvas != null) {
			_canvas.worldCamera = Camera.main;
		}

		Open(_type);
	}

	public void ManagerClose(Action<bool> callBack)
    {
        if (UGUIUtility.CanObjectBeClickedNow(_canvas, _exitButton.gameObject) 
			|| UGUIUtility.CanObjectBeClickedNow(_canvas, _confirmButton.gameObject))
        {
            callBack(true);
			ForceToCloseImmediately();
        }
        else
        {
            callBack(false);
        }
    }

	private void ForceToCloseImmediately()
	{
        _windowInfoReceipt = null;
		gameObject.SetActive (false);
		Destroy (gameObject);
	}

	private void HandleConfirm(){
		// TODO
		ForceToCloseImmediately();
	}

	private void HandleExit(){
		// TODO
		ForceToCloseImmediately();
	}

	public void ShowUI(NoGiftType type){
		_type = type;
		
		if (_windowInfoReceipt == null)
		{
			_windowInfoReceipt = new WindowInfo(Open, ManagerClose, _canvas, ForceToCloseImmediately);
			WindowManager.Instance.ApplyToOpen(_windowInfoReceipt, true);
		}
	}

	private void Open(NoGiftType type){
		if (type == NoGiftType.AlreadyGot){
			_alreadyGotText.SetActive(true);
			_runOutText.SetActive(false);
		}else if (type == NoGiftType.NotExist){
			// not use
			_alreadyGotText.SetActive(false);
			_runOutText.SetActive(true);
		}else if (type == NoGiftType.RunOut){
			_alreadyGotText.SetActive(false);
			_runOutText.SetActive(true);
		}else if (type == NoGiftType.Channel){
			// not use
			_alreadyGotText.SetActive(false);
			_runOutText.SetActive(true);
		}
	}
}
