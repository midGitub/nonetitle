using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CitrusFramework;
using Facebook.Unity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

[System.Serializable]
public class StartPrePurchaseEvent : UnityEvent<string> 
{
}

[System.Serializable]
public class StartPurchaseEvent : UnityEvent<string> 
{
}

[System.Serializable]
public class OnPurchaseCompletedEvent : UnityEvent<IAPData>
{
}

[System.Serializable]
public class OnPurchaseFailedEvent : UnityEvent<bool, IAPData, PurchaseFailureReason>
{
}

public class StoreManager : IStoreListener
{
	private static int _initRetryCount = 0;
	private static readonly int InitRetryCountMax = 3;

	private static StoreManager _instance = new StoreManager();
	public static StoreManager Instance { get { return _instance; } }

	//StartPrePurchaseEvent is used only for this case:
	//when starting purchase, but UnityPurchasing is not inited, so init UnityPurchasing first, 
	//then start purchase. When start init UnityPurchasing in this case, the event occurs
	public StartPrePurchaseEvent StartPrePurchaseEvent = new StartPrePurchaseEvent();
	public StartPurchaseEvent StartPurchaseEvent = new StartPurchaseEvent();
	public OnPurchaseCompletedEvent OnPurchaseCompletedEvent = new OnPurchaseCompletedEvent();
	public OnPurchaseFailedEvent OnPurchaseFailedEvent = new OnPurchaseFailedEvent();

	//when initialized, call the event once and then remove all listeners
	private UnityEvent OnStoreInitializedEvent = new UnityEvent();

	private IStoreController _controller;
	private IExtensionProvider _extensions;
	private ConfigurationBuilder _builder;

	private ProductCatalog _catalog;

	public string currentReceiptID = "";

	//Important note by nichos:
	//This field is null before UnityPurchasing module is initialized.
	//So don't forget to check it if it's null if you use it
	public IStoreController Controller
	{
		get { return _controller; }
	}

	public IExtensionProvider ExtensionProvider
	{
		get { return _extensions; }
	}

	public bool IsInitialized
	{
		get { return _controller != null; }
	}

	#region Init

	private StoreManager()
	{
		_catalog = ProductCatalog.LoadDefaultCatalog();

		StandardPurchasingModule module = StandardPurchasingModule.Instance();
		module.useFakeStoreUIMode = FakeStoreUIMode.StandardUser;
		_builder = ConfigurationBuilder.Instance(module);

		foreach(var product in _catalog.allProducts)
		{
			if(product.allStoreIDs.Count > 0)
			{
				var ids = new IDs();
				foreach(var storeID in product.allStoreIDs)
				{
					ids.Add(storeID.id, storeID.store);
				}
				_builder.AddProduct(product.id, product.type, ids);
			}
			else {
				_builder.AddProduct(product.id, product.type);
			}
		}

		foreach(var myItme in IAPCatalogConfig.Instance.ListSheet)
		{
			_builder.AddProduct(myItme.ID, (ProductType)myItme.ProductType, new IDs {
				{myItme.GPStoreID, GooglePlay.Name},
				{myItme.APStoreID, AppleAppStore.Name}
			});
		}

		InitStore();
	}

	public void InitStore()
	{
		if(_controller == null)
		{
			Debug.Log("InitStore called");
			_initRetryCount = 0;
			UnityPurchasing.Initialize(this, _builder);
		}
	}

	#endregion

	#region Start purchase

