using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreAnalysisData
{
	public Dictionary<string, object> AnalysisDic;

	private string _storeEntrance = "Buy";
	private string _openPosition = "Lobby";
	private string _localItemId = "";
	private string _storeSpecificId = "";
	private string _buyStatusInformation = "";
	private double _realMoney = 0;
	private string _transactionId = "";
	private string _orderID = "";// 追踪用ID (deviceId + 商品创建时间)


	public string StoreEntrance
	{
		get
		{
			return _storeEntrance;
		}

		set
		{
			_storeEntrance = value;
			AnalysisDic["str0"] = _storeEntrance;
		}
	}

	public string OpenPosition
	{
		get
		{
			return _openPosition;
		}

		set
		{
			_openPosition = value;
			AnalysisDic["str1"] = _openPosition;
		}
	}

	public string LocalItemId
	{
		get
		{
			return _localItemId;
		}

		set
		{
			_localItemId = value;
			AnalysisDic["str2"] = _localItemId;
		}
	}

	public string StoreSpecificId
	{
		get
		{
			return _storeSpecificId;
		}

		set
		{
			_storeSpecificId = value;
			AnalysisDic["str3"] = _storeSpecificId;
		}
	}

	public string BuyStatusInformation
	{
		get
		{
			return _buyStatusInformation;
		}

		set
		{
			_buyStatusInformation = value;
			AnalysisDic["str5"] = _buyStatusInformation;
		}
	}

	public double RealMoney
	{
		get
		{
			return _realMoney;
		}

		set
		{
			_realMoney = value;
			AnalysisDic["double0"] = _realMoney;
		}
	}

	public string TransactionId
	{
		get {
			return _transactionId;
		}
		set {
			_transactionId = value;
			AnalysisDic["str7"] = _transactionId;
		}
	}

	public string OrderID{
		get { return _orderID; }
		set {
			_orderID = value;
			AnalysisDic["str8"] = value;
		}
	}

	public StoreAnalysisData()
	{
		AnalysisDic = new Dictionary<string, object>();
		AnalysisDic["str0"] = StoreEntrance;
		AnalysisDic["str1"] = OpenPosition;
		AnalysisDic["str2"] = LocalItemId;
		AnalysisDic["str3"] = StoreSpecificId;
		AnalysisDic["str5"] = BuyStatusInformation;
		AnalysisDic["str7"] = TransactionId;
		AnalysisDic["str8"] = OrderID;
		AnalysisDic["double0"] = RealMoney;
	}

	public void FillData(Dictionary<string, object> dic)
	{
		#if DEBUG
		string logData = "";
		#endif

		foreach(var pair in AnalysisDic)
		{
			string k = pair.Key;
			object v = pair.Value;
			if(v != null && !string.IsNullOrEmpty(v.ToString()))
			{
				dic[k] = v;

				#if DEBUG
				logData += k + ":" + v + " , ";
				#endif
			}
		}

		#if DEBUG
		Debug.Log("StoreAnalysisData FillData: " + logData);
		#endif

		dic["integer0"] = UserBasicData.Instance.PiggyBankCoins;

		if (!_localItemId.IsNullOrEmpty()){
			IAPCatalogData iapT = IAPCatalogConfig.Instance.FindIAPItemByID(_localItemId);
			if (iapT != null){
				float credits = IAPCatalogConfig.Instance.GetCreditsWithPromotion(iapT);
				float creditsWithoutPromotion = IAPCatalogConfig.Instance.GetCreditsWithoutPromotion(iapT);
				int lucky = iapT.CreditsAddLongLucky * (int)creditsWithoutPromotion;
				dic["integer2"] = credits;
				dic["integer3"] = lucky;
                dic["str9"] = IAPCatalogConfig.Instance.ShouldPromoteItem(iapT).ToString();
            }
		}
	}
}
