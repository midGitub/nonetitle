using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserLevelUtility  {

	public static void UpdateUserLevelFromJson(JSONObject obj){
		LevelData userData = UserBasicData.Instance.UserLevel;
		float progress = FetchUserLevelProgress(obj, userData);
		LevelData newUserData = FetchUserLevel(userData, progress);
		UserBasicData.Instance.SetUserLevelData(newUserData, false);
		UserLevelSystem.Instance.UpdateUserLevelData();
	}

	public static LevelData FetchUserLevel(LevelData data, float progress){
		LevelConfigData refData = UserLevelConfig.Instance.GetLevelDataByLevel((int)data.Level + 1);
		LevelData newUserData = new LevelData();
		newUserData.Level = data.Level;
		newUserData.LevelPoint = refData.RequiredXP * progress;
		LogUtility.Log("Fetch new level point : " + newUserData.LevelPoint, Color.yellow);
		return newUserData;
	}

	private static float FetchUserLevelProgress(JSONObject obj, LevelData userData){
		float progress = 0.0f;
		LogUtility.Log("Fetch user data level : " + userData.Level + " point : " + userData.LevelPoint, Color.yellow);

		if (obj != null && obj.HasField(ServerFieldName.UserLevelProgress.ToString())){
			progress = obj.GetField(ServerFieldName.UserLevelProgress.ToString()).f;
			LogUtility.Log("Fetch leve progress from server : " + progress, Color.yellow);
		}else{
			// 没有这个字段，表示为老用户，需要通过旧表格来换算经验进度
			progress = GetLevelProgressOld2NewVersion(userData);
			LogUtility.Log("Fetch leve progress from local : " + progress, Color.yellow);
		}

		return progress;
	}

	public static float GetLevelProgressOld2NewVersion(LevelData data){
		float result = UserLevelConfig.Instance.GetLevelProgress((int)data.Level, data.LevelPoint, true);
		return result;
	}
}
