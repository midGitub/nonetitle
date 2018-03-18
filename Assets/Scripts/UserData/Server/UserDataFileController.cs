using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;

public static class UserDataFileController
{
	// public static readonly string UserDataBaseFileName = "UserBasicData";
	//public static readonly string UserMachineData = "UserMachineData";

	private static Dictionary<string, UserDataBase> FileNameDic = new Dictionary<string, UserDataBase>();

	public static string GetUserDataFileName(string Name, UserDataBase ub)
	{
		string result = "";

		if(!FileNameDic.ContainsKey(Name))
			FileNameDic[Name] = ub;

		if(UserLoginStateHelper.Instance.IsDeviceLoginState)
			result = Name;
		else
			result = UserDeviceLocalData.Instance.GetCurrSocialAppID + "_" + Name;

		return result;
	}

	// 创建根据社交ID名称的用户数据文件,如果账号为空返回空值
	public static bool CreateSocialIDUserDataFile()
	{
		foreach(var item in FileNameDic)
		{
			string path = Application.persistentDataPath + "/";
			string oldUserDataPath = path + item.Key;
			string newUserDataPath = path + GetUserDataFileName(item.Key, item.Value);

			if(FileHelper.CopyFile(oldUserDataPath, newUserDataPath))
			{
				LogUtility.Log(item.Value + "文件创建完成", Color.cyan);
			}
			else
			{
				LogUtility.Log(item.Value + "文件创建失败已存在", Color.cyan);
			}
		}
		return true;
	}

	public static void LoadAllFile()
	{
		foreach(var item in FileNameDic)
		{
			item.Value.Load();
		}

		CitrusEventManager.instance.Raise(new UserDataLoadEvent());
		CitrusEventManager.instance.Raise(new MachineUnlockEvent());
	}

	public static void SaveAllFile()
	{
		foreach(var item in FileNameDic)
		{
			item.Value.Save();
		}
	}

	// 户数据只有在 facebook登录后才上传和下载,在每次登录facebook回调的时候判断是否拥有数据 有的话
	// 拉取数据,如果没有 提示绑定,退出后本次玩还是玩facebook的数据,下次登录使用默认的游客数据
	// 退出本次还是用的 原来第一次登录绑定的FacebookID，下次重新进入游戏的时候 使用登录的facebook

}
