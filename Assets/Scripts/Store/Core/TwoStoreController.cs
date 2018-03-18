using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class TwoStoreController : MonoBehaviour {

    [SerializeField]
    private Button _exitButton;

    private WindowInfo _windowInfoReceipt = null;

    public void Show()
    {
		if (GroupConfig.Instance.IsProductExist(StoreType.SmallBuy)) 
		{
			if (_windowInfoReceipt == null)
			{
				_windowInfoReceipt = new WindowInfo(Open, ManagerClose, StoreController.Instance.StoreCanvas, ForceToCloseImmediately);
				WindowManager.Instance.ApplyToOpen(_windowInfoReceipt); 
			}
		}
    }

    public void HideTwoStore()
    {
        SelfClose(() => {
            WindowManager.Instance.TellClosed(_windowInfoReceipt);
            _windowInfoReceipt = null;
        });
    }

    public void Open()
    {
        StoreController.Instance.OpenStoreUI(OpenPos.Auto.ToString(), StoreType.SmallBuy); 
    }

    private void SelfClose(Action callBack)
    {
        StoreController.Instance.CloseAllStoreUI(callBack);
    }

    public void ManagerClose(Action<bool> callBack)
    {
        if (UGUIUtility.CanObjectBeClickedNow(StoreController.Instance.StoreCanvas, _exitButton.gameObject))
        {
            StoreController.Instance.CloseAllStoreUI(() => {
                _windowInfoReceipt = null; 
                callBack(true);
            });
        }
        else
        {
            callBack(false);
        }
    }

    public void ForceToCloseImmediately()
    {
        StoreController.Instance.CloseAllStoreUI(() =>
            {
            }, true);
        _windowInfoReceipt = null;
    }
}
