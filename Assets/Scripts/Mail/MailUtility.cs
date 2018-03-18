using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;

// TODO: 这里有一部分应该可以用多态来实现重构，而不用if-else，不过似乎邮件类型也不会用到特别多的种类，所以暂时用if-else替代也可以

/// <summary>
/// 邮件解析
/// </summary>
public class MailUtility{
	// 邮件type对应字符串
	public static readonly string NormalMail = "0";
	public static readonly string RankRewardMail = "1";
	public static readonly string SystemMail = "2";
	public static readonly string RewardMail = "3";
	public static readonly string BillBoardMail = "4";

	// 系统邮件msg标识符
	public static readonly string _system_msg = "sys_msg";//这里如果用msg的话，会存在一个“Msg”字段套用一个带"msg"的字段的情况，这样发送邮件时候似乎会有点问题（字符串不带反斜杠），所以需要一个不同名的字段
	public static readonly string _system_type = "sys_type";
	public static readonly string _system_credits = "credits";
	public static readonly string _system_level = "lv";
	public static readonly string _system_vip_level = "viplv";
	public static readonly string _system_exp = "exp";
	public static readonly string _system_vip_exp = "vipexp";
	public static readonly string _system_long_lucky = "ltlucky";
	public static readonly string _system_piggybank_credits = "pb_credits";// piggy bank
	public static readonly string _system_totalspin_count = "totalspin";
	public static readonly string _system_priority = "priority";

	#region message tag
	// 这些代码是林立建议的。
	// 这里写成可扩展的，一个类型，一个参数个数之类的，但是似乎暂时还用不到。

	// 记录每个标签下元素个数
	public static readonly List<MailMessageTag> MailMsgTagList = new List<MailMessageTag>(){
		new MailMessageTag(_system_msg, 1),
		new MailMessageTag(_system_type, 1),
		new MailMessageTag(_system_credits, 1),
		new MailMessageTag(_system_level, 1),
		new MailMessageTag(_system_vip_level, 1),
		new MailMessageTag(_system_exp, 1),
		new MailMessageTag(_system_vip_exp, 1),
		new MailMessageTag(_system_long_lucky, 1), 
		new MailMessageTag(_system_piggybank_credits, 1),
		new MailMessageTag(_system_totalspin_count, 1),
		new MailMessageTag(_system_priority, 1),
	};

	// 获得类型传送的参数个数
	public static int GetTypeParamNumber(string tag){
		MailMessageTag info = ListUtility.FindFirstOrDefault (MailMsgTagList, (MailMessageTag mailTag) => {
			return tag.Equals(mailTag.TagName);
		});

		return info.Num;
	}

	#endregion

	// not use
	public static readonly Dictionary<string, System.Type> MailTag2TypeDict = new Dictionary<string, System.Type>(){
		{_system_msg, typeof(string)},
		{_system_type, typeof(SystemMailType)},
		{_system_credits, typeof(ulong)},
		{_system_level, typeof(int)},
		{_system_vip_level, typeof(int)},
		{_system_exp, typeof(int)},
		{_system_vip_exp, typeof(int)},
		{_system_long_lucky, typeof(int)},
		{_system_piggybank_credits, typeof(ulong)},
		{_system_totalspin_count, typeof(int)},
		{_system_priority, typeof(int)},
	};

	// 字符串  => 邮件类型
	public static Dictionary<string, MailType> MailTypeDict = new Dictionary<string, MailType>{ 
		{NormalMail, MailType.Normal},
		{RankRewardMail, MailType.RankReward},
		{SystemMail, MailType.System},
	};

	// 获得邮件类型
	public static MailType GetMailType(string str){
		if (MailTypeDict.ContainsKey (str)) {
			return MailTypeDict [str];
		}

		return MailType.Normal;
	}

	// mailinfo - > mailinforExtension
	public static MailInforExtension MailInfor2MailInforExtension(MailInfor info){
		return String2MailInforExtension(info.Message);
	}

	// message string - > mailInforExtension
	public static MailInforExtension String2MailInforExtension(string str){
		JSONObject json = new JSONObject (str);
		MailInforExtension extension = new MailInforExtension ();
		AssignMailInforExtension (json, extension);

		return extension;
	}
		
	// mail json - > mailInforExtension
	public static MailInforExtension ServerJson2MailInforExtension(JSONObject json){
		MailInforExtension extension = null;

		if (json.HasField ("Type")) {
			MailType type = (MailType)json.GetField ("Type").n;
			// 只有系统邮件才会进行extension的处理
			if (type == MailType.System) {
				extension = new MailInforExtension ();
				if (json.HasField ("Msg")) {
					string str = json.GetField ("Msg").str;
					JSONObject obj = new JSONObject (str);
					AssignMailInforExtension (obj, extension);
				}
			}	
		}
		return extension;
	}

	// mail info - > message
	public static string MailInfor2Message(MailInfor info){
		MailInforExtension extension = MailInfor2MailInforExtension (info);
		return MailInfor2Message (info, extension);
	}

