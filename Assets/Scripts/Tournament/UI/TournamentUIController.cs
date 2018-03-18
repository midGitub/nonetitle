using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TournamentChangeSoundEvent : CitrusGameEvent 
{
	public bool SoundOn;
	public TournamentChangeSoundEvent(bool soundOn):base()
	{
		SoundOn = soundOn;
	}
}

public class TournamentUIController : MonoBehaviour
{
	public TournamentManager _tournamentManager;
	public GameObject NormalGameObject;
	public GameObject LoadingGameObject;
	public GameObject ErrorGameObject;
	public CountTimeUI TimeGameObject;
	public GameObject TimeUpGameObject;
	public GameObject ItemGroup;
	public GameObject ItemPool;

	public GameObject PigPoolPrefab;
	public GameObject OtherItemPrefab;
	public GameObject SelfItemPrefab;

	public static bool ShouldPlaySound = true;
	private int _showlastRewardStateTime = 10;
	private int _playsound5Time = 5;
	private int _timerLast75Sec = 75;
	private int _timerLast10Sec = 10;
	private ulong _defultPool = 0;
	private int _poolLength = 5;
	private int _doinglength = 3;
	private int _donelength = 4;
	private int _notInBoardDelayTime = 2;
	private int _notHasScoreDelayTime = 5;
	private CoinsPoolItem _pigPool;
	private TournamentItem _selfItem;
	private SelfTournamenItemEffect _selfEffect;
	private List<TournamentItem> _otherItem = new List<TournamentItem>();

	private Coroutine _normalTimerCoroutine;

	[SerializeField]
	private float _singleLoopTime;
	[SerializeField]
	private Vector3 _flickerLargerScale;
	[SerializeField]
	private Vector3 _flickerSmallScale;
	[SerializeField]
	private float _loopTimes;
	//private static Dictionary<TournamentMatchSection, TournamentUserInfor> _lastTournamentInfor = new Dictionary<TournamentMatchSection, TournamentUserInfor>();

	private void Awake()
	{
		CitrusEventManager.instance.AddListener<TournamentServerErrorEvent>(ServerErrorMessageP);
		CitrusEventManager.instance.AddListener<TournamentInforResultEvent>(TournamentInforResultMessageP);
		CitrusEventManager.instance.AddListener<TournamentChangeSoundEvent>(ChangeSound);
		Init();
	}

	private void OnDestroy()
	{
		CitrusEventManager.instance.RemoveListener<TournamentServerErrorEvent>(ServerErrorMessageP);
		CitrusEventManager.instance.RemoveListener<TournamentInforResultEvent>(TournamentInforResultMessageP);
		CitrusEventManager.instance.RemoveListener<TournamentChangeSoundEvent>(ChangeSound);
	}

	void ChangeSound(TournamentChangeSoundEvent info)
	{
		ShouldPlaySound = info.SoundOn;
	}

	public void Init()
	{
		ShowLoading();
		_pigPool = Instantiate(PigPoolPrefab).GetComponent<CoinsPoolItem>();
		SetGameObjectDefult(_pigPool.gameObject);
		_selfItem = Instantiate(SelfItemPrefab).GetComponent<TournamentItem>();
		_selfEffect = _selfItem.GetComponent<SelfTournamenItemEffect>();
		SetGameObjectDefult(_selfItem.gameObject);
		for(int i = 0; i < _poolLength; i++)
		{
			var it = Instantiate(OtherItemPrefab).GetComponent<TournamentItem>();
			_otherItem.Add(it);
			SetGameObjectDefult(it.gameObject);
		}
		if (_normalTimerCoroutine == null)
		{
			_normalTimerCoroutine = StartCoroutine(StartTimer());
		}
	}

	/// <summary>
	/// 手动的刷新数据
	/// </summary>
	public void ReUpdateInfor()
	{
		if(!string.IsNullOrEmpty(UserBasicData.Instance.UDID))
		{
			_tournamentManager.UpdateInfor();
			TournamentHelper.Instance.CheckReward();
		}
		ShowLoading();
	}

