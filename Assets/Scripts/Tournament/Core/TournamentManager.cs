using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;

public enum TournamentMatchSection
{
	Small = 1,
	Normal,
	Big,
}

public class TournamentManager : MonoBehaviour
{
	private const int _smallBitNum = 500;
	private const int _normalBitNum = 5000;
	private const float _reportGetInforWaitTime = 0.2f;
	public int UpdateGetTournamentInforTime = 10;
	public static int DelayTime = 0;
	public TournamentUIController _uiController;
	private TournamentMatchSection _currMatchSention;
	private int _currBet;
	public int CurrBet { get { return _currBet; } }

	private bool _isAcceptPrizeState = false;
	private bool _isStart = false;
	public static bool _hasEnd = false;
	private bool _trigger = true;
/*
	private void Update()
	{
		TournamentTimeHelper.Instance.RemainderCtime((doing) => { _isAcceptPrizeState = !doing; });
		if(_isAcceptPrizeState)
		{
			if(_trigger)
			{
				_trigger = false;
				_uiController.ShowLoading();
				GetInfor(_currBet);
			}
		}
		else
		{
			if(!_trigger)
			{
				_trigger = true;
				if(_isStart)
				{
					GetInfor(_currBet);
					_uiController.CreatFalseDataAndShow();
					Debug.Log("Start next tournament");

//					TournamentHelper.Instance.IsReportedResultInThisRound = false;
				}

			}
		}
		//Debug.Log(_isAcceptPrizeState + "" + TournamentTimeHelper.Instance.GetCtime());
	}
*/
	private void Awake()
	{
		CitrusEventManager.instance.AddListener<CallTournamentServerEvent>(CallTournamentMessageP);
		CitrusEventManager.instance.AddListener<TournamentServerErrorEvent>(ServerErrorMessageP);
		CitrusEventManager.instance.AddListener<TournamentInforResultEvent>(TournamentInforResultMessageP);
		CitrusEventManager.instance.AddListener<TournamentReportScoreSuccessEvent>(ReportScoreSuccess);
	}

	private void OnDestroy()
	{
		CitrusEventManager.instance.RemoveListener<CallTournamentServerEvent>(CallTournamentMessageP);
		CitrusEventManager.instance.RemoveListener<TournamentServerErrorEvent>(ServerErrorMessageP);
		CitrusEventManager.instance.RemoveListener<TournamentInforResultEvent>(TournamentInforResultMessageP);
		CitrusEventManager.instance.RemoveListener<TournamentReportScoreSuccessEvent>(ReportScoreSuccess);
	}


	private void ServerErrorMessageP(TournamentServerErrorEvent tsem)
	{
		Debug.Log(tsem.Error + tsem.ErrorFunction + "锦标数据错误");//有状态错误
	}

	private void CallTournamentMessageP(CallTournamentServerEvent ctsm)
	{
		switch(ctsm.FunctionName)
		{
			case TournamentServerFunction.CheckReward:
				TournamentHelper.Instance.CheckReward();
				break;
			case TournamentServerFunction.GetInfor:
				TournamentHelper.Instance.GetTournamentInform(ctsm.Bet);
				break;
			case TournamentServerFunction.GetReward:
				TournamentHelper.Instance.GetReward();
				break;
			case TournamentServerFunction.Report:
				TournamentHelper.Instance.Report(ctsm.Bet, ctsm.AllScore, ctsm.WinScore);
				break;
			default:
				break;
		}
	}

	private void TournamentInforResultMessageP(TournamentInforResultEvent trm)
	{
		//var infor = trm.Infor;
		//LogUtility.Log(infor.Bet + "," + infor.State + "," + infor.TimeRemain, Color.cyan);
		TournamentTimeHelper.Instance.CTimeThanSTime(trm.Infor.TimeRemain);


		//foreach(var item in infor.RankInfor)
		//{
		//	LogUtility.Log(item.Value.Score + "," + item.Value.Rank + "," + item.Value.UDID, Color.cyan);
		//}
	}

	private void ReportScoreSuccess(TournamentReportScoreSuccessEvent repms)
	{
		//GetInfor(_currBet);
	}

	/// <summary>
	/// 开始锦标赛并且检查奖励状态
	/// </summary>
	/// <param name="StartBet">Start bet.</param>
	public void StartTournament(int StartBet)
	{
		// 刚开始先段位请求分数
		GetInfor(StartBet);
		_currMatchSention = BetToSection(StartBet);
		_currBet = StartBet;
		_isStart = true;
		// 检查奖励
		// TournamentHelper.Instance.CheckReward();
	}

