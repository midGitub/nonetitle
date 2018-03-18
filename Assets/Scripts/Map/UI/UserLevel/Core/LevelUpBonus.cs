using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using UnityEngine;

public static class LevelUpBonus
{
	/// <summary>
	/// 添加当前等级的奖励
	/// </summary>
	/// <param name="currlevel">Currlevel.</param>
	public static void GetUserLevelUpBonus(int currlevel, bool nowSave = true)
	{
		var levelData = UserLevelConfig.Instance.GetLevelDataByLevel(currlevel);
		UserBasicData.Instance.AddCredits((ulong)levelData.LevelUpBonusCredits, FreeCreditsSource.LevelUpBonus, false);
		VIPSystem.Instance.AddVIPPoint(levelData.LevelUpBonusVIPPoints, false);
		UserBasicData.Instance.AddLongLucky(levelData.LevelUpBonusLTLucky, false);

		if(nowSave) { UserBasicData.Instance.Save(); }

		LogUtility.Log("等级奖励" + "Credits:" + levelData.LevelUpBonusCredits + "  VIPPOInt" + levelData.LevelUpBonusVIPPoints +

		               "   longlucky" + levelData.LevelUpBonusLTLucky, Color.yellow
					  );

		CitrusEventManager.instance.Raise(new UserLevelUpEvent(levelData));
		AnalysisManager.Instance.LevelUp(currlevel, levelData.LevelUpBonusCredits, levelData.LevelUpBonusLTLucky);
	}

}
