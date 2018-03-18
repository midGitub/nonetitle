using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CitrusFramework;

public class ShowADSController : Singleton<ShowADSController>
{
	int _showPlaqueTime;
	// 进机台的次数
	[HideInInspector]
	public int _inGameTime = 0;
	int _needShowTime;

	public void Start()
	{
		Init();
	}

	public void Init()
	{
		var data = ADSConfig.Instance.Sheet.dataArray[(int)UserBasicData.Instance.PlayerPayState];
		_needShowTime = UnityEngine.Random.Range(1, data.PlaqueADSTimeInterval[0]);
		UserBasicData.Instance.NoAds = !data.PlaqueADSOPen;
		ADSManager.Instance.Init();

		CitrusEventManager.instance.AddListener<ShowInterstitialEvent>(ShowInterstitialEventP);
		CitrusEventManager.instance.AddListener<LoadSceneFinishedEvent>(InGameSceneEvent);

	}
	private void InGameSceneEvent(LoadSceneFinishedEvent lfe)
	{
		if(lfe.SceneName == ScenesController.GameSceneName)
		{
			_inGameTime++;
		}
	}
	private void ShowInterstitialEventP(ShowInterstitialEvent e)
	{
		ShowPlaqueADS();
	}

	public bool TryGetRewardADSVedio()
	{
#if UNITY_EDITOR
		return true;
#endif
		if(!ADSManager.Instance.HaveVideoAD())
		{
			return false;
		}

	    if (!AdStrategyConfig.Instance.IsRewardLobbyAdActive(GroupConfig.Instance.GetAdStrategyId()))
	    {
	        Debug.Log("广告策略中对此玩家不展示广告");
            return false;
        }
		var data = ADSConfig.Instance.Sheet.dataArray[(int)UserBasicData.Instance.PlayerPayState];
		float needTime = UserBasicData.Instance.TotalPayAmount * data.RewardADSTimeInterval[0] + data.RewardADSTimeInterval[1];
		needTime = needTime > data.RewardADSTimeInterval[2] ? data.RewardADSTimeInterval[2] : needTime;

		if((NetworkTimeHelper.Instance.GetNowTime() - UserDeviceLocalData.Instance.LastGetGetRewardADSVedioTime).TotalSeconds > needTime)
		{
			if(data.GetRewardADSLimit == 0)
			{
				return true;
			}
			else if(UserDeviceLocalData.Instance.RewardADSVedioPlayTime < data.GetRewardADSLimit)
			{
				return true;
			}
			else
			{
				Debug.Log("广告有限制");
				return false;
			}
		}
		else
		{
			//Debug.Log("广告时间不到");
		}

		return false;
	}

	public bool TryGetPlaqueADS()
	{
        if (!ADSManager.Instance.HavePictureAD())
		{
			Debug.Log("NoPADS");
			return false;
		}
        if (!AdStrategyConfig.Instance.IsInterstitialActive(GroupConfig.Instance.GetAdStrategyId()))
        {
            Debug.Log("广告策略中对此玩家不展示广告");
            return false;
        }
        var data = ADSConfig.Instance.Sheet.dataArray[(int)UserBasicData.Instance.PlayerPayState];
		if(UserBasicData.Instance.NoAds)
		{
			Debug.Log("玩家购买了没有广告");
			return false;
		}
		if((NetworkTimeHelper.Instance.GetNowTime() - UserBasicData.Instance.RegistrationTime).TotalDays < data.RegisterDaysOpenPlaqueADS)
		{
			Debug.Log("时间少于3天");
			return false;
		}
		if((NetworkTimeHelper.Instance.GetNowTime() - UserDeviceLocalData.Instance.LastShowPlagueTime).TotalMinutes < data.PlaqueADSTimeInterval[1])
		{
			if(_showPlaqueTime >= data.PlaqueADSTimeInterval[2])
			{
				Debug.Log("展示的时间太短");
				return false;
			}

			if(_needShowTime == _inGameTime)
			{
				Debug.Log("展示");
				return true;
			}

		}
		else
		{
			_showPlaqueTime = 0;
			if(_needShowTime == _inGameTime)
			{
				//Debug.LogError("1");
				Debug.Log("展示");
				return true;
			}
		}

		return false;
	}

	public void ShowPlaqueADS()
	{
		if(TryGetPlaqueADS())
		{
			Debug.Log("可以播放插屏广告");
			UserDeviceLocalData.Instance.LastShowPlagueTime = NetworkTimeHelper.Instance.GetNowTime();
			_inGameTime = 0;
			var data = ADSConfig.Instance.Sheet.dataArray[(int)UserBasicData.Instance.PlayerPayState];
			_needShowTime = UnityEngine.Random.Range(1, data.PlaqueADSTimeInterval[0]);
			_showPlaqueTime++;
			ADSManager.Instance.ShowPictureADS();
		}
	}

	public void ShowRewardADSVedio()
	{
        // ADSManager.Instance.SetAnalysisData(RewardAdType.Lobby);
		ADSManager.Instance.ShowVideoADS();
	}
}