	private void TournamentInforResultMessageP(TournamentInforResultEvent trm)
	{
		var infor = trm.Infor;
		bool cstate = true;
		TournamentTimeHelper.Instance.RemainderCtime(out cstate);

		if (infor.State == 0)
		{
			if (TournamentManager._hasEnd)
				return;
			else
				TournamentManager._hasEnd = true;
		}

		//Important fix by nichos:
		//Comment this part, since it might trigger another GetInfor after entering accepting prize state
//		if(cstate != infor.State)
//		{
//			_tournamentManager.UpdateInfor();
//			LogUtility.Log("本地状态和服务器状态不同步 等待同步进行加载", Color.red);
//			return;
//		}

		ShowNormale();
		int baselength;

		// 锦标赛进行时
		if(infor.State == 1)
		{
			baselength = _doinglength;
			_pigPool.SetValue(infor.Pool, TournamentManager.BetToSection(infor.Bet));
			SetUseItem(_pigPool.gameObject);

			//if(infor.RankInfor.ContainsKey(UserBasicData.Instance.UDID))
			//{
			//	if(!_lastTournamentInfor.ContainsKey(TournamentManager.BetToSection(infor.Bet)))
			//	{
			//		_selfEffect.ShowUpEffect();
			//		_lastTournamentInfor[TournamentManager.BetToSection(infor.Bet)] = infor.RankInfor[UserBasicData.Instance.UDID];
			//	}
			//	else
			//	{
			//		var cif = infor.RankInfor[UserBasicData.Instance.UDID];
			//		var lif = _lastTournamentInfor[TournamentManager.BetToSection(infor.Bet)];
			//		if(lif.Rank > cif.Rank)
			//		{
			//			_selfEffect.ShowUpEffect();
			//		}
			//		else if(lif.Rank < cif.Rank)
			//		{
			//			_selfEffect.ShowDownEffect();
			//		}
			//		_lastTournamentInfor[TournamentManager.BetToSection(infor.Bet)] = cif;
			//	}

			//}
		}
		// 非锦标赛 领奖阶段
		else
		{
			baselength = _donelength;
			//_lastTournamentInfor.Clear();
			if(infor.RankInfor.ContainsKey(UserBasicData.Instance.UDID) &&
			   infor.RankInfor[UserBasicData.Instance.UDID].Rank > 4)// 如果是第五名之上,而且在榜上
			{
				baselength = _donelength - 1;//少一个位置为自己留下位置
			}
			_pigPool.gameObject.SetActive(false);
		}
		if(trm.Infor.RankInfor.Keys.Count < 1)
		{
			//ShowLoading();
			return;
		}
		SetAllItemDefult();
		if(infor.RankInfor.ContainsKey(UserBasicData.Instance.UDID))
		{
			TournamentManager.DelayTime = 0;

			TournamentUserInfor selfInfo = infor.RankInfor[UserBasicData.Instance.UDID];

			List<string> keyslist = new List<string>(infor.RankInfor.Keys);
			for(int i = 0; i < baselength; i++)
			{
				string key = keyslist[i];
				var it = infor.RankInfor[key];
				if(key == UserBasicData.Instance.UDID)
				{
					_selfItem.SetValue(key, it.Rank, it.Score, it.Reward.Coins, infor
									   .State, it.Icon);
					SetUseItem(_selfItem.gameObject);
					continue;
				}

				_otherItem[i].SetValue(key, it.Rank, it.Score, it.Reward.Coins, infor
									   .State, it.Icon);
				SetUseItem(_otherItem[i].gameObject);
			}

			// 第五名或之上在榜单单独拿出来
			if(selfInfo.Rank > 4)
			{
				_selfItem.SetValue(UserBasicData.Instance.UDID, selfInfo.Rank, selfInfo.Score, selfInfo.Reward.Coins, 
					infor.State, selfInfo.Icon);
				SetUseItem(_selfItem.gameObject);
			}

			if(infor.State == 0)
			{
				AnalysisManager.Instance.TournamentFinalRank(GameScene.Instance.PuzzleMachine.MachineName,
					((int)TournamentManager.BetToSection(infor.Bet)).ToString(), selfInfo.Rank,
					(int)selfInfo.Score, (int)selfInfo.Reward.Coins, (int)infor.Pool
				);
			}
		}
		else
		{
			if (infor.SelfRankInfor == null)
				TournamentManager.DelayTime = _notHasScoreDelayTime;
			else
				TournamentManager.DelayTime = _notInBoardDelayTime;
			List<string> keyslist = new List<string>(infor.RankInfor.Keys);
			for(int i = 0; i < baselength - 1; i++)
			{
				string key = keyslist[i];
				var it = infor.RankInfor[key];
				_otherItem[i].SetValue(key, it.Rank, it.Score, it.Reward.Coins, infor
									   .State, it.Icon);
				SetUseItem(_otherItem[i].gameObject);
				//Debug.Log("Tournament rank:" + it.Rank.ToString());
			}

			_selfItem.SetValue(UserBasicData.Instance.UDID, 200, LocalScore(infor.Bet), 0, infor
							   .State, FacebookHelper.GetProfilePictureURLWithID(UserIDWithPrefix.GetFBIDNoPrefix()));

			// 中奖未上棒 取上次的排名并且检查是否存在不存在就默认
			if(infor.State == 0 && infor.SelfRankInfor != null)
			{
				var it = infor.SelfRankInfor;
				_selfItem.SetValue(UserBasicData.Instance.UDID, it.Rank, it.Score, it.Reward.Coins, infor
								   .State, it.Icon);

				AnalysisManager.Instance.TournamentFinalRank(GameScene.Instance.PuzzleMachine.MachineName,
					((int)TournamentManager.BetToSection(infor.Bet)).ToString(), it.Rank,
					(int)it.Score, (int)it.Reward.Coins, (int)infor.Pool);
			}
			else
			{
				_selfItem.SetValue(UserBasicData.Instance.UDID, 200, LocalScore(infor.Bet), 0, infor
							   .State, FacebookHelper.GetProfilePictureURLWithID(UserIDWithPrefix.GetFBIDNoPrefix()));
			}

			SetUseItem(_selfItem.gameObject);
		}
	}

