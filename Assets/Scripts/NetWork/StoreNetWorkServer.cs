using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CitrusFramework;
using UnityEngine;
using UnityEngine.Purchasing;

public class StoreNetWorkServer : Singleton<StoreNetWorkServer>
{
	static readonly float _verifyTimeoutTime = 60.0f;

	private bool _isRevalidationing = false;
	public bool IsRevalidationing { get { return _isRevalidationing; } }

	#region Verify

	public void StartVerifyPurchase(IAPData iapData)
	{
		StartCoroutine(VerifyPurchaseCoroutine(iapData));
	}

	IEnumerator VerifyPurchaseCoroutine(IAPData iapData)
	{
		AnalysisManager.Instance.StartVerify();
		string receipt = iapData.Receipt;
		Dictionary<string, string> extra = iapData.tokens;
		string platform = PlatformManager.Instance.GetPlatformString();

		#if UNITY_ANDROID || UNITY_EDITOR
		string key = extra["signature"];
		string receiptData = extra["originalJson"];

		Dictionary<string, object> dic = new Dictionary<string, object>();
		dic.Add("ProjectName", BuildUtility.GetProjectName());
		dic.Add("Platform", platform);
		dic.Add("receiptData", receiptData);
		dic.Add("sign", key);

		//add verify info for server track event
		AddVerifyForServerTrack(dic, iapData);

		string jsonStr = MiniJSON.Json.Serialize(dic);
		Debug.Log("Verify jsonstring:" + jsonStr);
		WWW www = new WWW(ServerConfig.PayServerUrl + "check_google_receipt", Encoding.UTF8.GetBytes(jsonStr));

		#elif UNITY_IOS
		Dictionary<string, object> dic = new Dictionary<string, object> ();
		dic.Add ("ProjectName", BuildUtility.GetProjectName());
		dic.Add("Platform", "apple");
		// dic.Add ("Platform", platform);
		Dictionary<string, object> payment = new Dictionary<string, object> ();
		payment.Add ("receipt", extra["receiptBase64"]);
		payment.Add ("productId", iapData.StoreSpecificId);
		dic.Add ("receiptData", payment);

		//add verify info for server track event
		AddVerifyForServerTrack(dic, iapData);

		string jsonStr = MiniJSON.Json.Serialize (dic);
		GameDebug.Log (jsonStr);
		Dictionary<string, string> headers = new Dictionary<string, string> ();
		headers.Add ("Content-Type", "application/json");
		WWW www = new WWW (ServerConfig.PayServerUrl + "purchaseitem",Encoding.UTF8.GetBytes(jsonStr), headers);
		#endif

		UserDeviceLocalData.Instance.SetIAPState(StoreManager.Instance.currentReceiptID, IAPData.IAPState.BeginVerify);
		float timer = 0;
		bool failed = false;
		while(!www.isDone && string.IsNullOrEmpty(www.error))
		{
			if(timer > _verifyTimeoutTime)
			{
				failed = true;
				break;
			}
			timer += Time.deltaTime;
			yield return null;
		}

		if(failed || !string.IsNullOrEmpty(www.error))
		{
			//when time out , I am still assuming the order is valid
			//if I have some www error, I still assume the order is valid
			Debug.LogError("There is a time out in this purchase");
			if(extra != null)
			{
				//todo by nichos: here is not good enough
				if(Application.internetReachability == NetworkReachability.NotReachable)
				{
					StoreController.Instance.CurrStoreAnalysisData.BuyStatusInformation = NetworkReachability.NotReachable.ToString();
					AnalysisManager.Instance.FailVerify("Internet Not Reachable");
					//Verify next time
					StoreManager.Instance.ServerErrorNextTimeReceipt(StoreManager.Instance.currentReceiptID, iapData.LocalItemId);
				}
				else
				{
					string success = "";
					if(failed)
					{
						StoreController.Instance.CurrStoreAnalysisData.BuyStatusInformation = "TimeOut";
						success = "Timeout";
					}
					else
					{
						StoreController.Instance.CurrStoreAnalysisData.BuyStatusInformation = www.error;
						success = www.error;
					}

					LogUtility.Log("Verify success", Color.blue);
					StoreManager.Instance.ReceiveIAPPackSuccess(iapData);

					//when timeout, we don't care if it a sandbox account
					SDKEventManager.Instance.OnPurchaseVerifySuccess(false, iapData, "Verify success");

					StoreManager.Instance.currentReceiptID = "";
				}
			}
		}
		else
		{
			Debug.Log("www respond:" + www.text);
			var result = new JSONObject(www.text);

			#if UNITY_ANDROID
			if(result.GetField("errorCode").n == 0)
			#else
			if(result.GetField ("verified").str == "success")
			#endif
			{
				// verify success
				Debug.Log("Verify success");
				StoreController.Instance.CurrStoreAnalysisData.BuyStatusInformation = "VerifySuccess";
                //filter sandbox purchase data when verify success
			    bool isSandbox = result.HasField("IsSandbox") && result.GetField("IsSandbox").b;
                //the sanbox flag is only vertify appstore account which server send to client, so we need to vertify gp account locally
				
				#if UNITY_ANDROID
				if (iapData.tokens.ContainsKey("order"))
            	{
                	isSandbox = iapData.tokens["order"].IsNullOrEmpty();
                	LogUtility.Log("StoreModule : android purchase : order id: " + iapData.tokens["order"]);
            	}
				#endif
                
				StoreManager.Instance.ReceiveIAPPackSuccess(iapData);

				SDKEventManager.Instance.OnPurchaseVerifySuccess(isSandbox, iapData, "VerifyOK");
				StoreManager.Instance.currentReceiptID = "";
			}
			else
			{
				Debug.Log("Verify fail");
				StoreController.Instance.CurrStoreAnalysisData.BuyStatusInformation = result.HasField ("errorCode") ? 
					result.GetField("errorCode").ToString() : "none errorCode";
				
				#if UNITY_ANDROID
				ServerErrorCode errorCode = (ServerErrorCode)result.GetField("errorCode").n;
				if (errorCode !=  null){
					AnalysisManager.Instance.FailVerify(errorCode.ToString());
				}
				else{
					LogUtility.Log("ServerErrorCode is invalid : " + errorCode.ToString(), Color.red);
				}
				#else
				AnalysisManager.Instance.FailVerify();
				#endif

				IAPData data = UserDeviceLocalData.Instance.GetIAPData(StoreManager.Instance.currentReceiptID);
				if (data != null)
				{
					string ItemID = data.LocalItemId;
					StoreManager.Instance.OnPurchaseFailedEvent.Invoke(false, iapData, PurchaseFailureReason.Unknown);
				}
				UserDeviceLocalData.Instance.SetIAPState(StoreManager.Instance.currentReceiptID, IAPData.IAPState.Failed);
				StoreManager.Instance.currentReceiptID = "";
			}
		}
	}

