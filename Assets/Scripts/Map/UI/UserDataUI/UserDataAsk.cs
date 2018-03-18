using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CitrusFramework;

public class UserDataAsk : MonoBehaviour
{
	[Serializable]
	public class AskText
	{
		public Text CoinsText;
		public Text LevelText;
		public Text VipLevelText;
		public Image VipIcon;


		public void ShowText(ulong coins, VIPData vipdata, LevelData level)
		{
			CoinsText.text = StringUtility.FormatNumberString(coins, true, true);

			VipLevelText.text = vipdata.VIPLevelName;
			VipIcon.sprite = VIPConfig.Instance.GetDiamondImageByLevelName(vipdata.VIPLevelName);

			var NextUserLevelConfigData = UserLevelConfig.Instance.GetLevelDataByLevel((int)(level.Level + 1));
			LevelText.text = "LeveL " + level.Level + "  " + (int)(level.LevelPoint / NextUserLevelConfigData.RequiredXP * 100) + "%";
		}
	}

	public AskText UseServerText;
	public AskText UseLocalText;

	private LevelData _vIPLevelData;
	private LevelData _level;
	private ulong _credits;

	private void OnEnable()
	{
		var jsonObject = UserDataHelper.Instance.MD5DifferentJsonObject;

		//comment by nichos: todo, when jsonObject is null, it might be a loophole
		if(jsonObject == null)
			return;
		
		_level = new LevelData
		{
			Level = (float)jsonObject.GetField(ServerFieldName.UserLevel.ToString()).n,
			LevelPoint =
				(float)jsonObject.GetField(ServerFieldName.UserLevelPoint.ToString()).n
		};

		_vIPLevelData = new LevelData
		{
			LevelPoint = (int)jsonObject.GetField(ServerFieldName.VipPoint.ToString()).n,

			Level = VIPConfig.Instance.GetPointAboutVIPLevel((int)jsonObject.GetField(ServerFieldName.VipPoint.ToString()).n)
		};

		var vipCoinfigData = VIPConfig.Instance.FindVIPDataByLevel((int)_vIPLevelData.Level);

		_credits = (ulong)jsonObject.GetField(ServerFieldName.Credits.ToString()).n;

		UseServerText.ShowText(_credits, vipCoinfigData, _level);


		UseLocalText.ShowText(UserBasicData.Instance.Credits, VIPSystem.Instance.GetCurrVIPInforData, UserLevelSystem.Instance.CurrUserLevelData);

		AnalysisManager.Instance.OpenLoadServerData((int)_level.Level, (int)_vIPLevelData.LevelPoint, _credits);
	}


	public void SendUseServerMessage()
	{
		Callback callback = () => {
			UserDataUIController.Instance.UseServerData();
			AnalysisManager.Instance.SelectLoadServerData((int)_level.Level, (int)_vIPLevelData.LevelPoint, _credits);
		};

		if(IsLocalLevelHigher())
			ShowUserDataChooseWarning(callback);
		else
			callback();
	}

	public void SendLocalMessage()
	{
		Callback callback = () => {
			UserDataUIController.Instance.UseLocal();
			AnalysisManager.Instance.SelectUseLocalData((int)_level.Level, (int)_vIPLevelData.LevelPoint, _credits);
		};

		if(IsServerLevelHigher())
			ShowUserDataChooseWarning(callback);
		else
			callback();
	}

	void ShowUserDataChooseWarning(Callback callback)
	{
		GameObject obj = UGUIUtility.InstantiateUI("Common/UserDataChooseWarningUI");
		UserDataChooseWarningController controller = obj.GetComponent<UserDataChooseWarningController>();
		controller.Init(callback, null);
	}

	bool IsLocalLevelHigher()
	{
		int r = LevelData.Compare(UserLevelSystem.Instance.CurrUserLevelData, _level);
		return r > 0;
	}

	bool IsServerLevelHigher()
	{
		int r = LevelData.Compare(UserLevelSystem.Instance.CurrUserLevelData, _level);
		return r < 0;
	}
}
