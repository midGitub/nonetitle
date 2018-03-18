using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TLStoreItem : GeneralStoreItem {

    /// <summary>
    ///  价格文字
    /// </summary>
    [SerializeField]
    private Text _oldpriceText;

    /// <summary>
    /// 优惠百分比
    /// </summary>
    [SerializeField]
    private Text _preferentialText;

    /// <summary>
    /// 商品包含多少credits
    /// </summary>
    [SerializeField]
    private Text _creditsText;

    /// <summary>
    /// 商品增加的vip点数
    /// </summary>
    [SerializeField]
    private Text _vipPoint;

    [SerializeField]
    private AjustWidthByContent _ajust;

    public override void UpdateVal()
    {
        var item = IAPCatalogConfig.Instance.FindIAPItemByID(_productID);

        FillPriceText(item);

        if(_oldpriceText != null)
        {
            _hasDollarSymbol = _priceText.text.Contains(_dollarSymbol);
            if (_hasDollarSymbol) 
            {
				_oldpriceText.text = "Was : $" + item.OldPrice.ToString ();
            } 
            else 
            {
				try{
					// Xhj 不得不转换oldPrice的货币种类，用服务器给出的本地化价格除以本地保存的美元价格乘以本地计算出的美元原价
					_oldpriceText.text = UpdateOldPrice(_priceText.text, item.Price, item.OldPrice);
				}
				catch(Exception e){
                    Debug.LogError(e.Message);
					_oldpriceText.text = "Was : $" + item.OldPrice.ToString ();
				}
            } 
        }

        _ajust.AjustTargetWidth(_oldpriceText.text.Length);

        if (_vipPoint != null)
        {
            _vipPoint.text = StringUtility.FormatNumberStringWithComma((ulong)item.OneDLAddVIPPoint);
        }

        if(_creditsText != null)
        {
            _creditsText.text = StringUtility.FormatNumberStringWithComma((ulong)item.CREDITS);
        }

        if (_preferentialText != null)
        {
//            LogUtility.Log("TLStoreUI " + price + " " + oldPrice + " " + ((1f - (price / oldPrice)) * 100f), Color.magenta);
//            _preferentialText.text = Mathf.Round((1f - (item.Price / item.OldPrice)) * 100f) + "%";
			_preferentialText.text = item.Preferential;
        }
    }   

    private string UpdateOldPrice(string currentPriceStr, float localPrice, float oldPriceAsDoller){
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
            LogUtility.Log("TLStoreUI " + price + " " + localPrice + " " + oldPriceAsDoller, Color.magenta);
            price = (int) (price / localPrice * oldPriceAsDoller);
            LogUtility.Log("TLStoreUI " + currencySymbol + price.ToString ("f2"), Color.magenta);

            result = currencySymbol + price.ToString ("f2");
        }catch(Exception e){
            LogUtility.Log("Price invalid exception = " + e.Message, Color.red);
        }

        return result;
    }   
}
