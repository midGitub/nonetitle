using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PiggyStoreItem : GeneralStoreItem {

    /// <summary>
    ///  价格文字
    /// </summary>
    [SerializeField]
    private Text _oldpriceText;

    /// <summary>
    /// 整个oldPrice框
    /// </summary>
    [SerializeField]
    private GameObject _oldPriceGObj;

    [SerializeField]
    private PiggyTempAjustWidth _ajust;

    public override void UpdateVal()
    {
        var item = IAPCatalogConfig.Instance.FindIAPItemByID(_productID);

        FillPriceText(item);

        _ajust.AjustTargetWidth(_priceText);

        if(_oldpriceText != null)
        {
            float oldprice = Mathf.Floor(UserBasicData.Instance.PiggyBankCoins / _oldpriceDivisor) - 0.01f;
            if(oldprice <= item.Price) 
            { 
                _oldPriceGObj.gameObject.SetActive(false); 
            } 
            else 
            { 
                _oldPriceGObj.gameObject.SetActive(true);
                _hasDollarSymbol = _priceText.text.Contains(_dollarSymbol);
                if (_hasDollarSymbol) 
                {
                    _oldpriceText.text = "$" + item.OldPrice.ToString ();
                } 
                else 
                {
                    try
                    {
                        // Xhj 不得不转换oldPrice的货币种类，用服务器给出的本地化价格除以本地保存的美元价格乘以本地计算出的美元原价
                        _oldpriceText.text = UpdateOldPrice(_priceText.text, item.Price, oldprice);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                        _oldpriceText.text = "$" + item.OldPrice.ToString (); 
                    
                    }
                }
            }
				
			if (_oldpriceText.preferredWidth + _oldpriceText.preferredWidth / _oldpriceText.text.Length * 2 > _oldPriceGObj.GetComponent<RectTransform> ().sizeDelta.x) 
			{
				float width = _oldpriceText.preferredWidth + _oldpriceText.preferredWidth / _oldpriceText.text.Length * 2;
				_oldPriceGObj.GetComponent<RectTransform> ().sizeDelta = new Vector2 (width, _oldPriceGObj.GetComponent<RectTransform> ().sizeDelta.y);
			}
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
            price = (int) price / localPrice * oldPriceAsDoller;
            result = currencySymbol + price.ToString ("f2");
        }catch(Exception e){
            LogUtility.Log("Price invalid exception = " + e.Message, Color.red);
        }

        return result;
    }   
}
