using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;

public class ScoreAppSystem : Singleton<ScoreAppSystem> {

	public void Init()
	{
	}

	void Start () 
	{
		//CitrusEventManager.instance.AddListener<EpicWinEvent>(EpicWinEvent);
		CitrusEventManager.instance.AddListener<MachineGotFiveStarEvent>(MachineGotFiveStarEvent);
	}

	void Update () 
	{
		
	}

	public void EpicWinEvent()
	{
		CheckUserScoredRecord();
	}

	private void MachineGotFiveStarEvent(MachineGotFiveStarEvent e)
	{
		CheckUserScoredRecord();
	}


	private void CheckUserScoredRecord()
	{
		var firstScore = IsFirstScore();
		var userInNoDisturbState = UserInNoDisturbState();
		int totalspin = MapSettingConfig.Instance.Read<int>("TotalSpinCountToOpenScoreApp",50);
		bool isTotalSpinEnough = UserMachineData.Instance.TotalSpinCount > totalspin;

		if (firstScore && !userInNoDisturbState && isTotalSpinEnough) 
		{
			UIManager.Instance.OpenScoreAppUi();
		}
	}

	//check if user had alread scored our app
	private bool IsFirstScore()
	{
		return UserBasicData.Instance.IsFirstScore;
	}

	//2 days wait for cooldown
	private bool UserInNoDisturbState()
	{
		var lastCloseDate = UserBasicData.Instance.LastCloseScorePageTime;
		var timeCompareResult = System.DateTime.Now.CompareTo(lastCloseDate.AddDays(2));
		var result = timeCompareResult < 0;
		return result;
	}

	public void SetClosePageTime()
	{
		UserBasicData.Instance.SetLastCloseScorePageDate(System.DateTime.Now);
	}

	public void UserScoredApp()
	{
		UserBasicData.Instance.IsFirstScore = false;
		GiveUserReward();
	}

	public void GiveUserReward()
	{
		var reward = CoreConfig.Instance.MiscConfig.ScoredInStoreReward;
		UserBasicData.Instance.AddCredits((ulong)reward, FreeCreditsSource.RateGameBonus, true);
		GetCredits.Instance.PlayGetCreditEffect();
	}
}