	public void InitiatePurchase(string productID)
	{
		if(!DeviceUtility.IsConnectInternet())
		{
			Debug.LogError("Purchase failed because no network");
			NoCollectionErrorObject.Instance.Show();
			return;
		}

		if(StoreNetWorkServer.Instance.IsRevalidationing)
		{
			Debug.LogError("It's still revalidating, please wait until revalidate ends");
			string text = LocalizationConfig.Instance.GetValue("purchase_validating");
			WarningLayer.ShowWarningLayer(text);
			return;
		}

		UnityAction purchaseAction = () => {
			var r = NetworkTimeHelper.Instance.GetNowTime().Ticks.ToString();
			r = DeviceUtility.GetDeviceId() + "_" + r;

			Product iapProduct = GetProductById(productID);

			currentReceiptID = r;
			var data = new IAPData(r, productID);
			UserDeviceLocalData.Instance.SetIAPData(data);

			StartPurchaseEvent.Invoke(productID);
			_controller.InitiatePurchase(productID);

			Debug.Log("StoreManager InitiatePurchase: " + currentReceiptID);
		};

		if(_controller == null)
		{
			Debug.Log("Init store first before purchase");

			OnStoreInitializedEvent.AddListener(purchaseAction);
			StartPrePurchaseEvent.Invoke(productID);
			InitStore();
		}
		else
		{
			purchaseAction.Invoke();
		}
	}

	#endregion

	#region Callbacks

	public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
	{
		LogUtility.Log("StoreManager: initialize succeess", Color.red);
		this._controller = controller;
		this._extensions = extensions;
		OnStoreInitializedEvent.Invoke();
		OnStoreInitializedEvent.RemoveAllListeners();
	}

	public void OnInitializeFailed(InitializationFailureReason error)
	{
		Debug.LogError(string.Format("StoreManager: initialize fail: {0}", error.ToString()));
		if (_initRetryCount < InitRetryCountMax)
		{
			UnityPurchasing.Initialize (this, _builder);
			++_initRetryCount;
		}
	}

	public void OnPurchaseFailed(Product i, PurchaseFailureReason error)
	{
		Debug.LogError("Purchase Failed: " + error.ToString());

		//Critical patch by Nichos:
		//On android, after initiating one purchase, there might be two callbacks, one success, one fail respectively.
		//To fix this bug, ignore the second fail callback if the state is not we expected
		IAPData.IAPState curState = UserDeviceLocalData.Instance.GetIAPState(currentReceiptID);
		if(curState == IAPData.IAPState.BeginPurchase)
		{
			Debug.Log("It should be a fail purchase");

			OnPurchaseFailedEvent.Invoke(false, UserDeviceLocalData.Instance.VerityIAPDic[currentReceiptID], error);
			UserDeviceLocalData.Instance.SetIAPState(currentReceiptID, IAPData.IAPState.Failed);
			currentReceiptID = "";

			StoreController.Instance.CurrStoreAnalysisData.BuyStatusInformation = error.ToString();
			StoreController.Instance.CurrStoreAnalysisData.TransactionId = i.transactionID;
			AnalysisManager.Instance.ChannelFailPurchase(error.ToString());
		}
		else
		{
			LogUtility.Log("Receipt state is wrong, so ignore it: " + curState.ToString(), Color.magenta);
		}
	}

