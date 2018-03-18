using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ThreeStoreController : MonoBehaviour {

    public Button exitButton;

    private WindowInfo _windowInfoReceipt = null;

    public void TryShow(OpenPos openPos)
    {
        if (_windowInfoReceipt == null)
        {
			bool canOpen = false;
            switch (openPos)
            {
				case OpenPos.EnterLobby:
					canOpen = CanOpenEnterLobby();
					if(canOpen)
						_windowInfoReceipt = new WindowInfo(EnterLobbyOpen, ManagerClose, StoreController.Instance.StoreCanvas, ForceToCloseImmediately);
                    break;

				case OpenPos.GameUp:
					canOpen = true;
                    _windowInfoReceipt = new WindowInfo(GameUpOpen, ManagerClose, StoreController.Instance.StoreCanvas, ForceToCloseImmediately);
					break;

                case OpenPos.Lobby:
					canOpen = true;
                    _windowInfoReceipt = new WindowInfo(LobbyOpen, ManagerClose, StoreController.Instance.StoreCanvas, ForceToCloseImmediately);
					break;
            }
            
			if(canOpen)
				WindowManager.Instance.ApplyToOpen(_windowInfoReceipt);
        }
    }

    public void HideThreeStore()
    {
        SelfClose(() => {
            WindowManager.Instance.TellClosed(_windowInfoReceipt);
            _windowInfoReceipt = null; 
        });
    }

	bool CanOpenEnterLobby()
	{
		bool result = false;

		if (UserDeviceLocalData.Instance.IsNewGame)
		{
			result = false;
		}
		else
		{
			TimeSpan span = NetworkTimeHelper.Instance.GetNowTime() - UserDeviceLocalData.Instance.LastShowThreeStoreTime;
			result = (span.TotalMinutes > 30 && !UserDeviceLocalData.Instance.IsFirstLoginToday);
		}
		return result;
	}

    void EnterLobbyOpen()
    {
		StoreController.Instance.OpenStoreUI(OpenPos.EnterLobby.ToString(), StoreType.Deal); 
		UserDeviceLocalData.Instance.LastShowThreeStoreTime = NetworkTimeHelper.Instance.GetNowTime();
    }

    public void GameUpOpen()
    {
        StoreController.Instance.OpenStoreUI(OpenPos.GameUp.ToString(), StoreType.Deal); 
    }

    public void LobbyOpen()
    {
        StoreController.Instance.OpenStoreUI(OpenPos.Lobby.ToString(), StoreType.Deal); 
    }

    private void SelfClose(Action callBack)
    {
        StoreController.Instance.CloseAllStoreUI(callBack);
    }

    public void ManagerClose(Action<bool> callBack)
    {
        if (UGUIUtility.CanObjectBeClickedNow(StoreController.Instance.StoreCanvas, exitButton.gameObject))
        {
            StoreController.Instance.CloseAllStoreUI(() =>
                {
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

//    void OnGUI()
//    {
//        if (GUI.Button(new Rect(10, 100, 100, 50), "ExitSPos"))
//        {
//            Debug.Log(Camera.main.WorldToScreenPoint(exitButton.transform.position));
//            Vector3 screenPos = Camera.main.WorldToScreenPoint(exitButton.transform.position);
//            PointerEventData exit = new PointerEventData(EventSystem.current);                            // This section prepares a list for all objects hit with the raycast
//            exit.position = new Vector2(screenPos.x, screenPos.y);
//            List<RaycastResult> objectsHit = new List<RaycastResult> ();
//            EventSystem.current.RaycastAll(exit, objectsHit);
//            Debug.Log(objectsHit.Count);
//            int i;
//            for (i = 0; i < objectsHit.Count; i++)
//            {
//                Debug.Log(objectsHit[i].gameObject.name);
//            }
//        }         
//    }
}
