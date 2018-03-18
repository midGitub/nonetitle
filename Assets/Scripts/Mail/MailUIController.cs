using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;
using System;
using UnityEngine.UI;

public class MailUIController : MonoBehaviour
{
	public MailScrollviewController MailSVController;
	public GameObject NoMailShowText;
	public GameObject CollectAllButton;
    [SerializeField]
    private Button _exitButton;
    [SerializeField]
    private Canvas _canvas;

    private WindowInfo _windowInfoReceipt = null;

	void Start()
	{
        _canvas.worldCamera = Camera.main;
		CitrusEventManager.instance.AddListener<LoadSceneFinishedEvent>(LoadSceneCallback);
	}

	public void OnEnable()
	{
		Init();
	}

	public void OnDisable()
	{
		CitrusEventManager.instance.RemoveListener<UpdateMailUIEvent>(UpdateMailUI);
	}

	public void Init()
	{
		RemoveReadedBounsMail();
		if(TryGetMailToShow())
		{
			TryShowCollectAllButton();
			MailSVController.InitMSVC();
			CitrusEventManager.instance.AddListener<UpdateMailUIEvent>(UpdateMailUI);
		}
	}

	void LoadSceneCallback(LoadSceneFinishedEvent e)
	{
		if(_windowInfoReceipt != null)
			Hide();
	}

	private bool TryGetMailToShow()
	{
		var first = DictionaryUtility.DictionaryFirst(UserBasicData.Instance.MailInforDic, (obj) => { 
			return obj.State != MailState.Readed; 
		});
		bool isNotnull = first != default(string);
		MailSVController.gameObject.SetActive(isNotnull);
		NoMailShowText.gameObject.SetActive(!isNotnull);
		CollectAllButton.SetActive(isNotnull);
		return isNotnull;
	}

	public void TryShowCollectAllButton()
	{
		var first = DictionaryUtility.DictionaryFirst(UserBasicData.Instance.MailInforDic, (obj) => { 
			return (obj.State == MailState.DoneConfirm || obj.State == MailState.WaitingConfirm)
				&& MailUtility.HasBonus(obj); 
		});
		bool isNotnull = first != default(string);
		CollectAllButton.SetActive(isNotnull);
	}

	/// <summary>
	/// 移除读过的奖励的邮件,在每次打开邮件的时候调用这个
	/// </summary>
	private void RemoveReadedBounsMail()
	{
		Dictionary<string, MailInfor> willMoveDic = new Dictionary<string, MailInfor>();
		DictionaryUtility.DictionaryWhere(UserBasicData.Instance.MailInforDic, willMoveDic, (obj) => {
			return obj.State == MailState.Readed && MailUtility.HasBonus(obj);// 用接口判断是否邮件有奖励信息
		});
		
		foreach(var item in willMoveDic.Keys)
		{
			UserBasicData.Instance.MailInforDic.Remove(item);
		}

		UserBasicData.Instance.Save();
	}

	public void UpdateMailUI(UpdateMailUIEvent updateUI)
	{
//		MailSVController.DataUpdate(true);
		MailSVController.ItemUpdateList(MailSVController.LastIndex);
	}

	public void CallUpdateBarState()
	{
		CitrusEventManager.instance.Raise(new UpdateMailBarStateEvent(MailServerFunction.GetMail.ToString()));
		// 更新别人先更新自己
		RemoveReadedBounsMail();
		MailSVController.DataUpdate();
	}

    public void Show()
    {
        if (_windowInfoReceipt == null)
        {
            _windowInfoReceipt = new WindowInfo(Open, ManagerClose, _canvas, ForceToCloseImmediately);
            WindowManager.Instance.ApplyToOpen(_windowInfoReceipt, true);// 自动打开的话，需要插到队列顶端，手动打开的话，屏幕上是没有界面的，所以这里填true应该没问题吧？？？
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
        if (_windowInfoReceipt != null)
        {
            WindowManager.Instance.TellClosed(_windowInfoReceipt);
            _windowInfoReceipt = null;
        }

		CitrusEventManager.instance.RemoveListener<LoadSceneFinishedEvent>(LoadSceneCallback);
    }

    public void ForceToCloseImmediately()
    {
        gameObject.SetActive(false); 
        _windowInfoReceipt = null;
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
}
