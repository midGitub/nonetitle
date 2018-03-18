using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IAPData
{
	public enum IAPState
	{
		BeginPurchase = 0,
		FinishPurchase,
		BeginVerify,
		Verified,
		Failed,             //cheat
		DelayPurchase = 8,
		CancelPurchase,
		End
	}

	public IAPData(string r, string localItemId, string storeSpecificId = "", string transactionId = "", IAPState s = IAPState.BeginPurchase, long t = -1, Dictionary<string, string> extra = null)
	{
		Receipt = r;
		LocalItemId = localItemId;
		StoreSpecificId = storeSpecificId;
		TransactionId = transactionId;
		mState = s;
		mUpdateTime = t > 0 ? t : NetworkTimeHelper.Instance.GetNowTime().Ticks;
		if(extra != null)
			tokens = extra;
	}

	public string Receipt;
	public string LocalItemId; //diamondId
	public string StoreSpecificId; //the id from gp or apple store
	public string TransactionId; //transactionId from gp or apple store

	IAPState mState;
	public IAPState State
	{
		set
		{
			mState = value;
			mUpdateTime = NetworkTimeHelper.Instance.GetNowTime().Ticks;
		}
		get
		{
			return mState;
		}
	}

	long mUpdateTime;
	public long UpdateTime
	{
		get
		{
			return mUpdateTime;
		}
	}
	public Dictionary<string, string> tokens = new Dictionary<string, string>();
}


