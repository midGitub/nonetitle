using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using UnityEngine;

public class BonusManager : Singleton<BonusManager>
{
	private static string _tournamentRewardPath = "Tournament/RewardUI";
	private GameObject _rournamentRewardGameobject;
	private RewardInfor _lastRewardInfor;
	private bool _isShowedRewardUI = false;
	private bool _isAcceptPrizeState = false;
	private bool _trigger = true;
	private Coroutine updateCheckRewardIe;

	private void Awake()
	{
		CitrusEventManager.instance.AddListener<CheckRewardResultEvent>(CheckRewardResultMessageP);
		CitrusEventManager.instance.AddListener<GetRewardResultEvent>(GetRewardResultMessageP);
		CitrusEventManager.instance.AddListener<CanShowTournamentRewardUIEvent>(CanShowTouranmenRewardUIEventP);
		CitrusEventManager.instance.AddListener<TournamentServerErrorEvent>(ServerErrorMessageP);
	}

	private void OnEnable()
	{
		//if(ScenesController.Instance.GetCurrSceneName() == ScenesController.GameSceneName)
		//{
		//	CheckReward();
		//}

#if UNITY_EDITOR
		if(ScenesController.Instance.GetCurrSceneName() == ScenesController.GameSceneName)
		{
			CheckReward();
		}
#endif
	}

	private IEnumerator UpdateCheckReward()
	{
		while(true)
		{
			TournamentTimeHelper.Instance.RemainderCtime(out _isAcceptPrizeState);
			_isAcceptPrizeState = !_isAcceptPrizeState;
			if(_isAcceptPrizeState)
			{
				if(_trigger)
				{
					_trigger = false;
					Debug.Log("Start tournament get reward");
					yield return new WaitForSeconds(1);
					TournamentHelper.Instance.CheckReward();
				}
			}
			else
			{
				_trigger = true;
			}
			yield return new WaitForSeconds(1);
		}
	}

	public void CheckReward()
	{
		if(updateCheckRewardIe != null)
			return;
		
		_trigger = true;
		Debug.Log("Start check reward");
		updateCheckRewardIe = StartCoroutine(UpdateCheckReward());
	}

	private void ServerErrorMessageP(TournamentServerErrorEvent tsem)
	{
		Debug.Log("BonusManager server error: " + tsem.ErrorFunction + tsem.Error);//有状态错误
		if(tsem.ErrorFunction == TournamentServerFunction.CheckReward)
		{
			// 服务器错误或是网络错误进行errorUI展示,如果不是12错误就展示 如果是12错误就就是状态不同加载等待加载
			if(tsem.Error != "12")
			{
				// 重新得到
				if(Application.internetReachability != NetworkReachability.NotReachable)
				{
					// 如果当前账户注册过就重新获取奖励,没有注册过就忽略没有注册不可能中奖并且参加锦标赛
					if(UserBasicData.Instance.UDID != "")
					{
						TournamentHelper.Instance.CheckReward();
					}
					Debug.Log("用户没有注册忽略检查奖励");
				}
				return;
			}
			Debug.Log("忽略检查奖励");
		}
		else if(tsem.ErrorFunction == TournamentServerFunction.GetReward)
		{
			LogUtility.Log("错误GetReward错误", Color.red);
			// 如果错误不是没有网络就重新删除服务器奖励信息我已经GetReward的了
			if(Application.internetReachability != NetworkReachability.NotReachable)
			{
				//服务器数据清除
				TournamentHelper.Instance.GetReward();
			}
		}
	}

	private void CanShowTouranmenRewardUIEventP(CanShowTournamentRewardUIEvent cst)
	{
		if(!_isShowedRewardUI && _lastRewardInfor != null)
		{
			// 开启领取奖励UI
			_rournamentRewardGameobject = UGUIUtility.CreateObj(AssetManager.Instance.LoadAsset<GameObject>(_tournamentRewardPath), this.gameObject);
			_rournamentRewardGameobject.SetActive(true);
			_rournamentRewardGameobject.GetComponent<TournamentRewardUI>().SetValue(_lastRewardInfor.Rank, _lastRewardInfor.Coins);
			var sb = _rournamentRewardGameobject.GetComponentInChildren<ShareButton>();
			sb.ChangeShareText(SharePlace.Tournament, _lastRewardInfor.Rank.ToString(), _lastRewardInfor.Rank);
			sb.ShareFinishedEvent.AddListener(GetTournamentReward);
			sb.SToggle.isOn = ShareManager.Instance.TournamentCanShare;
			sb.SToggle.onValueChanged.AddListener((arg0) => { ShareManager.Instance.TournamentCanShare = arg0; });
			_isShowedRewardUI = true;
			AudioManager.Instance.PlaySound(AudioType.TournamentCollect);
			//停止自动
			if(GameScene.Instance != null)
			{
				GameScene.Instance.PuzzleMachine.SetSpinMode(SpinMode.Normal);
			}
		}
	}

	private void CheckRewardResultMessageP(CheckRewardResultEvent crrm)
	{
		// 重复领奖的信息不处理
		if(_lastRewardInfor != null)
		{
			LogUtility.Log("重复领奖不处理", Color.red);
			return;
		}
		// 如果没中奖或是领过 里面就是空的
		if(crrm.RewardInforList.Count > 0)
		{
			LogUtility.Log("Tournament win to rank", Color.red);
			// 奖励信息赋值给他
			_lastRewardInfor = crrm.RewardInforList[0];
			//服务器数据清除
			TournamentHelper.Instance.GetReward();

			if(ScenesController.Instance.GetCurrSceneName() == ScenesController.MainMapSceneName
			   || (!GameScene.Instance.PuzzleMachine._effect.IsShowingSpecialWinEffect &&
				  GameScene.Instance.PuzzleMachine.GetState() == MachineState.Idle))
			{
				CitrusEventManager.instance.Raise(new CanShowTournamentRewardUIEvent());
			}
		}
		else {
			//LogUtility.Log("没有中奖");
		}
	}

	private void GetRewardResultMessageP(GetRewardResultEvent ms)
	{
		LogUtility.Log("奖励领取成功", Color.cyan);
	}


	public void GetTournamentReward()
	{
		// 关闭领取奖励UI
		_isShowedRewardUI = false;

		if(_lastRewardInfor != null)
		{
			LogUtility.Log("奖励领取" + _lastRewardInfor.Bet + "," + _lastRewardInfor.Coins, Color.cyan);
			UserBasicData.Instance.AddCredits(_lastRewardInfor.Coins, FreeCreditsSource.TournamentBonus, true);
			UserBasicData.Instance.AddLongLucky((int)(_lastRewardInfor.Coins * CoreConfig.Instance.LuckyConfig.RewardFactor), true);
			// 清除数据
			_lastRewardInfor = null;
		}

		if(_rournamentRewardGameobject != null)
		{
			Destroy(_rournamentRewardGameobject);
		}
	}

	public void Init()
	{
	}
}
