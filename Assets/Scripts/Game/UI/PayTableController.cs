using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PayTableController : MonoBehaviour {

    public Canvas canvas;
    public Animator animator;
    public CloseGameObject closeGameObject;

    private WindowInfo _windowInfoReceipt = null;
	private Vector3 _ipadScale = new Vector3(1.1f, 1.1f, 1.0f);//  这里要偷个懒，拷贝了UIAdapterBehaviour里的代码，因为实在不想去每个paytable挂脚本了

	void Start()
	{
		Button closeButton = closeGameObject.GetComponent<Button>();

		//disable the persistent callbacks in prefab
		int eventCount = closeButton.onClick.GetPersistentEventCount();
		for(int i = 0; i < eventCount; i++)
		{
			closeButton.onClick.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
		}

		//add new listener
		closeButton.onClick.AddListener(Hide);

		PerformCanvasAdapt();

		bool isIpad = DeviceUtility.IsIPadResolution();
		DoIpaddapt(isIpad);
	}

    public void Show(bool isFirst)
    {
        if (_windowInfoReceipt == null)
        {
            if (isFirst)
                _windowInfoReceipt = new WindowInfo(FirstOpen, ManagerClose, canvas, ForceToCloseImmediately);
            else
                _windowInfoReceipt = new WindowInfo(NotFirstOpen, ManagerClose, canvas, ForceToCloseImmediately);
            
            WindowManager.Instance.ApplyToOpen(_windowInfoReceipt);
        }
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
		//todo, by nichos:
		//Do we really need this code? I think it could be removed
//		if (_windowInfoReceipt != null && WindowManager.Instance != null)
//        {
//            WindowManager.Instance.TellClosed(_windowInfoReceipt);
//            _windowInfoReceipt = null;
//        }
    }

    public void ForceToCloseImmediately()
    {
        gameObject.SetActive(false); 
        _windowInfoReceipt = null;
    }

    public void FirstOpen()
    {
        if(animator)
            animator.enabled = true;

        gameObject.SetActive(true);     
    }

    public void NotFirstOpen()
    {
        gameObject.SetActive(true);    
    }

    private void SelfClose(Action callBack)
    {
        closeGameObject.Close(gameObject, callBack);
    }

    public void ManagerClose(Action<bool> callBack)
    {
        if (UGUIUtility.CanObjectBeClickedNow(canvas, closeGameObject.gameObject))
        {
            closeGameObject.Close(gameObject, () => {
                _windowInfoReceipt = null;
                callBack(true);
            });
        }
        else
        {
            callBack(false);
        }
    }

	void PerformCanvasAdapt()
	{
		CanvasScaler scaler = gameObject.GetComponentInChildren<CanvasScaler>(true);
		Debug.Assert(scaler != null);
		if(scaler != null && scaler.gameObject.GetComponent<CanvasScalerAdaptor>() == null)
			scaler.gameObject.AddComponent<CanvasScalerAdaptor>();
	}

	private void DoIpaddapt(bool isIpad){
		if (isIpad)
		{
            Transform panel = gameObject.transform.Find("Canvas (1)/Panel");
            Debug.Assert(panel != null);
			Vector3 localScale = panel.localScale;
			Vector3 newScale =  new Vector3(localScale.x * _ipadScale.x, localScale.y * _ipadScale.y, localScale.z * _ipadScale.z);
			panel.localScale = newScale;
		}
	}
}
