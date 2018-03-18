using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using UnityEngine;
using UnityEngine.Events;

public class UserLevelSystem : Singleton<UserLevelSystem>
{
	public LevelData CurrUserLevelData { get { return UserBasicData.Instance.UserLevel; } }
	public LevelConfigData CurrUserLevelConfigData { get; private set; }
	public LevelConfigData NextUserLevelConfigData { get; private set; }

	public UnityEvent LevelDataChangeEvent = new UnityEvent();

	// Use this for initialization
	public void Awake()
	{
		UpdateUserLevelData();
		CitrusEventManager.instance.AddListener<UserDataLoadEvent>(UpdateLevelDataMessageP);
	}

	public void UpdateLevelDataMessageP(UserDataLoadEvent loadms)
	{
		UpdateUserLevelData();
	}

	public void UpdateUserLevelData()
	{
		CurrUserLevelConfigData = UserLevelConfig.Instance.GetLevelDataByLevel((int)CurrUserLevelData.Level);
		NextUserLevelConfigData = UserLevelConfig.Instance.GetLevelDataByLevel((int)(CurrUserLevelData.Level + 1));
		LogUtility.Log("现在用户等级"+CurrUserLevelData.Level+"下一个等级需要的经验"+NextUserLevelConfigData.RequiredXP, Color.yellow);
	}

	public void AddLevelXP(int point, bool nowSave = true)
	{
		if(point <= 0) { return; }
		// 乘以比率
		point = point * CurrUserLevelConfigData.XPBetMultiplier;

		LogUtility.Log("Add Level XP point = " + point, Color.yellow);

		LevelData newLD = CurrUserLevelData;
		newLD.LevelPoint += point;

		LogUtility.Log("new level point = " + newLD.LevelPoint, Color.yellow);

		bool isLevelUp = false;
		int oldLevel = (int)newLD.Level;

		//添加点数
		UserBasicData.Instance.SetUserLevelData(newLD, false);
		while(newLD.LevelPoint >= NextUserLevelConfigData.RequiredXP)
		{
			newLD.LevelPoint -= NextUserLevelConfigData.RequiredXP;
			newLD.Level = NextUserLevelConfigData.Level;
			// 添加等级
			UserBasicData.Instance.SetUserLevelData(newLD, false);
			UpdateUserLevelData();
			LevelUpBonus.GetUserLevelUpBonus((int)CurrUserLevelData.Level,false);
			Debug.Log("升级添加奖励");
			isLevelUp = true;
		}

		if(nowSave) { 
			UserBasicData.Instance.Save();
		}

		if (isLevelUp) {
			CitrusEventManager.instance.Raise(new LevelupTipEvent(oldLevel, (int)newLD.Level));
			CitrusEventManager.instance.Raise(new MachineUnlockEvent());
		}

		//LogUtility.Log("添加的Level经验点数" + point + "现在的等级+点数" + UserBasicData.Instance.UserLevel.Level + " " +
					   //UserBasicData.Instance.UserLevel.LevelPoint, Color.blue);

		LevelDataChangeEvent.Invoke();
	}

	public void TestCleanLevel()
	{
		UserBasicData.Instance.SetUserLevelData(new LevelData { Level = 1, LevelPoint = 0 }, true);
		UpdateUserLevelData();
	}
}
