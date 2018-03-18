using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class UserGroup {
	
	public static int GetUserGroupID()
	{
		/*#if UNITY_EDITOR
		Debug.Log("paytime:"+UserBasicData.Instance.LastPayTime);
		Debug.Log("buynumber:"+UserBasicData.Instance.BuyNumber);
		Debug.Log("payamount:"+UserBasicData.Instance.TotalPayAmount);
		Debug.Log("group:"+GetUserGroupIDWith (UserBasicData.Instance.LastPayTime, UserBasicData.Instance.BuyNumber, UserBasicData.Instance.TotalPayAmount));
		for(int i = 0;i < 1000;)
		{
			for(int k = 0;k <= 10 ;k+=4)
			{
				Debug.Log("0"+" "+k+" "+i+" "+GetUserGroupIDWith(NetworkTimeHelper.Instance.GetNowTime(),k,i));
				Debug.Log("1"+" "+k+" "+i+" "+GetUserGroupIDWith(NetworkTimeHelper.Instance.GetNowTime().AddDays(-10),k,i));
			}
			if(i == 0 ) i = 10;
			else if(i == 10) i = 20;
			else if(i == 20) i = 50;
			else if(i == 50) i = 800;
			else i = 2000;
		}
		#endif*/
		int result = 0;
		if (IsNewBie())
			result = 1;
		else
		{
			GroupMember member = new GroupMember ();
			result = GetUserGroupIDWith (member);
		}
			
		return result;
	}
		
	private static bool IsNewBie()
	{
		bool flag = false;
		if (TimeUtility.DaysLeft (NetworkTimeHelper.Instance.GetNowTime (), UserDeviceLocalData.Instance.FirstEnterGameTime) < 4 && !UserBasicData.Instance.IsPayUser)
			flag = true;
		return flag;
	}
		
	static int GetUserGroupIDWith(GroupMember member)
	{
		return GroupRuleConfig.Instance.GetIDWith(new GroupMemberRepresent (member));
	}
}
