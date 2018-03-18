using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;
using System;

public class TournamentTimeHelper : SimpleSingleton<TournamentTimeHelper>
{
	private int _maxTimeSpan = 15;
	private int _baseTime = 6;// 锦标赛时间间隔

	private Coroutine _timeUpdateIE;
	private Coroutine _changeBetIE;

	/// <summary>
	/// 如果一直不获胜自动刷新数据间隔
	/// </summary>
//	public int UpdateGetTournamentInforTime = 10;
	public int ChangeBetCoolDownTime = 1;

	/// <summary>
	/// 锦标赛剩余的时间,doing锦标赛进行 doingfalse 锦标赛结束 领奖状态
	/// </summary>
	/// <returns>The ctime.</returns>
	/// <param name="doing">Doing.</param>
	public int RemainderCtime()
	{
		int CLastTimeSecond;
		var serverTime = NetworkTimeHelper.Instance.GetNowTime();
		CLastTimeSecond = (serverTime.Minute % _baseTime) * 60 + serverTime.Second;
		CLastTimeSecond = (_baseTime - 1) * 60 - CLastTimeSecond;
		if(CLastTimeSecond < 0)
		{
			CLastTimeSecond += 60;
		}
		return CLastTimeSecond;
	}

	public int RemainderCtime(out bool doing)
	{
		int CLastTimeSecond;
		var serverTime = NetworkTimeHelper.Instance.GetNowTime();
		CLastTimeSecond = (serverTime.Minute % _baseTime) * 60 + serverTime.Second;
		CLastTimeSecond = (_baseTime - 1) * 60 - CLastTimeSecond;
		if(CLastTimeSecond < 0)
		{
			doing = false;
			CLastTimeSecond += 60;
		}
		else
		{
			doing = true;

		}
		return CLastTimeSecond;
	}

	public void CTimeThanSTime(float serverLastTime)
	{

		if(Mathf.Abs(RemainderCtime() - serverLastTime) > _maxTimeSpan)
		{
			// 更新服务器时间
			NetworkTimeHelper.Instance.UpdateServerTime((a) => { });
		}
		//Debug.Log("客户端时间" + NetworkTimeHelper.Instance.GetNowTime());
		//Debug.Log("服务器和客户端时间差值" + (Mathf.Abs(RemainderCtime() - serverLastTime)));
	}

	/// <summary>
	/// 开始自动计时调用刷新如果重复调用 就会停止上一次的调用
	/// </summary>
	/// <param name="helper">Helper.</param>
	/// <param name="timeDone">Time done.</param>
	/*public void StartAutoGetTournamentInforInfor(MonoBehaviour helper, Action timeDone)
	{
		if(_timeUpdateIE != null)
		{
			helper.StopCoroutine(_timeUpdateIE);
		}
		_timeUpdateIE = helper.StartCoroutine(WaitTimeDo(timeDone, UpdateGetTournamentInforTime));
	}
*/
	public void StartAutoGetTournamentInforInfor(MonoBehaviour helper,int waitTime, Action timeDone)
	{
		if(_timeUpdateIE != null)
		{
			helper.StopCoroutine(_timeUpdateIE);
		}
		_timeUpdateIE = helper.StartCoroutine(WaitTimeDo(timeDone, waitTime));
	}

	public void StopAutoAutoGetTournamentInforInfor(MonoBehaviour helper)
	{
		if(_timeUpdateIE != null)
		{
			helper.StopCoroutine(_timeUpdateIE);
			_timeUpdateIE = null;
		}
	}

	public void ChangeBetCoolDown(MonoBehaviour helper, Action timeDone)
	{
		if(_changeBetIE != null)
		{
			helper.StopCoroutine(_changeBetIE);
		}
		_changeBetIE = helper.StartCoroutine(WaitTimeDo(timeDone, ChangeBetCoolDownTime));
	}

	private IEnumerator WaitTimeDo(Action timeDone, float waitTime)
	{
		yield return new WaitForSeconds(waitTime);
		timeDone();
	}
}
