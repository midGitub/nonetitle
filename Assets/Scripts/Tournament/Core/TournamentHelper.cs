using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;

public class TournamentInformation
{
	public int Bet;
	/// <summary>
	/// 锦标赛状态 1正在进行 0锦标赛结束发奖励
	/// </summary>
	public int State;
	public float TimeRemain;
	public ulong Pool;
	//public Dictionary<string, int> Reward = new Dictionary<string, int>();
	public TournamentUserInfor SelfRankInfor;

	/// <summary>
	/// 比赛状态中是左右3名的信息,比赛结束是前五名有奖励的用户,然后自己寻找ID是否中奖根据UDID
	/// </summary>
	public Dictionary<string, TournamentUserInfor> RankInfor = new Dictionary<string, TournamentUserInfor>();
}

public class TournamentUserInfor
{
	public string Icon;
	public string UDID;
	public int Rank;
	public ulong Score;
	public int VipPoint;
	public TournamentReward Reward;
}

public class TournamentReward
{
	/// <summary>
	/// 是否已经领取
	/// </summary>
	public bool IsTaken;
	/// <summary>
	/// 奖励的钱数
	/// </summary>
	public ulong Coins;
}

public class RewardInfor
{
	/// <summary>
	/// 范围
	/// </summary>
	public int Bet;

	public int Rank;

	/// <summary>
	/// 奖金
	/// </summary>
	public ulong Coins;
}

public class TournamentHelper : Singleton<TournamentHelper>
{
	private enum FieldName
	{
		bet,
		score,
		state,
		timeRemain,
		reward,
		rankInfo,
		Icon,
		UDID,
		rank,
		VipPoint,
		val,
		winScore,
		isTaken,
		coins,
		pool,
		selfRankInfo,
	}

//	private bool _isReportedResultInThisRound = false;
//	public bool IsReportedResultInThisRound
//	{
//		get { return _isReportedResultInThisRound; }
//		set { _isReportedResultInThisRound = value; }
//	}

	public void GetTournamentInform(int Bet)
	{
		StartCoroutine(GetTournamentInformIE(Bet));
	}

