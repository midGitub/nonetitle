using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CitrusFramework;
using UnityEngine.EventSystems;

public class VIPUIController : Singleton<VIPUIController>
{
	[Serializable]
	public struct VIPIcon
	{
		public Image icon;
		public Text nameText;
	}

	public Canvas ShowCanvas;

	public Image PanlImage;
	public Color BGTransparentColor;
	public Color BGNotTransparentColor;

	public ImageBar vipLevelBar;
	public VIPIcon CurrLevelIcon;
	public VIPIcon NextLevelIcon;

	public Text CurrPointText;
	public Text NextLevelNoteText;

    public CloseGameObject closeGameObject;

    private WindowInfo _windowInfoReceipt = null;

	public void ShowVIPInforUI(bool BGTransparent = false)
	{
		if(ShowCanvas.gameObject.activeSelf) { return; }

		ShowCanvas.gameObject.SetActive(true);
		PanlImage.color = BGTransparent ? BGTransparentColor : BGNotTransparentColor;
		ShowVIPPoint();
	}

	private void ShowVIPPoint()
	{
		var currLevel = VIPSystem.Instance.GetCurrVIPLevelData;
		var currLevelInfor = VIPSystem.Instance.GetCurrVIPInforData;
		var nextLevelInfor = VIPSystem.Instance.GetNextLevelVIPInforData;

	    CurrLevelIcon.icon.sprite = VIPConfig.Instance.GetDiamondImageByLevelName(currLevelInfor.VIPLevelName);
		CurrLevelIcon.nameText.text = currLevelInfor.VIPLevelName;

		NextLevelIcon.icon.sprite = VIPConfig.Instance.GetDiamondImageByLevelName(nextLevelInfor.VIPLevelName);
		NextLevelIcon.nameText.text = nextLevelInfor.VIPLevelName;
		Debug.Log("VIP need point: " + currLevelInfor.VIPLevelNeedPoint + ", " + nextLevelInfor.VIPLevelNeedPoint);
		
		//添加最高级VIP逻辑判断（最高级隐藏下一等级信息）
		bool maxLv = currLevel.Level == nextLevelInfor.VIPLeveL;
		bool show = !maxLv;
		NextLevelIcon.icon.gameObject.SetActive (show);
		NextLevelIcon.nameText.gameObject.SetActive (show);
		NextLevelNoteText.gameObject.SetActive(show);

		CurrPointText.text = maxLv ? currLevel.LevelPoint.ToString() : currLevel.LevelPoint.ToString()+"/"+ nextLevelInfor.VIPLevelNeedPoint;
		if (maxLv) 
		{
			vipLevelBar.barImage.fillAmount = 1;
		}
		else
		{
			vipLevelBar.ChangeBarState(currLevelInfor.VIPLevelNeedPoint, nextLevelInfor.VIPLevelNeedPoint, currLevel.LevelPoint);
		} 
	}

    public void Show(bool overLap = false)
    {
        if (_windowInfoReceipt == null)
        {
            _windowInfoReceipt = new WindowInfo(Open, ManagerClose, ShowCanvas, ForceToCloseImmediately);
            WindowManager.Instance.ApplyToOpen(_windowInfoReceipt,overLap); 
        }
    }

    public void Hide()
    {
        SelfClose(() => {
            WindowManager.Instance.TellClosed(_windowInfoReceipt);
            _windowInfoReceipt = null;
        });
    }

    public void Open()
    {
        ShowVIPInforUI();
    }

    private void SelfClose(Action callBack)
    {
        closeGameObject.Close(ShowCanvas.gameObject, callBack); 
    }

    public void ManagerClose(Action<bool> callBack)
    {
        if (UGUIUtility.CanObjectBeClickedNow(ShowCanvas, closeGameObject.gameObject))
        {
            closeGameObject.Close(ShowCanvas.gameObject, () => {
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
        ShowCanvas.gameObject.SetActive(false); 
        _windowInfoReceipt = null;
    }

//    void OnGUI()
//    {
//        if (GUI.Button(new Rect(10, 100, 100, 50), "ExitSPos"))
//        {
//            Debug.Log(closeGameObject.transform.position);
//            Vector3 screenPos = closeGameObject.transform.position;
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