	// mail info + extension - > message
	private static string MailInfor2Message(MailInfor info, MailInforExtension extension){
		string str = MailTextHelper.Instance.GetText (info);
		if (extension == null) {
			return str;
		} else {
			return MailInforExtension2Message (str, extension);
		}
	}

	// 获得邮件奖励
	public static void GetMailReward(MailInfor info, MailInforExtension extension){
		if (info.Bonus != 0) {
			UserBasicData.Instance.AddCredits ((ulong)info.Bonus, FreeCreditsSource.ReadMailBoxBonus, true);	
		} else if (extension != null) {
			GetMailExtensionReward (extension);
		}
	}

	private static void GetMailExtensionReward(MailInforExtension extension){
		if (extension.SystemType != SystemMailType.None) {
			
			#if false // 不能用这种方式去处理，因为有时是恢复的进度，有时是增加的补偿，需要区分set和add的区别
			if (extension.Credits > 0) {
			}
			if (extension.Exp > 0) {
			}
			if (extension.Level > 0) {
			}
			if (extension.VipExp > 0) {
			}
			if (extension.VipLevel > 0) {
			}
			if (extension.LongLucky > 0) {
			}
			if (extension.PiggyBankCredits > 0) {
			}
			if (extension.TotalSpinCount > 0) {
			}
			#endif

			if (extension.SystemType == SystemMailType.ProgressRecoveryMail)
			{
				// 玩家等级
				if (extension.Level >= UserBasicData.Instance.UserLevel.Level)
				{
					LevelData data = LevelData.CreateData(extension.Level, extension.Exp);
					UserBasicData.Instance.SetUserLevelData(data, false);	
					UserLevelSystem.Instance.UpdateUserLevelData();
				}
				// 玩家ltlucky
				UserBasicData.Instance.SetLongLucky(extension.LongLucky, false);
				#if false
				// 玩家VIP点数的设置
				int vipLevel = VIPConfig.Instance.GetPointAboutVIPLevel (extension.VipExp);
				LevelData vipData = LevelData.CreateData (vipLevel, extension.VipExp);
				VIPSystem.Instance.UpdateLevelData (vipData);
				UserBasicData.Instance.SetVIPPoint (extension.VipExp, false);
				#else
				// 玩家vip点数的增加
				VIPSystem.Instance.AddVIPPoint(extension.VipExp, false);
				#endif
				// 玩家筹码
				UserBasicData.Instance.AddCredits(extension.Credits, FreeCreditsSource.ReadMailBoxBonus, true);
				// 因为更新了玩家等级信息，所以需要发送一下这个事件
				CitrusEventManager.instance.Raise(new UserDataLoadEvent ());
			}
			else if (extension.SystemType == SystemMailType.CompensateMail)
			{
				UserBasicData.Instance.AddCredits(extension.Credits, FreeCreditsSource.ReadMailBoxBonus, false);
				UserBasicData.Instance.AddLongLucky(extension.LongLucky, true);
			}
			else if (extension.SystemType == SystemMailType.VipCompensateMail)
			{
				ulong credits = (ulong)(MailDefine.VipCompensateFactor * (float)extension.Credits);
				UserBasicData.Instance.AddCredits(credits, FreeCreditsSource.ReadMailBoxBonus, false);
				// 玩家vip点数的增加
				VIPSystem.Instance.AddVIPPoint(extension.VipExp, false);
				UserBasicData.Instance.AddLongLucky(extension.LongLucky, true);
				// 因为更新了玩家等级信息，所以需要发送一下这个事件
				CitrusEventManager.instance.Raise(new UserDataLoadEvent ());
			}
			else if (extension.SystemType == SystemMailType.SaleOffIAP)
			{
				// TODO:暂时不做
			}
			else if (extension.SystemType == SystemMailType.Reward || extension.SystemType == SystemMailType.BillBoard)
			{
				UserBasicData.Instance.AddCredits(extension.Credits, FreeCreditsSource.ReadMailBoxBonus, false);
				UserBasicData.Instance.AddLongLucky(extension.LongLucky,false);
				VIPSystem.Instance.AddVIPPoint(extension.VipExp);
				LevelData data = LevelData.CreateData(extension.Level, extension.Exp);
				data.Level += UserBasicData.Instance.UserLevel.Level;
				data.LevelPoint += UserBasicData.Instance.UserLevel.LevelPoint;
				UserBasicData.Instance.SetUserLevelData(data, false);
				UserLevelSystem.Instance.UpdateUserLevelData();
				CitrusEventManager.instance.Raise(new UserDataLoadEvent ());
			}
		}
	}

	// 获得提示性奖励文字
	public static string GetMailBonusText(MailInfor info, MailInforExtension extension){
		if (info.Bonus != 0) {
			return StringUtility.FormatNumberStringWithComma ((ulong)info.Bonus);
		} else if (extension != null) {
			return GetMailBonusExtensionText (extension);
		}
		return "";
	}

