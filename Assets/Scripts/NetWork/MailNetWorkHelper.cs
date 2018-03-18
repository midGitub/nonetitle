using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;
using System;

public enum MailServerFunction
{
	GetReward,
	GetMail,
	DeletMail,
	AddMail,
	GetBillBoard,
}

public class MailNetWorkHelper : NetWorkHelperAbstract<MailNetWorkHelper>
{
	private const string _getMail = "mail/get";
	private const string _mailMove = "mail/remove";
	private const string _addMail = "test/mail/add";
	private const string _getReward = "bonus/get";
	private const string _getBillBoard = "notice/get";

	public MailNetWorkHelper() : base()
	{
#if DEBUG
		_baseUrl = "http://inner.linux.citrusjoy.cn:49010/";
#endif
	}

	public override void AddServerFunctionName()
	{
		ServerFunctionName = new Dictionary<string, string>
		{
			{MailServerFunction.GetMail.ToString(),_getMail},
			{MailServerFunction.DeletMail.ToString(),_mailMove},
			{MailServerFunction.AddMail.ToString(),_addMail},
			{MailServerFunction.GetReward.ToString(),_getReward},
			{MailServerFunction.GetBillBoard.ToString(),_getBillBoard}
		};
	}

	protected override void AddDefaultValueToDic(Dictionary<string, object> dic , string functionName = null)
	{
		base.AddDefaultValueToDic(dic);
		if(functionName == MailServerFunction.GetBillBoard.ToString() || functionName == MailServerFunction.GetReward.ToString())
			dic.Add("Version", BuildUtility.GetBundleVersion());
		if (functionName == MailServerFunction.GetBillBoard.ToString())
		{
			dic.Add("RecvID", int.Parse(UserBasicData.Instance.LastNoticeID));
			dic.Add("Lang", "en");

			#if DEBUG
			dic.Add("IsDebug",1);
			#else	
			dic.Add("IsDebug",0);
			#endif

			dic.Add("Channel", PlatformManager.Instance.GetServerChannelString());
		}
	}
}
