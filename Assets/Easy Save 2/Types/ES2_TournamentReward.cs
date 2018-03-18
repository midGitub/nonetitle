using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MiniJSON;

public class ES2_TournamentReward : ES2Type
{

	public ES2_TournamentReward() : base(typeof(TournamentLastRewardInfor))
	{
	}

	public override object Read(ES2Reader reader)
	{
		return Json2TouranmentReward(new JSONObject(reader.Read<System.String>()));
	}


	public override void Write(object data, ES2Writer writer)
	{
		TournamentLastRewardInfor ti = (TournamentLastRewardInfor)data;
		writer.Write(TouranmentReward2Json(ti));
	}

	private string TouranmentReward2Json(TournamentLastRewardInfor trinfor)
	{
		Dictionary<string, object> baseDic = new Dictionary<string, object>();
		baseDic.Add("LastWirteTime", trinfor.LastWirteTime);
		baseDic.Add("Getted", trinfor.Getted);
		baseDic.Add("Bet", trinfor.RewardInformation.Bet);
		baseDic.Add("Rank", trinfor.RewardInformation.Rank);
		baseDic.Add("Coins", trinfor.RewardInformation.Coins);
		return Json.Serialize(baseDic);
	}

	private TournamentLastRewardInfor Json2TouranmentReward(JSONObject jsob)
	{
		TournamentLastRewardInfor tl = new TournamentLastRewardInfor
		{
			LastWirteTime = Convert.ToDateTime(jsob.GetField("LastWirteTime").str),
			Getted = jsob.GetField("Getted").b,
			RewardInformation = new RewardInfor
			{
				Bet = (int)jsob.GetField("Bet").n,
				Rank = (int)jsob.GetField("Rank").n,
				Coins = (ulong)jsob.GetField("Coins").n
			}
		};

		return tl;
	}
}

public class TournamentLastRewardInfor
{
	/// <summary>
	/// 上次写入的时间
	/// </summary>
	public DateTime LastWirteTime;
	/// <summary>
	/// 是否已经得到,如果得到过就不再获取这条信息
	/// </summary>
	public bool Getted = true;
	public RewardInfor RewardInformation;
}
