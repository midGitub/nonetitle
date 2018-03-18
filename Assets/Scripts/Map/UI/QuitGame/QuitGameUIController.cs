using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class QuitGameUIController : MonoBehaviour {

    [SerializeField]
    private Button _exitButton;
    [SerializeField]
    private Canvas _canvas;

    private WindowInfo _windowInfoReceipt = null;
	
    void Start(){
        _canvas.worldCamera = Camera.main;
    }

    public void Show()
    {
        if (_windowInfoReceipt == null)
        {
            _windowInfoReceipt = new WindowInfo(Open, ManagerClose, _canvas, ForceToCloseImmediately);
            WindowManager.Instance.ApplyToOpen(_windowInfoReceipt);
        }
    }

    public void Quit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        //Application.Quit();
		System.Diagnostics.Process.GetCurrentProcess().Kill();
        #endif
    }

    public void Hide()
    {
        SelfClose(() => {
            WindowManager.Instance.TellClosed(_windowInfoReceipt);
            _windowInfoReceipt = null;
        });
    }

    void OnDestroy()
    {
		//By nichos:
		//I comment this code, otherwise it would cause non-fatal crash. Guess how?
//        if (_windowInfoReceipt != null)
//        {
//            WindowManager.Instance.TellClosed(_windowInfoReceipt);
//            _windowInfoReceipt = null;
//        }
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }

    private void SelfClose(Action callBack)
    {
        gameObject.SetActive(false);
        callBack(); 
    }

    public void ManagerClose(Action<bool> callBack)
    {
        if (UGUIUtility.CanObjectBeClickedNow(_canvas, _exitButton.gameObject))
        {
            gameObject.SetActive(false);
            _windowInfoReceipt = null;
            callBack(true);
        }
        else
        {
            callBack(false);
        }
    }	

    public void ForceToCloseImmediately()
    {
        gameObject.SetActive(false); 
        _windowInfoReceipt = null;
    }
}
