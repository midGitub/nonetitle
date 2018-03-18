using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class StoreItem : MonoBehaviour, IPointerClickHandler
{
    protected static readonly string _dollarSymbol = "$";
    protected static readonly char[] _numberArray = new char[]{
		'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 
	};
    protected bool _hasDollarSymbol = false;

    [SerializeField]
    protected string _productID;

	///// <summary>
	///// 标题文本
	///// </summary>
	//[SerializeField]
	//private Text _titleText;

	/// <summary> 
	/// 描述性文字
	/// </summary>
	[SerializeField]
	private Text _descriptionText;

	[SerializeField]
	private Text _vipPoint;

	/// <summary>
	///  价格文字
	/// </summary>
	[SerializeField]
    protected Text _priceText;

	/// <summary>
	///  价格文字
	/// </summary>
	[SerializeField]
    protected Text _oldpriceText;

	/// <summary>
	///  价格文字
	/// </summary>
	[SerializeField]
	private Text _preferentialText;

	/// <summary>
	/// 标题
	/// </summary>
	[SerializeField]
	private Text _titleText;

	[SerializeField]
	private int _localProductID;


	[SerializeField]
	private Image _vipImage;

    public string ProductID { set { _productID = value; } }

	private void OnEnable()
	{
		// StoreManager.Instance.OnStoreControllerInitSuccessEvent.AddListener(Init);
		UpdateProductID();
		UpdateVal();
		VIPSystem.Instance.VIPLevelDataChangeEvent.AddListener(UpdateVal);
	}

	private void OnDisable()
	{
		if (VIPSystem.Instance != null) {
			VIPSystem.Instance.VIPLevelDataChangeEvent.RemoveListener(UpdateVal);
		}
	}

	//public void OnDestroy()
	//{
	//	StoreManager.Instance.OnStoreControllerInitSuccessEvent.RemoveListener(Init);
	//}

	public virtual void UpdateVal()
	{
		IAPCatalogData item = IAPCatalogConfig.Instance.FindIAPItemByID(_productID);

        FillPriceText(item);

		float credits = IAPCatalogConfig.Instance.GetCreditsWithPromotion(item);
		
		if(_descriptionText != null)
			_descriptionText.text = StringUtility.FormatNumberStringWithComma((ulong)credits);

		if (_vipPoint != null) 
		{
			_vipPoint.text = StringUtility.FormatNumberStringWithComma ((ulong)IAPCatalogConfig.Instance.GetVIPPoint (item));
			AdjustVipImagePosition ();
		}

		if(_oldpriceText != null)
		{
			// 旧价格是当前价格的2倍
			try
			{
				_oldpriceText.text = UpdateOldPrice(item, _priceText.text);
			}
			catch(Exception e)
			{
				if(IAPCatalogConfig.Instance.ShouldPromoteItem(item)) 
					item.OldPrice *= PromotionHelper.Instance.PromotedCreditFactor;
				
				_oldpriceText.text = "$" + item.OldPrice.ToString ();
			}
		}

		if(_preferentialText != null)
		{
			_preferentialText.text = item.Preferential;
		}

		if(_titleText != null)
		{
			_titleText.text = item.Title;
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		// Debug.Log("In");
		PurchaseProduct();
	}

	/// <summary>
	/// 开始购买
	/// </summary>
    protected void PurchaseProduct()
	{
        Debug.Log("----------Spark a purchase, productID is " + _productID);
		StoreManager.Instance.InitiatePurchase(_productID);
		AudioManager.Instance.PlaySound(AudioType.Click);
		// UnityEngine.Purchasing.IAPButton.IAPButtonStoreManager.Instance.InitiatePurchase(_productID);
	}

    protected void FillPriceText(IAPCatalogData item)
    {
		if(StoreManager.Instance.Controller != null)
        {
            var onlineProduct = StoreManager.Instance.GetProductById(_productID);
			_priceText.text = onlineProduct.metadata.localizedPriceString;
        }
        else
        {
            _priceText.text = "$" + item.Price.ToString();
        }

        #if UNITY_EDITOR
        _priceText.text = "$" + item.Price.ToString();
        #endif 
    }

	private string UpdateOldPrice(IAPCatalogData item, string currentPriceStr){
		// 获得货币值开始的索引
		int index = currentPriceStr.IndexOfAny(_numberArray);
		// 获得货币开始符号
		string currencySymbol = "";
		if (index > 0)
			currencySymbol = currentPriceStr.Substring (0, index);

        string result = currentPriceStr;
        try{
			float price = StringUtility.ConstructPrice(currentPriceStr);
			// 优惠前的价格
			price = price * 2.0f;

			if(IAPCatalogConfig.Instance.ShouldPromoteItem(item))
				price *= PromotionHelper.Instance.PromotedCreditFactor;

			result = currencySymbol + price.ToString ();
        }catch(Exception e){
            LogUtility.Log("Price invalid exception = " + e.Message, Color.red);
        }

        return result;
	}

	void UpdateProductID()
	{
		
		if (StoreController.Instance.TwoStore.activeInHierarchy)
		{
			int[] productID = GroupConfig.Instance.GetProductIDArray(StoreType.SmallBuy);
			SetProductID(productID);
		}
		else if (StoreController.Instance.BigStore.activeInHierarchy)
		{
			int[] productID = GroupConfig.Instance.GetProductIDArray(StoreType.Buy);
			SetProductID(productID);
		}
		else if (StoreController.Instance.ThreeStore.activeInHierarchy)
		{
			int[] productID = GroupConfig.Instance.GetProductIDArray(StoreType.Deal);
			SetProductID(productID);
		}
	}

	void SetProductID(int[] array)
	{
		if (array.Length > _localProductID)
			ProductID = array [_localProductID].ToString();
	}


	void AdjustVipImagePosition()
	{
		if (_vipImage != null) 
		{
			Vector3 imagevec = _vipImage.rectTransform.localPosition, pointvec = _vipPoint.rectTransform.localPosition;
			if (imagevec.x < pointvec.x + _vipPoint.preferredWidth * _vipPoint.transform.localScale.x + 10) 
			{
				float offset = pointvec.x + _vipPoint.preferredWidth * _vipPoint.transform.localScale.x + 10 - imagevec.x;
				_vipImage.rectTransform.localPosition += new Vector3 (offset, 0, 0); 
			}
		}
	}
}
