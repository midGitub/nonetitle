using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GroupMember
{
	public float PayAmount;
	public int PayCount;
	public DateTime LastPaytime;
//	public DateTime LastLogintime;
	public float HistoryMaxPay;
	public int SpinAverage;
	public int SessionAverage;
	public GroupMember()
	{
		PayAmount = UserBasicData.Instance.TotalPayAmount;
		PayCount = UserBasicData.Instance.BuyNumber;
		LastPaytime = UserBasicData.Instance.LastPayTime; 
		HistoryMaxPay = UserBasicData.Instance.HistoryMaxPaid;
		if(UserBasicData.Instance.SpinDays != 0 )
			SpinAverage = (int) ((double)UserMachineData.Instance.TotalSpinCount)/ UserBasicData.Instance.SpinDays;
		if (UserBasicData.Instance.LoginDays != 0)
			SessionAverage = (int)((double)UserBasicData.Instance.LoginTimes) / UserBasicData.Instance.LoginDays;
		//LastLogintime = UserBasicData.Instance.LastLoginDateTime;
	}
}

public class GroupMemberRepresent
{
	public int PayAmount;
	public int PayCount;
	public int HasPayInValidPeriod;
	public int HistoryMaxPay;
	public int SpinAverage;
	public int SessionAverage;
	public GroupMemberRepresent(GroupMember member)
	{
		PayAmount = GroupRepresentConfig.Instance.GetRepresent((int)member.PayAmount, GroupRepresent.PayAmount);
		PayCount = GroupRepresentConfig.Instance.GetRepresent((int)member.PayCount, GroupRepresent.PayCount);
		HasPayInValidPeriod = GroupRepresentConfig.Instance.GetHasPayInValidPeriodRepresent(member.LastPaytime);
		HistoryMaxPay = GroupRepresentConfig.Instance.GetRepresent((int)member.HistoryMaxPay,GroupRepresent.HistoryMaxPay);
		SpinAverage = GroupRepresentConfig.Instance.GetRepresent(member.SpinAverage, GroupRepresent.Spin);
		SessionAverage = GroupRepresentConfig.Instance.GetRepresent(member.SessionAverage, GroupRepresent.Session);
//		int days = (NetworkTimeHelper.Instance.GetNowTime() - member.LastLogintime).Days;
//		Login = GroupRepresentConfig.Instance.GetRepresent(days, GroupRepresent.LastLogin);
	}
}

public class GroupRuleConfig : SimpleSingleton<GroupRuleConfig>
{
	public static readonly string Name = "GroupRule";
	private GroupRuleSheet _sheet;
	private int[] hash;
	private int payinweekweight, countsweight, amoutweight;
	private GroupMemberRepresent _lastQueryGroupMember;
	private int _lastQueryID;
	public GroupRuleConfig()
	{
		LoadData ();
	}

	private void LoadData()
	{
		_sheet = GameConfig.Instance.LoadExcelAsset<GroupRuleSheet>(Name);
//		recpy ();
	}

	public static void Reload()
	{
		Debug.Log("Reload GroupRule");
		GroupRuleConfig.Instance.LoadData ();
	}

	// because member has not change frequency,so just keep the last query will be better than hash.
	public int GetIDWith(GroupMemberRepresent member)
	{
		bool hasfound =false;
		int resultID = 0;
		if (_lastQueryGroupMember != null && IsEqual(member, _lastQueryGroupMember))
		{
			resultID = _lastQueryID;
		}
		else
		{
			for (int i = 0; i < _sheet.DataArray.Length; i++) 
			{
				GroupRuleData temp = _sheet.dataArray [i];
				if (IsEqual(member, temp))
				{
					Debug.Assert(!hasfound, "Group Rule Excel has conflict");
					hasfound = true;
					_lastQueryID = temp.ID;
					#if RELEASE
					break;
					#endif
				}
			}
		}

		return resultID;
	}

	public bool IsEqual(GroupMemberRepresent member1,GroupMemberRepresent member2)
	{
		bool result = true;
		if(member1.PayAmount!=member2.PayAmount)
			result = false;
		else if(member1.PayCount!=member2.PayCount)
			result = false;	
		else if(member1.HasPayInValidPeriod!=member2.HasPayInValidPeriod)
			result = false;
		else if(member1.HistoryMaxPay!=member2.HistoryMaxPay)
			result = false;
		return result;
	}

	public bool IsEqual(GroupMemberRepresent member,GroupRuleData data)
	{
		bool result = true;
		if(!data.PayAmout.Contains(member.PayAmount))
			result = false;
		else if(!data.PayCount.Contains(member.PayCount))
			result = false;	
		else if(!data.HasPayInWeek.Contains(member.HasPayInValidPeriod))
			result = false;
		else if(!data.HistoryMaxPaid.Contains(member.HistoryMaxPay))
			result = false;
		if (result)
		{
			_lastQueryGroupMember = member;
			_lastQueryID = data.ID;
		}
		return result;
	}


	//because usergroup will be call so many times and group ID will increase so get a hash to make it fast
	//maybe make the hash value to be some to the group ID will be more fast,but it is not as robustness as this
	/*void recpy() 
	{
		payinweekweight = 1;
		countsweight = GroupRepresentConfig.Instance.GetHasPayInWeekLen();
		amoutweight = countsweight*GroupRepresentConfig.Instance.GetPayCountLen();
		hash = new int[MaxLen ()]; 
		for (int i = 0; i < _sheet.DataArray.Length; i++) 
		{
			GroupRuleData temp = _sheet.dataArray [i];
			int key = GetKeyInPresent(temp.HasPayInWeek,temp.PayCount,temp.PayAmout);
			if (key < MaxLen ()) 
				hash [key] = _sheet.DataArray [i].ID;
			else
				Debug.Assert (false, "Error GroupRuleExel");
			
		}
	}

	public int GetIDWith(DateTime lastpaytime,int paycounts,float payamout)
	{
		return hash [GetKey (lastpaytime, paycounts, payamout)];
	}
		
	int GetKey(DateTime lastpaytime,int paycounts,float payamout)
	{
		int payinweekpresent = GroupRepresentConfig.Instance.GetHasPayInValidPeriodRepresent (lastpaytime);
		int countsrepresent = GroupRepresentConfig.Instance.GetPayCountRepresent (paycounts);
		int amoutrepresent = GroupRepresentConfig.Instance.GetPayAmountRepresent((int)payamout);
		return GetKeyInPresent (payinweekpresent, countsrepresent, amoutrepresent);
	}

	int GetKeyInPresent(int payinweekpresent,int countsrepresent,int amoutrepresent)
	{
		int result = 0;
		result += payinweekweight * payinweekpresent;
		result += countsrepresent * countsweight;
		result += amoutrepresent * amoutweight;
		return result;
	}

	int MaxLen()
	{
		return amoutweight * GroupRepresentConfig.Instance.GetPayAmoutLen();
	}
	*/
}

