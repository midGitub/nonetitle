using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ActiveGroupRuleConfig : SimpleSingleton<ActiveGroupRuleConfig>
{
	public static readonly string Name = "GroupRule2";
	private GroupRule2Sheet _sheet;
	private GroupMemberRepresent _lastQueryGroupMember;
	private int _lastQueryID;
	public ActiveGroupRuleConfig()
	{
		LoadData ();
	}

	private void LoadData()
	{
		_sheet = GameConfig.Instance.LoadExcelAsset<GroupRule2Sheet>(Name);
	}

	public static void Reload()
	{
		Debug.Log("Reload GroupRule");
		ActiveGroupRuleConfig.Instance.LoadData ();
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
				GroupRule2Data temp = _sheet.dataArray [i];
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
		if(member1.SpinAverage != member2.SpinAverage)
			result = false;	
		else if(member1.SessionAverage != member2.SessionAverage)
			result = false;
		return result;
	}

	public bool IsEqual(GroupMemberRepresent member,GroupRule2Data data)
	{
		bool result = true;
		if(!data.Spin.Contains(member.SpinAverage))
			result = false;	
		else if(!data.Session.Contains(member.SessionAverage))
			result = false;
		if (result)
		{
			_lastQueryGroupMember = member;
			_lastQueryID = data.ID;
		}
		return result;
	}
		
}