	private IEnumerator GetTournamentInformIE(int Bet)
	{
		Dictionary<string, object> sendDic = new Dictionary<string, object>();
		sendDic.Add(FieldName.bet.ToString(), Bet);
		bool error = false; 
		JSONObject resultJS = null;
		JSONObject errorJS = null;
		yield return StartCoroutine(TournamentNetworkHelper.Instance.NetCall
			(this, TournamentServerFunction.GetInfor.ToString(), sendDic, (r) => { resultJS = r; }, (obj) => { error = true; errorJS = obj; },true));
		if(error)
		{
			string er = "";
			if(errorJS != null)
			{
				er = errorJS.GetField("error").n.ToString();
			}
			// 这里发送错误信息  
			CitrusEventManager.instance.Raise(new TournamentServerErrorEvent(TournamentServerFunction.GetInfor, er));
			yield break;
		}

		TournamentInformation infor = JSONToTInfor(resultJS);

		CitrusEventManager.instance.Raise(new TournamentInforResultEvent(infor));

		//By nichos: comment this part because it doens't work
		//to make sure this event only occurs once in one round
//		if(infor.State == 0)
//		{
//			if(!_isReportedResultInThisRound)
//			{
//				_isReportedResultInThisRound = true;
//				CitrusEventManager.instance.Raise(new TournamentInforResultEvent(infor));
//			}
//			else
//			{
//				Debug.Log("GetTournamentInformIE: Ignore raise event TournamentInforResultEvent");
//			}
//		}
//		else
//		{
//			CitrusEventManager.instance.Raise(new TournamentInforResultEvent(infor));
//		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="bet"></param>
	/// <param name="score">一局5分钟内赢得分数包含这局</param>
	/// <param name="winScore">这次旋转赢得的分数</param>
	public void Report(int bet, ulong score, ulong winScore)
	{
		StartCoroutine(ReportIE(bet, score, winScore));
	}

	private IEnumerator ReportIE(int bet, ulong score, ulong winScore)
	{
		Dictionary<string, object> sendDic = new Dictionary<string, object>();
		sendDic.Add(FieldName.bet.ToString(), bet);
		sendDic.Add(FieldName.score.ToString(), score);
		sendDic.Add(FieldName.Icon.ToString(), FacebookHelper.GetProfilePictureURLWithID(UserIDWithPrefix.GetFBIDNoPrefix()));
		sendDic.Add(FieldName.VipPoint.ToString(), VIPSystem.Instance.GetCurrVIPLevelData.LevelPoint);
		sendDic.Add(FieldName.winScore.ToString(), winScore);
		bool error = false; 
		JSONObject resultJS = null; 
		JSONObject errorJS = null;
		yield return StartCoroutine(TournamentNetworkHelper.Instance.NetCall
		                            (this, TournamentServerFunction.Report.ToString(), sendDic, (r) => { resultJS = r; }, (obj) => { error = true; errorJS = obj; },true));
		if(error)
		{
			string er = "";
			if(errorJS != null)
			{
				er = errorJS.GetField("error").n.ToString();
			}
			// 这里发送错误信息  
			CitrusEventManager.instance.Raise(new TournamentServerErrorEvent(TournamentServerFunction.Report, er));
			yield break;
		}

		// 这里发送 得到信息成功信息
	}

	public void GetReward()
	{
		StartCoroutine(GetRewardIE());
	}

	private IEnumerator GetRewardIE()
	{
		Dictionary<string, object> sendDic = new Dictionary<string, object>();
		bool error = false; 
		JSONObject resultJS = null; 
		JSONObject errorJS = null;
		yield return StartCoroutine(TournamentNetworkHelper.Instance.NetCall
			(this, TournamentServerFunction.GetReward.ToString(), sendDic, (r) => { resultJS = r; }, (obj) => { error = true; errorJS = obj; },true));
		if(error)
		{
			string er = "";
			if(errorJS != null)
			{
				er = errorJS.GetField("error").n.ToString();
			}
			// 这里发送错误信息  
			CitrusEventManager.instance.Raise(new TournamentServerErrorEvent(TournamentServerFunction.GetReward, er));
			yield break;
		}

		// 这里发送 得到信息成功信息
		CitrusEventManager.instance.Raise(new GetRewardResultEvent());
	}

	public void CheckReward()
	{
		StartCoroutine(CheckRewardIE());
	}

	private IEnumerator CheckRewardIE()
	{
		Dictionary<string, object> sendDic = new Dictionary<string, object>();
		bool error = false; 
		JSONObject resultJS = null; 
		JSONObject errorJS = null;
		yield return StartCoroutine(TournamentNetworkHelper.Instance.NetCall
		                            (this, TournamentServerFunction.CheckReward.ToString(), sendDic, (r) => { resultJS = r; }, (obj) => { error = true; errorJS = obj; },true));
		if(error)
		{
			string er = "";
			if(errorJS != null)
			{
				er = errorJS.GetField("error").n.ToString();
			}
			// 这里发送错误信息 
			CitrusEventManager.instance.Raise(new TournamentServerErrorEvent(TournamentServerFunction.CheckReward, er));
			yield break;
		}
		var list = resultJS.GetField(FieldName.reward.ToString()).list;
		// 没有奖励或是领过都是空的
		List<RewardInfor> newlist = new List<RewardInfor>();
		for(int i = 0; i < list.Count; i++)
		{
			var item = list[i];
			newlist.Add(
				new RewardInfor
				{
					Bet = (int)item.GetField(FieldName.bet.ToString()).n,
					Rank = (int)item.GetField(FieldName.rank.ToString()).n,
					Coins = (ulong)item.GetField(FieldName.coins.ToString()).n
				}
			);
		}
		// 这里发送 得到信息成功信息
		CitrusEventManager.instance.Raise(new CheckRewardResultEvent(newlist));
	}

	public TournamentInformation JSONToTInfor(JSONObject js)
	{
		var newInfor = new TournamentInformation();
		newInfor.Bet = (int)js.GetField(FieldName.bet.ToString()).n;
		newInfor.State = (int)js.GetField(FieldName.state.ToString()).n;
		newInfor.TimeRemain = (float)js.GetField(FieldName.timeRemain.ToString()).n;
		newInfor.Pool = (ulong)js.GetField(FieldName.pool.ToString()).n;

		//for(int i = 0; i < rewlist.Count; i++)
		//{
		//	newInfor.Reward.Add(i.ToString(), (int)rewlist[i].GetField(FieldName.val.ToString()).n);
		//}

		var rankinfolist = js.GetField(FieldName.rankInfo.ToString()).list;

		for(int i = 0; i < rankinfolist.Count; i++)
		{
			var currjs = rankinfolist[i];

			var newinfor = JsToTournamentUserInfor(currjs);
			newInfor.RankInfor.Add(currjs.GetField(FieldName.UDID.ToString()).str, newinfor);
		}

		if(js.HasField(FieldName.selfRankInfo.ToString()))
		{
			var selfjs = js.GetField(FieldName.selfRankInfo.ToString());
			newInfor.SelfRankInfor = JsToTournamentUserInfor(selfjs);
		}

		return newInfor;
	}

	private TournamentUserInfor JsToTournamentUserInfor(JSONObject currjs)
	{
		var ti = new TournamentUserInfor
		{
			Icon = currjs.GetField(FieldName.Icon.ToString()).str,
			UDID = currjs.GetField(FieldName.UDID.ToString()).str,
			Rank = (int)currjs.GetField(FieldName.rank.ToString()).n,
			Score = (ulong)currjs.GetField(FieldName.score.ToString()).n,
			VipPoint = (int)currjs.GetField(FieldName.VipPoint.ToString()).n,
		};

		if(currjs.HasField(FieldName.reward.ToString()))
		{
			var rewjs = currjs.GetField(FieldName.reward.ToString());
			ti.Reward = new TournamentReward
			{
				//IsTaken = rewjs.GetField(FieldName.isTaken.ToString()).b,
				Coins = (ulong)rewjs.GetField(FieldName.val.ToString()).n
			};
		}
		else
		{
			ti.Reward = new TournamentReward();
		}

		return ti;
	}



}