	void AddVerifyForServerTrack(Dictionary<string, object> dict, IAPData iapData)
	{
		string channel = PlatformManager.Instance.GetServerChannelString();
		IAPCatalogData catalogData = IAPCatalogConfig.Instance.FindIAPItemByID(iapData.LocalItemId);
		string appVersion = BuildUtility.GetBundleVersion();
		string userId = AnalysisManager.Instance.GetTrackUserId();

		#if DEBUG
		int isDebug = 1;
		#else
		int isDebug = 0;
		#endif

		dict.Add("ItemId", iapData.StoreSpecificId);
		dict.Add("TransactionId", iapData.TransactionId);
		dict.Add("Channel", channel);
		dict.Add("Price", catalogData.Price);
		dict.Add("AppVersion", appVersion);
		dict.Add("UserId", userId);
		dict.Add("IsDebug", isDebug);
	}

	#endregion

	#region Revalidate

	public void TryStartRevalidation()
	{
		if(ShouldStartRevalidate())
			StartCoroutine(StartRevalidateCoroutine());
	}

	bool ShouldStartRevalidate()
	{
		bool result = UserDeviceLocalData.Instance.VerityIAPDic.Count > 0;
		return result;
	}

	IEnumerator StartRevalidateCoroutine()
	{
		if(_isRevalidationing)
		{
			Debug.LogError("StartRevalidateCoroutine: _isRevalidationing is true, should not be here");
			yield break;
		}

		//wait until StoreManager.Controller is init and non-null, in case we would use Controller later
		while(!DeviceUtility.IsConnectInternet() || !StoreManager.Instance.IsInitialized)
		{
			yield return new WaitForSeconds(1.0f);
		}
		
		_isRevalidationing = true;
		yield return StartCoroutine(PerformRevalidateCoroutine());
		_isRevalidationing = false;
	}

	IEnumerator PerformRevalidateCoroutine()
	{
		if(!DeviceUtility.IsConnectInternet())
		{
			Debug.LogError("PerformRevalidateCoroutine: No internet, this function should not be called");
			yield break;
		}

		Debug.Log("Start revalidate product count: " + UserDeviceLocalData.Instance.VerityIAPDic.Count);
		//if I still have anything in the IAP dic, then I need to deal with them one by one. Ideally, there is only one IAP left
		while(UserDeviceLocalData.Instance.VerityIAPDic.Count > 0)
		{
			IAPData data = UserDeviceLocalData.Instance.VerityIAPDic.First().Value;

            Debug.Log("Revalidate product: " + data.LocalItemId + ", " + data.Receipt);
			if(data.State != IAPData.IAPState.FinishPurchase
				&& data.State != IAPData.IAPState.BeginVerify 
				&& data.State != IAPData.IAPState.BeginPurchase)
			{
				UserDeviceLocalData.Instance.VerityIAPDic.Remove(data.Receipt);
				LogUtility.Log("State is not correct: " + data.State, Color.red);
			}
			else if(data.tokens.Count == 0)
			{
				LogUtility.Log("Tokens is empty", Color.red);
				UserDeviceLocalData.Instance.VerityIAPDic.Remove(data.Receipt);
			}
			else
			{
				LogUtility.Log("Handling revalidate item", Color.green);

				if(StoreManager.Instance.currentReceiptID == data.Receipt)
				{
					StoreManager.Instance.currentReceiptID = "";
					Debug.LogError("PerformRevalidateCoroutine: should not be here?");
					break;
				}

				StoreManager.Instance.currentReceiptID = data.Receipt;

				var AnalysisData = new StoreAnalysisData();
				AnalysisData.OpenPosition = "Auto";
				AnalysisData.LocalItemId = data.LocalItemId;
				AnalysisData.StoreSpecificId = data.StoreSpecificId;
				AnalysisData.RealMoney = IAPCatalogConfig.Instance.FindIAPItemByID(data.LocalItemId).Price;
				AnalysisData.TransactionId = data.TransactionId;
				AnalysisData.OrderID = DeviceUtility.GetDeviceId() + "_" + System.DateTime.Now + "_Revalidate";
				StoreController.Instance.CurrStoreAnalysisData = AnalysisData;

				yield return StartCoroutine(VerifyPurchaseCoroutine(data));
			}

			yield return new WaitForEndOfFrame();
		}

		StoreManager.Instance.currentReceiptID = "";
		UserDeviceLocalData.Instance.Save();
	}

	#endregion
}