	public void ShowError()
	{
		ErrorGameObject.SetActive(true);
		LoadingGameObject.SetActive(false);
		NormalGameObject.SetActive(false);
		if (_normalTimerCoroutine != null)
		{
			StopCoroutine(_normalTimerCoroutine);
			_normalTimerCoroutine = null;
		}
	}

	public void ShowLoading()
	{
		ErrorGameObject.SetActive(false);
		LoadingGameObject.SetActive(true);
		NormalGameObject.SetActive(false);
		if (_normalTimerCoroutine != null)
		{
			StopCoroutine(_normalTimerCoroutine);
			_normalTimerCoroutine = null;
		}
	}

	public void ShowNormale()
	{
		ErrorGameObject.SetActive(false);
		LoadingGameObject.SetActive(false);
		NormalGameObject.SetActive(true);
		if (_normalTimerCoroutine == null)
		{
			_normalTimerCoroutine = StartCoroutine(StartTimer());
		}
	}

	/// <summary>
	/// 创建假数据,并展示假数据UI,
	/// </summary>
	public void CreatFalseDataAndShow()
	{
		ShowNormale();
		SetAllItemDefult();

		_pigPool.SetValue(_defultPool, TournamentManager.BetToSection(_tournamentManager.CurrBet));
		SetUseItem(_pigPool.gameObject);
		// 假数据长度
		for(int i = 0; i < 2; i++)
		{
			_otherItem[i].SetValue("", (201 - _doinglength + i), 0, 0, 1, "");
			SetUseItem(_otherItem[i].gameObject);
		}
		_selfItem.SetValue(UserBasicData.Instance.UDID, 200, LocalScore(_tournamentManager.CurrBet), 0, 1, FacebookHelper.GetProfilePictureURLWithID(UserIDWithPrefix.GetFBIDNoPrefix()));
		SetUseItem(_selfItem.gameObject);
	}

	private void ServerErrorMessageP(TournamentServerErrorEvent tsem)
	{
		Debug.Log("Tournament error:" + tsem.ErrorFunction + tsem.Error);
		if(tsem.ErrorFunction == TournamentServerFunction.GetInfor)
		{
			// 服务器错误或是网络错误进行errorUI展示,如果不是12错误就展示 如果是12错误就就是状态不同加载等待加载
		/*	if(tsem.Error != "12")
			{
				ShowError();
				//LogUtility.Log("Tournament error:" + tsem.Error, Color.red);
				return;
			}
		*/
			if (tsem.Error == "")
			{
				if (!DeviceUtility.IsConnectInternet())
					ShowError();
			}
			else
			{
				ShowLoading();
				LogUtility.Log("锦标赛 网络错误 : " + tsem.Error, Color.cyan);
			}
			// 服务器状态错误进行加载展示

		}
	}

