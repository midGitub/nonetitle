using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniJSON;
using System.Text;

public enum TournamentServerFunction
{
	GetInfor,
	Report,
	GetReward,
	CheckReward
}

public class TournamentNetworkHelper : NetWorkHelperAbstract<TournamentNetworkHelper>
{

	// 得到信息
	private string _getInfor = "rank/get_info";
	// 上传数据
	private string _report = "rank/report";
	// 领取奖励 服务器状态设置为领取了
	private string _getReward = "rank/get_reward";
	private string _checkReward = "rank/check_reward";
	private float TimeOutTime = 10f;

	public TournamentNetworkHelper() : base()
	{
		//#if UNITY_EDITOR
		//		_baseUrl = "http://192.168.3.231:8063/";
		//#endif
	}

	public override void AddServerFunctionName()
	{
		ServerFunctionName = new Dictionary<string, string>();
		ServerFunctionName.Add(TournamentServerFunction.GetInfor.ToString(), _getInfor);
		ServerFunctionName.Add(TournamentServerFunction.Report.ToString(), _report);
		ServerFunctionName.Add(TournamentServerFunction.GetReward.ToString(), _getReward);
		ServerFunctionName.Add(TournamentServerFunction.CheckReward.ToString(), _checkReward);
	}
}