	// 金币提示性奖励文字
	private static string GetMailBonusExtensionText(MailInforExtension extension){
		if (extension.SystemType != SystemMailType.None) {
			if (extension.SystemType == SystemMailType.ProgressRecoveryMail)
			{
				string credits = StringUtility.FormatNumberStringWithComma(extension.Credits);
//				string vipexp = StringUtility.FormatNumberStringWithComma ((ulong)extension.VipExp);
//				string level = extension.Level.ToString ();
				return " " + credits; 
			}
			else if (extension.SystemType == SystemMailType.CompensateMail)
			{
				string credits = StringUtility.FormatNumberStringWithComma(extension.Credits);
				return " " + credits;
			}
			else if (extension.SystemType == SystemMailType.VipCompensateMail)
			{
				ulong credit = (ulong)(MailDefine.VipCompensateFactor * (float)extension.Credits);
				string credits = StringUtility.FormatNumberStringWithComma(credit);
				return " " + credits;
			}
			else if (extension.SystemType == SystemMailType.SaleOffIAP)
			{
				return "";
			}
			else if (extension.SystemType == SystemMailType.Reward || extension.SystemType == SystemMailType.BillBoard)
			{
				string result = " ";
				if (extension.Credits > 0)
				{
					result += StringUtility.FormatNumberStringWithComma(extension.Credits);
				}
				/*
				if (extension.Exp > 0)
				{
					result += " + " + StringUtility.FormatNumberStringWithComma((ulong)extension.Exp) + "EXP";
				}
				if (extension.VipExp > 0)
				{
					result += " + " + StringUtility.FormatNumberStringWithComma((ulong)extension.VipExp) + "VIP";
				}
				*/
				return result;
			}
		}
		return "";
	}

	// mailinfo  extension = > message
	private static string MailInforExtension2Message(string str, MailInforExtension extension){
		string result = str;
		try{
			if (extension.SystemType == SystemMailType.None) {
				return result;
			} else if (extension.SystemType == SystemMailType.ProgressRecoveryMail) {
				return string.Format (result, CombineColorTag(extension.Credits), CombineColorTag(extension.Level), CombineColorTag(extension.VipExp));;
			} else if (extension.SystemType == SystemMailType.CompensateMail) {
				return string.Format (result, CombineColorTag(extension.Credits));
			} else if (extension.SystemType == SystemMailType.VipCompensateMail) {
				return string.Format (result, CombineColorTag(extension.Credits), CombineColorTag(extension.VipExp));
			} else if (extension.SystemType == SystemMailType.SaleOffIAP) {
				return result;
			} else if (extension.SystemType == SystemMailType.Reward || extension.SystemType == SystemMailType.BillBoard)
			{
				return result;
			}
			else {
				Debug.Assert (false, "mailinfoextension systemtype fail " + extension.SystemType);
			}	
		}catch(System.Exception e){
			Debug.LogError ("MailinfoExtension2Message catch "+e.Message);	
		}

		return result;
	}

	private static string CombineColorTag(object obj){
		return "<color=#f5da24>" + obj.ToString () + "</color>";
	}

	// mailinfor extension赋值
	public static void AssignMailInforExtension(JSONObject json, MailInforExtension extension){
		if (json.HasField (_system_msg)) {
			extension.Msg = json.GetField (_system_msg).str;
		}
		if (json.HasField (_system_type)) {
			extension.SystemType = (SystemMailType)json.GetField (_system_type).n;
		}
		if (json.HasField (_system_credits)) {
			extension.Credits = (ulong)json.GetField (_system_credits).n;
		}
		if (json.HasField (_system_level)) {
			extension.Level = (int)json.GetField (_system_level).n;
		}
		if (json.HasField (_system_vip_level)) {
			extension.VipLevel = (int)json.GetField (_system_vip_level).n;
		}
		if (json.HasField (_system_exp)) {
			extension.Exp = (int)json.GetField (_system_exp).n;
		}
		if (json.HasField (_system_vip_exp)) {
			extension.VipExp = (int)json.GetField (_system_vip_exp).n;
		}
		if (json.HasField (_system_long_lucky)) {
			extension.LongLucky = (int)json.GetField (_system_long_lucky).n;
		}
		if (json.HasField (_system_piggybank_credits)) {
			extension.PiggyBankCredits = (ulong)json.GetField (_system_piggybank_credits).n;
		}
		if (json.HasField (_system_totalspin_count)) {
			extension.TotalSpinCount = (int)json.GetField (_system_totalspin_count).n;
		}
		if (json.HasField (_system_priority)) {
			extension.Priority = (int)json.GetField (_system_priority).n;
		}
	}
		
	public static bool HasBonus(MailInfor info){
		if (info == null)
			return false;

		MailInforExtension extension = MailInfor2MailInforExtension (info);
		bool hasBonus = info.Bonus > 0 | !info.Title.IsNullOrEmpty();
		if (extension != null) {
			hasBonus = hasBonus | extension.Credits > 0
			| extension.Exp > 0 | extension.Level > 0
			| extension.VipExp > 0 | extension.VipLevel > 0 | extension.LongLucky > 0
			| extension.PiggyBankCredits > 0 | extension.TotalSpinCount > 0;
		}
		return hasBonus;
	}
}