	private ulong LocalScore(int bet)
	{
		var state = TournamentManager.BetToSection(bet);
		var dic = UserDeviceLocalData.Instance.TournamentScoreInforDic;
		if(dic.ContainsKey(state))
		{
			var infor = dic[state];
			var times = NetworkTimeHelper.Instance.GetNowTime() - infor.WriteTime;
			//LogUtility.Log("差的时间"+times,Color.cyan);
			if(Mathf.Abs((float)times.TotalSeconds) > infor.LastTime)
			{
				// 数据不是次阶段的 重新刷新
				return 0;
			}

			return dic[state].AllScore;
		}

		return 0;
	}

	private void SetUseItem(GameObject go)
	{
		go.transform.SetParent(ItemGroup.transform);
		go.SetActive(true);
	}

	private void SetAllItemDefult()
	{
		SetGameObjectDefult(_selfItem.gameObject);
		for(int i = 0; i < _otherItem.Count; i++)
		{
			SetGameObjectDefult(_otherItem[i].gameObject);
		}
	}

	private void SetGameObjectDefult(GameObject go)
	{
		go.SetActive(false);
		go.transform.SetParent(ItemPool.transform, false);
		var rt = go.GetComponent<RectTransform>();
		rt.localPosition = Vector3.zero;
		go.transform.localScale = Vector3.one;
	}

	private IEnumerator StartTimer()
	{
		while(true)
		{
			bool isDoing = true;
			int time = TournamentTimeHelper.Instance.RemainderCtime(out isDoing);
			TimeGameObject.SerValue(time);
			SetTimerColor(time, isDoing);
			if(isDoing)
			{
				TimeGameObject.gameObject.SetActive(true);
				TimeUpGameObject.SetActive(false);
				if(!ErrorGameObject.activeSelf && !LoadingGameObject.activeSelf
					   && TournamentTimeHelper.Instance.RemainderCtime() == _playsound5Time)
				{
					StartCoroutine(TimeGameObject.TimerFlickerEffect(this, _playsound5Time, _loopTimes*_singleLoopTime, _singleLoopTime, _flickerLargerScale, _flickerSmallScale));
					if(ShouldPlaySound)
						AudioManager.Instance.PlaySound(AudioType.Last5Second);
					yield return new WaitForSeconds(1);
				}
			}
			else
			{
				if(TournamentTimeHelper.Instance.RemainderCtime() <= _showlastRewardStateTime)
				{
					TimeUpGameObject.SetActive(false);
					TimeGameObject.gameObject.SetActive(true);

					if(!ErrorGameObject.activeSelf && !LoadingGameObject.activeSelf
					   && TournamentTimeHelper.Instance.RemainderCtime() == _playsound5Time)
					{
						StartCoroutine(TimeGameObject.TimerFlickerEffect(this, _playsound5Time, _loopTimes*_singleLoopTime, _singleLoopTime, _flickerLargerScale, _flickerSmallScale));
						if(ShouldPlaySound)
							AudioManager.Instance.PlaySound(AudioType.Last5Second);
						yield return new WaitForSeconds(1);
					}
				}
				else
				{
					TimeUpGameObject.SetActive(true);
					TimeGameObject.gameObject.SetActive(false);
				}
			}

			yield return new WaitForEndOfFrame();
		}
	}

	private void SetTimerColor(int remainTime, bool isDoing){

		if (remainTime > _timerLast75Sec)
		{
			TimeGameObject.SetTimerColor (Color.white);
		}
		
		if (remainTime <= _timerLast75Sec && remainTime > _timerLast10Sec)	
		{
			TimeGameObject.SetTimerColor(Color.yellow);
		}

		if (remainTime <= _timerLast10Sec) 
		{
			var color = isDoing ? Color.red : Color.white;
			TimeGameObject.SetTimerColor(color);
		}
	}
}