	public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
	{
		bool validPurchase = true;
		string error = "";

		AnalysisManager.Instance.ChannelSuccessPurchase();

		// Unlock the appropriate content here.
		Debug.Log("StoreManager ProcessPurchase OK:" + e.purchasedProduct.definition.storeSpecificId);
		Debug.Log("Receipt: " + e.purchasedProduct.receipt);

		#if UNITY_EDITOR

		SkipVerifyToSuccessPurchase();
		return PurchaseProcessingResult.Complete;

		#elif UNITY_ANDROID || UNITY_IOS

		// Prepare the validator with the secrets we prepared in the Editor
		// obfuscation window.
		var validator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), Application.bundleIdentifier);

		try
		{
			// On Google Play, result has a single product ID.
			// On Apple stores, receipts contain multiple products.
			var result = validator.Validate(e.purchasedProduct.receipt);

			Debug.Log("Receipt is valid. Contents:");
			foreach (IPurchaseReceipt productReceipt in result)
			{
				Debug.Log(productReceipt.productID + ", " + productReceipt.transactionID);
			}
		}
		catch (IAPSecurityException ex)
		{
			Debug.Assert(false,ex.Message);
			error = "Validate error:" + ex.Message;
			Debug.Log("Invalid receipt, not unlocking content");
			validPurchase = false;
		}

		#endif

		if(validPurchase)
		{
			// Note by nichos:
			// I don't know why this piece of code is written here, but I think it's useless
			// So comment it for some time before we confirm it can be removed
//			if(!StoreNetWorkServer.Instance.IsRevalidationing && currentReceiptID == "")
//			{
//				var AnalysisData = new StoreAnalysisData();
//				AnalysisData.OpenPosition = "Auto";
//				AnalysisData.LocalItemId = e.purchasedProduct.definition.id;
//				AnalysisData.StoreSpecificId = e.purchasedProduct.definition.storeSpecificId;
//				AnalysisData.RealMoney = IAPCatalogConfig.Instance.FindIAPItemByID(e.purchasedProduct.definition.id).Price;
//				AnalysisData.OrderID = DeviceUtility.GetDeviceId() + "_" + System.DateTime.Now;
//				StoreController.Instance.CurrStoreAnalysisData = AnalysisData;
//			}

			Debug.Assert(!string.IsNullOrEmpty(currentReceiptID));

			if(MapSettingConfig.Instance.IsSkipVerifyPurchase)
				SkipVerifyToSuccessPurchase();
			else
				ProcessVerifyPurchase(e);
		}
		else
		{
			Debug.LogError("Not valid purchase");
			
			StoreController.Instance.CurrStoreAnalysisData.BuyStatusInformation = "NotValidPurchase";
			AnalysisManager.Instance.ChannelFailPurchase(error);
			
			OnPurchaseFailedEvent.Invoke(false, UserDeviceLocalData.Instance.VerityIAPDic[currentReceiptID], PurchaseFailureReason.Unknown);
			UserDeviceLocalData.Instance.SetIAPState(currentReceiptID, IAPData.IAPState.Failed);
			currentReceiptID = "";
		}

		return PurchaseProcessingResult.Complete;
	}

	void SkipVerifyToSuccessPurchase()
	{
		Debug.Log("SkipVerifyToSuccessPurchase called");

		ReceiveIAPPackSuccess(UserDeviceLocalData.Instance.VerityIAPDic[currentReceiptID]);
		UserDeviceLocalData.Instance.SetIAPState(currentReceiptID, IAPData.IAPState.Verified);

		AnalysisManager.Instance.SkipVerifyAndSuccessPurchase();

		currentReceiptID = "";
	}

	void ProcessVerifyPurchase(PurchaseEventArgs e)
	{
		Dictionary<string, object> receiptDic = MiniJSON.Json.Deserialize(e.purchasedProduct.receipt) as Dictionary<string, object>;

		//get transactionId here
		string transactionId = receiptDic["TransactionID"] as string;
		StoreController.Instance.CurrStoreAnalysisData.TransactionId = transactionId;

		Dictionary<string, string> extra = new Dictionary<string, string>();

		#if UNITY_ANDROID
		Dictionary<string, object> payload = MiniJSON.Json.Deserialize(receiptDic["Payload"] as string) as Dictionary<string, object>;
		extra.Add("signature", payload["signature"] as string);
		extra.Add("originalJson", payload["json"] as string);
		#elif UNITY_IOS
		extra.Add("receiptBase64", receiptDic["Payload"] as string);
		#endif

		IAPData data;
		if(!StoreNetWorkServer.Instance.IsRevalidationing && currentReceiptID != "")
		{
			data = UserDeviceLocalData.Instance.VerityIAPDic[currentReceiptID];
			LogUtility.Log("Start verify in normal workflow", Color.blue);
		}
		else
		{
			var r = DeviceUtility.GetDeviceId() + "_" + NetworkTimeHelper.Instance.GetNowTime().Ticks.ToString();
			data = new IAPData(r, e.purchasedProduct.definition.id);
			UserDeviceLocalData.Instance.SetIAPData(data);
			currentReceiptID = r;
			LogUtility.Log("Start verify from revalidating", Color.blue);
		}

		//record more important infos
		data.tokens = extra;
		data.StoreSpecificId = e.purchasedProduct.definition.storeSpecificId;
		data.TransactionId = transactionId;

		UserDeviceLocalData.Instance.SetIAPState(data.Receipt, IAPData.IAPState.FinishPurchase);

		StoreNetWorkServer.Instance.StartVerifyPurchase(data);
	}

	#endregion

	#region Purchase result

	public void ServerErrorNextTimeReceipt(string receipt, string itemId)
	{
		Debug.Log("ServerErrorNextTimeReceipt called: " + itemId);
		UserDeviceLocalData.Instance.SetIAPState(currentReceiptID, IAPData.IAPState.BeginVerify);
		currentReceiptID = "";
		if(UserDeviceLocalData.Instance.GetReceiptWaitingState(receipt))
		{
			OnPurchaseFailedEvent.Invoke(true, UserDeviceLocalData.Instance.VerityIAPDic[receipt], PurchaseFailureReason.Unknown);
			Debug.Log("ServerErrorNextTimeReceipt: purchase failed");
		}
	}

	public void ReceiveIAPPackSuccess(IAPData data)
	{
		Debug.Log("ReceiveIAPPackSuccess called:" + data.LocalItemId);
		OnPurchaseCompletedEvent.Invoke(data);
		Debug.Log("ReceiveIAPPackSuccess ok");
	}

	#endregion

	#region Misc

	public bool HasProductInCatalog(string productID)
	{
		foreach(var product in _catalog.allProducts)
		{
			if(product.id == productID)
				return true;
		}
		return false;
	}

	public Product GetProduct(string productID)
	{
		Product result = null;
		if(_controller != null)
			result = _controller.products.WithID(productID);
		return result;
	}

	public Product GetProductById(string product)
	{
		Product result = null;
		if(_controller != null)
			result = _controller.products.WithID(product);
		return result;
	}

    public string GetPriceString(IAPCatalogData item)
    {
        string result = "";
		if(_controller != null)
        {
            var onlineProduct = GetProductById(item.ID);
            string text = onlineProduct.metadata.localizedPriceString;
            Regex regEnglish = new Regex("^[a-zA-Z]");
            var arry = text.Substring(0, 1);

            if (regEnglish.IsMatch(arry))
            {
                result = text.Substring(2, text.Length - 2);
            }
            else
            {
                result = text;
            }
        }
        else
        {
            result = "$" + item.Price.ToString();
        }

		#if UNITY_EDITOR
        result = "$" + item.Price.ToString();
		#endif

        return result;
    }

    public void AddCreditsAndLuckyByItemId(string itemId, long winCredits)
    {
        IAPCatalogData iapData = IAPCatalogConfig.Instance.FindIAPItemByID(itemId);
        long origLucky = iapData.CreditsAddLongLucky * winCredits;
        int finalLucky = 0;

        if (iapData.LuckyTopLimit != 0)
        {
            finalLucky = origLucky > (uint)iapData.LuckyTopLimit ? iapData.LuckyTopLimit : (int)origLucky;
        }
        else
        {
            finalLucky = origLucky > int.MaxValue ? int.MaxValue : (int)origLucky;
        }

        UserBasicData.Instance.AddCredits((ulong)winCredits, FreeCreditsSource.NotFree, false);
        UserBasicData.Instance.AddLongLucky(finalLucky, true);

        Debug.Log("StoreModule: reward credits: " + winCredits + ", longlucky: " + finalLucky
		+ ", userCurLoonglucky: " + UserBasicData.Instance.LongLucky);
    }

	#endregion
}


