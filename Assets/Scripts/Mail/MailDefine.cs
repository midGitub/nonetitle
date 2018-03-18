using System.Collections;
using System.Collections.Generic;

public enum MailType{
	Normal = 0,// 普通邮件
	RankReward,// 锦标赛奖金
	System,// 系统邮件
	Max,
}


// 系统邮件类型，根据不同类型，来区分msg里面的自定义参数个数
public enum SystemMailType{
	None = 0,// 无参数
	ProgressRecoveryMail, // 进度恢复
	CompensateMail,// 补偿金
	VipCompensateMail, // vip玩家的补偿金
	SaleOffIAP,// 优惠内购
	Reward,
	BillBoard,
	Max,
}

// 本地邮件类型（本地生成）
public enum LocalMailType{
	UpdateMail,// 更新奖励邮件
	TournamentRewardMail,// not use
	AllLostRecordUserCompensateMail,// not use
	PayUserCompensateMail,// not use
	CheapIAPMail,// not use
	Max,
}

// 邮件确认状态
public enum MailState
{
	WaitingConfirm = 0,// 等待确认
	DoneConfirm,// 已确认
	Readed,// 已读
}

public class MailDefine{
	// vip用户补偿奖励系数
	public static readonly float VipCompensateFactor = 1.5f;
	
	public static string GetMailTypeString(string name){
		if (name.Equals(MailUtility.NormalMail)){
			return "NormalMail";
		}else if (name.Equals(MailUtility.RankRewardMail)){
			return "RankRewardMail";
		}else if (name.Equals(MailUtility.SystemMail)){
			return "SystemMail";
		}else if (name.Equals(MailUtility.RewardMail)){
			return "RewardMail";
		}else if (name.Equals(MailUtility.BillBoardMail)){
			return "BillBoardMail";
		}
		return "None";
	}

	public static string GetSystemMailTypeString(int type){
		if (type == (int)SystemMailType.ProgressRecoveryMail){
			return "ProgressRecoveryMail";
		}else if (type == (int)SystemMailType.CompensateMail){
			return "CompensateMail";
		}else if (type == (int)SystemMailType.VipCompensateMail){
			return "VipCompensateMail";
		}else if (type == (int)SystemMailType.SaleOffIAP){
			return "SaleOffIAP";
		}else if (type == (int)SystemMailType.Reward){
			return "Reward";
		}else if (type == (int)SystemMailType.BillBoard){
			return "BillBoard";
		}
		return "None";
	}
}