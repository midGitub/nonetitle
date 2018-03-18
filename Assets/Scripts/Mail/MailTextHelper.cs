using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 邮件MESSAGE 管理器
/// </summary>
public class MailTextHelper : SimpleSingleton<MailTextHelper>
{
	// 文本转换函数
	private static Dictionary<string, Func<MailInfor, string>> textFuncDic = new Dictionary<string, Func<MailInfor, string>>(){
		{MailUtility.NormalMail, NormalMailParse},
		{MailUtility.RankRewardMail, RankRewardMailParse},
		{MailUtility.SystemMail, SystemMailParse},
		{MailUtility.RewardMail,RewardMailParse},
		{MailUtility.BillBoardMail,BillBoardMailParse}
	};

	// 等级字符串
	private static Dictionary<int, string> rankText = new Dictionary<int, string>()
	{ 
		{1,"1st"}, {2,"2nd"}, {3,"3rd"}, {4,"4th"}, {5,"5th"}
	};
	// 分隔符
	private static readonly char DELIMETER = ',';

	#region json or mailinfo to message

	// 邮件中的msg就是本地邮件里的key
	private static string NormalMailParse(MailInfor info){
		return  MailTextConfig.Instance.TryGetTextWithKey(info.Message);
	}

	// 邮件中的msg = "game_torurament" + rank等级组合
	private static string RankRewardMailParse(MailInfor info){
		string message = info.Message;
		string[] strs = info.Message.Split (DELIMETER);
		Debug.Assert (strs.Length > 0);

		if (strs.Length > 0) {
			message = MailTextConfig.Instance.TryGetTextWithKey (strs[0]);
			if (strs.Length > 1) {
				int rankId;
				bool success = int.TryParse(strs [1], out rankId);// rank index
				if (success){
					string rank = "";
					if (rankText.ContainsKey (rankId)) {
						rank = rankText[rankId];
						message = message.Replace ("*", rank);	
					}
				}
			}
		}
		return message;
	}

	// 邮件中的msg是json串，需要解析出来，_system_msg下的内容为显示的message
	private static string SystemMailParse(MailInfor info){
		string message = "";
		JSONObject obj = new JSONObject (info.Message);
		if (obj.HasField (MailUtility._system_msg)) {
			message = obj.GetField (MailUtility._system_msg).str;
		}
		return message;
	}

	private static string RewardMailParse(MailInfor info)
	{
		return info.Title;
	}

	private static string BillBoardMailParse(MailInfor info)
	{
		return info.Title;
	}

	#endregion

	public string GetText(MailInfor info){
		if(textFuncDic.ContainsKey(info.Type))
		{
			return textFuncDic[info.Type](info);
		}
		else
		{
			Debug.LogError("邮件类型不存在危险直接返回默认Msg" + "type = " + info.Type + " message = " + info.Message);
			return "error mail content";
		}
	}
}