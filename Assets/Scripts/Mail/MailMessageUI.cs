using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

// zhousen 这个类好想没用到？？？ 

public class MailMessageUI : MonoBehaviour
{
	public Text TitleText;
	public Text MessageText;
	public Text BonusText;

	private Action readOrGetBonusAction;
	private Action DeleteAction;

	private MailInforExtension _mailInforExtension;

	public void ShowUI(MailInfor mailInfor, Action readAction,Action deleteAction)
	{
		TitleText.text = mailInfor.Title;
		TitleText.SetNativeSize();
		#if false
		MessageText.text = mailInfor.Message;
		#else
		MessageText.text = MailUtility.MailInfor2Message(mailInfor);
		#endif
		MessageText.SetNativeSize();
		BonusText.text = StringUtility.FormatNumberStringWithComma((ulong)mailInfor.Bonus);
		readOrGetBonusAction = readAction;
		DeleteAction = deleteAction;
		this.gameObject.SetActive(true);
	}

	public void ReadOrGetBonus()
	{
		if(readOrGetBonusAction != null)
		{
			readOrGetBonusAction();
		}
	}

	public void DeleteMail()
	{
		if(DeleteAction != null)
		{
			DeleteAction();
		}
	}
}
