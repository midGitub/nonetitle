using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MailTextConfig : SimpleSingleton<MailTextConfig>
{
	public static readonly string Name = "MailText";

	private MailTextSheet _mailTextSheet;

	public MailTextConfig()
	{
		LoadData();
	}

	void LoadData()
	{
		_mailTextSheet = GameConfig.Instance.LoadExcelAsset<MailTextSheet>(Name);
	}

	public static void Reload()
	{
		Debug.Log("Reload MailTextConfig");
		MailTextConfig.Instance.LoadData();
	}

	public string TryGetTextWithKey(string key)
	{
		var mailtext = ListUtility.FindFirstOrDefault(_mailTextSheet.dataArray, (obj) => { return obj.Key == key; });
		if(mailtext == default(MailTextData))
		{
			return key;
		}
		return mailtext.Val;
	}

	public MailTextData TryGetDataWithKey(string key)
	{
		var mailtext = ListUtility.FindFirstOrDefault(_mailTextSheet.dataArray, (obj) => { return obj.Key == key; });
		if(mailtext == default(MailTextData))
		{
			return null;
		}
		return mailtext;
	}
}
