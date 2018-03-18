using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class LastQuery
{
	public int ID;
	public int ActiveID;
	public LastQuery()
	{
		ID = -1;
		ActiveID = -1;
	}
}

public class GroupConfig : SimpleSingleton <GroupConfig>
{
	public static readonly string Name = "Group";

	private GroupSheet _sheet;

	private int _noProduct = 0;

	private LastQuery _lastQuery = new LastQuery();

	private GroupData _lastGroupData;

	private int NewBie = 1;

	public GroupConfig()
	{
		LoadData ();
	}

	private void LoadData()
	{
		_sheet = GameConfig.Instance.LoadExcelAsset<GroupSheet>(Name);
	}
		
	public static void Reload()
	{
		Debug.Log("Reload Group");
		GroupConfig.Instance.LoadData ();
	}

	public bool IsProductExist(StoreType type,int ID = -1)
	{
		if (ID == -1)
			ID = GetUserID ();
		if (IsArray(type)) 
		{
			return IsProductArrayExist(type, ID);
		}
		else
		{
			return GetProductID (type, ID) > 0;
		}
	}

    public int GetPayUserGroupId(int ID = -1, int activeID = -1)
    {
        ResetID(ref ID, ref activeID);
        return GetDataWith(ID, activeID).PayUserGroupId;
    }

    public int GetAdStrategyId(int ID = -1, int activeID = -1)
    {
        int result = 0;
        ResetID(ref ID, ref activeID);
        if (GetUserID() == NewBie)
            result = AdjustManager.Instance.GetAdStrategyId();
        else
            result = GetDataWith(ID, activeID).AdStrategy;

        return result;
    }


    public List<int> GetAvailabelWindows(int ID = -1, int activeID = -1)
    {
        ResetID(ref ID, ref activeID);
        return new List<int>(GetDataWith(ID, activeID).AvailableWindows);
    }

    public List<int> GetNotificationID(int ID = -1,int activeID = -1)
	{
		ResetID(ref ID,ref activeID);
		return new List<int>(GetDataWith(ID, activeID).Notification);
	}
		
	public int GetProductID (StoreType type,int ID = -1,int activeID = -1) 
	{
		int result = 0;
		if (ID == -1)
			ID = GetUserID ();
		if (activeID == -1)
			activeID = GetActiveID();
		if (type == StoreType.SpecialOffer)
			result = GetSpecialOffer (ID,activeID);
		else if (type == StoreType.Deal_TL)
			result = GetLimitTimeAPID (ID,activeID);
		else if (type == StoreType.WheelOfLuck)
			result = GetWheelOfLuck (ID,activeID);
		else if (type == StoreType.CrazyDice)
			result = GetDice (ID,activeID);
		return result;
	}

	public int[] GetProductIDArray(StoreType type,int ID = -1,int activeID = -1)
	{
		int[] result = new int[0];
		if (ID == -1)
			ID = GetUserID ();
		if (activeID == -1)
			activeID = GetActiveID();
		if (type == StoreType.Buy)
			result = GetBuy(ID,activeID);
		else if (type == StoreType.Deal)
			result = GetDeal(ID,activeID);
		else if (type == StoreType.SmallBuy)
			result = GetSmallBuy(ID,activeID);
		return result;
	}

	void ResetID(ref int ID,ref int activeID)
	{
		if (ID == -1)
			ID = GetUserID ();
		if (activeID == -1)
			activeID = GetActiveID();
	}

	bool IsProductArrayExist(StoreType type,int ID)
	{
		return !ListUtility.IsContainElement(GetProductIDArray(type, ID), _noProduct);
	}

	bool IsArray(StoreType type)
	{
		return type == StoreType.SmallBuy || type == StoreType.Buy || type == StoreType.Deal;
	}

    int GetUserID()
	{
		return UserGroup.GetUserGroupID ();
	}

	public int GetActiveID()
	{
		if (GetUserID() == NewBie)
			return NewBie;
		else
			return ActiveGroupRuleConfig.Instance.GetIDWith(new GroupMemberRepresent (new GroupMember ()));
	}

	int[] GetSmallBuy(int ID , int activeID)
	{
		return GetDataWith (ID,activeID).SmallBuy;
	}

	int[] GetBuy(int ID,int activeID)
	{
		return GetDataWith(ID,activeID).Buy;
	}

	int[] GetDeal(int ID,int activeID)
	{
		return GetDataWith(ID,activeID).Deal;
	}

	int GetLimitTimeAPID(int ID,int activeID)
	{
		return GetDataWith (ID,activeID).LimitedTimeAPID;
	}
		
	int GetWheelOfLuck(int ID,int activeID)
	{
		return GetDataWith (ID,activeID).WheelOfLuck;
	}

	int GetDice(int ID,int activeID)
	{
		return GetDataWith (ID,activeID).Dice;
	}

	int GetSpecialOffer(int ID,int activeID)
	{
		return GetDataWith (ID,activeID).SpecialOffer;
	}

    GroupData GetDataWith(int ID,int activeID)
	{
		GroupData result;
		if (_lastQuery.ID == ID && activeID == _lastQuery.ActiveID && _lastGroupData != null)
		{
			result = _lastGroupData;
		}
		else
		{
			int index = ListUtility.Find(_sheet.dataArray, (GroupData data) => {
				return data.GroupID.Contains(ID.ToString()) && data.GroupID.Contains(activeID.ToString());
			});
			if (index >= 0)
			{
				result = _sheet.dataArray [index];
				_lastQuery.ID = ID;
				_lastQuery.ActiveID = activeID;
				_lastGroupData = result;
			}
			else 
			{
				result = new GroupData();
			}
			_lastGroupData = result;
		}
		return result;
	}
}

