using System.Collections;
using System.Collections.Generic;
using CitrusFramework;
using UnityEngine;

public class CheckRewardResultEvent : CitrusGameEvent 
{
	public List<RewardInfor> RewardInforList;

	public CheckRewardResultEvent(List<RewardInfor> lri):base()
	{
		RewardInforList = lri;
	}
}
