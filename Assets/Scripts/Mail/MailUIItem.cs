using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CitrusFramework;

public class MailUIItem : MonoBehaviour
{
	public Text TitleText;
	public Text MessageText;
	public Text BonusText;
	public CloseAndOpenUI PointUI;
	public CloseAndOpenUI Button;
	public GameObject DeleteButton;
	public GameObject News;
	//public GameObject Mask;

	private MailInfor currMailInfor;
	private MailInforExtension curMailExtension;
	private string currMailKey;
	private MailMessageUI mailMessageUI;
	public CoinCollectController _coinEffectController;
	public MailUIController _mailUIController;

	public void UpdateData(string key)
	{
		var mailInfor = UserBasicData.Instance.MailInforDic[key];
		currMailInfor = mailInfor;
		curMailExtension = MailUtility.MailInfor2MailInforExtension (mailInfor);
		currMailKey = key;
		// todo 现在没有title之后有了加上去
		TitleText.text = mailInfor.Title;
		MessageText.text = MailUtility.MailInfor2Message(mailInfor);
		// 用接口封装
		BonusText.text = MailUtility.GetMailBonusText (currMailInfor, curMailExtension);

		// 如果是没有奖励的邮件直接默认已读
		if (!MailUtility.HasBonus(mailInfor))
		{
			mailInfor.State = MailState.Readed;
		}

		//更新他的状态,可能刷新领取状态
		UISetting(currMailInfor);
	}

	private void UISetting(MailInfor mailInfor)
	{
		News.SetActive(mailInfor.State == MailState.DoneConfirm);
		//Mask.SetActive(mailInfor.State == MailState.Readed);
		PointUI.UpdateState(mailInfor.State == MailState.DoneConfirm);
		if (!MailUtility.HasBonus(mailInfor))
		{
			Button.gameObject.SetActive(false);
			DeleteButton.SetActive(true);
		}
		else
		{
			Button.UpdateState(mailInfor.State == MailState.DoneConfirm);
			Debug.Log ("mailinfo state = "+mailInfor.State);
			DeleteButton.SetActive(false);
		}
	}

	public void Read()
	{
		// 已经读取过了不重新读取
		if(currMailInfor.State == MailState.Readed)
		{
			return;
		}
		currMailInfor.State = MailState.Readed;

		// 用接口封装
		MailUtility.GetMailReward (currMailInfor, curMailExtension);
		AnalysisManager.Instance.ReadMail (currMailInfor, curMailExtension);

		CitrusEventManager.instance.Raise(new UpdateMailUIEvent());

		if (_coinEffectController != null) {
			_coinEffectController.Show (true);
		}

		_mailUIController.TryShowCollectAllButton ();
	}

	public void DeleteMail()
	{
		if(currMailInfor.State == MailState.Readed)
		{
			// 加个保护，防止崩溃
			if (UserBasicData.Instance.MailInforDic.ContainsKey (currMailKey)) {
				UserBasicData.Instance.MailInforDic.Remove(currMailKey);
			}
			UserBasicData.Instance.Save();
			CitrusEventManager.instance.Raise(new UpdateMailUIEvent());
		}
	}

	public void ShowMessageUI()
	{
		mailMessageUI.ShowUI(currMailInfor, Read, DeleteMail);
	}
}