	/// <summary>
	///  刷新Infor 
	/// </summary>
	public void UpdateInfor()
	{
		GetInfor(_currBet);
	}

	public void ChangeBet(int bet)
	{
		var newState = BetToSection(bet);
		if(newState != _currMatchSention)
		{
			TournamentTimeHelper.Instance.ChangeBetCoolDown(this, () => {
				GetInfor(bet);
			});
			_currMatchSention = newState;
		}
		_currBet = bet;
	}

	public void Report(ulong winScore)
	{
/*
		if(_isAcceptPrizeState)
		{
			Debug.Log("领奖状态不上传");
			return;
		}
*/
		bool doing = false;
		TournamentTimeHelper.Instance.RemainderCtime(out doing);
		if (!doing)
		{
			Debug.Log("领奖状态不上传");
			return;
		}
			
		var state = BetToSection(_currBet);
		var dic = UserDeviceLocalData.Instance.TournamentScoreInforDic;
		if(dic.ContainsKey(state))
		{
			var infor = dic[state];
			var times = NetworkTimeHelper.Instance.GetNowTime() - infor.WriteTime;
			if(Mathf.Abs((float)times.TotalSeconds) > infor.LastTime)
			{
				// 数据不是次阶段的 重新刷新
				dic[state].AllScore = winScore;
				AnalysisManager.Instance.TournamentWinToRank(GameScene.Instance.PuzzleMachine.MachineName, ((int)state).ToString(),
															 (int)TournamentTimeHelper.Instance.RemainderCtime());

			}
			else { dic[state].AllScore = dic[state].AllScore + winScore; }
			dic[state].LastTime = TournamentTimeHelper.Instance.RemainderCtime();
			dic[state].WriteTime = NetworkTimeHelper.Instance.GetNowTime();

		}
		else
		{
			dic[state] = new TournamentScoreInfor
			{
				AllScore = winScore,
				LastTime = TournamentTimeHelper.Instance.RemainderCtime(),
				WriteTime = NetworkTimeHelper.Instance.GetNowTime()
			};

			AnalysisManager.Instance.TournamentWinToRank(GameScene.Instance.PuzzleMachine.MachineName, ((int)state).ToString(),
															 (int)TournamentTimeHelper.Instance.RemainderCtime());
		}

		CitrusEventManager.instance.Raise(new CallTournamentServerEvent
										  (TournamentServerFunction.Report, _currBet, dic[state].AllScore, winScore));
		//Debug.Log("总分" + dic[state].AllScore + "这句的分数" + winScore);
		// 同步刷新数据 延后0.5秒个时间在发送得到信息让添加信息先处理再得到排名信息
		CitrusFramework.UnityTimer.Instance.StartTimer
					   (this, _reportGetInforWaitTime, UpdateInfor);
		_hasEnd = false;
	}

	private void GetInfor(int bet)
	{
		bool doing = true;
		int remainTime = TournamentTimeHelper.Instance.RemainderCtime(out doing);

		//锦标赛正在进行
		if(doing)
		{
			// 更新数据
			CitrusEventManager.instance.Raise(new CallTournamentServerEvent(TournamentServerFunction.GetInfor, bet));
			TournamentTimeHelper.Instance.StartAutoGetTournamentInforInfor(this,UpdateGetTournamentInforTime+DelayTime,() => { GetInfor(_currBet); });
		}
		else
		{
			// 领奖状态
			if (!_hasEnd)
			{
				TournamentTimeHelper.Instance.StartAutoGetTournamentInforInfor(this,UpdateGetTournamentInforTime+DelayTime,() => { GetInfor(_currBet); });
				CitrusEventManager.instance.Raise(new CallTournamentServerEvent (TournamentServerFunction.GetInfor, bet));

			}
			else
			{
				TournamentTimeHelper.Instance.StopAutoAutoGetTournamentInforInfor(this);
				StartAnotherTournament(remainTime);
			}

		}
	}

	private void StartAnotherTournament(int remainTime)
	{
		UnityTimer.Start(this, remainTime, () =>
		{
			_uiController.CreatFalseDataAndShow();
		});
		remainTime += Random.Range(0, 5); //To Reduce the server burden when tournament start
		TournamentTimeHelper.Instance.StartAutoGetTournamentInforInfor(this, remainTime, () =>
		{
			GetInfor(_currBet);
		});
	}

	public static TournamentMatchSection BetToSection(int bet)
	{
		if(bet <= _smallBitNum)
		{
			return TournamentMatchSection.Small;
		}
		else if(bet <= _normalBitNum)
		{
			return TournamentMatchSection.Normal;
		}
		else
		{
			return TournamentMatchSection.Big;
		}
	}

}
