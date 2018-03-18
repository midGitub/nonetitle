using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserLevelConfig : SimpleSingleton<UserLevelConfig>
{
	public static readonly string Name = "LevelConfig";
	private static int _maxUserLevel = 200;// 默认200级

	private LevelConfigSheet _sheet;

	private LevelConfigData LevelCalculationFormula(int currLevel)
	{
		LevelConfigData ld = _sheet.dataArray[_maxUserLevel];

		LevelConfigData nextLevel = new LevelConfigData
		{
			Level = currLevel,
			// 计算公式
			RequiredXP = _sheet.dataArray[_maxUserLevel - 1].RequiredXP + (currLevel - _maxUserLevel) * ld.RequiredXP,
			RequiredXPBefore = _sheet.dataArray[_maxUserLevel - 1].RequiredXPBefore + (currLevel - _maxUserLevel) * ld.RequiredXPBefore,
			XPBetMultiplier = ld.XPBetMultiplier,
			LevelUpBonusCredits = ld.LevelUpBonusCredits,
			LevelUpBonusVIPPoints = ld.LevelUpBonusVIPPoints,
			LevelUpBonusLTLucky = ld.LevelUpBonusLTLucky,
		};

		return nextLevel;
	}

	public UserLevelConfig()
	{
		LoadData();

		// Test();
	}

	private void LoadData()
	{
		_sheet = GameConfig.Instance.LoadExcelAsset<LevelConfigSheet>(Name);
		_maxUserLevel = _sheet.dataArray.Length - 1;// 最后一个数是个附加参考值，不算在玩家等级内
	}

	public static void Reload()
	{
		Debug.Log("Reload UserLevelConfig");
		UserLevelConfig.Instance.LoadData();
	}

	public LevelConfigData GetLevelDataByLevel(int currLevel)
	{
		LevelConfigData result = null;
		if(currLevel > _maxUserLevel)
			result = LevelCalculationFormula(currLevel);
		else
			result = _sheet.dataArray[currLevel - 1];
		return result;
	}

	public float GetLevelProgress(int currLevel, float levelPoint, bool isOriginal){
		float result = 0.0f;
		float requiredXP = 0.0f;
		LevelConfigData data = null;

		if (currLevel > _maxUserLevel){
			data = LevelCalculationFormula(currLevel + 1);
		}else{
			data = _sheet.dataArray[currLevel];	
		}

		if (isOriginal){// 用旧表经验换算
			requiredXP = data.RequiredXPBefore;
		}else{
			requiredXP = data.RequiredXP;
		}

		Debug.Assert(requiredXP != 0.0f);
		result = levelPoint / requiredXP;
		return result;
	}

	private void Test(){
		float progress = 0.0f;
		int[] levelArray = new int[]{ 10, 15, 25, 45, 65, 85, 99, 100, 101, 105 };
		float[] pointArray = new float[]{ 50000, 60000, 70000, 100000, 150000, 250000, 400000, 600000, 1000000, 2000000 };

		CoreDebugUtility.Log("Start Level Test");
		for(int i = 0; i < levelArray.Length; ++i){
			progress = GetLevelProgress(levelArray[i], pointArray[i], false);
			CoreDebugUtility.Log("progress = "+progress);
		}

		CoreDebugUtility.Log("Start Level Test 2");
		for(int i = 0; i < levelArray.Length; ++i){
			progress = GetLevelProgress(levelArray[i], pointArray[i], true);
			CoreDebugUtility.Log("progress = "+progress);
		}
	}
}
