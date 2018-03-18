using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 本地邮件工厂， 用来产生本地的各种邮件 (似乎现在并不需要）
/// </summary>
public class LocalMailFactory {
	// 邮箱内容key
	private static readonly string[] _mailKeys = new string[]{ 
		"sys_update", "game_tournament", "lost_record_user_compensate", "pay_user_compensate", "cheap_iap"
	};

	// 邮箱key生成器
	private static readonly int mailKey_min = 0;
	private static readonly int mailKey_max = 1000;

	public static void CreateMail(LocalMailType type){
		string key = "";
		Debug.Assert ((int)type < _mailKeys.Length, "create mail index out of array");
		key = _mailKeys[(int)type];

		if (type == LocalMailType.UpdateMail) {
			CreateUpdateMail (key);
		} else if (type == LocalMailType.TournamentRewardMail) {
			CreateTournamentRewardMail (key);
		} else if (type == LocalMailType.AllLostRecordUserCompensateMail) {
			CreateAllLostRecordUserCompensateMail (key);
		} else if (type == LocalMailType.PayUserCompensateMail) {
			CreatePayUserCompensateMail (key);
		} else if (type == LocalMailType.CheapIAPMail) {
			CreateCheapIAPMail (key);
		}
	}

	private static void CreateUpdateMail(string key){
		var maildata = MailTextConfig.Instance.TryGetDataWithKey(key);
		MailInfor mf = new MailInfor
		{
			Message = maildata.Val,
			State = MailState.DoneConfirm,
			Type = MailUtility.NormalMail,
			Bonus = maildata.Bonus,
		};

		string keyrand = key + UnityEngine.Random.Range(mailKey_min, mailKey_max);
		UserBasicData.Instance.MailInforDic[keyrand] = mf;
		UserBasicData.Instance.Save();
	}

	private static void CreateTournamentRewardMail(string key){
		// not use
	}

	private static void CreateAllLostRecordUserCompensateMail(string key){
		// not use
	}

	private static void CreatePayUserCompensateMail(string key){
		// not use
	}

	private static void CreateCheapIAPMail(string key){
		// not use
	}
}


/// <summary>
/// 创建本地邮件的管理器
/// </summary>
public class CreatMailSelf: SimpleSingleton<CreatMailSelf>
{
	private string updateKey = "sys_update";
	private int mailKeyRandom = 1000;
    private string _christmasMaxWin= "activity_christmas_max_win";
    /// <summary>
    /// 创建更新奖励邮件
    /// </summary>
    public void CreatUpdateMail()
	{
		var maildata = MailTextConfig.Instance.TryGetDataWithKey(updateKey);
		MailInfor mf = new MailInfor
		{
			Message = maildata.Val,
			State = MailState.DoneConfirm,
			Type = "0",
			Bonus = maildata.Bonus,
		};

		string keyrand = updateKey + UnityEngine.Random.Range(0, mailKeyRandom);
		UserBasicData.Instance.MailInforDic[keyrand] = mf;
		UserBasicData.Instance.Save();
	}

    public void CreateChristmasMaxWinRewardMail()
    {
        MailTextData maildata = MailTextConfig.Instance.TryGetDataWithKey(_christmasMaxWin);
        MailInfor mf = new MailInfor
        {
            Message = maildata.Val,
            State = MailState.DoneConfirm,
            Type = "0",
            Bonus = (int)UserBasicData.Instance.MaxWinDuringActivity
        };

        string key = _christmasMaxWin + NetworkTimeHelper.Instance.GetNowTime();
        UserBasicData.Instance.MailInforDic[key] = mf;
        UserBasicData.Instance.Save();
    }
}
