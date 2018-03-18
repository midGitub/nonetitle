using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class GeneralStoreItem : MonoBehaviour, IPointerClickHandler {

    protected static readonly float _oldpriceDivisor = 1000;
    protected static readonly string _dollarSymbol = "$";
    protected static readonly char[] _numberArray = new char[]{
        '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 
    };
    protected bool _hasDollarSymbol = false;

    [SerializeField]
    protected string _productID;

    /// <summary>
    ///  价格文字
    /// </summary>
    [SerializeField]
    protected Text _priceText;

    public string ProductID { set { _productID = value; } }

    public virtual void UpdateVal()
    {
        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
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
    }

    protected void FillPriceText(IAPCatalogData item)
    {
		if(StoreManager.Instance.Controller != null)
        {
            var onlineProduct = StoreManager.Instance.GetProductById(_productID);
            string text = onlineProduct.metadata.localizedPriceString;
            Regex regEnglish = new Regex("^[a-zA-Z]");
            var arry = text.Substring(0, 1);

            if(regEnglish.IsMatch(arry))
            {
                _priceText.text = text.Substring(2, text.Length - 2);
            }
            else
            {
                _priceText.text = text;
            }
        }
        else
        {
            _priceText.text = "$" + item.Price.ToString();
        }

        #if UNITY_EDITOR
        _priceText.text = "$" + item.Price.ToString();
        #endif 
    }
}
